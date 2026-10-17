using System.Text.Json;
using GreenGenius.App.Json;
using GreenGenius.Common.Data.Enums;
using Shouldly;

namespace GreenGenius.Api.Tests.Json;

public class JsonDefaultsExtensionsTest
{
    private static readonly JsonSerializerOptions AppOptions = new JsonSerializerOptions().UseAppJsonDefaults();

    [Fact]
    public void UseAppJsonDefaults_ShouldWriteEnumAsAString()
    {
        var json = JsonSerializer.Serialize(PlantFamily.Solanaceae, AppOptions);

        json.ShouldBe("\"Solanaceae\"");
    }

    [Fact]
    public void DeserializeWithAppJsonDefaults_WhenEnumIsAString_ShouldReadTheEnum()
    {
        const string json = "\"Solanaceae\"";

        var family = json.DeserializeWithAppJsonDefaults<PlantFamily>();

        family.ShouldBe(PlantFamily.Solanaceae);
    }

    [Fact]
    public void DeserializeWithAppJsonDefaults_WhenJsonIsBroken_ShouldThrowJsonException()
    {
        const string json = "{ nope";

        Should.Throw<JsonException>(() => json.DeserializeWithAppJsonDefaults<PlantFamily>());
    }
}