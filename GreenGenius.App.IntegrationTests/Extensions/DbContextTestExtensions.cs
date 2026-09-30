using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;

namespace GreenGenius.App.IntegrationTests.Extensions;

public static class DbContextTestExtensions
{
    public static async Task<Garden> PersistGardenAsync(this ApplicationDbContext dbContext, string name, Guid ownerId)
    {
        var garden = new Garden
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = name
        };

        dbContext.Add(garden);
        
        await dbContext.SaveChangesAsync();

        return garden;
    }
}