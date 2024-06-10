using System.Text.Json;
using System.Text;

namespace CRUDBlazor.Client.Services
{
    public class CustomHttpClient
    {
        private readonly HttpClient _httpClient;

        public CustomHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
            };

            // Aquí es donde incluirías las credenciales.
            request.Headers.Add("Cookie", "auth");

            return await _httpClient.SendAsync(request);
        }
    }
}
