"""Read-only direct state-metadata extent audit; not a complete alias proof.

Requires Capstone. Disassembles pinned bytes without executing the SDK.
Payload stores, allocator contracts and caller pointer provenance remain open.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path

from capstone import Cs, CS_ARCH_X86, CS_MODE_64, CS_AC_WRITE, CS_OP_MEM


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

    # Match partial-register writes too: an EBX/R10D write replaces its alias.
    families = {'rbx': {'rbx', 'ebx', 'bx', 'bl', 'bh'},
                'r10': {'r10', 'r10d', 'r10w', 'r10b'},
                'rdx': {'rdx', 'edx', 'dx', 'dl', 'dh'}}
    md = Cs(CS_ARCH_X86, CS_MODE_64)
    md.detail = True
    result = []
    for address, length, count, digest, alias, assignment, initial_end, extent in [
        (0x180191470, 356, 98, 'd1898c59dce50b403094fb4ffb1f034a3b2522ba81a7d6d86a8859c2045a1756',
         'rbx', 0x1801914b0, 0x1801914de, 0x20),
        (0x180192ae0, 1070, 292, '7bb47efde0a5858d4a3e90a722e7d280982d784916caafa1417789047a9ecac0',
         'r10', 0x180192b09, 0x180192b23, 0xc0),
    ]:
        body = read(address, length)
        assert hashlib.sha256(body).hexdigest() == digest
        decoded = list(md.disasm(body, address))
        assert len(decoded) == count
        cursor = address
        for instruction in decoded:
            assert instruction.address == cursor
            cursor += instruction.size
        assert cursor == address + length
        alias_writes = [i for i in decoded
                        if families[alias].intersection(i.reg_name(r) for r in i.regs_access()[1])]
        assert (alias_writes[0].address, alias_writes[0].mnemonic, alias_writes[0].op_str) == (
            assignment, 'mov', alias + ', rdx')
        # Initial RDX metadata stores precede its first partial/full overwrite.
        assert not any(families['rdx'].intersection(i.reg_name(r) for r in i.regs_access()[1])
                       for i in decoded if i.address < initial_end)
        if alias == 'rbx':
            # Genuine epilogue restores RBX with MOV, not POP.
            assert [(i.address, i.mnemonic, i.op_str) for i in alias_writes[1:]] == [
                (0x1801915bb, 'mov', 'rbx, qword ptr [r11 + 0x38]')]
            tail = [i for i in decoded if i.address >= 0x1801915bb]
            assert [i.mnemonic for i in tail] == [
                'mov', 'mov', 'mov', 'mov', 'pop', 'pop', 'pop', 'pop', 'pop', 'ret']
            assert not any(o.type == CS_OP_MEM and (
                i.reg_name(o.mem.base) in families[alias] or i.reg_name(o.mem.index) in families[alias])
                for i in tail for o in i.operands)
        else:
            assert len(alias_writes) == 1
        direct, other = [], []
        for i in decoded:
            for operand in i.operands:
                if operand.type != CS_OP_MEM or not operand.access & CS_AC_WRITE:
                    continue
                mem = operand.mem
                base = i.reg_name(mem.base)
                if base == alias or (base == 'rdx' and i.address < initial_end):
                    assert mem.index == 0 and 0 <= mem.disp and mem.disp + operand.size <= extent
                    direct.append(dict(address=hex(i.address), offset=mem.disp, bytes=operand.size))
                elif base not in ('rsp', 'rbp'):
                    other.append(dict(address=hex(i.address), instruction=i.mnemonic + ' ' + i.op_str))
        assert direct and max(s['offset'] + s['bytes'] for s in direct) == extent
        calls = [i for i in decoded if i.mnemonic == 'call']
        if alias == 'rbx':
            assert [i.address for i in calls] == [0x180191575, 0x1801915ae]
            assert all(i.op_str == '0x180192ae0' for i in calls)
        else:
            assert not calls
            # RBX is nonvolatile at the caller's two calls. Check this callee's
            # actual save/restore and stack adjustment rather than assuming it.
            assert decoded[0].mnemonic == 'mov' and decoded[0].op_str == 'qword ptr [rsp + 8], rbx'
            stack_writes = [i for i in decoded if 'rsp' in [i.reg_name(r) for r in i.regs_access()[1]]]
            assert [(i.mnemonic, i.op_str) for i in stack_writes] == [
                ('push', 'r12'), ('push', 'r13'), ('push', 'r14'), ('push', 'r15'),
                ('pop', 'r15'), ('pop', 'r14'), ('pop', 'r13'), ('pop', 'r12'), ('ret', '')]
            restore = next(i for i in decoded if i.address == 0x180192eee)
            assert restore.mnemonic == 'mov' and restore.op_str == 'rbx, qword ptr [rsp + 0x28]'
            assert not any(families['rbx'].intersection(i.reg_name(r) for r in i.regs_access()[1])
                           for i in decoded if i.address > restore.address)
        result.append(dict(function=hex(address), bodyBytes=length, instructions=count,
                           metadataExtent=extent, directMetadataStores=direct,
                           otherNonstackStoresNotYetProven=other))

    assert read(0x18019155e, 4).hex() == '488d5320'
    assert read(0x18019158e, 7).hex() == '488d93e0000000'
    table = struct.unpack('<10I', read(0x1801946f8, 40))
    assert image + table[8 - 3] == 0x18019463e
    assert read(0x180194618, 5).hex() == '8b0783c0fd'  # kind minus three
    assert read(0x18019463e, 5).hex() == 'baa0010000'  # allocation unit
    assert 0x20 + 0xc0 == 0xe0 and 0xe0 + 0xc0 == 0x1a0
    return dict(status='PASS PARTIAL direct metadata extents and kind8 allocation unit',
                functions=result, calleeOffsets=[0x20, 0xe0], composedMetadataExtent=0x1a0,
                kind8AllocationUnit=0x1a0, allocationCountAndCallerPointersProven=False,
                payloadStoresProven=False, aliasStoreProofComplete=False,
                runtimePredicateApproved=False, vendorExecuted=False)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--sdk', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    result = audit(args.sdk / 'MyScriptEngine.dll')
    args.output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({key: value for key, value in result.items() if key != 'functions'}))
