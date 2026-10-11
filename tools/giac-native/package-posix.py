#!/usr/bin/env python3
# SPDX-License-Identifier: GPL-3.0-or-later
"""Hash-pinned POSIX source/runtime packaging; never downloads prebuilt CAS code."""
import argparse
import hashlib
import json
import re
import shutil
import subprocess
import sys
import tarfile
import tempfile
from pathlib import Path

COMMIT = 'bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c'
INPUTS = (
    ('giac-source.tar.gz', '0b5ddcad99da975cc8b6c21c01e8c42ddd4b058fb38fba72ee438727aa577caa',
     f'https://codeload.github.com/geogebra/giac/tar.gz/{COMMIT}'),
    ('gmp-6.3.0.tar.xz', 'a3c2b80201b89e68616f4ad30bc66aee4927c3ce50e33929ca819d5c43538898',
     'https://ftp.gnu.org/gnu/gmp/gmp-6.3.0.tar.xz'),
    ('mpfr-4.2.2.tar.xz', 'b67ba0383ef7e8a8563734e2e889ef5ec3c3b898a01d00fa0a6869ad81c6ce01',
     'https://www.mpfr.org/mpfr-4.2.2/mpfr-4.2.2.tar.xz'),
)
GCC_EXCEPTION = ('COPYING.RUNTIME', '9d6b43ce4d8de0c878bf16b54d8e7a10d9bd42b75178153e3af6a815bdc90f74',
                 'https://raw.githubusercontent.com/gcc-mirror/gcc/releases/gcc-16.2.0/COPYING.RUNTIME')
RECIPE_FILES = ('prepare-giac-runtime.sh', 'giac-native/Makefile.posix',
                'giac-native/build-posix.sh', 'giac-native/icas.cpp',
                'giac-native/test-transport.py', 'giac-native/package-posix.py',
                'giac-native/test-posix-packaging.py')

def digest(path):
    sha = hashlib.sha256()
    with Path(path).open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            sha.update(block)
    return sha.hexdigest()

def specs(rid):
    if rid not in ('linux-x64', 'osx-x64', 'osx-arm64'):
        raise ValueError(f'Unsupported RID: {rid}')
    return INPUTS + ((GCC_EXCEPTION,) if rid == 'linux-x64' else ())

def recipe_hash(tools):
    text = '\n'.join(f'{name}:{digest(tools / name)}' for name in RECIPE_FILES)
    return hashlib.sha256(text.encode('utf-8')).hexdigest()

def payload_names(rid):
    names = {'icas', 'COPYING', 'THIRD-PARTY-NOTICES.txt', 'SOURCES.txt',
             'licenses/LGPL-3.0.txt', 'licenses/TinyMT-BSD-3-Clause.txt'}
    if rid == 'linux-x64':
        names.add('licenses/COPYING.RUNTIME')
    return names

def validate_output(output, rid):
    output.mkdir(parents=True, exist_ok=True)
    present = {p.relative_to(output).as_posix() for p in output.rglob('*') if p.is_file()}
    unexpected = present - payload_names(rid) - {'manifest.json'}
    if unexpected:
        raise ValueError(f'Choose a clean solver output directory; unexpected files: {sorted(unexpected)}')
    if any(p.is_symlink() for p in output.rglob('*')):
        raise ValueError('Solver output directory must not contain symbolic links')

def verify_inputs(cache, rid):
    for name, expected, _ in specs(rid):
        if digest(cache / name) != expected:
            raise ValueError(f'SHA256 mismatch: {name}')

def cached(output, sources, tools, rid):
    validate_output(output, rid)
    manifest = json.loads((output / 'manifest.json').read_text(encoding='utf-8'))
    archive_name = f'BlurMask-Giac-corresponding-sources-bdbb1c41-{rid}.tar.gz'
    expected_inputs = [dict(file=name, sha256=sha, url=url) for name, sha, url in specs(rid)]
    if (manifest['runtime'] != rid or manifest['buildRecipeSha256'] != recipe_hash(tools)
            or manifest['sourceArchive'] != archive_name or manifest['inputs'] != expected_inputs):
        raise ValueError('Cached source/recipe provenance differs')
    payload = {entry['file']: entry['sha256'] for entry in manifest['payloadFiles']}
    if set(payload) != payload_names(rid):
        raise ValueError('Cached payload set differs')
    if any(digest(output / name) != sha for name, sha in payload.items()):
        raise ValueError('Cached runtime payload hash mismatch')
    if digest(output / 'icas') != manifest['executableSha256']:
        raise ValueError('Cached executable hash mismatch')
    if digest(sources / archive_name) != manifest['sourceArchiveSha256']:
        raise ValueError('Cached corresponding source hash mismatch')
    if not (output / 'icas').stat().st_mode & 0o111:
        raise ValueError('Cached solver lacks executable permission')

