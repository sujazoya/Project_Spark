# Project Spark Level 111 - standalone quote-safe CSV parser fix.
# Requires only this PS1. Windows PowerShell 5.1 compatible.
# Patches the installed Unity builder; preserves all Level 111 models, scenes and .meta files.
[CmdletBinding()]
param(
    [string]$UnityProjectPath = 'D:\Project_Spark_Master_Project\Project_Spark',
    [switch]$DryRun
)
$ErrorActionPreference = 'Stop'

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$assets = Join-Path $project 'Assets'
$settings = Join-Path $project 'ProjectSettings'
if (-not (Test-Path -LiteralPath $assets -PathType Container) -or
    -not (Test-Path -LiteralPath $settings -PathType Container)) {
    throw "Unity project not found: $project"
}

$root = Join-Path $assets 'ProjectSpark\Level111\SIX19'
$builder = Join-Path $root 'Unity\Editor\SparkL111FullLayoutBuilder.cs'
$manifest = Join-Path $root 'Data\L111_ModelManifest.csv'
if (-not (Test-Path -LiteralPath $builder -PathType Leaf)) {
    throw "Unity builder missing: $builder"
}
if (-not (Test-Path -LiteralPath $manifest -PathType Leaf)) {
    throw "Level 111 manifest missing: $manifest"
}

# Validate the actual imported manifest with PowerShell's quote-aware parser.
$rows = @(Import-Csv -LiteralPath $manifest)
if ($rows.Count -ne 176) {
    throw "Manifest record count unexpected: $($rows.Count). Expected 176; no files changed."
}
$problem = $rows | Where-Object { $_.asset_id -eq 'RetimerHeatsink' } | Select-Object -First 1
if ($null -eq $problem -or $problem.obj -ne '171_L111_RetimerHeatsink.obj') {
    throw 'RetimerHeatsink OBJ field differs from expected. No files changed; inspect the manifest.'
}

# Confirm this source file is the known header-aware OBJ builder.
$content = [System.IO.File]::ReadAllText($builder)
if (-not $content.Contains('GetManifestColumn(source, "obj")')) {
    throw "Builder does not read the OBJ manifest header. No files changed. Use the complete Level 111 CSV-parser-fixed kit instead."
}
$old = "result.Add(lines[i].Split(','));"
$new = 'result.Add(ParseCsvFields(lines[i]));'
$anchor = '        private static float F('
$signature = 'private static string[] ParseCsvFields(string line)'

if ($content.Contains($signature)) {
    if ($content.Contains($old)) {
        throw 'Builder has conflicting CSV parsing implementations. No files changed.'
    }
    Write-Host '[L111] Quote-aware CSV parser already installed. No builder change needed.' -ForegroundColor Green
}
else {
    if (-not $content.Contains($old)) {
        throw 'Expected Split-based CSV reader not found. No files changed; avoid replacing customized code.'
    }
    if (-not $content.Contains($anchor)) {
        throw 'Safe helper insertion point not found. No files changed; avoid replacing customized code.'
    }

    # C# helper handles quoted commas and doubled quotes; no external libraries.
    $helper = @'
        // Parse single-line CSV records including commas within quotes and doubled quotes.
        private static string[] ParseCsvFields(string line)
        {
            List<string> fields = new List<string>(16);
            System.Text.StringBuilder field = new System.Text.StringBuilder(line.Length);
            bool insideQuotes = false;
            for (int i = 0; i < line.Length; ++i)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        field.Append('"');
                        ++i;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (c == ',' && !insideQuotes)
                {
                    fields.Add(field.ToString());
                    field.Length = 0;
                }
                else
                {
                    field.Append(c);
                }
            }
            if (insideQuotes)
                throw new FormatException("Unclosed quoted field.");
            fields.Add(field.ToString());
            return fields.ToArray();
        }

'@
    $revised = $content.Replace($old, $new)
    $revised = $revised.Replace($anchor, $helper + "`r`n" + $anchor)
    if ($revised -eq $content -or -not $revised.Contains($signature) -or $revised.Contains($old)) {
        throw 'Patch self-check failed. No files changed.'
    }

    if ($DryRun) {
        Write-Host "[DRY RUN] Would patch: $builder" -ForegroundColor Yellow
    }
    else {
        $backupDir = Join-Path $project ('ProjectSpark_Backups\Level111_CSV_Standalone_' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        $backupFile = Join-Path $backupDir 'SparkL111FullLayoutBuilder.cs'
        Copy-Item -LiteralPath $builder -Destination $backupFile
        $encoding = [System.Text.UTF8Encoding]::new($false)
        [System.IO.File]::WriteAllText($builder, $revised, $encoding)
        $installed = [System.IO.File]::ReadAllText($builder)
        if ($installed -ne $revised) {
            Copy-Item -LiteralPath $backupFile -Destination $builder -Force
            throw 'Write verification failed; original builder restored.'
        }
        Write-Host '[L111] CSV parser patched successfully.' -ForegroundColor Green
        Write-Host "[L111] Backup: $backupFile"
    }
}

$modelsPath = Join-Path $root 'Models_OBJ'
$missing = @($rows | Where-Object { -not (Test-Path -LiteralPath (Join-Path $modelsPath $_.obj) -PathType Leaf) })
if ($missing.Count -gt 0) {
    Write-Warning "[L111] $($missing.Count) manifest-listed OBJ files are missing. First: $($missing[0].obj)"
}
else {
    Write-Host '[L111] All 176 manifest-listed OBJ files are present.' -ForegroundColor Green
}
Write-Host '[L111] The script did not change the manifest, models, other levels, Unity scene or .meta files.'
Write-Host '[L111] Reopen Unity and run: Tools > Project Spark > Automatic Level Generator > 111 > 3. Validate Sources and Report Blockers.'
