"""Check the constructor's shifted raw quantization lane, without SDK execution.

Conditional on live disjoint packet/header objects and successful allocation.
This is not a whole-constructor alias proof or retained-graph admission.
"""
import argparse
import hashlib
import json
import runpy
import struct
from pathlib import Path

from capstone import Cs, CS_ARCH_X86, CS_MODE_64


def audit(binary, resource):
    upstream = runpy.run_path(str(Path(__file__).with_name('audit_english_ctc_selected_header.py')))
    header = upstream['audit'](binary, resource)
    raw = binary.read_bytes()
    pe = struct.unpack_from('<I', raw, 60)[0]
    optional = pe + 24
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

    code = read(0x1801941b0, 1352)
    digest = hashlib.sha256(code).hexdigest()
    assert digest == 'c9a84b82b5e81426fbce1cce534a4bdacf7a30e243b89a0fb467f77106859bd7'
    decoder = Cs(CS_ARCH_X86, CS_MODE_64)
    decoder.detail = True
    instructions = list(decoder.disasm(code, 0x1801941b0))
    assert len(instructions) == 381 and sum(i.size for i in instructions) == len(code)
    decoded = {i.address: (i.mnemonic, i.op_str) for i in instructions}
    for address, expected in {
        0x1801941d3: ('mov', 'rdi, r8'),
        0x1801941da: ('mov', 'rbx, rdx'),
        0x1801941e1: ('mov', 'ebp, 0x50'),
        0x180194204: ('mov', 'eax, dword ptr [r8 + 4]'),
        0x180194208: ('dec', 'eax'),
        0x18019420a: ('mov', 'dword ptr [rdx], eax'),
    }.items():
        assert decoded[address] == expected, hex(address)
    # RBP remains the header stride through the entire first construction loop.
    assert [i.address for i in instructions if i.address < 0x1801944b2 and
            {'rbp', 'ebp', 'bp', 'bpl'}.intersection(i.reg_name(r) for r in i.regs_access()[1])] == [0x1801941e1]
    sequence = [
        ('xor', 'esi, esi'), ('cmp', 'dword ptr [rbx], 1'), ('mov', 'edx, r8d'),
        ('jl', '0x1801944ec'), ('mov', 'ecx, 4'),
        ('mov', 'rax, qword ptr [rdi + 0x38]'), ('test', 'rax, rax'),
        ('je', '0x1801944cf'), ('mov', 'r8d, dword ptr [rcx + rax - 4]'),
        ('mov', 'rax, qword ptr [rbx + 8]'), ('inc', 'edx'), ('add', 'rcx, 4'),
        ('mov', 'dword ptr [rax + rbp + 0x28], r8d'), ('add', 'rbp, 0x50'),
        ('mov', 'r8d, 1'), ('cmp', 'edx, dword ptr [rbx]'), ('jle', '0x1801944c1'),
    ]
    at = next(n for n, i in enumerate(instructions) if i.address == 0x1801944b2)
    assert [(i.mnemonic, i.op_str) for i in instructions[at:at + len(sequence)]] == sequence
    assert decoded[0x18019449a] == ('mov', 'r8d, 1')
    count = header['packetHeaderCount']
    graph_count = count - 1
    quantization = header['rawQuantizationCodes']
    assert graph_count == len(quantization) == 4
    writes = []
    # Independent scalar replay of the pinned loop, with symbolic object offsets.
    counter, source_offset, target_offset = 1, 4, 0x50
    while counter <= graph_count:
        index = (source_offset - 4) // 4
        assert 0 <= index < len(quantization)
        writes.append(dict(sourceIndex=index, headerIndex=target_offset // 0x50,
                           headerByteOffset=target_offset + 0x28, code=quantization[index]))
        counter += 1
        source_offset += 4
        target_offset += 0x50
    assert [w['sourceIndex'] for w in writes] == [0, 1, 2, 3]
    assert [w['headerIndex'] for w in writes] == [1, 2, 3, 4]
    assert all(w['headerByteOffset'] + 4 <= count * 0x50 for w in writes)
    assert header['outputQuantizationCode'] is None
    return dict(status='PASS STATIC conditional constructor quantization-lane projection',
                completeConstructor=dict(address='0x1801941b0', bytes=len(code), instructions=len(instructions), sha256=digest),
                packetHeaderCount=count, graphUnitCount=graph_count, quantizationWrites=writes,
                appendedQuantizationSlotReadByThisLoop=False,
                condition='initial projected packet; live disjoint objects; successful allocation',
                wholeConstructorDataflowProven=False, retainedHeaderPreservationProven=False,
                allocationSeparationProven=False, runtimePredicateApproved=False, vendorExecuted=False)


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--sdk', type=Path, required=True)
    p.add_argument('--models', type=Path, required=True)
    p.add_argument('--output', type=Path, required=True)
    args = p.parse_args()
    result = audit(args.sdk / 'MyScriptEngine.dll', args.models / 'en-US/en_US-ak-cur.res')
    args.output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(result))