def imports(executable, rid):
    if rid == 'linux-x64':
        dynamic = subprocess.check_output(['readelf', '-d', str(executable)], text=True)
        names = re.findall(r'Shared library: \[([^\]]+)\]', dynamic)
        allowed = {'libc.so.6', 'libm.so.6', 'libpthread.so.0', 'librt.so.1', 'libdl.so.2',
                   'ld-linux-x86-64.so.2'}
        if not names or set(names) - allowed:
            raise ValueError(f'Unexpected Linux runtime dependencies: {names}')
        header = subprocess.check_output(['readelf', '-h', str(executable)], text=True)
        if not re.search(r'Machine:\s+Advanced Micro Devices X86-64', header):
            raise ValueError('Linux executable is not x86-64')
    else:
        dynamic = subprocess.check_output(['otool', '-L', str(executable)], text=True)
        names = [line.strip().split(' (')[0] for line in dynamic.splitlines()[1:] if line.strip()]
        allowed = {'/usr/lib/libSystem.B.dylib', '/usr/lib/libc++.1.dylib', '/usr/lib/libc++abi.dylib'}
        if not names or set(names) - allowed:
            raise ValueError(f'Unexpected macOS runtime dependencies: {names}')
        architectures = subprocess.check_output(['lipo', '-archs', str(executable)], text=True).split()
        if architectures != (['arm64'] if rid == 'osx-arm64' else ['x86_64']):
            raise ValueError(f'Unexpected macOS executable architectures: {architectures}')
    return names

