@echo off
chcp 65001 > nul

REM ===== 配置区域（修改这里） =====
set "TARGET_DIR=D:\RimWorld_Mods\RimTalk-ExpandMemory"
set "VERSION=3.3.0"
REM ================================

echo ================================
echo 快速部署到: %TARGET_DIR%
echo ================================
echo.

REM 检查DLL
if not exist "1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ? 未编译，正在编译...
    dotnet build -c Release
    if errorlevel 1 (
        echo ? 编译失败
        pause
        exit /b 1
    )
)

REM 清理
if exist "%TARGET_DIR%" rmdir /s /q "%TARGET_DIR%"
mkdir "%TARGET_DIR%"

REM 复制
echo 复制文件...
xcopy "About" "%TARGET_DIR%\About\" /E /I /Y /Q > nul
xcopy "1.6" "%TARGET_DIR%\1.6\" /E /I /Y /Q > nul
if exist "Defs" xcopy "Defs" "%TARGET_DIR%\Defs\" /E /I /Y /Q > nul
if exist "Languages" xcopy "Languages" "%TARGET_DIR%\Languages\" /E /I /Y /Q > nul

REM 清理调试文件
del /f /q "%TARGET_DIR%\1.6\Assemblies\*.pdb" 2>nul

echo ? 完成！
echo ?? %TARGET_DIR%
pause
