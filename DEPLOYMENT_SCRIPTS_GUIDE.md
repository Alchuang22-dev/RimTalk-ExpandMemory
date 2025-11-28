# ?? 部署脚本使用指南

## ?? 可用脚本

### **1. quick-deploy.bat** ? (推荐日常使用)
```
用途: 快速部署到RimWorld游戏目录
速度: 最快 (~5秒)
场景: 开发测试、快速迭代
```

**使用方法:**
```cmd
.\quick-deploy.bat
```

**注意事项:**
- 修改脚本中的RimWorld路径（默认：`D:\steam\steamapps\common\RimWorld`）
- 自动清理旧版本
- 不包含文档，仅Mod文件

---

### **2. deploy-v3.2.bat** ?? (完整部署)
```
用途: 完整部署（包含SQLite检测和向量数据库）
速度: 中等 (~10秒)
场景: 正式部署、功能测试
```

**使用方法:**
```cmd
.\deploy-v3.2.bat
```

**功能特点:**
- ? 自动检测SQLite依赖
- ? 询问是否启用向量数据库
- ? 从NuGet自动复制SQLite文件
- ? 详细部署日志
- ? 文件验证

**交互式选项:**
```
是否启用向量数据库功能？
  [Y] 是 - 复制SQLite依赖（向量数据库可用）
  [N] 否 - 跳过SQLite（禁用向量数据库）
```

---

### **3. package-release.bat** ?? (发布打包)
```
用途: 创建发布包（GitHub Release / Steam Workshop）
速度: 慢 (~20秒)
场景: 正式发布
```

**使用方法:**
```cmd
.\package-release.bat
```

**输出内容:**
```
Release/
  ├─ RimTalk-ExpandMemory-v3.2.0/     (Mod文件夹)
  │  ├─ About/
  │  ├─ 1.6/Assemblies/
  │  ├─ Defs/
  │  ├─ Languages/
  │  ├─ README.md
  │  └─ Docs/
  │
  └─ RimTalk-ExpandMemory-v3.2.0.zip  (压缩包)
```

---

### **4. build-release.bat** ??? (旧版，保留兼容)
```
用途: 基础部署（v3.0兼容）
速度: 快
场景: 不需要SQLite时使用
```

---

## ?? 推荐工作流

### **开发阶段**
```cmd
# 1. 修改代码
# 2. 编译（VS自动 或 dotnet build）
# 3. 快速部署
.\quick-deploy.bat

# 4. 启动RimWorld测试
# 5. 重复1-4
```

### **测试阶段**
```cmd
# 1. 完整编译
dotnet build -c Release

# 2. 完整部署（包含SQLite）
.\deploy-v3.2.bat
选择 [Y] 启用向量数据库

# 3. 全面测试
- 基础功能
- 语义嵌入
- 向量数据库
```

### **发布阶段**
```cmd
# 1. 确认版本号（About.xml）
# 2. 更新CHANGELOG.md
# 3. 打包发布
.\package-release.bat

# 4. 上传
- GitHub Release
- Steam Workshop
```

---

## ?? 配置说明

### **修改RimWorld路径**

**quick-deploy.bat:**
```cmd
set "RIMWORLD=D:\steam\steamapps\common\RimWorld"  ← 修改这里
```

**deploy-v3.2.bat:**
```cmd
set "RIMWORLD_DIR=D:\steam\steamapps\common\RimWorld"  ← 修改这里
```

### **修改版本号**

**package-release.bat:**
```cmd
set "VERSION=3.2.0"  ← 修改这里
```

**About.xml:**
```xml
<modVersion>3.2.0</modVersion>  ← 同步修改
```

---

## ?? 脚本对比

| 特性 | quick-deploy | deploy-v3.2 | package-release | build-release |
|------|-------------|-------------|-----------------|---------------|
| **速度** | ??? | ?? | ? | ?? |
| **SQLite检测** | ? | ? | ? | ? |
| **自动NuGet** | ? | ? | ? | ? |
| **文件验证** | ? | ? | ? | ?? |
| **文档打包** | ? | ? | ? | ? |
| **ZIP压缩** | ? | ? | ? | ? |
| **推荐场景** | 开发测试 | 完整部署 | 正式发布 | 简单部署 |

---

## ?? 故障排除

### **问题1: "DLL不存在"**
```
原因: 未编译或编译失败
解决: 
  1. 打开项目在VS中编译
  2. 或运行: dotnet build -c Release
```

### **问题2: "SQLite.Interop.dll未找到"**
```
原因: NuGet包未正确还原
解决:
  1. 运行: dotnet restore
  2. 重新编译
  3. 或手动从NuGet包复制
```

### **问题3: "RimWorld目录不存在"**
```
原因: 路径配置错误
解决: 修改脚本中的RIMWORLD_DIR变量
```

### **问题4: "ZIP打包失败"**
```
原因: PowerShell不可用
解决: 手动压缩Release文件夹
```

---

## ?? 最佳实践

### **1. 版本管理**
```cmd
# 更新版本前
1. 修改 About.xml → modVersion
2. 修改 package-release.bat → VERSION
3. 更新 CHANGELOG.md

# 确保一致性
```

### **2. 测试流程**
```cmd
# 最小测试
.\quick-deploy.bat → 启动RimWorld → 测试基础功能

# 完整测试
.\deploy-v3.2.bat (启用SQLite) → 全面测试 → 确认无误

# 发布前
.\package-release.bat → 解压检查 → 手动测试ZIP包
```

### **3. 清理策略**
```cmd
# 开发时
- 使用quick-deploy（自动清理）

# 发布前
- 删除Release文件夹
- 重新打包
```

---

## ?? 快速参考

### **日常开发**
```cmd
code .              # 编辑代码
# VS自动编译 或 Ctrl+Shift+B
.\quick-deploy.bat  # 部署测试
```

### **完整部署**
```cmd
dotnet build -c Release
.\deploy-v3.2.bat
# 选择 [Y] 启用向量数据库
```

### **发布版本**
```cmd
# 1. 更新版本号
# 2. 打包
.\package-release.bat

# 3. 上传
git tag v3.2.0
git push --tags
# 上传 Release\*.zip 到GitHub
```

---

## ?? 脚本维护

### **添加新功能时**
1. 更新 `deploy-v3.2.bat` - 添加新文件复制逻辑
2. 更新 `package-release.bat` - 确保新文件被打包
3. 更新本文档 - 说明新功能

### **修改目录结构时**
1. 检查所有脚本中的路径
2. 更新robocopy/xcopy命令
3. 测试所有脚本

---

**选择合适的脚本，让部署更高效！** ???
