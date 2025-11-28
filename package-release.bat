@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

echo ================================
echo RimTalk v3.2.0 发布打包工具
echo ================================
echo.

set "VERSION=3.2.0"
set "RELEASE_DIR=Release\RimTalk-ExpandMemory-v%VERSION%"
set "ZIP_NAME=RimTalk-ExpandMemory-v%VERSION%.zip"

echo ?? 版本: v%VERSION%
echo ?? 输出: %RELEASE_DIR%
echo.

REM ===== 检查编译 =====
echo [1/5] 检查编译...
if not exist "1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ? 请先编译项目！
    pause
    exit /b 1
)
echo ? 编译完成

REM ===== 清理旧版本 =====
echo.
echo [2/5] 清理旧版本...
if exist "Release" rmdir /s /q "Release"
mkdir "Release"
mkdir "%RELEASE_DIR%"
echo ? 目录已创建

REM ===== 复制Mod文件 =====
echo.
echo [3/5] 复制Mod文件...

REM About
mkdir "%RELEASE_DIR%\About"
copy /y "About\About.xml" "%RELEASE_DIR%\About\" > nul
if exist "About\Preview.png" copy /y "About\Preview.png" "%RELEASE_DIR%\About\" > nul
echo ? About/

REM Assemblies
mkdir "%RELEASE_DIR%\1.6\Assemblies"
copy /y "1.6\Assemblies\RimTalkMemoryPatch.dll" "%RELEASE_DIR%\1.6\Assemblies\" > nul
echo ? RimTalkMemoryPatch.dll

REM SQLite依赖（如果存在）
if exist "1.6\Assemblies\System.Data.SQLite.dll" (
    copy /y "1.6\Assemblies\System.Data.SQLite.dll" "%RELEASE_DIR%\1.6\Assemblies\" > nul
    echo ? System.Data.SQLite.dll
    
    mkdir "%RELEASE_DIR%\1.6\Assemblies\x86"
    mkdir "%RELEASE_DIR%\1.6\Assemblies\x64"
    
    if exist "1.6\Assemblies\x86\SQLite.Interop.dll" (
        copy /y "1.6\Assemblies\x86\SQLite.Interop.dll" "%RELEASE_DIR%\1.6\Assemblies\x86\" > nul
        echo ? SQLite.Interop.dll (x86)
    )
    
    if exist "1.6\Assemblies\x64\SQLite.Interop.dll" (
        copy /y "1.6\Assemblies\x64\SQLite.Interop.dll" "%RELEASE_DIR%\1.6\Assemblies\x64\" > nul
        echo ? SQLite.Interop.dll (x64)
    )
) else (
    echo ??  SQLite依赖不存在（向量数据库功能将不可用）
)

REM 其他资源
if exist "Defs" (
    robocopy "Defs" "%RELEASE_DIR%\Defs" /E /NFL /NDL /NJH /NJS > nul
    echo ? Defs/
)

if exist "Languages" (
    robocopy "Languages" "%RELEASE_DIR%\Languages" /E /NFL /NDL /NJH /NJS > nul
    echo ? Languages/
)

if exist "Textures" (
    robocopy "Textures" "%RELEASE_DIR%\Textures" /E /NFL /NDL /NJH /NJS > nul
    echo ? Textures/
)

REM ===== 复制文档 =====
echo.
echo [4/5] 复制文档...

REM 主文档
copy /y "README.md" "%RELEASE_DIR%\" > nul
echo ? README.md

REM 发布说明
if exist "RELEASE_NOTES_v3.0.0.md" copy /y "RELEASE_NOTES_v3.0.0.md" "%RELEASE_DIR%\" > nul
if exist "CHANGELOG.md" copy /y "CHANGELOG.md" "%RELEASE_DIR%\" > nul

REM 用户指南
mkdir "%RELEASE_DIR%\Docs"
if exist "SEMANTIC_EMBEDDING_GUIDE.md" copy /y "SEMANTIC_EMBEDDING_GUIDE.md" "%RELEASE_DIR%\Docs\" > nul
if exist "VECTOR_DATABASE_IMPLEMENTATION.md" copy /y "VECTOR_DATABASE_IMPLEMENTATION.md" "%RELEASE_DIR%\Docs\" > nul
if exist "VERIFICATION_GUIDE_v3.0.md" copy /y "VERIFICATION_GUIDE_v3.0.md" "%RELEASE_DIR%\Docs\" > nul
echo ? 文档已复制

REM ===== 打包ZIP =====
echo.
echo [5/5] 打包ZIP...

REM 检查PowerShell（用于压缩）
where powershell >nul 2>&1
if %errorlevel% neq 0 (
    echo ??  PowerShell不可用，跳过ZIP打包
    goto :skip_zip
)

REM 使用PowerShell压缩
powershell -Command "Compress-Archive -Path '%RELEASE_DIR%\*' -DestinationPath 'Release\%ZIP_NAME%' -Force"

if exist "Release\%ZIP_NAME%" (
    echo ? ZIP已创建: %ZIP_NAME%
    
    REM 显示文件大小
    for %%F in ("Release\%ZIP_NAME%") do (
        set "SIZE=%%~zF"
        set /a "SIZE_MB=!SIZE! / 1048576"
        echo ?? 大小: !SIZE_MB! MB
    )
) else (
    echo ? ZIP创建失败
)

:skip_zip

REM ===== 完成 =====
echo.
echo ================================
echo ? 发布打包完成！
echo ================================
echo.
echo ?? 输出位置:
echo   文件夹: %RELEASE_DIR%
if exist "Release\%ZIP_NAME%" (
    echo   ZIP包: Release\%ZIP_NAME%
)
echo.
echo ?? 包含内容:
echo   ├─ About/About.xml
echo   ├─ 1.6/Assemblies/
echo   │  ├─ RimTalkMemoryPatch.dll
if exist "%RELEASE_DIR%\1.6\Assemblies\System.Data.SQLite.dll" (
    echo   │  ├─ System.Data.SQLite.dll
    echo   │  ├─ x86/SQLite.Interop.dll
    echo   │  └─ x64/SQLite.Interop.dll
)
echo   │
echo   ├─ Defs/ (如果存在)
echo   ├─ Languages/ (如果存在)
echo   ├─ Textures/ (如果存在)
echo   ├─ README.md
echo   └─ Docs/ (用户指南)
echo.
echo ?? 下一步:
echo   1. 测试Mod（复制到RimWorld/Mods）
echo   2. 上传到GitHub Release
echo   3. 更新Steam Workshop
echo.
pause
