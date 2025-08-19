using System.Net.Http.Headers;

namespace ContractViewerApi;

public class Apis
{
    public static readonly Api ContractList = new(nameof(ContractListApi), ContractListApi.Connection.Port);
    public static readonly Api ContractDetails = new(nameof(ContractDetailsApi), ContractDetailsApi.Connection.Port);
    public static readonly Api User = new(nameof(UserApi), UserApi.Connection.Port);
    public static readonly Api Document = new(nameof(DocumentApi), DocumentApi.Connection.Port);
}

public record Api(string Name, int Port);

public static class ApisExtensions
{
    public static void AddHttpClientForApi(this IServiceCollection services, Api api)
    {
        services.AddHttpClient(api.Name, client => client.BaseAddress = new Uri($"http://localhost:{api.Port}"));
    }
    
    public static HttpClient GetClient(this IHttpClientFactory factory, Api api, HttpContext ctx)
    {
        var client = factory.CreateClient(api.Name);
        var rawAuth = ctx.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(rawAuth))
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", rawAuth);
        }
        return client;
    }
    
    public static HttpClient GetClient(this IHttpClientFactory factory, Api api)
    {
        return factory.CreateClient(api.Name);
    }
}