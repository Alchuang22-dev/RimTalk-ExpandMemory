# ? RimTalk ExpandMemory v3.3.2 - 最终部署检查清单

**版本：** v3.3.2  
**发布日期：** 2025-01-XX  
**检查时间：** _______________

---

## ?? **部署前检查**

### **1. 代码完整性**

- [ ] 所有修改文件已提交
  - [ ] `Source\Patches\AIResponsePostProcessor.cs`
  - [ ] `Source\Memory\AIDatabase\AIDatabaseCommands.cs`
  - [ ] `Source\Memory\MemoryTypes.cs`
  - [ ] `Source\Memory\EventRecordKnowledgeGenerator.cs`
  - [ ] `Source\Memory\PawnStatusKnowledgeGenerator.cs`
  - [ ] `Source\Memory\AI\EmbeddingService.cs`
  - [ ] `Source\Memory\MemoryManager.cs`
  - [ ] `Source\Memory\SemanticScoringSystem.cs`
  - [ ] `CHANGELOG_v3.3.2.md`

- [ ] 编译状态检查
  ```
  运行命令：MSBuild RimTalk-ExpandMemory.csproj /p:Configuration=Release
  预期结果：Build succeeded. 0 Warning(s). 0 Error(s).
  ```

- [ ] DLL文件生成
  - [ ] 文件存在：`1.6\Assemblies\RimTalkMemoryPatch.dll`
  - [ ] 文件大小：约200-300KB
  - [ ] 文件日期：最新

### **2. 版本信息**

- [ ] `About\About.xml` 版本号已更新
  ```xml
  <version>3.3.2</version>
  ```

- [ ] `CHANGELOG_v3.3.2.md` 完整无误
  - [ ] 核心修复说明
  - [ ] 性能优化说明
  - [ ] 已知问题说明
  - [ ] 性能对比数据

- [ ] `README.md` 版本号已更新
  ```markdown
  ## 最新版本
  **v3.3.2** - 性能优化与卡顿修复
  ```

---

## ?? **功能测试**

### **3. 基础功能测试**

启动游戏并测试以下功能：

- [ ] Mod正常加载
  - [ ] 无红色错误
  - [ ] 无黄色警告（除已知问题）

- [ ] 对话生成
  - [ ] 对话流畅，无卡顿 ?
  - [ ] UI响应正常 ?
  - [ ] Token隐藏式命令生效（看不到`[DB:xxx]`）

- [ ] 时间显示
  - [ ] 记忆时间显示口语化（"刚才"、"昨天"等）
  - [ ] 不显示精确时间戳

- [ ] 日志输出
  - [ ] Player.log干净（日志明显减少）
  - [ ] 无频繁的Embedding/RAG日志
  - [ ] 错误日志正常显示

### **4. 性能测试**

- [ ] AI响应速度
  - [ ] 对话生成无卡顿
  - [ ] UI保持流畅
  - [ ] 预期响应<10ms

- [ ] 语义评分
  - [ ] 首次启动偶尔超时（正常）
  - [ ] 运行一段时间后超时减少
  - [ ] 超时后自动降级（不影响游戏）

- [ ] Token消耗
  - [ ] 开发者模式查看对话历史
  - [ ] 确认查询结果未显示给用户
  - [ ] Token明显减少

### **5. 边缘情况测试**

- [ ] 冷启动测试
  - [ ] 首次启动游戏
  - [ ] 记录超时频率（预期：80-100%）
  - [ ] 确认自动降级正常

- [ ] 长时间运行
  - [ ] 游戏运行2小时+
  - [ ] 记录超时频率（预期：<10%）
  - [ ] 确认缓存生效

- [ ] 禁用语义嵌入
  - [ ] 设置中禁用语义嵌入
  - [ ] 确认仍然流畅运行
  - [ ] 无超时警告

---

## ?? **文件准备**

### **6. 发布包准备**

