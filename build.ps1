param(
    [ValidateSet('win-x64','win-arm64','linux-x64','linux-arm64','osx-x64','osx-arm64')]
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'

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
dotnet publish .\BlurMask.csproj `
    -c Release `
    -r $Runtime `
    --self-contained true `
    -p:PublishAot=true

if ($LASTEXITCODE -ne 0) {
    throw "Native AOT publish failed with exit code $LASTEXITCODE."
}

$targetFramework = "net11.0"
$publishDir = Join-Path $PSScriptRoot "bin\Release\$targetFramework\$Runtime\publish"

if ($Runtime.StartsWith('win-')) {
    $required = @('av_libglesv2.dll', 'libSkiaSharp.dll', 'libHarfBuzzSharp.dll')
    foreach ($name in $required) {
        $path = Join-Path $publishDir $name
        if (-not (Test-Path $path)) {
            throw "Native AOT publish is incomplete. Missing required Avalonia dependency: $name"
        }
    }

    Write-Host "Verified Avalonia native dependencies. Keep the entire publish directory together."
}

Write-Host "Published to: $publishDir"
