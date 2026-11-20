# Level 111 - The Signal Crucible - additive safe installer.
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$UnityProjectPath,
    [switch]$DryRun
)
$ErrorActionPreference = 'Stop'

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
if (-not (Test-Path (Join-Path $project 'Assets') -PathType Container) -or
    -not (Test-Path (Join-Path $project 'ProjectSettings') -PathType Container)) {
    throw "Not a Unity project. Expected Assets and ProjectSettings: $project"
}
$source = Join-Path $PSScriptRoot 'ProjectSpark_Level111_SIX19'
if (-not (Test-Path $source -PathType Container)) {
    throw "Package folder missing beside installer: $source"
}
$destination = Join-Path $project 'Assets\ProjectSpark\Level111\SIX19'
$hasModels = Test-Path (Join-Path $source 'Models_OBJ') -PathType Container
$hasTextures = Test-Path (Join-Path $source 'Textures') -PathType Container
if (-not $hasModels -or -not $hasTextures) {
    if (-not (Test-Path (Join-Path $destination 'Models_OBJ') -PathType Container) -or
        -not (Test-Path (Join-Path $destination 'Textures') -PathType Container)) {
        throw 'This is an upgrade-only package. Install the complete Level111 kit first.'
    }
}
# Validate the full original model kit BEFORE modifying any Unity project files.
$manifestPath = Join-Path $source 'Data\L111_ModelManifest.csv'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Missing model manifest: $manifestPath"
}
$modelRecords = @(Import-Csv -LiteralPath $manifestPath)
if ($modelRecords.Count -ne 176) {
    throw "Expected 176 manifest model records; found $($modelRecords.Count)."
}
foreach ($record in $modelRecords) {
    $objName = $record.obj
    if ([string]::IsNullOrWhiteSpace($objName) -or $objName -eq 'NOT_INCLUDED' -or
        -not $objName.EndsWith('.obj', [System.StringComparison]::OrdinalIgnoreCase) -or
        [System.IO.Path]::GetFileName($objName) -ne $objName) {
        throw "Invalid obj filename in manifest for $($record.asset_id): '$objName'"
    }
    if (-not (Test-Path -LiteralPath (Join-Path $source "Models_OBJ\$objName") -PathType Leaf)) {
        throw "Missing original OBJ source model: $objName"
    }
}
if (-not (Test-Path -LiteralPath (Join-Path $source 'Models_OBJ\177_L111_AssembledReference.obj') -PathType Leaf)) {
    throw 'Missing original assembled reference OBJ.'
}
Write-Host '[L111] Preflight OK: 176/176 manifest OBJ assets and assembled reference OBJ.'
$backup = Join-Path $project ('ProjectSpark_Backups\Level111_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$copied = 0; $unchanged = 0; $backedUp = 0
$files = @(Get-ChildItem -LiteralPath $source -Recurse -File)
Write-Host "[L111] Target Unity Project: $project"
Write-Host "[L111] Source files: $($files.Count)"
Write-Host "[L111] DryRun = $DryRun"
foreach ($file in $files) {
    $relative = $file.FullName.Substring($source.Length).TrimStart('\', '/')
    # Do not copy pre-existing Windows metadata or older level content.
    $targetFile = Join-Path $destination $relative
    $same = $false
    if (Test-Path -LiteralPath $targetFile -PathType Leaf) {
        $sourceHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        $targetHash = (Get-FileHash -LiteralPath $targetFile -Algorithm SHA256).Hash
        $same = ($sourceHash -eq $targetHash)
    }
    if ($same) { $unchanged++; continue }
    if ($DryRun) { Write-Host "[DRY RUN] $relative"; $copied++; continue }
    $parent = Split-Path -Parent $targetFile
    if (-not (Test-Path -LiteralPath $parent)) { New-Item -Path $parent -ItemType Directory -Force | Out-Null }
    if (Test-Path -LiteralPath $targetFile -PathType Leaf) {
        $backupFile = Join-Path $backup $relative
        $backupParent = Split-Path -Parent $backupFile
        if (-not (Test-Path -LiteralPath $backupParent)) { New-Item -Path $backupParent -ItemType Directory -Force | Out-Null }
        Copy-Item -LiteralPath $targetFile -Destination $backupFile -Force
        $backedUp++
    }
    Copy-Item -LiteralPath $file.FullName -Destination $targetFile -Force
    $copied++
}
Write-Host "[L111] New/updated: $copied; unchanged: $unchanged; backups: $backedUp"
if ($backedUp -gt 0) { Write-Host "[L111] Backups: $backup" }
Write-Host '[L111] Only Assets/ProjectSpark/Level111/SIX19 was targeted. Other levels/shared generator unchanged.'
Write-Host '[L111] Unity: Tools > Project Spark > Automatic Level Generator > 111 > 3. Validate Sources and Report Blockers'
Write-Host '[L111] Then run: 111 > 1. Build FULL Level_111_Layout'
Write-Warning '[L111] ALL electrical routes/vias and W185 remain SCHEMATIC_REQUIRED. Not a completed gameplay level.'
