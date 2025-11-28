@echo off
chcp 65001 >nul
echo ========================================
echo   清理旧版本文档和脚本
echo   Cleanup Old Version Files
echo ========================================
echo.

REM 设置颜色
color 0E

echo [警告] 此操作将删除以下文件：
echo.
echo 旧版本文档：
echo   - RELEASE_NOTES_v3.0.0.md
echo   - DEPLOYMENT_GUIDE_v3.0.md
echo   - DEPLOYMENT_SUCCESS_v3.0.md
echo   - DEPLOYMENT_COMPLETE_v3.0.md
echo   - RELEASE_COMPLETE_v3.0.md
echo   - VERIFICATION_GUIDE_v3.0.md
echo   - COMPATIBILITY_CHECK_v3.3.md
echo   - URGENT_FIXES_COMPLETE.md
echo   - FINAL_OPTIMIZATION_COMPLETE.md
echo   - DEPLOY_v3.3.2_FINAL.md
echo   - DEPLOYMENT_SUCCESS_v3.3.2.md
echo   - OPTIMIZATION_REVIEW_v3.3.2.md
echo   - FINAL_REVIEW_v3.3.2.md
echo.
echo 旧版本脚本：
echo   - deploy-v3.2.bat
echo   - deploy-folder-quick.bat
echo   - deploy-now.bat
echo   - deploy-to-folder.bat
echo   - quick-deploy.bat
echo   - workshop-package.bat
echo   - package-release.bat
echo   - cleanup.bat
echo.
echo 保留文件（v3.3.2核心）：
echo   ? CHANGELOG_v3.3.2.md
echo   ? DEPLOYMENT_SUMMARY_v3.3.2.md
echo   ? DEPLOYMENT_CHECKLIST_v3.3.2.md
echo   ? DEPLOYMENT_COMPLETE_v3.3.2.md
echo   ? STEAM_WORKSHOP_RELEASE_v3.3.2.md
echo   ? v3.3.2_SUMMARY.md
echo   ? VERSION_COMPARISON_v3.3.2.md
echo   ? deploy-v3.3.2.bat
echo   ? build-release.bat
echo.

set /p confirm="确认删除？(Y/N): "
if /i not "%confirm%"=="Y" (
    echo 取消操作
    pause
    exit /b 0
)

echo.
echo [1/3] 删除旧版本文档...

REM v3.0文档
if exist "RELEASE_NOTES_v3.0.0.md" del "RELEASE_NOTES_v3.0.0.md"
if exist "DEPLOYMENT_GUIDE_v3.0.md" del "DEPLOYMENT_GUIDE_v3.0.md"
if exist "DEPLOYMENT_SUCCESS_v3.0.md" del "DEPLOYMENT_SUCCESS_v3.0.md"
if exist "DEPLOYMENT_COMPLETE_v3.0.md" del "DEPLOYMENT_COMPLETE_v3.0.md"
if exist "RELEASE_COMPLETE_v3.0.md" del "RELEASE_COMPLETE_v3.0.md"
if exist "VERIFICATION_GUIDE_v3.0.md" del "VERIFICATION_GUIDE_v3.0.md"

REM v3.3临时文档
if exist "COMPATIBILITY_CHECK_v3.3.md" del "COMPATIBILITY_CHECK_v3.3.md"
if exist "URGENT_FIXES_COMPLETE.md" del "URGENT_FIXES_COMPLETE.md"
if exist "FINAL_OPTIMIZATION_COMPLETE.md" del "FINAL_OPTIMIZATION_COMPLETE.md"

REM v3.3.2临时文档
if exist "DEPLOY_v3.3.2_FINAL.md" del "DEPLOY_v3.3.2_FINAL.md"
if exist "DEPLOYMENT_SUCCESS_v3.3.2.md" del "DEPLOYMENT_SUCCESS_v3.3.2.md"
if exist "OPTIMIZATION_REVIEW_v3.3.2.md" del "OPTIMIZATION_REVIEW_v3.3.2.md"
if exist "FINAL_REVIEW_v3.3.2.md" del "FINAL_REVIEW_v3.3.2.md"

echo ? 文档删除完成

echo.
echo [2/3] 删除旧版本脚本...

if exist "deploy-v3.2.bat" del "deploy-v3.2.bat"
if exist "deploy-folder-quick.bat" del "deploy-folder-quick.bat"
if exist "deploy-now.bat" del "deploy-now.bat"
if exist "deploy-to-folder.bat" del "deploy-to-folder.bat"
if exist "quick-deploy.bat" del "quick-deploy.bat"
if exist "workshop-package.bat" del "workshop-package.bat"
if exist "package-release.bat" del "package-release.bat"
if exist "cleanup.bat" del "cleanup.bat"

echo ? 脚本删除完成

echo.
echo [3/3] 删除旧指南文档...

if exist "DEPLOY_NOW_GUIDE.md" del "DEPLOY_NOW_GUIDE.md"
if exist "DEPLOY_NOW.md" del "DEPLOY_NOW.md"
if exist "DEPLOYMENT_CHECKLIST.md" del "DEPLOYMENT_CHECKLIST.md"
if exist "DEPLOYMENT_SCRIPTS_GUIDE.md" del "DEPLOYMENT_SCRIPTS_GUIDE.md"
if exist "GIT_COMMIT_GUIDE.md" del "GIT_COMMIT_GUIDE.md"

echo ? 指南删除完成

echo.
echo ========================================
echo   清理完成！
echo ========================================
echo.
echo 保留的核心文件：
echo.
echo ?? 文档（7个）：
echo   ? CHANGELOG_v3.3.2.md
echo   ? DEPLOYMENT_SUMMARY_v3.3.2.md
echo   ? DEPLOYMENT_CHECKLIST_v3.3.2.md
echo   ? DEPLOYMENT_COMPLETE_v3.3.2.md
echo   ? STEAM_WORKSHOP_RELEASE_v3.3.2.md
echo   ? v3.3.2_SUMMARY.md
echo   ? VERSION_COMPARISON_v3.3.2.md
echo.
echo ?? 脚本（2个）：
echo   ? deploy-v3.3.2.bat
echo   ? build-release.bat
echo.
echo ?? 技术文档（保留）：
echo   ? ADVANCED_SCORING_DESIGN.md
echo   ? SEMANTIC_EMBEDDING_IMPLEMENTATION.md
echo   ? VECTOR_DATABASE_IMPLEMENTATION.md
echo   ? RAG_RETRIEVAL_IMPLEMENTATION.md
echo   ? AI_DATABASE_MOUNTING_GUIDE.md
echo   ? 其他技术文档...
echo.
echo ========================================

pause
