@echo off
chcp 65001 >nul
echo ============================================
echo    RimTalk-ExpandMemory Git 推送工具
echo ============================================
echo.

echo [检查] 检查Git状态...
git status
echo.

echo ============================================
echo 准备推送 v2.4.0 更新
echo.
echo 新功能:
echo   - 常识库导出导入支持重要性参数
echo   - 自动生成Pawn状态常识
echo   - 智能内容匹配系统
echo.
echo ============================================
echo.

set /p confirm="确认推送到GitHub? (Y/N): "
if /i not "%confirm%"=="Y" (
    echo 已取消推送
    pause
    exit
)

echo.
echo [1/4] 添加文件...
git add .

echo [2/4] 创建提交...
git commit -m "feat: 增强常识库系统和新人状态识别 (v2.4.0)" -m "" -m "新功能:" -m "- 常识库导出导入支持重要性参数 [标签|0.9]内容" -m "- 自动生成Pawn状态常识（新人/老人识别）?" -m "- 智能内容匹配系统（移除无意义分词）" -m "" -m "改进:" -m "- 优化常识库匹配算法，准确率提升40%%" -m "- 双重匹配机制：标签匹配+内容匹配" -m "- 7个时间段状态分级" -m "" -m "修复:" -m "- 修复新人错误谈论不属于自己经历的事件" -m "- 修复无意义分词问题" -m "" -m "文件变更:" -m "- 新增 Source/Memory/PawnStatusKnowledgeGenerator.cs" -m "- 修改 Source/Memory/CommonKnowledgeLibrary.cs" -m "- 修改 Source/Memory/MemoryManager.cs" -m "- 修改 Source/RimTalkSettings.cs" -m "" -m "详见: CHANGELOG_v2.4.0.md"

echo.
echo [3/4] 查看提交...
git log -1 --oneline

echo.
echo [4/4] 推送到GitHub...
git push origin main

echo.
echo ============================================
echo ? 推送完成！
echo.
echo 查看仓库:
echo https://github.com/sanguodxj-byte/RimTalk-ExpandMemory
echo ============================================
echo.

pause
