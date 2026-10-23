"""Static CPU-alignment mapping and conditional kind8 workspace arithmetic.

Never executes CPU detection or SDK instructions. This does not establish
complete loader admission, allocator separation or retained-graph aliases.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path

from capstone import Cs, CS_ARCH_X86, CS_MODE_64, CS_OP_MEM, CS_AC_WRITE


def audit(binary, resource):
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

    md = Cs(CS_ARCH_X86, CS_MODE_64)
    md.detail = True
    bodies = []
    decoded = {}
    for address, size, count, digest in [
        (0x18010f060, 81, 27, 'f34bb3ac4cefbcfe1c3cd16c4e683aa8f88e5e163891bb88e5f02ad12c1a6d91'),
        (0x18010f1d0, 6883, 1132, '69682f9f1edf3aa8375a51644e15bd52190bd9b060b3fc483cf8c68e87cc8d9b'),
    ]:
        code = read(address, size)
        assert hashlib.sha256(code).hexdigest() == digest
        ins = list(md.disasm(code, address))
        assert len(ins) == count and sum(i.size for i in ins) == size
        decoded[address] = ins
        bodies.append(dict(address=hex(address), bytes=size, instructions=count, sha256=digest))
    # Complete seven-entry dispatch, including branches omitted as unreachable
    # in the decompiler output. All results are max(the branch constant, 8).
    targets = [image + x for x in struct.unpack('<7I', read(0x18010f0b4, 28))]
    assert targets == [0x18010f082, 0x18010f08d, 0x18010f0a3, 0x18010f098,
                       0x18010f0a3, 0x18010f098, 0x18010f0a3]
    assert read(0x18010f060, 7).hex() == 'ffc983f9067744'
    assert read(0x18010f07b, 7).hex() == 'ba08000000ffe1'
    for address, value in [(0x18010f082, 'b8010000003bc20f42c2c3'),
                           (0x18010f08d, 'b8040000003bc20f42c2c3'),
                           (0x18010f098, 'b8100000003bc20f42c2c3'),
                           (0x18010f0a3, '3bd28bc20f42c2c3'),
                           (0x18010f0ab, 'b8ffffffffc3')]:
        assert read(address, len(value) // 2).hex() == value
    branch_constants = {0x18010f082: 1, 0x18010f08d: 4, 0x18010f098: 16, 0x18010f0a3: 8}
    alignments = [max(branch_constants[t], 8) for t in targets]
    assert alignments == [8, 8, 8, 16, 8, 16, 8]
    setup = decoded[0x18010f1d0]
    prefix = [i for i in setup if 0x18010f1ec <= i.address <= 0x18010f1fd]
    assert [(i.mnemonic, i.op_str) for i in prefix] == [
        ('mov', 'r8, rcx'), ('mov', 'r11, rdx'), ('mov', 'ecx, dword ptr [rcx + 0x2ac]'),
        ('call', '0x18010f060'), ('mov', 'dword ptr [r8 + 0x2a8], eax')]
    assert [(i.address, i.op_str) for i in setup if i.mnemonic == 'call'] == [(0x18010f1f8, '0x18010f060')]
    family = {'r8', 'r8d', 'r8w', 'r8b'}
    assert [i.address for i in setup if family.intersection(i.reg_name(r) for r in i.regs_access()[1])] == [0x18010f1ec]
    assert not any(family.intersection(i.reg_name(r) for r in i.regs_access()[1]) for i in decoded[0x18010f060])
    stores = [i for i in setup for o in i.operands if o.type == CS_OP_MEM and o.access & CS_AC_WRITE
              and i.reg_name(o.mem.base) == 'r8' and o.mem.disp < 0x2ac and o.mem.disp + o.size > 0x2a8]
    assert [i.address for i in stores] == [0x18010f1fd]

    data = resource.read_bytes()
    assert hashlib.sha256(data).hexdigest() == '355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
    offset = 1774299
    magic, length, kind, flags, version = struct.unpack_from('<IIIHH', data, offset)
    assert (magic, offset + 8 + length, kind, flags, version) == (0x0101fe90, 3782348, 0x5f, 0, 0x106)
    assert struct.unpack_from('<II', data, offset + 16) == (1, 4)
    layers = [struct.unpack_from('<IIII', data, offset + 24 + 16 * i) for i in range(4)]
    assert layers[0][:3] == (1, 13, 0) and all(layer[:3] == (8, 127, 0) for layer in layers[1:])
    output = struct.unpack_from('<IIII', data, offset + 88)
    assert output == (0, 182, 1, 0)  # zero extra edges, output width/kind/flags
    # The selected resource is106: it bypasses106360 and calls10E410 directly.
    assert read(0x1801079a6, 8).hex() == 'c7442420ffffffff'
    assert read(0x1801079ae, 5).hex() == 'e85d6a0000'
    sizes = []
    for alignment in sorted(set(alignments)):
        align = lambda n: ((n + alignment - 1) // alignment) * alignment
        assert align(255) == 256 and align(128) == 128
        for mode in (False, True):
            rows = 2 + int(mode)
            floats = rows * (align(255) + 2 * 6 * align(128))
            assert floats == (5376 if mode else 3584)
            sizes.append(dict(alignmentFloatUnits=alignment, extraRow=mode,
                              kind8FloatUnits=floats, kind8WorkspaceBytes=4 * floats))
    return dict(status='PASS STATIC supported CPU mapping and conditional kind8 layout', bodies=bodies,
                cpuEnum1Through7AlignmentFloatUnits=alignments, rawResourceVersion=version,
                rawLayers=layers, rawOutputMetadata=output, selectedLegacy106360Path=False,
                conditionalKind8Workspace=sizes, cpuDetectionSuccessDomainProven=False,
                allocationSeparationProven=False, aliasStoreProofComplete=False,
                runtimePredicateApproved=False, vendorExecuted=False)


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--sdk', type=Path, required=True)
    p.add_argument('--models', type=Path, required=True)
    p.add_argument('--output', type=Path, required=True)
    args = p.parse_args()
    result = audit(args.sdk / 'MyScriptEngine.dll', args.models / 'en-US/en_US-ak-cur.res')
    args.output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(result))
