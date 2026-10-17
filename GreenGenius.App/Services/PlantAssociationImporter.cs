using System.Text.Json;
using GreenGenius.App.Json;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.App.Services;

public sealed class PlantAssociationImporter(
    ApplicationDbContext dbContext,
    ILogger<PlantAssociationImporter> logger,
    string? associationsDirectory = null)
{
    private readonly string _associationsDirectory = associationsDirectory
                                                     ?? Environment.GetEnvironmentVariable(
                                                         "GREENGENIUS_ASSOCIATIONS_DIRECTORY")
                                                     ?? Path.Combine(AppContext.BaseDirectory, "Data", "Associations");

    public async Task<int> ImportAsync(CancellationToken ct = default)
    {
        if (!Directory.Exists(_associationsDirectory))
        {
            logger.LogWarning("Associations directory {Directory} missing: nothing to synchronise",
                _associationsDirectory);
            return 0;
        }

        var entries = await ReadEntriesAsync(ct);
        var plants = await LoadPlantsAsync(entries, ct);
        var existing = (await dbContext.PlantAssociations
                .Include(association => association.Mechanisms)
                .ToListAsync(ct))
            .ToDictionary(association => (association.PlantAId, association.PlantBId));

        var current = new HashSet<(Guid PlantAId, Guid PlantBId)>();
        foreach (var entry in entries)
        {
            if (!HasMechanism(entry))
            {
                continue;
            }

            if (!TryResolvePair(entry.Json, plants, entry.File, out var pair))
            {
                continue;
            }

            if (!current.Add(pair))
            {
                logger.LogWarning("Association {File} skipped: pair already imported by an earlier file", entry.File);
                continue;
            }

            Upsert(existing, pair, entry.Json);
        }

        foreach (var stale in existing.Where(pair => !current.Contains(pair.Key)))
        {
            dbContext.PlantAssociations.Remove(stale.Value);
        }

        await dbContext.SaveChangesAsync(ct);
        return current.Count;
    }

    private async Task<List<AssociationEntry>> ReadEntriesAsync(CancellationToken ct)
    {
        var entries = new List<AssociationEntry>();
        foreach (var path in Directory.GetFiles(_associationsDirectory, "*.json")
                     .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal))
        {
            var name = Path.GetFileName(path);
            var json = await ReadJsonAsync(path, ct);
            if (json is not null && IsComplete(json, name))
            {
                entries.Add(new AssociationEntry(name, json));
            }
        }

        return entries;
    }

    private bool IsComplete(PlantAssociationJson json, string file)
    {
        if (json.PlantA is null || json.PlantB is null || json.Mechanisms is null)
        {
            logger.LogWarning("Association {File} skipped: PlantA, PlantB or Mechanisms is null", file);
            return false;
        }

        return true;
    }

    private async Task<PlantAssociationJson?> ReadJsonAsync(string file, CancellationToken ct)
    {
        var name = Path.GetFileName(file);
        try
        {
            var json = (await File.ReadAllTextAsync(file, ct)).DeserializeWithAppJsonDefaults<PlantAssociationJson>();
            if (json is null)
            {
                logger.LogWarning("Association {File} skipped: empty JSON", name);
            }

            return json;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Association {File} skipped: invalid JSON", name);
            return null;
        }
    }

    private async Task<Dictionary<string, Plant>> LoadPlantsAsync(List<AssociationEntry> entries, CancellationToken ct)
    {
        var keys = entries
            .SelectMany(entry => new[] { entry.Json.PlantA, entry.Json.PlantB })
            .Distinct()
            .ToList();

        return await dbContext.Plants
            .Where(plant => plant.Key != null && keys.Contains(plant.Key))
            .ToDictionaryAsync(plant => plant.Key!, ct);
    }

    private bool HasMechanism(AssociationEntry entry)
    {
        if (entry.Json.Mechanisms.Count > 0)
        {
            return true;
        }

        logger.LogWarning("Association {File} skipped: no mechanism", entry.File);
        return false;
    }

    private bool TryResolvePair(
        PlantAssociationJson json,
        Dictionary<string, Plant> plants,
        string file,
        out (Guid PlantAId, Guid PlantBId) pair)
    {
        pair = default;
        if (!plants.TryGetValue(json.PlantA, out var plantA) || !plants.TryGetValue(json.PlantB, out var plantB))
        {
            logger.LogWarning("Association {File} skipped: unknown plant key", file);
            return false;
        }

        var comparison = string.CompareOrdinal(json.PlantA, json.PlantB);
        if (comparison == 0)
        {
            logger.LogWarning("Association {File} skipped: a plant is not associated with itself", file);
            return false;
        }

        pair = comparison < 0
            ? (plantA.Id, plantB.Id)
            : (plantB.Id, plantA.Id);
        return true;
    }

    private void Upsert(
        Dictionary<(Guid PlantAId, Guid PlantBId), PlantAssociation> existing,
        (Guid PlantAId, Guid PlantBId) pair,
        PlantAssociationJson json)
    {
        var mechanisms = json.Mechanisms.Distinct().ToList();
        if (existing.TryGetValue(pair, out var association))
        {
            association.Effect = json.Effect;
            association.Evidence = json.Evidence;
            association.NoteFr = json.NoteFr;
            ReplaceMechanisms(association, mechanisms);
            return;
        }

        var id = Guid.NewGuid();
        var added = new PlantAssociation
        {
            Id = id,
            PlantAId = pair.PlantAId,
            PlantBId = pair.PlantBId,
            Effect = json.Effect,
            Evidence = json.Evidence,
            NoteFr = json.NoteFr,
            Mechanisms = mechanisms
                .Select(mechanism => NewMechanism(id, mechanism))
                .ToList()
        };
        dbContext.PlantAssociations.Add(added);
        existing[pair] = added;
    }

    private static void ReplaceMechanisms(PlantAssociation association, List<Mechanism> mechanisms)
    {
        var kept = association.Mechanisms
            .Where(mechanism => mechanisms.Contains(mechanism.Mechanism))
            .ToList();
        var missing = mechanisms
            .Where(mechanism => kept.All(row => row.Mechanism != mechanism))
            .Select(mechanism => NewMechanism(association.Id, mechanism));

        association.Mechanisms.Clear();
        association.Mechanisms.AddRange(kept);
        association.Mechanisms.AddRange(missing);
    }

    private static PlantAssociationMechanism NewMechanism(Guid associationId, Mechanism mechanism) => new()
    {
        PlantAssociationId = associationId,
        Mechanism = mechanism
    };

    private sealed record AssociationEntry(string File, PlantAssociationJson Json);
}