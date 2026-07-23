# 全门派换书 (Taiwu Exchange Book)

《太吾绘卷：天幕心帷》Mod — 在门派界面添加"门派换书"和"技艺换书"按钮，方便兑换功法书和技艺书。

> 此仓库是 Steam 创意工坊原 Mod [全门派换书](https://steamcommunity.com/sharedfiles/filedetails/?id=2975299672) 的社区维护分支，适配游戏最新版本。

## 功能

- 在门派据点界面显示"门派换书"（武功秘籍）和"技艺换书"按钮
- 支持筛选、搜索功法书/技艺书
- 可选"仅门派地格才能换书"（Config 设置项）
- 修复了功法书遗留在 `_skillBooks` 字典中的内存泄漏问题

## 构建

### 前置

- [.NET 10 SDK](https://dotnet.microsoft.com/download)（或 .NET 8+）
- 已安装《太吾绘卷》（Steam，App ID 838350）

### 编译

```bash
# 后端（net8.0）
dotnet build src/ExchangeBookBackend/ExchangeBookBackend.csproj -c Release

# 前端（netstandard2.1）
dotnet build src/ExchangeBook/ExchangeBook.csproj -c Release
```

产物自动输出到 `mod/Plugins/`。

如果游戏未装在 Steam 默认路径（注册表找不到），可通过 MSBuild 属性指定：

```bash
dotnet build /p:TaiwuGameDir="D:\Games\The Scroll Of Taiwu"
```

### 部署

将 `mod/` 目录复制到 `<游戏根>/Mod/全门派换书/`：

```bash
xcopy mod\* "G:\SteamLibrary\steamapps\common\The Scroll Of Taiwu\Mod\全门派换书\" /E /Y
```

## 项目结构

```
src/
├── ExchangeBook/          # 前端（Unity UI 补丁）
│   ├── ExchangeBook.csproj
│   ├── MainPatch.cs       # 入口 + Harmony 补丁
│   ├── UI_ExchangeBookPlus.cs  # 换书 UI
│   └── UIBuilder.cs       # UI 构建工具
├── ExchangeBookBackend/   # 后端（数据修复补丁）
│   ├── ExchangeBookBackend.csproj
│   ├── ModMain.cs         # 入口
│   └── SkillBookLeakFix.cs # 功法书泄漏修复
mod/
└── Config.Lua             # Mod 配置
```

## 致谢

原始 Mod 作者：**p（发射的熟鸡蛋自用修复）**

此仓库为社区维护版本，适配游戏最新版本。
