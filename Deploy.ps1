# RimTalk-ExpandMemory v3.3.4 自动部署脚本
# 用途：将编译好的Mod文件部署到RimWorld Mods目录

param(
    [string]$RimWorldPath = "D:\SteamLibrary\steamapps\common\RimWorld",
    [switch]$Force
)

# 颜色输出函数
function Write-Success { param($msg) Write-Host $msg -ForegroundColor Green }
function Write-Info { param($msg) Write-Host $msg -ForegroundColor Cyan }
function Write-Warn { param($msg) Write-Host $msg -ForegroundColor Yellow }
function Write-Err { param($msg) Write-Host $msg -ForegroundColor Red }

# 标题
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  RimTalk-ExpandMemory v3.3.4 - Deployment Script" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# 路径配置
$sourcePath = $PSScriptRoot
$modName = "RimTalk-ExpandMemory"
$modPath = Join-Path $RimWorldPath "Mods\$modName"

Write-Info "?? Source: $sourcePath"
Write-Info "?? Target: $modPath"
Write-Host ""

# 检查RimWorld路径
if (-not (Test-Path $RimWorldPath)) {
    Write-Err "? RimWorld not found at: $RimWorldPath"
    Write-Warn "?? Please specify correct path:"
    Write-Host "   .\Deploy.ps1 -RimWorldPath 'C:\Program Files\RimWorld'"
    exit 1
}

# 检查源文件
$requiredFolders = @("About", "Assemblies")
foreach ($folder in $requiredFolders) {
    if (-not (Test-Path (Join-Path $sourcePath $folder))) {
        Write-Err "? Missing required folder: $folder"
        exit 1
    }
}

# 创建Mod目录
if (-not (Test-Path $modPath)) {
    Write-Info "?? Creating mod directory..."
    New-Item -ItemType Directory -Path $modPath -Force | Out-Null
    Write-Success "? Created: $modPath"
} else {
    Write-Info "?? Mod directory exists"
    if ($Force) {
        Write-Warn "??  Force mode: Will overwrite existing files"
    }
}

Write-Host ""
Write-Info "?? Starting deployment..."
Write-Host ""

# 部署进度
$deployedCount = 0
$totalSteps = 5

# 1. About文件夹
Write-Host "[$($deployedCount+1)/$totalSteps] Copying About..." -NoNewline
try {
    Copy-Item -Path (Join-Path $sourcePath "About") -Destination $modPath -Recurse -Force
    Write-Success " ?"
    $deployedCount++
} catch {
    Write-Err " ?"
    Write-Err "Error: $_"
    exit 1
}

# 2. Assemblies文件夹（编译后的DLL）
Write-Host "[$($deployedCount+1)/$totalSteps] Copying Assemblies..." -NoNewline
try {
    Copy-Item -Path (Join-Path $sourcePath "Assemblies") -Destination $modPath -Recurse -Force
    Write-Success " ?"
    $deployedCount++
} catch {
    Write-Err " ?"
    Write-Err "Error: $_"
    exit 1
}

# 3. Languages文件夹（可选）
Write-Host "[$($deployedCount+1)/$totalSteps] Copying Languages..." -NoNewline
$langPath = Join-Path $sourcePath "Languages"
if (Test-Path $langPath) {
    try {
        Copy-Item -Path $langPath -Destination $modPath -Recurse -Force
        Write-Success " ?"
    } catch {
        Write-Warn " ??  (Optional, skipped)"
    }
} else {
    Write-Warn " ??  (Not found, skipped)"
}
$deployedCount++

# 4. Defs文件夹（可选）
Write-Host "[$($deployedCount+1)/$totalSteps] Copying Defs..." -NoNewline
$defsPath = Join-Path $sourcePath "Defs"
if (Test-Path $defsPath) {
    try {
        Copy-Item -Path $defsPath -Destination $modPath -Recurse -Force
        Write-Success " ?"
    } catch {
        Write-Warn " ??  (Optional, skipped)"
    }
} else {
    Write-Warn " ??  (Not found, skipped)"
}
$deployedCount++

# 5. Docs文件夹（文档）
Write-Host "[$($deployedCount+1)/$totalSteps] Copying Docs..." -NoNewline
$docsPath = Join-Path $sourcePath "Docs"
if (Test-Path $docsPath) {
    try {
        Copy-Item -Path $docsPath -Destination $modPath -Recurse -Force
        Write-Success " ?"
    } catch {
        Write-Warn " ??  (Optional, skipped)"
    }
} else {
    Write-Warn " ??  (Not found, skipped)"
}
$deployedCount++

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Success "? Deployment Complete!"
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""

# 统计信息
Write-Info "?? Deployment Summary:"
Write-Host "   ? Mod Name: $modName"
Write-Host "   ? Version: v3.3.4 (Cache Optimization)"
Write-Host "   ? Location: $modPath"

# 检查DLL文件
$dllPath = Join-Path $modPath "Assemblies\RimTalk-ExpandMemory.dll"
if (Test-Path $dllPath) {
    $dllInfo = Get-Item $dllPath
    Write-Host "   ? DLL Size: $([math]::Round($dllInfo.Length/1KB, 2)) KB"
    Write-Host "   ? DLL Date: $($dllInfo.LastWriteTime.ToString('yyyy-MM-dd HH:mm'))"
}

Write-Host ""
Write-Info "?? Next Steps:"
Write-Host "   1. Launch RimWorld"
Write-Host "   2. Open Mod Manager (Options → Mods)"
Write-Host "   3. Enable 'RimTalk-ExpandMemory'"
Write-Host "   4. Restart game if prompted"
Write-Host "   5. Check Mod Settings → RimTalk-Expand Memory"
Write-Host "   6. Verify '?? 启用Prompt Caching' is checked"
Write-Host ""

Write-Info "?? New Features in v3.3.4:"
Write-Host "   ? ConversationCache optimization (4-5x hit rate)"
Write-Host "   ? PromptCache optimization (3-4x hit rate)"
Write-Host "   ? Prompt Caching (50% cost reduction)"
Write-Host "   ? Cache capacity doubled (200 + 100)"
Write-Host "   ?? Total API cost reduction: ~80%"
Write-Host ""

Write-Success "?? Happy gaming and enjoy the cost savings!"
Write-Host ""
