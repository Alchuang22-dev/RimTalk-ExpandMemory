# 一键部署脚本 - 简化版
# 使用方法：右键点击此文件 → "使用PowerShell运行"

$ErrorActionPreference = "Stop"

# 路径配置
$sourcePath = Split-Path -Parent $MyInvocation.MyCommand.Path
$targetPath = "D:\SteamLibrary\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RimTalk-ExpandMemory v3.3.4 部署" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 创建目标目录
if (-not (Test-Path $targetPath)) {
    Write-Host "创建目录: $targetPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
}

# 复制文件
Write-Host "正在复制文件..." -ForegroundColor Green

try {
    # About
    Write-Host "  [1/3] About..." -NoNewline
    Copy-Item -Path "$sourcePath\About" -Destination "$targetPath\About" -Recurse -Force
    Write-Host " 完成" -ForegroundColor Green
    
    # Assemblies
    Write-Host "  [2/3] Assemblies..." -NoNewline
    Copy-Item -Path "$sourcePath\Assemblies" -Destination "$targetPath\Assemblies" -Recurse -Force
    Write-Host " 完成" -ForegroundColor Green
    
    # Languages (如果存在)
    Write-Host "  [3/3] Languages..." -NoNewline
    if (Test-Path "$sourcePath\Languages") {
        Copy-Item -Path "$sourcePath\Languages" -Destination "$targetPath\Languages" -Recurse -Force
        Write-Host " 完成" -ForegroundColor Green
    } else {
        Write-Host " 跳过" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  部署成功！" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Mod位置: $targetPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "下一步:" -ForegroundColor Yellow
    Write-Host "  1. 启动RimWorld"
    Write-Host "  2. 在Mod管理器中启用 'RimTalk-ExpandMemory'"
    Write-Host "  3. 重启游戏"
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "部署失败: $_" -ForegroundColor Red
    Write-Host ""
}

Write-Host "按任意键退出..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
