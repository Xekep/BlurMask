"""Prepare the checked RU engine data profile from local resources and a static PE.

Reads files only; no vendor engine execution. The resulting data is separate
from the independent library code. Only the inspected resource/PE pair is supported.
"""
import argparse
import hashlib
import importlib.util
import json
import struct
from pathlib import Path

AK_SHA = '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
LK_SHA = 'd26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
ENGINE_SHA = '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'


def pe_read(data, address, count, format):
    pe, = struct.unpack_from('<I', data, 0x3c)
    if data[pe:pe + 4] != b'PE\0\0':
        raise ValueError('Expected PE image')
    sections, = struct.unpack_from('<H', data, pe + 6)
    optional_size, = struct.unpack_from('<H', data, pe + 20)
    optional = pe + 24
    if struct.unpack_from('<H', data, optional)[0] != 0x20b or sections > 96:
        raise ValueError('Expected bounded PE32+ image')
    base, = struct.unpack_from('<Q', data, optional + 24)
    rva = address - base
    length = count * struct.calcsize('<' + format)
    for section in range(sections):
        offset = optional + optional_size + 40 * section
        _, start, size, raw = struct.unpack_from('<IIII', data, offset + 8)
        if start <= rva and rva + length <= start + size:
            offset = raw + rva - start
            if offset + length > len(data):
                raise ValueError('Truncated static table')
            return list(struct.unpack_from('<' + format * count, data, offset))
    raise ValueError('Static table outside file-backed PE sections')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--acoustic', type=Path, required=True)
    parser.add_argument('--lexical', type=Path, required=True)
    parser.add_argument('--engine-pe', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    acoustic, lexical, pe = (p.read_bytes() for p in (args.acoustic, args.lexical, args.engine_pe))
    if hashlib.sha256(acoustic).hexdigest() != AK_SHA or hashlib.sha256(lexical).hexdigest() != LK_SHA:
        raise ValueError('Unsupported Russian resource pair')
    if hashlib.sha256(pe).hexdigest() != ENGINE_SHA:
        raise ValueError('Unsupported source engine image')
    spec = importlib.util.spec_from_file_location('legacy_inspector', Path(__file__).with_name('inspect-legacy-ink.py'))
    inspector = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(inspector)
    configurations = inspector.inspect(acoustic)['configurations']
    configuration, = [c for c in configurations if c['offset'] == 189734]
    table = configuration['global_tail']['field_118_table']
    if table['models'] != 4 or table['classes'] != 192:
        raise ValueError('Unexpected confidence table dimensions')
    coefficients = table['coefficients']
    profile = dict(schemaVersion=1, profile='legacy-ru-lite-directional-v1',
        acousticSha256=AK_SHA, lexicalSha256=LK_SHA,
        sourceEngineSha256=hashlib.sha256(pe).hexdigest(),
        activations=dict(hidden=pe_read(pe, 0x180260fd0, 48, 'i'),
                         output=pe_read(pe, 0x180261090, 128, 'i')),
        temporal=dict(minima=pe_read(pe, 0x1802b6bd0, 274, 'h'),
                      maxima=pe_read(pe, 0x1802b6e00, 274, 'h')),
        spatial=dict(minima=pe_read(pe, 0x1802b70e0, 274, 'h'),
                     maxima=pe_read(pe, 0x1802b7310, 274, 'h')),
        centers=[[coefficients[(m * 192 + i) * 2] for i in range(180)] for m in range(4)],
        slopes=[[coefficients[(m * 192 + i) * 2 + 1] for i in range(180)] for m in range(4)])
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps(profile, indent=2, allow_nan=False) + '\n').encode('utf-8'))
    print(f'Prepared {args.output}: four confidence families, two274-column ranges, 176 activation entries')


if __name__ == '__main__':
    main()