def package(output, sources, cache, work, tools, rid):
    validate_output(output, rid)
    verify_inputs(cache, rid)
    executable = work / 'build-core/icas'
    dependencies = imports(executable, rid)
    shutil.copy2(executable, output / 'icas')
    (output / 'icas').chmod(0o755)
    licenses = output / 'licenses'
    licenses.mkdir(exist_ok=True)
    source = work / 'sources'
    shutil.copy2(source / 'gmp-6.3.0/COPYINGv3', output / 'COPYING')
    shutil.copy2(source / 'gmp-6.3.0/COPYING.LESSERv3', licenses / 'LGPL-3.0.txt')
    shutil.copy2(source / f'giac-{COMMIT}/src/giac/headers/tinymt32_license.h',
                 licenses / 'TinyMT-BSD-3-Clause.txt')
    if rid == 'linux-x64':
        shutil.copy2(cache / 'COPYING.RUNTIME', licenses / 'COPYING.RUNTIME')
    archive_name = f'BlurMask-Giac-corresponding-sources-bdbb1c41-{rid}.tar.gz'
    compiler = subprocess.check_output(['g++' if rid == 'linux-x64' else 'clang++', '--version'],
                                       text=True).splitlines()[0]
    notice = f'''Giac CLI built for BlurMask ({rid}), GeoGebra/giac commit {COMMIT}.
Giac: Bernard Parisse and contributors, GPL-3.0-or-later.
The independently built transport is GPL-3.0-or-later; full GPL text: COPYING.
Corresponding source and complete recipe: {archive_name}, same release as this runtime.

Statically linked from the enclosed source archives:
GMP 6.3.0: GNU contributors, LGPL-3.0-or-later OR GPL-2.0-or-later.
MPFR 4.2.2: INRIA and contributors, LGPL-3.0-or-later.
TinyMT: Mutsuo Saito and Makoto Matsumoto, BSD-3-Clause (license supplied).
Compiler: {compiler}
'''
    notice += ('GCC/libstdc++ runtime: GPL-3.0-or-later with GCC Runtime Library Exception 3.1 (supplied).\n'
               if rid == 'linux-x64' else 'Apple system libc++/libSystem are used and not redistributed.\n')
    notice += ('No FLTK, Xcas GUI, readline, PARI, CoCoALib, NTL, GSL or upstream prebuilt CAS libraries.\n'
               'OS system libraries are not redistributed. Dynamic system dependencies: ' + ', '.join(dependencies) + '\n')
    (output / 'THIRD-PARTY-NOTICES.txt').write_text(notice, encoding='utf-8')
    manifest = dict(schemaVersion=1, runtime=rid, transport='BlurMask stdin/stdout --sage',
                    giacCommit=COMMIT, compiler=compiler, staticNumericLibraries=True,
                    compilerRuntime='static' if rid == 'linux-x64' else 'dynamic-system',
                    macosDeploymentTarget=None if rid == 'linux-x64' else '11.0',
                    sourceArchive=archive_name, buildRecipeSha256=recipe_hash(tools),
                    executableSha256=digest(output / 'icas'), executableBytes=(output / 'icas').stat().st_size,
                    systemImports=dependencies,
                    inputs=[dict(file=n, sha256=h, url=u) for n, h, u in specs(rid)])
    # Record the build manifest inside the source archive before adding its hash;
    # avoiding a circular sourceArchiveSha256 reference, as in the Windows recipe.
    sources.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix='giac-corresponding-', dir=cache) as temporary:
        stage = Path(temporary)
        (stage / 'archives').mkdir()
        for name, _, _ in specs(rid):
            shutil.copy2(cache / name, stage / 'archives' / name)
        for name in RECIPE_FILES:
            target = stage / 'tools' / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(tools / name, target)
        for name in ('COPYING', 'THIRD-PARTY-NOTICES.txt'):
            shutil.copy2(output / name, stage / name)
        shutil.copytree(licenses, stage / 'licenses')
        (stage / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
        (stage / 'BUILD.txt').write_text(
            f'Native {rid} source build. Requires bash, make, m4, curl, tar/xz, Python 3, '
            'GCC/G++ (Linux) or Xcode Clang (macOS). Linux also requires binutils; macOS uses otool/lipo.\n'
            f'Run: bash tools/prepare-giac-runtime.sh {rid} OUTPUT SOURCES CACHE JOBS\n'
            'For offline reuse, copy the files in archives/ to CACHE before running. '
            'Only missing archives are downloaded; every input SHA256 is always checked.\n'
            'No GUI or repository prebuilt library is used. GMP/MPFR are built static; '
            'the upstream local gmp.h is replaced by the native generated GMP header.\n', encoding='utf-8')
        with tarfile.open(sources / archive_name, 'w:gz') as archive:
            archive.add(stage, arcname='.', recursive=True)
    source_hash = digest(sources / archive_name)
    (sources / (archive_name + '.sha256')).write_text(f'{source_hash}  {archive_name}\n', encoding='ascii')
    (output / 'SOURCES.txt').write_text(
        f'Corresponding sources: release asset {archive_name} (SHA256 {source_hash}). '
        'See THIRD-PARTY-NOTICES.txt and manifest.json.\n', encoding='utf-8')
    manifest['sourceArchiveSha256'] = source_hash
    manifest['payloadFiles'] = [dict(file=name, sha256=digest(output / name))
                                for name in sorted(payload_names(rid))]
    (output / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
    cached(output, sources, tools, rid)
    print(f'Corresponding sources: {sources / archive_name}')

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('command', choices=('specs', 'recipe-hash', 'validate-output', 'verify-inputs', 'cached', 'package'))
    parser.add_argument('--rid', required=True)
    for name in ('output', 'sources', 'cache', 'work', 'tools'):
        parser.add_argument('--' + name, type=Path)
    args = parser.parse_args()
    specs(args.rid)
    if args.command == 'specs':
        for row in specs(args.rid):
            print('\t'.join(row))
    elif args.command == 'recipe-hash':
        print(recipe_hash(args.tools))
    elif args.command == 'validate-output':
        validate_output(args.output, args.rid)
    elif args.command == 'verify-inputs':
        verify_inputs(args.cache, args.rid)
    elif args.command == 'cached':
        cached(args.output, args.sources, args.tools, args.rid)
    else:
        package(args.output, args.sources, args.cache, args.work, args.tools, args.rid)

if __name__ == '__main__':
    try:
        main()
    except (ValueError, KeyError, OSError, subprocess.SubprocessError) as error:
        print(f'Giac runtime preparation: {error}', file=sys.stderr)
        sys.exit(1)
