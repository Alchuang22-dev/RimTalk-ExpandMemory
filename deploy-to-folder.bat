@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

echo ================================
echo RimTalk v3.3.0 部署到文件夹
echo ================================
echo.

REM 配置
set "VERSION=3.3.0"
set "DEFAULT_TARGET=D:\RimWorld_Mods\RimTalk-ExpandMemory"

REM 询问目标路径
echo ?? 请输入目标文件夹路径
echo    （直接回车使用默认路径）
echo.
echo 默认路径: %DEFAULT_TARGET%
echo.
set /p "TARGET_DIR=目标路径: "

REM 如果用户没有输入，使用默认路径
if "%TARGET_DIR%"=="" (
    set "TARGET_DIR=%DEFAULT_TARGET%"
    echo ? 使用默认路径
) else (
    echo ? 使用自定义路径: %TARGET_DIR%
)

echo.

REM 检查编译
echo [1/5] 检查编译...
if not exist "1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ? 未找到编译输出！
    echo.
    echo 是否立即编译？
    choice /c YN /n /m "[Y] 是  [N] 否: "
    
    if errorlevel 2 (
        echo 已取消
        pause
        exit /b 1
    )
    
    echo.
    echo 正在编译...
    dotnet build -c Release
    
    if errorlevel 1 (
        echo ? 编译失败！
        pause
        exit /b 1
    )
    
    echo ? 编译完成
) else (
    echo ? 找到编译输出
)

echo.

REM 清理目标目录
echo [2/5] 准备目标目录...
if exist "%TARGET_DIR%" (
    echo ???  目标路径已存在，是否清理？
    choice /c YN /n /m "[Y] 清理旧文件  [N] 保留并覆盖: "
    
    if errorlevel 1 if not errorlevel 2 (
        echo 清理中...
        rmdir /s /q "%TARGET_DIR%"
    )
)

if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"
echo ? 目标目录已准备

echo.

REM 复制文件
echo [3/5] 复制Mod文件...

REM About
if not exist "%TARGET_DIR%\About" mkdir "%TARGET_DIR%\About"
copy /y "About\About.xml" "%TARGET_DIR%\About\" > nul
if exist "About\Preview.png" copy /y "About\Preview.png" "%TARGET_DIR%\About\" > nul
echo ? About/

REM Assemblies
if not exist "%TARGET_DIR%\1.6\Assemblies" mkdir "%TARGET_DIR%\1.6\Assemblies"
copy /y "1.6\Assemblies\RimTalkMemoryPatch.dll" "%TARGET_DIR%\1.6\Assemblies\" > nul
echo ? RimTalkMemoryPatch.dll

REM SQLite（如果存在）
set SQLITE_COPIED=0
if exist "1.6\Assemblies\System.Data.SQLite.dll" (
    copy /y "1.6\Assemblies\System.Data.SQLite.dll" "%TARGET_DIR%\1.6\Assemblies\" > nul
    echo ? System.Data.SQLite.dll
    set SQLITE_COPIED=1
    
    if not exist "%TARGET_DIR%\1.6\Assemblies\x86" mkdir "%TARGET_DIR%\1.6\Assemblies\x86"
    if not exist "%TARGET_DIR%\1.6\Assemblies\x64" mkdir "%TARGET_DIR%\1.6\Assemblies\x64"
    
    if exist "1.6\Assemblies\x86\SQLite.Interop.dll" (
        copy /y "1.6\Assemblies\x86\SQLite.Interop.dll" "%TARGET_DIR%\1.6\Assemblies\x86\" > nul
        echo ? SQLite.Interop.dll (x86)
    )
    
    if exist "1.6\Assemblies\x64\SQLite.Interop.dll" (
        copy /y "1.6\Assemblies\x64\SQLite.Interop.dll" "%TARGET_DIR%\1.6\Assemblies\x64\" > nul
        echo ? SQLite.Interop.dll (x64)
    )
)

REM 其他资源
if exist "Defs" (
    robocopy "Defs" "%TARGET_DIR%\Defs" /E /NFL /NDL /NJH /NJS > nul
    echo ? Defs/
)

if exist "Languages" (
    robocopy "Languages" "%TARGET_DIR%\Languages" /E /NFL /NDL /NJH /NJS > nul
    echo ? Languages/
)

if exist "Textures" (
    robocopy "Textures" "%TARGET_DIR%\Textures" /E /NFL /NDL /NJH /NJS > nul
    echo ? Textures/
)

echo.

REM 复制文档（可选）
echo [4/5] 复制文档...
echo.
echo 是否复制文档到Docs子目录？
choice /c YN /n /m "[Y] 是  [N] 否（跳过）: "

if errorlevel 1 if not errorlevel 2 (
    if not exist "%TARGET_DIR%\Docs" mkdir "%TARGET_DIR%\Docs"
    
    copy /y "README.md" "%TARGET_DIR%\" > nul
    
    if exist "SEMANTIC_EMBEDDING_GUIDE.md" copy /y "SEMANTIC_EMBEDDING_GUIDE.md" "%TARGET_DIR%\Docs\" > nul
    if exist "VECTOR_DATABASE_IMPLEMENTATION.md" copy /y "VECTOR_DATABASE_IMPLEMENTATION.md" "%TARGET_DIR%\Docs\" > nul
    if exist "RAG_RETRIEVAL_IMPLEMENTATION.md" copy /y "RAG_RETRIEVAL_IMPLEMENTATION.md" "%TARGET_DIR%\Docs\" > nul
    if exist "COMPATIBILITY_CHECK_v3.3.md" copy /y "COMPATIBILITY_CHECK_v3.3.md" "%TARGET_DIR%\Docs\" > nul
    
    echo ? 文档已复制
) else (
    echo ??  已跳过文档
)

echo.

REM 清理
echo [5/5] 清理无关文件...
del /f /q "%TARGET_DIR%\*.pdb" 2>nul
del /f /q "%TARGET_DIR%\*.xml" 2>nul
del /f /q "%TARGET_DIR%\*.bat" 2>nul
del /f /q "%TARGET_DIR%\1.6\Assemblies\*.pdb" 2>nul
echo ? 清理完成

echo.
echo ================================
echo ? 部署成功！
echo ================================
echo.
echo ?? 目标位置: %TARGET_DIR%
echo ?? 版本: v%VERSION%
echo.
echo ?? 部署内容:
echo   ├─ About/About.xml
echo   ├─ 1.6/Assemblies/
echo   │  └─ RimTalkMemoryPatch.dll
if %SQLITE_COPIED%==1 (
    echo   │  ├─ System.Data.SQLite.dll ?
    echo   │  ├─ x86/SQLite.Interop.dll
    echo   │  └─ x64/SQLite.Interop.dll
)
echo   ├─ Defs/ (如果存在)
echo   ├─ Languages/ (如果存在)
echo   └─ Textures/ (如果存在)
echo.
echo ?? 下一步:
if "%TARGET_DIR%"=="%DEFAULT_TARGET%" (
    echo   这是一个独立文件夹，不是RimWorld Mods目录
    echo   如需安装到游戏:
    echo   1. 复制整个文件夹到: RimWorld\Mods\
    echo   2. 或使用 quick-deploy.bat 直接部署到游戏
) else (
    echo   如果这是RimWorld\Mods目录，可以直接启动游戏测试
)
echo.
echo 是否打开目标文件夹？
choice /c YN /n /m "[Y] 是  [N] 否: "

if errorlevel 1 if not errorlevel 2 (
    explorer "%TARGET_DIR%"
)

echo.
pause
