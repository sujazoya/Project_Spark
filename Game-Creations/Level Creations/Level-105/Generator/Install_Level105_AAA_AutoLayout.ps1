# Project Spark Level 105 ENX-13 + ECX-4 Full AutoLayout upgrade.
# Non-destructive source installer. Run from extracted bundle while Unity is CLOSED.
param([Parameter(Mandatory = $true)][string]$UnityProjectPath)
$ErrorActionPreference = 'Stop'

$project = (Resolve-Path -LiteralPath $UnityProjectPath).Path.TrimEnd('\','/')
$assets = Join-Path $project 'Assets'
$version = Join-Path $project 'ProjectSettings/ProjectVersion.txt'
if (!(Test-Path -LiteralPath $assets -PathType Container) -or !(Test-Path -LiteralPath $version -PathType Leaf)) {
    throw 'Invalid Unity project path: expected Assets/ and ProjectSettings/ProjectVersion.txt.'
}

$upgrade = Join-Path $PSScriptRoot 'GeneratorUpgrade'
$package = Join-Path $PSScriptRoot 'SourceAssets/Level105'
foreach ($path in @(
    (Join-Path $upgrade 'Editor/SparkAutoL105FullLayoutBuilder.cs'),
    (Join-Path $upgrade 'Editor/SparkAutoL105PBRMaterials.cs'),
    (Join-Path $upgrade 'Editor/SparkAutoL105SourceValidator.cs'),
    (Join-Path $upgrade 'Runtime/SparkAutoL105LayoutRecord.cs'),
    (Join-Path $upgrade 'Runtime/SparkAutoL105WorkshopFit.cs'),
    (Join-Path $upgrade 'Recipes/Level105_ENX13.recipe.json'),
    (Join-Path $package 'Data/L105_AutoGen_Models_128.csv'),
    (Join-Path $package 'Models_OBJ_Fallback/101_L105_PCB_ENX13_270x190mm.obj'),
    (Join-Path $package 'Models_OBJ_Fallback/110_L105_ECX4_RemovableCard.obj'),
    (Join-Path $package 'Models_OBJ_Fallback/129_L105_AssembledMainboardReference.obj'),
    (Join-Path $package 'Textures/Unity_Packed/pcb_URP_MetallicSmoothness.png')
)) { if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Incomplete package: $path" } }

$runtimeHits = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter 'SparkAutoLevelRuntime.cs')
if ($runtimeHits.Count -ne 1) { throw "One existing Universal AutoLevelGenerator V2 required; found $($runtimeHits.Count). Install V2 separately before Level 105." }
$generator = Split-Path -Parent (Split-Path -Parent $runtimeHits[0].FullName)
$editor = Join-Path $generator 'Editor'
$runtime = Join-Path $generator 'Runtime'
$recipes = Join-Path $generator 'Recipes'
foreach ($name in @('SparkAutoL105FullLayoutBuilder.cs','SparkAutoL105PBRMaterials.cs','SparkAutoL105SourceValidator.cs','SparkAutoL105LayoutRecord.cs','SparkAutoL105WorkshopFit.cs')) {
    $hits = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter $name)
    if ($hits.Count -gt 1) { throw "Duplicate $name detected. Resolve before installing." }
    if ($hits.Count -eq 1) {
        $desired = if ($name -like '*Builder.cs' -or $name -like '*PBRMaterials.cs' -or $name -like '*Validator.cs') { Join-Path $editor $name } else { Join-Path $runtime $name }
        if ($hits[0].FullName -ne $desired) { throw "$name exists in another folder: $($hits[0].FullName)" }
    }
}

$manifestHits = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter 'ModelManifest.csv' | Where-Object {
    $_.FullName -match '(?i)Level105|ENX13|ExpansionNexus'
})
if ($manifestHits.Count -gt 1) { throw 'Multiple Level105 ModelManifest.csv files found. Remove duplicates before installing.' }
$sourceRoot = if ($manifestHits.Count -eq 1) {
    Split-Path -Parent (Split-Path -Parent $manifestHits[0].FullName)
} else { Join-Path $assets 'ProjectSpark/Level105' }

