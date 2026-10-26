# Progress — btd6-modhelper-i18n

实现路径：**文档模式**。本会话工具表中不含 `create_plan` / `get_progress` / `tasklist`，按 `notes.md`
的 Code Mode 兜底条款回退到文档模式，计划与进度记录在本目录的 `tasks.md` / `progress.md`。

## 状态

| 阶段 | 状态 | 证据 |
|---|---|---|
| requirements.md | 完成 | 6 条验收标准 A1-A6 均可核验 |
| design.md | 完成 | 4 条接口契约 + 4 条被否决方案及理由 |
| tasks.md | 完成 | T1-T9，T1-T7 已实现 |
| 实现 | 完成 | 见下表 |
| 验证 | A1-A6 通过 | 见下表 |
| Quality Mode review | 未执行 | 会话工具表中无 Code Review 工具，回退文档模式自审 |

## 验收核验结果

| 标准 | 结果 | 证据 |
|---|---|---|
| A1 文件名=Language 枚举成员 | 通过 | `FileNamesMatchGameLanguageEnum` 3/3 全绿 |
| A2 语言文件结构合法 | 通过 | `AllFilesAreWellFormed` 通过 |
| A3 五门键集一致且 ≥130 | 通过 | `ReviewedLanguagesShareTheSameKeys` 通过；脚本读数 130/130/130/130/130 |
| A4 运行时确实导入 130 条 | 通过 | `Imported 130 ChineseSimplified localizations from BloonsTD6 Mod Helper`（11:57:20.795 会话） |
| A5 弹窗按钮不再是原始 key | 通过 | `LocalizationHelper.YesText/NoText/OkText` 三属性 + DLL 内 `YesButton/NoButton/OkButton` 键 |
| A6 干净编译 | 通过 | `已成功生成。0 个错误`（游戏关闭状态下） |

## 代码修改清单（本次会话累计）

- `Api/Helpers/LocalizationHelper.cs`：`Contains` → `EndsWith` 精确匹配；新增 `YesText/NoText/OkText`
- `Api/Internal/ModHelperGithub.cs`：下载成功弹窗走 `ModDownloadSuccess`
- `Api/Internal/TemplateMod.cs`：模板目录冲突 / 升级项目弹窗接入 i18n
- `Api/Internal/EpicCompatibility.cs`：弹窗按钮改用已解析文本
- `MelonMain.cs`：兼容插件探测名忽略空格差异
- `UI/Menus/ModsMenu.cs`：`Mods` 标题 / `Restart` / `Disable` 接入 i18n
- `Tests/LocalizationTests.cs`：新增 3 个回归测试
- `Localization/*.json`：5 门语言各 130 条；3 个文件改名对齐枚举
- `Wiki/Localization.md`：补写文件名必须是枚举成员名
- `BloonsTD6 Mod Helper/CHANGELOG.md`：`[Unreleased]` 段记 6 条变更

## Backlog（非本次范围）

- 其余 15 门语言重译（每门 130 条）
- topic 标签 `new-towers` / `new-heroes` / `new-bloons` 的显示层映射
