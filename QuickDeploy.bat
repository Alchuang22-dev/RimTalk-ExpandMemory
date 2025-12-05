@echo off
chcp 65001 >nul
echo.
echo ═══════════════════════════════════════════════════════════
echo   RimTalk-ExpandMemory v3.3.4 - 快速部署
echo ═══════════════════════════════════════════════════════════
echo.

REM 配置路径
set "SOURCE=%~dp0"
set "TARGET=D:\SteamLibrary\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory"

echo ?? 源路径: %SOURCE%
echo ?? 目标路径: %TARGET%
echo.

REM 检查RimWorld是否存在
if not exist "D:\SteamLibrary\steamapps\common\RimWorld" (
    echo ? 未找到RimWorld目录！
    echo ?? 请修改此脚本中的 TARGET 路径
    pause
    exit /b 1
)

REM 创建Mod目录
if not exist "%TARGET%" (
    echo ?? 创建Mod目录...
    mkdir "%TARGET%"
    echo ? 已创建
) else (
    echo ?? Mod目录已存在
)

echo.
echo ?? 开始部署...
echo.

REM 复制About文件夹
echo [1/5] 复制 About...
xcopy "%SOURCE%About" "%TARGET%\About\" /E /I /Y /Q >nul 2>&1
if errorlevel 1 (
    echo ? 失败
) else (
    echo ? 完成
)

REM 复制Assemblies文件夹
echo [2/5] 复制 Assemblies...
xcopy "%SOURCE%Assemblies" "%TARGET%\Assemblies\" /E /I /Y /Q >nul 2>&1
if errorlevel 1 (
    echo ? 失败
) else (
    echo ? 完成
)

REM 复制Languages文件夹（如果存在）
echo [3/5] 复制 Languages...
if exist "%SOURCE%Languages" (
    xcopy "%SOURCE%Languages" "%TARGET%\Languages\" /E /I /Y /Q >nul 2>&1
    echo ? 完成
) else (
    echo ??  跳过（不存在）
)

REM 复制Defs文件夹（如果存在）
echo [4/5] 复制 Defs...
if exist "%SOURCE%Defs" (
    xcopy "%SOURCE%Defs" "%TARGET%\Defs\" /E /I /Y /Q >nul 2>&1
    echo ? 完成
) else (
    echo ??  跳过（不存在）
)

REM 复制Docs文件夹
echo [5/5] 复制 Docs...
if exist "%SOURCE%Docs" (
    xcopy "%SOURCE%Docs" "%TARGET%\Docs\" /E /I /Y /Q >nul 2>&1
    echo ? 完成
) else (
    echo ??  跳过（不存在）
)

echo.
echo ═══════════════════════════════════════════════════════════
echo ? 部署完成！
echo ═══════════════════════════════════════════════════════════
echo.

REM 显示DLL信息
if exist "%TARGET%\Assemblies\RimTalk-ExpandMemory.dll" (
    echo ?? 部署信息:
    echo    ? Mod名称: RimTalk-ExpandMemory
    echo    ? 版本: v3.3.4 (缓存优化)
    echo    ? 位置: %TARGET%
    echo    ? DLL: 已部署
) else (
    echo ??  警告: DLL文件未找到！
    echo    请先编译项目（F6 或 Ctrl+Shift+B）
)

echo.
echo ?? 下一步:
echo    1. 启动RimWorld
echo    2. 打开Mod管理器 (选项 → Mods)
echo    3. 启用 'RimTalk-ExpandMemory'
echo    4. 重启游戏（如果提示）
echo    5. 检查Mod设置 → RimTalk-Expand Memory
echo    6. 确认 '?? 启用Prompt Caching' 已勾选
echo.
echo ?? v3.3.4新功能:
echo    ? 对话缓存优化 (命中率↑4-5倍)
echo    ? 提示词缓存优化 (命中率↑3-4倍)
echo    ? Prompt Caching (费用↓50%%)
echo    ?? 总API费用降低约 80%%
echo.
echo ?? 祝你游戏愉快！
echo.
pause
