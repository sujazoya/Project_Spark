# PROJECT SPARK / LEVEL 114 / THE THERMAL DOMINION / TDX-22
# Additive, idempotent install into Assets/ProjectSpark/Level114/TDX22 only.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$UnityProjectPath,
    [switch]$DryRun,
    [switch]$ReplaceOriginalArt
)
$ErrorActionPreference = 'Stop'
$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
if (-not (Test-Path -LiteralPath (Join-Path $project 'Assets') -PathType Container) -or
    -not (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt') -PathType Leaf)) {
    throw "Unity project not found (Assets + ProjectSettings/ProjectVersion.txt required): $project"
}
$source = Join-Path $PSScriptRoot 'ProjectSpark_Level114_TDX22'
$destination = Join-Path $project 'Assets\ProjectSpark\Level114\TDX22'
$manifest = Join-Path $source 'Data\L114_ModelManifest.csv'
if (-not (Test-Path -LiteralPath $manifest -PathType Leaf)) {
    throw "Level 114 source folder/Data is missing beside the installer: $source"
}
$modelsPresent = Test-Path -LiteralPath (Join-Path $source 'Models_OBJ') -PathType Container
$texturesPresent = Test-Path -LiteralPath (Join-Path $source 'Textures') -PathType Container
if (-not $modelsPresent -or -not $texturesPresent) {
    if (-not (Test-Path -LiteralPath (Join-Path $destination 'Models_OBJ') -PathType Container) -or
        -not (Test-Path -LiteralPath (Join-Path $destination 'Textures') -PathType Container)) {
        throw 'Upgrade-only package requires Level 114 complete source assets already installed.'
    }
}
# Fail fast on incomplete complete-package model conversion.
if ($modelsPresent) {
    $objCount = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_OBJ') -Filter '*.obj' -File).Count
    if ($objCount -ne 225) { throw "Expected 225 Level 114 OBJ models including assembled reference, found $objCount" }
}
$backup = Join-Path $project ('ProjectSpark_Backups\Level114_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$copied = 0; $unchanged = 0; $preserved = 0; $backedUp = 0
$files = @(Get-ChildItem -LiteralPath $source -Recurse -File | Where-Object { $_.Extension -ne '.meta' })
Write-Host "[L114] Installing isolated Level 114 files ($($files.Count)) into: $destination"
Write-Host "[L114] DryRun=$DryRun; ReplaceOriginalArt=$ReplaceOriginalArt"
foreach ($file in $files) {
    $relative = $file.FullName.Substring($source.Length).TrimStart([char[]]@('\','/'))
    $target = Join-Path $destination $relative
    $existing = Test-Path -LiteralPath $target -PathType Leaf
    $originalArt = $relative -match '^(Models_OBJ|Models_GLB|Textures)[\\/]'
    if ($existing) {
        if ((Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash -eq
            (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash) {
            $unchanged++
            continue
        }
        if ($originalArt -and -not $ReplaceOriginalArt) {
            $preserved++
            Write-Host "[L114] Preserved original art: $relative"
            continue
        }
    }
    if ($DryRun) {
        Write-Host "[DRY RUN] Would install: $relative"
        $copied++
        continue
    }
    $targetDir = Split-Path -Parent $target
    if (-not (Test-Path -LiteralPath $targetDir -PathType Container)) {
        New-Item -Path $targetDir -ItemType Directory -Force | Out-Null
    }
    if ($existing) {
        $backupFile = Join-Path $backup $relative
        $backupDir = Split-Path -Parent $backupFile
        if (-not (Test-Path -LiteralPath $backupDir -PathType Container)) {
            New-Item -Path $backupDir -ItemType Directory -Force | Out-Null
        }
        Copy-Item -LiteralPath $target -Destination $backupFile -Force
        $backedUp++
    }
    Copy-Item -LiteralPath $file.FullName -Destination $target -Force
    $copied++
}
Write-Host "[L114] New/updated=$copied; unchanged=$unchanged; original art preserved=$preserved; backups=$backedUp"
if ($backedUp -gt 0) { Write-Host "[L114] Backups: $backup" }
Write-Host '[L114] Other levels and shared Project Spark gameplay classes were not targeted.'
Write-Host '[L114] Unity menu: Tools > Project Spark > Automatic Level Generator > 114 > 3. Validate Sources and Report Blockers'
Write-Host '[L114] Then: 114 > 1. Build FULL Level_114_Layout'
Write-Warning '[L114] VISUAL/AUTHORING KIT ONLY: no verified pin-level schematic, no certified thermal model or 6/6 gameplay.'
