[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$UnityProjectPath)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$assets = Join-Path $project 'Assets'
$version = Join-Path $project 'ProjectSettings\ProjectVersion.txt'
if (-not (Test-Path -LiteralPath $assets -PathType Container) -or
    -not (Test-Path -LiteralPath $version -PathType Leaf)) {
    throw "Not a valid Unity project: Assets or ProjectVersion.txt not found under '$project'."
}
$source = Join-Path $PSScriptRoot 'ProjectSpark_Level123_AFX31'
$mode = 'Full'
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    $source = Join-Path $PSScriptRoot 'GeneratorUpgrade'
    $mode = 'UpgradeOnly'
}
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    throw 'Extract the Level123 FULL or GeneratorUpgrade ZIP to this installer directory first.'
}
$destination = Join-Path $assets 'ProjectSpark\Level123\AFX31'
if ($mode -eq 'Full') {
    $models = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ModelManifest.csv'))
    $bom = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ComponentBOM.csv'))
    $placements = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ComponentPlacements.csv'))
    $routes = @(Import-Csv -LiteralPath (Join-Path $source 'Data\CopperRoutes_400_REQUIRES_SCHEMATIC.csv'))
    $vias = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ViaTasks_156_REQUIRES_SCHEMATIC.csv'))
    $obj = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_OBJ') -Filter '*.obj' -File)
    $glb = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_GLB') -Filter '*.glb' -File)
    $maps = @(Get-ChildItem -LiteralPath (Join-Path $source 'Textures') -Filter '*_BaseColor.png' -File)
    if ($models.Count -ne 368 -or $bom.Count -ne 312 -or $placements.Count -ne 312 -or
        $routes.Count -ne 400 -or $vias.Count -ne 156 -or $obj.Count -ne 369 -or
        $glb.Count -ne 369 -or $maps.Count -ne 44) {
        throw "Invalid source counts: models=$($models.Count), BOM=$($bom.Count), placements=$($placements.Count), routes=$($routes.Count), vias=$($vias.Count), OBJ=$($obj.Count), GLB=$($glb.Count), materials=$($maps.Count). No file changes made."
    }
    foreach ($entry in $models) {
        if ($entry.obj -eq 'NOT_INCLUDED' -or -not $entry.obj.EndsWith('.obj', [StringComparison]::OrdinalIgnoreCase) -or
            -not $entry.glb.EndsWith('.glb', [StringComparison]::OrdinalIgnoreCase) -or
            -not (Test-Path -LiteralPath (Join-Path $source ('Models_OBJ\' + $entry.obj)) -PathType Leaf) -or
            -not (Test-Path -LiteralPath (Join-Path $source ('Models_GLB\' + $entry.glb)) -PathType Leaf)) {
            throw "Invalid model manifest OBJ/GLB filename for '$($entry.asset_id)'. No file changes made."
        }
    }
} else {
    if (-not (Test-Path -LiteralPath (Join-Path $destination 'Data\ModelManifest.csv') -PathType Leaf)) {
        throw 'Generator-only upgrade needs Level123 original source already installed. Install the FULL ZIP first.'
    }
}

$sourceFiles = @(Get-ChildItem -LiteralPath $source -Recurse -File)
$plan = New-Object 'System.Collections.Generic.List[object]'
$unchanged = 0
foreach ($file in $sourceFiles) {
    $relative = $file.FullName.Substring($source.Length).TrimStart([char[]]@('\','/'))
    $relativeSlash = $relative.Replace('\','/')
    if ($relativeSlash.EndsWith('.meta', [StringComparison]::OrdinalIgnoreCase)) { continue }
    if ($mode -eq 'UpgradeOnly' -and -not
        ($relativeSlash.StartsWith('Unity/', [StringComparison]::OrdinalIgnoreCase) -or
         $relativeSlash.StartsWith('Documentation/', [StringComparison]::OrdinalIgnoreCase) -or
         $relativeSlash.StartsWith('Tests/', [StringComparison]::OrdinalIgnoreCase))) { continue }
    $target = Join-Path $destination $relative
    $isUpdateAllowed = $relativeSlash.StartsWith('Unity/', [StringComparison]::OrdinalIgnoreCase) -or
                       $relativeSlash.StartsWith('Documentation/', [StringComparison]::OrdinalIgnoreCase) -or
                       $relativeSlash.StartsWith('Tests/', [StringComparison]::OrdinalIgnoreCase)
    $exists = Test-Path -LiteralPath $target -PathType Leaf
    if ($exists) {
        $hashOld = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
        $hashNew = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        if ($hashOld -eq $hashNew) { $unchanged++; continue }
        if (-not $isUpdateAllowed) {
            throw "Refusing to overwrite modified original data/art: '$target'. No changes made."
        }
    }
    $plan.Add([PSCustomObject]@{ From = $file.FullName; To = $target; Relative = $relative; Backup = $exists })
}

$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupDirectory = Join-Path $project ('ProjectSpark_Level123_Backups_' + $timestamp)
$copied = 0
foreach ($item in $plan) {
    if ($item.Backup) {
        $backupFile = Join-Path $backupDirectory $item.Relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $backupFile) -Force | Out-Null
        Copy-Item -LiteralPath $item.To -Destination $backupFile -Force
    }
    New-Item -ItemType Directory -Path (Split-Path -Parent $item.To) -Force | Out-Null
    Copy-Item -LiteralPath $item.From -Destination $item.To -Force
    $copied++
}
Write-Host "[L123] Installed $mode package: $copied files; $unchanged already identical." -ForegroundColor Green
Write-Host "[L123] Unity assets installed under: $destination"
Write-Host 'Unity menu: Tools > Project Spark > Automatic Level Generator > 122'
Write-Host 'Run: 3 Validate Sources; 4 Rebuild URP PBR Materials; 1 Build FULL Level_123_Layout.'
Write-Host 'Select Table_top then run: 2 Place assembled VISUAL preview on SELECTED tabletop.'
Write-Host 'Source art/Unity editor integration only. All flight-control, electrical wiring and independent safety certification BLOCKED.'
