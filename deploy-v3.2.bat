@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

echo ================================
echo RimTalk-ExpandMemory v3.2.0
echo 完整部署脚本（包含向量数据库）
echo ================================
echo.

REM ===== 配置 =====
set "RIMWORLD_DIR=D:\steam\steamapps\common\RimWorld"
set "MOD_DIR=%RIMWORLD_DIR%\Mods\RimTalk-ExpandMemory"
set "VERSION=3.2.0"

echo ?? RimWorld目录: %RIMWORLD_DIR%
echo ?? Mod目录: %MOD_DIR%
echo ?? 版本: v%VERSION%
echo.

REM ===== 步骤1: 检查编译输出 =====
echo [1/6] 检查编译输出...
if not exist "1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ? 主DLL不存在！请先编译项目。
    pause
    exit /b 1
)
echo ? 找到主DLL: RimTalkMemoryPatch.dll

REM 检查SQLite依赖
set SQLITE_MISSING=0
if not exist "1.6\Assemblies\System.Data.SQLite.dll" (
    echo ??  SQLite.dll不存在（向量数据库需要）
    set SQLITE_MISSING=1
)
if not exist "1.6\Assemblies\x86\SQLite.Interop.dll" (
    echo ??  SQLite.Interop.dll (x86) 不存在
    set SQLITE_MISSING=1
)
if not exist "1.6\Assemblies\x64\SQLite.Interop.dll" (
    echo ??  SQLite.Interop.dll (x64) 不存在
    set SQLITE_MISSING=1
)

if !SQLITE_MISSING!==1 (
    echo.
    echo ? 是否启用向量数据库功能？（需要SQLite）
    echo    [Y] 是 - 将尝试从NuGet包复制SQLite文件
    echo    [N] 否 - 跳过SQLite（禁用向量数据库功能）
    choice /c YN /n /m "请选择 (Y/N): "
    
    if errorlevel 2 (
        echo ??  跳过SQLite依赖，向量数据库功能将不可用
        set INCLUDE_SQLITE=0
    ) else (
        echo ?? 将复制SQLite依赖...
        set INCLUDE_SQLITE=1
    )
) else (
    echo ? 找到所有SQLite依赖
    set INCLUDE_SQLITE=1
)

echo.

REM ===== 步骤2: 创建Mod目录 =====
echo [2/6] 准备Mod目录...
if not exist "%RIMWORLD_DIR%\Mods" (
    echo ? RimWorld Mods目录不存在: %RIMWORLD_DIR%\Mods
    pause
    exit /b 1
)

if exist "%MOD_DIR%" (
    echo ???  清理旧版本...
    rmdir /s /q "%MOD_DIR%" 2>nul
)

mkdir "%MOD_DIR%"
echo ? Mod目录已创建

echo.

REM ===== 步骤3: 复制About信息 =====
echo [3/6] 复制About信息...
if not exist "%MOD_DIR%\About" mkdir "%MOD_DIR%\About"
copy /y "About\About.xml" "%MOD_DIR%\About\" > nul
if exist "About\Preview.png" copy /y "About\Preview.png" "%MOD_DIR%\About\" > nul
if exist "About\PublishedFileId.txt" copy /y "About\PublishedFileId.txt" "%MOD_DIR%\About\" > nul
echo ? About信息已复制

echo.

REM ===== 步骤4: 复制DLL文件 =====
echo [4/6] 复制DLL文件...
if not exist "%MOD_DIR%\1.6\Assemblies" mkdir "%MOD_DIR%\1.6\Assemblies"

REM 主DLL
copy /y "1.6\Assemblies\RimTalkMemoryPatch.dll" "%MOD_DIR%\1.6\Assemblies\" > nul
echo ? RimTalkMemoryPatch.dll

REM SQLite依赖（如果启用）
if !INCLUDE_SQLITE!==1 (
    REM 尝试从编译输出复制
    if exist "1.6\Assemblies\System.Data.SQLite.dll" (
        copy /y "1.6\Assemblies\System.Data.SQLite.dll" "%MOD_DIR%\1.6\Assemblies\" > nul
        echo ? System.Data.SQLite.dll
    ) else (
        REM 尝试从NuGet包复制
        for /r "%USERPROFILE%\.nuget\packages" %%f in (System.Data.SQLite.dll) do (
            if exist "%%f" (
                copy /y "%%f" "%MOD_DIR%\1.6\Assemblies\" > nul
                echo ? System.Data.SQLite.dll (from NuGet)
                goto :sqlite_dll_found
            )
        )
        echo ? System.Data.SQLite.dll 未找到
        :sqlite_dll_found
    )
    
    REM SQLite.Interop.dll (x86)
    if not exist "%MOD_DIR%\1.6\Assemblies\x86" mkdir "%MOD_DIR%\1.6\Assemblies\x86"
    if exist "1.6\Assemblies\x86\SQLite.Interop.dll" (
        copy /y "1.6\Assemblies\x86\SQLite.Interop.dll" "%MOD_DIR%\1.6\Assemblies\x86\" > nul
        echo ? SQLite.Interop.dll (x86)
    ) else (
        REM 从NuGet复制
        for /r "%USERPROFILE%\.nuget\packages\system.data.sqlite.core" %%f in (x86\SQLite.Interop.dll) do (
            if exist "%%f" (
                copy /y "%%f" "%MOD_DIR%\1.6\Assemblies\x86\" > nul
                echo ? SQLite.Interop.dll (x86, from NuGet)
                goto :sqlite_x86_found
            )
        )
        echo ??  SQLite.Interop.dll (x86) 未找到
        :sqlite_x86_found
    )
    
    REM SQLite.Interop.dll (x64)
    if not exist "%MOD_DIR%\1.6\Assemblies\x64" mkdir "%MOD_DIR%\1.6\Assemblies\x64"
    if exist "1.6\Assemblies\x64\SQLite.Interop.dll" (
        copy /y "1.6\Assemblies\x64\SQLite.Interop.dll" "%MOD_DIR%\1.6\Assemblies\x64\" > nul
        echo ? SQLite.Interop.dll (x64)
    ) else (
        REM 从NuGet复制
        for /r "%USERPROFILE%\.nuget\packages\system.data.sqlite.core" %%f in (x64\SQLite.Interop.dll) do (
            if exist "%%f" (
                copy /y "%%f" "%MOD_DIR%\1.6\Assemblies\x64\" > nul
                echo ? SQLite.Interop.dll (x64, from NuGet)
                goto :sqlite_x64_found
            )
        )
        echo ??  SQLite.Interop.dll (x64) 未找到
        :sqlite_x64_found
    )
)

