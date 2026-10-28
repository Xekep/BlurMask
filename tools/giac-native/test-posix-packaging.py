#!/usr/bin/env python3
# SPDX-License-Identifier: GPL-3.0-or-later
"""Independent packaging controls: no compiler or target binary execution."""
import importlib.util
import json
import hashlib
import tarfile
import tempfile
from pathlib import Path
from unittest.mock import patch

spec = importlib.util.spec_from_file_location('packager', Path(__file__).with_name('package-posix.py'))
packager = importlib.util.module_from_spec(spec)
spec.loader.exec_module(packager)

def rejects(action, kind=ValueError):
    try:
        action()
    except kind:
        return
    raise AssertionError('Expected rejection')

def main():
    tools = Path(__file__).resolve().parents[1]
    # Pin equality is checked against the pre-existing Windows recipe, without
    # sharing the POSIX input definitions with the reference.
    windows = (tools / 'prepare-giac-runtime.ps1').read_text(encoding='utf-8-sig')
    for name, sha, url in packager.INPUTS:
        assert f"Name='{name}'; Hash='{sha}'; Url='{url}'" in windows
    assert len(packager.specs('linux-x64')) == 4
    assert len(packager.specs('osx-x64')) == len(packager.specs('osx-arm64')) == 3
    rejects(lambda: packager.specs('linux-arm64'))
    assert len(packager.recipe_hash(tools)) == 64
    checks = 8
    with tempfile.TemporaryDirectory(prefix='giac-posix-controls-') as directory:
        root = Path(directory)
        output = root / 'output'
        packager.validate_output(output, 'linux-x64')
        (output / 'injected.so').write_bytes(b'not a library')
        rejects(lambda: packager.validate_output(output, 'linux-x64'))
        (output / 'injected.so').unlink()
        for name, _, _ in packager.INPUTS:
            (root / name).write_bytes(b'incorrect source bytes')
        rejects(lambda: packager.verify_inputs(root, 'osx-arm64'))
        checks += 2
        names = packager.payload_names('linux-x64')
        for name in names:
            path = output / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text('independent cache fixture ' + name, encoding='utf-8')
        # Windows stat treats extensionless files as non-executable. The cache
        # validation is expected to fail that permission guard on this host;
        # all provenance/hash checks before it are still exercised.
        (output / 'icas').chmod(0o755)
        archive_name = 'NovaPen-Giac-corresponding-sources-bdbb1c41-linux-x64.tar.gz'
        (root / archive_name).write_bytes(b'own corresponding-source control')
        manifest = dict(runtime='linux-x64', buildRecipeSha256=packager.recipe_hash(tools),
                        sourceArchive=archive_name,
                        inputs=[dict(file=n, sha256=h, url=u) for n, h, u in packager.specs('linux-x64')],
                        payloadFiles=[dict(file=n, sha256=packager.digest(output / n)) for n in sorted(names)],
                        executableSha256=packager.digest(output / 'icas'),
                        sourceArchiveSha256=packager.digest(root / archive_name))
        manifest_path = output / 'manifest.json'
        manifest_path.write_text(json.dumps(manifest), encoding='utf-8')
        if (output / 'icas').stat().st_mode & 0o111:
            packager.cached(output, root, tools, 'linux-x64')
        else:
            rejects(lambda: packager.cached(output, root, tools, 'linux-x64'))
        manifest['buildRecipeSha256'] = '0' * 64
        manifest_path.write_text(json.dumps(manifest), encoding='utf-8')
        rejects(lambda: packager.cached(output, root, tools, 'linux-x64'))
        manifest['buildRecipeSha256'] = packager.recipe_hash(tools)
        manifest_path.write_text(json.dumps(manifest), encoding='utf-8')
        (output / 'icas').write_bytes(b'changed executable')
        rejects(lambda: packager.cached(output, root, tools, 'linux-x64'))
        checks += 3
        # Exercise corresponding-source creation with small independent inputs.
        # Neither an installed CAS nor an executable is required or invoked.
        fake_cache, fake_work, fake_out, fake_sources = [root / n for n in ('cache', 'work', 'out', 'sources')]
        fake_cache.mkdir()
        fake_inputs = []
        for name, _, url in packager.INPUTS + (packager.GCC_EXCEPTION,):
            data = ('own source control ' + name).encode('ascii')
            (fake_cache / name).write_bytes(data)
            fake_inputs.append((name, hashlib.sha256(data).hexdigest(), url))
        for name in ('gmp-6.3.0/COPYINGv3', 'gmp-6.3.0/COPYING.LESSERv3',
                     f'giac-{packager.COMMIT}/src/giac/headers/tinymt32_license.h'):
            path = fake_work / 'sources' / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text('own license control ' + name, encoding='ascii')
        (fake_work / 'build-core').mkdir()
        (fake_work / 'build-core/icas').write_bytes(b'own non-executable transport control')
        with patch.object(packager, 'INPUTS', tuple(fake_inputs[:3])), \
                patch.object(packager, 'GCC_EXCEPTION', fake_inputs[3]), \
                patch.object(packager, 'imports', return_value=['libc.so.6']), \
                patch.object(packager.subprocess, 'check_output', return_value='own compiler control\n'), \
                patch.object(packager, 'cached'):
            packager.package(fake_out, fake_sources, fake_cache, fake_work, tools, 'linux-x64')
        result = json.loads((fake_out / 'manifest.json').read_text(encoding='utf-8'))
        source_archive = fake_sources / result['sourceArchive']
        assert result['sourceArchiveSha256'] == packager.digest(source_archive)
        assert result['executableSha256'] == packager.digest(fake_out / 'icas')
        assert set(e['file'] for e in result['payloadFiles']) == names
        with tarfile.open(source_archive) as archive:
            archived = set(archive.getnames())
            assert all('./tools/' + name in archived for name in packager.RECIPE_FILES)
            assert all('./archives/' + name in archived for name, _, _ in fake_inputs)
            assert './COPYING' in archived and './licenses/LGPL-3.0.txt' in archived
            source_manifest = json.load(archive.extractfile('./manifest.json'))
            assert source_manifest['buildRecipeSha256'] == packager.recipe_hash(tools)
            assert 'sourceArchiveSha256' not in source_manifest  # no circular reference
        for entry in result['payloadFiles']:
            assert entry['sha256'] == packager.digest(fake_out / entry['file'])
        checks += 7
    # Independent readelf/otool fixtures exercise platform dependency rejection;
    # subprocess only returns these literal texts, no target process is run.
    with patch.object(packager.subprocess, 'check_output', side_effect=[
            '0 (NEEDED) Shared library: [libm.so.6]\n0 (NEEDED) Shared library: [libc.so.6]',
            '  Machine:                           Advanced Micro Devices X86-64\n']):
        assert packager.imports(Path('/own/icas'), 'linux-x64') == ['libm.so.6', 'libc.so.6']
    for bad in ('libgmp.so.10', 'libmpfr.so.6', 'libstdc++.so.6', 'libgcc_s.so.1', 'libfltk.so.1'):
        with patch.object(packager.subprocess, 'check_output', return_value=f'Shared library: [{bad}]'):
            rejects(lambda: packager.imports(Path('/own/icas'), 'linux-x64'))
        checks += 1
    for rid, architecture in (('osx-x64', 'x86_64'), ('osx-arm64', 'arm64')):
        with patch.object(packager.subprocess, 'check_output', side_effect=[
                '/own/icas:\n\t/usr/lib/libc++.1.dylib (compatibility version 1.0.0)\n'
                '\t/usr/lib/libSystem.B.dylib (compatibility version 1.0.0)\n', architecture + '\n']):
            assert len(packager.imports(Path('/own/icas'), rid)) == 2
        checks += 1
    with patch.object(packager.subprocess, 'check_output', return_value=
            '/own/icas:\n\t@rpath/libgmp.dylib (compatibility version 1.0.0)\n'):
        rejects(lambda: packager.imports(Path('/own/icas'), 'osx-arm64'))
    checks += 2
    print(f'PASS {checks} own POSIX source-pin, cache, payload, dependency and architecture controls; no target execution.')

if __name__ == '__main__':
    main()
