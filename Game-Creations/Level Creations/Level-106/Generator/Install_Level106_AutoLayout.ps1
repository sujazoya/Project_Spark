# Project Spark Level 106 -- SFX-14 First Boot, Universal AutoLevelGenerator V2 addon.
# Close Unity and back up the project first. PowerShell 5.1+.
param([Parameter(Mandatory=$true)][string]$UnityProjectPath)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath $UnityProjectPath).Path
$assets = Join-Path $project 'Assets'
if (-not (Test-Path -LiteralPath $assets) -or -not (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt'))) {
    throw 'Not a Unity project root. Expected Assets and ProjectSettings\ProjectVersion.txt.'
}
$cores = @(Get-ChildItem -LiteralPath $assets -File -Filter 'SparkAutoLevelRuntime.cs' -Recurse -ErrorAction SilentlyContinue)
if ($cores.Count -ne 1) { throw "Expected exactly ONE installed Universal Generator V2 core. Found $($cores.Count)." }
$generator = Split-Path -Parent (Split-Path -Parent $cores[0].FullName)
foreach ($f in @('Editor\SparkAutoLevelSourceImporter.cs','Editor\SparkAutoRecipeRegistry.cs','Editor\SparkAutoLevelSceneBuilder.cs')) {
    if (-not (Test-Path -LiteralPath (Join-Path $generator $f))) { throw "Universal Generator V2 required; missing $f" }
}
$scriptFiles = @(
    'GeneratorUpgrade\Editor\SparkAutoL106FullLayoutBuilder.cs',
    'GeneratorUpgrade\Runtime\SparkAutoL106LayoutRecord.cs',
    'GeneratorUpgrade\Runtime\SparkAutoL106WorkshopFit.cs',
    'GeneratorUpgrade\Recipes\Level106_SFX14.recipe.json'
)
foreach ($f in $scriptFiles) { if (-not (Test-Path -LiteralPath (Join-Path $PSScriptRoot $f))) { throw "Addon incomplete: $f" } }
$sourcePackage = Join-Path $PSScriptRoot 'SourceAssets\Level106'
$withAssets = Test-Path -LiteralPath (Join-Path $sourcePackage 'Models_FBX\107_L106_SFX14_BarePCB.fbx')
$existing = @(Get-ChildItem -LiteralPath $assets -File -Filter L106_Models_136.csv -Recurse -ErrorAction SilentlyContinue)
if ($existing.Count -gt 1) { throw 'Multiple Level 106 source manifests found. Keep one source folder and retry.' }
if ($existing.Count -eq 1) {
    $sourceRoot = Split-Path -Parent (Split-Path -Parent $existing[0].FullName)
    Write-Host "Reusing installed Level 106 source: $sourceRoot"
} else {
    if (-not $withAssets) { throw 'No Level 106 source assets installed. Use the FULL Level 106 package rather than upgrade-only.' }
    $sourceRoot = Join-Path $assets 'ProjectSpark\Level106'
    Write-Host "Creating Level 106 source folder: $sourceRoot"
}
$addons = @('SparkAutoL106FullLayoutBuilder.cs','SparkAutoL106LayoutRecord.cs','SparkAutoL106WorkshopFit.cs')
foreach ($name in $addons) {
    $hits = @(Get-ChildItem -LiteralPath $assets -File -Filter $name -Recurse -ErrorAction SilentlyContinue)
    if ($hits.Count -gt 1) { throw "Duplicate $name scripts. Resolve duplicates before upgrading." }
    if ($hits.Count -eq 1 -and -not ($hits[0].FullName.StartsWith($generator + [System.IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase))) {
        throw "Existing $name outside installed generator: $($hits[0].FullName)"
    }
}
$recipeHits = @(Get-ChildItem -LiteralPath $assets -File -Filter 'Level106_SFX14.recipe.json' -Recurse -ErrorAction SilentlyContinue)
if ($recipeHits.Count -gt 1) { throw 'Duplicate Level 106 recipes detected. Keep one.' }
$editor = Join-Path $generator 'Editor'
$runtime = Join-Path $generator 'Runtime'
$recipes = Join-Path $generator 'Recipes'
foreach ($dir in @($editor,$runtime,$recipes,$sourceRoot)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
function Copy-ReplaceWithBackup([string]$from,[string]$to) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $to) | Out-Null
    if (Test-Path -LiteralPath $to) {
        Copy-Item -LiteralPath $to -Destination ($to + '.before_l106_upgrade_' + $stamp + '.bak') -Force
    }
    Copy-Item -LiteralPath $from -Destination $to -Force
    Write-Host "Installed: $to"
}
Copy-ReplaceWithBackup (Join-Path $PSScriptRoot $scriptFiles[0]) (Join-Path $editor 'SparkAutoL106FullLayoutBuilder.cs')
Copy-ReplaceWithBackup (Join-Path $PSScriptRoot $scriptFiles[1]) (Join-Path $runtime 'SparkAutoL106LayoutRecord.cs')
Copy-ReplaceWithBackup (Join-Path $PSScriptRoot $scriptFiles[2]) (Join-Path $runtime 'SparkAutoL106WorkshopFit.cs')
if ($withAssets) {
    $copied = 0; $preserved = 0
    Get-ChildItem -LiteralPath $sourcePackage -File -Recurse | ForEach-Object {
        $relative = $_.FullName.Substring($sourcePackage.Length).TrimStart([char]'\',[char]'/')
        $dest = Join-Path $sourceRoot $relative
        if (Test-Path -LiteralPath $dest) { $script:preserved++; return }
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $dest
        $script:copied++
    }
    Write-Host "Copied source asset files: $copied; preserved existing files: $preserved"
}
# Always install renderable OBJ fallback, even when using existing Level 106 FBX source.
$fallback = Join-Path $PSScriptRoot 'GeneratorUpgrade\Models_OBJ_Fallback'
$objs = @(Get-ChildItem -LiteralPath $fallback -File -Filter '*.obj')
if ($objs.Count -ne 137) { throw "Missing OBJ fallback models: expected 137, got $($objs.Count)." }
$destMesh = Join-Path $sourceRoot 'Models_OBJ_Fallback'
New-Item -ItemType Directory -Force -Path $destMesh | Out-Null
$added = 0; $kept = 0
Get-ChildItem -LiteralPath $fallback -File | ForEach-Object {
    $target = Join-Path $destMesh $_.Name
    if (Test-Path -LiteralPath $target) { $script:kept++; return }
    Copy-Item -LiteralPath $_.FullName -Destination $target
    $script:added++
}
Write-Host "OBJ fallbacks: $added added, $kept already present."
$norm = Join-Path $PSScriptRoot 'GeneratorUpgrade\NormalizedData'
foreach ($f in @('L106_AutoGen_Models_136.csv','L106_AutoGen_Routes_140.csv',
    'L106_AutoGen_Vias_42.csv','L106_AutoGen_Objectives_6.json',
    'L106_AutoGen_Faults_3.csv','L106_AutoGen_W141_Safety.json')) {
    Copy-ReplaceWithBackup (Join-Path $norm $f) (Join-Path $sourceRoot ('Data\' + $f))
}
if ($recipeHits.Count -eq 1) { $recipePath = $recipeHits[0].FullName }
else { $recipePath = Join-Path $recipes 'Level106_SFX14.recipe.json' }
$recipe = Get-Content -LiteralPath (Join-Path $PSScriptRoot $scriptFiles[3]) -Raw | ConvertFrom-Json
$recipe.sourceAssetsFolder = 'Assets/' + $sourceRoot.Substring($assets.Length).TrimStart([char]'\',[char]'/').Replace('\','/')
if (Test-Path -LiteralPath $recipePath) {
    Copy-Item -LiteralPath $recipePath -Destination ($recipePath + '.before_l106_upgrade_' + $stamp + '.bak') -Force
}
[System.IO.File]::WriteAllText($recipePath, ($recipe | ConvertTo-Json -Depth 15), (New-Object System.Text.UTF8Encoding($false)))
Write-Host "Installed recipe: $recipePath"
Write-Host 'COMPLETE. Reopen Unity and let FBX/OBJ imports and scripts compile.'
Write-Host 'Generate: Tools > Project Spark > Automatic Level Generator > 106 > 1. Build FULL Level_106_Layout'
Write-Host 'Place: Select Table_top; Tools > Project Spark > Automatic Level Generator > 106 > 2. Place Level 106 on SELECTED tabletop'
Write-Warning 'Electrical data unapproved: 140 routes, 42 vias, W141 pins; U14 absent from source BOM. Do not certify gameplay.'
