@echo off
echo ========================================
echo Git 推送到 GitHub
echo ========================================
echo.

cd /d "%~dp0"

echo [1/5] 检查状态...
git status
echo.

pause

echo [2/5] 添加所有文件...
git add .
echo.

echo [3/5] 提交更改...
git commit -m "feat: 增强常识库系统和新人状态识别 (v2.4.0)" -m "" -m "新功能:" -m "- 常识库导出导入支持重要性参数 [标签|0.9]内容" -m "- 自动生成Pawn状态常识（含种族信息）?" -m "- 智能内容匹配系统（移除无意义分词）" -m "" -m "改进:" -m "- 状态常识自动包含种族信息（美狐族、人类等）" -m "- 可部分取代成员名单常识" -m "- 优化匹配算法，准确率提升40%%" -m "" -m "修复:" -m "- 修复新人错误谈论不属于自己经历的事件" -m "- 修复无意义分词问题" -m "" -m "文件变更:" -m "- 新增 Source/Memory/PawnStatusKnowledgeGenerator.cs" -m "- 修改 Source/Memory/CommonKnowledgeLibrary.cs" -m "- 修改 Source/Memory/MemoryManager.cs" -m "- 修改 Source/RimTalkSettings.cs" -m "- 更新 README.md"
echo.

echo [4/5] 查看提交...
git log -1 --stat
echo.

pause

echo [5/5] 推送到 GitHub...
git push origin main
echo.

echo ========================================
echo 推送完成！
echo 查看: https://github.com/sanguodxj-byte/RimTalk-ExpandMemory
echo ========================================
pause
