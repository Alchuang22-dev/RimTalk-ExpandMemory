# ?? 设置菜单诊断指南

## 问题：找不到Mod设置

### ? 检查清单

#### **1. 检查Mod是否加载**
```
1. 启动RimWorld
2. 打开开发模式（按 ~ 键）
3. 查看日志（按 F12 或查看 Player.log）
4. 搜索: "[RimTalk-Expand Memory] Loaded successfully"
```

**如果看到这条日志** → Mod已加载，继续下一步  
**如果没看到** → Mod未加载，检查安装

---

#### **2. 检查设置菜单位置**
```
主菜单 → Options → Mod Settings

在列表中找：
- "RimTalk-Expand Memory" ← 应该在这里
- 或 "RimTalk Expand Memory"
- 或 "RimTalk Memory Patch"
```

**如果找不到** → 继续诊断

---

#### **3. 检查DLL是否部署**

运行命令检查：
```powershell
Test-Path "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\1.6\Assemblies\RimTalkMemoryPatch.dll"
```

**应该返回：** `True`

**如果返回False** → 重新部署：
```cmd
.\deploy-now.bat
```

---

#### **4. 检查About.xml**

查看文件：
```
D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\About\About.xml
```

应该包含：
```xml
<ModMetaData>
  <name>RimTalk - Expand Memory</name>
  <packageId>cj.rimtalk.expandmemory</packageId>
  <modVersion>3.3.0</modVersion>
  ...
</ModMetaData>
```

---

#### **5. 检查Mod加载顺序**

在Mod管理器中：
```
确保顺序：
1. Harmony
2. Core  
3. RimTalk ← 必须
4. RimTalk-ExpandMemory ← 在这里

如果顺序错误 → 调整顺序 → 重启游戏
```

---

#### **6. 查看Player.log（详细诊断）**

日志位置：
```
Windows: C:\Users\[用户名]\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log
```

搜索关键词：
```
1. "RimTalk-Expand Memory"
2. "RimTalkMemoryPatchMod"
3. "SettingsCategory"
4. "Error" 或 "Exception"
```

---

## ?? 常见问题和解决方案

### **问题1：Mod列表中完全看不到**

**原因：** DLL未部署或About.xml错误

**解决：**
```cmd
# 重新部署
.\deploy-now.bat

# 检查文件
dir "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\*" /s
```

---

### **问题2：Mod在列表中，但无设置按钮**

**原因：** `SettingsCategory()` 方法未正确实现

**解决：** 检查RimTalkMod.cs是否正确编译到DLL

验证：
```powershell
# 检查DLL中是否包含SettingsCategory方法
# （需要反编译工具，如dnSpy）
```

---

### **问题3：设置打开后是空白**

**原因：** 翻译键缺失或UI渲染错误

**临时解决：** 
```csharp
// 在DoSettingsWindowContents中添加调试输出
Log.Message("Settings window opened!");
```

---

### **问题4：翻译键显示为原始键名**

**示例：** 显示 "RimTalk_Settings_KnowledgeLibraryTitle" 而不是 "常识库管理"

**原因：** 语言文件缺失

**解决：**
```
检查：Languages\ChineseSimplified\Keyed\MemoryPatch.xml

应该包含：
<LanguageData>
  <RimTalk_Settings_KnowledgeLibraryTitle>常识库管理</RimTalk_Settings_KnowledgeLibraryTitle>
  ...
</LanguageData>
```

---

## ?? 快速修复步骤

### **方案A：完全重新部署（推荐）**

```cmd
# 1. 清理旧版本
Remove-Item "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory" -Recurse -Force

# 2. 重新编译
dotnet build -c Release

# 3. 重新部署
.\deploy-now.bat

# 4. 重启RimWorld
```

---

### **方案B：手动验证（如果方案A无效）**

```cmd
# 1. 检查DLL
Test-Path "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\1.6\Assemblies\RimTalkMemoryPatch.dll"

# 2. 检查About.xml
Get-Content "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\About\About.xml"

# 3. 检查语言文件
Test-Path "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\Languages\ChineseSimplified\Keyed\MemoryPatch.xml"

# 4. 查看DLL大小（应该>100KB）
(Get-Item "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\1.6\Assemblies\RimTalkMemoryPatch.dll").Length
```

---

## ?? 应该看到的界面

### **正常的Mod设置界面应该是：**

```
╔═══════════════════════════════════╗
║ RimTalk - Expand Memory          ║
╠═══════════════════════════════════╣
║                                   ║
║ ?? 常识库管理                     ║
║ └─ [打开常识库] 按钮              ║
║                                   ║
║ ▼ 动态注入设置                    ║
║   ? 启用动态注入                  ║
║   最大注入记忆: 10 [滑块]        ║
║   ...                             ║
║                                   ║
║ ? 记忆容量设置                    ║
║ ? 衰减速率设置                    ║
║ ? 总结设置                        ║
║ ? AI配置                          ║
║                                   ║
╚═══════════════════════════════════╝
```

---

## ?? 如果以上都无效

### **最后手段：手动测试**

在DevMode下运行命令：
```
1. 按 ~ 开启DevMode
2. 按 F1 打开Debug Actions
3. 搜索 "Settings"
4. 找到 "Open Mod Settings"
5. 点击执行
```

然后在列表中找 "RimTalk-Expand Memory"

---

## ?? 报告问题

如果问题仍然存在，请提供：

```
1. Player.log 文件（最后100行）
2. Mod加载顺序截图
3. Mod设置界面截图（如果能打开）
4. RimWorld版本
5. RimTalk版本
```

---

**现在运行 `.\deploy-now.bat` 重新部署试试！** ??
