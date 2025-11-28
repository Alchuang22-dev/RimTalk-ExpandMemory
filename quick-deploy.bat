@echo off
chcp 65001 > nul

echo ================================
echo RimTalk v3.2.0 快速部署
echo ================================
echo.

REM 配置（修改这里的路径）
set "RIMWORLD=D:\steam\steamapps\common\RimWorld"
set "MOD=%RIMWORLD%\Mods\RimTalk-ExpandMemory"

echo 目标: %MOD%
echo.

REM 检查DLL
if not exist "1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ? 请先编译项目！
    pause
    exit /b 1
)

REM 清理旧版本
if exist "%MOD%" rmdir /s /q "%MOD%"
mkdir "%MOD%"

REM 复制文件
echo 正在部署...
xcopy "About" "%MOD%\About\" /E /I /Y /Q > nul
xcopy "1.6" "%MOD%\1.6\" /E /I /Y /Q > nul
if exist "Defs" xcopy "Defs" "%MOD%\Defs\" /E /I /Y /Q > nul
if exist "Languages" xcopy "Languages" "%MOD%\Languages\" /E /I /Y /Q > nul

REM 清理
del /f /q "%MOD%\1.6\Assemblies\*.pdb" 2>nul
del /f /q "%MOD%\*.md" 2>nul

echo ? 完成！
echo.
echo ?? %MOD%
pause
