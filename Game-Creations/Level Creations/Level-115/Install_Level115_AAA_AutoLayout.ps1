[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$UnityProjectPath
)

$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath $UnityProjectPath).Path.TrimEnd([char[]]@('\','/'))
$assets = Join-Path $project 'Assets'
$settings = Join-Path $project 'ProjectSettings\ProjectVersion.txt'
$source = Join-Path $PSScriptRoot 'Level115_SHX23'
$target = Join-Path $assets 'ProjectSpark\Level115\Level115_SHX23'

Write-Host 'Project Spark | Level 115 - The Secure Enclave (SHX-23)' -ForegroundColor Cyan
if (-not (Test-Path -LiteralPath $settings -PathType Leaf) -or
    -not (Test-Path -LiteralPath $assets -PathType Container)) {
    throw 'The selected path is not a Unity project root (Assets and ProjectSettings/ProjectVersion.txt required).'
}
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    throw 'Extract the COMPLETE Level 115 ZIP. The installer must remain next to the Level115_SHX23 folder.'
}
$required = @(
    'Data\ModelManifest.csv',
    'Data\ComponentBOM.csv',
    'Data\ComponentPlacements.csv',
    'Data\CopperRoutes_272_REQUIRES_SCHEMATIC.csv',
    'Data\ViaTasks_92_REQUIRES_SCHEMATIC.csv',
    'Models_OBJ\197_L115_PCB_Main.obj',
    'Models_OBJ\241_L115_AssembledReference.obj',
    'Models_GLB\197_L115_PCB_Main.glb',
    'Unity\Editor\SparkL115FullLayoutBuilder.cs',
    'Unity\Editor\SparkL115URPPBRMaterialBuilder.cs',
    'Unity\Runtime\SparkL115LayoutRecords.cs'
)
foreach ($relative in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $source $relative) -PathType Leaf)) {
        throw "The extracted package is incomplete: $relative"
    }
}
$manifest = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ModelManifest.csv'))
$bom = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ComponentBOM.csv'))
$routes = @(Import-Csv -LiteralPath (Join-Path $source 'Data\CopperRoutes_272_REQUIRES_SCHEMATIC.csv'))
$vias = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ViaTasks_92_REQUIRES_SCHEMATIC.csv'))
if ($manifest.Count -ne 240 -or $bom.Count -ne 196 -or $routes.Count -ne 272 -or $vias.Count -ne 92) {
    throw "Source manifest validation failed. Models=$($manifest.Count), BOM=$($bom.Count), routes=$($routes.Count), vias=$($vias.Count)."
}
$modularOBJ = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_OBJ') -Filter '*_L115_*.obj' -File)
if ($modularOBJ.Count -ne 241) { throw "Expected 240 modular OBJ plus one assembled reference, found $($modularOBJ.Count)." }

# Reject competing Level115 code in a different source root to avoid CS0101 / ambiguous menus.
$duplicateScripts = @(Get-ChildItem -LiteralPath $assets -Recurse -File -Filter 'SparkL115FullLayoutBuilder.cs' -ErrorAction SilentlyContinue)
foreach ($scriptFile in $duplicateScripts) {
    $intended = Join-Path $target 'Unity\Editor\SparkL115FullLayoutBuilder.cs'
    if (-not [string]::Equals($scriptFile.FullName, $intended, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Duplicate Level 115 builder detected: $($scriptFile.FullName). Remove/resolve duplicates manually before installing."
    }
}

$backupDir = Join-Path $project (Join-Path 'ProjectSpark_Level115_Backups' (Get-Date -Format 'yyyyMMdd_HHmmss'))
$changed = 0; $added = 0; $unchanged = 0; $preserved = 0
$sourceFull = (Resolve-Path -LiteralPath $source).Path.TrimEnd([char[]]@('\','/'))
$sourceFiles = @(Get-ChildItem -LiteralPath $source -Recurse -File)
foreach ($src in $sourceFiles) {
    $relative = $src.FullName.Substring($sourceFull.Length).TrimStart([char[]]@('\','/'))
    $dest = Join-Path $target $relative
    $destDir = Split-Path -Parent $dest
    if (-not (Test-Path -LiteralPath $destDir -PathType Container)) {
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }
    if (-not (Test-Path -LiteralPath $dest -PathType Leaf)) {
        Copy-Item -LiteralPath $src.FullName -Destination $dest
        $added++
        continue
    }
    if ((Get-FileHash -LiteralPath $src.FullName -Algorithm SHA256).Hash -eq
        (Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash) {
        $unchanged++
        continue
    }
    if ($relative -like 'Unity\*' -and $relative.EndsWith('.cs', [StringComparison]::OrdinalIgnoreCase)) {
        $backupFile = Join-Path $backupDir $relative
        $backupParent = Split-Path -Parent $backupFile
        New-Item -ItemType Directory -Path $backupParent -Force | Out-Null
        Copy-Item -LiteralPath $dest -Destination $backupFile -Force
        Copy-Item -LiteralPath $src.FullName -Destination $dest -Force
        $changed++
    }
    else {
        Write-Warning "Existing source asset preserved (different checksum): $relative"
        $preserved++
    }
}
Write-Host ("Installed Level 115 source files: added {0}, updated code {1}, unchanged {2}, preserved existing art/data {3}." -f $added,$changed,$unchanged,$preserved)
if ($changed -gt 0) { Write-Host "Previous generated code backups saved outside Assets: $backupDir" }
Write-Host "Unity source directory: $target"
Write-Host 'Open Unity 6.3 URP and run:' -ForegroundColor Green
Write-Host '  Tools > Project Spark > Automatic Level Generator > 115 > 3. Validate Sources and Report Blockers'
Write-Host '  Tools > Project Spark > Automatic Level Generator > 115 > 4. Rebuild URP PBR Materials'
Write-Host '  Tools > Project Spark > Automatic Level Generator > 115 > 1. Build FULL Level_115_Layout'
Write-Host '  Select Table_top -> 115 > 2. Place assembled VISUAL preview on SELECTED tabletop'
Write-Warning 'The installer does NOT approve nets or pins. W001-W272, V01-V92 and W273 require an approved schematic. Objectives remain NOT CERTIFIABLE.'
