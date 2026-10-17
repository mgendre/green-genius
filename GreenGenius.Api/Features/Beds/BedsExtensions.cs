using System.Linq.Expressions;
using GreenGenius.Api.Features.Beds.Dtos;
using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Beds;

public static class BedsExtensions
{
    public static readonly Expression<Func<Bed, BedDto>> AsDto = bed => new BedDto
    {
        Id = bed.Id,
        Name = bed.Name,
        GardenId = bed.GardenId,
        Plants = bed.BedPlants
            .OrderBy(bp => bp.Plant.NameFr)
            .Select(bp => new PlantSummaryDto
            {
                Id = bp.Plant.Id,
                NameFr = bp.Plant.NameFr,
                BinomialName = bp.Plant.BinomialName,
                Emoji = bp.Plant.Emoji,
                Family = bp.Plant.Family
            })
            .ToList()
    };

    private static readonly Func<Bed, BedDto> AsDtoCompiled = AsDto.Compile();

    public static BedDto ToDto(this Bed bed)
    {
        return AsDtoCompiled(bed);
    }

    extension(DbSet<Bed> beds)
    {
        public async Task<Bed> GetBedWithPlantsAsync(Guid gardenId, Guid bedId)
        {
            var bed = await beds
                .Include(bed => bed.BedPlants)
                .Where(bed => bed.Id == bedId && bed.GardenId == gardenId)
                .FirstOrDefaultAsync();

            return bed ?? throw BedNotFound(gardenId, bedId);
        }

        public async Task<Bed> GetBedWithoutPlantsAsync(Guid gardenId, Guid bedId)
        {
            var bed = await beds
                .Where(bed => bed.Id == bedId && bed.GardenId == gardenId)
                .FirstOrDefaultAsync();

            return bed ?? throw BedNotFound(gardenId, bedId);
        }

        public async Task ThrowWhenBedMissingAsync(Guid gardenId, Guid bedId)
        {
            var present = await beds
                .AnyAsync(bed => bed.Id == bedId && bed.GardenId == gardenId);

            if (!present)
            {
                throw BedNotFound(gardenId, bedId);
            }
        }
    }

    private static DataNotFoundException BedNotFound(Guid gardenId, Guid bedId)
    {
        return new DataNotFoundException($"Bed {bedId} not found in garden {gardenId}");
    }
}