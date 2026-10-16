using System.Text.Json;
using System.Text.Json.Serialization;

namespace GreenGenius.App.Extensions;

public static class JsonDefaultsExtensions
{
    private static readonly JsonSerializerOptions AppOptions = new JsonSerializerOptions().UseAppJsonDefaults();

    public static JsonSerializerOptions UseAppJsonDefaults(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static T? DeserializeWithAppJsonDefaults<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, AppOptions);
    }
}
