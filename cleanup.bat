@echo off
chcp 65001 > nul
echo ================================
echo 清理多余文档和脚本
echo ================================
echo.

set "RIMWORLD_MOD_DIR=D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory"

echo [1/2] 清理RimWorld Mod目录中的多余文件...
if exist "%RIMWORLD_MOD_DIR%" (
    echo 删除文档文件...
    del /f /q "%RIMWORLD_MOD_DIR%\*.md" 2>nul
    del /f /q "%RIMWORLD_MOD_DIR%\*.bat" 2>nul
    del /f /q "%RIMWORLD_MOD_DIR%\*.ps1" 2>nul
    del /f /q "%RIMWORLD_MOD_DIR%\*.txt" 2>nul
    
    echo 删除PDB调试文件...
    del /f /q "%RIMWORLD_MOD_DIR%\1.6\Assemblies\*.pdb" 2>nul
    
    echo ? RimWorld Mod目录清理完成
) else (
    echo ?? RimWorld Mod目录不存在
)

echo.
echo [2/2] 清理项目目录中的临时文件...
if exist "Release-Package" (
    echo 删除发布包目录...
    rmdir /s /q "Release-Package"
)

if exist "RimTalk-ExpandMemory-v3.0.0.zip" (
    echo 删除旧的压缩包...
    del /f /q "RimTalk-ExpandMemory-v3.0.0.zip"
)

echo 删除PDB文件...
del /f /q "1.6\Assemblies\*.pdb" 2>nul

echo ? 项目临时文件清理完成

echo.
echo ================================
echo ? 清理完成！
echo ================================
echo.
echo 保留的文档（仅在GitHub仓库）：
echo - README.md
echo - ADVANCED_SCORING_DESIGN.md
echo - TOKEN_CONSUMPTION_ANALYSIS.md
echo - RIMTALK_INTEGRATION_SAFETY.md
echo - 等技术文档
echo.
echo RimWorld Mod目录只包含：
echo - About/
echo - 1.6/Assemblies/ (仅DLL)
echo - Defs/
echo - Languages/
echo - Textures/
echo.
pause
