using GreenGenius.Common.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Common.Data.Extensions;

public static class EfExtensions
{
    public static async ValueTask<T> GetAsync<T>(this DbSet<T> dbSet, params object[] keys) where T : class
    {
        return await dbSet.FindAsync(keys) ?? 
               throw new DataNotFoundException("Could not find entity of type " + typeof(T).Name + 
                                               " with key (" + string.Join(",", keys) + ")");
    }
}