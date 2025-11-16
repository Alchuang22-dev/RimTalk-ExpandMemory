param(
  [string]$ModPath = 'D:\steam\steamapps\common\Rimworld\Mods\RimTalk-MemoryPatch'
)

Write-Host "Checking: $ModPath" -ForegroundColor Cyan
if (!(Test-Path $ModPath)) { Write-Host 'Missing mod folder.' -ForegroundColor Red; exit 1 }

$files = @(
  'About\About.xml',
  'Defs\MainButtonDef.xml',
  'Languages\ChineseSimplified\DefInjected\MainButtonDef\RimTalk_Memory.xml',
  'Languages\ChineseSimplified\Keyed\MemoryPatch.xml',
  '1.6\Assemblies\RimTalkMemoryPatch.dll'
)

$allOk = $true
foreach ($f in $files) {
  $p = Join-Path $ModPath $f
  if (Test-Path $p) { Write-Host "OK  $f" -ForegroundColor Green }
  else { Write-Host "ERR $f" -ForegroundColor Red; $allOk = $false }
}

if ($allOk) { Write-Host 'Deployment looks good.' -ForegroundColor Green; exit 0 }
else { Write-Host 'Deployment incomplete.' -ForegroundColor Yellow; exit 2 }
