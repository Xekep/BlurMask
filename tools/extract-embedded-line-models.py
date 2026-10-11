"""Extract the two checked serialized 2e3 networks from a pinned Text DLL.

Read-only PE inspection; never loads or executes the DLL. The application
can read the extracted .res files directly without Python or this DLL.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path

DLL_HASH = '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
MODELS = [
    (0x18029e9d0, 85812, 'line-2e3-30.res', '10f4b43cd02873f1995f543dece70e70c1f22cbdb7dcbf63e1c5c205966ac70e'),
    (0x180277280, 161332, 'line-2e3-50.res', '01f9fd0bf4b5c0c3c1acc170ee82938e24a132696acbfbcfb34573a9d0550c7e'),
]


def extract(data):
    if len(data) > 16 * 1024 * 1024 or hashlib.sha256(data).hexdigest() != DLL_HASH:
        raise ValueError('Only the checked Text DLL SHA256 is supported')
    if data[:2] != b'MZ': raise ValueError('Missing PE DOS header')
    pe = struct.unpack_from('<I', data, 0x3c)[0]
    if data[pe:pe+4] != b'PE\0\0': raise ValueError('Missing PE signature')
    machine, count = struct.unpack_from('<HH', data, pe+4)
    optional_size = struct.unpack_from('<H', data, pe+20)[0]
    optional = pe+24
    if machine != 0x8664 or count > 96 or struct.unpack_from('<H', data, optional)[0] != 0x20b:
        raise ValueError('Expected a bounded PE32+ x64 binary')
    image_base = struct.unpack_from('<Q', data, optional+24)[0]
    sections = optional+optional_size
    result = []
    for address, size, name, expected_hash in MODELS:
        rva = address-image_base
        blob = None
        for index in range(count):
            section = sections+index*40
            _, start, raw_bytes, raw_start = struct.unpack_from('<IIII', data, section+8)
            if start <= rva and rva+size <= start+raw_bytes:
                offset = raw_start+rva-start
                if offset+size > len(data): raise ValueError('Truncated model section')
                blob = data[offset:offset+size]
                break
        if blob is None or hashlib.sha256(blob).hexdigest() != expected_hash:
            raise ValueError('Embedded model bounds or SHA256 mismatch')
        if struct.unpack_from('<4sIIHH', blob) != (b'\x90\xfe\1\1', size-8, 0x5f, 0, 0x106):
            raise ValueError('Unsupported embedded model header')
        result.append((name, blob, dict(path=name, bytes=size, sha256=expected_hash, sourceAddress=hex(address))))
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('dll', type=Path)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    if args.dll.stat().st_size > 16*1024*1024: raise ValueError('DLL exceeds the inspected size bound')
    models = extract(args.dll.read_bytes())
    args.output.mkdir(parents=True, exist_ok=True)
    for name, blob, _ in models: (args.output/name).write_bytes(blob)
    manifest = dict(sourceDllSha256=DLL_HASH, models=[record for _, _, record in models])
    (args.output/'embedded-manifest.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf8')
    print('PASS extracted two bounded serialized line classifiers with original SHA256.')


if __name__ == '__main__': main()
