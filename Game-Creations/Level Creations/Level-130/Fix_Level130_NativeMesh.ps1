[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$UnityProjectPath,
    [switch]$DryRun
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath $UnityProjectPath).Path
if (-not (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt') -PathType Leaf)) { throw 'Not a Unity project root.' }
$root = Join-Path $project 'Assets\ProjectSpark\Level130\H610ME'
$builder = Join-Path $root 'Unity\Editor\SparkL130BoardBuilder.cs'
if (-not (Test-Path -LiteralPath $builder -PathType Leaf)) { throw "Missing installed builder: $builder" }
$payload = Join-Path $PSScriptRoot 'Payload'
$rows = @(Import-Csv -LiteralPath (Join-Path $PSScriptRoot 'PayloadHashes.csv'))
if ($rows.Count -ne 459) { throw 'Incorrect payload count; extract the complete hotfix ZIP.' }
$pending = New-Object 'System.Collections.Generic.List[object]'
$seen = @{}
foreach ($row in $rows) {
    $rel = $row.relative_path.Replace('/','\')
    if ($rel -notmatch '^(Models_Native\\L130_[a-zA-Z0-9_]+\.spark130mesh|Unity\\Editor\\SparkL130NativeMeshImporter\.cs)$' -or $seen.ContainsKey($rel)) { throw "Invalid duplicate or unsafe payload path: $rel" }
    $seen[$rel] = $true
    $src = Join-Path $payload $rel
    $dst = Join-Path $root $rel
    if (-not (Test-Path -LiteralPath $src -PathType Leaf)) { throw "Missing payload: $src. Extract the entire ZIP." }
    if ((Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -ne $row.sha256) { throw "Payload hash mismatch: $src" }
    $exists = Test-Path -LiteralPath $dst -PathType Leaf
    if ($exists -and ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -eq $row.sha256)) { continue }
    $pending.Add([PSCustomObject]@{Relative=$rel;Source=$src;Destination=$dst;Exists=$exists})
}
$manifest = @(Import-Csv -LiteralPath (Join-Path $root 'Data\ModelManifest.csv'))
if ($manifest.Count -ne 456) { throw "Incorrect installed manifest count: $($manifest.Count)" }
$unique = @{}
foreach ($row in $manifest) {
    if ($row.fbx -notmatch '^L130_[a-zA-Z0-9_]+\.fbx$' -or $unique.ContainsKey($row.fbx)) { throw "Invalid installed manifest model: $($row.fbx)" }
    $unique[$row.fbx]=$true
    $native = 'Models_Native\' + [IO.Path]::GetFileNameWithoutExtension($row.fbx) + '.spark130mesh'
    if (-not $seen.ContainsKey($native)) { throw "No native mesh for $($row.fbx)" }
    if (-not (Test-Path -LiteralPath (Join-Path (Join-Path $root 'Models_FBX') $row.fbx) -PathType Leaf)) { throw "Original FBX missing: $($row.fbx)" }
}
foreach ($f in @('L130_CompleteMotherboard','L130_CompleteMotherboard_Batched')) {
    if (-not $seen.ContainsKey("Models_Native\$f.spark130mesh")) { throw "Native showcase missing: $f" }
}
$content = [IO.File]::ReadAllText($builder)
$lf = $content.Replace("`r`n","`n")
$old = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'OriginalRequireModel.txt')).Replace("`r`n","`n")
$new = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'NativeRequireModel.txt')).Replace("`r`n","`n")
$patch = $false
if ($lf.Contains($new)) { Write-Host '[L130] Builder already patched.' }
elseif ($lf.Contains($old) -and $lf.IndexOf($old) -eq $lf.LastIndexOf($old)) { $lf = $lf.Replace($old,$new); $patch = $true }
else { throw 'Installed RequireModel method differs from the supplied kit. No files changed. Send the installed SparkL130BoardBuilder.cs for a targeted patch.' }
if ($patch -and $content.Contains("`r`n")) { $lf = $lf.Replace("`n","`r`n") }
Write-Host "[L130] Verified 458 native meshes and importer. Pending payload files: $($pending.Count); patch builder: $patch"
if ($DryRun) { Write-Host '[L130] Dry run complete. No writes.'; return }
$backup = Join-Path $project ('ProjectSpark_Backups\Level130_NativeMesh_' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
New-Item -ItemType Directory -Path $backup -Force | Out-Null
Copy-Item -LiteralPath $builder -Destination (Join-Path $backup 'SparkL130BoardBuilder.cs')
foreach ($p in $pending) {
    if ($p.Exists) {
        $bk = Join-Path $backup $p.Relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $bk) -Force | Out-Null
        Copy-Item -LiteralPath $p.Destination -Destination $bk
    }
}
$done = New-Object 'System.Collections.Generic.List[object]'
try {
    foreach ($p in $pending) {
        New-Item -ItemType Directory -Path (Split-Path -Parent $p.Destination) -Force | Out-Null
        $done.Add($p)
        Copy-Item -LiteralPath $p.Source -Destination $p.Destination -Force
        if ((Get-FileHash -LiteralPath $p.Destination -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $p.Source -Algorithm SHA256).Hash) { throw "Copy verification failed: $($p.Relative)" }
    }
    if ($patch) {
        [IO.File]::WriteAllText($builder,$lf,(New-Object System.Text.UTF8Encoding($false)))
        if ([IO.File]::ReadAllText($builder) -ne $lf) { throw 'Builder write verification failed.' }
    }
} catch {
    Copy-Item -LiteralPath (Join-Path $backup 'SparkL130BoardBuilder.cs') -Destination $builder -Force
    foreach ($p in $done) {
        if ($p.Exists) { Copy-Item -LiteralPath (Join-Path $backup $p.Relative) -Destination $p.Destination -Force }
        elseif (Test-Path -LiteralPath $p.Destination) { Remove-Item -LiteralPath $p.Destination -Force }
    }
    throw
}
Write-Host '[L130] Native mesh hotfix installed successfully.' -ForegroundColor Green
Write-Host "[L130] Backup: $backup"
Write-Host '[L130] Original FBX, textures, manifests, scenes and .meta files unchanged.'
Write-Host '[L130] Open Unity, compile scripts, reimport Models_Native if necessary, then run Level 130 source validation.'
