using System.Net.Http.Json;

namespace GreenGenius.App.IntegrationTests.Extensions;

public static class HttpClientExtensions
{
    extension(HttpClient httpClient)
    {
        public async Task<ApiResponse<TR>> PostAndRead<TR, T>(string requestUri, T value)
        {
            var response = await httpClient.PostAsJsonAsync(requestUri, value);
            return new ApiResponse<TR>(response, await ReadFromJson<TR>(response));
        }

        public async Task<ApiResponse<TR>> PutAndRead<TR, T>(string requestUri, T value)
        {
            var response = await httpClient.PutAsJsonAsync(requestUri, value);
            return new ApiResponse<TR>(response, await ReadFromJson<TR>(response));
        }

        public async Task<IList<TR>> GetAndReadList<TR>(string requestUri)
        {
            var response = await httpClient.GetAsync(requestUri);
            var list = await ReadFromJson<IEnumerable<TR>>(response);
            return list?.ToList() ?? [];
        }
    }

    private static async Task<TR?> ReadFromJson<TR>(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<TR>();
        }
        catch (Exception e)
        {
            var stringContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException("Could not parse json, content: " + stringContent, e);
        }
    }
}
