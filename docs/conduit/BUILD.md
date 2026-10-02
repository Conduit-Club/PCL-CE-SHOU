# Conduit-Club 基线构建

本项目是 Conduit-Club 基于 PCL-CE 独立维护的第三方社团 Minecraft 启动器。当前仅准备构建与发布设施，界面、更新源和社团功能尚未定制。

## 上游

- 只同步 `PCL-Community/PCL-CE` 的 `dev`，不直接同步 PCL。
- 2026-10-02 基线：`a42a7699948aebd1e2563df025274af255c1f559`，相对原仓库快进 13 个提交，无合并冲突。
- 上游元数据版本：`2.15.1-beta.1`；发行标签另加 `conduit-` 前缀，不冒充 CE 官方发行版。

## 环境

本机 .NET SDK 10.0.401 / Windows Desktop Runtime 10.0.12 位于 `D:\Programs_Dev\dotnet`。
SDK 通过微软官方 `https://dot.net/v1/dotnet-install.ps1` 安装，使用 `-Channel 10.0 -Quality GA -Architecture x64 -InstallDir D:\Programs_Dev\dotnet`。
用户 PATH 和 DOTNET_ROOT 指向该目录；更改后请新开终端。NuGet 缓存位于 `D:\DevCache\nuget`。

```powershell
./scripts/conduit/Publish.ps1 -Dotnet D:\Programs_Dev\dotnet\dotnet.exe
dotnet test PCL.Core.Test/PCL.Core.Test.csproj -c Beta -p:Platform=x64
```

脚本默认构建 Windows x64、Beta、自包含单文件程序；输出到 `artifacts/conduit`，ZIP 内含 EXE、许可文件及构建信息。无需额外安装 .NET 运行时，不是 MSI/Setup 安装向导。重新构建前将旧输出目录移走，避免混入旧文件。可使用 `-Architecture ARM64` 交叉编译，但不能替代 ARM64 真机验证。

## 发布

`Conduit Build` 工作流可在 GitHub Actions 手动运行，会上传 x64 构建产物，不使用上游密钥或镜像服务。本地构建后通过 `gh release create` 发布 ZIP 和 SHA-256 文件；先推送源码，再将 Release 固定到已验证的源码提交，开发基线标记为 prerelease。

原 CE Release 工作流仅在 `PCL-Community/PCL-CE` 仓库运行，避免重写社团 Release 说明、要求 CE 签名密钥或触发上游 MirrorChyan 上传。这是对上游工作流的少量必要差异，未来同步时需保留并检查。

## 当前限制

- 本基线不嵌入外部服务凭据：Microsoft OAuth、CurseForge、Natayark/联机等依赖凭据的功能尚不可视为可用；后续应配置社团自己的合法凭据。
- 未做社团 UI、服务器和更新源改造；现有更新行为仍来自 CE，社团正式发行前需要单独处理。
- 未使用上游 GPG 私钥，也未做 Authenticode 签名；SHA-256 仅用于核对文件完整性。
- 编译和单元测试不能替代登录、下载、安装游戏、启动 Minecraft 的端到端验证。
