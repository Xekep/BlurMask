param(
    [Parameter(Mandatory = $true)][string]$InstallerPath,
    [Parameter(Mandatory = $true)][string]$PublishDirectory,
    [switch]$SkipLaunch
)

$ErrorActionPreference = 'Stop'
$installer = (Resolve-Path -LiteralPath $InstallerPath).Path
$payload = (Resolve-Path -LiteralPath $PublishDirectory).Path
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{A78C0733-F377-4CC3-91D0-D2621D4532CC}_is1'
$runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$menuLink = Join-Path ([Environment]::GetFolderPath('Programs')) 'BlurMask.lnk'
$desktopLink = Join-Path ([Environment]::GetFolderPath('Desktop')) 'BlurMask.lnk'
if ((Test-Path -LiteralPath $uninstallKey) -or (Test-Path -LiteralPath $menuLink) -or (Test-Path -LiteralPath $desktopLink) -or
    (Get-ItemProperty -LiteralPath $runKey -Name BlurMask -ErrorAction SilentlyContinue)) {
    throw 'An existing BlurMask installation/shortcut/startup entry is present. Run this test on a clean Windows user or CI runner.'
}
try { $existingApp = [Threading.Mutex]::OpenExisting('Xekep.BlurMask.SingleInstance.v1') }
catch [Threading.WaitHandleCannotBeOpenedException] { $existingApp = $null }
if ($existingApp) { $existingApp.Dispose(); throw 'BlurMask is running. The installer test will not close it.' }

$testRoot = Join-Path (Split-Path $PSScriptRoot -Parent) ('.artifacts\installer-test\' + [Guid]::NewGuid().ToString('N'))
$installDir = Join-Path $testRoot 'Installed App'
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
function Invoke-TestProcess([string]$File, [string[]]$Arguments) {
    $testProcess = Start-Process -FilePath $File -ArgumentList $Arguments -WindowStyle Hidden -PassThru
    if (-not $testProcess.WaitForExit(60000)) {
        $testProcess.Kill()
        throw "Test process timed out: $File"
    }
    if ($testProcess.ExitCode -ne 0) { throw "Process failed ($($testProcess.ExitCode)): $File. Logs: $testRoot" }
}
function Assert-Payload {
    foreach ($name in @('BlurMask.exe', 'av_libglesv2.dll', 'libSkiaSharp.dll', 'libHarfBuzzSharp.dll')) {
        $installed = Join-Path $installDir $name
        if (-not (Test-Path -LiteralPath $installed) -or
            (Get-FileHash -LiteralPath $installed).Hash -ne (Get-FileHash -LiteralPath (Join-Path $payload $name)).Hash) {
            throw "Installed payload mismatch: $name"
        }
    }
    if (Get-ChildItem -LiteralPath $installDir -Filter '*.pdb') { throw 'Installer contains debug symbols.' }
}

$installedSuccessfully = $false
try {
    $arguments = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', '/LANG=english',
        '/NOCLOSEAPPLICATIONS', '/NORESTARTAPPLICATIONS', '/TASKS=desktopicon,startup',
        "/DIR=`"$installDir`"", "/LOG=`"$(Join-Path $testRoot 'install.log')`"")
    Invoke-TestProcess $installer $arguments
    $installedSuccessfully = $true
    Assert-Payload
    if (-not (Test-Path -LiteralPath $uninstallKey) -or -not (Test-Path -LiteralPath $menuLink) -or -not (Test-Path -LiteralPath $desktopLink)) {
        throw 'Uninstall registration or shortcuts were not created.'
    }
    $startup = (Get-ItemProperty -LiteralPath $runKey -Name BlurMask).BlurMask
    if ($startup -ne ('"' + (Join-Path $installDir 'BlurMask.exe') + '"')) { throw 'Startup command does not quote the executable path.' }
    if (-not $SkipLaunch) { Invoke-TestProcess (Join-Path $installDir 'BlurMask.exe') @('--smoke-test') }

    # Reinstall is the same AppId/path used by upgrades. Replace a native DLL to
    # prove the payload is restored even when assembly versions remain unchanged.
    $marker = Join-Path $installDir 'user-file.txt'
    'keep this file' | Set-Content -LiteralPath $marker
    'outdated native library' | Set-Content -LiteralPath (Join-Path $installDir 'libHarfBuzzSharp.dll')
    $arguments[-1] = "/LOG=`"$(Join-Path $testRoot 'upgrade.log')`""
    Invoke-TestProcess $installer $arguments
    Assert-Payload
    if ((Get-Content -LiteralPath $marker) -ne 'keep this file') { throw 'Upgrade removed a user file.' }
}
finally {
    $uninstaller = Join-Path $installDir 'unins000.exe'
    if ($installedSuccessfully -and (Test-Path -LiteralPath $uninstaller)) {
        Invoke-TestProcess $uninstaller @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/LOG=`"$(Join-Path $testRoot 'uninstall.log')`"")
    }
}
if ((Test-Path -LiteralPath (Join-Path $installDir 'BlurMask.exe')) -or (Test-Path -LiteralPath $uninstallKey) -or
    (Test-Path -LiteralPath $menuLink) -or (Test-Path -LiteralPath $desktopLink) -or
    (Get-ItemProperty -LiteralPath $runKey -Name BlurMask -ErrorAction SilentlyContinue)) {
    throw 'Uninstall left application files, registration, shortcuts or startup entry.'
}
if ((Get-Content -LiteralPath $marker) -ne 'keep this file') { throw 'Uninstall removed a user file.' }
Write-Host "Installer install/upgrade/uninstall checks passed. Logs: $testRoot"
