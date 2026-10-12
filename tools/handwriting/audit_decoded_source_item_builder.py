"""Static, SHA-pinned postdecoder/source-identity binding audit; never loads vendor code."""
import argparse
import hashlib
import json
import struct
from pathlib import Path

PINS = {
    'MyScriptDocument.dll': '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f',
    'MyScriptInk.dll': 'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a',
}


class Image:
    def __init__(self, path):
        self.name, self.data = path.name, path.read_bytes()
        if hashlib.sha256(self.data).hexdigest() != PINS[self.name]:
            raise ValueError('Unpinned image: ' + self.name)
        pe = struct.unpack_from('<I', self.data, 0x3c)[0]
        if self.data[pe:pe + 4] != b'PE\0\0':
            raise ValueError('Invalid PE signature')
        count = struct.unpack_from('<H', self.data, pe + 6)[0]
        size = struct.unpack_from('<H', self.data, pe + 20)[0]
        self.sections = [struct.unpack_from('<IIII', self.data, pe + 24 + size + i * 40 + 8)
                         for i in range(count)]

    def at(self, va, size):
        rva = va - 0x180000000
        for _, start, length, offset in self.sections:
            if start <= rva and rva + size <= start + length:
                pos = offset + rva - start
                if pos + size <= len(self.data):
                    return self.data[pos:pos + size]
        raise ValueError(f'Unmapped bounded address: {va:x}')


def audit(folder):
    doc, ink = (Image(folder / name) for name in PINS)
    proofs = []

    def pointer(image, address, target, meaning):
        if struct.unpack('<Q', image.at(address, 8))[0] != target:
            raise ValueError('Unexpected pointer: ' + meaning)
        proofs.append(dict(image=image.name, address=f'{address:x}', target=f'{target:x}', meaning=meaning))

    def call(address, target, meaning):
        raw = doc.at(address, 5)
        if raw[0] != 0xe8 or address + 5 + struct.unpack('<i', raw[1:])[0] != target:
            raise ValueError('Unexpected direct call: ' + meaning)
        proofs.append(dict(image=doc.name, address=f'{address:x}', target=f'{target:x}', meaning=meaning))

    call(0x18004844c, 0x180046ae0, 'EED success -> postdecoded record processing')
    call(0x180046da2, 0x180045c60, 'Postdecode -> per-record source-item candidate construction')
    call(0x180045d27, 0x180043650, 'Source-item constructor -> initialized40-byte item allocation')
    call(0x18004846f, 0x180043ac0, 'Only after postdecode: selected record -> block copy')
    pointer(doc, 0x1801eda60, 0x18003da20, 'EEF.virtual40 state merge (not source-item construction)')
    pointer(doc, 0x1801edb60, 0x18003dc50, 'EF0 explicit24-byte three-model parameter constructor')
    pointer(doc, 0x1801fd1e8, 0x18004a370, 'EF1 explicit64-byte transformer parameter constructor')
    pointer(ink, 0x1800ded78, 0x400000c6d, 'C6D pipeline processor descriptor')
    pointer(ink, 0x1800dedb8, 0x1800150c0, 'C6D serialized pipelineDescription reader through serviceBC1')
    pointer(ink, 0x1800dec38, 0x400000c79, 'C79 source-position owner descriptor')
    pointer(ink, 0x1800dec50, 0x1800dec80, 'C79 service table')
    pointer(ink, 0x1800dec80 + 0x30, 0x180010850, 'C79.virtual30 prepared position range -> source identities')
    return dict(format='pinned-decoded-source-item-builder-audit-v1', sources=PINS,
                proofs=proofs, vendorCodeExecuted=False,
                actualProducer='EED047c50 ->046ae0 ->045c60; 046ae0 then C79.virtual30 -> item source IDs',
                directResourceLayout='EED serialized SequenceRecognizer and C6D pipelineDescription; not old source135 layout',
                unresolved=['Native final chosen sequence/position consumer input production',
                            'Actual C79 point-position/pen-separator snapshot producer',
                            'Proven old Russian247/135 to modern EED adapter or matching modern resource',
                            'Live recognized glyph regions -> C1A line x-height integration'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('binary_folder', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    result = audit(args.binary_folder)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps(result, indent=2) + '\n').encode('utf-8'))
    print(f"PASS {len(result['proofs'])} pinned postdecoder/identity table and direct-call proofs; no vendor execution")
