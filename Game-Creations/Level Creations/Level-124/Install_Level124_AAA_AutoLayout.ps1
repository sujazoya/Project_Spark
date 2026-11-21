[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)][string]$UnityProjectPath,
  [switch]$DryRun
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$assets = Join-Path $project 'Assets'
$version = Join-Path $project 'ProjectSettings\ProjectVersion.txt'
if (-not (Test-Path -LiteralPath $assets -PathType Container) -or
    -not (Test-Path -LiteralPath $version -PathType Leaf)) {
  throw "Not a Unity project: $project (missing Assets or ProjectVersion.txt)."
}
$source = Join-Path $PSScriptRoot 'ProjectSpark_Level124_OBX32'
$mode = 'Full'
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
  $source = Join-Path $PSScriptRoot 'GeneratorUpgrade'
  $mode = 'UpgradeOnly'
}
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
  throw 'Extract the FULL or Generator Upgrade ZIP with the installer in the same directory.'
}
$destination = Join-Path $assets 'ProjectSpark\Level124\OBX32'
if ($mode -eq 'Full') {
  $models = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ModelManifest.csv'))
  $bom = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ComponentBOM.csv'))
  $placements = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ComponentPlacements.csv'))
  $routes = @(Import-Csv -LiteralPath (Join-Path $source 'Data\CopperRoutes_416_REQUIRES_SCHEMATIC.csv'))
  $vias = @(Import-Csv -LiteralPath (Join-Path $source 'Data\ViaTasks_164_REQUIRES_SCHEMATIC.csv'))
  $obj = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_OBJ') -File -Filter '*.obj')
  $glb = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_GLB') -File -Filter '*.glb')
  $mtl = @(Get-ChildItem -LiteralPath (Join-Path $source 'Models_OBJ') -File -Filter '*.mtl')
  $baseColor = @(Get-ChildItem -LiteralPath (Join-Path $source 'Textures') -File -Filter '*_BaseColor.png')
  $urpPacked = @(Get-ChildItem -LiteralPath (Join-Path $source 'URP_Packed') -File -Filter '*_URP_MetallicSmoothness.png')
  if ($models.Count -ne 384 -or $bom.Count -ne 328 -or $placements.Count -ne 328 -or
      $routes.Count -ne 416 -or $vias.Count -ne 164 -or $obj.Count -ne 385 -or
      $glb.Count -ne 385 -or $mtl.Count -ne 385 -or $baseColor.Count -ne 46 -or
      $urpPacked.Count -ne 46) {
    throw "Count mismatch: models=$($models.Count), BOM=$($bom.Count), placements=$($placements.Count), routes=$($routes.Count), vias=$($vias.Count), OBJ=$($obj.Count), GLB=$($glb.Count), MTL=$($mtl.Count), PBR=$($baseColor.Count), packed=$($urpPacked.Count). No files copied."
  }
  foreach ($item in $models) {
    if ($item.obj -eq 'NOT_INCLUDED' -or -not $item.obj.EndsWith('.obj', [StringComparison]::OrdinalIgnoreCase) -or
        -not $item.glb.EndsWith('.glb', [StringComparison]::OrdinalIgnoreCase) -or
        -not (Test-Path -LiteralPath (Join-Path $source ('Models_OBJ\' + $item.obj)) -PathType Leaf) -or
        -not (Test-Path -LiteralPath (Join-Path $source ('Models_GLB\' + $item.glb)) -PathType Leaf)) {
      throw "Bad Level124 model path for $($item.asset_id). No files copied."
    }
  }
  if (-not (Test-Path -LiteralPath (Join-Path $source 'Models_OBJ\385_L124_AssembledReference.obj')) -or
      -not (Test-Path -LiteralPath (Join-Path $source 'Models_GLB\385_L124_AssembledReference.glb'))) {
    throw 'Missing full assembled 385th reference model. No files copied.'
  }
} else {
  if (-not (Test-Path -LiteralPath (Join-Path $destination 'Data\ModelManifest.csv') -PathType Leaf)) {
    throw 'Upgrade-only package requires original full Level124 art installed first.'
  }
}

$allFiles = @(Get-ChildItem -LiteralPath $source -File -Recurse)
$pending = New-Object 'System.Collections.Generic.List[object]'
$already = 0
foreach ($file in $allFiles) {
  $relative = $file.FullName.Substring($source.Length).TrimStart([char[]]@('\','/'))
  $relativeNorm = $relative.Replace('\','/')
  if ($relativeNorm.EndsWith('.meta', [StringComparison]::OrdinalIgnoreCase)) { continue }
  if ($mode -eq 'UpgradeOnly' -and -not (
      $relativeNorm.StartsWith('Unity/', [StringComparison]::OrdinalIgnoreCase) -or
      $relativeNorm.StartsWith('Tests/', [StringComparison]::OrdinalIgnoreCase) -or
      $relativeNorm.StartsWith('Documentation/', [StringComparison]::OrdinalIgnoreCase))) { continue }
  $target = Join-Path $destination $relative
  $updatable = $relativeNorm.StartsWith('Unity/', [StringComparison]::OrdinalIgnoreCase) -or
               $relativeNorm.StartsWith('Tests/', [StringComparison]::OrdinalIgnoreCase) -or
               $relativeNorm.StartsWith('Documentation/', [StringComparison]::OrdinalIgnoreCase)
  $exists = Test-Path -LiteralPath $target -PathType Leaf
  if ($exists) {
    $oldHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
    $newHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    if ($oldHash -eq $newHash) { $already++; continue }
    if (-not $updatable) {
      throw "Refusing to overwrite modified source art/data: $target. No files copied."
    }
  }
  $pending.Add([PSCustomObject]@{From=$file.FullName;To=$target;Rel=$relative;NeedsBackup=$exists})
}
Write-Host "[L124] Package ${mode}: $($pending.Count) proposed changes; $already already identical."
if ($DryRun) {
  Write-Host '[L124] DryRun completed; zero file changes.' -ForegroundColor Yellow
  return
}
$backupBase = Join-Path $project ('ProjectSpark_Level124_Backups_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$copies = 0
foreach ($item in $pending) {
  if ($item.NeedsBackup) {
    $backupFile = Join-Path $backupBase $item.Rel
    New-Item -ItemType Directory -Path (Split-Path -Parent $backupFile) -Force | Out-Null
    Copy-Item -LiteralPath $item.To -Destination $backupFile -Force
  }
  New-Item -ItemType Directory -Path (Split-Path -Parent $item.To) -Force | Out-Null
  Copy-Item -LiteralPath $item.From -Destination $item.To -Force
  $copies++
}
Write-Host "[L124] Installed $copies files to $destination" -ForegroundColor Green
Write-Host 'Unity: Tools > Project Spark > Automatic Level Generator > 124'
Write-Host 'First 3 Validate Sources; 4 Rebuild URP; 5 Create COMPLETE visual mainboard; then 1 Build FULL Layout.'
Write-Host 'Existing Levels 1-123 were not targeted. Electrical/spacecraft/gameplay certification is BLOCKED.'
