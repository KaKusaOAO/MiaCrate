using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Mochi.Utils;

namespace MiaCrate.Auth;

public abstract class HttpAuthenticationService : BaseAuthenticationService
{
    public IWebProxy Proxy { get; }

    protected HttpAuthenticationService(IWebProxy proxy)
    {
        Proxy = proxy;
    }

    protected HttpClient CreateHttpClient()
    {
        var client = new HttpClient(new HttpClientHandler
        {
            Proxy = Proxy
        });
        
        client.Timeout = TimeSpan.FromSeconds(15);
        client.DefaultRequestHeaders.CacheControl.NoCache = true;
        return client;
    }

    public async Task<string> PerformPostRequestAsync(Uri uri, string post, string contentType)
    {
        using var client = CreateHttpClient();
        var content = new StringContent(post, Encoding.UTF8, contentType);
        var response = await client.PostAsync(uri, content);
        var result = await response.Content.ReadAsStringAsync();
        
        Logger.Verbose($"Successful read, server response was {response.StatusCode}");
        Logger.Verbose($"Response: {result}");
        return result;
    }

    public string PerformPostRequest(Uri uri, string post, string contentType) =>
        PerformPostRequestAsync(uri, post, contentType).Result;

    public async Task<string> PerformGetRequestAsync(Uri uri, string? authentication = null)
    {
        using var client = CreateHttpClient();
        if (authentication != null)
        {
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authentication);
        }

        var response = await client.GetAsync(uri);
        var result = await response.Content.ReadAsStringAsync();
        
        Logger.Verbose($"Successful read, server response was {response.StatusCode}");
        Logger.Verbose($"Response: {result}");
        return result;
    }

    public string PerformGetRequest(Uri uri, string? authentication = null) =>
        PerformGetRequestAsync(uri, authentication).Result;
}