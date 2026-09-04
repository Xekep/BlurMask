param(
    [ValidateSet('win-x64','win-arm64','linux-x64','linux-arm64','osx-x64','osx-arm64')]
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'

$isWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
$isLinux = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)
$isMacOS = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)

$valid = ($isWindows -and $Runtime.StartsWith('win-')) -or
         ($isLinux -and $Runtime.StartsWith('linux-')) -or
         ($isMacOS -and $Runtime.StartsWith('osx-'))

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

$publishDir = Join-Path $PSScriptRoot "bin\Release\net11.0\$Runtime\publish"

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
