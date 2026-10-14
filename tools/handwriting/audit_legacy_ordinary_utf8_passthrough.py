"""Static default0491a0 no-marker identity-path proof. Vendor DLLs never execute."""
import argparse
import hashlib
import json
import struct
from pathlib import Path
from audit_legacy_glyph_utf8_bindings import Image, PINS


def audit(folder):
    engine = Image(folder/'MyScriptEngine.dll')
    proofs = []

    def opcode(address, hex_bytes, meaning):
        expected = bytes.fromhex(hex_bytes)
        if engine.at(address, len(expected)) != expected:
            raise ValueError('Unexpected pinned opcode: '+meaning)
        proofs.append({'address': f'{address:x}', 'bytes': hex_bytes, 'meaning': meaning})

    opcode(0x1800491cd, '4c8be2', 'Original caller vector pointer retained inR12')
    opcode(0x18004926c, '4532ff', 'Circled replacement flagR15B initiallyzero')
    opcode(0x180049279, 'c7442438f3b08080', 'Search start marker UTF8F0000 in scratch')
    opcode(0x180049281, 'c744243cf3b08081', 'Search end marker UTF8F0001')
    opcode(0x1800492d4, '0f8418100000', 'No start marker ->04a2f2 writeback gate')
    opcode(0x18004a2c4, '450fb6fe', 'Flag set from successful circled scratch replacement')
    opcode(0x18004a2f2, '4584ff', 'Test circled replacement flag')
    opcode(0x18004a2f5, '0f8434010000', 'Flagzero ->04a42f; skip all original-vector writes')
    opcode(0x18004a300, '41c744240800000000', 'Original count cleared only beyond guarded writeback gate')
    opcode(0x18004a439, '448838', 'Optional caller changeflag receivesR15B')
    base = struct.unpack('<Q', engine.at(0x1802521f0, 8))[0]
    index = struct.unpack('<Q', engine.at(0x1802521f8, 8))[0]
    canonical = 0
    for position in struct.unpack('<5722I', engine.at(index, 5722*4)):
        header, cp = struct.unpack('<II', engine.at(base+position*4, 8))
        if header >> 8 != 1:
            continue
        canonical += 1
        parts = struct.unpack('<'+'I'*(header & 255), engine.at(base+(position+2)*4, (header & 255)*4))
        if 0xf0000 in parts:
            raise ValueError('Canonical decomposition could introduce the start marker')
    for position in struct.unpack('<940I', engine.at(0x1801f9da0, 940*4)):
        cp = struct.unpack('<I', engine.at(base+position*4+4, 4))[0]
        if cp == 0xf0000:
            raise ValueError('Composition could introduce the start marker')
    if canonical != 2060:
        raise ValueError('Unexpected canonical table dimensions')
    return {'format': 'pinned-legacy-ordinary-utf8-passthrough-v1', 'engineSha256': PINS['MyScriptEngine.dll'],
            'proofs': proofs, 'canonicalRowsWithoutStartMarkerTarget': canonical,
            'compositionRowsWithoutStartMarkerResult': 940, 'vendorCodeExecuted': False,
            'scope': 'Default charset UTF8, successful helper, valid Unicode, no F0000. Does not classify/publish021000.'}


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('folder', type=Path)
    p.add_argument('--output', required=True, type=Path)
    args = p.parse_args()
    result = audit(args.folder)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps(result, indent=2)+'\n').encode('ascii'))
    print(f'PASS {len(result["proofs"])} pinned opcode/branch controls +2060/940 marker nonintroduction checks.')
