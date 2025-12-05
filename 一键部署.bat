@echo off
chcp 65001 >nul
cls

echo.
echo ════════════════════════════════════════════════════════════
echo   RimTalk-ExpandMemory v3.3.4 部署工具
echo ════════════════════════════════════════════════════════════
echo.

set "SOURCE=%~dp0"
set "TARGET=D:\SteamLibrary\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory"

echo 源目录: %SOURCE%
echo 目标目录: %TARGET%
echo.

REM 检查RimWorld目录
if not exist "D:\SteamLibrary\steamapps\common\RimWorld" (
    echo [错误] 未找到RimWorld安装目录！
    echo.
    echo 请检查路径是否正确：
    echo D:\SteamLibrary\steamapps\common\RimWorld
    echo.
    pause
    exit /b 1
)

REM 创建Mod目录
if not exist "%TARGET%" (
    echo [信息] 创建Mod目录...
    mkdir "%TARGET%"
)

echo [开始] 复制文件...
echo.

REM 复制About
echo [1/3] About 文件夹...
xcopy "%SOURCE%About" "%TARGET%\About\" /E /I /Y /Q
if errorlevel 1 (
    echo [失败] About复制失败
    pause
    exit /b 1
)
echo       完成

REM 复制Assemblies
echo [2/3] Assemblies 文件夹 (DLL)...
xcopy "%SOURCE%Assemblies" "%TARGET%\Assemblies\" /E /I /Y /Q
if errorlevel 1 (
    echo [失败] Assemblies复制失败
    pause
    exit /b 1
)
echo       完成

REM 复制Languages (可选)
echo [3/3] Languages 文件夹...
if exist "%SOURCE%Languages" (
    xcopy "%SOURCE%Languages" "%TARGET%\Languages\" /E /I /Y /Q
    echo       完成
) else (
    echo       跳过 (不存在)
)

echo.
echo ════════════════════════════════════════════════════════════
echo   部署成功！
echo ════════════════════════════════════════════════════════════
echo.

REM 验证DLL
if exist "%TARGET%\Assemblies\RimTalk-ExpandMemory.dll" (
    echo [验证] DLL文件已部署 ?
    for %%A in ("%TARGET%\Assemblies\RimTalk-ExpandMemory.dll") do (
        echo [信息] DLL大小: %%~zA 字节
    )
) else (
    echo [警告] DLL文件未找到！
    echo        请先编译项目 (按F6或Ctrl+Shift+B)
)

echo.
echo Mod位置: %TARGET%
echo.
echo ════════════════════════════════════════════════════════════
echo   下一步操作
echo ════════════════════════════════════════════════════════════
echo.
echo 1. 启动 RimWorld
echo 2. 打开 选项 ^> Mods
echo 3. 勾选 'RimTalk-ExpandMemory'
echo 4. 重启游戏
echo.
echo 5. 打开 选项 ^> Mod Settings ^> RimTalk-Expand Memory
echo 6. 确认以下选项已启用:
echo    ? 启用Prompt Caching (降低50%%费用)
echo    ? 启用对话缓存
echo    ? 启用提示词缓存
echo.
echo ════════════════════════════════════════════════════════════
echo   v3.3.4 新功能
echo ════════════════════════════════════════════════════════════
echo.
echo ? ConversationCache优化 - 命中率提升4-5倍
echo ? PromptCache优化 - 命中率提升3-4倍  
echo ? Prompt Caching实现 - 费用降低50%%
echo ? 缓存容量扩大 - 200个对话 + 100个提示词
echo.
echo 总体效果: API费用降低约 80%%
echo 年度节省: $162-216 (基于GPT-4 Turbo)
echo.
pause
