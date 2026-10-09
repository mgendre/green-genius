using System.Text.Json;

namespace GreenGenius.Api.Tests.Services;

internal static class PlantJsonWriter
{
    private static readonly JsonSerializerOptions Options = new();

    public static async Task WriteAsync(string directory, string key, object value)
    {
        var json = JsonSerializer.Serialize(value, Options);
        await File.WriteAllTextAsync(Path.Combine(directory, key + ".json"), json);
    }
}
