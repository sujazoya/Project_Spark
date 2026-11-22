[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$UnityProjectPath,
    [switch]$DryRun,
    [switch]$ReplaceExisting
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $UnityProjectPath).Path
if (-not (Test-Path -LiteralPath (Join-Path $root 'ProjectSettings\ProjectVersion.txt') -PathType Leaf)) { throw 'Unity project root is invalid.' }
$relativeRoot = 'Assets\ProjectSpark\Level130\H610ME'
$source = Join-Path $PSScriptRoot $relativeRoot
if (-not (Test-Path -LiteralPath $source -PathType Container)) { throw 'Extract the full package first. Assets source folder missing.' }
$destination = Join-Path $root $relativeRoot
$manifest = Join-Path $PSScriptRoot 'SHA256SUMS.csv'
$rows = @(Import-Csv -LiteralPath $manifest)
if ($rows.Count -eq 0) { throw 'Package hash manifest is empty.' }
$models = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_FBX') -File -Filter '*.fbx')
if ($models.Count -ne 458) { throw ('Expected 458 binary FBX files, got {0}' -f $models.Count) }
if (@(Import-Csv -LiteralPath (Join-Path $source 'Data\ModelManifest.csv')).Count -ne 456) { throw 'Expected 456 model manifest records.' }
$pending = New-Object System.Collections.Generic.List[object]
$known = @{}
foreach ($row in $rows) {
    $rel = $row.relative_path.Replace('/','\')
    if ([string]::IsNullOrWhiteSpace($rel) -or [IO.Path]::IsPathRooted($rel) -or $rel.Contains(':') -or ($rel -split '\\') -contains '..' -or $known.ContainsKey($rel)) { throw ('Unsafe or duplicate relative path: {0}' -f $rel) }
    $known[$rel] = $true
    $src = Join-Path $source $rel
    $dst = Join-Path $destination $rel
    if (-not (Test-Path -LiteralPath $src -PathType Leaf)) { throw ('Missing source: {0}' -f $rel) }
    if ((Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -ne $row.sha256) { throw ('Source hash mismatch: {0}' -f $rel) }
    $exists = Test-Path -LiteralPath $dst -PathType Leaf
    if ($exists) {
        if ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -eq $row.sha256) { continue }
        if (-not $ReplaceExisting) { throw ('Existing file differs: {0}. Back up your project or explicitly use -ReplaceExisting.' -f $rel) }
    }
    $pending.Add([PSCustomObject]@{ Relative=$rel; Source=$src; Destination=$dst; Exists=$exists })
}
$mode = if ($DryRun) { 'DryRun' } else { 'Install' }
Write-Host ('[L130] Package {0}: {1} proposed changes; {2} verified source files.' -f $mode,$pending.Count,$rows.Count)
if ($DryRun) { Write-Host '[L130] DryRun completed; zero file changes.'; return }
$backup = Join-Path $root ('Level130_InstallerBackups\' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
foreach ($item in $pending) {
    if ($item.Exists) {
        $bk = Join-Path $backup $item.Relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $bk) -Force | Out-Null
        Copy-Item -LiteralPath $item.Destination -Destination $bk
    }
    New-Item -ItemType Directory -Path (Split-Path -Parent $item.Destination) -Force | Out-Null
    Copy-Item -LiteralPath $item.Source -Destination $item.Destination -Force
}
Write-Host '[L130] Installation completed. Reopen Unity and run Validate Sources.' -ForegroundColor Green
