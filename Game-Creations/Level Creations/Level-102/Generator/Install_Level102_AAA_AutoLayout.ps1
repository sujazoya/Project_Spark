param(
    [Parameter(Mandatory = $true)]
    [string]$UnityProjectPath
)
$ErrorActionPreference = 'Stop'

function Copy-Safely([string]$Source,[string]$Target,[bool]$Replace) {
    $parent = Split-Path -Parent $Target
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    if (Test-Path -LiteralPath $Target) {
        if (-not $Replace) { $script:Skipped++; return }
        $relative = $Target.Substring($script:AssetsRoot.Length).TrimStart('\','/')
        $backup = Join-Path $script:BackupRoot $relative
        $backupParent = Split-Path -Parent $backup
        New-Item -ItemType Directory -Path $backupParent -Force | Out-Null
        Copy-Item -LiteralPath $Target -Destination $backup -Force
        $script:BackedUp++
    }
    Copy-Item -LiteralPath $Source -Destination $Target -Force
    $script:Copied++
}

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$script:AssetsRoot = Join-Path $project 'Assets'
if (-not (Test-Path (Join-Path $project 'ProjectSettings\ProjectVersion.txt'))) {
    throw "Not a Unity project: $project (ProjectSettings\ProjectVersion.txt missing)"
}
$packageRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceArt = Join-Path $packageRoot 'SourceAssets\Level102_HSX10'
$script:BackupRoot = Join-Path $project ('ProjectSpark_Level102_UpgradeBackups\' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$script:Copied = 0; $script:Skipped = 0; $script:BackedUp = 0

$matches = @(Get-ChildItem -LiteralPath $script:AssetsRoot -Recurse -File -Filter 'ModelManifest.csv' -ErrorAction SilentlyContinue | Where-Object {
    $_.FullName -match '(?i)[\\/]Data[\\/]ModelManifest\.csv$' -and $_.FullName -match '(?i)(Level102|L102|HSX10)'
})
if ($matches.Count -gt 1) {
    $all = $matches.FullName -join "`n  "
    throw "Multiple Level 102 data roots found. Remove duplicates, then rerun.`n  $all"
}
if ($matches.Count -eq 1) {
    $targetRoot = Split-Path -Parent (Split-Path -Parent $matches[0].FullName)
    Write-Host "Using existing Level 102 asset root: $targetRoot"
} else {
    if (-not (Test-Path (Join-Path $sourceArt 'Data\ModelManifest.csv'))) {
        throw 'Upgrade-only package requires previously installed Level102_HSX10 source files. Use the COMPLETE package instead.'
    }
    $targetRoot = Join-Path $script:AssetsRoot 'ProjectSpark\Level102\Level102_HSX10'
    Write-Host "Installing new Level 102 asset root: $targetRoot"
}

# Preserve original art and data. Copy source only where missing; exclude legacy editor utilities.
if (Test-Path -LiteralPath $sourceArt) {
    $assetsToCopy = @(Get-ChildItem -LiteralPath $sourceArt -Recurse -File)
    foreach ($file in $assetsToCopy) {
        $relative = $file.FullName.Substring($sourceArt.Length).TrimStart('\','/')
        if ($relative -match '^Unity[\\/]Editor[\\/]') { continue }
        if ($relative -match '^Unity[\\/]Runtime[\\/]') { continue }
        $dest = Join-Path $targetRoot $relative
        Copy-Safely -Source $file.FullName -Target $dest -Replace $false
    }
}

foreach ($folder in @('Models_OBJ_Fallback','URP_Packed')) {
    $src = Join-Path $packageRoot $folder
    if (-not (Test-Path -LiteralPath $src)) { throw "Missing upgrade folder: $folder" }
    foreach ($file in Get-ChildItem -LiteralPath $src -File) {
        $dest = Join-Path (Join-Path $targetRoot $folder) $file.Name
        # Fallback assets are managed by this upgrade, so newer corrected geometry replaces older versions.
        Copy-Safely -Source $file.FullName -Target $dest -Replace $true
    }
}

$upgrade = Join-Path $packageRoot 'GeneratorUpgrade\Unity'
foreach ($folder in @('Editor','Runtime')) {
    $src = Join-Path $upgrade $folder
    foreach ($file in Get-ChildItem -LiteralPath $src -Filter '*.cs' -File) {
        $dest = Join-Path (Join-Path $targetRoot ('Unity\' + $folder)) $file.Name
        Copy-Safely -Source $file.FullName -Target $dest -Replace $true
    }
}
# Retain the original marker if it wasn't installed with earlier source kits.
$originalMarker = Join-Path $sourceArt 'Unity\Runtime\SparkL102VisualMarker.cs'
if (Test-Path -LiteralPath $originalMarker) {
    Copy-Safely -Source $originalMarker -Target (Join-Path $targetRoot 'Unity\Runtime\SparkL102VisualMarker.cs') -Replace $false
}

$docDst = Join-Path $targetRoot 'Documentation\AAA_AutoLayout_Upgrade'
Get-ChildItem -LiteralPath (Join-Path $packageRoot 'GeneratorUpgrade\Docs') -File | ForEach-Object {
    Copy-Safely -Source $_.FullName -Target (Join-Path $docDst $_.Name) -Replace $true
}

$required = @(
    (Join-Path $targetRoot 'Data\ModelManifest.csv'),
    (Join-Path $targetRoot 'Models_OBJ_Fallback\083_L102_PCB_Base.obj'),
    (Join-Path $targetRoot 'Models_OBJ_Fallback\L102_Assembled_Reference.obj'),
    (Join-Path $targetRoot 'URP_Packed\PCB_Navy_URP_MetallicSmoothness.png'),
    (Join-Path $targetRoot 'Unity\Editor\SparkL102FullLayoutBuilder.cs'),
    (Join-Path $targetRoot 'Unity\Runtime\SparkL102LayoutRecords.cs')
)
foreach ($p in $required) { if (-not (Test-Path -LiteralPath $p)) { throw "Post-install check failed: $p" } }
$objs = @(Get-ChildItem (Join-Path $targetRoot 'Models_OBJ_Fallback') -Filter '*.obj' -File)
if ($objs.Count -ne 105) { throw "Expected exactly 105 OBJ models, found $($objs.Count)" }
Write-Host ""
Write-Host "Level 102 HSX-10 install finished. New/replaced: $script:Copied; source retained: $script:Skipped; backup copies: $script:BackedUp"
Write-Host "Source root: $targetRoot"
Write-Host "Backups (if overwritten): $script:BackupRoot"
Write-Host ""
Write-Host 'Now open Unity 6.3 and allow all asset/script imports to finish.'
Write-Host '1) Tools > Project Spark > Automatic Level Generator > 102 > 3. Validate Imported Art and Source'
Write-Host '2) Tools > Project Spark > Automatic Level Generator > 102 > 1. Build FULL Level_102_Layout'
Write-Host '3) Select Table_top > 102 > 2. Place Level 102 on SELECTED tabletop'
Write-Host 'NOTE: Tabletop output is VISUAL ONLY. Electrical certification is BLOCKED_UNVERIFIED_NETLIST.'
