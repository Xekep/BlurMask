"""Static packet helper coverage and conditional no-mapping path; no SDK execution.

This does not prove that the selected serialized model produces that packet,
or close constructor allocation/alias writes. It grants no runtime predicate.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path


def audit(binary):
    raw = binary.read_bytes()
    assert hashlib.sha256(raw).hexdigest() == 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
    pe = struct.unpack_from('<I', raw, 60)[0]
    assert raw[pe:pe + 4] == b'PE\0\0'
    optional = pe + 24
    assert struct.unpack_from('<H', raw, optional)[0] == 0x20b
    image = struct.unpack_from('<Q', raw, optional + 24)[0]
    start = optional + struct.unpack_from('<H', raw, pe + 20)[0]
    sections = [struct.unpack_from('<IIII', raw, start + i * 40 + 8)
                for i in range(struct.unpack_from('<H', raw, pe + 6)[0])]

    def read(address, count):
        rva = address - image
        for _, va, length, offset in sections:
            if va <= rva and rva + count <= va + length:
                value = raw[offset + rva - va:offset + rva - va + count]
                assert len(value) == count
                return value
        raise AssertionError('Unbacked native address')

    # Complete registered body, including optional paths. Frozen Ghidra export:
    # 393 instructions covering every one of these 1466 bytes without gaps.
    assert hashlib.sha256(read(0x180106360, 1466)).hexdigest() == '2308f01d9a4308d0b23c9d11d18209ad5516c348f77ee200a9f34595f341cb13'
    # Genuine caller passes packet at RBP-40 and exact size112, not a guessed ABI.
    assert read(0x180107e3c, 21).hex() == '41b9700000004c8d45c0488bd6498bcde80fe5ffff'
    pins = {
        0x18010636f: '4d8be8',       # retain packet in R13
        0x180106394: '458b505c',     # count at +5C
        0x180106398: '33c9',         # ECX=0
        0x18010639e: '33ff',         # mapping-index EDI=0
        0x1801063a4: '4533ff',       # mapping-index R15D=0
        0x1801063a7: '49394860',     # matching count/pointer presence
        0x1801063b3: '0f95c1',
        0x1801063ba: '33c0',
        0x1801063bc: '4585d2',
        0x1801063bf: '0f95c0',
        0x1801063c2: '3bc1',
        0x1801063c4: '0f855d040000',
        0x1801063ca: '4d8b4068',     # optional mapping at +68
        0x1801063d1: '4d85c0',
        0x1801063d4: '0f84d3000000', # null mapping skips first scan
        0x1801064ad: '4585d2',
        0x1801064b0: '0f8ec4000000', # count0 skips second scan
        0x1801065a5: '897c2430',     # builder argument EDI=0
        0x1801065b0: '44897c2428',   # builder argument R15D=0
        0x1801065b5: 'c7442420ffffffff',
        0x1801065bd: 'e84e7e0000',   # builder10E410 remains in the path
        0x1801065ca: '85ff',
        0x1801065cc: '0f8ead010000', # skip first post-builder mapping writes
        0x18010677f: '4585ff',
        0x180106782: '0f8e60010000', # skip second post-builder mapping writes
        0x1801068e8: 'b001',
    }
    for address, value in pins.items():
        assert read(address, len(value) // 2).hex() == value, hex(address)

    def target(address, opcode):
        code = read(address, len(opcode) + 4)
        assert code[:len(opcode)] == opcode
        return address + len(code) + struct.unpack_from('<i', code, len(opcode))[0]

    assert target(0x180107e4c, b'\xe8') == 0x180106360
    assert target(0x1801063d4, b'\x0f\x84') == 0x1801064ad
    assert target(0x1801064b0, b'\x0f\x8e') == 0x18010657a
    assert target(0x1801065bd, b'\xe8') == 0x18010e410
    assert target(0x1801065cc, b'\x0f\x8e') == 0x18010677f
    assert target(0x180106782, b'\x0f\x8e') == 0x1801068e8
    return dict(status='PASS STATIC packet helper body and conditional no-mapping path',
                completeBodyBytes=1466, instructionPins=len(pins),
                conditionalPacket={'count5C': 0, 'pointer60': 0, 'pointer68': 0},
                builder='10E410 still requires allocation and alias proof',
                rawModelToPacketProven=False, aliasStoreProofComplete=False,
                runtimePredicateApproved=False, vendorExecuted=False)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--sdk', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    result = audit(args.sdk / 'MyScriptEngine.dll')
    args.output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(result))
