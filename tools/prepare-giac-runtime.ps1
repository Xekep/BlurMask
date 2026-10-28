param(
    [string]$OutputDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) '.artifacts\giac-runtime\win-x64'),
    [string]$SourcesDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) '.artifacts\giac-sources'),
    [string]$CacheDirectory = (Join-Path ([IO.Path]::GetTempPath()) 'NovaPen-Giac-source-build'),
    [ValidateRange(1, 16)][int]$Jobs = 4
)

$ErrorActionPreference = 'Stop'
if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows)) {
    throw 'This helper builds the native Windows x64 Giac runtime.'
}
$output = [IO.Path]::GetFullPath($OutputDirectory)
$sourcesOutput = [IO.Path]::GetFullPath($SourcesDirectory)
$cache = [IO.Path]::GetFullPath($CacheDirectory)
if ($cache.Contains(' ')) { throw 'The Autoconf build cache path must not contain spaces; pass -CacheDirectory explicitly.' }
$recipe = Join-Path $PSScriptRoot 'giac-native'
New-Item -ItemType Directory -Force -Path $output, $sourcesOutput, $cache | Out-Null
if (Get-ChildItem -LiteralPath $output -File -Recurse | Where-Object { $_.Extension -eq '.dll' -or ($_.Extension -eq '.exe' -and $_.Name -ne 'icas.exe') }) {
    throw 'The solver output directory contains an unexpected native executable or DLL; choose a clean output directory.'
}

$inputs = @(
    @{ Name='giac-source.tar.gz'; Hash='0b5ddcad99da975cc8b6c21c01e8c42ddd4b058fb38fba72ee438727aa577caa'; Url='https://codeload.github.com/geogebra/giac/tar.gz/bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c' },
    @{ Name='gmp-6.3.0.tar.xz'; Hash='a3c2b80201b89e68616f4ad30bc66aee4927c3ce50e33929ca819d5c43538898'; Url='https://ftp.gnu.org/gnu/gmp/gmp-6.3.0.tar.xz' },
    @{ Name='mpfr-4.2.2.tar.xz'; Hash='b67ba0383ef7e8a8563734e2e889ef5ec3c3b898a01d00fa0a6869ad81c6ce01'; Url='https://www.mpfr.org/mpfr-4.2.2/mpfr-4.2.2.tar.xz' },
    @{ Name='w64devkit.7z.exe'; Hash='18d0a4c71a166f8401ab6305781bec5882b40b5e06ba9807c61cb5f3b3c6325e'; Url='https://github.com/skeeto/w64devkit/releases/download/v2.10.0/w64devkit-x64-2.10.0.7z.exe' },
    @{ Name='m4.pkg.tar.zst'; Hash='b561e8871c7599502d9fae7196ee965b3fed3d7af09154eb8d2f5e4d9e5e5106'; Url='https://repo.msys2.org/msys/x86_64/m4-1.4.21-1-x86_64.pkg.tar.zst' },
    @{ Name='licenses/COPYING.RUNTIME'; Hash='9d6b43ce4d8de0c878bf16b54d8e7a10d9bd42b75178153e3af6a815bdc90f74'; Url='https://raw.githubusercontent.com/gcc-mirror/gcc/releases/gcc-16.2.0/COPYING.RUNTIME' },
    @{ Name='licenses/winpthreads-COPYING'; Hash='63263614cdd29f2f93cba85e992f041b31f9fc7b4033692f31269489a8a1b177'; Url='https://raw.githubusercontent.com/mingw-w64/mingw-w64/v14.0.0/mingw-w64-libraries/winpthreads/COPYING' }
)
$windowsRecipeNames = @('Makefile', 'build.sh', 'icas.cpp', 'test-transport.ps1')
$recipeFiles = @($PSCommandPath) + @($windowsRecipeNames | Sort-Object | ForEach-Object { Join-Path $recipe $_ })
$recipeDescription = ($recipeFiles | ForEach-Object { (Split-Path $_ -Leaf) + ':' + (Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash.ToLowerInvariant() }) -join "`n"
$sha = [Security.Cryptography.SHA256]::Create()
try { $recipeHash = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($recipeDescription))).Replace('-','').ToLowerInvariant() }
finally { $sha.Dispose() }
$archiveName = 'NovaPen-Giac-corresponding-sources-bdbb1c41.tar.gz'
$sourceArchive = Join-Path $sourcesOutput $archiveName
$manifestPath = Join-Path $output 'manifest.json'
if ((Test-Path -LiteralPath $manifestPath) -and (Test-Path -LiteralPath $sourceArchive)) {
    try {
        $cached = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        $valid = $cached.buildRecipeSha256 -eq $recipeHash -and $cached.sourceArchive -eq $archiveName -and $cached.inputs.Count -eq $inputs.Count
        foreach ($inputFile in $inputs) {
            $entry = @($cached.inputs | Where-Object file -eq $inputFile.Name)
            $valid = $valid -and $entry.Count -eq 1 -and $entry[0].sha256 -eq $inputFile.Hash -and $entry[0].url -eq $inputFile.Url
        }
        $valid = $valid -and (Get-FileHash -LiteralPath $sourceArchive -Algorithm SHA256).Hash -eq $cached.sourceArchiveSha256
        foreach ($file in @('icas.exe','COPYING','THIRD-PARTY-NOTICES.txt','SOURCES.txt','licenses/LGPL-3.0.txt','licenses/COPYING.MinGW-w64-runtime.txt','licenses/COPYING.RUNTIME','licenses/winpthreads-COPYING','licenses/TinyMT-BSD-3-Clause.txt')) {
            $entry = @($cached.payloadFiles | Where-Object file -eq $file)
            $valid = $valid -and $entry.Count -eq 1 -and (Test-Path -LiteralPath (Join-Path $output $file))
            if ($valid) { $valid = (Get-FileHash -LiteralPath (Join-Path $output $file) -Algorithm SHA256).Hash -eq $entry[0].sha256 }
        }
        if ($valid -and (Get-FileHash -LiteralPath (Join-Path $output 'icas.exe') -Algorithm SHA256).Hash -eq $cached.executableSha256) {
            Write-Host 'Using verified Giac runtime and corresponding source archive from cache.'
            Write-Output (Join-Path $output 'icas.exe')
            return
        }
    } catch { Write-Host 'Cached Giac payload verification failed; rebuilding.' }
}
foreach ($inputFile in $inputs) {
    $path = Join-Path $cache $inputFile.Name
    New-Item -ItemType Directory -Force -Path (Split-Path $path -Parent) | Out-Null
    if (-not (Test-Path -LiteralPath $path)) {
        Write-Host "Downloading $($inputFile.Name)"
        & curl.exe --fail --location --retry 3 --silent --show-error $inputFile.Url --output $path
        if ($LASTEXITCODE -ne 0) { throw "Download failed: $($inputFile.Name)" }
    }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $inputFile.Hash) { throw "SHA256 mismatch: $($inputFile.Name)" }
}

