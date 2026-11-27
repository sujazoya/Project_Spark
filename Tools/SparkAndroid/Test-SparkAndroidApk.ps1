param(
    [Parameter(Mandatory)]
    [string] $Apk,
    [Parameter(Mandatory)]
    [string] $UnityAndroidPlayer,
    [switch] $RequireDevelopment
)

$ErrorActionPreference = 'Stop'
$package = Get-Item -LiteralPath $Apk
$toolchain = Get-ChildItem -LiteralPath (Join-Path $UnityAndroidPlayer 'SDK/build-tools') -Directory |
    Where-Object { (Test-Path (Join-Path $_.FullName 'aapt2.exe')) -and
        (Test-Path (Join-Path $_.FullName 'lib/apksigner.jar')) } |
    Sort-Object -Property @{Expression = { [version]($_.Name -replace '-.*$','') }} -Descending |
    Select-Object -First 1
if (!$toolchain) { throw 'No installed Android build tools provide aapt2 and apksigner.' }

$java = Join-Path $UnityAndroidPlayer 'OpenJDK/bin/java.exe'
$signature = Join-Path $toolchain.FullName 'lib/apksigner.jar'
$aapt = Join-Path $toolchain.FullName 'aapt2.exe'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($package.FullName)
try {
    foreach ($entry in 'AndroidManifest.xml','classes.dex','lib/arm64-v8a/libunity.so','lib/arm64-v8a/libil2cpp.so') {
        if (!$zip.GetEntry($entry) -or $zip.GetEntry($entry).Length -eq 0) {
            throw "Required APK entry missing or empty: $entry"
        }
    }
    if (!($zip.Entries | Where-Object { $_.FullName -like 'assets/bin/Data/*' } | Select-Object -First 1)) {
        throw 'Unity game data is missing from the APK.'
    }
    $abis = @($zip.Entries | ForEach-Object {
        if ($_.FullName -match '^lib/([^/]+)/[^/]+\.so$') { $Matches[1] }
    } | Sort-Object -Unique)
    if ($abis.Count -ne 1 -or $abis[0] -ne 'arm64-v8a') { throw "Unexpected ABI list: $abis" }
    foreach ($library in $zip.Entries | Where-Object { $_.FullName -match '^lib/arm64-v8a/[^/]+\.so$' }) {
        $binary = [IO.BinaryReader]::new($library.Open())
        try { $header = $binary.ReadBytes(20) }
        finally { $binary.Dispose() }
        if ($header.Length -ne 20 -or $header[0] -ne 0x7f -or
            [Text.Encoding]::ASCII.GetString($header, 1, 3) -ne 'ELF' -or
            $header[4] -ne 2 -or $header[5] -ne 1 -or $header[18] -ne 183 -or $header[19] -ne 0) {
            throw "Native library is not a little-endian ARM64 ELF binary: $($library.FullName)"
        }
    }
}
finally { $zip.Dispose() }

$badging = & $aapt dump badging $package.FullName
if ($LASTEXITCODE -ne 0 -or !($badging -match '^launchable-activity:')) {
    throw 'The packaged manifest has no readable launch activity.'
}
if (!($badging -match "uses-permission: name='android.permission.RECORD_AUDIO'")) {
    throw 'Microphone permission is missing from the packaged manifest.'
}
if ($RequireDevelopment -and !($badging -match '^application-debuggable')) {
    throw 'The APK is missing the development debug flag.'
}
$manifest = & $aapt dump xmltree --file AndroidManifest.xml $package.FullName
if ($LASTEXITCODE -ne 0 -or !($manifest -match 'android.speech.RecognitionService') -or
    !($manifest -match 'android.intent.action.TTS_SERVICE')) {
    throw 'Packaged recognition or TTS service visibility is missing.'
}
& $java -jar $signature verify --verbose --print-certs $package.FullName
if ($LASTEXITCODE -ne 0) { throw 'APK signature verification failed.' }

"PASS Android APK archive, ARM64 libraries, launch activity, microphone permission, and speech service visibility."
$badging | Where-Object {
    $_ -match '^(package:|sdkVersion:|targetSdkVersion:|launchable-activity:|native-code:|application-debuggable)'
}
"APK $($package.FullName)"
"Size $($package.Length) bytes, modified $($package.LastWriteTime.ToString('O'))"
"SHA256 $((Get-FileHash -LiteralPath $package.FullName -Algorithm SHA256).Hash)"
