using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PCL.Core.App;
using PCL.Core.Conduit;
using PCL.Core.Minecraft.Profile;
using PCL.Core.Minecraft.Profile.Models;

namespace PCL;

public partial class ClubLaunchPanel : UserControl
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private string _snapshot = "";
    private bool _ready;
    public ClubLaunchPanel()
    {
        InitializeComponent();
        Servers.ItemsSource = ClubCatalog.Servers;
        Loaded += (_, _) => { Servers.SelectedValue = ClubCatalog.FindServer(Config.System.ClubServer).Id; _ready = true; RefreshAccounts(); _timer.Start(); };
        Unloaded += (_, _) => { _ready = false; _timer.Stop(); };
        _timer.Tick += (_, _) => RefreshAccounts();
    }
    private void RefreshAccounts()
    {
        MicrosoftStatus.Text = MicrosoftConfigured ? "微软登录已配置" : "微软登录尚未配置 · 其他登录可用";
        ProfileService.Load();
        var profiles = ProfileService.Profiles.ToArray();
        var current = ProfileService.Current?.ProfileId;
        var snapshot = string.Join("|", profiles.Select(p => $"{p.ProfileId}:{p.UserName}:{p.Server}:{p.ServerName}:{p.ProfileType}")) + current;
        if (_snapshot == snapshot) return;
        _snapshot = snapshot;
        Accounts.ItemsSource = profiles.Select(p => new { Profile = p, Title = p.UserName + (p.ProfileId == current ? "  · 当前账户" : ""), Source = ClubCatalog.AccountSource(p) }).ToArray();
        EmptyHint.Visibility = profiles.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    private void Account_Click(object sender, MouseButtonEventArgs e)
    {
        if (ModLaunch.isLaunching || ProfileService.IsCreatingProfile) { HintService.Hint("请先完成或取消当前登录 / 启动，再切换账户。"); return; }
        ProfileService.Select((McProfile)((MyListItem)sender).Tag);
        ModLaunch.mcLoginMsLoader.State = ModBase.LoadState.Waiting;
        ModLaunch.mcLoginAuthLoader.State = ModBase.LoadState.Waiting;
        ModLaunch.mcLoginLegacyLoader.State = ModBase.LoadState.Waiting;
        ModMain.frmLaunchLeft.RefreshPage(true);
        RefreshAccounts();
    }
    private void Add_Click(object sender, MouseButtonEventArgs e)
    {
        if (ModLaunch.isLaunching || ProfileService.IsCreatingProfile) { HintService.Hint("请先完成或取消当前登录 / 启动。"); return; }
        var provider = ((MyButton)sender).Tag?.ToString();
        if (provider == "other") { ModBase.RunInNewThread(ProfileUi.CreateProfile); return; }
        if (provider == "microsoft" && !MicrosoftConfigured && !ConfigureMicrosoft()) return;
        ProfileService.IsCreatingProfile = true;
        if (provider != "microsoft")
        {
            PageLoginAuth.draggedAuthServer = provider == "mua" ? ClubCatalog.MuaAuth : ClubCatalog.LittleSkinAuth;
            PageLoginAuth.draggedAuthServerOAuthSupported = null;
        }
        ModMain.frmLaunchLeft.RefreshPage(true, provider == "microsoft" ? ModLaunch.McLoginType.Ms : ModLaunch.McLoginType.Auth);
    }
    private static bool MicrosoftConfigured => !string.IsNullOrWhiteSpace(Config.System.ClubMicrosoftClientId) || !string.IsNullOrWhiteSpace(Secrets.MSOAuthClientId);
    private void MicrosoftConfig_Click(object sender, MouseButtonEventArgs e)
    {
        if (ModLaunch.isLaunching || ProfileService.IsCreatingProfile) { HintService.Hint("请先完成或取消当前登录 / 启动。"); return; }
        ConfigureMicrosoft();
    }
    private bool ConfigureMicrosoft()
    {
        var id = ModMain.MyMsgBoxInput("配置社团 Microsoft OAuth", "请输入社团应用的 Client ID（公开标识，不是 Client Secret）。更换 ID 后可能需要重新登录。尚未申请时可取消并使用 MUA Union 或 LittleSkin。", Config.System.ClubMicrosoftClientId);
        if (string.IsNullOrWhiteSpace(id)) return false;
        if (!Guid.TryParse(id.Trim(), out _)) { HintService.Hint("Client ID 应为有效的 UUID。"); return false; }
        Config.System.ClubMicrosoftClientId = id.Trim();
        ModLaunch.mcLoginMsLoader.State = ModBase.LoadState.Waiting;
        RefreshAccounts();
        return true;
    }
    private void Server_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_ready && Servers.SelectedItem is ClubServer server) Config.System.ClubServer = server.Id;
    }
}
