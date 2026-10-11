param(
    [ValidateSet('win-x64','win-arm64','linux-x64','linux-arm64','osx-x64','osx-arm64')]
    [string]$Runtime = 'win-x64',
    [string]$Version,
    [string]$OutputDirectory,
    [switch]$DynamicNativeLibraries
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent

$runningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
$runningOnLinux = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)
$runningOnMacOS = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)

$valid = ($runningOnWindows -and $Runtime.StartsWith('win-')) -or
         ($runningOnLinux -and $Runtime.StartsWith('linux-')) -or
         ($runningOnMacOS -and $Runtime.StartsWith('osx-'))

if (-not $valid) {
    throw "Native AOT does not support cross-OS publishing. Requested RID: $Runtime"
}

Write-Host "Publishing BlurMask (.NET 11 Native AOT) for $Runtime..."
$publishArguments = @('publish', (Join-Path $repoRoot 'src/BlurMask/BlurMask.csproj'), '-c', 'Release',
    '-r', $Runtime, '--self-contained', 'true', '-p:PublishAot=true')
if ($Version) {
    if ($Version -notmatch '^\d+\.\d+\.\d+(?:\.\d+)?(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$') { throw 'Invalid application version.' }
    $publishArguments += "-p:Version=$Version"
}
if ($OutputDirectory) {
    $publishArguments += @('-o', [IO.Path]::GetFullPath($OutputDirectory))
    # Keep installer/custom builds separate from a running binary in bin/Release.
    $publishArguments += "-p:OutputPath=$(Join-Path $repoRoot ".artifacts\build-output\$Runtime\")"
}
if ($Runtime.StartsWith('win-')) { $publishArguments += "-p:StaticNativeLibraries=$((-not $DynamicNativeLibraries).ToString().ToLowerInvariant())" }
Push-Location $repoRoot
try { dotnet @publishArguments }
finally { Pop-Location }

if ($LASTEXITCODE -ne 0) {
    throw "Native AOT publish failed with exit code $LASTEXITCODE."
}

$targetFramework = "net11.0"
$publishDir = Join-Path $repoRoot "src\BlurMask\bin\Release\$targetFramework\$Runtime\publish"
if ($OutputDirectory) { $publishDir = [IO.Path]::GetFullPath($OutputDirectory) }

if ($Runtime.StartsWith('win-')) {
    $required = @('BlurMask.exe')
    if ($DynamicNativeLibraries) { $required += @('av_libglesv2.dll', 'libSkiaSharp.dll', 'libHarfBuzzSharp.dll') }
    foreach ($name in $required) {
        $path = Join-Path $publishDir $name
        if (-not (Test-Path $path)) {
            throw "Native AOT publish is incomplete. Missing required Avalonia dependency: $name"
        }
    }

    if (-not $DynamicNativeLibraries) {
        if (Get-ChildItem -LiteralPath $publishDir -Filter '*.dll') { throw 'Static Native AOT publish unexpectedly contains DLLs.' }
        Write-Host 'Verified Native AOT application executable. Rendering libraries are statically linked.'
    }
    else { Write-Host 'Verified dynamic native dependencies. Keep the entire publish directory together.' }
}

if ($Runtime -eq 'win-x64') {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere)) { throw 'Visual Studio locator is required to package the licensed VC runtime.' }
    $installations = @(& $vswhere -all -prerelease -products '*' -property installationPath)
    $redists = foreach ($installation in $installations) {
        $redistRoot = Join-Path $installation 'VC/Redist/MSVC'
        if (Test-Path -LiteralPath $redistRoot) {
            Get-ChildItem -Path (Join-Path $redistRoot '*/x64/Microsoft.VC*.CRT') -Directory
        }
    }
    $redist = $redists | Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'vcruntime140_1.dll') } |
        Sort-Object FullName -Descending | Select-Object -First 1
    if (-not $redist) { throw 'The Visual Studio x64 redistributable runtime directory was not found.' }
    python (Join-Path $repoRoot 'tools/fetch-ink-math-runtime.py') $Runtime --output $publishDir --vc-redist-dir $redist.FullName
    if ($LASTEXITCODE -ne 0) { throw 'Formula runtime packaging failed.' }
}
elseif ($Runtime -in @('linux-x64', 'osx-x64', 'osx-arm64')) {
    python3 (Join-Path $repoRoot 'tools/fetch-ink-math-runtime.py') $Runtime --output $publishDir
    if ($LASTEXITCODE -ne 0) { throw 'Formula runtime packaging failed.' }
}
else { Write-Warning "Formula runtime is not packaged for $Runtime; handwritten ink and text recognition remain available." }

if ($Runtime -in @('win-x64', 'linux-x64', 'osx-x64', 'osx-arm64')) {
    $solverDirectory = Join-Path $repoRoot ".artifacts/giac-runtime/$Runtime"
    $sourcesDirectory = Join-Path $repoRoot '.artifacts/installers'
    if ($Runtime -eq 'win-x64') {
        & (Join-Path $PSScriptRoot 'prepare-giac-runtime.ps1') -OutputDirectory $solverDirectory -SourcesDirectory $sourcesDirectory | Out-Host
    }
    else {
        & bash (Join-Path $PSScriptRoot 'prepare-giac-runtime.sh') $Runtime $solverDirectory $sourcesDirectory
        if ($LASTEXITCODE -ne 0) { throw 'Giac runtime packaging failed.' }
    }
    $solverDestination = Join-Path $publishDir "runtimes/$Runtime/solver"
    New-Item -ItemType Directory -Path $solverDestination -Force | Out-Null
    Copy-Item -Path (Join-Path $solverDirectory '*') -Destination $solverDestination -Recurse -Force
}
else { Write-Warning "Bundled Giac is not available for $Runtime." }

Write-Host "Published to: $publishDir"
