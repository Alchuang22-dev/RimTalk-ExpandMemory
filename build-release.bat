@echo off
chcp 65001 > nul
echo ================================
echo RimTalk-ExpandMemory v3.0.0
echo 部署到RimWorld
echo ================================
echo.

REM 设置RimWorld目录
set "RIMWORLD_DIR=D:\steam\steamapps\common\RimWorld"
set "MOD_DIR=%RIMWORLD_DIR%\Mods\RimTalk-ExpandMemory"

echo ?? RimWorld目录: %RIMWORLD_DIR%
echo ?? Mod目录: %MOD_DIR%
echo.

echo [1/3] 检查编译输出...
if not exist "1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ? DLL不存在: 1.6\Assemblies\RimTalkMemoryPatch.dll
    echo 请先编译项目！
    pause
    exit /b 1
)
echo ? 找到 1.6\Assemblies\RimTalkMemoryPatch.dll

echo.
echo [2/3] 部署到RimWorld游戏目录...
if not exist "%RIMWORLD_DIR%\Mods" (
    echo ? RimWorld Mods目录不存在: %RIMWORLD_DIR%\Mods
    pause
    exit /b 1
)

REM 创建Mod目录
if not exist "%MOD_DIR%" mkdir "%MOD_DIR%"

REM 复制核心Mod文件（不包含文档和脚本）
echo 复制 About...
robocopy "About" "%MOD_DIR%\About" /MIR /NFL /NDL /NJH /NJS /XF *.md *.txt > nul

echo 复制 1.6/Assemblies (仅DLL)...
if not exist "%MOD_DIR%\1.6\Assemblies" mkdir "%MOD_DIR%\1.6\Assemblies"
copy /y "1.6\Assemblies\*.dll" "%MOD_DIR%\1.6\Assemblies\" > nul

echo 复制 Defs...
if exist "Defs" robocopy "Defs" "%MOD_DIR%\Defs" /MIR /NFL /NDL /NJH /NJS > nul

echo 复制 Languages...
if exist "Languages" robocopy "Languages" "%MOD_DIR%\Languages" /MIR /NFL /NDL /NJH /NJS > nul

echo 复制 Textures...
if exist "Textures" robocopy "Textures" "%MOD_DIR%\Textures" /MIR /NFL /NDL /NJH /NJS > nul

echo ? 部署到RimWorld完成

echo.
echo [3/3] 清理多余文件...
REM 删除可能误复制的文档和脚本
del /f /q "%MOD_DIR%\*.md" 2>nul
del /f /q "%MOD_DIR%\*.bat" 2>nul
del /f /q "%MOD_DIR%\*.ps1" 2>nul
del /f /q "%MOD_DIR%\*.txt" 2>nul
del /f /q "%MOD_DIR%\1.6\Assemblies\*.pdb" 2>nul

echo ? 清理完成

echo.
echo ================================
echo ? 部署完成！
echo ================================
echo.
echo ?? Mod位置: %MOD_DIR%
echo.
echo Mod目录包含：
echo ? About/About.xml
echo ? 1.6/Assemblies/RimTalkMemoryPatch.dll (仅DLL，无PDB)
echo ? Defs/ (如果存在)
echo ? Languages/
echo ? Textures/ (如果存在)
echo.
echo ? 不包含：
echo   - 文档 (*.md)
echo   - 脚本 (*.bat, *.ps1)
echo   - 调试文件 (*.pdb)
echo   - 临时文件
echo.
echo ?? 下一步：启动RimWorld测试
echo.
pause
