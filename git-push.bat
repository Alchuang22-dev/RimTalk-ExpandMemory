@echo off
chcp 65001 >nul
setlocal

:: =================================================
:: Git Auto Commit & Push Script
:: =================================================

echo.
echo [GIT] Starting Git operations...
echo.

:: 1. Check git status
echo [INFO] Checking Git status...
git status

echo.
echo [INFO] Adding all changes...
git add -A

:: 2. Check if there are changes to commit
git diff-index --quiet HEAD --
if %ERRORLEVEL% EQU 0 (
    echo [INFO] No changes to commit
    goto :end
)

:: 3. Prompt for commit message
set /p COMMIT_MSG="Enter commit message (or press Enter for default): "

if "%COMMIT_MSG%"=="" (
    set "COMMIT_MSG=refactor: Clean up code and update configurations"
)

echo.
echo [INFO] Committing changes...
echo [INFO] Commit message: %COMMIT_MSG%
git commit -m "%COMMIT_MSG%"

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Commit failed!
    goto :end
)

echo.
echo [SUCCESS] Commit successful!

:: 4. Push to remote
echo.
echo [INFO] Pushing to remote repository...
git push origin main

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Push failed! Trying alternative branch name...
    git push origin master
    
    if %ERRORLEVEL% NEQ 0 (
        echo [ERROR] Push failed on both main and master branches!
        echo Please check your network connection and remote repository settings.
        goto :end
    )
)

echo.
echo [SUCCESS] ===================================
echo [SUCCESS] Push completed successfully!
echo [SUCCESS] ===================================

:end
echo.
pause
