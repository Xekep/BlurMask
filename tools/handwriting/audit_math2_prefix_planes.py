"""Math2 raw48 -> primary/secondary/optional prefix-plane static producer.
Only PE/ORTM data are read; no runtime source edits or native execution.
"""
import hashlib
import json
import math
import struct
import subprocess
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
from capstone.x86 import X86_OP_MEM, X86_REG_RIP

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
out = Path(sys.argv[1]).resolve()
regions = [(0x180035d60, 0x180035e60, '77855b1a9947b85557b447ddadc730b7537fd3050d81e667cef6b9f215857a49'),
 (0x180033d60, 0x180033ed0, '0f1d1f73403ff279137f10cd41a8bd9713046d61bebc33ea3085e175c11dce1b'),
 (0x1800359f0, 0x180035c50, '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'),
 (0x180036250, 0x180036360, '52e63eb5a71693f125f9d69b891fa13925cac2178ee630b428b936fdece78c63'),
 (0x180038610, 0x18003a260, '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'),
 (0x18003a260, 0x18003b220, '43b6395c140ccc4975bcc1dde81378e5df1745dd5bdd1cbdb88a79007780f19d'),
 (0x18003bd40, 0x18003c0d0, '1c024eb35449188805605af08077203d40979a4b68f6f89828ae9d36a0eb1cbc'),
 (0x18003cac0, 0x18003d1e0, 'f51653c489d94baa7dbc4fdca884f33431c585e07ccdd8d41c717bcadcc2091d'),
 (0x18003da20, 0x18003dbe0, '7c241b6479679efd9b69719990470c73d77912aba9b385585d9d07ee84e89900'),
 (0x1800450c0, 0x180045460, '155f71ec1b5fea2831e30c94fa71dfcd3d3c8753d41bfe22dd27df8b9eb51693'),
 (0x180049630, 0x18004a1f0, 'e65e9cf06b42789cb9f9f20f9f98a4d9dc8ef55397169de319cead75e92b1e77'),
 (0x18004a1f0, 0x18004a2f0, '83f03fb75eb466ed26fe9b68ec3cf9e459bcd3d2503df91fab35225352af4d95')]
