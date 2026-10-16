using System.Text.Json;
using GreenGenius.App.Extensions;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.App.Services;

public sealed class PlantAssociationImporter(
    ApplicationDbContext dbContext,
    ILogger<PlantAssociationImporter> logger,
    string? associationsDirectory = null)
{
    private readonly string _associationsDirectory = associationsDirectory
        ?? Environment.GetEnvironmentVariable("GREENGENIUS_ASSOCIATIONS_DIRECTORY")
        ?? Path.Combine(AppContext.BaseDirectory, "Data", "Associations");

    public async Task<int> ImportAsync(CancellationToken ct = default)
    {
        if (!Directory.Exists(_associationsDirectory))
        {
            logger.LogWarning("Associations directory {Directory} missing: nothing to synchronise", _associationsDirectory);
            return 0;
        }

        var entries = await ReadEntriesAsync(ct);
        var plants = await LoadPlantsAsync(entries, ct);
        var existing = (await dbContext.PlantAssociations.ToListAsync(ct))
            .ToDictionary(association => (association.PlantAId, association.PlantBId));

        var current = new HashSet<(Guid PlantAId, Guid PlantBId)>();
        foreach (var entry in entries)
        {
            if (!TryResolvePair(entry.Json, plants, entry.File, out var pair))
            {
                continue;
            }

            current.Add(pair);
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
        foreach (var file in Directory.GetFiles(_associationsDirectory, "*.json"))
        {
            var json = await ReadJsonAsync(file, ct);
            if (json is not null)
            {
                entries.Add(new AssociationEntry(Path.GetFileName(file), json));
            }
        }

        return entries;
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

        pair = string.CompareOrdinal(json.PlantA, json.PlantB) < 0
            ? (plantA.Id, plantB.Id)
            : (plantB.Id, plantA.Id);
        return true;
    }

    private void Upsert(
        Dictionary<(Guid PlantAId, Guid PlantBId), PlantAssociation> existing,
        (Guid PlantAId, Guid PlantBId) pair,
        PlantAssociationJson json)
    {
        if (existing.TryGetValue(pair, out var association))
        {
            association.Polarity = json.Polarity;
            association.Mechanism = json.Mechanism;
            association.Evidence = json.Evidence;
            association.NoteFr = json.NoteFr;
            return;
        }

        var added = new PlantAssociation
        {
            Id = Guid.NewGuid(),
            PlantAId = pair.PlantAId,
            PlantBId = pair.PlantBId,
            Polarity = json.Polarity,
            Mechanism = json.Mechanism,
            Evidence = json.Evidence,
            NoteFr = json.NoteFr
        };
        dbContext.PlantAssociations.Add(added);
        existing[pair] = added;
    }

    private sealed record AssociationEntry(string File, PlantAssociationJson Json);
}
