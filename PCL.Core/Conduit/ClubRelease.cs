using System;
using System.Linq;
using System.Text.Json;
using PCL.Core.Utils;

namespace PCL.Core.Conduit;

public sealed record ClubRelease(string Tag, string Title, string Notes, string PageUrl)
{
    public const string CurrentTag = "conduit-v2.15.1-club.1";
    public bool IsNewerThan(string currentTag)
    {
        const string prefix = "conduit-v";
        if (!Tag.StartsWith(prefix, StringComparison.Ordinal) || !currentTag.StartsWith(prefix, StringComparison.Ordinal)) return false;
        try { return SemVer.Parse(Tag[prefix.Length..]) > SemVer.Parse(currentTag[prefix.Length..]); }
        catch { return false; }
    }

    public static ClubRelease? ParseLatest(string json)
    {
        using var document = JsonDocument.Parse(json);
        ClubRelease? latest = null;
        foreach (var release in document.RootElement.EnumerateArray()
                     .Where(r => !r.GetProperty("draft").GetBoolean())
                     .OrderByDescending(r => r.GetProperty("published_at").GetDateTimeOffset()))
        {
            var tag = release.GetProperty("tag_name").GetString() ?? "";
            var url = release.GetProperty("html_url").GetString() ?? "";
            if (!tag.StartsWith("conduit-v", StringComparison.Ordinal) || !SemVer.TryParse(tag[9..], out _)
                || !url.StartsWith(ClubCatalog.Repository + "/releases/tag/", StringComparison.Ordinal)) continue;
            var candidate = new ClubRelease(tag, release.GetProperty("name").GetString() ?? tag, release.GetProperty("body").GetString() ?? "", url);
            if (latest is null || candidate.IsNewerThan(latest.Tag)) latest = candidate;
        }
        return latest;
    }
}
