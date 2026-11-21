[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$UnityProjectPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$projectSettings = Join-Path $project 'ProjectSettings\ProjectVersion.txt'
$assets = Join-Path $project 'Assets'
if (-not (Test-Path -LiteralPath $projectSettings -PathType Leaf) -or
    -not (Test-Path -LiteralPath $assets -PathType Container)) {
    throw "Not a Unity project: $project (missing Assets or ProjectSettings/ProjectVersion.txt)"
}

$source = Join-Path $PSScriptRoot 'ProjectSpark_Level116_NVX24'
$mode = 'Full'
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    $source = Join-Path $PSScriptRoot 'GeneratorUpgrade'
    $mode = 'UpgradeOnly'
}
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    throw 'Extract the COMPLETE or UPGRADE-ONLY Level116 ZIP. Missing ProjectSpark_Level116_NVX24 / GeneratorUpgrade companion folder.'
}

$destination = Join-Path $assets 'ProjectSpark\Level116\NVX24'
$sourceManifest = Join-Path $source 'Data\ModelManifest.csv'
$sourceBOM = Join-Path $source 'Data\ComponentBOM.csv'
if ($mode -eq 'Full') {
    if (-not (Test-Path -LiteralPath $sourceManifest) -or -not (Test-Path -LiteralPath $sourceBOM)) {
        throw 'Complete Level 116 source manifest or BOM is missing.'
    }
    $modelRows = @(Import-Csv -LiteralPath $sourceManifest)
    $bomRows = @(Import-Csv -LiteralPath $sourceBOM)
    $objs = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_OBJ') -Filter '*.obj' -File)
    $glbs = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_GLB') -Filter '*.glb' -File)
    if ($modelRows.Count -ne 256 -or $bomRows.Count -ne 208 -or $objs.Count -ne 257 -or $glbs.Count -ne 257) {
        throw "Bad source counts: manifest=$($modelRows.Count), BOM=$($bomRows.Count), OBJ=$($objs.Count), GLB=$($glbs.Count). Nothing installed."
    }
} else {
    $installedManifest = Join-Path $destination 'Data\ModelManifest.csv'
    if (-not (Test-Path -LiteralPath $installedManifest -PathType Leaf)) {
        throw "Upgrade-only requires Level 116 full source already installed at $destination. Use the complete kit first."
    }
}

$allFiles = @(Get-ChildItem -LiteralPath $source -File -Recurse)
if ($allFiles.Count -eq 0) { throw 'Upgrade source directory contains no files.' }

$plan = New-Object System.Collections.Generic.List[object]
$skipped = 0
$backupCount = 0
foreach ($file in $allFiles) {
    $relative = $file.FullName.Substring($source.Length).TrimStart([char[]]@('\','/'))
    $relativeUnix = $relative.Replace('\','/')
    if ($mode -eq 'UpgradeOnly' -and -not
        ($relativeUnix.StartsWith('Unity/',[StringComparison]::OrdinalIgnoreCase) -or
         $relativeUnix.StartsWith('Documentation/',[StringComparison]::OrdinalIgnoreCase))) {
        continue
    }
    $target = Join-Path $destination $relative
    $isSafeToUpdate = $relativeUnix.StartsWith('Unity/',[StringComparison]::OrdinalIgnoreCase) -or
                       $relativeUnix.StartsWith('Documentation/',[StringComparison]::OrdinalIgnoreCase)
    if (Test-Path -LiteralPath $target -PathType Leaf) {
        $fromHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        $toHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
        if ($fromHash -eq $toHash) {
            $skipped++
            continue
        }
        if (-not $isSafeToUpdate) {
            throw "Existing original source differs: $target. Refusing to overwrite model/texture/data. Back up and reconcile manually."
        }
        $backupCount++
    }
    $plan.Add([PSCustomObject]@{ Source=$file.FullName; Destination=$target; Relative=$relative; Backup=$isSafeToUpdate -and (Test-Path -LiteralPath $target -PathType Leaf) })
}

$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupRoot = Join-Path $project ("ProjectSpark_L116_BackedUp_" + $stamp)
if ($backupCount -gt 0) { New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null }

$installed = 0
foreach ($item in $plan) {
    if ($item.Backup) {
        $backupFile = Join-Path $backupRoot $item.Relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $backupFile) -Force | Out-Null
        Copy-Item -LiteralPath $item.Destination -Destination $backupFile -Force
    }
    New-Item -ItemType Directory -Path (Split-Path -Parent $item.Destination) -Force | Out-Null
    Copy-Item -LiteralPath $item.Source -Destination $item.Destination -Force
    $installed++
}

Write-Host "[L116] Installed Level 116 $mode source files: $installed; identical skipped: $skipped." -ForegroundColor Green
if ($backupCount -gt 0) { Write-Host "[L116] Backed up $backupCount replaced files OUTSIDE Assets: $backupRoot" }
Write-Host "[L116] Level116 source: $destination" -ForegroundColor Cyan
Write-Host '[L116] Open Unity 6.3 URP and wait for the entire OBJ import/compilation.'
Write-Host '[L116] Tools > Project Spark > Automatic Level Generator > 116 > 3. Validate Sources and Report Blockers'
Write-Host '[L116] Then > 116 > 4. Rebuild URP PBR Materials and > 116 > 1. Build FULL Level_116_Layout'
Write-Host '[L116] Status remains VISUAL ONLY / electrical schematic NOT_CERTIFIABLE.'
