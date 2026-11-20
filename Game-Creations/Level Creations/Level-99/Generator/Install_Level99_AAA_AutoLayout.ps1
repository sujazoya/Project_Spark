[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)] [string]$UnityProjectPath,
    [switch]$ReplaceSourceAssets
)
$ErrorActionPreference = 'Stop'
$project = [System.IO.Path]::GetFullPath($UnityProjectPath).TrimEnd('\','/')
$assets = Join-Path $project 'Assets'
if (-not (Test-Path -LiteralPath $assets -PathType Container) -or
    -not (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings') -PathType Container)) {
    throw "Not a Unity project (Assets and ProjectSettings missing): $project"
}
$bundleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceRoot = Join-Path $bundleRoot 'SourceAssets\Level99'
$upgradeRoot = Join-Path $bundleRoot 'GeneratorUpgrade'
if (-not (Test-Path -LiteralPath (Join-Path $sourceRoot 'Data\L99_AutoGen_Models_80.csv'))) {
    throw 'Generated Level 99 data missing. Extract the complete ZIP first.'
}
$installedImporters = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter 'SparkAutoLevelSourceImporter.cs')
if ($installedImporters.Count -ne 1) {
    throw "Expected exactly ONE existing Universal Auto Level Generator V2; found $($installedImporters.Count). Remove duplicate generators before installing."
}
$generator = Split-Path -Parent (Split-Path -Parent $installedImporters[0].FullName)
Write-Host "Updating existing generator: $generator"
$existingManifests = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter 'Model_Manifest_80.csv')
if ($existingManifests.Count -gt 1) { throw 'Multiple Level 99 source manifests exist. Consolidate duplicate kits before installing.' }
if ((Test-Path -LiteralPath (Join-Path $bundleRoot 'UPGRADE_ONLY.txt')) -and $existingManifests.Count -eq 0) {
    throw 'Upgrade-only ZIP requires an existing Level 99 source kit (FBX/GLB + textures). Use the FULL ZIP instead.'
}
if ($existingManifests.Count -eq 1) {
    $levelAssets = Split-Path -Parent (Split-Path -Parent $existingManifests[0].FullName)
    Write-Host "Reusing existing Level 99 source folder: $levelAssets"
} else {
    $levelAssets = Join-Path $assets 'ProjectSpark\Level99'
    Write-Host "Installing new Level 99 source folder: $levelAssets"
}
$backupRoot = Join-Path $project ('ProjectSpark_Level99_Backups\' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$script:changed = 0; $script:preserved = 0
function Copy-Safely([string]$from,[string]$to,[bool]$overwrite) {
    $parent = Split-Path -Parent $to
    if (-not (Test-Path -LiteralPath $parent)) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
    if (Test-Path -LiteralPath $to -PathType Leaf) {
        if (-not $overwrite) { $script:preserved++; return }
        $relative = $to.Substring($project.Length).TrimStart('\','/')
        $backup = Join-Path $backupRoot $relative
        $backupParent = Split-Path -Parent $backup
        if (-not (Test-Path -LiteralPath $backupParent)) {
            New-Item -ItemType Directory -Force -Path $backupParent | Out-Null
        }
        Copy-Item -LiteralPath $to -Destination $backup -Force
    }
    Copy-Item -LiteralPath $from -Destination $to -Force
    $script:changed++
}

Get-ChildItem -LiteralPath $sourceRoot -File -Recurse | ForEach-Object {
    $relative = $_.FullName.Substring($sourceRoot.Length).TrimStart('\','/')
    $destination = Join-Path $levelAssets $relative
    $isGeneratedOwned = $relative -like 'Data\L99_AutoGen_*' -or
                        $relative -like 'Data\L99_VisualHitTargets_*' -or
                        $relative -like 'Data\L99_GLB_OBJ_ConversionAudit.json' -or
                        $relative -like 'Models_OBJ_Fallback\*'
    Copy-Safely $_.FullName $destination ($isGeneratedOwned -or [bool]$ReplaceSourceAssets)
}
foreach ($dir in @('Editor','Runtime','Recipes')) {
    $from = Join-Path $upgradeRoot $dir
    if (-not (Test-Path -LiteralPath $from)) { continue }
    Get-ChildItem -LiteralPath $from -File | ForEach-Object {
        $destination = Join-Path (Join-Path $generator $dir) $_.Name
        Copy-Safely $_.FullName $destination $true
        Write-Host "Generator file: $destination"
    }
}
Write-Host ''
Write-Host 'LEVEL 99 FULL LAYOUT GENERATOR INSTALLED.'
Write-Host "Changed/copied files: $changed | Preserved existing files: $preserved"
Write-Host "Replacement backups: $backupRoot"
Write-Host 'Open Unity. Let compilation and model importing finish.'
Write-Host 'Build: Tools > Project Spark > Automatic Level Generator > 99 > 1. Build FULL Level_99_Layout'
Write-Host 'Place: select Table_top > 99 > 2. Place Level 99 on SELECTED tabletop'
Write-Warning 'Art/layout ONLY: 82 routes, 18 vias, and W083 physical pins remain SCHEMATIC_REQUIRED. No electrical objectives certified.'
