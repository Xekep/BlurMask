param(
    [string]$ToolsDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) '.artifacts\tools')
)

$ErrorActionPreference = 'Stop'
if (-not [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    throw 'Inno Setup requires Windows.'
}

# Latest stable release verified at https://jrsoftware.org/isdl.php on 2026-10-10.
# Hash from the immutable official GitHub release asset. Update both together.
$innoVersion = '7.1.0'
$expectedHash = '0362a383ed217d4c4239b5933866dd96d3eb2102737da92f80f6057a4b40df2f'
$toolsRoot = [IO.Path]::GetFullPath($ToolsDirectory)
$innoDirectory = Join-Path $toolsRoot "inno-setup-$innoVersion-x64"
$compiler = Join-Path $innoDirectory 'ISCC.exe'
if (Test-Path -LiteralPath $compiler) {
    if ((& $compiler --version).Trim() -ne $innoVersion -or $LASTEXITCODE -ne 0) {
        throw "Unexpected compiler version at $compiler"
    }
    Write-Output $compiler
    return
}

New-Item -ItemType Directory -Path $toolsRoot -Force | Out-Null
$download = Join-Path $toolsRoot "innosetup-$innoVersion-x64.exe"
$url = "https://github.com/jrsoftware/issrc/releases/download/is-7_1_0/innosetup-$innoVersion-x64.exe"
if (-not (Test-Path -LiteralPath $download)) {
    Write-Host "Downloading Inno Setup $innoVersion x64 from the official release..."
    Invoke-WebRequest -Uri $url -OutFile $download
}
if ((Get-FileHash -LiteralPath $download -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expectedHash) {
    throw "Inno Setup download SHA-256 mismatch. Remove $download and retry."
}

Write-Host "Installing verified Inno Setup $innoVersion in $innoDirectory..."
$arguments = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', '/CURRENTUSER', '/NOICONS',
    '/TASKS=""', "/DIR=`"$innoDirectory`"", "/LOG=`"$(Join-Path $toolsRoot 'inno-setup-install.log')`"")
$installation = Start-Process -FilePath $download -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($installation.ExitCode -ne 0) { throw "Inno Setup installation failed: $($installation.ExitCode)" }
if (-not (Test-Path -LiteralPath $compiler)) { throw "ISCC.exe missing after installation: $compiler" }
if ((& $compiler --version).Trim() -ne $innoVersion -or $LASTEXITCODE -ne 0) {
    throw "Unexpected compiler version at $compiler"
}
Write-Output $compiler
