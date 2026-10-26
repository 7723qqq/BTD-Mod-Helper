# Design — BTD Mod Helper 本地化

## 1. 现有接口事实（读码确认，非推测）

```csharp
// Api/ModContent.Methods.cs:634
public static string Localize(BloonsMod mod, string key, string text)
{
    var id = GetId(mod, key);                       // GetId = mod.IDPrefix + name
    LocalizationManager.Instance.defaultTable[id] = text;   // 英文默认值入表
    return id;                                     // 返回的是 key，不是译文
}

// Extensions/SystemExtensions/StringExt.cs:50
public static string Localize(this string id) => GetBtd6Localization(id);
```

两条契约：

- `Localize(key, text)` 返回 **key 字符串**，把它直接交给 UI 会显示 key。
- 解析必须再调用一次 `.Localize()` 扩展，或交给会自动解析的组件（`ModHelperText` 的 `AutoLocalize=true`）。

**`LocalizationHelper` 的常量 `Yes/No/Ok/Cancel` 是游戏原生 key**（例如 `Yes = "DataConsentYes"`），**不是显示文本**；上游 9 处 `ShowPopup` 曾直接传字面量 `"Yes"/"No"`，而 `ShowPopup` 不做本地化。

## 2. 新增接口契约

`Api/Helpers/LocalizationHelper.cs` 新增三个**只读属性**（不是字段，保证每次访问都解析到当前语言）：

```csharp
public static string YesText => ModHelper.Localize("YesButton", "Yes").Localize();
public static string NoText  => ModHelper.Localize("NoButton",  "No").Localize();
public static string OkText  => ModHelper.Localize("OkButton",  "OK").Localize();
```

- 键名：`YesButton` / `NoButton` / `OkButton`（自有命名空间 `BloonsTD6 Mod Helper-` 下，避免与游戏原生 key 混淆）。
- 行为表：有当前语言译文 → 显示译文；无译文 → 显示英文默认值；**永不显示 key**。
- 原常量 `Yes/No/Ok/Cancel` 保留（供需要的调用方取原生 key），语义改为「key 常量」，用法必须配 `.Localize()`。

## 3. 文件所有权与命名契约

- `Localization/<Lang>.json` 的 `<Lang>` **必须等于** 游戏 `Language` 枚举成员名。
- 加载规则（`LocalizationHelper.ImportLocalization`）：
  ```csharp
  if (!resourceName.EndsWith($"Localization.{language.ToString()}.json")) continue;
  ```
  用 `EndsWith` 而非 `Contains`：`Contains("Localization.Spanish")` 会同时命中 `Localization.Spanish_LATAM.json`。
- 嵌入资源路径形如 `BloonsTD6 Mod Helper.Localization.<Lang>.json`，由 `btd6.targets` 的 `Localization/**/*.json` 通配产生，故**新增文件零配置**。

## 4. 状态与数据形状

- 键集基线：`ChineseSimplified.json`（130 条）。
- 每条：`"BloonsTD6 Mod Helper-<Key>": "<译文>"`，UTF-8 无 BOM、CRLF、2 空格缩进（与既有 20 个文件一致，保证 diff 只显示内容变化）。
- 含占位符的值用 `{0}`，调用处 `string.Format(key.Localize(), arg)`。注意 `ModHelperGithub` 会执行 `message.Replace(",", "\n")`，故译文**不得使用半角逗号**（中文用「，」、日文用「、」或「。」）。

## 5. 回归测试契约（`Tests/LocalizationTests.cs`，xUnit）

| 测试 | 断言 | 依赖 |
|---|---|---|
| `FileNamesMatchGameLanguageEnum` | `Localization/*.json` 的每个文件名 ∈ 游戏支持语言集合 | 读 `{Btd6Path}/Btd6ModHelper/Localization/*` 目录名（该目录由 `LocalizationHelper.Initialize()` 按枚举逐个创建，是枚举的**磁盘镜像**，无需加载 Il2Cpp 程序集）。取不到游戏目录时用 `SkippableFact` 跳过。 |
| `AllFilesAreWellFormed` | 每个文件合法 JSON、无空值、条目数 ≥ 82 | 纯文件系统 |
| `ReviewedLanguagesShareTheSameKeys` | zh-Hans/zh-Hant/ja/ru/de 五文件键集与 `ChineseSimplified.json` 完全一致且 ≥130 条 | 纯文件系统 |

`Tests.csproj` 已引用 xUnit 2.9.2 与 `Xunit.SkippableFact`，`ModSourcesDiscovery.SolutionRoot` 提供仓库根目录，无需改动工程文件。

## 6. 被否决的方案与理由

- **改代码做文件名别名映射**（用 `Contains` + 多候选名）→ 长期维护两套命名规则，且下次游戏加语言仍会踩坑。改名是一次性根因修复。
- **用游戏原生 `DataConsentYes` 做按钮文本** → 该常量上游从未引用，无法确认游戏表内存在；若不存在，`GetText` 返回 key 本身，正是线上出现 `DataConsentYes` 的形态。
- **把 15 门未审校语言一并补到 130 条** → 约 1950 条，只能靠机翻，与「人工审校」目标自相矛盾。降级为 backlog，由 A3 把范围钉死在 5 门。
- **翻译 `new Info("Text",…)` 子物体名** → 它们是 `GetDescendent<T>` 的查找键，翻译即产生 NullReferenceException。

## 7. 文档与代码冲突记录

- `Wiki/Localization.md` 说「把 `/Localization/[Language].json` 放进工程即可自动嵌入」，未提「`[Language]` 必须是枚举成员名」。按此文档操作正是 `SimplifiedChinese.json` 这类错误的来源。**采纳代码事实**，并在 docs 阶段补写这一点。
