"""Static successful-result domain of the native CPU selector.

Reads and decodes bytes; never executes CPUID, XGETBV or vendor SDK code.
The feature mask is deliberately unconstrained, so no host capabilities are
assumed. This does not prove loader aliases or approve a runtime predicate.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64, CS_OP_IMM, CS_OP_MEM


def audit(binary):
    raw = binary.read_bytes()
    assert hashlib.sha256(raw).hexdigest() == 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
    pe = struct.unpack_from('<I', raw, 60)[0]
    assert raw[pe:pe + 4] == b'PE\0\0'
    optional = pe + 24
    assert struct.unpack_from('<H', raw, optional)[0] == 0x20b
    image = struct.unpack_from('<Q', raw, optional + 24)[0]
    section_start = optional + struct.unpack_from('<H', raw, pe + 20)[0]
    sections = [struct.unpack_from('<IIII', raw, section_start + i * 40 + 8)
                for i in range(struct.unpack_from('<H', raw, pe + 6)[0])]

    def read(address, size):
        for _, va, length, offset in sections:
            if va <= address - image and address - image + size <= va + length:
                return raw[offset + address - image - va:offset + address - image - va + size]
        raise AssertionError('Unbacked address')

    code = read(0x18010ee70, 463)
    digest = 'e6923479bfb14ef110b63a2c63dfaaf6b8bfaa4c5b57370d666d8ff6474e23fb'
    assert hashlib.sha256(code).hexdigest() == digest
    md = Cs(CS_ARCH_X86, CS_MODE_64)
    md.detail = True
    instructions = list(md.disasm(code, 0x18010ee70))
    assert len(instructions) == 133 and sum(i.size for i in instructions) == len(code)
    by_address = {i.address: i for i in instructions}

    def block(start, stop, expected):
        assert [(i.mnemonic, i.op_str) for i in instructions if start <= i.address < stop] == expected

    # The output pointer, requested enum and temporary are not guessed from C.
    writes = lambda names: [(i.address, i.mnemonic, i.op_str) for i in instructions
                            if names.intersection(i.reg_name(r) for r in i.regs_access()[1])]
    r10_writes = writes({'r10', 'r10d', 'r10w', 'r10b'})
    assert r10_writes == [
        (0x18010ee8c, 'xor', 'r10d, r10d'),
        (0x18010ef2a, 'mov', 'r10d, 6'),
        (0x18010ef47, 'mov', 'r10d, 5'),
        (0x18010ef64, 'mov', 'r10d, 3'),
        (0x18010ef81, 'setne', 'r10b'),
        (0x18010ef85, 'inc', 'r10d'),
        (0x18010ef9a, 'mov', 'r10d, edi')]
    edi_writes = writes({'rdi', 'edi', 'di', 'dil'})
    assert edi_writes[0] == (0x18010ee8f, 'mov', 'edi, edx')
    assert all(i.mnemonic == 'mov' and i.op_str == 'rdi, qword ptr [rsp + 0x68]'
               for i in instructions if i.address in {x[0] for x in edi_writes[1:]})
    r14_writes = writes({'r14', 'r14d', 'r14w', 'r14b'})
    assert r14_writes[0] == (0x18010ee91, 'mov', 'r14, r8')
    assert all(mnemonic == 'pop' and operands == 'r14' for _, mnemonic, operands in r14_writes[1:])
    output_stores = [i for i in instructions if i.mnemonic == 'mov' and i.operands[0].type == CS_OP_MEM
                     and i.reg_name(i.operands[0].mem.base) == 'r14']
    assert [(i.address, i.op_str) for i in output_stores] == [
        (0x18010ef30, 'dword ptr [r14], r10d'),
        (0x18010ef4f, 'dword ptr [r14], r10d'),
        (0x18010ef6c, 'dword ptr [r14], r10d'),
        (0x18010ef88, 'dword ptr [r14], r10d')]
    block(0x18010ef20, 0x18010ef35, [
        ('test', 'edi, edi'), ('jne', '0x18010ef97'), ('test', 'r9b, 8'),
        ('je', '0x18010ef41'), ('mov', 'r10d, 6'),
        ('mov', 'dword ptr [r14], r10d'), ('mov', 'al, 1')])
    block(0x18010ef41, 0x18010ef52, [
        ('test', 'r9b, 4'), ('je', '0x18010ef5e'), ('mov', 'r10d, 5'),
        ('mov', 'al, 1'), ('mov', 'dword ptr [r14], r10d')])
    block(0x18010ef5e, 0x18010ef6f, [
        ('test', 'r9b, 2'), ('je', '0x18010ef7b'), ('mov', 'r10d, 3'),
        ('mov', 'al, 1'), ('mov', 'dword ptr [r14], r10d')])
    block(0x18010ef7b, 0x18010ef8b, [
        ('test', 'r9b, 1'), ('mov', 'al, 1'), ('setne', 'r10b'),
        ('inc', 'r10d'), ('mov', 'dword ptr [r14], r10d')])
    block(0x18010ef97, 0x18010efb7, [
        ('lea', 'eax, [rdi - 1]'), ('mov', 'r10d, edi'), ('cmp', 'eax, 6'),
        ('ja', '0x18010efef'), ('lea', 'rdx, [rip - 0x10efa9]'), ('cdqe', ''),
        ('mov', 'ecx, dword ptr [rdx + rax*4 + 0x10f040]'),
        ('add', 'rcx, rdx'), ('jmp', 'rcx')])
    targets = [image + rva for rva in struct.unpack('<7I', read(0x18010f040, 28))]
    assert targets == [0x18010ef30, 0x18010f023, 0x18010f010, 0x18010f010,
                       0x18010effd, 0x18010efb7, 0x18010f036]
    for address, mask in ((0x18010f023, 1), (0x18010f010, 2), (0x18010effd, 4), (0x18010efb7, 8)):
        branch = by_address[address]
        following = by_address[address + branch.size]
        assert (branch.mnemonic, branch.op_str) == ('test', f'r9b, {mask}')
        assert (following.mnemonic, following.op_str) == ('jne', '0x18010ef30')
    assert by_address[0x18010f036].mnemonic == 'lea'  # enum7 always enters the error tail
    assert (by_address[0x18010f03d].mnemonic, by_address[0x18010f03d].op_str) == ('jmp', '0x18010efc8')
    assert [(i.address, i.op_str) for i in instructions if i.mnemonic == 'call'] == [(0x18010efed, 'rax')]
    assert [(i.address, i.op_str) for i in instructions if i.mnemonic == 'jmp'
            and i.operands[0].type != CS_OP_IMM] == [(0x18010efb5, 'rcx')]
    assert [i.address for i in instructions if i.mnemonic == 'ret'] == [
        0x18010ef40, 0x18010ef5d, 0x18010ef7a, 0x18010ef96, 0x18010effc]
    block(0x18010efef, 0x18010effd, [
        ('mov', 'rdi, qword ptr [rsp + 0x68]'), ('xor', 'al, al'),
        ('add', 'rsp, 0x40'), ('pop', 'r14'), ('ret', '')])
    for instruction in instructions:
        if instruction.mnemonic.startswith('j') and instruction.operands[0].type == CS_OP_IMM:
            assert instruction.operands[0].imm in by_address
    # Finite abstract routing, not hardware execution. All16 possible feature
    # masks are allowed, including impossible combinations. Explicit requests
    # outside1..7 fail the unsigned bound; enum7 always reports unsupported.
    results = []
    for features in range(16):
        automatic = 6 if features & 8 else 5 if features & 4 else 3 if features & 2 else 1 + bool(features & 1)
        results.append((0, features, int(automatic)))
        for requested, mask in enumerate((0, 1, 2, 2, 4, 8, None), 1):
            if mask is not None and (mask == 0 or features & mask):
                results.append((requested, features, requested))
    domain = sorted({value for _, _, value in results})
    assert domain == [1, 2, 3, 4, 5, 6]
    return dict(status='PASS STATIC successful CPU-selector domain', bodySha256=digest,
                bodyBytes=len(code), instructions=len(instructions), dispatchTargets=list(map(hex, targets)),
                successfulEnumDomain=domain, abstractSuccessfulCases=len(results),
                hardwareFeatureMaskConstrained=False, cpuInstructionsExecuted=False,
                allocatorSeparationProven=False, aliasStoreProofComplete=False,
                runtimePredicateApproved=False, vendorExecuted=False)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--sdk', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    report = audit(args.sdk / 'MyScriptEngine.dll')
    args.output.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(report))
