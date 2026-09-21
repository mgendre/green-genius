using System.Net.Http.Json;

namespace GreenGenius.App.IntegrationTests.Extensions;

public static class HttpClientExtensions
{
    extension(HttpClient httpClient)
    {
        public TR? PostAndRead<TR, T>(string requestUri, T value, out HttpResponseMessage response) 
        {
            response = httpClient.PostAsJsonAsync(requestUri, value).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<TR>().GetAwaiter().GetResult();
        }

        public TR? GetAndRead<TR>(string requestUri, out HttpResponseMessage response) 
        {
            response = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<TR>().GetAwaiter().GetResult();
        }
    }
}