pins = [(0x180038974, 'mov', 'edx, 0x10'),
 (0x180038987, 'mov', 'r9, qword ptr [rax + 0x18]'),
 (0x18003898b, 'call', 'r9'),
 (0x18003899c, 'mov', 'edx, 0x10'),
 (0x1800389b3, 'call', 'r9'),
 (0x1800389c3, 'mov', 'edx, 0x10'),
 (0x1800389de, 'call', 'r9'),
 (0x1800389ee, 'mov', 'edx, 0x10'),
 (0x180038a05, 'call', 'r9'),
 (0x180038a21, 'cmp', 'dword ptr [rsi + 0x3c], -1'),
 (0x180038a2d, 'jle', '0x180038a8a'),
 (0x180038b18, 'mov', 'ecx, dword ptr [rdi + rbx + 8]'),
 (0x180038b32, 'mov', 'dword ptr [rdx], 0xffffffff'),
 (0x180038b38, 'inc', 'dword ptr [rdi + rbx + 8]'),
 (0x180039119, 'call', '0x180036250'),
 (0x18003911e, 'mov', 'r13, rax'),
 (0x180039121, 'mov', 'r15d, 0xffffffff'),
 (0x18003912a, 'je', '0x180039154'),
 (0x18003914b, 'comiss', 'xmm0, dword ptr [r9 + rdx*4]'),
 (0x180039150, 'setbe', 'r15b'),
 (0x18003917d, 'call', '0x1800351e0'),
 (0x180039187, 'mov', 'dword ptr [rax + 0x28], edi'),
 (0x18003918a, 'mov', 'qword ptr [rax + 8], rbx'),
 (0x18003918e, 'mov', 'qword ptr [rax + 0x10], r13'),
 (0x180039192, 'mov', 'dword ptr [rax + 0x18], r15d'),
 (0x180039196, 'mov', 'qword ptr [rax + 0x20], 0xffffffffffffffff'),
 (0x180039543, 'cmp', 'ecx, dword ptr [rsi + 0x30]'),
 (0x180039548, 'mov', 'dword ptr [rdx + rax], ebx'),
 (0x18003954b, 'mov', 'dword ptr [rax], ebx'),
 (0x18003954d, 'cmp', 'dword ptr [rsi + 0x3c], -1'),
 (0x180039553, 'mov', 'dword ptr [rax + r8], ebx'),
 (0x18003958d, 'movsxd', 'rdi, dword ptr [r11 + rsi*8 + 0x28]'),
 (0x180039592, 'shl', 'rdi, 4'),
 (0x1800395ad, 'mov', 'r15, qword ptr [r11 + rsi*8 + 0x20]'),
 (0x1800395b2, 'cmp', 'r15, -1'),
 (0x1800395b6, 'jne', '0x1800395bd'),
 (0x1800395b8, 'mov', 'r15, qword ptr [r11 + rsi*8 + 8]'),
 (0x1800395bd, 'movzx', 'eax, word ptr [r11 + rsi*8 + 0x10]'),
 (0x1800395e1, 'mov', 'r9d, dword ptr [rdi + rax + 8]'),
 (0x1800395e6, 'mov', 'r8, qword ptr [rdi + rax]'),
 (0x1800395f1, 'call', '0x180033de0'),
 (0x18003960b, 'call', '0x180033d60'),
 (0x180039630, 'mov', 'word ptr [rdx], r15w'),
 (0x180039634, 'inc', 'dword ptr [rbx + 8]'),
 (0x180039648, 'mov', 'r9d, dword ptr [rdi + r12 + 8]'),
 (0x18003964d, 'mov', 'r8, qword ptr [rdi + r12]'),
 (0x180039651, 'call', '0x180033de0'),
 (0x18003966b, 'call', '0x180033d60'),
 (0x180039690, 'movzx', 'eax, word ptr [rsp + 0x60]'),
 (0x180039695, 'mov', 'word ptr [rdx], ax'),
 (0x1800396a0, 'inc', 'dword ptr [rbx + 8]'),
 (0x1800396ad, 'cmp', 'dword ptr [rax + 0x3c], -1'),
 (0x1800396bd, 'movzx', 'edi, word ptr [r15 + rsi*8 + 0x18]'),
 (0x1800396d5, 'call', '0x180033de0'),
 (0x1800396eb, 'call', '0x180033d60'),
 (0x18003970c, 'mov', 'word ptr [rdx], di'),
 (0x18003970f, 'inc', 'dword ptr [rbx + 8]'),
 (0x18003974f, 'mov', 'qword ptr [rbp - 0x68], rax'),
 (0x180039757, 'mov', 'qword ptr [rbp - 0x80], r10'),
 (0x18003975f, 'mov', 'qword ptr [rbp - 0x78], r10'),
 (0x180039763, 'mov', 'qword ptr [rbp - 0x58], rax'),
 (0x180039767, 'mov', 'qword ptr [rsp + 0x78], r11'),
 (0x18003976c, 'mov', 'qword ptr [rbp - 0x60], r12'),
 (0x1800397b2, 'mov', 'r15d, dword ptr [r12]'),
 (0x1800397b8, 'mov', 'r15d, 0xffffffff'),
 (0x18003625a, 'mov', 'rcx, qword ptr [rdx + 0x18]'),
 (0x18003625e, 'movss', 'xmm0, dword ptr [rip + 0x1f3b22]'),
 (0x180036266, 'movsxd', 'rax, r9d'),
 (0x180036269, 'imul', 'rax, rcx'),
 (0x18003626d, 'movsxd', 'rbx, ecx'),
 (0x180036277, 'mov', 'rax, 0xffffffffffffffff'),
 (0x1800362b8, 'comiss', 'xmm1, xmm0'),
 (0x1800362bb, 'jbe', '0x1800362c0'),
 (0x1800362c5, 'comiss', 'xmm1, xmm3'),
 (0x1800362cb, 'cmovbe', 'r11, rax'),
 (0x1800362cf, 'comiss', 'xmm2, xmm0'),
 (0x1800362d2, 'jbe', '0x1800362dd'),
 (0x1800362e4, 'comiss', 'xmm1, xmm0'),
 (0x1800362e7, 'jbe', '0x1800362ec'),
 (0x1800362f1, 'comiss', 'xmm1, xmm3'),
 (0x1800362f7, 'cmovbe', 'rax, r11'),
 (0x1800362fb, 'comiss', 'xmm2, xmm0'),
 (0x1800362fe, 'jbe', '0x180036308'),
 (0x180036327, 'comiss', 'xmm1, xmm0'),
 (0x18003632a, 'jbe', '0x18003632f'),
 (0x18003632f, 'comiss', 'xmm1, xmm2'),
 (0x180036335, 'cmovbe', 'rcx, rax'),
 (0x18003d1ba, 'mov', 'rax, qword ptr [rdi + 0xd8]'),
 (0x18003d1c6, 'mov', 'qword ptr [rdx], rax'),
 (0x18003d1ce, 'mov', 'rax, qword ptr [rdi + 0xe8]'),
 (0x18003d1d5, 'mov', 'qword ptr [rdx], rax'),
 (0x180045289, 'mov', 'r12d, dword ptr [rcx + 0x120]'),
 (0x18004538c, 'mov', 'dword ptr [rdi + 0x9c], r12d'),
 (0x180035ada, 'movups', 'xmm0, xmmword ptr [rbp + 0x20]'),
 (0x180035ade, 'movups', 'xmmword ptr [rdi + 0x20], xmm0'),
 (0x18004a2ce, 'mov', 'qword ptr [rdx + 0x120], 0xffffffffffffffff'),
 (0x18004a0c8, 'lea', 'rdx, [rip + 0x1d0a01]'),
 (0x18004a0f3, 'mov', 'dword ptr [rbx + 0x120], esi')]

b = Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
md = Cs(CS_ARCH_X86, CS_MODE_64)
md.detail = True
instructions = {}
for start, end, sha in regions:
    raw = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == sha, hex(start)
    instructions.update((i.address, i) for i in md.disasm(raw, start))
for address, mnemonic, operands in pins:
    i = instructions[address]
    assert (i.mnemonic, i.op_str) == (mnemonic, operands), hex(address)
extra_pins = {
    0x180035d74: ('cmp', 'byte ptr [rdx + 0x38], 0'),
    0x180035d8b: ('jne', '0x180035d93'),
    0x180035d8d: ('cmp', 'dword ptr [rdx + 0x5c], 0'),
    0x180035d91: ('jle', '0x180035dec'),
    0x180035d96: ('mov', 'edx, 0x10'),
    0x180035daa: ('call', 'r9'),
    0x180035dad: ('mov', 'qword ptr [rbx + 0x88], rax'),
    0x180035ddd: ('mov', 'qword ptr [rax + rdx - 0x10], r8'),
    0x180035de2: ('mov', 'qword ptr [rax + rdx - 8], r8'),
    0x180045277: ('movzx', 'r8d, byte ptr [rax + 0x40]'),
    0x180045385: ('mov', 'byte ptr [rdi + 0x98], r8b'),
    0x18004a137: ('mov', 'byte ptr [rbx + 0x40], r13b'),
    0x18003bd56: ('mov', 'qword ptr [rdx + 0x110], 1'),
    0x18003bd64: ('mov', 'dword ptr [rdx + 0x120], 0'),
    0x18003be25: ('mov', 'qword ptr [rbx + 0xf0], rax'),
    0x18003be31: ('mov', 'dword ptr [rax], 0'),
    0x18003a353: ('mov', 'dword ptr [rdi + 0x98], 1'),
    0x18003a820: ('mov', 'dword ptr [rdi + 0x98], 1'),
}
for address, expected in extra_pins.items():
    i = instructions[address]
    assert (i.mnemonic, i.op_str) == expected, hex(address)
bindings = [(0x1801ed940, 0x180035d60),
            (0x1801ed958, 0x18003a260), (0x1801eda48, 0x18003bd40),
            (0x1801eda50, 0x18003cac0), (0x1801eda60, 0x18003da20)]
