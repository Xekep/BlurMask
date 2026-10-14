"""Read-only SHA-pinned legacy UTF8 producer binding audit. Never loads DLL code."""
import argparse
import hashlib
import json
import struct
from pathlib import Path

PINS = {'MyScriptText.dll': '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f',
        'MyScriptEngine.dll': 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'}


class Image:
    def __init__(self, path):
        self.name, self.data = path.name, path.read_bytes()
        if hashlib.sha256(self.data).hexdigest() != PINS[self.name]:
            raise ValueError('Unpinned image: '+self.name)
        pe = struct.unpack_from('<I', self.data, 0x3c)[0]
        if self.data[pe:pe+4] != b'PE\0\0':
            raise ValueError('Invalid PE signature')
        count = struct.unpack_from('<H', self.data, pe+6)[0]
        header = struct.unpack_from('<H', self.data, pe+20)[0]
        self.sections = [struct.unpack_from('<IIII', self.data, pe+24+header+i*40+8) for i in range(count)]

    def at(self, address, size):
        rva = address-0x180000000
        for _, start, length, offset in self.sections:
            if start <= rva and rva+size <= start+length:
                pos = offset+rva-start
                if pos+size <= len(self.data):
                    return self.data[pos:pos+size]
        raise ValueError(f'Unmapped bounded address: {address:x}')


def audit(folder):
    text, engine = [Image(folder/name) for name in PINS]
    proof = []

    def pointer(image, address, expected, meaning):
        if struct.unpack('<Q', image.at(address, 8))[0] != expected:
            raise ValueError('Unexpected pointer: '+meaning)
        proof.append({'image': image.name, 'address': f'{address:x}', 'target': f'{expected:x}', 'meaning': meaning})

    def call(image, address, target, meaning):
        raw = image.at(address, 5)
        if raw[0] != 0xe8 or address+5+struct.unpack('<i', raw[1:])[0] != target:
            raise ValueError('Unexpected call: '+meaning)
        proof.append({'image': image.name, 'address': f'{address:x}', 'target': f'{target:x}', 'meaning': meaning})

    pointer(text, 0x1802dc7b8, 0x300000273, 'Legacy273 descriptor')
    pointer(text, 0x1802dc7d0, 0x1802dc740, 'Legacy273 method table')
    pointer(text, 0x1802dc790, 0x18013c920, 'Legacy273.virtual50 UTF8 producer')
    pointer(text, 0x1802dc638, 0x300000274, 'Builtin glyph274 descriptor')
    pointer(text, 0x1802dc650, 0x1802dc680, 'Builtin glyph274 method table')
    pointer(text, 0x1802dc6d0, 0x180139d90, 'Glyph274.virtual50 token count')
    pointer(text, 0x1802dc6d8, 0x180139c90, 'Glyph274.virtual58 label/alternate provider')
    pointer(text, 0x1802dc6e8, 0x18013a0b0, 'Glyph274.virtual68 byte emitter')
    call(text, 0x18013c9b3, 0x18013c840, 'Legacy273 -> repeated native token producer')
    pointer(engine, 0x180252188, 0x1801dd740, 'Default service22 registration')
    pointer(engine, 0x1801dd760, 0x1801dd7a0, 'Service22 method table')
    pointer(engine, 0x1801dd880, 0x1800491a0, 'Service22.virtualE0 canonical/circled normalization')
    call(engine, 0x180049242, 0x180048190, 'Canonical recursive decomposition/reordering')
    call(engine, 0x180049256, 0x18004aa10, 'Canonical composition')
    return {'format': 'pinned-legacy-glyph-utf8-bindings-v1', 'sources': PINS, 'proofs': proof,
            'vendorCodeExecuted': False,
            'unresolved': ['Actual result273+18 control alphabet lifecycle', 'Engine Unicode normalizer portable data/implementation',
                           '021000 text-source owner/category publication', 'Wrapper group-prefix construction',
                           'Native winner source references to original indexed input owner']}


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('folder', type=Path)
    p.add_argument('--output', required=True, type=Path)
    args = p.parse_args()
    result = audit(args.folder)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps(result, indent=2)+'\n').encode('ascii'))
    print(f'PASS {len(result["proofs"])} pinned static bindings; vendor code not executed.')