$sevenZip = (Get-Command 7z.exe -ErrorAction SilentlyContinue).Source
if (-not $sevenZip) { $sevenZip = Join-Path $env:ProgramFiles '7-Zip\7z.exe' }
if (-not (Test-Path -LiteralPath $sevenZip)) { throw 'Install 7-Zip to extract the pinned portable toolchain.' }
$gitCommand = Get-Command git.exe -ErrorAction Stop
$gitRoot = Split-Path (Split-Path $gitCommand.Source -Parent) -Parent
$gitBash = Join-Path $gitRoot 'bin\bash.exe'
$gitUsr = Join-Path $gitRoot 'usr\bin'
if (-not (Test-Path -LiteralPath $gitBash)) { throw 'Git for Windows with bash is required for the source build.' }
$toolchain = Join-Path $cache 'toolchain\w64devkit'
if (-not (Test-Path -LiteralPath (Join-Path $toolchain 'bin\g++.exe'))) {
    & $sevenZip x (Join-Path $cache 'w64devkit.7z.exe') "-o$(Join-Path $cache 'toolchain')" -y | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Toolchain extraction failed.' }
}
$priorPath = $env:PATH
try {
    $env:PATH = (Join-Path $toolchain 'bin') + ';' + $priorPath
    $sourceRoot = Join-Path $cache 'sources'
    New-Item -ItemType Directory -Force -Path $sourceRoot, (Join-Path $cache 'm4') | Out-Null
    foreach ($archive in @('giac-source.tar.gz', 'gmp-6.3.0.tar.xz', 'mpfr-4.2.2.tar.xz')) {
        $directory = switch ($archive) {
            'giac-source.tar.gz' { 'giac-bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c' }
            'gmp-6.3.0.tar.xz' { 'gmp-6.3.0' }
            'mpfr-4.2.2.tar.xz' { 'mpfr-4.2.2' }
        }
        if (-not (Test-Path -LiteralPath (Join-Path $sourceRoot $directory))) {
            & tar.exe -xf (Join-Path $cache $archive) -C $sourceRoot
            if ($LASTEXITCODE -ne 0) { throw "Source extraction failed: $archive" }
        }
    }
    if (-not (Test-Path -LiteralPath (Join-Path $cache 'm4\usr\bin\m4.exe'))) {
        & tar.exe -xf (Join-Path $cache 'm4.pkg.tar.zst') -C (Join-Path $cache 'm4')
        if ($LASTEXITCODE -ne 0) { throw 'm4 extraction failed.' }
    }
    foreach ($dll in @('msys-2.0.dll', 'msys-iconv-2.dll')) {
        Copy-Item -LiteralPath (Join-Path $gitUsr $dll) -Destination (Join-Path $cache "m4\usr\bin\$dll") -Force
    }
    Write-Host 'Building static Giac + GMP + MPFR from pinned sources (logs in the build cache).'
    & $gitBash (Join-Path $recipe 'build.sh') $cache $recipe $Jobs | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Giac source build failed; inspect $cache/build-*/{configure,build,install}.log" }
    $executable = Join-Path $cache 'build-core-win64\icas.exe'
    & (Join-Path $recipe 'test-transport.ps1') -Executable $executable
    Copy-Item -LiteralPath $executable -Destination (Join-Path $output 'icas.exe') -Force
    $imports = & (Join-Path $toolchain 'bin\objdump.exe') -p $executable
    $dlls = @($imports | Select-String 'DLL Name:' | ForEach-Object { ($_.Line -split 'DLL Name:')[1].Trim() })
    $allowed = @('ADVAPI32.dll','bcrypt.dll','CRYPT32.dll','GDI32.dll','KERNEL32.dll','msvcrt.dll','SHELL32.dll','USER32.dll','WS2_32.dll')
    foreach ($dll in $dlls) { if ($dll -notin $allowed) { throw "Unexpected runtime dependency: $dll" } }
} finally { $env:PATH = $priorPath }

