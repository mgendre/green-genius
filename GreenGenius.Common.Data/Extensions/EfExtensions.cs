using GreenGenius.Common.Data.Exceptions;
using GreenGenius.Common.Data.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenGenius.Common.Data.Extensions;

public static class EfExtensions
{
    public static async ValueTask<T> GetAsync<T>(this DbSet<T> dbSet, params object[] keys) where T : class
    {
        return await dbSet.FindAsync(keys) ?? 
               throw new DataNotFoundException("Could not find entity of type " + typeof(T).Name + 
                                               " with key (" + string.Join(",", keys) + ")");
    }

    public static void ConfigureOwnerIdQueryFilter<T>(
        this EntityTypeBuilder<T> builder,
        ApplicationDbContext dbContext) where T : class, IHasOwner
    {
        builder.HasQueryFilter(entity => entity.OwnerId == dbContext.CurrentUserId);
    }
}
