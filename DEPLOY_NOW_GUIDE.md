# ?? 立即部署指南

## ?? 你的RimWorld路径

```
RimWorld: D:\steam\steamapps\common\RimWorld
Mods目录: D:\steam\steamapps\common\RimWorld\Mods
目标Mod: RimTalk-ExpandMemory
```

---

## ? 最快部署方式

### **方法1：一键部署（推荐）**

直接运行：
```cmd
.\deploy-now.bat
```

**这个脚本会：**
1. 自动检测并编译（如需要）
2. 清理旧版本
3. 复制所有Mod文件到RimWorld\Mods
4. 清理调试文件
5. 验证部署结果

**时间：** ~10秒

---

### **方法2：手动部署**

```cmd
# 1. 编译
dotnet build -c Release

# 2. 复制整个1.6文件夹和About文件夹
xcopy "About" "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\About\" /E /I /Y
xcopy "1.6" "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\1.6\" /E /I /Y

# 3. 删除调试文件
del "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\1.6\Assemblies\*.pdb"
```

---

## ?? 部署检查清单

### **编译前检查**
- [ ] 已安装.NET Framework 4.7.2 SDK
- [ ] 已还原NuGet包
- [ ] 无编译错误

### **部署后验证**
- [ ] About.xml存在
- [ ] RimTalkMemoryPatch.dll存在
- [ ] SQLite文件存在（可选）
- [ ] 无.pdb调试文件

### **游戏内检查**
- [ ] Mod出现在列表中
- [ ] 加载顺序正确（RimTalk之后）
- [ ] 无红色错误提示
- [ ] Mod设置可打开

---

## ?? 完整文件结构

部署后，你的Mod目录应该是：

```
D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\
├─ About\
│  ├─ About.xml
│  └─ Preview.png (可选)
│
├─ 1.6\
│  └─ Assemblies\
│     ├─ RimTalkMemoryPatch.dll ? 必需
│     ├─ System.Data.SQLite.dll (向量DB)
│     ├─ x86\
│     │  └─ SQLite.Interop.dll
│     └─ x64\
│        └─ SQLite.Interop.dll
│
├─ Defs\ (可选)
├─ Languages\ (可选)
└─ Textures\ (可选)
```

---

## ?? 启动游戏

### **1. 启用Mod**
```
主菜单 → Mods → 找到 "RimTalk - Expand Memory"
点击勾选框启用
```

### **2. 调整加载顺序**
```
确保顺序:
1. Harmony
2. Core
3. RimTalk
4. RimTalk-ExpandMemory ← 必须在RimTalk之后
5. 其他Mods
```

### **3. 重启游戏**
```
点击 "Restart" 或关闭游戏重新启动
```

---

## ?? 配置建议

### **首次使用（保守配置）**

进入Mod设置 → RimTalk-ExpandMemory：

```
? 启用动态注入
? 启用每日总结
? 启用Pawn状态常识

? 禁用语义嵌入（需API）
? 禁用向量数据库（实验性）
? 禁用RAG检索（实验性）
```

### **完整功能（需配置API）**

```
? 启用所有基础功能
? 启用语义嵌入
  → 配置API Key（DeepSeek/OpenAI/Gemini）
? 启用向量数据库
? 启用RAG检索
```

---

## ?? 常见问题

### **问题1：Mod不显示**
```
检查:
1. 文件夹名称: RimTalk-ExpandMemory
2. About.xml是否存在
3. RimWorld版本: 1.4+
```

### **问题2：红色错误**
```
检查:
1. RimTalk是否已安装
2. 加载顺序是否正确
3. 查看Player.log文件
```

### **问题3：功能不工作**
```
检查:
1. Mod设置是否正确
2. 是否在游戏中（非主菜单）
3. DevMode查看日志
```

---

## ?? 验证部署

### **快速验证**
```cmd
# 运行deploy-now.bat后会自动显示验证结果
```

### **手动验证**
```cmd
dir "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\1.6\Assemblies"

# 应该看到:
RimTalkMemoryPatch.dll
System.Data.SQLite.dll (可选)
x86\ (文件夹)
x64\ (文件夹)
```

### **游戏内验证**
```
1. 启动游戏
2. 打开DevMode（按 ~ 键）
3. 查看Log中是否有:
   [RimTalk Memory Patch v3.3] Successfully patched...
```

---

## ?? 部署历史

### **v3.3.0**
- 部署时间: [待填写]
- 功能: RAG检索 + 向量DB + 语义嵌入
- 状态: ? / ?

### **v3.2.0**
- 功能: 向量数据库
- 状态: ? / ?

### **v3.1.0**
- 功能: 语义嵌入
- 状态: ? / ?

### **v3.0.0**
- 功能: 智能注入 + 主动召回
- 状态: ? / ?

---

## ?? 立即开始

**最简单的方式：**

1. 打开命令行
2. 进入项目目录
3. 运行：
```cmd
.\deploy-now.bat
```

4. 等待完成
5. 启动RimWorld
6. 享受！

---

**准备好了吗？运行 `deploy-now.bat` 开始部署！** ???
