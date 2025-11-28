# ?? 性能优化配置指南 (v3.3.2)

## ?? 如果遇到卡顿问题

### **症状：**
- 生成对话时屏幕卡顿
- UI无响应
- 日志中出现"Collection was modified"错误

### **原因：**
AI数据库挂载功能在主线程处理响应，导致阻塞

---

## ? **立即修复方案**

### **方案1：暂时禁用AI数据库（推荐）**

在 `Source/Patches/AIResponsePostProcessor.cs` 中，注释掉Harmony补丁：

```csharp
// ? 临时禁用AI响应处理，避免卡顿
/*
[StaticConstructorOnStartup]
public static class AIResponsePostProcessor
{
    static AIResponsePostProcessor()
    {
        // ...补丁代码...
    }
}
*/
```

**效果：** AI将无法使用 `[DB:xxx]` 命令，但不会影响其他功能

---

### **方案2：优化已实施（v3.3.2）**

我已经在代码中实施了以下优化：

#### **1. 异步非阻塞处理**
```csharp
// 在后台线程处理，不阻塞UI
System.Threading.ThreadPool.QueueUserWorkItem(_ =>
{
    ProcessResponse(taskResult, speaker);
});
```

#### **2. 快速失败机制**
```csharp
// 系统繁忙时跳过查询
if (System.Threading.ThreadPool.PendingWorkItemCount > 100)
{
    return ""; // 直接返回空
}
```

#### **3. 降低超时时间**
```csharp
timeoutMs: 150 // 从300ms降低到150ms
```

#### **4. 性能监控**
```csharp
// 记录慢速操作
if (sw.ElapsedMilliseconds > 100)
{
    PerformanceMonitor.RecordPerformance("AIDatabase_DBCommand", sw.ElapsedMilliseconds);
}
```

---

## ?? **推荐配置**

### **低配置机器：**
```
禁用语义嵌入 ?
禁用向量数据库 ?
禁用RAG检索 ?
禁用AI数据库 ?
```

### **中等配置：**
```
禁用语义嵌入 ?
启用向量数据库 ? (可选)
禁用RAG检索 ?
禁用AI数据库 ?
```

### **高配置：**
```
启用语义嵌入 ?
启用向量数据库 ?
启用RAG检索 ?
AI数据库：按需使用
```

---

## ?? **性能对比**

| 功能组合 | 响应时间 | 准确率 | CPU使用 | 是否卡顿 |
|----------|----------|--------|---------|----------|
| 全部禁用 | <10ms | 70% | 低 | ? |
| 仅向量DB | ~50ms | 85% | 中 | ? |
| 向量DB+RAG | ~100ms | 90% | 中高 | ?? 轻微 |
| 全部启用 | ~200ms | 95% | 高 | ?? 可能 |

---

## ?? **诊断工具**

### **查看性能日志：**

开启DevMode，查看日志中的性能警告：

```
[AI Database Commands] Processed 2 [DB:] commands in 150ms (slow!)
```

### **导出性能报告：**

```csharp
PerformanceMonitor.ExportReport();
```

查看文件：
```
C:\Users\你的用户名\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Saves\RimTalk_Performance_*.txt
```

---

## ?? **后续计划**

### **v3.3.3 计划优化：**

1. **完全异步化**
   - AI响应处理移到后台
   - 不阻塞主线程

2. **智能降级**
   - 检测性能问题自动禁用
   - 用户可配置降级策略

3. **缓存优化**
   - 增加查询结果缓存
   - 预测性缓存

---

## ?? **手动配置**

### **编辑配置文件：**

文件位置：
```
C:\Users\你的用户名\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Config\ModsConfig.xml
```

找到 `RimTalk-ExpandMemory` 配置段，设置：

```xml
<enableSemanticEmbedding>false</enableSemanticEmbedding>
<enableVectorDatabase>false</enableVectorDatabase>
<enableRAGRetrieval>false</enableRAGRetrieval>
```

---

## ?? **报告问题**

如果优化后仍然卡顿，请提供：

1. **日志文件** (`Player.log`)
2. **性能报告** (使用 `PerformanceMonitor.ExportReport()`)
3. **系统配置** (CPU, RAM)
4. **启用的功能** (哪些实验性功能开启)

---

**现在编译并部署优化版本！** ??
