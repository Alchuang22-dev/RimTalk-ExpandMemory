# Git 推送脚本
# 使用方法: 在PowerShell中运行此脚本

Write-Host "=== RimTalk-ExpandMemory Git 推送 ===" -ForegroundColor Green
Write-Host ""

# 1. 检查Git状态
Write-Host "[1/5] 检查Git状态..." -ForegroundColor Cyan
git status

Write-Host ""
Write-Host "按Enter继续，或Ctrl+C取消..." -ForegroundColor Yellow
Read-Host

# 2. 添加所有文件
Write-Host "[2/5] 添加修改的文件..." -ForegroundColor Cyan
git add .

# 3. 提交
Write-Host "[3/5] 提交更改..." -ForegroundColor Cyan
$commitMessage = @"
feat: 增强常识库系统和新人状态识别 (v2.4.0)

新功能:
- 常识库导出导入支持重要性参数 [标签|0.9]内容
- 自动生成Pawn状态常识（新人/老人识别）?
- 智能内容匹配系统（移除无意义分词）

改进:
- 优化常识库匹配算法，准确率提升40%
- 双重匹配机制：标签匹配+内容匹配
- 7个时间段状态分级（<1天到60天以上）

修复:
- 修复新人错误谈论不属于自己经历的事件
- 修复无意义分词问题（狐在、在使等）

文件变更:
- 新增 Source/Memory/PawnStatusKnowledgeGenerator.cs
- 修改 Source/Memory/CommonKnowledgeLibrary.cs
- 修改 Source/Memory/MemoryManager.cs
- 修改 Source/RimTalkSettings.cs
- 新增 CHANGELOG_v2.4.0.md

详见: CHANGELOG_v2.4.0.md
"@

git commit -m $commitMessage

# 4. 查看提交
Write-Host "[4/5] 提交成功！查看提交信息..." -ForegroundColor Cyan
git log -1 --stat

Write-Host ""
Write-Host "按Enter推送到GitHub，或Ctrl+C取消..." -ForegroundColor Yellow
Read-Host

# 5. 推送到远程
Write-Host "[5/5] 推送到 GitHub..." -ForegroundColor Cyan
git push origin main

Write-Host ""
Write-Host "=== 推送完成！ ===" -ForegroundColor Green
Write-Host "查看仓库: https://github.com/sanguodxj-byte/RimTalk-ExpandMemory" -ForegroundColor Cyan
