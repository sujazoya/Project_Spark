[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$UnityProjectPath,[switch]$DryRun)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $UnityProjectPath).Path
if (-not (Test-Path -LiteralPath (Join-Path $root 'ProjectSettings\ProjectVersion.txt') -PathType Leaf)) { throw 'Not a Unity project root.' }
$relativeRoot = 'Assets\ProjectSpark\Level128\X870E'
$dest = Join-Path $root $relativeRoot
$source = Join-Path $PSScriptRoot $relativeRoot
if (-not (Test-Path -LiteralPath (Join-Path $dest 'Data\ModelManifest.csv') -PathType Leaf)) { throw 'Install original Level 128 first.' }
if (@(Import-Csv -LiteralPath (Join-Path $dest 'Data\ModelManifest.csv')).Count -ne 710) { throw 'Original model manifest has unexpected count.' }
$rows = @(Import-Csv -LiteralPath (Join-Path $PSScriptRoot 'PATCH_SHA256.csv'))
if ($rows.Count -ne 714) { throw 'Hotfix inventory mismatch.' }
$pending = New-Object System.Collections.Generic.List[object]
$seen = @{}
foreach ($row in $rows) {
    $rel = $row.relative_path.Replace('/','\')
    if ($seen.ContainsKey($rel) -or ($rel -notmatch '^(Models_Native\\L128_[A-Za-z0-9_]+\.spark128mesh|Unity\\Editor\\SparkL128(BoardBuilder|NativeMeshImporter)\.cs)$')) { throw ('Unexpected patch path: {0}' -f $rel) }
    $seen[$rel] = $true
    $src = Join-Path $source $rel
    $dst = Join-Path $dest $rel
    if ((Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -ne $row.sha256) { throw ('Invalid patch hash: {0}' -f $rel) }
    if ((Test-Path -LiteralPath $dst -PathType Leaf) -and ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -eq $row.sha256)) { continue }
    $pending.Add([PSCustomObject]@{ Relative=$rel; Source=$src; Destination=$dst })
}
Write-Host ('[L128 NativeMesh] {0} verified files; {1} pending.' -f $rows.Count,$pending.Count)
if ($DryRun) { Write-Host 'DryRun completed; no changes.'; return }
$backup = Join-Path $root ('Level128_NativeMeshHotfix_Backups\' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
# Back up all overwritten files before any replacement.
foreach ($item in $pending) {
    if (Test-Path -LiteralPath $item.Destination -PathType Leaf) {
        $bk = Join-Path $backup $item.Relative
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $bk) | Out-Null
        Copy-Item -LiteralPath $item.Destination -Destination $bk
    }
}
foreach ($item in $pending) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $item.Destination) | Out-Null
    Copy-Item -LiteralPath $item.Source -Destination $item.Destination -Force
}
Write-Host '[L128] Native mesh hotfix installed. Open Unity, wait for script compilation and asset imports, then validate sources.' -ForegroundColor Green
