param(
    [Parameter(Mandatory = $true)][string]$InstallerPath,
    [Parameter(Mandatory = $true)][string]$PublishDirectory,
    [string]$SolverDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) '.artifacts\giac-runtime\win-x64'),
    [switch]$SkipLaunch
)

$ErrorActionPreference = 'Stop'
$installer = (Resolve-Path -LiteralPath $InstallerPath).Path
$payload = (Resolve-Path -LiteralPath $PublishDirectory).Path
$solverPayload = (Resolve-Path -LiteralPath $SolverDirectory).Path
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{A78C0733-F377-4CC3-91D0-D2621D4532CC}_is1'
$runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$menuLink = Join-Path ([Environment]::GetFolderPath('Programs')) 'NovaPen.lnk'
$desktopLink = Join-Path ([Environment]::GetFolderPath('Desktop')) 'NovaPen.lnk'
$legacyMenuLink = Join-Path ([Environment]::GetFolderPath('Programs')) 'BlurMask.lnk'
$legacyDesktopLink = Join-Path ([Environment]::GetFolderPath('Desktop')) 'BlurMask.lnk'
if ((Test-Path -LiteralPath $legacyMenuLink) -or (Test-Path -LiteralPath $legacyDesktopLink) -or
    (Get-ItemProperty -LiteralPath $runKey -Name BlurMask -ErrorAction SilentlyContinue)) {
    throw 'Existing legacy shortcuts/startup entry found. Use a clean test user; this probe will not overwrite them.'
}
if ((Test-Path -LiteralPath $uninstallKey) -or (Test-Path -LiteralPath $menuLink) -or (Test-Path -LiteralPath $desktopLink) -or
    (Get-ItemProperty -LiteralPath $runKey -Name NovaPen -ErrorAction SilentlyContinue)) {
    throw 'An existing NovaPen installation/shortcut/startup entry is present. Run this test on a clean Windows user or CI runner.'
}
try { $existingApp = [Threading.Mutex]::OpenExisting('Xekep.BlurMask.SingleInstance.v1') }
catch [Threading.WaitHandleCannotBeOpenedException] { $existingApp = $null }
if ($existingApp) { $existingApp.Dispose(); throw 'NovaPen is running. The installer test will not close it.' }

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
    foreach ($name in @('NovaPen.exe')) {
        $installed = Join-Path $installDir $name
        if (-not (Test-Path -LiteralPath $installed) -or
            (Get-FileHash -LiteralPath $installed).Hash -ne (Get-FileHash -LiteralPath (Join-Path $payload $name)).Hash) {
            throw "Installed payload mismatch: $name"
        }
    }
    if (Get-ChildItem -LiteralPath $installDir -Filter '*.pdb') { throw 'Installer contains debug symbols.' }
    if (Get-ChildItem -LiteralPath $installDir -Filter '*.dll') { throw 'Single-executable installer left native DLLs.' }
    foreach ($name in @('onnxruntime.dll','msvcp140.dll','msvcp140_1.dll','vcruntime140.dll','vcruntime140_1.dll','LICENSE')) {
        $relative = Join-Path 'runtimes/win-x64/native' $name
        $installed = Join-Path $installDir $relative
        if (-not (Test-Path -LiteralPath $installed) -or
            (Get-FileHash -LiteralPath $installed).Hash -ne (Get-FileHash -LiteralPath (Join-Path $PublishDirectory $relative)).Hash) {
            throw "Installed formula runtime is missing or differs: $name"
        }
    }
    foreach ($source in (Get-ChildItem -LiteralPath $solverPayload -File -Recurse)) {
        $relative = $source.FullName.Substring($solverPayload.Length).TrimStart('\', '/')
        $installed = Join-Path (Join-Path $installDir 'runtimes/win-x64/solver') $relative
        if (-not (Test-Path -LiteralPath $installed) -or (Get-FileHash -LiteralPath $installed).Hash -ne (Get-FileHash -LiteralPath $source.FullName).Hash) {
            throw "Installed Giac payload is missing or differs: $relative"
        }
    }
    Invoke-TestProcess (Join-Path $installDir 'NovaPen.exe') @('--math-solver-test')
}

