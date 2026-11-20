param(
    [Parameter(Mandatory = $true)]
    [string]$UnityProjectPath
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath $UnityProjectPath).Path
$assets = Join-Path $project 'Assets'
if (-not (Test-Path -LiteralPath $assets -PathType Container)) {
    throw "Unity project Assets folder not found: $assets"
}
$found = @(Get-ChildItem -LiteralPath $assets -Filter 'SparkAutoLevelSceneBuilder.cs' -File -Recurse)
if ($found.Count -ne 1) {
    throw "Expected exactly one existing SparkAutoLevelSceneBuilder.cs, found $($found.Count). Keep only one generator installation."
}
$editorDir = $found[0].Directory.FullName
if ($found[0].Directory.Name -ne 'Editor') {
    throw "Existing SceneBuilder is not inside an Editor folder: $editorDir"
}
$base = (Get-Item $editorDir).Parent.FullName
$runtimeDir = Join-Path $base 'Runtime'
if (-not (Test-Path -LiteralPath $runtimeDir)) {
    throw "Existing generator Runtime folder not found: $runtimeDir"
}
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$copyPairs = @(
    @{ From = (Join-Path $scriptRoot 'Runtime\SparkAutoFullLayoutRecord.cs'); To = (Join-Path $runtimeDir 'SparkAutoFullLayoutRecord.cs') },
    @{ From = (Join-Path $scriptRoot 'Editor\SparkAutoFullLevelLayoutBuilder.cs'); To = (Join-Path $editorDir 'SparkAutoFullLevelLayoutBuilder.cs') }
)
Write-Host "Using existing generator: $base" -ForegroundColor Cyan
$backupDir = Join-Path $base ('FullLayoutUpgradeBackup_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
New-Item -Path $backupDir -ItemType Directory -Force | Out-Null
foreach ($pair in $copyPairs) {
    if (-not (Test-Path -LiteralPath $pair.From -PathType Leaf)) {
        throw "Upgrade file not found: $($pair.From)"
    }
    if (Test-Path -LiteralPath $pair.To -PathType Leaf) {
        Copy-Item -LiteralPath $pair.To -Destination $backupDir -Force
    }
    Copy-Item -LiteralPath $pair.From -Destination $pair.To -Force
    Write-Host "Installed: $($pair.To)" -ForegroundColor Green
}
Write-Host "No existing SceneBuilder was replaced; no second generator was installed." -ForegroundColor Green
Write-Host "Reopen Unity, then select Tools > Project Spark > Automatic Level Generator > Build FULL Level 110 Layout."
