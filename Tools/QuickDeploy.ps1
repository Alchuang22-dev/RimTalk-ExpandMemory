$ErrorActionPreference = 'Stop'
$RimWorldPath = 'D:\steam\steamapps\common\RimWorld'
$targetModPath = Join-Path $RimWorldPath 'Mods\RimTalk-MemoryPatch'

Write-Host "=== RimTalk Memory Patch Deployment ===" -ForegroundColor Cyan
Write-Host "Target: $targetModPath" -ForegroundColor Yellow
Write-Host ""

# Clean old deployment
if (Test-Path $targetModPath) {
    Write-Host "[1/4] Removing old deployment..." -ForegroundColor Yellow
    Remove-Item $targetModPath -Recurse -Force
}

# Create directory structure
Write-Host "[2/4] Creating directories..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $targetModPath -Force | Out-Null
New-Item -ItemType Directory -Path "$targetModPath\1.6\Assemblies" -Force | Out-Null

# Copy files
Write-Host "[3/4] Copying files..." -ForegroundColor Yellow
$folders = @('About', 'Defs', 'Languages', '1.6')
foreach ($folder in $folders) {
    if (Test-Path $folder) {
        Write-Host "  Copying: $folder" -ForegroundColor Gray
        Copy-Item -Path $folder -Destination $targetModPath -Recurse -Force
    }
}

# Verify deployment
Write-Host "[4/4] Verifying deployment..." -ForegroundColor Yellow
$checkFiles = @(
    'About\About.xml',
    'Defs\MainButtonDef.xml',
    'Languages\ChineseSimplified\DefInjected\MainButtonDef\RimTalk_Memory.xml',
    'Languages\ChineseSimplified\Keyed\MemoryPatch.xml',
    '1.6\Assemblies\RimTalkMemoryPatch.dll'
)

$allOk = $true
foreach ($file in $checkFiles) {
    $path = Join-Path $targetModPath $file
    if (Test-Path $path) {
        Write-Host "  [OK] $file" -ForegroundColor Green
    } else {
        Write-Host "  [ERR] $file" -ForegroundColor Red
        $allOk = $false
    }
}

Write-Host ""
if ($allOk) {
    Write-Host "=== Deployment Successful ===" -ForegroundColor Green
    Write-Host "Mod installed at: $targetModPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Launch RimWorld" -ForegroundColor White
    Write-Host "2. Enable: Harmony -> RimTalk -> RimTalk-MemoryPatch" -ForegroundColor White
    Write-Host "3. Restart game" -ForegroundColor White
} else {
    Write-Host "=== Deployment Failed ===" -ForegroundColor Red
    Write-Host "Some files are missing." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
