# Tasks — BTD Mod Helper 本地化

按顺序执行；每步都写明验证方式与验收证据。已完成项标记 `[x]`。

## T1 对齐文件名与游戏语言枚举 — 完成

- `git mv SimplifiedChinese.json → ChineseSimplified.json`、`TraditionalChinese.json → ChineseTraditional.json`、`Spanish_Spain.json → Spanish.json`
- 验证：与 `Il2CppNinjaKiwi.Localization.dll` 的 `Language` 成员比对；游戏目录 `Btd6ModHelper/Localization/` 下 23 个目录名即枚举成员。
- 证据：`Localization/` 下 20 个文件名全部命中枚举。

## T2 嵌入资源精确匹配 — 完成

- `LocalizationHelper.ImportLocalization` 的 `Contains` 改 `EndsWith($"Localization.{language}.json")`。
- 验证：模拟匹配脚本，每种语言命中资源数恰为 1（原实现下 `Spanish` 命中 2 个）。
- 证据：`每种游戏语言命中资源数 != 1 的情况: 0`。

## T3 弹窗按钮本地化 — 完成

- 新增 `LocalizationHelper.YesText/NoText/OkText`；9 处 `ShowPopup` 调用改用之。
- 验证：`dotnet build` 0 error；游戏内「需要兼容插件」弹窗按钮显示「是 / 否」。
- 证据：DLL 内含 `"BloonsTD6 Mod Helper-YesButton": "是"` 等 3 条。

## T4 补齐运行时生成的键 — 完成

- `SortingMethod` 六项（`RecentlyUpdated/Popularity/Alphabetical/New/Old/Relevance`）运行时由枚举名拼 key，静态扫描抓不到。
- 验证：模组浏览器排序下拉显示「最近更新」。
- 证据：DLL 内含 `"…-RecentlyUpdated": "最近更新"` 等 6 条。

## T5 界面硬编码接入 i18n — 完成

- `ModHelperGithub` 的下载成功弹窗、`TemplateMod` 的模板目录冲突 / 升级项目弹窗。
- 验证：`grep -rn "Successfully downloaded" --include=*.cs` 不再出现在显示路径上。
- 证据：`ModDownloadSuccess` / `TemplateDirExists` / `UpgradeProjectTitle` / `UpgradeProjectBody` 四条键已进 DLL。

## T6 五门语言译文重写 — 完成

- zh-Hans、zh-Hant、ja、ru、de 各 130 条，键集与 `ChineseSimplified.json` 完全一致。
- 验证：`ReviewedLanguagesShareTheSameKeys` 测试；脚本 `unmapped 0 / extra 0`。
- 证据：`Imported 130 ChineseSimplified localizations from BloonsTD6 Mod Helper`。

## T7 回归测试 — 本次实现

- 新增 `Tests/LocalizationTests.cs`，三个测试见 design §5。
- 验证：`BloonsTD6="C:\Program Files\Epic Games\BloonsTD6" dotnet test Tests/Tests.csproj --filter FullyQualifiedName~LocalizationTests`
- 验收：3 个测试全绿（`FileNamesMatchGameLanguageEnum` 在提供游戏路径时不跳过）。

## T8 文档补写 — 文档阶段执行

- 在 `Wiki/Localization.md` 补一句：文件名必须是 `Language` 枚举成员名，否则永远不会被加载。
- 验证：文件内可 grep 到该说明。

## T9 变更说明 — 交付阶段执行

- 写 `.changeset/` 或等价发布说明，列出 5 个行为变化与 1 个 backlog。

## Backlog（非本次范围）

- 其余 15 门语言重译，每门 130 条。
- `new-towers` / `new-heroes` / `new-bloons` 等 topic 标签的显示层映射。
