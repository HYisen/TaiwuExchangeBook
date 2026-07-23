# 全门派换书 (Taiwu Exchange Book)

《太吾绘卷：天幕心帷》Mod — 在门派界面添加"门派换书"和"技艺换书"按钮，方便兑换功法书和技艺书。

> Community fork of the original Steam Workshop mod [全门派换书](https://steamcommunity.com/sharedfiles/filedetails/?id=2975299672), updated for game version 1.0.67.

## 功能

- 在门派据点界面显示"门派换书"（武功秘籍）和"技艺换书"按钮
- 支持筛选、搜索功法书/技艺书
- 可选"仅门派地格才能换书"（Config 设置项）
- 修复了功法书遗留在 `_skillBooks` 字典中的内存泄漏问题

## Install

### Download (easy)

1. Subscribe to the [original mod](https://steamcommunity.com/sharedfiles/filedetails/?id=2975299672) on Steam Workshop.
2. Go to [Releases](../../releases) and download the latest `ExchangeBook.zip`.
3. Extract into `<Steam>\steamapps\workshop\content\838350\2975299672\`, overwriting the existing files.
4. Restart the game.

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

### Deploy

Copy `mod/` into `<GameDir>/Mod/全门派换书/`:

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

## How this fork was updated (AI-assisted)

This fork was updated from 1.0.36 to 1.0.67 in a single session with a coding AI. To reproduce this workflow for another mod:

1. **Point the AI at the broken mod**. The human gave the AI the mod's workshop folder path and said "it's facing error since game update."
2. **Let the AI set up its own tools**. It detected .NET SDK was missing; the human told it "install that with scoop." It installed the SDK, then `ilspycmd`.
3. **The AI reads the game logs on its own**. It found `MissingMethodException` on `RowItemLine.Set(RowItemMain, bool, bool)` in `Player.log`.
4. **The AI decompiles and diffs signatures**. It decompiled both the mod's DLLs (which had `.pdb` files, giving high-quality output) and the current game's `Assembly-CSharp.dll`, then grep'd the decompiled source for the broken method to discover the new signature.
5. **Iterate with short feedback**. The AI's first IL patch had a stack imbalance. The human simply said "the error message changed" and the AI debugged and fixed it on its own. It then rebuilt the backend and patched the rest of the frontend source.
6. **The AI deploys; the human tests**. The AI copied the fixed DLLs into `<GameDir>\Mod\全门派换书\`; the human launched the game and confirmed it worked.

Everything else — environment setup, decompilation, signature diffing, IL patching, source fixes, project scaffolding, build, deployment, and GitHub release — was autonomous.

## Credits

- Original mod author: **p（发射的熟鸡蛋自用修复）**
- This fork entirely done by AI, with human input limited to: the mod path, "install with scoop", and "the error message changed"
