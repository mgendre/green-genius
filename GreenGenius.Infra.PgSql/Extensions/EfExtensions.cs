using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Infra.Database.Extensions;

public static class EfExtensions
{
    public static async ValueTask<T> GetAsync<T>(this DbSet<T> dbSet, params object[] keys) where T : class
    {
        return await dbSet.FindAsync(keys) ?? 
               throw new KeyNotFoundException("Could not find entity of type " + typeof(T).Name + 
                                              " with key (" + string.Join(",", keys) + ")");
    }
}