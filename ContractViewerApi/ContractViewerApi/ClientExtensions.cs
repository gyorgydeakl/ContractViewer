using System.Text.Json;

namespace ContractViewerApi;

public static class ClientExtensions
{
    public static async Task<T> GetAsync<T>(this HttpClient client, string url) where T : class
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        if (jsonResponse is T tResponse)
        {
            return tResponse;
        }
        return JsonSerializer.Deserialize<T>(jsonResponse, JsonSerializerOptions.Web) ??
               throw new Exception("Could not deserialize response.");
    }

    public static async Task<TRes> PostAsync<TReq, TRes>(this HttpClient client, string url, TReq body)
    {
        using var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        var str = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TRes>(str, JsonSerializerOptions.Web);
        return result ?? throw new InvalidOperationException("Could not deserialize response.");
    }
}