# Project Spark - Level 104 Memory Matrix (MMX-12) AAA-style visual-layout upgrade
# Safe for Windows PowerShell 5.1+. No administrative privileges required.
# Never edits files for Levels 1-103. Never auto-creates electrical netlists.
[CmdletBinding()]
param(
    [string]$UnityProjectPath = 'D:\Project_Spark_Master_Project\Project_Spark'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Require-Directory([string]$Path, [string]$Description) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "$Description not found: $Path"
    }
}

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$assets = Join-Path $project 'Assets'
Require-Directory $assets 'Unity Assets folder'
Require-Directory (Join-Path $project 'ProjectSettings') 'Unity ProjectSettings folder'

$package = $PSScriptRoot
$sourceKit = Join-Path $package 'SourceAssets\Level104_MMX12'
$upgrade = Join-Path $package 'GeneratorUpgrade'
Require-Directory $sourceKit 'Level 104 package SourceAssets'
Require-Directory (Join-Path $sourceKit 'Models_OBJ') 'Generated OBJ models'
Require-Directory (Join-Path $sourceKit 'URP_Packed') 'URP packed PBR maps'
Require-Directory $upgrade 'GeneratorUpgrade scripts'

$requiredSources = @(
    (Join-Path $upgrade 'Editor\SparkL104FullLayoutBuilder.cs'),
    (Join-Path $upgrade 'Editor\SparkL104URPPBRMaterialBuilder.cs'),
    (Join-Path $upgrade 'Runtime\SparkL104LayoutMetadata.cs')
)
foreach ($required in $requiredSources) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Missing required generator script: $required"
    }
}

# Prefer an already-imported source root: keeps Unity .meta GUIDs and references stable.
$existingManifests = @(Get-ChildItem -LiteralPath $assets -Recurse -Filter 'ModelManifest.csv' -File |
    Where-Object { $_.Directory.Name -eq 'Data' -and $_.Directory.Parent.Name -eq 'Level104_MMX12' })
if ($existingManifests.Count -gt 1) {
    throw 'Multiple installed Level104_MMX12 source manifests detected. Resolve duplicates before installing.'
}
if ($existingManifests.Count -eq 1) {
    $destinationKit = $existingManifests[0].Directory.Parent.FullName
    Write-Host "Using existing Level 104 kit: $destinationKit" -ForegroundColor Cyan
} else {
    $destinationKit = Join-Path $assets 'ProjectSpark\Level104\Level104_MMX12'
    if (-not (Test-Path -LiteralPath (Join-Path $sourceKit 'Data\ModelManifest.csv'))) {
        throw 'Upgrade-only package requires the ORIGINAL Level 104 kit to already be installed.'
    }
    Write-Host "Installing new Level 104 kit: $destinationKit" -ForegroundColor Cyan
}

$backupRoot = Join-Path $project (
    'ProjectSpark_Level104_InstallerBackup_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$script:backupCount = 0
$script:copiedCount = 0
$script:unchangedCount = 0
$script:keptOriginalCount = 0

function Install-OneFile([string]$SourceFile, [string]$DestinationFile, [bool]$PreserveOriginal) {
    $destParent = Split-Path -Path $DestinationFile -Parent
    if (-not (Test-Path -LiteralPath $destParent -PathType Container)) {
        New-Item -ItemType Directory -Path $destParent -Force | Out-Null
    }
    if (Test-Path -LiteralPath $DestinationFile -PathType Leaf) {
        if ($PreserveOriginal) {
            $script:keptOriginalCount++
            return
        }
        $sameHash = (Get-FileHash -LiteralPath $SourceFile -Algorithm SHA256).Hash -eq
                    (Get-FileHash -LiteralPath $DestinationFile -Algorithm SHA256).Hash
        if ($sameHash) {
            $script:unchangedCount++
            return
        }
        # Back up ANY replaced file, retaining its location relative to the project.
        $relativeProjectFile = $DestinationFile.Substring($project.Length).TrimStart([char[]]@('\', '/'))
        $backupFile = Join-Path $backupRoot $relativeProjectFile
        $backupParent = Split-Path -Path $backupFile -Parent
        if (-not (Test-Path -LiteralPath $backupParent -PathType Container)) {
            New-Item -ItemType Directory -Path $backupParent -Force | Out-Null
        }
        Copy-Item -LiteralPath $DestinationFile -Destination $backupFile -Force
        $script:backupCount++
    }
    Copy-Item -LiteralPath $SourceFile -Destination $DestinationFile -Force
    $script:copiedCount++
}

function Install-Tree([string]$SourceDir, [string]$DestinationDir, [bool]$PreserveOriginal) {
    if (-not (Test-Path -LiteralPath $SourceDir -PathType Container)) { return }
    $root = (Get-Item -LiteralPath $SourceDir).FullName
    foreach ($item in (Get-ChildItem -LiteralPath $root -File -Recurse)) {
        $relative = $item.FullName.Substring($root.Length).TrimStart([char[]]@('\', '/'))
        $destination = Join-Path $DestinationDir $relative
        Install-OneFile $item.FullName $destination $PreserveOriginal
    }
}

# Source art is immutable if already imported: no destruction or GUID regeneration.
foreach ($directory in @('Models_FBX', 'Models_GLB', 'Textures', 'Data', 'Documentation', 'Unity')) {
    Install-Tree (Join-Path $sourceKit $directory) (Join-Path $destinationKit $directory) $true
}

# Generated OBJ fallbacks and masks are safe to update; back up previous versions.
Install-Tree (Join-Path $sourceKit 'Models_OBJ') (Join-Path $destinationKit 'Models_OBJ') $false
Install-Tree (Join-Path $sourceKit 'URP_Packed') (Join-Path $destinationKit 'URP_Packed') $false

# Install Level-104-only scripts, no shared level manager edits.
Install-Tree (Join-Path $upgrade 'Editor') (Join-Path $destinationKit 'Unity\Editor') $false
Install-Tree (Join-Path $upgrade 'Runtime') (Join-Path $destinationKit 'Unity\Runtime') $false
Install-Tree (Join-Path $upgrade 'Documentation') (Join-Path $destinationKit 'Documentation\AAA_AutoLayout') $false

Write-Host ''
Write-Host '[L104] Installation complete.' -ForegroundColor Green
Write-Host "New/updated files: $script:copiedCount"
Write-Host "Backed-up files:   $script:backupCount"
Write-Host "Unchanged files:   $script:unchangedCount"
Write-Host "Originals kept:    $script:keptOriginalCount"
if ($script:backupCount -gt 0) { Write-Host "Backup location: $backupRoot" }
Write-Host 'Existing Levels 1-103 were not modified.'
Write-Host 'Open Unity and allow imports to finish.'
Write-Host 'Menu: Tools > Project Spark > Automatic Level Generator > 104 > 3. Validate Source and 3D Models'
Write-Host 'Then: Tools > Project Spark > Automatic Level Generator > 104 > 1. Build FULL Level_104_Layout'
Write-Host 'Select Table_top; use 104 > 2. Place Level 104 on SELECTED tabletop (visual-only preview).'
Write-Warning 'Electrical routes/vias and W125 pins remain NOT CERTIFIABLE. Unity compilation/Play Mode not tested.'
