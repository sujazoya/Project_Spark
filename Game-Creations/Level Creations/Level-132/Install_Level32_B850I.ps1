param([Parameter(Mandatory=$true)][string]$UnityProjectPath,[switch]$DryRun,[switch]$ReplaceExisting)
$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot 'Assets/ProjectSpark/Level32/B850ILightning'
$dest=Join-Path $UnityProjectPath 'Assets/ProjectSpark/Level32/B850ILightning'
if(!(Test-Path (Join-Path $UnityProjectPath 'ProjectSettings/ProjectVersion.txt'))){throw 'Not a Unity project'}
$rows=Import-Csv (Join-Path $PSScriptRoot 'SHA256SUMS.csv')
foreach($row in $rows){$src=Join-Path $root $row.relative_path;$dst=Join-Path $dest $row.relative_path;if((Get-FileHash $src -Algorithm SHA256).Hash -ne $row.sha256){throw "Hash mismatch: $src"};if((Test-Path $dst) -and ((Get-FileHash $dst -Algorithm SHA256).Hash -ne $row.sha256) -and !$ReplaceExisting){throw "Existing different asset: $dst"}}
if($DryRun){Write-Host "Verified $($rows.Count) files; no changes";return}
$backup=Join-Path $UnityProjectPath ('SparkBackups/B850I_'+(Get-Date -Format yyyyMMdd_HHmmss_fff))
foreach($row in $rows){$src=Join-Path $root $row.relative_path;$dst=Join-Path $dest $row.relative_path;if(Test-Path $dst){if((Get-FileHash $dst -Algorithm SHA256).Hash -eq $row.sha256){continue};$bak=Join-Path $backup $row.relative_path;New-Item -ItemType Directory -Force (Split-Path $bak)|Out-Null;Copy-Item $dst $bak};New-Item -ItemType Directory -Force (Split-Path $dst)|Out-Null;Copy-Item $src $dst -Force}
Write-Host 'B850I Level 32 installed. Other board folders unchanged.'
