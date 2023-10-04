using System.Net.Http;
using System.Threading.Tasks;
using System;


/// <summary>
/// Class <c>HTTP</c> Handles HTTP requests asynchronously
/// </summary>
public class HTTP
{
    private static readonly HttpClient client = new HttpClient();


    public async Task<Request> request(string url)
    {
        Request req = new Request(url, client);
        HttpResponseMessage rep = await req.get();
        rep.EnsureSuccessStatusCode();
        return req;
    }
}


/// <summary>
/// Class <c>Request</c> Holds a HTTP request and response
/// </summary>
public class Request : Url
{
    private HttpClient client;
    private HttpResponseMessage response;

    public HttpResponseMessage Response
    {
        get { return this.response; }
    }
    public Request(string url, HttpClient client)
    {
        var timeOffset = DateTimeOffset.Now.ToUniversalTime();
        var epoch = timeOffset.ToUnixTimeSeconds();

        this.key = epoch.ToString();
        this.value = url;
        this.client = client;
    }

    public async Task<HttpResponseMessage> get()
    {
        this.response = await this.client.GetAsync(this.value);
        return this.response;
    }
}