for address, expected in bindings:
    assert b.qwords(address, 1)[0] == expected, hex(address)

# Decode the actual detection-output literal at the mapper, not a guessed
# field role based on the nearby max_pred_counts/classification indices.
i = instructions[0x18004a0c8]
assert i.mnemonic == 'lea' and i.operands[1].type == X86_OP_MEM
assert i.operands[1].mem.base == X86_REG_RIP
offset = b.off(i.address + i.size + i.operands[1].mem.disp)
assert b.data[offset:offset + 17] == b'detection_output\0'
minimum = struct.unpack_from('<f', b.data, b.off(0x180036266 + 0x1f3b22))[0]
assert struct.pack('<f', minimum) == bytes.fromhex('ffff7fff')

# Compose the independently pinned current IO layout and default V18 allocator
# proof. The dependency's historical implementation-gap text is not imported
# as the current runtime status.
out.parent.mkdir(parents=True, exist_ok=True)
dependency = out.with_name(out.stem + '-tensor-lifecycle.json')
subprocess.run([sys.executable, str(ROOT / 'tools/handwriting/audit_math2_tensor_lifecycle.py'),
                str(dependency)], check=True)
lifecycle = json.loads(dependency.read_bytes())
models = lifecycle['models']
assert models[0]['io']['outputs'] == ['output', 'output_len', 'output_seg',
                                    'counting_output', 'max_pred_counts']
assert models[2]['io']['outputs'] == ['reco_output', 'attn_seg', 'state_out_k', 'state_out_v']
assert 'detection_output' not in models[2]['io']['outputs']
assert 'classification_output' not in models[0]['io']['outputs']

# Prepared argmax discriminators: the native COMISS/JBE/CMOVBE sequence skips
# unordered operands and retains the first strict maximum. All-minimum has
# index -1; a conventional argmax initialized to index0 would be wrong.
def native_comparison_path(values):
    best, index = minimum, -1
    for at, value in enumerate(values):
        unordered = math.isnan(value) or math.isnan(best)
        below_or_equal = unordered or value <= best
        previous = best
        if not below_or_equal:
            best = value
        # Native index select is a second COMISS against the old best.
        if not (math.isnan(value) or math.isnan(previous) or value <= previous):
            index = at
    return index

import itertools
argmax_controls = 0
for count in range(1, 6):
    for values in itertools.product([minimum, -math.inf, -3.0, -2.0, 0.0, math.inf, math.nan], repeat=count):
        eligible = [(value, at) for at, value in enumerate(values)
                    if not math.isnan(value) and value > minimum]
        expected = -1 if not eligible else max(eligible, key=lambda row: (row[0], -row[1]))[1]
        assert native_comparison_path(values) == expected
        argmax_controls += 1
assert native_comparison_path([minimum] * 297) == -1
assert native_comparison_path([-1.0, -1.0, -2.0]) == 0

# Prepared plane-layout checks only, including nonuniform prior histories,
# repeated parents, alternate -1 versus a merely low16-FFFF value, and shrink.
old_primary = [[10, 11], [20], [30, 31, 32]]
old_secondary = [[110, 111], [120], [130, 131, 132]]
old_auxiliary = [[210, 211], [220], [230, 231, 232]]
planes_before = json.dumps([old_primary, old_secondary, old_auxiliary])
layout_controls = []
for parents in ([2, 0, 2], [1, 1], [1, 0], [2]):
    storage = bytearray(len(parents) * 48)
    for at, parent in enumerate(parents):
        symbol = 0x1002a + at
        alternate = (0xffffffffffffffff if at == 0 else
                     0xffffffff0000ffff if at == 1 else 0x10064 + at)
        struct.pack_into('<QQI', storage, at * 48 + 8,
                         symbol, 0x100c8 + at, 0x10001 + at)
        struct.pack_into('<Qi', storage, at * 48 + 0x20, alternate, parent)
    primary, secondary, auxiliary = [], [], []
    for at in range(len(parents)):
        start = at * 48
        parent = struct.unpack_from('<i', storage, start + 0x28)[0]
        alternate = struct.unpack_from('<Q', storage, start + 0x20)[0]
        label = (struct.unpack_from('<Q', storage, start + 8)[0]
                 if alternate == 0xffffffffffffffff else alternate)
        primary.append(old_primary[parent] + [label & 0xffff])
        secondary.append(old_secondary[parent] + [struct.unpack_from('<H', storage, start + 0x10)[0]])
        auxiliary.append(old_auxiliary[parent] + [struct.unpack_from('<H', storage, start + 0x18)[0]])
    assert primary[0] == old_primary[parents[0]] + [42]
    if len(parents) > 1:
        assert primary[1][-1] == 0xffff  # not fallback to its candidate symbol
    assert [row[-1] for row in secondary] == [200 + at for at in range(len(parents))]
    assert [row[-1] for row in auxiliary] == [1 + at for at in range(len(parents))]
    assert json.dumps([old_primary, old_secondary, old_auxiliary]) == planes_before
    layout_controls.append(dict(parents=parents, primary=primary, secondary=secondary, auxiliary=auxiliary))

