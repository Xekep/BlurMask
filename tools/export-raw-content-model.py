"""Export the checked Raw Content2 resource to an IKRRAW01 application pack.

No downloads or vendor DLL execution. Version1 accepts only the observed profile;
the source resource is available under models/raw-content.
"""
import argparse
import hashlib
import importlib.util
import struct
import zlib
from pathlib import Path

SPEC = importlib.util.spec_from_file_location('math_export', Path(__file__).with_name('export-ink-math-model.py'))
CONTAINER = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(CONTAINER)
EXPECTED = '9bba7902c752fe937fc36f06573bd128c6b08d21a1b378dc460bc61608407739'
LIMIT = 16 * 1024 * 1024

def parse(data):
    kind, flags, version, end = CONTAINER.header(data)
    if (kind, version) != (0xee5, 0x102) or flags not in (0, 3) or end != len(data) or len(data) > LIMIT:
        raise ValueError('Unsupported Raw Content2 resource')
    if flags:
        t0, t1, t2, t3, t4 = CONTAINER.tables(kind, version)
        decoded = bytearray(len(data) - 16)
        for i, value in enumerate(data[16:]):
            a, b, c = i & 255, (i >> 8) & 255, (i >> 16) & 255
            x = t1[(t0[value] + a) & 255]; x = t2[(x + b) & 255]; x = t3[(x + c) & 255]
            x = t3[t4[x]]; x = t2[(x - c) & 255]; x = t1[(x - b) & 255]; decoded[i] = t0[(x - a) & 255]
        stream = zlib.decompressobj(-15); plain = stream.decompress(decoded, LIMIT)
        if not stream.eof or stream.unused_data or stream.unconsumed_tail:
            raise ValueError('Invalid or oversized resource payload')
        data = CONTAINER.MAGIC + struct.pack('<IIHH', len(plain) + 8, kind, 0, version) + plain
    # The model's alphabet, metadata, categories and graph were investigated as
    # one fixed profile. Reject future resources until their features are checked.
    if hashlib.sha256(data).hexdigest() != EXPECTED:
        raise ValueError('Unverified Raw Content2 profile; this exporter accepts the observed 4.5 resource only')
    if CONTAINER.header(data, 91) != (0x2b, 0, 0x104, 4010) or data[4010] != 0:
        raise ValueError('Invalid alphabet/profile boundary')
    if CONTAINER.header(data, 4011) != (0x51, 0, 0x100, len(data)):
        raise ValueError('Invalid ORT container')
    size = struct.unpack_from('<Q', data, 4035)[0]; graph = data[4043:]
    if size != len(graph) or graph[4:8] != b'ORTM': raise ValueError('Invalid ORT graph')
    return graph

def pack(graph):
    body = struct.pack('<6I', 11, 16, 111, 14, 1, len(graph)) + graph
    return b'IKRRAW01' + struct.pack('<II', 1, len(body)) + hashlib.sha256(body).digest() + body

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('resource', type=Path); parser.add_argument('output', type=Path)
    args = parser.parse_args()
    if args.resource.stat().st_size > LIMIT: raise ValueError('Resource is too large')
    graph = parse(args.resource.read_bytes()); result = pack(graph)
    args.output.parent.mkdir(parents=True, exist_ok=True); args.output.write_bytes(result)
    print(f'Exported {len(result)} bytes; SHA256 {hashlib.sha256(result).hexdigest()}')

if __name__ == '__main__': main()
