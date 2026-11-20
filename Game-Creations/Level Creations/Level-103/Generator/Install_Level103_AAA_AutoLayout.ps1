param(
    [Parameter(Mandatory = $true)]
    [string]$UnityProjectPath
)
$ErrorActionPreference = 'Stop'

function Copy-Safely([string]$Source, [string]$Destination, [bool]$Replace) {
    $parent = Split-Path -Parent $Destination
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    if (Test-Path -LiteralPath $Destination) {
        if (-not $Replace) { $script:Skipped++; return }
        $relative = $Destination.Substring($script:AssetsRoot.Length).TrimStart('\','/')
        $backup = Join-Path $script:BackupRoot $relative
        $backupParent = Split-Path -Parent $backup
        New-Item -ItemType Directory -Path $backupParent -Force | Out-Null
        Copy-Item -LiteralPath $Destination -Destination $backup -Force
        $script:BackedUp++
    }
    Copy-Item -LiteralPath $Source -Destination $Destination -Force
    $script:Copied++
}

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$script:AssetsRoot = Join-Path $project 'Assets'
if (-not (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt'))) {
    throw "Not a Unity project: $project (missing ProjectSettings\ProjectVersion.txt)"
}
if (-not (Test-Path -LiteralPath $script:AssetsRoot -PathType Container)) {
    throw "Assets directory missing: $script:AssetsRoot"
}
$packageRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceArt = Join-Path $packageRoot 'SourceAssets\Level103_CPX11'
$script:BackupRoot = Join-Path $project ('ProjectSpark_Level103_UpgradeBackups\' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$script:Copied = 0; $script:Skipped = 0; $script:BackedUp = 0

$matches = @(Get-ChildItem -LiteralPath $script:AssetsRoot -Recurse -File -Filter 'ModelManifest.csv' -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -match '(?i)[\\/]Data[\\/]ModelManifest\.csv$' -and $_.FullName -match '(?i)(Level103|L103|CPX11)' })
if ($matches.Count -gt 1) {
    throw "Multiple Level 103 ModelManifest.csv roots found. Remove duplicate installed copies first:`n$($matches.FullName -join "`n")"
}
if ($matches.Count -eq 1) {
    $targetRoot = Split-Path -Parent (Split-Path -Parent $matches[0].FullName)
    Write-Host "Existing Level 103 source identified: $targetRoot"
} else {
    if (-not (Test-Path -LiteralPath (Join-Path $sourceArt 'Data\ModelManifest.csv'))) {
        throw 'Upgrade-only package requires original Level103_CPX11 installed in Unity Assets. Use the COMPLETE ZIP.'
    }
    $targetRoot = Join-Path $script:AssetsRoot 'ProjectSpark\Level103\Level103_CPX11'
    Write-Host "Installing original Level 103 source: $targetRoot"
}

# Detect duplicate generator compilation units BEFORE writing. Other levels stay untouched.
foreach ($name in @('SparkL103FullLayoutBuilder.cs','SparkL103PBRMaterialBuilder.cs','SparkL103LayoutRecords.cs')) {
    $intendedSubfolder = if ($name -eq 'SparkL103LayoutRecords.cs') { 'Runtime' } else { 'Editor' }
    $intended = Join-Path $targetRoot ('Unity\' + $intendedSubfolder + '\' + $name)
    $elsewhere = @(Get-ChildItem -LiteralPath $script:AssetsRoot -Recurse -File -Filter $name -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -ne $intended })
    if ($elsewhere.Count -gt 0) {
        throw "Duplicate compiled Level 103 generator script $name outside the intended root. Move it before continuing:`n$($elsewhere.FullName -join "`n")"
    }
}
foreach ($folder in @('Models_OBJ_Fallback','URP_Packed','GeneratorUpgrade\Unity\Editor','GeneratorUpgrade\Unity\Runtime','GeneratorUpgrade\Docs')) {
    if (-not (Test-Path -LiteralPath (Join-Path $packageRoot $folder))) {
        throw "Missing upgrade folder: $folder. Extract the ZIP completely before running."
    }
}

# Original FBX/GLB/textures/CSVs/legacy art tools are retained, not overwritten.
if (Test-Path -LiteralPath $sourceArt) {
    foreach ($file in Get-ChildItem -LiteralPath $sourceArt -Recurse -File) {
        $relative = $file.FullName.Substring($sourceArt.Length).TrimStart('\','/')
        Copy-Safely -Source $file.FullName -Destination (Join-Path $targetRoot $relative) -Replace $false
    }
}
foreach ($folder in @('Models_OBJ_Fallback','URP_Packed')) {
    $from = Join-Path $packageRoot $folder
    foreach ($file in Get-ChildItem -LiteralPath $from -File) {
        Copy-Safely -Source $file.FullName -Destination (Join-Path (Join-Path $targetRoot $folder) $file.Name) -Replace $true
    }
}
foreach ($subfolder in @('Editor','Runtime')) {
    $from = Join-Path $packageRoot ('GeneratorUpgrade\Unity\' + $subfolder)
    $to = Join-Path $targetRoot ('Unity\' + $subfolder)
    foreach ($file in Get-ChildItem -LiteralPath $from -File -Filter '*.cs') {
        Copy-Safely -Source $file.FullName -Destination (Join-Path $to $file.Name) -Replace $true
    }
}
$docs = Join-Path $targetRoot 'Documentation\AAA_AutoLayout_Upgrade'
foreach ($file in Get-ChildItem -LiteralPath (Join-Path $packageRoot 'GeneratorUpgrade\Docs') -File) {
    Copy-Safely -Source $file.FullName -Destination (Join-Path $docs $file.Name) -Replace $true
}

$required = @(
    'Data\ModelManifest.csv',
    'Data\ComponentBOM.csv',
    'Data\ComponentPlacements.csv',
    'Models_FBX\089_L103_PCB_Main.fbx',
    'Models_GLB\089_L103_PCB_Main.glb',
    'Models_OBJ_Fallback\089_L103_PCB_Main.obj',
    'Models_OBJ_Fallback\L103_CPX11_Assembled_REFERENCE_ONLY.obj',
    'URP_Packed\PCB_Navy_URP_MetallicSmoothness.png',
    'Unity\Editor\SparkL103FullLayoutBuilder.cs',
    'Unity\Editor\SparkL103PBRMaterialBuilder.cs',
    'Unity\Runtime\SparkL103LayoutRecords.cs'
)
foreach ($relative in $required) {
    $p = Join-Path $targetRoot $relative
    if (-not (Test-Path -LiteralPath $p)) { throw "Post-install required asset not found: $p" }
}
$objectCount = @(Get-ChildItem -LiteralPath (Join-Path $targetRoot 'Models_OBJ_Fallback') -File -Filter '*.obj').Count
if ($objectCount -ne 113) { throw "Expected 113 Unity OBJ mesh fallbacks; found $objectCount" }
$packedCount = @(Get-ChildItem -LiteralPath (Join-Path $targetRoot 'URP_Packed') -File -Filter '*.png').Count
if ($packedCount -ne 23) { throw "Expected 23 URP metallic/smoothness textures; found $packedCount" }

Write-Host ''
Write-Host 'LEVEL 103 / CPX-11 / THE POWER CORE - UPGRADE INSTALLED'
Write-Host "New/replaced: $script:Copied; existing originals preserved: $script:Skipped; backed up: $script:BackedUp"
Write-Host "Unity source root: $targetRoot"
Write-Host "Backups (if any): $script:BackupRoot"
Write-Host ''
Write-Host 'Open Unity 6.3 and wait for FBX/OBJ/texture imports and script compilation.'
Write-Host '1. Tools > Project Spark > Automatic Level Generator > 103 > 3. Validate Source and 3D Models'
Write-Host '2. Tools > Project Spark > Automatic Level Generator > 103 > 1. Build FULL Level_103_Layout'
Write-Host '3. Select Table_top > 103 > 2. Place Level 103 on SELECTED tabletop'
Write-Host '4. Optional: 103 > 4. Rebuild URP PBR Materials'
Write-Host 'SECURITY / GAMEPLAY: 116 routes, 30 vias and W117 pin map are UNVERIFIED. No VRM/CPU gameplay or 6/6 certification.'