- [ ] 创建发布目录：`release_temp\`

- [ ] 复制文件
  - [ ] `1.6\` → `release_temp\1.6\`
  - [ ] `About\` → `release_temp\About\`
  - [ ] `README.md` → `release_temp\`
  - [ ] `CHANGELOG_v3.3.2.md` → `release_temp\`

- [ ] 压缩包创建
  - [ ] 文件名：`RimTalk-ExpandMemory-v3.3.2.zip`
  - [ ] 文件大小：<5MB
  - [ ] 测试解压正常

### **7. 文档准备**

- [ ] `DEPLOYMENT_SUMMARY_v3.3.2.md`（本地记录）
- [ ] `STEAM_WORKSHOP_RELEASE_v3.3.2.md`（Steam描述）
- [ ] GitHub Release说明（稍后创建）

---

## ?? **部署执行**

### **8. Git提交与推送**

- [ ] Git状态检查
  ```bash
  git status
  预期：所有更改已暂存
  ```

- [ ] 提交信息检查
  ```bash
  git log -1
  预期：包含v3.3.2完整说明
  ```

- [ ] 推送到GitHub
  ```bash
  git push origin main
  预期：推送成功，无错误
  ```

- [ ] GitHub在线检查
  - [ ] 提交历史正确
  - [ ] 文件内容无误
  - [ ] CHANGELOG可访问

### **9. GitHub Release创建**

访问：https://github.com/sanguodxj-byte/RimTalk-ExpandMemory/releases/new

- [ ] Tag版本：`v3.3.2`
- [ ] Release标题：`v3.3.2 - 性能优化与卡顿修复`
- [ ] Release描述：（复制自`STEAM_WORKSHOP_RELEASE_v3.3.2.md`）
- [ ] 上传附件：`RimTalk-ExpandMemory-v3.3.2.zip`
- [ ] 勾选：`Set as the latest release`
- [ ] 点击：`Publish release`

### **10. Steam Workshop更新**

- [ ] 打开Steam Workshop管理页面

- [ ] 更新文件
  - [ ] 删除旧版本文件
  - [ ] 上传新版本文件（从`release_temp\`）

- [ ] 更新描述
  - [ ] 标题：`RimTalk ExpandMemory v3.3.2`
  - [ ] 简短描述：紧急卡顿修复，推荐更新
  - [ ] 完整描述：（复制自`STEAM_WORKSHOP_RELEASE_v3.3.2.md`）
  - [ ] 更新日志：（复制自`CHANGELOG_v3.3.2.md`）

- [ ] 更新标签
  - [ ] `1.5`, `1.6`
  - [ ] `Quality of Life`
  - [ ] `Performance`
  - [ ] `AI`

- [ ] 可见性设置
  - [ ] `Public`（公开）

- [ ] 点击：`Save & Exit`

---

## ?? **发布通知**

### **11. 用户通知**

- [ ] Steam Workshop公告
  ```
  标题：v3.3.2紧急更新 - 修复卡顿问题
  内容：（简要说明核心修复）
  ```

- [ ] GitHub Discussions（如有）
  ```
  话题：v3.3.2发布
  内容：（更新日志摘要）
  ```

- [ ] Discord通知（如有）
  ```
  频道：#announcements
  内容：@everyone v3.3.2已发布！
  ```

### **12. 监控准备**

- [ ] 设置GitHub Issues通知

- [ ] 关注Steam Workshop评论

- [ ] 准备快速响应模板
  - [ ] 常见问题FAQ
  - [ ] 调试信息收集指南
  - [ ] 已知问题说明

---

## ?? **部署后监控**

### **13. 首日监控（0-24小时）**

- [ ] 崩溃报告：预期<5个
- [ ] 卡顿报告：预期0个
- [ ] 超时警告反馈：预期少量（已说明）
- [ ] 用户满意度：预期>80%好评

### **14. 首周监控（1-7天）**

- [ ] 累计崩溃率：预期<0.1%
- [ ] 性能问题报告：预期<10个
- [ ] 超时频率统计：验证是否符合CHANGELOG
- [ ] 用户满意度：预期>90%好评

### **15. 问题追踪**

已知问题追踪：

| 问题 | 预期频率 | 实际频率 | 状态 |
|------|---------|---------|------|
| 语义评分超时 | 5-15%（正常运行） | _____ | _____ |
| Collection modified | 偶尔（可忽略） | _____ | _____ |
| 其他 | - | _____ | _____ |

---

## ? **最终确认**

### **16. 部署完成检查**

- [ ] Git推送成功
- [ ] GitHub Release创建
- [ ] Steam Workshop更新
- [ ] 用户通知发送
- [ ] 监控系统就绪

### **17. 回滚准备**

如果出现严重问题，回滚方案：

- [ ] 旧版本DLL备份：`backup\v3.3.1\`
- [ ] Git回滚命令准备：
  ```bash
  git revert HEAD
  git push origin main
  ```
- [ ] Steam Workshop回滚准备

---

## ?? **部署总结**

**部署时间：** _______________  
**部署人员：** _______________  
**部署结果：** [ ] 成功 / [ ] 失败 / [ ] 部分成功

**遇到的问题：**
```
（记录部署过程中遇到的问题）
```

**解决方案：**
```
（记录问题的解决方法）
```

**备注：**
```
（其他需要记录的信息）
```

---

## ?? **紧急联系**

**负责人：** _______________  
**联系方式：** _______________

**紧急问题处理流程：**
1. 评估问题严重性
2. 决定是否需要回滚
3. 通知用户
4. 准备热修复（如需要）

---

**检查完成，准备部署！** ??
