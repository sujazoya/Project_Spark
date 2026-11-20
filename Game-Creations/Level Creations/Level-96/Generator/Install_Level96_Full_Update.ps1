[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$UnityProjectPath,
    [switch]$OverwriteSource
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $UnityProjectPath).Path
if (-not (Test-Path -LiteralPath (Join-Path $root 'Assets') -PathType Container)) {
    throw "Not a Unity project: Assets folder missing in $root"
}
$package = Split-Path -Parent $MyInvocation.MyCommand.Path
$fromAssets = Join-Path $package 'UnityAssets\Level96'
$targetAssets = Join-Path $root 'Assets\ProjectSpark\Level96'
$existingGenerator = Join-Path $root 'Assets\My_Assets\_Project_Spark\Scripts\ProjectSpark\AutoLevelGenerator'
$otherGenerator = Join-Path $root 'Assets\ProjectSpark\AutoLevelGenerator'
if (Test-Path -LiteralPath (Join-Path $existingGenerator 'Editor') -PathType Container) {
    $generator = $existingGenerator
} elseif (Test-Path -LiteralPath (Join-Path $otherGenerator 'Editor') -PathType Container) {
    $generator = $otherGenerator
} else {
    throw "Existing Universal Generator V2 not found. Install V2 first; no duplicate generator was created. Checked: $existingGenerator and $otherGenerator"
}
Write-Host "Using Unity project: $root"
Write-Host "Using existing generator: $generator"
$backup = Join-Path $package ('BACKUP_BEFORE_L96_UPGRADE_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$scriptCopied = 0
$sourceCopied = 0
$sourcePreserved = 0

# Source updates are additive by default: preserve existing art and Unity GUIDs.
$srcFiles = Get-ChildItem -LiteralPath $fromAssets -File -Recurse
foreach ($src in $srcFiles) {
    $relative = $src.FullName.Substring($fromAssets.Length).TrimStart([char[]]@('\','/'))
    $dest = Join-Path $targetAssets $relative
    $dir = Split-Path -Parent $dest
    if (-not (Test-Path -LiteralPath $dir -PathType Container)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    if (Test-Path -LiteralPath $dest -PathType Leaf) {
        if (-not $OverwriteSource) { $sourcePreserved++; continue }
        $existingHash=(Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash
        $newHash=(Get-FileHash -LiteralPath $src.FullName -Algorithm SHA256).Hash
        if ($existingHash -eq $newHash) { $sourcePreserved++; continue }
        $bak = Join-Path $backup (Join-Path 'Level96Source' $relative)
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $bak) | Out-Null
        Copy-Item -LiteralPath $dest -Destination $bak -Force
    }
    Copy-Item -LiteralPath $src.FullName -Destination $dest -Force
    $sourceCopied++
}
Write-Host "Level 96 assets copied: $sourceCopied; preserved: $sourcePreserved"

# Update only the Level-96-specific builder/records; no V2 or other level classes replaced.
foreach ($folderName in @('Editor','Runtime','Recipes')) {
    $srcDir = Join-Path $package (Join-Path 'GeneratorUpgrade' $folderName)
    $dstDir = Join-Path $generator $folderName
    if (-not (Test-Path -LiteralPath $dstDir -PathType Container)) {
        New-Item -ItemType Directory -Force -Path $dstDir | Out-Null
    }
    foreach ($src in (Get-ChildItem -LiteralPath $srcDir -File)) {
        $dest = Join-Path $dstDir $src.Name
        if (Test-Path -LiteralPath $dest -PathType Leaf) {
            $existingHash=(Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash
            $newHash=(Get-FileHash -LiteralPath $src.FullName -Algorithm SHA256).Hash
            if ($existingHash -eq $newHash) { Write-Host "Unchanged: $dest"; continue }
            $bak = Join-Path $backup (Join-Path $folderName $src.Name)
            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $bak) | Out-Null
            Copy-Item -LiteralPath $dest -Destination $bak -Force
        }
        Copy-Item -LiteralPath $src.FullName -Destination $dest -Force
        Write-Host "Installed: $dest"
        $scriptCopied++
    }
}
Write-Host "COMPLETE - Installed $scriptCopied Level 96 generator files."
if (Test-Path -LiteralPath $backup) { Write-Host "Backups: $backup" }
Write-Host "Reopen Unity and import OBJ fallbacks and PBR textures."
Write-Host "Tools > Project Spark > Automatic Level Generator > 96 > 1. Build FULL Level_96_Layout"
Write-Host "Select your Table_top anchor, then 96 > 2. Place Level 96 on SELECTED tabletop"
Write-Warning "Geometry and hierarchy only: 187 contact nets, 58 routes, 12 vias and W059 physical pins remain SCHEMATIC_REQUIRED. Gameplay certification remains blocked."
