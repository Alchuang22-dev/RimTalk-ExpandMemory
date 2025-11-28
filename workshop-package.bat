@echo off
chcp 65001 > nul

echo ================================
echo Steam创意工坊 发布准备
echo ================================
echo.

set "VERSION=3.2.0"
set "WORKSHOP_DIR=Workshop\RimTalk-ExpandMemory"

echo ?? 版本: v%VERSION%
echo ?? 输出: %WORKSHOP_DIR%
echo.

REM 询问是否包含SQLite
echo ? 选择发布版本:
echo.
echo   [1] 标准版（无SQLite，推荐首次发布）
echo   [2] 完整版（含SQLite向量数据库）
echo.
choice /c 12 /n /m "请选择 (1/2): "

if errorlevel 2 (
    set INCLUDE_SQLITE=1
    echo ? 将包含SQLite（完整版）
) else (
    set INCLUDE_SQLITE=0
    echo ? 不包含SQLite（标准版）
)

echo.
echo [1/4] 检查编译...
if not exist "1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ? 请先编译项目！
    pause
    exit /b 1
)
echo ? 编译完成

echo.
echo [2/4] 清理并创建目录...
if exist "Workshop" rmdir /s /q "Workshop"
mkdir "Workshop"
mkdir "%WORKSHOP_DIR%"
echo ? 目录已创建

echo.
echo [3/4] 复制Mod文件...

REM About
mkdir "%WORKSHOP_DIR%\About"
copy /y "About\About.xml" "%WORKSHOP_DIR%\About\" > nul
if exist "About\Preview.png" (
    copy /y "About\Preview.png" "%WORKSHOP_DIR%\About\" > nul
    echo ? About/ (含Preview.png)
) else (
    echo ??  About/ (缺少Preview.png，请手动添加)
)

REM Assemblies
mkdir "%WORKSHOP_DIR%\1.6\Assemblies"
copy /y "1.6\Assemblies\RimTalkMemoryPatch.dll" "%WORKSHOP_DIR%\1.6\Assemblies\" > nul
echo ? RimTalkMemoryPatch.dll

REM 其他资源
if exist "Defs" (
    robocopy "Defs" "%WORKSHOP_DIR%\Defs" /E /NFL /NDL /NJH /NJS > nul
    echo ? Defs/
)

if exist "Languages" (
    robocopy "Languages" "%WORKSHOP_DIR%\Languages" /E /NFL /NDL /NJH /NJS > nul
    echo ? Languages/
)

echo.
echo [4/4] 清理和验证...
del /f /q "%WORKSHOP_DIR%\*.md" 2>nul
del /f /q "%WORKSHOP_DIR%\1.6\Assemblies\*.pdb" 2>nul
echo ? 清理完成

echo.
echo ? 创意工坊包已准备完成！
pause
