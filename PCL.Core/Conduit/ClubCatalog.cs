using System;
using System.Collections.Generic;
using PCL.Core.Minecraft.Profile.Models;

namespace PCL.Core.Conduit;

public sealed record ClubServer(string Id, string Name, string Address)
{
    public string DisplayName => string.IsNullOrEmpty(Address) ? Name : $"{Name} · {Address}";
}

/// <summary>社团公开文档中的入口；不包含账户凭据。</summary>
public static class ClubCatalog
{
    public const string Website = "https://conduit-club.github.io/";
    public const string Repository = "https://github.com/Conduit-Club/PCL-CE-SHOU";
    public const string MuaAuth = "https://skin.mualliance.ltd/api/union/yggdrasil";
    public const string LittleSkinAuth = "https://littleskin.cn/api/yggdrasil";
    public static IReadOnlyList<ClubServer> Servers { get; } = Array.AsReadOnly(new[]
    {
        new ClubServer("none", "只启动游戏", ""),
        new ClubServer("smp", "SMP · 多人生存", "smp.shoumc.com"),
        new ClubServer("create", "Create · 创造建筑", "create.shoumc.com"),
        new ClubServer("shou", "SHOU · 校园展示", "shou.shoumc.com")
    });

    public static ClubServer FindServer(string? id)
    {
        foreach (var server in Servers)
            if (server.Id == id) return server;
        return Servers[0];
    }

    public static string ResolveLaunchServer(string? selectedAddress, string? instanceAddress)
        => selectedAddress ?? instanceAddress ?? string.Empty;

    public static bool CanJoin(McProfile profile) => profile.ProfileType == ProfileType.Microsoft
        || (profile.ProfileType is ProfileType.Authlib or ProfileType.YggdrasilConnect
            && (IsProvider(profile.Server, MuaAuth) || IsProvider(profile.Server, LittleSkinAuth)));

    public static bool IsProvider(string? address, string expected)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out var actual)) return false;
        var target = new Uri(expected);
        var path = actual.AbsolutePath.TrimEnd('/');
        return actual.Scheme == Uri.UriSchemeHttps && actual.Host == target.Host && actual.Port == target.Port
            && (path == target.AbsolutePath || path == target.AbsolutePath + "/authserver");
    }

    public static string AccountSource(McProfile profile) => profile.ProfileType switch
    {
        ProfileType.Microsoft => "微软正版",
        ProfileType.Offline => "离线账户 · 无法进入社团服务器",
        _ when IsProvider(profile.Server, MuaAuth) => "MUA Union",
        _ when IsProvider(profile.Server, LittleSkinAuth) => "LittleSkin",
        _ => "第三方 · " + (profile.ServerName ?? profile.Server ?? "自定义认证")
    };
}
