# Project Spark Level 107 — PFX-15 full-layout addon for Universal Generator V2.
# Stop Unity and back up your project before installation. PowerShell 5.1+.
param([Parameter(Mandatory=$true)][string]$UnityProjectPath)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath $UnityProjectPath).Path
$assets = Join-Path $project 'Assets'
if (-not (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt')) -or
    -not (Test-Path -LiteralPath $assets)) { throw 'Not a Unity project root: expected Assets/ and ProjectSettings/ProjectVersion.txt.' }
$coreHits = @(Get-ChildItem -LiteralPath $assets -File -Filter SparkAutoLevelRuntime.cs -Recurse -ErrorAction SilentlyContinue)
if ($coreHits.Count -ne 1) { throw "Expected one installed Universal Generator V2 core, found $($coreHits.Count). Do not create duplicate generator copies." }
$generator = Split-Path -Parent (Split-Path -Parent $coreHits[0].FullName)
if (-not (Test-Path -LiteralPath (Join-Path $generator 'Editor\SparkAutoLevelSourceImporter.cs')) -or
    -not (Test-Path -LiteralPath (Join-Path $generator 'Editor\SparkAutoRecipeRegistry.cs'))) {
    throw 'Universal Generator V2 required. Missing importer or recipe registry.'
}
$srcScripts = @('GeneratorUpgrade\Editor\SparkAutoL107FullLayoutBuilder.cs',
 'GeneratorUpgrade\Runtime\SparkAutoL107LayoutRecord.cs',
 'GeneratorUpgrade\Runtime\SparkAutoL107WorkshopFit.cs',
 'GeneratorUpgrade\Recipes\Level107_PFX15.recipe.json')
foreach ($src in $srcScripts) {
 if (-not (Test-Path -LiteralPath (Join-Path $PSScriptRoot $src))) { throw "Package incomplete: $src" }
}
$packageAssets = Join-Path $PSScriptRoot 'SourceAssets\Level107'
$hasFullAssets = Test-Path -LiteralPath (Join-Path $packageAssets 'Models_FBX\113_L107_PFX15_BarePCB.fbx')
$existingManifests = @(Get-ChildItem -LiteralPath $assets -File -Filter L107_Models_144.csv -Recurse -ErrorAction SilentlyContinue)
if ($existingManifests.Count -gt 1) { throw 'Duplicate Level 107 source manifests detected. Keep one source package under Assets and retry.' }
if ($existingManifests.Count -eq 1) {
 $sourceRoot = Split-Path -Parent (Split-Path -Parent $existingManifests[0].FullName)
 Write-Host "Using existing Level 107 source folder: $sourceRoot"
} else {
 if (-not $hasFullAssets) { throw 'No existing Level 107 source and this upgrade-only ZIP has no models. Use the FULL package first.' }
 $sourceRoot = Join-Path $assets 'ProjectSpark\Level107'
 Write-Host "Preparing Level 107 source folder: $sourceRoot"
}
# Fail safely if another Level 107 builder copy would cause duplicate C# class errors.
$addonNames = @('SparkAutoL107FullLayoutBuilder.cs','SparkAutoL107LayoutRecord.cs','SparkAutoL107WorkshopFit.cs')
foreach ($addonName in $addonNames) {
 $hits = @(Get-ChildItem -LiteralPath $assets -File -Filter $addonName -Recurse -ErrorAction SilentlyContinue)
 if ($hits.Count -gt 1) { throw "Duplicate Level 107 script $addonName found. Keep only the installed generator addon copy." }
 if ($hits.Count -eq 1 -and -not ($hits[0].FullName.StartsWith($generator + [System.IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase))) {
   throw "A Level 107 script already exists outside your installed generator: $($hits[0].FullName). Move it before upgrading."
 }
}
$recHits = @(Get-ChildItem -LiteralPath $assets -File -Filter Level107_PFX15.recipe.json -Recurse -ErrorAction SilentlyContinue)
if ($recHits.Count -gt 1) { throw 'Duplicate Level107_PFX15.recipe.json files found. Keep one before upgrading.' }
$editor = Join-Path $generator 'Editor'; $runtime = Join-Path $generator 'Runtime'; $recipes = Join-Path $generator 'Recipes'
New-Item -ItemType Directory -Force -Path $editor,$runtime,$recipes,$sourceRoot | Out-Null
function Backup-Then-Copy([string]$source,[string]$target) {
 New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
 if (Test-Path -LiteralPath $target) {
    Copy-Item -LiteralPath $target -Destination ($target + '.before_l107_upgrade.bak') -Force
 }
 Copy-Item -LiteralPath $source -Destination $target -Force
 Write-Host "Installed: $target"
}
Backup-Then-Copy (Join-Path $PSScriptRoot $srcScripts[0]) (Join-Path $editor 'SparkAutoL107FullLayoutBuilder.cs')
Backup-Then-Copy (Join-Path $PSScriptRoot $srcScripts[1]) (Join-Path $runtime 'SparkAutoL107LayoutRecord.cs')
Backup-Then-Copy (Join-Path $PSScriptRoot $srcScripts[2]) (Join-Path $runtime 'SparkAutoL107WorkshopFit.cs')
if ($hasFullAssets) {
 $copied=0; $skipped=0
 Get-ChildItem -LiteralPath $packageAssets -File -Recurse | ForEach-Object {
  $relative = $_.FullName.Substring($packageAssets.Length).TrimStart([char]'\',[char]'/')
  $target = Join-Path $sourceRoot $relative
  if (Test-Path -LiteralPath $target) { $script:skipped++; return }
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
  Copy-Item -LiteralPath $_.FullName -Destination $target
  $script:copied++
 }
 Write-Host "Copied Level 107 source assets: $copied; preserved existing files: $skipped"
}
$norm = Join-Path $PSScriptRoot 'GeneratorUpgrade\NormalizedData'
foreach ($file in @('L107_AutoGen_Models_144.csv','L107_AutoGen_Routes_148.csv',
   'L107_AutoGen_Vias_46.csv','L107_AutoGen_Objectives_6.json','L107_AutoGen_Faults_3.csv')) {
 Backup-Then-Copy (Join-Path $norm $file) (Join-Path $sourceRoot ('Data\' + $file))
}
if ($recHits.Count -eq 1) { $recipePath = $recHits[0].FullName }
else { $recipePath = Join-Path $recipes 'Level107_PFX15.recipe.json' }
$recipe = Get-Content -LiteralPath (Join-Path $PSScriptRoot $srcScripts[3]) -Raw | ConvertFrom-Json
$recipe.sourceAssetsFolder = 'Assets/' + $sourceRoot.Substring($assets.Length).TrimStart([char]'\',[char]'/').Replace('\','/')
$recipeContent = $recipe | ConvertTo-Json -Depth 12
if (Test-Path -LiteralPath $recipePath) {
 Copy-Item -LiteralPath $recipePath -Destination ($recipePath + '.before_l107_upgrade.bak') -Force
}
# Write UTF-8 without BOM for Unity/JsonUtility compatibility.
[System.IO.File]::WriteAllText($recipePath, $recipeContent, (New-Object System.Text.UTF8Encoding($false)))
Write-Host "Installed Level 107 recipe: $recipePath"
Write-Host 'COMPLETE. Reopen Unity; wait for FBX import and compilation.'
Write-Host 'Build: Tools > Project Spark > Automatic Level Generator > 107 > 1. Build FULL Level_107_Layout.'
Write-Host 'Place: Select Table_top then run 107 > 2. Place Level 107 on SELECTED tabletop.'
Write-Warning '148 routes, 46 vias, and W149 pins lack approved physical mappings; gameplay certification remains blocked.'
