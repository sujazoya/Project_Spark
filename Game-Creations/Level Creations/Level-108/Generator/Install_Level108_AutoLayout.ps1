# Project Spark - Level 108 DFX-16 full-layout upgrade
# Requires existing Universal AutoLevelGenerator V2; never duplicates core generator classes.
# PowerShell 5.1+; close Unity and back up your project before use.
# Usage: powershell -ExecutionPolicy Bypass -File .\Install_Level108_AutoLayout.ps1 -UnityProjectPath 'D:\Unity\ProjectSpark'
param([Parameter(Mandatory=$true)][string]$UnityProjectPath)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath $UnityProjectPath).Path
$assets = Join-Path $project 'Assets'
if (-not (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt')) -or
    -not (Test-Path -LiteralPath $assets)) {
    throw 'UnityProjectPath must point to the Unity project root (Assets and ProjectSettings folders).'
}
$coreHits = @(Get-ChildItem -LiteralPath $assets -File -Filter SparkAutoLevelRuntime.cs -Recurse -ErrorAction SilentlyContinue)
if ($coreHits.Count -ne 1) {
    throw "Expected ONE Universal Generator V2 SparkAutoLevelRuntime.cs under Assets; found $($coreHits.Count). Remove duplicates or install V2 first."
}
$generator = Split-Path -Parent (Split-Path -Parent $coreHits[0].FullName)
if (-not (Test-Path -LiteralPath (Join-Path $generator 'Editor\SparkAutoLevelSourceImporter.cs'))) {
    throw 'Installed generator is missing SparkAutoLevelSourceImporter.cs. Use Universal Generator V2 before this upgrade.'
}
$manifests = @(Get-ChildItem -LiteralPath $assets -File -Filter L108_Models_152.csv -Recurse -ErrorAction SilentlyContinue)
if ($manifests.Count -gt 1) {
    throw 'Multiple Level 108 source manifests found. Remove duplicate imported asset sets before upgrade.'
}
$packageAssets = Join-Path $PSScriptRoot 'SourceAssets\Level108'
$hasPackageModels = Test-Path -LiteralPath (Join-Path $packageAssets 'Models_FBX\119_L108_DFX16_BarePCB.fbx')
if ($manifests.Count -eq 1) {
    $sourceRoot = Split-Path -Parent (Split-Path -Parent $manifests[0].FullName)
    Write-Host "Using existing Level 108 assets: $sourceRoot"
} else {
    if (-not $hasPackageModels) {
        throw 'No existing Level 108 source manifest and upgrade-only ZIP has no meshes. Use the COMPLETE Level 108 package or import the original Level 108 ZIP first.'
    }
    $sourceRoot = Join-Path $assets 'ProjectSpark\Level108'
    Write-Host "Preparing new Level 108 source folder: $sourceRoot"
}
# Collision checks before touching the project.
$recipeHits = @(Get-ChildItem -LiteralPath $assets -File -Filter Level108_DFX16.recipe.json -Recurse -ErrorAction SilentlyContinue)
if ($recipeHits.Count -gt 1) {
    throw 'More than one Level108_DFX16.recipe.json detected; keep one, then retry.'
}
$srcScripts = @(
    'GeneratorUpgrade\Editor\SparkAutoL108FullLayoutBuilder.cs',
    'GeneratorUpgrade\Runtime\SparkAutoL108LayoutRecord.cs',
    'GeneratorUpgrade\Runtime\SparkAutoL108WorkshopFit.cs',
    'GeneratorUpgrade\Recipes\Level108_DFX16.recipe.json'
)
foreach ($src in $srcScripts) {
    if (-not (Test-Path -LiteralPath (Join-Path $PSScriptRoot $src))) { throw "Package incomplete: $src" }
}
$editor = Join-Path $generator 'Editor'
$runtime = Join-Path $generator 'Runtime'
$recipes = Join-Path $generator 'Recipes'
New-Item -ItemType Directory -Force -Path $editor,$runtime,$recipes,$sourceRoot | Out-Null
function Backup-Then-Copy([string]$source,[string]$target) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
    if (Test-Path -LiteralPath $target) {
        Copy-Item -LiteralPath $target -Destination ($target + '.before_l108_upgrade.bak') -Force
    }
    Copy-Item -LiteralPath $source -Destination $target -Force
    Write-Host "Installed: $target"
}
Backup-Then-Copy (Join-Path $PSScriptRoot $srcScripts[0]) (Join-Path $editor 'SparkAutoL108FullLayoutBuilder.cs')
Backup-Then-Copy (Join-Path $PSScriptRoot $srcScripts[1]) (Join-Path $runtime 'SparkAutoL108LayoutRecord.cs')
Backup-Then-Copy (Join-Path $PSScriptRoot $srcScripts[2]) (Join-Path $runtime 'SparkAutoL108WorkshopFit.cs')
if ($hasPackageModels) {
    $copied = 0
    $skipped = 0
    Get-ChildItem -LiteralPath $packageAssets -File -Recurse | ForEach-Object {
        $relative = $_.FullName.Substring($packageAssets.Length).TrimStart([char]'\',[char]'/')
        $target = Join-Path $sourceRoot $relative
        if (Test-Path -LiteralPath $target) { $script:skipped++; return }
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $target
        $script:copied++
    }
    Write-Host "Copied Level 108 assets: $copied; preserved existing assets: $skipped"
}
# Normalized data are generated from original source CSV; overwrite ONLY these derived files.
$normalized = Join-Path $PSScriptRoot 'GeneratorUpgrade\NormalizedData'
foreach ($file in @('L108_AutoGen_Models_152.csv','L108_AutoGen_Routes_156.csv',
                   'L108_AutoGen_Vias_50.csv','L108_AutoGen_Objectives_6.json',
                   'L108_AutoGen_Faults_3.csv')) {
    Backup-Then-Copy (Join-Path $normalized $file) (Join-Path $sourceRoot ('Data\' + $file))
}
if ($recipeHits.Count -eq 1) { $recipePath = $recipeHits[0].FullName }
else { $recipePath = Join-Path $recipes 'Level108_DFX16.recipe.json' }
$recipe = Get-Content -LiteralPath (Join-Path $PSScriptRoot $srcScripts[3]) -Raw | ConvertFrom-Json
$recipe.sourceAssetsFolder = $sourceRoot.Substring($project.Length).TrimStart([char]'\',[char]'/').Replace('\','/')
$recipeText = $recipe | ConvertTo-Json -Depth 10
if (Test-Path -LiteralPath $recipePath) {
    Copy-Item -LiteralPath $recipePath -Destination ($recipePath + '.before_l108_upgrade.bak') -Force
}
Set-Content -LiteralPath $recipePath -Value $recipeText -Encoding UTF8
Write-Host "Installed recipe: $recipePath"
Write-Host 'COMPLETE - Reopen Unity and wait for all scripts/models to import.'
Write-Host 'Then choose Tools > Project Spark > Automatic Level Generator > 108 > 1. Build FULL Level_108_Layout.'
Write-Host 'Select your tabletop mesh and choose 108 > 2. Place Level 108 on SELECTED tabletop.'
Write-Warning 'All 156 routes, 50 vias and W157 physical pins remain UNASSIGNED/NOT_CERTIFIABLE. Generated hierarchy is source-backed art and integration scaffolding only.'
