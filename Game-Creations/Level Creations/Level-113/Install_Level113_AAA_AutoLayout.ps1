# Level 113 - The Redundancy Citadel - additive safe installer.
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
$source = Join-Path $PSScriptRoot 'ProjectSpark_Level113_RDX21'
if (-not (Test-Path $source -PathType Container)) {
    throw "Package folder missing beside installer: $source"
}
$destination = Join-Path $project 'Assets\ProjectSpark\Level113\RDX21'
$hasModels = Test-Path (Join-Path $source 'Models_OBJ') -PathType Container
$hasTextures = Test-Path (Join-Path $source 'Textures') -PathType Container
if (-not $hasModels -or -not $hasTextures) {
    if (-not (Test-Path (Join-Path $destination 'Models_OBJ') -PathType Container) -or
        -not (Test-Path (Join-Path $destination 'Textures') -PathType Container)) {
        throw 'This is an upgrade-only package. Install the complete Level113 kit first.'
    }
}
$backup = Join-Path $project ('ProjectSpark_Backups\Level113_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$copied = 0; $unchanged = 0; $backedUp = 0
$files = @(Get-ChildItem -LiteralPath $source -Recurse -File)
Write-Host "[L113] Target Unity Project: $project"
Write-Host "[L113] Source files: $($files.Count)"
Write-Host "[L113] DryRun = $DryRun"
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
Write-Host "[L113] New/updated: $copied; unchanged: $unchanged; backups: $backedUp"
if ($backedUp -gt 0) { Write-Host "[L113] Backups: $backup" }
Write-Host '[L113] Only Assets/ProjectSpark/Level113/RDX21 was targeted. Other levels/shared generator unchanged.'
Write-Host '[L113] Unity: Tools > Project Spark > Automatic Level Generator > 113 > 3. Validate Sources and Report Blockers'
Write-Host '[L113] Then run: 113 > 1. Build FULL Level_113_Layout'
Write-Warning '[L113] ALL electrical routes/vias and W233 remain SCHEMATIC_REQUIRED. Not a completed gameplay level.'
