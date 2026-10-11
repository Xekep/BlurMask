"""Check release inventory against the solver manifests inside portable packages."""
import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import tarfile
import zipfile


def digest(stream):
    return hashlib.file_digest(stream, 'sha256').hexdigest()


class Archive:
    def __init__(self, path):
        self.zip = path.suffix == '.zip'
        self.archive = zipfile.ZipFile(path) if self.zip else tarfile.open(path, 'r:gz')
        members = self.archive.namelist() if self.zip else [m.name for m in self.archive.getmembers() if m.isfile()]
        self.names = {}
        for name in members:
            normalized = str(PurePosixPath(name))
            if normalized in self.names:
                raise ValueError(f'Duplicate archive entry: {normalized}')
            self.names[normalized] = name

    def __enter__(self):
        return self

    def __exit__(self, *args):
        self.archive.close()

    def open(self, name):
        actual = self.names[name]
        return self.archive.open(actual) if self.zip else self.archive.extractfile(actual)

    def json(self, name):
        with self.open(name) as stream:
            return json.loads(stream.read().decode('utf-8-sig'))


def verify_package(package, sources):
    with Archive(package) as archive:
        names = [name for name in archive.names if name.startswith('runtimes/') and name.endswith('/solver/manifest.json')]
        if len(names) != 1:
            raise ValueError(f'{package.name}: expected one bundled solver manifest, got {names}')
        manifest = archive.json(names[0])
        runtime = manifest['runtime']
        prefix = f'runtimes/{runtime}/solver/'
        if names[0] != prefix + 'manifest.json':
            raise ValueError(f'{package.name}: solver runtime/path mismatch')
        source_name = manifest['sourceArchive']
        if Path(source_name).name != source_name:
            raise ValueError('Corresponding source must be a release asset basename')
        source = sources / source_name
        with source.open('rb') as stream:
            source_hash = digest(stream)
        if source_hash != manifest['sourceArchiveSha256']:
            raise ValueError(f'{package.name}: corresponding source SHA256 mismatch')
        checksum = source.with_name(source.name + '.sha256').read_text(encoding='utf-8-sig').split()
        if checksum != [source_hash, source_name]:
            raise ValueError(f'{package.name}: corresponding source checksum sidecar mismatch')
        payload = {entry['file']: entry['sha256'] for entry in manifest['payloadFiles']}
        executable = 'icas.exe' if runtime == 'win-x64' else 'icas'
        if not {'COPYING', 'SOURCES.txt', 'THIRD-PARTY-NOTICES.txt', executable} <= payload.keys():
            raise ValueError(f'{package.name}: bundled solver/license/source pointer incomplete')
        for name, expected in payload.items():
            with archive.open(prefix + name) as stream:
                if digest(stream) != expected:
                    raise ValueError(f'{package.name}: bundled solver payload hash mismatch: {name}')
        if payload[executable] != manifest['executableSha256']:
            raise ValueError(f'{package.name}: executable SHA256 differs from payload')
    with Archive(source) as archive:
        manifests = [name for name in archive.names if name.endswith('/manifest.json') or name == 'manifest.json']
        if len(manifests) != 1:
            raise ValueError(f'{source_name}: expected one source build manifest')
        build = archive.json(manifests[0])
        for field in ['runtime', 'giacCommit', 'buildRecipeSha256', 'executableSha256', 'inputs']:
            if build[field] != manifest[field]:
                raise ValueError(f'{source_name}: source/binary build identity differs: {field}')
    print(f'PASS {package.name}: {runtime} solver payload and matching {source_name} / {source_hash}')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory', type=Path)
    parser.add_argument('--package', type=Path, help='Check one package against sources in directory')
    args = parser.parse_args()
    if args.package:
        packages = [args.package]
    else:
        mac = list(args.directory.glob('BlurMask-osx-*.tar.gz'))
        if len(mac) != 1:
            raise ValueError('Release needs exactly one macOS host package')
        packages = [args.directory / 'BlurMask-win-x64.zip', args.directory / 'BlurMask-linux-x64.tar.gz', *mac]
    for package in packages:
        verify_package(package, args.directory)


if __name__ == '__main__':
    main()
