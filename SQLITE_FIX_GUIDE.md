# ?? SQLite.Interop.dll 加载失败修复指南

**错误信息：**
```
Exception loading SQLite.Interop.dll: System.BadImageFormatException
Invalid Image: D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\1.6\Assemblies\x64\SQLite.Interop.dll
```

---

## ?? **问题分析**

### **根本原因**
RimWorld是**32位（x86）程序**，但尝试加载了**64位（x64）**的SQLite.Interop.dll

### **为什么会发生？**
System.Data.SQLite会自动查找本地DLL：
1. 优先查找 `x64\SQLite.Interop.dll`（64位）
2. 如果失败，查找 `x86\SQLite.Interop.dll`（32位）

但如果x64目录存在且DLL存在，会先尝试加载，导致BadImageFormatException。

---

## ? **解决方案**

### **方案1：删除x64文件夹（推荐）**

RimWorld永远是32位，不需要x64版本。

**步骤：**
```powershell
# 删除x64文件夹
Remove-Item -Path "1.6\Assemblies\x64" -Recurse -Force

# 保留x86文件夹
# 1.6\Assemblies\x86\SQLite.Interop.dll
```

**优点：**
- ? 彻底解决问题
- ? 减少文件大小
- ? 避免混淆

**缺点：**
- ? 如果未来RimWorld变成64位，需要重新添加

---

### **方案2：修改加载顺序（高级）**

强制System.Data.SQLite优先加载x86版本。

**步骤：**
在Mod加载时，手动设置SQLite查找路径：

```csharp
// 在Mod.cs的构造函数中添加
[StaticConstructorOnStartup]
public class RimTalkMemoryPatchMod : Mod
{
    static RimTalkMemoryPatchMod()
    {
        // 强制使用x86版本的SQLite.Interop.dll
        string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string sqlitePath = Path.Combine(assemblyPath, "x86");
        
        // 设置DLL搜索路径
        Environment.SetEnvironmentVariable("PATH", 
            sqlitePath + ";" + Environment.GetEnvironmentVariable("PATH"));
    }
    
    // ...其他代码
}
```

**优点：**
- ? 保留x64文件夹（以备未来使用）

**缺点：**
- ? 需要修改代码
- ? 复杂度增加

---

### **方案3：使用.config文件（不推荐）**

创建`RimTalkMemoryPatch.dll.config`配置文件。

**不推荐原因：**
- RimWorld不支持DLL配置文件
- 过于复杂

---

## ?? **推荐操作**

### **立即执行（方案1）**

```powershell
# 1. 进入项目目录
cd "C:\Users\Administrator\Desktop\rim mod\RimTalk-ExpandMemory"

# 2. 删除x64文件夹
Remove-Item -Path "1.6\Assemblies\x64" -Recurse -Force

# 3. 验证x86文件夹存在
Get-ChildItem -Path "1.6\Assemblies\x86"

# 4. 重新编译（可选，但推荐）
msbuild /p:Configuration=Release

# 5. 提交Git
git add 1.6/Assemblies
git commit -m "Fix: Remove x64 SQLite.Interop.dll (RimWorld is x86 only)"
```

---

## ?? **文件结构对比**

### **修复前**
```
1.6/Assemblies/
├── RimTalkMemoryPatch.dll
├── System.Data.SQLite.dll
├── x64/
│   └── SQLite.Interop.dll  ← 导致错误
└── x86/
    └── SQLite.Interop.dll  ← 正确版本
```

### **修复后**
```
1.6/Assemblies/
├── RimTalkMemoryPatch.dll
├── System.Data.SQLite.dll
└── x86/
    └── SQLite.Interop.dll  ← 唯一版本，自动加载
```

---

## ?? **验证修复**

### **1. 检查文件结构**
```powershell
Get-ChildItem -Path "1.6\Assemblies" -Recurse
```

**预期输出：**
```
1.6/Assemblies/
├── RimTalkMemoryPatch.dll
├── System.Data.SQLite.dll
└── x86/
    └── SQLite.Interop.dll
```

**不应该有：**
- ? x64文件夹

### **2. 启动RimWorld测试**

启动游戏，查看日志：

**成功标志：**
```
[Vector DB] Initialized successfully
[Memory Manager] Database loaded
```

**失败标志：**
```
Exception loading SQLite.Interop.dll
BadImageFormatException
```

---

## ?? **自动化脚本**

创建一个修复脚本：

```powershell
# fix-sqlite.ps1
$projectRoot = "C:\Users\Administrator\Desktop\rim mod\RimTalk-ExpandMemory"
$x64Path = Join-Path $projectRoot "1.6\Assemblies\x64"

if (Test-Path $x64Path) {
    Write-Host "Removing x64 folder..." -ForegroundColor Yellow
    Remove-Item -Path $x64Path -Recurse -Force
    Write-Host "? x64 folder removed!" -ForegroundColor Green
} else {
    Write-Host "? x64 folder already removed!" -ForegroundColor Green
}

# 验证x86存在
$x86Path = Join-Path $projectRoot "1.6\Assemblies\x86\SQLite.Interop.dll"
if (Test-Path $x86Path) {
    Write-Host "? x86 SQLite.Interop.dll exists!" -ForegroundColor Green
} else {
    Write-Host "? x86 SQLite.Interop.dll missing!" -ForegroundColor Red
}

Write-Host "`nDone!" -ForegroundColor Cyan
```

---

## ?? **更新.gitignore**

确保不会再次提交x64文件夹：

```gitignore
# .gitignore
1.6/Assemblies/x64/
```

---

## ?? **总结**

### **问题**
RimWorld（x86）尝试加载x64版本的SQLite.Interop.dll → BadImageFormatException

### **原因**
System.Data.SQLite优先查找x64文件夹

### **解决**
删除`1.6/Assemblies/x64`文件夹，保留x86版本

### **结果**
- ? SQLite正常加载
- ? 向量数据库正常工作
- ? Mod稳定运行

---

**立即执行修复命令：**
```powershell
Remove-Item -Path "1.6\Assemblies\x64" -Recurse -Force
```
