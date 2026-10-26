# Requirements — BTD Mod Helper 本地化（中文为主，兼顾多语言）

任务 id：`btd6-modhelper-i18n`
仓库：`G:\bt6`（fork：`7723qqq/BTD-Mod-Helper`，分支 `i18n/zh-hans-zh-hant`）

## 目标

让 Mod Helper 自己的界面跟随玩家游戏语言显示，其中简体中文、繁体中文、日语、俄语、德语使用**人工审校**文本（非机翻），并修掉那些会让本地化**静默失效**的基础设施 bug。

## 影响对象

安装了 Mod Helper、且游戏语言为上述任一语言的 BTD6 玩家（Steam 与 Epic 版均受影响）。

## 可观察的行为变化

1. 模组菜单 / 模组浏览器 / 设置面板显示中文（或日/俄/德），而非英文。
2. 「需要兼容插件」弹窗只在插件**确实缺失**时出现，不再误报。
3. 弹窗按钮显示「是 / 否 / 确定」，而不是 `DataConsentYes` 这类原始 key。
4. 模组浏览器排序项显示「最近更新 / 热门度 / …」，不再固定英文。

## 验收标准

每条均可由命令或可观察行为核验。

- **A1 文件名与游戏语言枚举一致。** `Localization/*.json` 的文件名必须等于游戏 `Language` 枚举成员名。
  核验：对照 `MelonLoader/Il2CppAssemblies/Il2CppNinjaKiwi.Localization.dll` 的枚举成员；并新增自动化测试 `LocalizationFileNamesMatchGameLanguageEnum` 通过。
  背景：旧文件名 `SimplifiedChinese` / `TraditionalChinese` / `Spanish_Spain` 与枚举 `ChineseSimplified` / `ChineseTraditional` / `Spanish` 不匹配，导致三种语言**从不加载**。

- **A2 每个语言文件结构合法。** 均为合法 JSON、无空值、条目数 ≥ 82。
  核验：新增测试 `AllLocalizationFilesAreWellFormed` 通过。

- **A3 五门审校语言键集对齐。** `ChineseSimplified` / `ChineseTraditional` / `Japanese` / `Russian` / `German` 均含 **130** 条，且键集与 `ChineseSimplified.json` 完全一致（含运行时生成的 `RecentlyUpdated` / `YesButton` / `ModDownloadSuccess` 等 48 个新增键）。
  核验：`Localization` 目录级脚本比对 + 测试断言。

- **A4 运行时确实导入。** 游戏语言为简体中文时，`MelonLoader/Latest.log` 出现
  `Imported 130 ChineseSimplified localizations from BloonsTD6 Mod Helper`。
  核验：启动游戏后读日志。已实测通过（11:57:20.795 那次会话）。

- **A5 弹窗按钮不再是原始 key。** 传给 `ShowPopup` 的按钮参数必须是已解析文本。
  核验：代码审查确认使用 `LocalizationHelper.YesText/NoText/OkText`；游戏内弹窗显示「是/否/确定」。

- **A6 干净编译。** 游戏关闭状态下 `dotnet build` 输出 `已成功生成` 且 0 个错误。
  核验：命令输出。

## 非目标

- **其余 15 门语言的重译**（Polish、Korean、Italian、Türkçe、Dutch、Swedish、Norwegian、Danish、Finnish、Thai、Arabic、Spanish、Spanish_LATAM、Portuguese_Brazil、Ukrainian 之外的同批机翻）仍保持机翻原文，列入 backlog。
  理由：每门 130 条，共约 1950 条；一次性产出无法保证质量，与「一点点重做」的既定节奏冲突。
- 模组作者发布的**模组名 / 简介 / 作者名**：数据来自 GitHub 各仓库，不属于本模组文案。
- 模组浏览器「按主题」下拉里的 `new-towers` / `new-heroes` / `new-bloons`：GitHub topic 标签，属数据而非界面文案。
- 更换游戏字体：BTD6 中文字体按原版文本子集化，缺字形只能靠改写用词规避（已在繁体/简体里替换「筛/秽/剥/倾/逾/侦/曳」等）。

## 已决问题与理由

- **D1 用改文件名而不是加别名映射。** 加映射会让「资源名 ↔ 枚举名」长期存在两套规则；改名是根因修复，且对上游是 3 行 `git mv`。
- **D2 弹窗按钮用自有 key（`YesButton`/`NoButton`/`OkButton`），不用游戏原生 `DataConsentYes`。** 该常量在上游 `LocalizationHelper` 里从未被引用过（死代码），无法确认游戏表里存在；用自有 key 可保证「有译文则显示译文，缺译文回退英文」的确定行为。
- **D3 不翻译 `new Info("Text", …)` 这类子物体名。** 它们是 `GetDescendent<T>("Text")` 的查找键，翻译会直接导致 NullReferenceException。
- **D4 顺手把 `Spanish_Spain.json` 改成 `Spanish.json`。** 与中文同一个 bug（文件名与枚举不符），改动是 1 次改名，收益是西班牙语从「从不加载」变为正常加载。
- **D5 精确后缀匹配嵌入资源。** 原 `Contains("Localization.Spanish")` 会把 `Localization.Spanish_LATAM.json` 一起吞掉，后者覆盖前者；改为 `EndsWith($"Localization.{language}.json")`。
