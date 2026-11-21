# Project Spark Level 113 RDX21 - source-verified standalone CSV parser hotfix.
# Verified against original RDX21 kit: 206 model manifest rows; 8 quoted-comma rows (J1-J8).
# All 206 manifest OBJ models and 207_L113_AssembledReference.obj exist in the original archive.
# Windows PowerShell 5.1 compatible. Requires ONLY this .ps1 file.
# Patches an installed Level 113 C# layout builder only if a known unsafe CSV reader is found.
# Creates an external backup; does not edit manifests, models, scenes or .meta files.
[CmdletBinding()]
param(
    [string]$UnityProjectPath = 'D:\Project_Spark_Master_Project\Project_Spark',
    [string]$BuilderPath = '',
    [switch]$DryRun
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

function Get-SingleFile([string]$root, [string]$fileName, [string]$description) {
    if (-not (Test-Path -LiteralPath $root -PathType Container)) { return $null }
    $hits = @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter $fileName -ErrorAction Stop)
    if ($hits.Count -gt 1) {
        $paths = ($hits | ForEach-Object { $_.FullName }) -join "`r`n"
        throw "Multiple $description files found. No changes made. Specify one or remove duplicate kits:`r`n$paths"
    }
    if ($hits.Count -eq 1) { return $hits[0].FullName }
    return $null
}

$project = [System.IO.Path]::GetFullPath($UnityProjectPath)
$assets = Join-Path $project 'Assets'
$settings = Join-Path $project 'ProjectSettings'
if (-not (Test-Path -LiteralPath $assets -PathType Container) -or
    -not (Test-Path -LiteralPath $settings -PathType Container)) {
    throw "Unity project not found: $project"
}

