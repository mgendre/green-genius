using System.Text.Json;
using GreenGenius.App.Extensions;
using GreenGenius.Common.Data.Enums;
using Shouldly;

namespace GreenGenius.Api.Tests.Extensions;

public class JsonDefaultsExtensionsTest
{
    [Fact]
    public void UseAppJsonDefaults_ShouldWriteEnumAsAString()
    {
        var options = new JsonSerializerOptions().UseAppJsonDefaults();

        var json = JsonSerializer.Serialize(PlantFamily.Solanaceae, options);

        json.ShouldBe("\"Solanaceae\"");
    }

    [Fact]
    public void UseAppJsonDefaults_ShouldNotWriteEnumAsAString()
    {
        var options = new JsonSerializerOptions();

        var json = JsonSerializer.Serialize(PlantFamily.Solanaceae, options);

        json.ShouldNotBe("\"Solanaceae\"");
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
