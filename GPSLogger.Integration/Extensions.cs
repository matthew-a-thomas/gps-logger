using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GPSLogger.Integration
{
    public static class Extensions
    {
        public static async Task<T> GetAsync<T>(this HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var text = await response.Content.ReadAsStringAsync();
            var deserialized = JsonConvert.DeserializeObject<T>(text);
            return deserialized;
        }

        public static async Task<string> PostAsync(this HttpClient client, string url, object contents)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = JsonConvert.SerializeObject(contents);
            var encodedContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, encodedContent);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
    }
}
