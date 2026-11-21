[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$UnityProjectPath)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$assets = Join-Path $project 'Assets'
$version = Join-Path $project 'ProjectSettings\ProjectVersion.txt'
if (-not (Test-Path -LiteralPath $assets -PathType Container) -or
    -not (Test-Path -LiteralPath $version -PathType Leaf)) {
    throw "Invalid Unity project (Assets or ProjectVersion.txt missing): $project"
}
$source = Join-Path $PSScriptRoot 'ProjectSpark_Level117_NFX25'
$mode = 'Full'
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    $source = Join-Path $PSScriptRoot 'GeneratorUpgrade'
    $mode = 'UpgradeOnly'
}
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    throw 'Extract the complete or generator-only Level 117 ZIP before running this installer.'
}
$destination = Join-Path $assets 'ProjectSpark\Level117\NFX25'
if ($mode -eq 'Full') {
    $models = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ModelManifest.csv'))
    $bom = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ComponentBOM.csv'))
    $routes = @(Import-Csv -LiteralPath (Join-Path $source 'Data\CopperRoutes_304_REQUIRES_SCHEMATIC.csv'))
    $vias = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ViaTasks_108_REQUIRES_SCHEMATIC.csv'))
    $obj = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_OBJ') -Filter '*.obj' -File)
    $glb = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_GLB') -Filter '*.glb' -File)
    if ($models.Count -ne 272 -or $bom.Count -ne 220 -or
        $routes.Count -ne 304 -or $vias.Count -ne 108 -or
        $obj.Count -ne 273 -or $glb.Count -ne 273) {
        throw "Source invalid: models=$($models.Count), BOM=$($bom.Count), routes=$($routes.Count), vias=$($vias.Count), OBJ=$($obj.Count), GLB=$($glb.Count). No changes made."
    }
} else {
    $existing = Join-Path $destination 'Data\ModelManifest.csv'
    if (-not (Test-Path -LiteralPath $existing -PathType Leaf)) {
        throw 'Generator upgrade requires the previously installed full Level 117 source. Use complete package first.'
    }
}

$sourceFiles = @(Get-ChildItem -LiteralPath $source -Recurse -File)
$plan = New-Object System.Collections.Generic.List[object]
$skipped = 0
foreach ($file in $sourceFiles) {
    $relative = $file.FullName.Substring($source.Length).TrimStart([char[]]@('\','/'))
    $relUnix = $relative.Replace('\','/')
    if ($mode -eq 'UpgradeOnly' -and -not
        ($relUnix.StartsWith('Unity/', [StringComparison]::OrdinalIgnoreCase) -or
         $relUnix.StartsWith('Documentation/', [StringComparison]::OrdinalIgnoreCase) -or
         $relUnix.StartsWith('Tests/', [StringComparison]::OrdinalIgnoreCase))) { continue }
    if ($relative.EndsWith('.meta', [StringComparison]::OrdinalIgnoreCase)) { continue }
    $target = Join-Path $destination $relative
    $canUpdate = $relUnix.StartsWith('Unity/', [StringComparison]::OrdinalIgnoreCase) -or
                 $relUnix.StartsWith('Documentation/', [StringComparison]::OrdinalIgnoreCase) -or
                 $relUnix.StartsWith('Tests/', [StringComparison]::OrdinalIgnoreCase)
    $exists = Test-Path -LiteralPath $target -PathType Leaf
    if ($exists) {
        $oldHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
        $newHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        if ($oldHash -eq $newHash) { $skipped++; continue }
        if (-not $canUpdate) {
            throw "Original source differs; refusing overwrite: $target. No files have yet been copied."
        }
    }
    $plan.Add([PSCustomObject]@{ From=$file.FullName; To=$target; Relative=$relative; Backup=$exists })
}
$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backup = Join-Path $project ('ProjectSpark_Level117_Backups_' + $stamp)
$copied = 0
foreach ($item in $plan) {
    if ($item.Backup) {
        $backupFile = Join-Path $backup $item.Relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $backupFile) -Force | Out-Null
        Copy-Item -LiteralPath $item.To -Destination $backupFile -Force
    }
    New-Item -ItemType Directory -Path (Split-Path -Parent $item.To) -Force | Out-Null
    Copy-Item -LiteralPath $item.From -Destination $item.To -Force
    $copied++
}
Write-Host "[L117] Installed $mode package: $copied files; identical skipped: $skipped." -ForegroundColor Green
Write-Host "[L117] Assets path: $destination"
Write-Host 'Unity Menu: Tools > Project Spark > Automatic Level Generator > 117'
Write-Host 'Order: 3. Validate Sources; 4. Rebuild URP PBR Materials; 1. Build FULL Level_117_Layout.'
Write-Host 'For preview: select existing Table_top > 2. Place assembled VISUAL preview.'
Write-Host 'Certification remains BLOCKED until approved electrical schematic and Unity Play Mode verification.'
