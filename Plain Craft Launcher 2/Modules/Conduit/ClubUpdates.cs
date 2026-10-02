using System.Net.Http;
using PCL.Core.Conduit;

namespace PCL;

public static class ClubUpdates
{
    private static readonly HttpClient Client = CreateClient();
    public static ClubRelease Latest { get; private set; }
    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("PCL-CE-SHOU/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }
    public static async Task<ClubRelease> CheckAsync()
    {
        var json = await Client.GetStringAsync("https://api.github.com/repos/Conduit-Club/PCL-CE-SHOU/releases?per_page=30");
        return Latest = ClubRelease.ParseLatest(json);
    }
}
