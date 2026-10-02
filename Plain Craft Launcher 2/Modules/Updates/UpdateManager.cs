using System.IO;
using PCL.Core.App;
using PCL.Core.Conduit;

namespace PCL;

/// <summary>社团版只手动检查自己的 GitHub Releases，不后台更新或拉取 CE 公告。</summary>
public static class UpdateManager
{
    public static bool isUpdateWaitingRestart => false;
    // 保留旧公告模块的接口，但不注册任何上游源，也不启动公告请求。
    public static UpdatesWrapperModel remoteServer = new(new List<IUpdateSource>());
    public static ModLoader.LoaderTask<int, int> serverLoader = new("Conduit updates", _ => { });
    public static UpdateEnums.VersionStatus GetVersionStatus() => ClubUpdates.Latest is not { } latest
        ? UpdateEnums.VersionStatus.Unknown
        : latest.IsNewerThan(ClubRelease.CurrentTag) ? UpdateEnums.VersionStatus.NotLatest : UpdateEnums.VersionStatus.Latest;
    public static void UpdateStart(UpdateEnums.UpdateType type, string receivedKey = null, bool forceValidated = false)
        => ModBase.OpenWebsite(ClubUpdates.Latest?.PageUrl ?? ClubCatalog.Repository + "/releases");
    public static void UpdateRestart(bool triggerRestartAndByEnd, bool triggerRestart = true) { }
    public static void ShowCEAnnounce() { }
    internal static void DownloadLatestPCL(ModLoader.LoaderBase loaderToSyncProgress = null)
    {
        // 整合包随附当前已验证的社团启动器，绝不下载 CE 替换它。
        ModBase.CopyFile(Basics.ExecutablePath, Path.Combine(ModBase.pathTemp, "CE-Latest.exe"));
    }
}
