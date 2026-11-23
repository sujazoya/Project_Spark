[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$UnityProjectPath,[switch]$DryRun,[switch]$ReplaceExisting)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $UnityProjectPath).Path
if (-not (Test-Path -LiteralPath (Join-Path $root 'ProjectSettings\ProjectVersion.txt') -PathType Leaf)) { throw 'Invalid Unity project root.' }
$allowed = @(
    'Assets\My_Assets\_Project_Spark\Scripts\ProjectSpark\Levels\UI\SparkCampaignPanel.cs',
    'Assets\ProjectSpark\AAAArchitecture\Documentation\INTEGRATION_AND_TESTS.md',
    'Assets\ProjectSpark\AAAArchitecture\Documentation\VISUAL_ARCHITECT_TESTS.md',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Action.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\ActiveTab.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Brief.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\CardBezel.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\CardFooter.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Chamber.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\CompleteIcon.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Fade.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\GalleryCard.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Header.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\HeaderPanorama.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Index.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\LockedIcon.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Notice.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Objective.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Rail.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\ReadyIcon.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Selection.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Shell.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\Tab.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8\UnknownIcon.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Action.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\ActiveTab.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Brief.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\CardBezel.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\CardFooter.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Chamber.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\GalleryCard.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Header.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\HeaderPanorama.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Index.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Notice.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Objective.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Rail.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Selection.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Shell.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProArtV8Neon\Tab.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Action.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\ActiveTab.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Brief.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Chamber.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Fade.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\GalleryCard.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Header.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Index.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Notice.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Objective.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Rail.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Selection.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Shell.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ProfessionalArt\Tab.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Action.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\ActiveTab.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Brief.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Chamber.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\GalleryCard.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Header.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Index.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Notice.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Objective.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Rail.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Selection.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Shell.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\ShowcaseArt\Tab.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\SparkAAAArchitect.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\SparkAAAArchitectWindow.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\SparkCinematicUIArchitect.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\SparkFinalHDArchitect.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\SparkLivePreviewArchitect.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\SparkProfessionalArchitect.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\SparkShowcaseArchitect.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\SparkV8NeonArchitect.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Backdrop.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Badge.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Button.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Card.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Frame.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Grid.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Hero.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\LaboratoryHD.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Panel.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Primary.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Scan.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\StudioArt\Well.bytes',
    'Assets\ProjectSpark\AAAArchitecture\Editor\Visuals\SparkAAAStudio.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\Visuals\SparkAAAVisualArchitect.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\Visuals\SparkAAAVisualSettings.cs',
    'Assets\ProjectSpark\AAAArchitecture\Editor\Visuals\SparkAAAVisualWindow.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkAAACatalog.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkAAAEventGateway.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkAAALevelCard.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkAAAMissionGateway.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkAAAMissionPanel.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkAAAPreferences.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkAAAUIFactory.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkCinematicClock.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkCinematicSkin.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkCinematicUV.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkFinalPresentation.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkFinalPresentationSettings.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkLivePreview.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkLivePreviewAsset.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkLivePreviewSettings.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkProfessionalSettings.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkProfessionalSkin.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkShowcaseSkin.cs',
    'Assets\ProjectSpark\AAAArchitecture\Runtime\SparkShowcaseStage.cs',
    'Assets\ProjectSpark\AAAArchitecture\Shaders\SparkCinematicSurface.shader',
    'Assets\ProjectSpark\AAAArchitecture\Shaders\SparkShowcaseStage.shader'
)
$rows = @(Import-Csv -LiteralPath (Join-Path $PSScriptRoot 'SHA256SUMS.csv'))
if ($rows.Count -ne $allowed.Count) { throw 'Unexpected package manifest count.' }
$seen=@{}; $pending=New-Object System.Collections.Generic.List[object]
foreach ($row in $rows) {
    $rel=$row.relative_path.Replace('/','\')
    if ($allowed -notcontains $rel -or $seen.ContainsKey($rel)) { throw ('Unapproved or duplicate path: {0}' -f $rel) }
    $seen[$rel]=$true; $src=Join-Path $PSScriptRoot $rel; $dst=Join-Path $root $rel
    if (-not (Test-Path -LiteralPath $src -PathType Leaf) -or (Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -ne $row.sha256) { throw ('Source verification failed: {0}' -f $rel) }
    $exists=Test-Path -LiteralPath $dst -PathType Leaf
    if ($exists) {
        if ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -eq $row.sha256) {continue}
        if (-not $ReplaceExisting) {throw ('Existing source differs: {0}. Review and explicitly use -ReplaceExisting.' -f $rel)}
    }
    $pending.Add([PSCustomObject]@{Relative=$rel;Source=$src;Destination=$dst;Exists=$exists})
    Write-Host ('Change planned: {0}' -f $rel)
}
# Detect alternate copies of the named C# scripts before adding or replacing any source.
$expected = @{}
foreach ($relative in $allowed) {
    if ([IO.Path]::GetExtension($relative) -eq '.cs') {
        $expected[[IO.Path]::GetFileName($relative)] = [IO.Path]::GetFullPath((Join-Path $root $relative))
    }
}
$duplicates = @(Get-ChildItem -LiteralPath (Join-Path $root 'Assets') -Filter '*.cs' -File -Recurse | Where-Object {
    $expected.ContainsKey($_.Name) -and $_.FullName -ne $expected[$_.Name]
})
if ($duplicates.Count -gt 0) {
    $duplicates | ForEach-Object { Write-Warning ('Alternate script location: ' + $_.FullName) }
    throw 'Resolve alternate script locations before installing. Do not delete or move scripts blindly; preserve their .meta files. No changes made.'
}
Write-Host ('Verified {0} source files; {1} changes.' -f $rows.Count,$pending.Count)
if ($DryRun) { Write-Host 'Dry run complete. No project files changed.'; return }
if ($pending.Count -eq 0) { Write-Host 'All packaged files already match.'; return }
$backup = Join-Path $root ('SparkV8Neon_CompleteRepair_Backups\'+(Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
foreach ($item in $pending) {
    if ($item.Exists) {
        $bk=Join-Path $backup $item.Relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $bk) -Force | Out-Null
        Copy-Item -LiteralPath $item.Destination -Destination $bk
    }
}
$written=New-Object System.Collections.Generic.List[object]
try {
    foreach ($item in $pending) {
        New-Item -ItemType Directory -Path (Split-Path -Parent $item.Destination) -Force | Out-Null
        $written.Add($item); Copy-Item -LiteralPath $item.Source -Destination $item.Destination -Force
        if ((Get-FileHash -LiteralPath $item.Destination -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $item.Source -Algorithm SHA256).Hash) { throw ('Destination verification failed: ' + $item.Relative) }
    }
} catch {
    foreach ($item in $written) {
        if ($item.Exists) { Copy-Item -LiteralPath (Join-Path $backup $item.Relative) -Destination $item.Destination -Force }
        elseif (Test-Path -LiteralPath $item.Destination -PathType Leaf) { Remove-Item -LiteralPath $item.Destination -Force }
    }
    throw
}
Write-Host ('Installed. Original source backups, if any: {0}' -f $backup) -ForegroundColor Green
