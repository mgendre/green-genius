using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GreenGenius.App.IntegrationTests.Extensions;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    extension(HttpClient httpClient)
    {
        public async Task<ApiResponse<TR>> PostAndRead<TR, T>(string requestUri, T value)
        {
            var response = await httpClient.PostAsJsonAsync(requestUri, value, JsonOptions);
            return new ApiResponse<TR>(response, await ReadFromJson<TR>(response));
        }

        public async Task<ApiResponse<TR>> PutAndRead<TR, T>(string requestUri, T value)
        {
            var response = await httpClient.PutAsJsonAsync(requestUri, value, JsonOptions);
            return new ApiResponse<TR>(response, await ReadFromJson<TR>(response));
        }

        public async Task<IList<TR>> GetAndReadList<TR>(string requestUri)
        {
            var response = await httpClient.GetAsync(requestUri);
            var list = await ReadFromJson<IEnumerable<TR>>(response);
            return list?.ToList() ?? [];
        }

        public async Task<ApiResponse<TR>> GetAndRead<TR>(string requestUri)
        {
            var response = await httpClient.GetAsync(requestUri);
            return new ApiResponse<TR>(response, await ReadFromJson<TR>(response));
        }
    }

    private static async Task<TR?> ReadFromJson<TR>(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<TR>(JsonOptions);
        }
        catch (Exception e)
        {
            var stringContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException("Could not parse json, content: " + stringContent, e);
        }
    }
}
