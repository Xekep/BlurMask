param(
    [string]$Version,
    [string]$PublishDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) '.artifacts\installer-publish\win-x64'),
    [string]$OutputDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) '.artifacts\installers'),
    [string]$SolverDirectory,
    [string]$SolverSourcesArchive,
    [switch]$SkipPublish
)

$ErrorActionPreference = 'Stop'
if (-not [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    throw 'The Windows installer must be built on Windows.'
}
if ([string]::IsNullOrWhiteSpace($Version)) {
    if ($env:GITHUB_REF_TYPE -eq 'tag') { $Version = $env:GITHUB_REF_NAME -replace '^v', '' }
    elseif ($env:GITHUB_RUN_NUMBER) { $Version = "1.0.0-dev.$($env:GITHUB_RUN_NUMBER)" }
    else { $Version = '1.0.0' }
}
if ($Version -notmatch '^(?<core>\d+\.\d+\.\d+(?:\.\d+)?)(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$') {
    throw 'Version must be numeric (e.g. 1.2.3), optionally with a prerelease suffix (e.g. 1.2.3-rc.1).'
}
$parts = $Matches.core.Split('.')
if ($parts | Where-Object { [long]$_ -gt 65535 }) { throw 'Version components must be at most 65535.' }
$numericVersion = ($parts + @('0') * (4 - $parts.Count)) -join '.'
$publishDir = [IO.Path]::GetFullPath($PublishDirectory)
$outputDir = [IO.Path]::GetFullPath($OutputDirectory)
if (-not $SkipPublish) {
    & (Join-Path $PSScriptRoot 'build.ps1') -Runtime win-x64 -Version $Version -OutputDirectory $publishDir
}
$required = @('BlurMask.exe')
foreach ($name in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $publishDir $name))) { throw "Incomplete publish directory: missing $name" }
}
python (Join-Path $PSScriptRoot 'package-experimental-math-model.py') --output $publishDir --rid win-x64 --verify-only
if ($LASTEXITCODE -ne 0) { throw 'Bundled experimental Math2 assets/runtime missing or invalid.' }
$nativeDlls = @(Get-ChildItem -LiteralPath $publishDir -Filter '*.dll')
if ($nativeDlls.Count -ne 0) { throw 'The Windows installer expects the single-executable Native AOT build.' }
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
if ([string]::IsNullOrWhiteSpace($SolverDirectory)) {
    $SolverDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) '.artifacts\giac-runtime\win-x64'
    & (Join-Path $PSScriptRoot 'prepare-giac-runtime.ps1') -OutputDirectory $SolverDirectory -SourcesDirectory $outputDir | Out-Host
}
$solverDir = (Resolve-Path -LiteralPath $SolverDirectory).Path
foreach ($name in @('icas.exe', 'COPYING', 'THIRD-PARTY-NOTICES.txt', 'manifest.json', 'SOURCES.txt')) {
    if (-not (Test-Path -LiteralPath (Join-Path $solverDir $name) -PathType Leaf)) { throw "Incomplete bundled solver: missing $name" }
}
$solverManifest = Get-Content -LiteralPath (Join-Path $solverDir 'manifest.json') -Raw | ConvertFrom-Json
$solverHash = (Get-FileHash -LiteralPath (Join-Path $solverDir 'icas.exe') -Algorithm SHA256).Hash.ToLowerInvariant()
if ($solverManifest.runtime -ne 'win-x64' -or $solverManifest.static -ne $true -or $solverManifest.executableSha256 -ne $solverHash -or
    (Get-ChildItem -LiteralPath $solverDir -Filter '*.dll' -Recurse)) {
    throw 'Bundled solver manifest does not match the Windows runtime.'
}
$sourceName = [string]$solverManifest.sourceArchive
if ([string]::IsNullOrWhiteSpace($sourceName) -or [IO.Path]::GetFileName($sourceName) -ne $sourceName -or -not $sourceName.EndsWith('.tar.gz')) {
    throw 'Bundled solver must identify its corresponding source release archive.'
}
if ([string]::IsNullOrWhiteSpace($SolverSourcesArchive)) { $SolverSourcesArchive = Join-Path $outputDir $sourceName }
$sourceArchive = (Resolve-Path -LiteralPath $SolverSourcesArchive).Path
$sourceHash = (Get-FileHash -LiteralPath $sourceArchive -Algorithm SHA256).Hash.ToLowerInvariant()
if ($solverManifest.sourceArchiveSha256 -ne $sourceHash -or
    -not (Get-Content -LiteralPath (Join-Path $solverDir 'SOURCES.txt') -Raw).Contains($sourceHash)) {
    throw 'Corresponding source archive differs from the source hash shipped with the runtime.'
}
$releaseSourceArchive = Join-Path $outputDir $sourceName
if ($sourceArchive -ne $releaseSourceArchive) { Copy-Item -LiteralPath $sourceArchive -Destination $releaseSourceArchive -Force }
"$sourceHash  $sourceName" | Set-Content -LiteralPath "$releaseSourceArchive.sha256" -Encoding ascii
$compiler = & (Join-Path $PSScriptRoot 'install-inno-setup.ps1')
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
& $compiler '--quiet-progress' "--define=AppVersion=$Version" "--define=NumericVersion=$numericVersion" "--define=PublishDir=$publishDir" "--define=SolverDir=$solverDir" "--define=InstallerOutputDir=$outputDir" (Join-Path (Split-Path $PSScriptRoot -Parent) 'packaging\BlurMask.iss')
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed: $LASTEXITCODE" }
$installer = Join-Path $outputDir "BlurMask-$Version-win-x64-Setup.exe"
if (-not (Test-Path -LiteralPath $installer)) { throw "Installer not created: $installer" }
$hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($installer))" | Set-Content -LiteralPath "$installer.sha256" -Encoding ascii
@{
    product = 'BlurMask'; version = $Version; runtime = 'win-x64'; innoSetup = '7.1.0';
    commit = $env:GITHUB_SHA; sha256 = $hash
    solver = @{ giacCommit = $solverManifest.giacCommit; executableSha256 = $solverHash; sourceArchive = $sourceName; sourceSha256 = $sourceHash }
} | ConvertTo-Json | Set-Content -LiteralPath "$installer.json" -Encoding utf8
Write-Host "Installer created: $installer"
Write-Output $installer
