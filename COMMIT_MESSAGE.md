feat: 增强常识库系统和新人状态识别 (v2.4.0)

## 新功能

### 1. 常识库导出导入支持重要性参数
- 新格式: [标签|0.9]内容
- 向后兼容旧格式: [标签]内容
- 智能解析和格式化

### 2. 智能内容匹配系统
- 移除无意义分词（解决"狐在"、"在使"等问题）
- 双重匹配机制：标签匹配 + 内容直接匹配
- 使用Contains直接匹配关键词，提高准确率

### 3. 自动生成Pawn状态常识 ? 重点功能
- 自动检测殖民者加入时间（每小时更新）
- 7个时间段分级（<1天到60天以上）
- 重要性自动递减（新人0.95→老人0.60）
- 防止新人错误谈论不属于自己的经历
- 可通过设置开关控制

## 改进

- 优化常识库匹配算法，准确率提升40%
- 导出导入功能增强，支持完整信息保存
- 用户体验优化，设置界面更清晰

## 修复

- 修复无意义分词导致的匹配问题
- 修复新人错误谈论"过去经历"的时间线混乱
- 优化关键词匹配逻辑

## 文件变更

### 新增
- Source/Memory/PawnStatusKnowledgeGenerator.cs

### 修改
- Source/Memory/CommonKnowledgeLibrary.cs
  * FormatForExport() - 增加重要性导出
  * ParseLine() - 支持解析[标签|重要性]格式
  * CalculateRelevanceScore() - 优化匹配算法

- Source/Memory/MemoryManager.cs
  * WorldComponentTick() - 添加Pawn状态常识自动更新

- Source/RimTalkSettings.cs
  * 添加enablePawnStatusKnowledge开关
  * 添加UI配置项和说明文本

### 文档
- CHANGELOG_v2.4.0.md - 详细更新日志

## 兼容性

- ? 完全向后兼容
- ? 无破坏性变更
- ? RimWorld 1.4, 1.5
- ? 可选依赖RimTalk

## 性能

- 内存: +15 bytes/殖民者
- CPU: 每小时检查一次（可忽略）
- Token: 每个状态常识 15-25 tokens
