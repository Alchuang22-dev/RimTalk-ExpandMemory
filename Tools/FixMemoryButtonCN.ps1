$ErrorActionPreference = 'Stop'
$target = "Languages/ChineseSimplified/DefInjected/MainButtonDef/RimTalk_Memory.xml"
$enc = New-Object System.Text.UTF8Encoding $true
$content = @'
<?xml version="1.0" encoding="utf-8"?>
<LanguageData>
  <RimTalk_Memory.label>记忆</RimTalk_Memory.label>
  <RimTalk_Memory.description>查看殖民者的记忆与互动（对话需 RimTalk 集成，观察尚未实现）。</RimTalk_Memory.description>
</LanguageData>
'@

$dir = Split-Path -Parent $target
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
[System.IO.File]::WriteAllText($target, $content, $enc)
Write-Host "Wrote UTF-8 BOM file: $target" -ForegroundColor Green
