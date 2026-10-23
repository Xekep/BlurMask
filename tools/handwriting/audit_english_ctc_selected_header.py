"""Bounded raw106 header projection, conditional on successful stream reads.

This checks selected reader instructions and reconstructs only the initial
width/kind/flag lanes. It does not execute the SDK, validate the entire packet,
establish constructor aliases, or authorize the retained CTC graph.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path

from capstone import Cs, CS_ARCH_X86, CS_MODE_64


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

    code = read(0x180106dd0, 8980)
    digest = hashlib.sha256(code).hexdigest()
    assert digest == '0abf310b6958b37024de4175bec0e1e3221ea4bb4b3166d03cd9f039d865a1aa'
    instructions = list(Cs(CS_ARCH_X86, CS_MODE_64).disasm(code, 0x180106dd0))
    assert len(instructions) == 2410 and sum(i.size for i in instructions) == len(code)
    decoded = {i.address: (i.mnemonic, i.op_str) for i in instructions}
    assert decoded[0x180106e21] == ('movzx', 'r12d, r9w')

    def sequence(address, expected):
        index = next(i for i, instruction in enumerate(instructions) if instruction.address == address)
        actual = [(i.mnemonic, i.op_str) for i in instructions[index:index + len(expected)]]
        assert actual == expected, hex(address)

    # R12W is the version. The raw layer count is increased before it is
    # stored in packet+4. Allocation reserves one additional spare per lane.
    sequence(0x180106ef4, [
        ('mov', 'ebx, dword ptr [rsp + 0x6c]'), ('mov', 'eax, 0x101'),
        ('cmp', 'r12w, ax'), ('jbe', '0x180106f05'), ('inc', 'ebx'),
        ('test', 'r14d, r14d'), ('jne', '0x180106fb7'),
        ('mov', 'dword ptr [rbp - 0x3c], ebx'), ('lea', 'eax, [rbx + 1]'),
        ('movsxd', 'rsi, eax'), ('mov', 'edx, 0x1c'),
        ('mov', 'rax, qword ptr [r13 + 8]'), ('mov', 'r8, rsi'),
    ])
    # Six raw-lane writes use the current raw index (RBX advances by4).
    sequence(0x180107174, [
        ('mov', 'rcx, qword ptr [rbp - 0x28]'), ('inc', 'edi'),
        ('mov', 'eax, dword ptr [rsp + 0x64]'), ('mov', 'dword ptr [rbx + rcx], eax'),
        ('mov', 'rcx, qword ptr [rbp - 0x38]'), ('mov', 'eax, dword ptr [rsp + 0x58]'),
        ('mov', 'dword ptr [rbx + rcx], eax'), ('mov', 'rcx, qword ptr [rbp - 0x30]'),
        ('mov', 'eax, dword ptr [rsp + 0x70]'), ('mov', 'dword ptr [rbx + rcx], eax'),
        ('mov', 'rcx, qword ptr [rbp - 0x20]'), ('mov', 'eax, dword ptr [rsp + 0x60]'),
        ('mov', 'dword ptr [rbx + rcx], eax'), ('mov', 'rcx, qword ptr [rbp - 0x18]'),
        ('mov', 'eax, dword ptr [rsp + 0x5c]'), ('mov', 'dword ptr [rbx + rcx], eax'),
        ('mov', 'rcx, qword ptr [rbp - 8]'), ('mov', 'eax, dword ptr [rsp + 0x50]'),
        ('mov', 'dword ptr [rbx + rcx], eax'), ('add', 'rbx, 4'), ('jmp', '0x180106fe0'),
    ])
    # Output width/kind are written at packet.count-1, not at raw.count-1.
    sequence(0x180107337, [
        ('mov', 'eax, dword ptr [rbp - 0x3c]'), ('mov', 'r8b, 1'),
        ('mov', 'rcx, qword ptr [rbp - 0x28]'), ('dec', 'eax'),
        ('movsxd', 'rdx, eax'), ('mov', 'eax, dword ptr [rbp - 0x7c]'),
        ('mov', 'dword ptr [rcx + rdx*4], eax'),
        ('mov', 'eax, dword ptr [rbp - 0x3c]'), ('mov', 'rcx, qword ptr [rbp - 0x38]'),
        ('dec', 'eax'), ('movsxd', 'rdx, eax'), ('mov', 'eax, dword ptr [rbp + 0x30]'),
        ('mov', 'dword ptr [rcx + rdx*4], eax'), ('jmp', '0x18010742a'),
    ])
    sequence(0x180107473, [
        ('mov', 'eax, dword ptr [rbp - 0x3c]'), ('mov', 'rcx, qword ptr [rbp - 0x30]'),
        ('dec', 'eax'), ('movsxd', 'rdx, eax'), ('mov', 'eax, dword ptr [rbp + 0x34]'),
        ('mov', 'dword ptr [rcx + rdx*4], eax'),
    ])
    sequence(0x1801077f2, [
        ('movsxd', 'rdx, dword ptr [rbp - 0x3c]'), ('mov', 'r8, qword ptr [rbp - 0x38]'),
        ('lea', 'eax, [rdx - 1]'), ('movsxd', 'rcx, eax'),
        ('cmp', 'dword ptr [r8 + rcx*4], 4'), ('jne', '0x180107898'),
    ])
    assert decoded[0x1801079ae] == ('call', '0x18010e410')
    assert read(0x1801079a6, 8).hex() == 'c7442420ffffffff'

    data = resource.read_bytes()
    assert hashlib.sha256(data).hexdigest() == '355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
    offset = 1774299
    assert struct.unpack_from('<IIIHH', data, offset) == (0x0101fe90, 2008041, 0x5f, 0, 0x106)
    networks, count = struct.unpack_from('<II', data, offset + 16)
    assert (networks, count) == (1, 4)
    layers = [struct.unpack_from('<IIII', data, offset + 24 + 16 * i) for i in range(count)]
    assert layers == [(1, 13, 0, 2), (8, 127, 0, 2), (8, 127, 0, 2), (8, 127, 0, 2)]
    extra_edges, output_width, output_kind, output_flags = struct.unpack_from('<IIII', data, offset + 88)
    assert (extra_edges, output_width, output_kind, output_flags) == (0, 182, 1, 0)
    packet_count = count + 1
    lane_capacity = packet_count + 1
    lanes = dict(kinds=[r[0] for r in layers] + [output_kind],
                 widths=[r[1] for r in layers] + [output_width],
                 flags=[r[2] for r in layers] + [output_flags])
    assert packet_count == 5 and lane_capacity == 6
    assert all(len(values) == packet_count for values in lanes.values())
    assert all(r[0] not in (13, 14) for r in layers)
    assert output_kind not in (4, 5, 9, 10)
    return dict(status='PASS STATIC conditional initial header projection',
                completeReader=dict(address='0x180106dd0', bytes=len(code), instructions=len(instructions), sha256=digest),
                rawLayerCount=count, packetHeaderCount=packet_count, laneCapacity=lane_capacity,
                jointLaneAllocationBytes=7 * lane_capacity * 4, initialHeaderLanes=lanes,
                rawQuantizationCodes=[r[3] for r in layers], outputQuantizationCode=None,
                condition='successful stream reads; initial header projection only',
                completePacketProven=False, retainedHeaderPreservationProven=False,
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
