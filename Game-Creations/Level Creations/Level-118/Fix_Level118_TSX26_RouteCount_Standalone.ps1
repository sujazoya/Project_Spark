# Project Spark - Level 118 TSX26 Route Count Validation Hotfix
# Target: Assets/ProjectSpark/Level118/TSX26/Unity/Editor/SparkL118FullLayoutBuilder.cs
# Verified from original Level 118 TSX26 source ZIP: 320 routes, not 340.
# Windows PowerShell 5.1 compatible. No companion files required.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$UnityProjectPath,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$assets = Join-Path $project 'Assets'
$version = Join-Path $project 'ProjectSettings\ProjectVersion.txt'
if (-not (Test-Path -LiteralPath $assets -PathType Container) -or
    -not (Test-Path -LiteralPath $version -PathType Leaf)) {
    throw "Invalid Unity project. Missing Assets or ProjectVersion.txt: $project"
}

$root = Join-Path $assets 'ProjectSpark\Level118\TSX26'
$builder = Join-Path $root 'Unity\Editor\SparkL118FullLayoutBuilder.cs'
$manifest = Join-Path $root 'Data\ModelManifest.csv'
$bom = Join-Path $root 'Data\ComponentBOM.csv'
$placements = Join-Path $root 'Data\ComponentPlacements.csv'
$routes = Join-Path $root 'Data\CopperRoutes_320_REQUIRES_SCHEMATIC.csv'
$vias = Join-Path $root 'Data\ViaTasks_116_REQUIRES_SCHEMATIC.csv'

foreach ($file in @($builder, $manifest, $bom, $placements, $routes, $vias)) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw "Missing installed Level 118 source file: $file. Extract/install the original kit first. No files changed."
    }
}

$modelsData = @(Import-Csv -LiteralPath $manifest)
$bomData = @(Import-Csv -LiteralPath $bom)
$placementsData = @(Import-Csv -LiteralPath $placements)
$routesData = @(Import-Csv -LiteralPath $routes)
$viasData = @(Import-Csv -LiteralPath $vias)
Write-Host "[L118] Installed records: models=$($modelsData.Count), BOM=$($bomData.Count), placements=$($placementsData.Count), routes=$($routesData.Count), vias=$($viasData.Count)"
if ($modelsData.Count -ne 288 -or $bomData.Count -ne 236 -or
    $placementsData.Count -ne 236 -or $routesData.Count -ne 320 -or
    $viasData.Count -ne 116) {
    throw '[L118] CSV record counts differ from the verified original kit. No files changed.'
}

$source = [System.IO.File]::ReadAllText($builder)
if (-not $source.Contains('class SparkL118FullLayoutBuilder') -or
    -not $source.Contains('Wrong row counts')) {
    throw '[L118] Unexpected builder content; refusing to patch. No files changed.'
}

# Match the precise conditional comparison, not the 0.340-m board dimension.
$bad = 'routes\.Count\s*!=\s*340\s*\|\|\s*viaRows\.Count\s*!=\s*116'
$good = 'routes.Count != 320 || viaRows.Count != 116'
if ([regex]::IsMatch($source, $bad)) {
    $hits = [regex]::Matches($source, $bad).Count
    if ($hits -ne 1) {
        throw "[L118] Expected one validation typo, found $hits. No files changed."
    }
    $fixed = [regex]::Replace($source, $bad, $good)
    if (-not $fixed.Contains($good) -or $fixed -eq $source) {
        throw '[L118] Patch self-check failed. No files changed.'
    }
    if ($DryRun) {
        Write-Host "[L118][DRY RUN] Would back up and patch: $builder" -ForegroundColor Yellow
        Write-Host '[L118][DRY RUN] Change: routes.Count != 340  ->  routes.Count != 320' -ForegroundColor Yellow
        exit 0
    }
    $backupDir = Join-Path $project ('ProjectSpark_Backups\Level118_TSX26_RouteCount_' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
    New-Item -Path $backupDir -ItemType Directory -Force | Out-Null
    $backup = Join-Path $backupDir 'SparkL118FullLayoutBuilder.cs'
    Copy-Item -LiteralPath $builder -Destination $backup -ErrorAction Stop
    $bytes = [System.IO.File]::ReadAllBytes($builder)
    $hadBom = $bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191
    try {
        $encoding = [System.Text.UTF8Encoding]::new($hadBom)
        [System.IO.File]::WriteAllText($builder, $fixed, $encoding)
        $installed = [System.IO.File]::ReadAllText($builder)
        if ($installed -ne $fixed -or [regex]::IsMatch($installed, $bad)) {
            throw '[L118] Write verification failed.'
        }
    }
    catch {
        Copy-Item -LiteralPath $backup -Destination $builder -Force
        throw "[L118] Failed to install patch; original restored. Details: $_"
    }
    Write-Host '[L118] SUCCESS: corrected route-count validation 340 -> 320.' -ForegroundColor Green
    Write-Host "[L118] Backup: $backup"
}
elseif ($source.Contains($good)) {
    Write-Host '[L118] Already fixed: routes.Count != 320. No changes needed.' -ForegroundColor Green
}
else {
    throw '[L118] Builder has an unrecognized validation condition. No files changed.'
}

Write-Host '[L118] Data, models, scenes, .meta files, and other levels were not modified.'
Write-Host '[L118] Reopen Unity: Tools > Project Spark > Automatic Level Generator > 118 > 3. Validate Sources and Report Blockers.'
Write-Host '[L118] After validation passes, run 1. Build FULL Level_118_Layout.'
