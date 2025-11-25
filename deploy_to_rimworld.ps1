# RimTalk-ExpandMemory Mod 部署脚本
# 将编译好的 Mod 部署到 RimWorld Mods 目录

$ErrorActionPreference = "Stop"

# 路径配置
$SourceDir = "C:\Users\Administrator\Desktop\rim mod\RimTalk-ExpandMemory"
$TargetDir = "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "RimTalk-ExpandMemory Mod 部署工具" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# 检查源目录
if (-not (Test-Path $SourceDir)) {
    Write-Host "? 错误: 源目录不存在: $SourceDir" -ForegroundColor Red
    exit 1
}

Write-Host "?? 源目录: $SourceDir" -ForegroundColor Green
Write-Host "?? 目标目录: $TargetDir" -ForegroundColor Green
Write-Host ""

# 创建目标目录（如果不存在）
if (-not (Test-Path $TargetDir)) {
    Write-Host "?? 创建目标目录..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
}

# 定义需要复制的文件夹
$FoldersToCopy = @(
    "About",
    "Assemblies",
    "Defs",
    "Languages",
    "Textures",
    "Sounds",
    "Patches"
)

# 定义需要复制的文件
$FilesToCopy = @(
    "About.xml",
    "LICENSE",
    "README.md"
)

Write-Host "?? 开始部署..." -ForegroundColor Yellow
Write-Host ""

# 复制文件夹
foreach ($folder in $FoldersToCopy) {
    $sourcePath = Join-Path $SourceDir $folder
    $targetPath = Join-Path $TargetDir $folder
    
    if (Test-Path $sourcePath) {
        Write-Host "  ?? 复制 $folder..." -ForegroundColor Cyan
        
        # 删除旧文件夹
        if (Test-Path $targetPath) {
            Remove-Item -Path $targetPath -Recurse -Force
        }
        
        # 复制新文件夹
        Copy-Item -Path $sourcePath -Destination $targetPath -Recurse -Force
        Write-Host "     ? 完成" -ForegroundColor Green
    } else {
        Write-Host "  ??  跳过 $folder (不存在)" -ForegroundColor DarkGray
    }
}

Write-Host ""

# 复制根目录文件
foreach ($file in $FilesToCopy) {
    $sourcePath = Join-Path $SourceDir $file
    $targetPath = Join-Path $TargetDir $file
    
    if (Test-Path $sourcePath) {
        Write-Host "  ?? 复制 $file..." -ForegroundColor Cyan
        Copy-Item -Path $sourcePath -Destination $targetPath -Force
        Write-Host "     ? 完成" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "? 部署完成！" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Mod 已部署到: $TargetDir" -ForegroundColor White
Write-Host ""
Write-Host "下一步:" -ForegroundColor Yellow
Write-Host "1. 启动 RimWorld" -ForegroundColor White
Write-Host "2. 在 Mod 管理器中启用 'RimTalk-Expand Memory'" -ForegroundColor White
Write-Host "3. 确保 Harmony 在加载顺序中位于此 Mod 之前" -ForegroundColor White
Write-Host ""

# 显示部署的文件统计
$deployedFiles = Get-ChildItem -Path $TargetDir -Recurse -File
$totalSize = ($deployedFiles | Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host "?? 部署统计:" -ForegroundColor Cyan
Write-Host "  文件总数: $($deployedFiles.Count)" -ForegroundColor White
Write-Host "  总大小: $("{0:N2}" -f $totalSize) MB" -ForegroundColor White
Write-Host ""

# 检查 About.xml
$aboutXml = Join-Path $TargetDir "About\About.xml"
if (Test-Path $aboutXml) {
    Write-Host "? About.xml 已验证" -ForegroundColor Green
    
    # 显示版本信息
    $content = Get-Content $aboutXml -Raw
    if ($content -match 'Version:\s*([\d.]+)') {
        Write-Host "   版本: $($Matches[1])" -ForegroundColor White
    }
    if ($content -match '<author>(.+?)</author>') {
        Write-Host "   作者: $($Matches[1])" -ForegroundColor White
    }
} else {
    Write-Host "??  警告: About.xml 未找到" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "按任意键退出..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
