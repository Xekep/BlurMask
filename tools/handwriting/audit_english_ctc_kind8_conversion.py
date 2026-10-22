"""Audit retained kind8 conversion dispatch using PE bytes; never load the DLL."""
import argparse
import hashlib
import json
import struct
from pathlib import Path


def audit(binary, pins):
    raw = binary.read_bytes()
    assert hashlib.sha256(raw).hexdigest() == pins['nativeSha256'], 'Engine identity'
    pe = struct.unpack_from('<I', raw, 0x3c)[0]
    assert raw[pe:pe + 4] == b'PE\0\0'
    optional = pe + 24
    assert struct.unpack_from('<H', raw, optional)[0] == 0x20b
    image = struct.unpack_from('<Q', raw, optional + 24)[0]
    section_start = optional + struct.unpack_from('<H', raw, pe + 20)[0]
    sections = [struct.unpack_from('<IIII', raw, section_start + 40 * i + 8)
                for i in range(struct.unpack_from('<H', raw, pe + 6)[0])]

    def read(address, size):
        rva = address - image
        for _, va, length, offset in sections:
            if va <= rva and rva + size <= va + length:
                value = raw[offset + rva - va:offset + rva - va + size]
                assert len(value) == size
                return value
        raise AssertionError(f'Unmapped native range {address:x}+{size}')

    assert {r['function'] for r in pins['regions']} == {
        '1801953e0', '1801916c0', '180193240'}
    for region in pins['regions']:
        start, end = int(region['start'], 16), int(region['endInclusive'], 16)
        assert end - start + 1 == region['bytes']
        assert hashlib.sha256(read(start, region['bytes'])).hexdigest() == region['sha256']

    # The native expression is kind-1: actual kind8 uses entry7, not entry8.
    for address, code in {
        0x1801954de: '8b07',       # kind from node header
        0x1801954e4: 'ffc8',       # DEC EAX
        0x1801954f2: '83f80d',     # 14-entry range check
        0x180195504: '8b8c8224581900',
        0x18019550b: '4803ca',
        0x18019550e: 'ffe1',
    }.items():
        assert read(address, len(bytes.fromhex(code))).hex() == code
    assert pins['selectedKind'] == 8 and pins['jumpIndex'] == 7
    table = read(int(pins['jumpTableAddress'], 16), 14 * 4)
    assert hashlib.sha256(table).hexdigest() == pins['jumpTableSha256']
    destination = image + struct.unpack_from('<I', table, 7 * 4)[0]
    assert destination == int(pins['selectedTarget'], 16) == 0x1801955b1
    expected_edges = {
        0x1801955d0: 0x1801916c0, 0x1801955fa: 0x1801916c0,
        0x180191712: 0x180193240, 0x180191752: 0x180193240,
        **{address: 0x180198a00 for address in (
            0x18019327c, 0x1801932a9, 0x1801932cc, 0x1801932fd,
            0x180193349, 0x180193372, 0x1801933a0)},
    }
    assert len(pins['directEdges']) == len(expected_edges)
    assert {int(e['at'], 16): int(e['target'], 16)
            for e in pins['directEdges']} == expected_edges
    for edge in pins['directEdges']:
        address = int(edge['at'], 16)
        code = read(address, 5)
        assert code.hex() == edge['bytes'] and code[0] in (0xe8, 0xe9)
        assert address + 5 + struct.unpack_from('<i', code, 1)[0] == int(edge['target'], 16)

    return {
        'status': 'PASS',
        'scope': 'STATIC kind8 conversion dispatch and complete registered body identity',
        'registeredFunctions': len(pins['regions']),
        'bodyBytes': sum(r['bytes'] for r in pins['regions']),
        'directEdges': len(pins['directEdges']),
        'kind8Target': hex(destination),
        'vendorExecuted': False,
        'aliasStoreProofComplete': False,
        'runtimePredicateApproved': False,
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--sdk', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    pins = json.loads(Path(__file__).with_name('english-ctc-kind8-conversion-pins.json').read_bytes())
    result = audit(args.sdk / 'MyScriptEngine.dll', pins)
    args.output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(result))


if __name__ == '__main__':
    main()