$giacHeaders = Join-Path $cache 'sources\giac-bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c\src\giac\headers'
$licenseDir = Join-Path $output 'licenses'
New-Item -ItemType Directory -Force -Path $licenseDir | Out-Null
Copy-Item (Join-Path $cache 'sources\gmp-6.3.0\COPYINGv3') (Join-Path $output 'COPYING') -Force
Copy-Item (Join-Path $cache 'sources\gmp-6.3.0\COPYING.LESSERv3') (Join-Path $licenseDir 'LGPL-3.0.txt') -Force
Copy-Item (Join-Path $toolchain 'COPYING.MinGW-w64-runtime.txt') $licenseDir -Force
Copy-Item (Join-Path $cache 'licenses\COPYING.RUNTIME') $licenseDir -Force
Copy-Item (Join-Path $cache 'licenses\winpthreads-COPYING') $licenseDir -Force
Copy-Item (Join-Path $giacHeaders 'tinymt32_license.h') (Join-Path $licenseDir 'TinyMT-BSD-3-Clause.txt') -Force
$notices = @'
Giac CLI built for NovaPen, using GeoGebra/giac commit
bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c (2026-09-11).
Giac copyright Bernard Parisse and contributors, GPL-3.0-or-later.
The independently built transport is GPL-3.0-or-later; full GPL text: COPYING.
Corresponding source and build recipe: the accompanying release asset
NovaPen-Giac-corresponding-sources-bdbb1c41.tar.gz (same release as this runtime).