$recipeHits = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter 'Level105_ENX13.recipe.json')
if ($recipeHits.Count -gt 1) { throw 'Multiple Level 105 recipes found. Keep one.' }
$recipePath = if ($recipeHits.Count -eq 1) { $recipeHits[0].FullName } else { Join-Path $recipes 'Level105_ENX13.recipe.json' }
$backupDir = Join-Path $project ('ProjectSpark_Level105_Backups\' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
$script:backups = 0
function Backup-Old([string]$file) {
    if (!(Test-Path -LiteralPath $file -PathType Leaf)) { return }
    $rel = $file.Substring($project.Length).TrimStart([char]'\',[char]'/')
    $dest = Join-Path $backupDir $rel
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
    Copy-Item -LiteralPath $file -Destination $dest -Force
    $script:backups++
}
function Copy-Updated([string]$from, [string]$to) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $to) | Out-Null
    Backup-Old $to
    Copy-Item -LiteralPath $from -Destination $to -Force
    Write-Host "Updated: $to"
}

# Never replace source FBX/GLB/texture files; keep existing GUIDs and artist edits.
$script:copied = 0
$script:preserved = 0
Get-ChildItem -LiteralPath $package -Recurse -File | ForEach-Object {
    $rel = $_.FullName.Substring($package.Length).TrimStart([char]'\',[char]'/')
    $dest = Join-Path $sourceRoot $rel
    if (Test-Path -LiteralPath $dest -PathType Leaf) { $script:preserved++ }
    else {
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $dest
        $script:copied++
    }
}
foreach ($name in @('SparkAutoL105FullLayoutBuilder.cs','SparkAutoL105PBRMaterials.cs','SparkAutoL105SourceValidator.cs')) {
    Copy-Updated (Join-Path $upgrade "Editor/$name") (Join-Path $editor $name)
}
foreach ($name in @('SparkAutoL105LayoutRecord.cs','SparkAutoL105WorkshopFit.cs')) {
    Copy-Updated (Join-Path $upgrade "Runtime/$name") (Join-Path $runtime $name)
}
# Some older Level105 installs contain a malformed C# escaped character in the legacy
# material utility. Repair ONLY if the legacy source is already present in the user's
# Assets; otherwise the original file remains a .txt reference and is not compiled.
$legacyPath = Join-Path $sourceRoot 'Unity/Editor/SparkL105MaterialBuilder.cs'
if (Test-Path -LiteralPath $legacyPath -PathType Leaf) {
    $legacyText = Get-Content -LiteralPath $legacyPath -Raw
    if ($legacyText.Contains("Replace('\','/')")) {
        Copy-Updated (Join-Path $upgrade 'LegacyFix/SparkL105MaterialBuilder.cs') $legacyPath
        Write-Host 'Repaired existing legacy Level105 material-builder C# escape error.'
    }
}
$recipe = Get-Content -LiteralPath (Join-Path $upgrade 'Recipes/Level105_ENX13.recipe.json') -Raw | ConvertFrom-Json
$recipe.sourceAssetsFolder = 'Assets' + $sourceRoot.Substring($assets.Length).Replace('\','/')
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $recipePath) | Out-Null
Backup-Old $recipePath
$recipe | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $recipePath -Encoding UTF8

$fbx = @(Get-ChildItem -LiteralPath (Join-Path $sourceRoot 'Models_FBX') -File -Filter '*.fbx' -ErrorAction SilentlyContinue)
$glb = @(Get-ChildItem -LiteralPath (Join-Path $sourceRoot 'Models_GLB') -File -Filter '*.glb' -ErrorAction SilentlyContinue)
$obj = @(Get-ChildItem -LiteralPath (Join-Path $sourceRoot 'Models_OBJ_Fallback') -File -Filter '*.obj' -ErrorAction SilentlyContinue)
Write-Host ''
Write-Host 'LEVEL 105 FULL LAYOUT UPDATE INSTALLED'
Write-Host "Project: $project"
Write-Host "Source: $sourceRoot"
Write-Host "Recipe: $recipePath"
Write-Host "FBX count: $($fbx.Count) | GLB count: $($glb.Count) | renderable OBJ fallback count: $($obj.Count)"
Write-Host "New files: $copied | preserved source files: $preserved | backups: $backups"
Write-Host "Backups outside Assets: $backupDir"
if ($fbx.Count -lt 129 -or $glb.Count -lt 129) {
    Write-Warning 'Original FBX/GLB source is incomplete. Use the COMPLETE Level105 bundle to restore original art. OBJ fallbacks alone are not your final source assets.'
}
Write-Warning 'All 132 routes, 38 vias, pad-to-pin assignments and W133 physical pins remain UNVERIFIED / NOT CERTIFIABLE.'
Write-Host 'Reopen Unity. After compilation: Tools > Project Spark > Automatic Level Generator > 105 > 1. Build FULL Level_105_Layout'
Write-Host 'Then select Table_top and choose: 105 > 2. Place Level 105 on SELECTED tabletop.'
