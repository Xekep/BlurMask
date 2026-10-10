param(
    [string]$Version,
    [string]$PublishDirectory = (Join-Path $PSScriptRoot '.artifacts\installer-publish\win-x64'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '.artifacts\installers'),
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
$required = @('BlurMask.exe', 'av_libglesv2.dll', 'libSkiaSharp.dll', 'libHarfBuzzSharp.dll')
foreach ($name in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $publishDir $name))) { throw "Incomplete publish directory: missing $name" }
}
$compiler = & (Join-Path $PSScriptRoot 'tools\install-inno-setup.ps1')
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
& $compiler '--quiet-progress' "--define=AppVersion=$Version" "--define=NumericVersion=$numericVersion" "--define=PublishDir=$publishDir" "--define=InstallerOutputDir=$outputDir" (Join-Path $PSScriptRoot 'installer\BlurMask.iss')
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed: $LASTEXITCODE" }
$installer = Join-Path $outputDir "BlurMask-$Version-win-x64-Setup.exe"
if (-not (Test-Path -LiteralPath $installer)) { throw "Installer not created: $installer" }
$hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($installer))" | Set-Content -LiteralPath "$installer.sha256" -Encoding ascii
@{
    product = 'BlurMask'; version = $Version; runtime = 'win-x64'; innoSetup = '7.1.0';
    commit = $env:GITHUB_SHA; sha256 = $hash
} | ConvertTo-Json | Set-Content -LiteralPath "$installer.json" -Encoding utf8
Write-Host "Installer created: $installer"
Write-Output $installer
