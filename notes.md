# 工作笔记 — BTD Mod Helper 本地化

任务 id：`btd6-modhelper-i18n`
Spec：`specs/btd6-modhelper-i18n/{requirements,design,tasks,progress}.md`
Fork：`7723qqq/BTD-Mod-Helper` · 分支 `i18n/zh-hans-zh-hant`

`specs/` 通过目录联接暴露给工作目录
（`C:\Program Files\Epic Games\BloonsTD6\specs` → `G:\bt6\specs`）。

---

## 关键事实（调查得出，非推测）

- **本地化文件名必须等于游戏 `Language` 枚举成员名。**
  `SimplifiedChinese.json` / `TraditionalChinese.json` / `Spanish_Spain.json` 三个文件
  永远匹配不上枚举 `ChineseSimplified` / `ChineseTraditional` / `Spanish`，导致三种语言
  **静默回退英文**。`LocalizationHelper` 用
  `resourceName.EndsWith($"Localization.{language}.json")` 匹配。
  判断方式：反查 DLL 内嵌资源名 + 读 `Il2CppNinjaKiwi.Localization.dll` 的枚举成员。

- **`Localize(key, text)` 返回的是 key，不是译文。**
  解析必须再调 `.Localize()` 扩展，或交给 `ModHelperText`（`AutoLocalize=true`）。
  `ShowPopup` 的按钮参数**不**自动本地化，直接传 key 会把 key 显示出来。

- **`LocalizationHelper.Yes/No/Ok/Cancel` 是游戏原生 key**（如 `"DataConsentYes"`），
  不是显示文本；且这些常量在上游从未被引用（死代码），无法确认游戏表里存在。
  故改用自有 key `YesButton`/`NoButton`/`OkButton`。

- **C# 类型身份无法用 Rust 表达**（同 `melonloader-rs` 项目笔记）：
  `class BloonsMod : MelonMod` 是托管元数据里的继承引用。

---

## 踩过的坑

1. **游戏运行时编译会中断 PostBuild。**
   `btd6.targets` 的 `MSB3061` 报 `BTD6ModHelper\Old Mods\Btd6ModHelper.dll` 被 `BloonsTD6.exe`
   锁定，**拷贝到 `Mods\` 的步骤会中止**，源码改了但 DLL 不更新。判断依据是产物时间戳没动。
   处理：先 `taskkill //IM BloonsTD6.exe //F` 再 `dotnet build`。

2. **静态扫描 i18n 硬编码极易误报。**
   `new Info("Text", …)`、`GetDescendent<NK_TextMeshProUGUI>("Text")` 里的字符串是
   **Unity 子物体名**（查找键），翻译会直接导致 `NullReferenceException`。
   真正的显示入口是 `ShowPopup` / `ShowOkPopup` / `AddText` / `SetText` / `displayName` / `description`。
   51 条「硬编码」里约 46 条是子物体名。

3. **运行时生成的本地化 key 抓不到。**
   `ModHelper.Localize(method.ToString(), …)` 这类写法的 key 由枚举名拼出，字面量扫描会漏。
   全项目只此一处（`ModBrowserMenu.cs` 的 `SortingMethod`）。

4. **中文字形缺失不是译文问题。**
   BTD6 自带中文字体按原版文本**子集化**，`筛`、`秽`、`剥`、`倾`、`逾`、`侦`、`曳` 不在子集内，
   渲染成 □。只能改写用词规避，不能靠改字体。

5. **`git` 不走 Windows 系统代理。** 本机代理 `127.0.0.1:10793`，`curl` 通但 `git` 超时。
   处理：`git config http.proxy http://127.0.0.1:10793`（仓库级）。
   撤销：`git config --unset http.proxy`。

6. **`gh repo fork` 的 `--clone=false` 参数不被接受**（gh 2.97.0）。
   替代：`gh api --method POST repos/<owner>/<repo>/forks`。

7. **`raw.githubusercontent.com` 在本机不通**（`HTTP 000`，代理返回 502）。
   启动时 `UpdaterPlugin.Message.CheckForMessages().Wait()` 是**同步阻塞**调用，会卡 61 秒。
   实测可用镜像 `https://ghproxy.net/https://raw.githubusercontent.com` 取单文件 OK，
   但**批量 439 个请求会限流**，导致 `found 0 mods`，故最终回退未启用。

---

## 可复用的验证命令

```bash
# 语言文件结构与键集一致性（3 个回归测试）
cd /g/bt6 && BloonsTD6="C:\Program Files\Epic Games\BloonsTD6" \
  dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~LocalizationTests"

# 干净编译（必须先关游戏）
taskkill //IM BloonsTD6.exe //F && sleep 3
cd /g/bt6 && dotnet build "BloonsTD6 Mod Helper/BloonsTD6 Mod Helper.csproj" \
  -c Debug -p:BloonsTD6="C:\Program Files\Epic Games\BloonsTD6"

# 本地化是否真的被导入（最硬的证据）
grep -a "Imported .* localizations" \
  "C:/Program Files/Epic Games/BloonsTD6/MelonLoader/Latest.log"

# 产物里逐条配对验证译文（比位置推算可靠）
python3 -c "
b=open(r'C:\Program Files\Epic Games\BloonsTD6\Mods\Btd6ModHelper.dll','rb').read()
s='\"BloonsTD6 Mod Helper-Mods\": \"模组\"'.encode()
print('OK' if s in b else 'MISS')"

# 通过 Epic 启动（避免安全校验）
cmd //c start "" "com.epicgames.launcher://apps/7786b355a13b47a6b3915335117cd0b2?action=launch"
```
