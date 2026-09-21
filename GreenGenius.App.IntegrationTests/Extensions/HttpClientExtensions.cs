using System.Net.Http.Json;

namespace GreenGenius.App.IntegrationTests.Extensions;

public static class HttpClientExtensions
{
    extension(HttpClient httpClient)
    {
        public TR? PostAndRead<TR, T>(string requestUri, T value, out HttpResponseMessage response) 
        {
            response = httpClient.PostAsJsonAsync(requestUri, value).GetAwaiter().GetResult();
            return ReadFromJson<TR>(response);
        }
        
        public TR? PutAndRead<TR, T>(string requestUri, T value, out HttpResponseMessage response) 
        {
            response = httpClient.PutAsJsonAsync(requestUri, value).GetAwaiter().GetResult();
            return ReadFromJson<TR>(response);
        }

        public TR? GetAndRead<TR>(string requestUri, out HttpResponseMessage response) 
        {
            response = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
            return ReadFromJson<TR>(response);
        }
    }
    
    private static TR? ReadFromJson<TR>(HttpResponseMessage response)
    {
        try
        {
            return response.Content.ReadFromJsonAsync<TR>().GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            var stringContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            throw new InvalidOperationException("Could not parse json, content: " + stringContent, e);
        }
    }
}