$levelFolder = Join-Path $assets 'ProjectSpark\Level113'
$builderName = 'SparkL113FullLayoutBuilder.cs'
$manifestName = 'L113_ModelManifest.csv'
if ($BuilderPath -ne '') {
    $builder = [System.IO.Path]::GetFullPath($BuilderPath)
    if (-not (Test-Path -LiteralPath $builder -PathType Leaf)) { throw "Builder does not exist: $builder" }
    if ([System.IO.Path]::GetFileName($builder) -ne $builderName) {
        throw "BuilderPath must point to $builderName. No changes made."
    }
    if (-not $builder.StartsWith(($assets + [System.IO.Path]::DirectorySeparatorChar), [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'BuilderPath is outside this Unity project Assets directory. No changes made.'
    }
}
else {
    $builder = Get-SingleFile $levelFolder $builderName 'Level 113 builder'
    if ($null -eq $builder) { $builder = Get-SingleFile $assets $builderName 'Level 113 builder' }
    if ($null -eq $builder) {
        throw "Cannot find $builderName under $assets. No changes made."
    }
}

$manifest = Get-SingleFile $levelFolder $manifestName 'Level 113 model manifest'
if ($null -eq $manifest) { $manifest = Get-SingleFile $assets $manifestName 'Level 113 model manifest' }
if ($null -eq $manifest) {
    throw "Cannot find $manifestName in the Unity Assets folder. No changes made."
}
Write-Host "[L113] Builder: $builder"
Write-Host "[L113] Manifest: $manifest"

# Inspect the actual imported data. Do not assume fixed row counts or column offsets.
$rows = @(Import-Csv -LiteralPath $manifest)
if ($rows.Count -eq 0) { throw '[L113] Model manifest has no records; no changes made.' }
$columnNames = @($rows[0].PSObject.Properties.Name)
$objHeaders = @($columnNames | Where-Object { $_.Trim() -ieq 'obj' })
if ($objHeaders.Count -ne 1) {
    throw '[L113] Expected exactly one obj header in the model manifest. No changes made.'
}
$objHeader = $objHeaders[0]
# This original builder uses zero-based model field [6]; refuse to patch a reordered manifest.
$objIndex = [Array]::IndexOf($columnNames, $objHeader)
if ($objIndex -ne 6) {
    throw "[L113] The original builder expects obj at column index 6, but found index $objIndex. No changes made."
}
if ($rows.Count -ne 206) {
    throw "[L113] Expected 206 source-model records from the RDX21 original kit, found $($rows.Count). No changes made."
}
$invalid = New-Object 'System.Collections.Generic.List[string]'
$missing = New-Object 'System.Collections.Generic.List[string]'
$sourceRoot = Split-Path -Parent (Split-Path -Parent $manifest)
$modelFolder = Join-Path $sourceRoot 'Models_OBJ'
foreach ($row in $rows) {
    $objName = [string]$row.PSObject.Properties[$objHeader].Value
    $objName = $objName.Trim()
    if ([string]::IsNullOrWhiteSpace($objName) -or
        $objName -ieq 'NOT_INCLUDED' -or
        -not $objName.EndsWith('.obj', [System.StringComparison]::OrdinalIgnoreCase) -or
        $objName.IndexOf('/') -ge 0 -or $objName.IndexOf('\') -ge 0 -or
        $objName.IndexOf('..', [System.StringComparison]::Ordinal) -ge 0) {
        [void]$invalid.Add($objName)
    }
    elseif (-not (Test-Path -LiteralPath (Join-Path $modelFolder $objName) -PathType Leaf)) {
        [void]$missing.Add($objName)
    }
}
if ($invalid.Count -gt 0) {
    throw "[L113] Manifest itself has $($invalid.Count) invalid obj values (first: '$($invalid[0])'). No changes made. Inspect L113_ModelManifest.csv."
}
Write-Host "[L113] $($rows.Count) model records parsed; OBJ header is column 6. Quoted commas in J1-J8 are handled." -ForegroundColor Cyan
if ($missing.Count -gt 0) {
    Write-Warning "[L113] $($missing.Count) referenced OBJ files are missing under $modelFolder. First: $($missing[0]). This script cannot recreate model assets."
}
else {
    Write-Host "[L113] All $($rows.Count) model OBJ paths exist on disk." -ForegroundColor Green
}

$content = [System.IO.File]::ReadAllText($builder)
if ($content -notmatch 'class\s+SparkL113FullLayoutBuilder\b') {
    throw '[L113] Unexpected C# class name; refusing to patch an unrelated script.'
}
if (-not $content.Contains('models[i][6]') -or -not $content.Contains('L113_ModelManifest.csv')) {
    throw '[L113] Installed builder does not match the expected RDX21 OBJ-column layout; no changes made.'
}
if ($content -notmatch 'ReadCsv\s*\(') {
    throw '[L113] ReadCsv method not found. This builder uses a different parser; no changes made.'
}

$signature = 'private static string[] ParseCsvFields(string line)'
$old = "result.Add(lines[i].Split(','));"
$replacement = 'result.Add(ParseCsvFields(lines[i]));'
$anchor = 'private static List<string[]> ReadCsv(string assetPath)'
$alreadyFixed = $content.Contains($signature) -and $content.Contains($replacement) -and (-not $content.Contains($old))

if ($alreadyFixed) {
    Write-Host '[L113] Quote-safe CSV parser is already installed. No C# changes needed.' -ForegroundColor Green
}
else {
    if ($content.Contains($signature) -or $content.Contains($replacement)) {
        throw '[L113] Partial or conflicting CSV parser fix detected. No changes made; inspect the builder.'
    }
    if ($content.Split([string[]]@($old), [System.StringSplitOptions]::None).Count -ne 2) {
        throw "[L113] Known unsafe reader '$old' not found exactly once. No changes made. Please provide the Level 113 builder for a tailored fix."
    }
    if ($content.Split([string[]]@($anchor), [System.StringSplitOptions]::None).Count -ne 2) {
        throw '[L113] Safe ReadCsv insertion point was not found exactly once. No changes made.'
    }

    # C# CSV row parser: quoted commas, escaped double quotes, trailing empty columns.
    # The source read loop is preserved; quoted newlines are not handled.
    $helper = @'
        // Parse a CSV row without splitting commas inside quoted descriptions.
        // Handles doubled quote escapes and empty trailing fields.
        private static string[] ParseCsvFields(string line)
        {
            List<string> fields = new List<string>(16);
            System.Text.StringBuilder field = new System.Text.StringBuilder(line.Length);
            bool inQuotes = false;
            for (int i = 0; i < line.Length; ++i)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        field.Append('"');
                        ++i;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(field.ToString());
                    field.Length = 0;
                }
                else
                {
                    field.Append(c);
                }
            }
            if (inQuotes)
                throw new FormatException("Unclosed quoted CSV field.");
            fields.Add(field.ToString());
            return fields.ToArray();
        }
'@
    $revised = $content.Replace($old, $replacement)
    $revised = $revised.Replace($anchor, $helper + "`r`n        " + $anchor)
    $revised = $revised.Replace(
        '// These source CSVs are deliberately simple with no quoted delimiters.',
        '// Source CSV rows may contain quoted commas (notably J1-J8 in the model manifest).')
    if ($revised -eq $content -or -not $revised.Contains($signature) -or
        -not $revised.Contains($replacement) -or $revised.Contains($old)) {
        throw '[L113] Internal patch verification failed. No changes made.'
    }

    if ($DryRun) {
        Write-Host "[L113] DRY RUN: would patch $builder" -ForegroundColor Yellow
        Write-Host '[L113] No files were modified.'
    }
    else {
        $backupDir = Join-Path $project ('ProjectSpark_Backups\Level113_CSV_' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        $backupFile = Join-Path $backupDir $builderName
        Copy-Item -LiteralPath $builder -Destination $backupFile -ErrorAction Stop
        try {
            $encoding = [System.Text.UTF8Encoding]::new($false)
            [System.IO.File]::WriteAllText($builder, $revised, $encoding)
            if ([System.IO.File]::ReadAllText($builder) -cne $revised) {
                throw 'Write verification mismatch.'
            }
        }
        catch {
            Copy-Item -LiteralPath $backupFile -Destination $builder -Force
            throw "[L113] Failed to write patch; restored original builder. Details: $_"
        }
        Write-Host '[L113] CSV parser patched successfully.' -ForegroundColor Green
        Write-Host "[L113] Original builder backup: $backupFile"
    }
}

if ($content.Contains('ResolveObjFileName(')) {
    Write-Host '[L113] Builder contains ResolveObjFileName; check Unity validation for correct OBJ selection.'
}
elseif ($content.Contains('GetManifestColumn(')) {
    Write-Host '[L113] Builder contains GetManifestColumn; check Unity validation for correct OBJ selection.'
}
else {
    Write-Host '[L113] OBJ field index 6 verified against header and source builder.' -ForegroundColor Cyan
}
Write-Host '[L113] No model assets, manifests, scenes, Unity .meta files, or other levels were changed.'
Write-Host '[L113] Reopen Unity, wait for compilation, then run the Level 113 Validate Sources menu item.'
