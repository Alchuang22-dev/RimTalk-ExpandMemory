@echo off
chcp 65001 >nul
title RimTalk-ExpandMemory Mod 部署工具
color 0B

echo.
echo ========================================
echo  RimTalk-ExpandMemory Mod 部署工具
echo ========================================
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0deploy_to_rimworld.ps1"

if errorlevel 1 (
    echo.
    echo ? 部署失败！
    pause
    exit /b 1
)

pause
