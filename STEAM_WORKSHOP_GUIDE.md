# ?? Steam创意工坊发布指南

## ? 可以发布！但需要注意以下事项

---

## ?? 发布前检查清单

### **1. 依赖声明** ?
- [x] About.xml中已声明RimTalk依赖
- [x] 加载顺序正确（在RimTalk之后）
- [x] 描述中提醒用户安装RimTalk

### **2. 文件准备**
```
需要上传的文件：
? About/About.xml
? About/Preview.png (1024x1024或512x512)
? 1.6/Assemblies/RimTalkMemoryPatch.dll
? 1.6/Assemblies/System.Data.SQLite.dll ??
? 1.6/Assemblies/x86/SQLite.Interop.dll
? 1.6/Assemblies/x64/SQLite.Interop.dll
? Languages/ (如果有)
? Defs/ (如果有)

不要上传：
? Source/ (源代码)
? .git/
? *.md (文档)
? *.bat (脚本)
? *.pdb (调试文件)
```

### **3. SQLite处理** ??

**问题：** Steam可能拒绝第三方DLL

**解决方案A（推荐）：** 将SQLite设为可选
```xml
<description>
...
? Optional Features:
? Vector Database: Requires SQLite (auto-included, experimental)
  If missing, download from: [GitHub Release]
</description>
```

**解决方案B：** 提供无SQLite版本
```
发布两个版本:
1. 标准版（无向量数据库）
2. 完整版（含向量数据库）- GitHub Release
```

---

## ?? 发布步骤

### **方法1：使用RimWorld编辑器（推荐）**

#### **步骤1：准备Mod文件**
```cmd
# 运行打包脚本
.\package-release.bat

# 输出位置
Release\RimTalk-ExpandMemory-v3.2.0\
```

#### **步骤2：复制到RimWorld Mods**
```cmd
# 复制整个文件夹到
%LOCALAPPDATA%Low\Ludeon Studios\RimWorld by Ludeon Studios\Mods\
```

#### **步骤3：启动RimWorld**
```
1. 启动RimWorld
2. 主菜单 → Mods
3. 找到你的Mod
4. 点击 "Upload to Steam Workshop"
```

#### **步骤4：填写信息**
```
标题: RimTalk - Expand Memory
描述: (复制About.xml中的description)
标签: 
  - AI
  - Utility
  - 1.6
可见性: 公开
```

#### **步骤5：上传**
```
点击 "Upload" 等待完成
```

---

### **方法2：使用SteamCMD（高级）**

#### **步骤1：安装SteamCMD**
```
下载: https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip
解压到: C:\steamcmd\
```

#### **步骤2：创建VDF文件**
```vdf
"workshopitem"
{
  "appid"  "294100"
  "publishedfileid"  "0"
  "contentfolder"  "C:\path\to\RimTalk-ExpandMemory"
  "previewfile"  "C:\path\to\Preview.png"
  "visibility"  "0"
  "title"  "RimTalk - Expand Memory"
  "description"  "Enhanced AI Memory System for RimTalk..."
  "changenote"  "v3.2.0 - Vector Database Support"
}
```

#### **步骤3：上传**
```cmd
steamcmd +login your_username +workshop_build_item workshop_item.vdf +quit
```

---

## ?? 常见问题

### **问题1：Steam拒绝SQLite DLL**
```
错误: "Mod contains disallowed files"
```

**解决：**
1. 从创意工坊版本移除SQLite
2. 在描述中提供GitHub下载链接
3. 用户手动安装SQLite（可选功能）

**修改About.xml：**
```xml
<description>
...
? Vector Database (Optional):
  Download SQLite DLLs from:
  https://github.com/sanguodxj-byte/RimTalk-ExpandMemory/releases

  Extract to: RimWorld\Mods\RimTalk-ExpandMemory\1.6\Assemblies\
</description>
```

---

### **问题2：用户没有安装RimTalk**
```
错误: Mod dependency not found
```

**解决：** About.xml已配置自动提示
```xml
<modDependencies>
  <li>
    <packageId>ceteam.rimtalk</packageId>
    <steamWorkshopUrl>...</steamWorkshopUrl>
  </li>
</modDependencies>
```

RimWorld会自动提示用户订阅RimTalk

---

### **问题3：加载顺序错误**
```
错误: Mod loaded before dependency
```

**解决：** About.xml已配置
```xml
<loadAfter>
  <li>ceteam.rimtalk</li>
</loadAfter>
```

---

## ?? 创意工坊描述模板

### **中文版**

```
【RimTalk - 扩展记忆】增强AI记忆系统

?? 前置Mod：必须先安装 RimTalk
https://steamcommunity.com/sharedfiles/filedetails/?id=3381946324

?? 主要功能：
? 四层记忆架构（超短期→短期→事件日志→归档）
? 智能记忆注入（相关性评分）
? 自动记忆总结
? 常识库系统
? 语义嵌入（可选，需要API）
? 向量数据库（可选，实验性）

?? 详细文档：
https://github.com/sanguodxj-byte/RimTalk-ExpandMemory

?? 配置说明：
1. 先订阅并启用 RimTalk
2. 订阅本Mod，确保在RimTalk之后加载
3. 游戏内通过Mod设置配置功能

? 可选功能：
? 语义嵌入：需要DeepSeek/Gemini/OpenAI API（约$0.01/月）
? 向量数据库：需要SQLite（已包含，实验性）

?? 问题反馈：
https://github.com/sanguodxj-byte/RimTalk-ExpandMemory/issues

?? 交流讨论：
欢迎在下方评论区反馈问题和建议！

?? 版本：v3.2.0
?? 更新日志：查看GitHub Releases
```

