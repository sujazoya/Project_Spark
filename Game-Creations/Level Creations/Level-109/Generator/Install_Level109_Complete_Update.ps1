# Project Spark Level 109 complete update (consolidated source + mesh + tabletop fixes).
# Run with Unity CLOSED. Uses the installed Universal AutoLevelGenerator V2. Does not
# delete or overwrite existing Level_109_Layout prefabs/scenes, gameplay, or other levels.
param(
    [Parameter(Mandatory=$true)][string]$UnityProjectPath
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath $UnityProjectPath).Path.TrimEnd('\','/')
$assets = Join-Path $project 'Assets'
$projectVersion = Join-Path $project 'ProjectSettings/ProjectVersion.txt'
if (-not (Test-Path -LiteralPath $assets -PathType Container) -or
    -not (Test-Path -LiteralPath $projectVersion -PathType Leaf)) {
    throw 'Invalid Unity project root. Expected Assets and ProjectSettings/ProjectVersion.txt.'
}
$sourcePackage = Join-Path $PSScriptRoot 'SourceAssets/Level109'
$upgrade = Join-Path $PSScriptRoot 'GeneratorUpgrade'
foreach ($required in @(
    (Join-Path $upgrade 'Editor/SparkAutoL109FullLayoutBuilder.cs'),
    (Join-Path $upgrade 'Runtime/SparkAutoL109LayoutRecord.cs'),
    (Join-Path $upgrade 'Runtime/SparkAutoL109WorkshopFit.cs'),
    (Join-Path $upgrade 'Recipes/Level109_TRX17.recipe.json'),
    (Join-Path $sourcePackage 'Data/L109_Models_160.csv'),
    (Join-Path $sourcePackage 'Models_OBJ_Fallback/125_L109_TRX17_EmptyPCB.obj'),
    (Join-Path $sourcePackage 'Models_OBJ_Fallback/ProjectSpark_Level109_TRX17_Complete_Assembled_Board.obj')
)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) { throw "Incomplete update: $required" }
}
$runtimeHits = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter 'SparkAutoLevelRuntime.cs')
if ($runtimeHits.Count -ne 1) {
    throw "Install exactly one Universal AutoLevelGenerator V2 before this update (found $($runtimeHits.Count))."
}
$generator = Split-Path -Parent (Split-Path -Parent $runtimeHits[0].FullName)
$editorFolder = Join-Path $generator 'Editor'
$runtimeFolder = Join-Path $generator 'Runtime'
$recipeFolder = Join-Path $generator 'Recipes'
# Detect duplicate C# classes across Assets BEFORE editing any existing project files.
foreach ($name in @('SparkAutoL109FullLayoutBuilder.cs','SparkAutoL109LayoutRecord.cs','SparkAutoL109WorkshopFit.cs')) {
    $hits = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter $name)
    if ($hits.Count -gt 1) { throw "Duplicate $name found. Keep only one before upgrading." }
    if ($hits.Count -eq 1) {
        $wanted = if ($name -eq 'SparkAutoL109FullLayoutBuilder.cs') { Join-Path $editorFolder $name } else { Join-Path $runtimeFolder $name }
        if ($hits[0].FullName -ne $wanted) {
            throw "Existing $name is outside the detected generator: $($hits[0].FullName). Move it before updating."
        }
    }
}
$manifests = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter 'L109_Models_160.csv')
if ($manifests.Count -gt 1) { throw "Multiple Level109 sources detected; consolidate them before upgrading." }
if ($manifests.Count -eq 1) {
    $sourceRoot = Split-Path -Parent (Split-Path -Parent $manifests[0].FullName)
} else {
    $sourceRoot = Join-Path $assets 'ProjectSpark/Level109'
}
$recipeHits = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter 'Level109_TRX17.recipe.json')
if ($recipeHits.Count -gt 1) { throw 'Multiple Level 109 recipes detected; keep exactly one.' }
$recipePath = if ($recipeHits.Count -eq 1) { $recipeHits[0].FullName } else { Join-Path $recipeFolder 'Level109_TRX17.recipe.json' }
$backupDir = Join-Path $project ('ProjectSpark_Level109_Backups\' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
$script:backupCount = 0
function Backup-Existing([string]$file) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { return }
    $relative = $file.Substring($project.Length).TrimStart([char]'\',[char]'/')
    $destination = Join-Path $backupDir $relative
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
    Copy-Item -LiteralPath $file -Destination $destination -Force
    $script:backupCount++
}
function Copy-Upgrade([string]$source, [string]$dest) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
    Backup-Existing $dest
    Copy-Item -LiteralPath $source -Destination $dest -Force
    Write-Host "Updated: $dest"
}
# Retain original FBX/GLB art and existing Unity meta references. The imported OBJ
# copies are additional compatibility geometry; do not replace FBX GUIDs or move prefabs.
$script:copied = 0
$script:preserved = 0
Get-ChildItem -LiteralPath $sourcePackage -Recurse -File | ForEach-Object {
    $relative = $_.FullName.Substring($sourcePackage.Length).TrimStart([char]'\',[char]'/')
    $destination = Join-Path $sourceRoot $relative
    if (Test-Path -LiteralPath $destination -PathType Leaf) {
        # Source manifests and safety plans are updated only if absent; user edits are preserved.
        $script:preserved++
    } else {
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $destination
        $script:copied++
    }
}
Copy-Upgrade (Join-Path $upgrade 'Editor/SparkAutoL109FullLayoutBuilder.cs') (Join-Path $editorFolder 'SparkAutoL109FullLayoutBuilder.cs')
Copy-Upgrade (Join-Path $upgrade 'Runtime/SparkAutoL109LayoutRecord.cs') (Join-Path $runtimeFolder 'SparkAutoL109LayoutRecord.cs')
Copy-Upgrade (Join-Path $upgrade 'Runtime/SparkAutoL109WorkshopFit.cs') (Join-Path $runtimeFolder 'SparkAutoL109WorkshopFit.cs')
$recipe = Get-Content -LiteralPath (Join-Path $upgrade 'Recipes/Level109_TRX17.recipe.json') -Raw | ConvertFrom-Json
$relativeSource = 'Assets' + $sourceRoot.Substring($assets.Length).Replace('\','/')
$recipe.sourceAssetsFolder = $relativeSource
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $recipePath) | Out-Null
Backup-Existing $recipePath
$recipe | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $recipePath -Encoding UTF8
Write-Host ''
Write-Host 'LEVEL 109 UPDATE INSTALLED'
Write-Host "Unity project: $project"
Write-Host "Source folder: $sourceRoot"
Write-Host "Recipe: $recipePath"
Write-Host "Added source files: $copied | Preserved source files: $preserved | Backed-up files: $backupCount"
Write-Host "Backup folder (outside Assets): $backupDir"
Write-Host 'Next: reopen Unity; allow OBJ imports to finish; select Tools > Project Spark > Automatic Level Generator > 109 > 1. Build FULL Level_109_Layout.'
Write-Host 'For placement select your Table_top anchor and choose 109 > 2. Place Level 109 on SELECTED tabletop.'
Write-Warning 'Electrical routes, vias, W165 pin mappings and gameplay-system bindings are UNVERIFIED. No objectives can be certified by the art generator.'