Statically linked components:
GMP 6.3.0, GNU project contributors, LGPL-3.0-or-later OR GPL-2.0-or-later.
MPFR 4.2.2, INRIA and contributors, LGPL-3.0-or-later.
TinyMT, Mutsuo Saito and Makoto Matsumoto, BSD-3-Clause (license supplied).
MinGW-w64 runtime 14.0.0, project contributors (license supplied).
winpthreads 14.0.0, MinGW-w64 project and contributors (license supplied).
GCC 16.2.0 runtime/libstdc++, Free Software Foundation, GPL-3.0-or-later
with GCC Runtime Library Exception 3.1 (exception supplied).
No FLTK, Xcas GUI, readline, PARI, CoCoALib, NTL, GSL or dynamic vendor
libraries are included. Windows operating-system DLLs are not redistributed.
Toolchain source provenance: w64devkit 2.10.0 Dockerfile and source.tar release.
'@
Set-Content -LiteralPath (Join-Path $output 'THIRD-PARTY-NOTICES.txt') -Value $notices -Encoding UTF8
$manifest = [ordered]@{
    schemaVersion=1; runtime='win-x64'; transport='NovaPen stdin/stdout --sage'; giacCommit='bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c'
    compiler='GCC 16.2.0 (w64devkit 2.10.0)'; static=$true; sourceArchive=$archiveName; buildRecipeSha256=$recipeHash
    executableSha256=(Get-FileHash (Join-Path $output 'icas.exe') -Algorithm SHA256).Hash.ToLowerInvariant()
    executableBytes=(Get-Item (Join-Path $output 'icas.exe')).Length; systemImports=$dlls
    inputs=@($inputs | ForEach-Object { [ordered]@{file=$_.Name; sha256=$_.Hash; url=$_.Url} })
    payloadFiles=@(Get-ChildItem -LiteralPath $output -File -Recurse | Where-Object Name -notin @('manifest.json','SOURCES.txt') | Sort-Object FullName | ForEach-Object {
        [ordered]@{file=$_.FullName.Substring($output.Length + 1).Replace('\','/'); sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()}
    })
}
Set-Content -LiteralPath (Join-Path $output 'manifest.json') -Value ($manifest | ConvertTo-Json -Depth 6) -Encoding UTF8
$sourcePackage = Join-Path $cache ('corresponding-sources-' + $recipeHash.Substring(0, 16))
New-Item -ItemType Directory -Force -Path $sourcePackage, (Join-Path $sourcePackage 'archives'), (Join-Path $sourcePackage 'tools') | Out-Null
foreach ($archive in @('giac-source.tar.gz','gmp-6.3.0.tar.xz','mpfr-4.2.2.tar.xz')) { Copy-Item (Join-Path $cache $archive) (Join-Path $sourcePackage 'archives') -Force }
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $sourcePackage 'tools\prepare-giac-runtime.ps1') -Force
$sourceRecipe = Join-Path $sourcePackage 'tools\giac-native'
New-Item -ItemType Directory -Force -Path $sourceRecipe | Out-Null
foreach ($name in $windowsRecipeNames) { Copy-Item -LiteralPath (Join-Path $recipe $name) -Destination $sourceRecipe -Force }
Copy-Item -LiteralPath (Join-Path $output 'COPYING'), (Join-Path $output 'THIRD-PARTY-NOTICES.txt'), (Join-Path $output 'manifest.json') -Destination $sourcePackage -Force
Copy-Item -LiteralPath (Join-Path $output 'licenses') -Destination $sourcePackage -Recurse -Force
Set-Content -LiteralPath (Join-Path $sourcePackage 'BUILD.txt') -Encoding UTF8 -Value 'Install Git for Windows and 7-Zip. Run tools/prepare-giac-runtime.ps1 with -OutputDirectory and -SourcesDirectory. Copy the three archives in archives/ into -CacheDirectory beforehand for offline source reuse. The pinned toolchain and m4 package are build tools only; their download URLs and SHA256 are recorded in manifest.json. Giac is compiled without GUI and optional external CAS libraries; GMP and MPFR are compiled from the enclosed sources. build.sh replaces upstream bundled gmp.h with the native Win64 header generated from GMP 6.3.0.'
& tar.exe -czf $sourceArchive -C $sourcePackage .
if ($LASTEXITCODE -ne 0) { throw 'Corresponding source archive creation failed.' }
$sourceHash = (Get-FileHash -LiteralPath $sourceArchive -Algorithm SHA256).Hash.ToLowerInvariant()
$manifest.sourceArchiveSha256 = $sourceHash
Set-Content -LiteralPath "$sourceArchive.sha256" -Value "$sourceHash  $archiveName" -Encoding ASCII
Set-Content -LiteralPath (Join-Path $output 'SOURCES.txt') -Value "The corresponding sources are supplied as the release asset $archiveName (SHA256 $sourceHash). See THIRD-PARTY-NOTICES.txt and manifest.json." -Encoding UTF8
$manifest.payloadFiles += [ordered]@{file='SOURCES.txt'; sha256=(Get-FileHash -LiteralPath (Join-Path $output 'SOURCES.txt') -Algorithm SHA256).Hash.ToLowerInvariant()}
Set-Content -LiteralPath $manifestPath -Value ($manifest | ConvertTo-Json -Depth 6) -Encoding UTF8
Write-Host "Corresponding sources: $sourceArchive"
Write-Output (Join-Path $output 'icas.exe')
