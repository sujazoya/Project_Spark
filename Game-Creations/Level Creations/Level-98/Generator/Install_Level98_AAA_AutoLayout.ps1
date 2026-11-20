[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$UnityProjectPath,
    [switch]$ReplaceSourceAssets
)

$ErrorActionPreference = 'Stop'
$project = [System.IO.Path]::GetFullPath($UnityProjectPath).TrimEnd('\', '/')
$assets = Join-Path $project 'Assets'
$settings = Join-Path $project 'ProjectSettings'
if (-not (Test-Path -LiteralPath $assets -PathType Container) -or -not (Test-Path -LiteralPath $settings -PathType Container)) {
    throw "Not a Unity project: $project (Assets/ and ProjectSettings/ required)"
}
$bundleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceRoot = Join-Path $bundleRoot 'SourceAssets\Level98'
$generatorUpgrade = Join-Path $bundleRoot 'GeneratorUpgrade'
if (-not (Test-Path -LiteralPath (Join-Path $sourceRoot 'Data\L98_AutoGen_Models_74.csv'))) {
    throw 'Level 98 source assets missing. Extract the COMPLETE ZIP and run this script from its extracted root.'
}

# Discover existing generator, so no second copy and no duplicate assembly/class definitions.
$knownGenerator = Join-Path $assets 'My_Assets\_Project_Spark\Scripts\ProjectSpark\AutoLevelGenerator'
$knownImporter = Join-Path $knownGenerator 'Editor\SparkAutoLevelSourceImporter.cs'
$foundImporters = @(Get-ChildItem -LiteralPath $assets -File -Recurse -Filter 'SparkAutoLevelSourceImporter.cs')
if ($foundImporters.Count -ne 1) {
    throw "Expected ONE installed Universal Generator V2 importer; found $($foundImporters.Count). Resolve duplicate/absent generators before installing."
}
$generator = Split-Path -Parent (Split-Path -Parent $foundImporters[0].FullName)
Write-Host "Existing generator: $generator"
if ($generator -ne $knownGenerator) { Write-Host 'Existing generator is relocated; upgrading at the discovered path.' }

# Reuse existing L98 kit wherever installed, rather than copying duplicated 75 FBXs into Assets.
$existingManifests = @(Get-ChildItem -LiteralPath $assets -File -Recurse -Filter 'Model_Manifest_74.csv')
if ($existingManifests.Count -gt 1) {
    throw 'Multiple Level 98 model manifests detected. Please consolidate duplicate Level98 asset copies before installing.'
}
if ((Test-Path -LiteralPath (Join-Path $bundleRoot 'UPGRADE_ONLY.txt')) -and $existingManifests.Count -eq 0) {
    throw 'Upgrade-only package requires existing Level 98 source FBX/GLB/textures. Install the COMPLETE Level 98 ZIP instead.'
}
if ($existingManifests.Count -eq 1) {
    $levelAssets = Split-Path -Parent (Split-Path -Parent $existingManifests[0].FullName)
    Write-Host "Reusing installed Level 98 assets: $levelAssets"
} else {
    $levelAssets = Join-Path $assets 'ProjectSpark\Level98'
    Write-Host "New Level 98 source assets folder: $levelAssets"
}

$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupRoot = Join-Path $project ("ProjectSpark_Level98_Backups\" + $stamp)
$changed = 0
$preserved = 0
function Copy-Safely([string]$from, [string]$to, [bool]$replace) {
    $parent = Split-Path -Parent $to
    if (-not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    if (Test-Path -LiteralPath $to -PathType Leaf) {
        if (-not $replace) { $script:preserved++; return }
        $relative = $to.Substring($project.Length).TrimStart('\', '/')
        $backup = Join-Path $backupRoot $relative
        $backupParent = Split-Path -Parent $backup
        if (-not (Test-Path -LiteralPath $backupParent)) {
            New-Item -ItemType Directory -Path $backupParent -Force | Out-Null
        }
        Copy-Item -LiteralPath $to -Destination $backup -Force
    }
    Copy-Item -LiteralPath $from -Destination $to -Force
    $script:changed++
}

# Source models/textures are preserved by default. Repair manifests are always current/versioned.
$files = Get-ChildItem -LiteralPath $sourceRoot -File -Recurse
foreach ($file in $files) {
    $relative = $file.FullName.Substring($sourceRoot.Length).TrimStart('\', '/')
    $target = Join-Path $levelAssets $relative
    $isNormalized = ($relative -like 'Data\L98_AutoGen_*' -or
                     $relative -like 'Data\L98_VisualHitTargets_*' -or
                     $relative -like 'Data\L98_GLB_OBJ_ConversionAudit.json')
    Copy-Safely $file.FullName $target ($isNormalized -or [bool]$ReplaceSourceAssets)
}

foreach ($dir in @('Editor', 'Runtime', 'Recipes')) {
    $fromDir = Join-Path $generatorUpgrade $dir
    if (-not (Test-Path -LiteralPath $fromDir)) { continue }
    Get-ChildItem -LiteralPath $fromDir -File | ForEach-Object {
        $target = Join-Path (Join-Path $generator $dir) $_.Name
        Copy-Safely $_.FullName $target $true
        Write-Host "Updated generator file: $target"
    }
}

Write-Host ''
Write-Host 'COMPLETE - Level 98 AAA-style visual generator upgrade installed.'
Write-Host "Created/replaced files: $changed | Preserved: $preserved"
Write-Host "Backups (only when replacements existed): $backupRoot"
Write-Host 'Reopen Unity and let it finish model import and script compilation.'
Write-Host 'Menu: Tools > Project Spark > Automatic Level Generator > 98 > 1. Build FULL Level_98_Layout'
Write-Host 'Select Table_top: Tools > Project Spark > Automatic Level Generator > 98 > 2. Place Level 98 on SELECTED tabletop'
Write-Warning 'Electrical netlist NOT verified: 74 routes + 16 vias + W075 physical pins remain blocked. Do NOT certify gameplay from art.'