echo.

REM ===== 步骤5: 复制其他资源 =====
echo [5/6] 复制其他资源...

REM Defs
if exist "Defs" (
    if not exist "%MOD_DIR%\Defs" mkdir "%MOD_DIR%\Defs"
    robocopy "Defs" "%MOD_DIR%\Defs" /MIR /NFL /NDL /NJH /NJS > nul
    echo ? Defs/
) else (
    echo ??  Defs/ (不存在)
)

REM Languages
if exist "Languages" (
    if not exist "%MOD_DIR%\Languages" mkdir "%MOD_DIR%\Languages"
    robocopy "Languages" "%MOD_DIR%\Languages" /MIR /NFL /NDL /NJH /NJS > nul
    echo ? Languages/
) else (
    echo ??  Languages/ (不存在)
)

REM Textures
if exist "Textures" (
    if not exist "%MOD_DIR%\Textures" mkdir "%MOD_DIR%\Textures"
    robocopy "Textures" "%MOD_DIR%\Textures" /MIR /NFL /NDL /NJH /NJS > nul
    echo ? Textures/
) else (
    echo ??  Textures/ (不存在)
)

echo.

REM ===== 步骤6: 清理和验证 =====
echo [6/6] 清理和验证...

REM 删除调试文件
del /f /q "%MOD_DIR%\1.6\Assemblies\*.pdb" 2>nul
del /f /q "%MOD_DIR%\1.6\Assemblies\*.xml" 2>nul

REM 删除文档和脚本
del /f /q "%MOD_DIR%\*.md" 2>nul
del /f /q "%MOD_DIR%\*.bat" 2>nul
del /f /q "%MOD_DIR%\*.ps1" 2>nul
del /f /q "%MOD_DIR%\*.txt" 2>nul

echo ? 清理完成

echo.
echo ================================
echo ? 部署成功！
echo ================================
echo.
echo ?? Mod位置: %MOD_DIR%
echo ?? 版本: v%VERSION%
echo.
echo ?? 部署内容：
echo   ├─ About/
echo   │  ├─ About.xml
echo   │  └─ Preview.png (如果存在)
echo   │
echo   ├─ 1.6/Assemblies/
echo   │  ├─ RimTalkMemoryPatch.dll ?
if !INCLUDE_SQLITE!==1 (
    echo   │  ├─ System.Data.SQLite.dll ? (向量数据库)
    echo   │  ├─ x86/SQLite.Interop.dll ?
    echo   │  └─ x64/SQLite.Interop.dll ?
) else (
    echo   │  └─ (无SQLite - 向量数据库功能禁用)
)
echo   │
echo   ├─ Defs/ (如果存在)
echo   ├─ Languages/ (如果存在)
echo   └─ Textures/ (如果存在)
echo.

REM 检查必需文件
set ERROR_COUNT=0
if not exist "%MOD_DIR%\1.6\Assemblies\RimTalkMemoryPatch.dll" (
    echo ? 缺少: RimTalkMemoryPatch.dll
    set /a ERROR_COUNT+=1
)
if not exist "%MOD_DIR%\About\About.xml" (
    echo ? 缺少: About.xml
    set /a ERROR_COUNT+=1
)

if !ERROR_COUNT! GTR 0 (
    echo.
    echo ??  发现 !ERROR_COUNT! 个错误，请检查！
) else (
    echo ? 所有必需文件已部署
)

echo.
echo ?? 下一步：
echo   1. 启动RimWorld
echo   2. 在Mod列表中启用 "RimTalk-ExpandMemory"
if !INCLUDE_SQLITE!==1 (
    echo   3. 进入游戏后，在Mod设置中启用向量数据库功能
) else (
    echo   3. (向量数据库功能已禁用)
)
echo.
pause
