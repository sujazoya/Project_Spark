# Project Spark L111 - CSV quoted-comma parser repair, additive Unity editor patch.
[CmdletBinding()]
param(
    [string]$UnityProjectPath = 'D:\Project_Spark_Master_Project\Project_Spark',
    [switch]$DryRun
)
$ErrorActionPreference = 'Stop'

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
if (-not (Test-Path -LiteralPath (Join-Path $project 'Assets') -PathType Container) -or
    -not (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings') -PathType Container)) {
    throw "Not a Unity project: $project"
}
$targetRoot = Join-Path $project 'Assets\ProjectSpark\Level111\SIX19'
$targetBuilder = Join-Path $targetRoot 'Unity\Editor\SparkL111FullLayoutBuilder.cs'
$targetManifest = Join-Path $targetRoot 'Data\L111_ModelManifest.csv'
$sourceBuilder = Join-Path $PSScriptRoot 'SparkL111FullLayoutBuilder.cs'
$sourceManifest = Join-Path $PSScriptRoot 'L111_ModelManifest.csv'

foreach ($p in @($targetBuilder,$targetManifest,$sourceBuilder,$sourceManifest)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) { throw "Missing file: $p" }
}
# The quote-safe PowerShell reader proves what the OBJ field SHOULD be.
$cleanRows = @(Import-Csv -LiteralPath $sourceManifest)
if ($cleanRows.Count -ne 176) { throw "Hotfix manifest has unexpected record count: $($cleanRows.Count)" }
$problem = $cleanRows | Where-Object { $_.asset_id -eq 'RetimerHeatsink' } | Select-Object -First 1
if ($null -eq $problem -or $problem.obj -ne '171_L111_RetimerHeatsink.obj') {
    throw 'Hotfix manifest preflight failed: RetimerHeatsink OBJ name does not match canonical source.'
}

$restoreManifest = $false
try {
    $localRows = @(Import-Csv -LiteralPath $targetManifest)
    if ($localRows.Count -ne 176) {
        $restoreManifest = $true
    } else {
        foreach ($r in $localRows) {
            $objName = $r.obj
            if ([string]::IsNullOrWhiteSpace($objName) -or
                $objName -eq 'NOT_INCLUDED' -or
                -not $objName.EndsWith('.obj', [System.StringComparison]::OrdinalIgnoreCase)) {
                $restoreManifest = $true
                break
            }
        }
    }
} catch { $restoreManifest = $true }

$backup = Join-Path $project ('ProjectSpark_Backups\Level111_CSV_Parser_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$items = @(@{ Source = $sourceBuilder; Target = $targetBuilder })
if ($restoreManifest) {
    Write-Warning '[L111] Installed manifest has invalid OBJ records; restoring the canonical manifest with backup.'
    $items += @{ Source = $sourceManifest; Target = $targetManifest }
}
Write-Host '[L111] Fix: parse quoted commas in CSV, especially line 172 of L111_ModelManifest.csv.'
Write-Host "[L111] Unity project: $project"
Write-Host "[L111] DryRun: $DryRun"
foreach ($item in $items) {
    $srcHash = (Get-FileHash -LiteralPath $item.Source -Algorithm SHA256).Hash
    $dstHash = (Get-FileHash -LiteralPath $item.Target -Algorithm SHA256).Hash
    if ($srcHash -eq $dstHash) {
        Write-Host "[L111] Already correct: $($item.Target)"
        continue
    }
    if ($DryRun) {
        Write-Host "[DRY RUN] Would replace: $($item.Target)"
        continue
    }
    $relative = $item.Target.Substring($targetRoot.Length).TrimStart('\','/')
    $backupFile = Join-Path $backup $relative
    $backupParent = Split-Path -Parent $backupFile
    if (-not (Test-Path -LiteralPath $backupParent)) {
        New-Item -Path $backupParent -ItemType Directory -Force | Out-Null
    }
    Copy-Item -LiteralPath $item.Target -Destination $backupFile
    Copy-Item -LiteralPath $item.Source -Destination $item.Target -Force
    $installedHash = (Get-FileHash -LiteralPath $item.Target -Algorithm SHA256).Hash
    if ($installedHash -ne $srcHash) { throw "Copy verification failed: $($item.Target)" }
    Write-Host "[L111] Repaired: $($item.Target)"
    Write-Host "[L111] Backup: $backupFile"
}

# Verify the installed manifest against all installed OBJ files (without modifying models).
$installedRecords = @(Import-Csv -LiteralPath $targetManifest)
$missing = New-Object System.Collections.Generic.List[string]
foreach ($record in $installedRecords) {
    $objName = $record.obj
    if ([string]::IsNullOrWhiteSpace($objName) -or
        -not (Test-Path -LiteralPath (Join-Path (Join-Path $targetRoot 'Models_OBJ') $objName) -PathType Leaf)) {
        $missing.Add("$($record.asset_id): $objName")
    }
}
if ($missing.Count -eq 0) {
    Write-Host "[L111] Model check: $($installedRecords.Count) manifest-listed OBJ files present."
} else {
    Write-Warning "[L111] $($missing.Count) OBJ files absent; reinstall the FULL Level111 kit if validation still fails. First entries: $($missing | Select-Object -First 5 | Out-String)"
}
Write-Host '[L111] Patch only targets Level 111; Unity .meta files and other levels are preserved.'
Write-Host '[L111] Reopen Unity > Tools > Project Spark > Automatic Level Generator > 111 > 3. Validate Sources and Report Blockers.'
