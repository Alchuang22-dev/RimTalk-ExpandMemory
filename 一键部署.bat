@echo off
chcp 65001 >nul
cls

echo.
echo ════════════════════════════════════════════════════════════
echo   RimTalk-ExpandMemory v3.3.4 部署工具
echo ════════════════════════════════════════════════════════════
echo.

set "SOURCE=%~dp0"
set "TARGET=D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory"

echo 源目录: %SOURCE%
echo 目标目录: %TARGET%
echo.

REM 检查RimWorld目录
if not exist "D:\steam\steamapps\common\RimWorld" (
    echo [错误] 未找到RimWorld安装目录！
    echo.
    echo 请检查路径是否正确：
    echo D:\steam\steamapps\common\RimWorld
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
echo [1/4] About 文件夹...
xcopy "%SOURCE%About" "%TARGET%\About\" /E /I /Y /Q
if errorlevel 1 (
    echo [失败] About复制失败
    pause
    exit /b 1
)
echo       完成

REM 复制Assemblies（从1.6/Assemblies，这是正确的编译输出目录）
echo [2/4] Assemblies 文件夹 (DLL)...
xcopy "%SOURCE%1.6\Assemblies" "%TARGET%\Assemblies\" /E /I /Y /Q
if errorlevel 1 (
    echo [失败] Assemblies复制失败
    pause
    exit /b 1
)
echo       完成

REM 复制Languages (可选)
echo [3/4] Languages 文件夹...
if exist "%SOURCE%Languages" (
    xcopy "%SOURCE%Languages" "%TARGET%\Languages\" /E /I /Y /Q
    echo       完成
) else (
    echo       跳过 (不存在)
)

REM 复制Defs (可选)
echo [4/4] Defs 文件夹...
if exist "%SOURCE%Defs" (
    xcopy "%SOURCE%Defs" "%TARGET%\Defs\" /E /I /Y /Q
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
if exist "%TARGET%\Assemblies\RimTalkMemoryPatch.dll" (
    echo [验证] DLL文件已部署
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
pause