$installedSuccessfully = $false
$legacyArtifactsCreated = $false
try {
    $arguments = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', '/LANG=english',
        '/NOCLOSEAPPLICATIONS', '/NORESTARTAPPLICATIONS', '/MERGETASKS=desktopicon',
        "/DIR=`"$installDir`"", "/LOG=`"$(Join-Path $testRoot 'install.log')`"")
    Invoke-TestProcess $installer $arguments
    $installedSuccessfully = $true
    Assert-Payload
    if (-not (Test-Path -LiteralPath $uninstallKey) -or -not (Test-Path -LiteralPath $menuLink) -or -not (Test-Path -LiteralPath $desktopLink)) {
        throw 'Uninstall registration or shortcuts were not created.'
    }
    $shortcutShell = New-Object -ComObject WScript.Shell
    try {
        $shortcut = $shortcutShell.CreateShortcut($menuLink)
        if ($shortcut.TargetPath -ne (Join-Path $installDir 'NovaPen.exe') -or $shortcut.WorkingDirectory -ne $installDir -or
            $shortcut.Arguments -ne '' -or $shortcut.IconLocation -ne ((Join-Path $installDir 'NovaPen.exe') + ',0')) {
            throw 'Start Menu shortcut must open NovaPen with the application icon, without minimized startup arguments.'
        }
    }
    finally { if ($shortcut) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($shortcut) }; [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($shortcutShell) }
    $startup = (Get-ItemProperty -LiteralPath $runKey -Name NovaPen).NovaPen
    if ($startup -ne ('"' + (Join-Path $installDir 'NovaPen.exe') + '" --minimized')) { throw 'Startup command must quote the executable and launch minimized.' }
    if (-not $SkipLaunch) { Invoke-TestProcess (Join-Path $installDir 'NovaPen.exe') @('--smoke-test', '--minimized') }

    # Simulate exact old-brand artifacts in the same AppId installation.
    $legacyExecutable = Join-Path $installDir 'BlurMask.exe'
    $legacyArtifactsCreated = $true
    'legacy executable' | Set-Content -LiteralPath $legacyExecutable
    $legacyShell = New-Object -ComObject WScript.Shell
    foreach ($legacyLink in @($legacyMenuLink, $legacyDesktopLink)) {
        $shortcut = $legacyShell.CreateShortcut($legacyLink)
        $shortcut.TargetPath = $legacyExecutable
        $shortcut.Save()
    }
    Set-ItemProperty -LiteralPath $runKey -Name BlurMask -Value ('"' + $legacyExecutable + '" --minimized')
    # Reinstall must replace the executable and remove the old dynamic graphics DLLs.
    $marker = Join-Path $installDir 'user-file.txt'
    'keep this file' | Set-Content -LiteralPath $marker
    'outdated executable' | Set-Content -LiteralPath (Join-Path $installDir 'NovaPen.exe')
    foreach ($name in @('libHarfBuzzSharp.dll', 'libSkiaSharp.dll', 'av_libglesv2.dll')) {
        'outdated native library' | Set-Content -LiteralPath (Join-Path $installDir $name)
    }
    $arguments[-1] = "/LOG=`"$(Join-Path $testRoot 'upgrade.log')`""
    Invoke-TestProcess $installer $arguments
    Assert-Payload
    if ((Test-Path -LiteralPath $legacyExecutable) -or (Test-Path -LiteralPath $legacyMenuLink) -or
        (Test-Path -LiteralPath $legacyDesktopLink) -or
        (Get-ItemProperty -LiteralPath $runKey -Name BlurMask -ErrorAction SilentlyContinue)) {
        throw 'Upgrade left old-brand executable, shortcuts or startup value.'
    }
    if ((Get-Content -LiteralPath $marker) -ne 'keep this file') { throw 'Upgrade removed a user file.' }
}
finally {
    # Only fixtures created above are cleaned up if an assertion/upgrade fails.
    if ($legacyArtifactsCreated) {
        foreach ($legacyLink in @($legacyMenuLink, $legacyDesktopLink)) {
            if (Test-Path -LiteralPath $legacyLink) { Remove-Item -LiteralPath $legacyLink }
        }
        Remove-ItemProperty -LiteralPath $runKey -Name BlurMask -ErrorAction SilentlyContinue
    }
    $uninstaller = Join-Path $installDir 'unins000.exe'
    if ($installedSuccessfully -and (Test-Path -LiteralPath $uninstaller)) {
        Invoke-TestProcess $uninstaller @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/LOG=`"$(Join-Path $testRoot 'uninstall.log')`"")
    }
}
if ((Test-Path -LiteralPath (Join-Path $installDir 'NovaPen.exe')) -or (Test-Path -LiteralPath $uninstallKey) -or
    (Test-Path -LiteralPath (Join-Path $installDir 'runtimes/win-x64/solver/icas.exe')) -or
    (Test-Path -LiteralPath $menuLink) -or (Test-Path -LiteralPath $desktopLink) -or
    (Get-ItemProperty -LiteralPath $runKey -Name NovaPen -ErrorAction SilentlyContinue)) {
    throw 'Uninstall left application files, registration, shortcuts or startup entry.'
}
if ((Get-Content -LiteralPath $marker) -ne 'keep this file') { throw 'Uninstall removed a user file.' }
Write-Host "Installer install/upgrade/uninstall checks passed. Logs: $testRoot"