---

### **英文版**

```
[RimTalk - Expand Memory] Enhanced AI Memory System

?? REQUIRES: RimTalk Mod (Install First!)
https://steamcommunity.com/sharedfiles/filedetails/?id=3381946324

?? Features:
? Four-layer memory architecture (ABM→SCM→ELS→CLPA)
? Smart memory injection with relevance scoring
? Automatic memory summarization
? Common knowledge library
? Semantic embedding (optional, API required)
? Vector database (optional, experimental)

?? Documentation:
https://github.com/sanguodxj-byte/RimTalk-ExpandMemory

?? Setup:
1. Subscribe & enable RimTalk first
2. Subscribe this mod, ensure it loads AFTER RimTalk
3. Configure in-game via Mod Options

? Optional Features:
? Semantic Embedding: Requires DeepSeek/Gemini/OpenAI API (~$0.01/month)
? Vector Database: SQLite included (experimental)

?? Issues:
https://github.com/sanguodxj-byte/RimTalk-ExpandMemory/issues

?? Feedback welcome in comments!

?? Version: v3.2.0
?? Changelog: See GitHub Releases
```

---

## ??? Preview.png 要求

### **尺寸**
```
推荐: 1024x1024 (正方形)
最小: 512x512
格式: PNG
```

### **内容建议**
```
? Mod名称（大标题）
? 主要功能图标
? RimWorld风格
? 简洁清晰
```

### **示例布局**
```
┌─────────────────────────┐
│  RimTalk                │
│  Expand Memory          │
│                         │
│  [记忆图标]             │
│  [数据库图标]           │
│  [AI图标]               │
│                         │
│  Enhanced Memory        │
│  for RimTalk AI         │
│                         │
│  v3.2.0                 │
└─────────────────────────┘
```

---

## ?? 发布策略

### **方案A：完整发布（推荐）**
```
优点:
? 功能完整
? 用户体验好
? 一次订阅搞定

缺点:
?? 可能被Steam拒绝SQLite
?? 文件较大（~5MB）

应对:
- 先尝试完整上传
- 如被拒，改用方案B
```

### **方案B：分离发布**
```
创意工坊版本:
- 核心功能（无SQLite）
- 语义嵌入（需用户配置API）
- 文件小（~1MB）

GitHub Release:
- 完整版本（含SQLite）
- 在创意工坊描述中提供下载链接

优点:
? 不会被Steam拒绝
? 核心功能可用

缺点:
? 用户需额外步骤
? 向量数据库需手动安装
```

---

## ?? 推荐发布方案

### **第1阶段：标准版**
```
发布内容:
? 核心记忆系统
? 智能注入
? 自动总结
? 常识库
? SQLite（移除）
? 向量数据库（禁用）

目标:
- 快速通过审核
- 获取用户反馈
```

### **第2阶段：完整版（可选）**
```
GitHub Release:
? 完整功能
? SQLite依赖
? 向量数据库

在创意工坊描述中添加:
"?? Advanced Features:
  Download full version with Vector DB from:
  https://github.com/sanguodxj-byte/RimTalk-ExpandMemory/releases"
```

---

## ?? 更新流程

### **修复Bug**
```cmd
# 1. 修改代码
# 2. 编译
dotnet build -c Release

# 3. 测试
.\quick-deploy.bat

# 4. 打包
.\package-release.bat

# 5. 上传创意工坊
（在RimWorld中点击Update）
```

### **添加功能**
```
1. 更新About.xml中的版本号
2. 编写更新日志
3. 发布GitHub Release
4. 更新创意工坊描述
5. 上传新版本
```

---

## ? 最终检查

### **发布前确认**
- [ ] About.xml依赖声明正确
- [ ] 描述中提醒安装RimTalk
- [ ] 移除源代码和文档
- [ ] 移除调试文件(.pdb)
- [ ] 测试加载顺序
- [ ] 测试核心功能
- [ ] 准备Preview.png

### **首次发布后**
- [ ] 监控评论区
- [ ] 收集Bug反馈
- [ ] 准备热修复
- [ ] 更新GitHub Issue

---

## ?? 建议

### **推荐发布方案**
```
第一次发布:
- 使用方案A（完整版）
- 如被拒，立即改为方案B

理由:
- 大多数Mod可以包含第三方DLL
- SQLite是常见库，通常不会被拒
- 提供最佳用户体验
```

### **如果被拒绝**
```
1. 移除SQLite相关文件
2. 在Settings中默认禁用向量数据库
3. 在描述中提供GitHub下载链接
4. 重新上传
```

---

**准备好了就发布吧！** ???

需要我帮你创建Preview.png设计方案吗？