report = dict(
    regions=[dict(start=hex(start), end=hex(end), sha256=sha) for start, end, sha in regions],
    instructionPins=len(pins) + len(extra_pins), bindings=len(bindings),
    actualModelHashes=lifecycle['modelSha256'], actualDecoderOutputs=models[2]['io']['outputs'],
    defaultAllocatorProof=dict(dependency=str(dependency),
        scope='default Engine V18 -> HeapAlloc flag8 only; custom allocation excluded'),
    coldSeed=dict(prefixDescriptors='width primary/secondary pairs of 16-byte zero descriptors; optional aux only if EEE3C>=0',
        activeCount='EEE98=1 and EEF110=1', costs='EEF F0[0]=0 explicitly; unused priors not consumed',
        ancestryAllocation='attn_seg -> EF140=1 -> EED98=1 -> EEE38=1; 035D60 allocates and explicitly clears parent-history descriptors',
        ancestry='EEE88 per-rank I32 vectors append FFFFFFFF seed explicitly',
        prefixImport='037790 can populate supplied prefix; empty-list conclusion requires that branch absent'),
    actualFreshSidecars=dict(sourceInformation64='036250 first strict maximum of last-frame EEF D8 log-prob row; includes BOS before E8 mask',
        conditional32='FFFFFFFF because EF1120 detection_output=-1 -> EED9C -> EEE3C=-1',
        auxiliaryPlanePresent=False,
        unrelatedIndices='EF1D4=max_pred_counts K; EF1D8=classification_output -> EEE4C; neither is EF1120'),
    selectedUpdate=dict(parent='signed I32 raw48+28',
        primary='copy primary[parent], append lowU16(raw48+20 unless full qword is -1, else raw48+8)',
        secondary='copy secondary[parent], append lowU16(raw48+10)',
        auxiliary='if EEE3C>=0: copy auxiliary[parent], append lowU16(raw48+18)',
        inactive='destination counts cleared for every width slot; only active rows filled',
        ancestry='separate EEE88 rank/time vectors append actual parent, or -1 for inactive rank',
        handoff='prefix buffers swap before EEF.V40 selected-parent tensor update; same raw48 owner'),
    preparedArgmaxControls=argmax_controls, preparedPlaneControls=layout_controls,
    runtimePlan='New same-Step sidecar factory over retained log probabilities + immutable same-selection prefix owner; no arbitrary same-shaped foreign EosHandoff',
    exclusions=['Configured C8 alias expansion, imported/prefilled prefixes and custom allocators',
        'Optional detection/classification/LM tensors absent from the actual raw3 models',
        'Completed168 construction/output and maxstep fallback handled separately',
        'No complete search scheduler, final hypothesis or acceptance/confidence claim'],
    runtimeChanges=False, vendorExecution=False, ortExecution=False)
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print(f'PASS {len(regions)} regions/{len(pins) + len(extra_pins)} pins/{len(bindings)} bindings; '
      f'{argmax_controls} prepared argmax + {len(layout_controls)} three-plane controls; static only')
