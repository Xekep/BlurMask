# CI diagnostic only: extract the official CLI; never run the GUI installer.
param([string]$OutputDirectory = '.artifacts/giac-test', [string]$ArchivePath)
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $output -Force | Out-Null
if (-not $ArchivePath) {
    $ArchivePath = Join-Path $output 'xcaswininst.exe'
    & curl.exe --fail --location --retry 2 --max-time 180 --output $ArchivePath 'https://xcas.univ-grenoble-alpes.fr/backup/giac/xcaswininst.exe'
    if ($LASTEXITCODE -ne 0) { throw 'Official Giac download failed.' }
}
$archive = (Resolve-Path -LiteralPath $ArchivePath).Path
if ((Get-Item -LiteralPath $archive).Length -ne 222920022 -or
    (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant() -ne '2811493b3caf207614ebcb6d0fab46fa76c3c7938c064076d1ebb96cc389514f') {
    throw 'Pinned Giac installer size/hash mismatch. Review an upstream update before changing this pin.'
}
$sevenZip = Join-Path $env:ProgramFiles '7-Zip/7z.exe'
if (-not (Test-Path -LiteralPath $sevenZip)) { throw 'CI requires 7-Zip to inspect the upstream NSIS archive.' }
& $sevenZip x $archive "-o$output/runtime" 'icas.exe' '*.dll' 'COPYING' '-y' | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Giac CLI extraction failed.' }
foreach ($file in @('icas.exe', 'giacxcas.dll', 'COPYING')) {
    if (-not (Test-Path -LiteralPath (Join-Path $output "runtime/$file"))) { throw "Missing Giac runtime component: $file" }
}
Write-Output (Join-Path $output 'runtime/icas.exe')
