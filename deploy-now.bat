@echo off
chcp 65001 > nul

echo ================================
echo RimTalk v3.3.0 快速部署
echo 目标: RimWorld Mods 目录
echo ================================
echo.

REM 你的RimWorld Mods路径
set "RIMWORLD_MODS=D:\steam\steamapps\common\RimWorld\Mods"
set "MOD_DIR=%RIMWORLD_MODS%\RimTalk-ExpandMemory"

echo ?? 目标路径: %MOD_DIR%
echo.

REM 检查RimWorld Mods目录
if not exist "%RIMWORLD_MODS%" (
    echo ? RimWorld Mods目录不存在！
    echo 路径: %RIMWORLD_MODS%
    pause
    exit /b 1
)

REM 检查编译
if not exist "1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ??  未找到编译输出，正在编译...
    dotnet build -c Release
    
    if errorlevel 1 (
        echo ? 编译失败！
        pause
        exit /b 1
    )
    
    echo ? 编译完成
)

echo.
echo ?? 开始部署...
echo.

REM 清理旧版本
if exist "%MOD_DIR%" (
    echo ???  清理旧版本...
    rmdir /s /q "%MOD_DIR%"
)

mkdir "%MOD_DIR%"

REM 复制About
echo ?? 复制About...
xcopy "About" "%MOD_DIR%\About\" /E /I /Y /Q > nul

REM 复制Assemblies
echo ?? 复制Assemblies...
xcopy "1.6\Assemblies" "%MOD_DIR%\1.6\Assemblies\" /E /I /Y /Q > nul

REM 复制可选资源
if exist "Defs" (
    echo ?? 复制Defs...
    xcopy "Defs" "%MOD_DIR%\Defs\" /E /I /Y /Q > nul
)

if exist "Languages" (
    echo ?? 复制Languages...
    xcopy "Languages" "%MOD_DIR%\Languages\" /E /I /Y /Q > nul
)

if exist "Textures" (
    echo ?? 复制Textures...
    xcopy "Textures" "%MOD_DIR%\Textures\" /E /I /Y /Q > nul
)

REM 清理调试文件
echo ?? 清理调试文件...
del /f /q "%MOD_DIR%\1.6\Assemblies\*.pdb" 2>nul
del /f /q "%MOD_DIR%\1.6\Assemblies\*.xml" 2>nul

echo.
echo ================================
echo ? 部署完成！
echo ================================
echo.
echo ?? Mod位置: %MOD_DIR%
echo.
echo ?? 部署文件:
dir /b "%MOD_DIR%" 2>nul
echo.

REM 检查DLL
if exist "%MOD_DIR%\1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ? RimTalkMemoryPatch.dll - OK
) else (
    echo ? 主DLL缺失！
)

REM 检查SQLite
if exist "%MOD_DIR%\1.6\Assemblies\System.Data.SQLite.dll" (
    echo ? System.Data.SQLite.dll - OK （向量数据库可用）
    if exist "%MOD_DIR%\1.6\Assemblies\x86\SQLite.Interop.dll" (
        echo ? SQLite.Interop.dll (x86) - OK
    )
    if exist "%MOD_DIR%\1.6\Assemblies\x64\SQLite.Interop.dll" (
        echo ? SQLite.Interop.dll (x64) - OK
    )
) else (
    echo ??  SQLite未部署（向量数据库功能将不可用）
)

echo.
echo ?? 下一步:
echo   1. 启动RimWorld
echo   2. 在Mod管理器中启用 "RimTalk-ExpandMemory"
echo   3. 确保加载顺序: RimTalk → RimTalk-ExpandMemory
echo   4. 重启游戏或新建存档测试
echo.
pause
