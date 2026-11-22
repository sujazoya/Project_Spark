[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$UnityProjectPath,[switch]$DryRun,[switch]$ReplaceExisting)
$ErrorActionPreference='Stop'
$root=(Resolve-Path -LiteralPath $UnityProjectPath).Path
if(-not (Test-Path -LiteralPath (Join-Path $root 'ProjectSettings\ProjectVersion.txt') -PathType Leaf)){throw 'Invalid Unity project root.'}
$relRoot='Assets\ProjectSpark\AAAArchitecture'
$source=Join-Path $PSScriptRoot $relRoot
$destination=Join-Path $root $relRoot
$rows=@(Import-Csv -LiteralPath (Join-Path $PSScriptRoot 'SHA256SUMS.csv'))
if($rows.Count -eq 0){throw 'Empty hash manifest.'}
$known=@{}; $pending=New-Object System.Collections.Generic.List[object]
foreach($row in $rows){
    $rel=$row.relative_path.Replace('/','\')
    if([string]::IsNullOrWhiteSpace($rel) -or [IO.Path]::IsPathRooted($rel) -or $rel.Contains(':') -or ($rel -split '\\') -contains '..' -or ($rel -split '\\') -contains '.' -or $known.ContainsKey($rel)){throw ('Unsafe or duplicate path: {0}' -f $rel)}
    if($rel -notmatch '^(Runtime|Editor|Documentation)\\'){throw ('Path outside source scope: {0}' -f $rel)}
    $known[$rel]=$true; $src=Join-Path $source $rel; $dst=Join-Path $destination $rel
    if(-not (Test-Path -LiteralPath $src -PathType Leaf)){throw ('Missing source: {0}' -f $rel)}
    if((Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -ne $row.sha256){throw ('Source hash mismatch: {0}' -f $rel)}
    $exists=Test-Path -LiteralPath $dst -PathType Leaf
    if($exists){if((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -eq $row.sha256){continue};if(-not $ReplaceExisting){throw ('Existing file differs: {0}. Explicit -ReplaceExisting required.' -f $rel)}}
    $pending.Add([PSCustomObject]@{Relative=$rel;Source=$src;Destination=$dst;Exists=$exists})
}
Write-Host ('Verified {0} files; {1} proposed changes.' -f $rows.Count,$pending.Count)
if($DryRun){Write-Host 'Dry run: no changes written.';return}
if($pending.Count -eq 0){Write-Host 'Already installed.';return}
$backup=Join-Path $root ('SparkAAA_InstallerBackups\'+(Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
# Backup all changed files before writing any file. Preserve Unity .meta files.
foreach($item in $pending){if($item.Exists){$bk=Join-Path $backup $item.Relative;New-Item -ItemType Directory -Path (Split-Path -Parent $bk) -Force|Out-Null;Copy-Item -LiteralPath $item.Destination -Destination $bk}}
$written=New-Object System.Collections.Generic.List[object]
try{
    foreach($item in $pending){New-Item -ItemType Directory -Path (Split-Path -Parent $item.Destination) -Force|Out-Null;$written.Add($item);Copy-Item -LiteralPath $item.Source -Destination $item.Destination -Force}
}catch{
    foreach($item in $written){if($item.Exists){Copy-Item -LiteralPath (Join-Path $backup $item.Relative) -Destination $item.Destination -Force}else{if(Test-Path -LiteralPath $item.Destination -PathType Leaf){Remove-Item -LiteralPath $item.Destination -Force}}}
    throw
}
Write-Host ('Installed into {0}. Source backups, if any: {1}' -f $destination,$backup) -ForegroundColor Green
