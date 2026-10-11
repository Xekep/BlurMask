"""Fetch pinned official CPU ONNX Runtime native library, never model weights."""
import argparse, hashlib, io, json, tarfile, urllib.request, zipfile
from pathlib import Path

VERSION = '1.23.2'
ASSETS = {
    'win-x64': ('onnxruntime-win-x64-1.23.2.zip', '0b38df9af21834e41e73d602d90db5cb06dbd1ca618948b8f1d66d607ac9f3cd', 'onnxruntime.dll'),
    'linux-x64': ('onnxruntime-linux-x64-1.23.2.tgz', '1fa4dcaef22f6f7d5cd81b28c2800414350c10116f5fdd46a2160082551c5f9b', 'libonnxruntime.so'),
    'osx-x64': ('onnxruntime-osx-universal2-1.23.2.tgz', '49ae8e3a66ccb18d98ad3fe7f5906b6d7887df8a5edd40f49eb2b14e20885809', 'libonnxruntime.dylib'),
    'osx-arm64': ('onnxruntime-osx-universal2-1.23.2.tgz', '49ae8e3a66ccb18d98ad3fe7f5906b6d7887df8a5edd40f49eb2b14e20885809', 'libonnxruntime.dylib'),
}

def fetch(rid, output, vc_redist_dir=None):
    archive, digest, filename = ASSETS[rid]
    url = f'https://github.com/microsoft/onnxruntime/releases/download/v{VERSION}/{archive}'
    with urllib.request.urlopen(url, timeout=120) as stream:
        data = stream.read(100_000_001)
    if len(data) > 100_000_000 or hashlib.sha256(data).hexdigest() != digest:
        raise ValueError('Official runtime archive size/hash mismatch')
    files = {}
    if archive.endswith('.zip'):
        with zipfile.ZipFile(io.BytesIO(data)) as z:
            for name in z.namelist():
                basename = Path(name).name
                if basename in (filename, 'LICENSE', 'ThirdPartyNotices.txt'):
                    files[basename] = z.read(name)
    else:
        with tarfile.open(fileobj=io.BytesIO(data), mode='r:gz') as t:
            for member in t.getmembers():
                basename = Path(member.name).name
                if member.isfile() and (basename in ('LICENSE', 'ThirdPartyNotices.txt') or basename.startswith(filename)):
                    files[filename if basename.startswith(filename) else basename] = t.extractfile(member).read()
    if filename not in files or 'LICENSE' not in files:
        raise ValueError('Runtime library/license absent')
    destination = output / 'runtimes' / rid / 'native'
    destination.mkdir(parents=True, exist_ok=True)
    for name, content in files.items(): (destination / name).write_bytes(content)
    report = dict(version=VERSION, rid=rid, source=url, archive_sha256=digest,
                  files={name:dict(bytes=len(content),sha256=hashlib.sha256(content).hexdigest()) for name,content in files.items()})
    if vc_redist_dir is not None:
        if rid != 'win-x64':raise ValueError('App-local VC CRT is Windows-only')
        report['vc_redist_source']=str(vc_redist_dir.resolve())
        for name in ('msvcp140.dll','msvcp140_1.dll','vcruntime140.dll','vcruntime140_1.dll'):
            content=(vc_redist_dir/name).read_bytes()
            if len(content)>2_000_000 or content[:2]!=b'MZ':raise ValueError('Unexpected VC CRT binary')
            (destination/name).write_bytes(content)
            report['files'][name]=dict(bytes=len(content),sha256=hashlib.sha256(content).hexdigest())
        (destination/'VC-REDISTRIBUTION.txt').write_text('Microsoft Visual C++ runtime files copied from the supplied Visual Studio VC/Redist directory. Redistribution is subject to your Visual Studio license and its REDIST list. See https://learn.microsoft.com/cpp/windows/redistributing-visual-cpp-files\n',encoding='utf8')
    (destination/'runtime-provenance.json').write_text(json.dumps(report,indent=2),encoding='utf8')
    print(destination / filename)

if __name__ == '__main__':
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('rid',choices=ASSETS);p.add_argument('--output',type=Path,required=True)
    p.add_argument('--vc-redist-dir',type=Path,help='Optional licensed Visual Studio VC/Redist x64 CRT directory (four dependency DLLs)')
    a=p.parse_args();fetch(a.rid,a.output,a.vc_redist_dir)
