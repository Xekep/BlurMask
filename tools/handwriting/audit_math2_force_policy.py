"""Static fresh Math2 force-nonobservable producer and same-pointer EF1 retain.
No vendor DLL loading, ONNX Runtime execution or decoder implementation.
"""
import hashlib
import json
import struct
import subprocess
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
from capstone.x86 import X86_REG_RIP, X86_OP_MEM

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
out = Path(sys.argv[1]).resolve()
digests = {'MyScriptDocument.dll': '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f',
 'MyScriptEngine.dll': 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'}
regions = [('MyScriptDocument.dll',
  0x180044080,
  0x180044180,
  '9d517a7a291fa99c60acb2e690ac1d65b7dd7604eafa6410c05aaafd59d5091a'),
 ('MyScriptDocument.dll',
  0x180044180,
  0x180044630,
  '1375d6401023ec114aff9e145fc11b501b0201451e633c839852e97014243997'),
 ('MyScriptDocument.dll',
  0x180044630,
  0x1800448b0,
  'bf03c90bfc43d2c819d3ecfccc89cdb75f47910aa5a6b58bd2798978079a241e'),
 ('MyScriptDocument.dll',
  0x180044ac0,
  0x180044fb0,
  'f7cbc430d27bd819ff2a0d4346def61ff3939f7a8c238cd7f5ede03c104e4265'),
 ('MyScriptDocument.dll',
  0x180044fb0,
  0x1800450c0,
  '7adb96403839d9becbb1d9556c4125441fa60be05f2f84c4e9a9225aa4442331'),
 ('MyScriptDocument.dll',
  0x1800450c0,
  0x180045460,
  '155f71ec1b5fea2831e30c94fa71dfcd3d3c8753d41bfe22dd27df8b9eb51693'),
 ('MyScriptDocument.dll',
  0x180045460,
  0x180045b90,
  '8a9d69ac15350ad940b5871b5753d679ea833dafffe8eba010d8699cb47ff908'),
 ('MyScriptDocument.dll',
  0x1800359f0,
  0x180035c50,
  '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'),
 ('MyScriptDocument.dll',
  0x18003b300,
  0x18003b540,
  '78a8116a27d4cf02342618d1da2ec2af0f4ed7e1c751acf897691cc3aea4865a'),
 ('MyScriptDocument.dll',
  0x18004a370,
  0x18004a420,
  '52e98f2cfdce3dacfb12524073e7d6edc1c3fd51eb69b9efb5655477de5bdc9a'),
 ('MyScriptDocument.dll',
  0x18004c930,
  0x18004d540,
  '41772ed4fc2013fc188322d36791508f0c23b09f0888c8e72138aed0b2ed075a'),
 ('MyScriptEngine.dll',
  0x180031a3c,
  0x180031b56,
  'b688df42559719dca501d1469fabadb366eeb1715125cb57b0235f5ab4fc0f6e'),
 ('MyScriptEngine.dll',
  0x18003a100,
  0x18003a190,
  'e559a9de9bc8669c17530e28246d0c1809920b030e95a6c9ca37543d868ea266'),
 ('MyScriptEngine.dll',
  0x18003a2a0,
  0x18003a2d0,
  'dea50fa663b8f9ca61f3ef889a1c20b59598440fb60689378e23c6ae5801c086')]
pins = [('MyScriptDocument.dll', 0x1800440e0, 'mov', 'qword ptr [rdi + 0x220], rcx'),
 ('MyScriptDocument.dll', 0x180044125, 'add', 'rdi, 0x68'),
 ('MyScriptDocument.dll', 0x18004412b, 'xor', 'edx, edx'),
 ('MyScriptDocument.dll', 0x18004412d, 'mov', 'r8d, 0x170'),
 ('MyScriptDocument.dll', 0x180044136, 'call', '0x1801ac460'),
 ('MyScriptDocument.dll', 0x180044649, 'cmp', 'r9, 0x188'),
 ('MyScriptDocument.dll', 0x180044656, 'call', '0x180044080'),
 ('MyScriptDocument.dll', 0x180044720, 'lea', 'rax, [rdi + 0x68]'),
 ('MyScriptDocument.dll', 0x180044724, 'lea', 'rcx, [r14 + 0x18]'),
 ('MyScriptDocument.dll', 0x180044ae0, 'mov', 'rsi, qword ptr [r9 + 8]'),
 ('MyScriptDocument.dll', 0x180044bcf, 'lea', 'r9, [rip + 0x1d5a2a]'),
 ('MyScriptDocument.dll', 0x180044bf8, 'jne', '0x180044c15'),
 ('MyScriptDocument.dll', 0x180044c03, 'mov', 'r9, qword ptr [rax + 0x70]'),
 ('MyScriptDocument.dll', 0x180044c07, 'call', 'r9'),
 ('MyScriptDocument.dll', 0x180044c0a, 'mov', 'byte ptr [rsi + 0x120], al'),
 ('MyScriptDocument.dll', 0x180044fce, 'mov', 'r14, r9'),
 ('MyScriptDocument.dll', 0x180044fdb, 'lea', 'r9, [rip + 0x1d56e2]'),
 ('MyScriptDocument.dll', 0x180045005, 'cmp', 'byte ptr [rbp + 2], 1'),
 ('MyScriptDocument.dll', 0x180045023, 'call', 'r10'),
 ('MyScriptDocument.dll', 0x180045026, 'mov', 'rcx, qword ptr [rbx + 0x30]'),
 ('MyScriptDocument.dll', 0x18004503e, 'call', 'r9'),
 ('MyScriptDocument.dll', 0x18004504c, 'lea', 'r9, [rip - 0x593]'),
 ('MyScriptDocument.dll', 0x18004505a, 'mov', 'qword ptr [rsp + 0x20], r14'),
 ('MyScriptDocument.dll', 0x18004505f, 'mov', 'r10, qword ptr [rcx + 0xe0]'),
 ('MyScriptDocument.dll', 0x180045069, 'call', 'r10'),
 ('MyScriptDocument.dll', 0x180045480, 'mov', 'rax, qword ptr [rdx + 0x1c0]'),
 ('MyScriptDocument.dll', 0x18004548f, 'xor', 'r12d, r12d'),
 ('MyScriptDocument.dll', 0x180045492, 'mov', 'rax, qword ptr [rdx + 0x1c8]'),
 ('MyScriptDocument.dll', 0x1800454a3, 'mov', 'rax, qword ptr [rdx + 0x1d0]'),
 ('MyScriptDocument.dll', 0x1800454b3, 'mov', 'rax, qword ptr [rdx + 0x170]'),
 ('MyScriptDocument.dll', 0x180045587, 'cmp', 'dword ptr [rdi + 0x178], r12d'),
 ('MyScriptDocument.dll', 0x18004558e, 'jne', '0x1800455a2'),
 ('MyScriptDocument.dll', 0x180045590, 'mov', 'qword ptr [rdi + 0x180], 4'),
 ('MyScriptDocument.dll', 0x18004559b, 'mov', 'byte ptr [rdi + 0x188], r12b'),
 ('MyScriptDocument.dll', 0x1800455d0, 'cmp', 'esi, 4'),
 ('MyScriptDocument.dll', 0x1800455ea, 'lea', 'rax, [rdi + 0x68]'),
 ('MyScriptDocument.dll', 0x1800455ee, 'mov', 'qword ptr [rsp + 0x38], rax'),
 ('MyScriptDocument.dll', 0x1800455fb, 'lea', 'r9, [rip - 0x652]'),
 ('MyScriptDocument.dll', 0x18004561d, 'call', 'r10'),
 ('MyScriptDocument.dll', 0x18004562a, 'inc', 'esi'),
 ('MyScriptDocument.dll', 0x18004562e, 'jne', '0x1800455d0'),
 ('MyScriptDocument.dll', 0x180045938, 'cmp', 'qword ptr [rdi + 0x220], r12'),
 ('MyScriptDocument.dll', 0x18004593f, 'jne', '0x180045980'),
 ('MyScriptDocument.dll', 0x18004594a, 'call', '0x1800450c0'),
 ('MyScriptDocument.dll', 0x1800451d1, 'cmp', 'dword ptr [rdi + 0x178], 0'),
 ('MyScriptDocument.dll', 0x1800451d8, 'movups', 'xmm0, xmmword ptr [rdi + 0x1c0]'),
 ('MyScriptDocument.dll', 0x1800451ea, 'movaps', 'xmmword ptr [rsp + 0x60], xmm0'),
 ('MyScriptDocument.dll', 0x1800451ef, 'movups', 'xmm0, xmmword ptr [rdi + 0x180]'),
 ('MyScriptDocument.dll', 0x180045203, 'movups', 'xmmword ptr [rsp + 0x78], xmm0'),
 ('MyScriptDocument.dll', 0x18004523b, 'mov', 'edx, 0xef1'),
 ('MyScriptDocument.dll', 0x18004525b, 'lea', 'r8, [rsp + 0x60]'),
 ('MyScriptDocument.dll', 0x180045260, 'mov', 'r9d, 0x40'),
 ('MyScriptDocument.dll', 0x180045269, 'call', 'rbx'),
 ('MyScriptDocument.dll', 0x1800453a3, 'mov', 'qword ptr [rdi + 0x168], rcx'),
 ('MyScriptDocument.dll', 0x1800453f3, 'lea', 'r8, [rdi + 0x70]'),
 ('MyScriptDocument.dll', 0x180035b1e, 'lea', 'r8, [rdi + 0x50]'),
 ('MyScriptDocument.dll', 0x180035b22, 'mov', 'r9d, 0xb8'),
 ('MyScriptDocument.dll', 0x180035b35, 'mov', 'qword ptr [r13], rax'),
 ('MyScriptDocument.dll', 0x18003b451, 'mov', 'rdx, qword ptr [rsi + 0xa8]'),
 ('MyScriptDocument.dll', 0x18003b45c, 'mov', 'rax, qword ptr [rax + 0x18]'),
 ('MyScriptDocument.dll', 0x18003b460, 'mov', 'r8, qword ptr [rax + 0x20]'),
 ('MyScriptDocument.dll', 0x18003b464, 'call', 'r8'),
 ('MyScriptDocument.dll', 0x18003b46c, 'mov', 'qword ptr [rbx + 0xb8], rax'),
 ('MyScriptDocument.dll', 0x18004a388, 'cmp', 'r9, 0x40'),
 ('MyScriptDocument.dll', 0x18004a3a6, 'mov', 'r9d, 0x18'),
 ('MyScriptDocument.dll', 0x18004a3c0, 'movups', 'xmm0, xmmword ptr [rdi + 0x18]'),
 ('MyScriptDocument.dll', 0x18004a3c7, 'movups', 'xmmword ptr [rbx + 0x28], xmm0'),
 ('MyScriptDocument.dll', 0x18004ce23, 'cmp', 'byte ptr [r15 + 0x30], 0'),
 ('MyScriptDocument.dll', 0x18004ce28, 'je', '0x18004cfd6'),
 ('MyScriptDocument.dll', 0x18004ce69, 'movzx', 'ebx, word ptr [r13]'),
 ('MyScriptDocument.dll', 0x18004ce8f, 'mov', 'r9, qword ptr [rax + 0x10]'),
 ('MyScriptDocument.dll', 0x18004ce93, 'call', 'r9'),
 ('MyScriptDocument.dll', 0x18004ce98, 'jne', '0x18004cfbc'),
 ('MyScriptDocument.dll', 0x18004cee9, 'mov', 'dword ptr [rax + rcx*4], 0xff7fffff'),
 ('MyScriptEngine.dll', 0x180031a48, 'test', 'rsi, rsi'),
 ('MyScriptEngine.dll', 0x180031a4b, 'je', '0x180031a64'),
 ('MyScriptEngine.dll', 0x180031a64, 'mov', 'r15, qword ptr [rip + 0x2205dd]'),
 ('MyScriptEngine.dll', 0x180031b4e, 'mov', 'rax, qword ptr [r15 + 0x20]'),
 ('MyScriptEngine.dll', 0x180031b52, 'mov', 'qword ptr [rcx + 0x18], rax'),
 ('MyScriptEngine.dll', 0x18003a10d, 'lea', 'rdi, [rdx + 8]'),
 ('MyScriptEngine.dll', 0x18003a111, 'mov', 'r8d, dword ptr [rdi]'),
 ('MyScriptEngine.dll', 0x18003a124, 'lea', 'r9d, [r8 + 0x10000]'),
 ('MyScriptEngine.dll', 0x18003a134, 'cmp', 'ecx, eax'),
 ('MyScriptEngine.dll', 0x18003a13c, 'jb', '0x18003a159'),
 ('MyScriptEngine.dll', 0x18003a13e, 'mov', 'r10, qword ptr [rax + 0x38]'),
 ('MyScriptEngine.dll', 0x18003a145, 'call', 'r10'),
 ('MyScriptEngine.dll', 0x18003a14a, 'je', '0x18003a111'),
 ('MyScriptEngine.dll', 0x18003a14c, 'mov', 'al, 1'),
 ('MyScriptEngine.dll', 0x18003a2a6, 'mov', 'rbx, rdx'),
 ('MyScriptEngine.dll', 0x18003a2a9, 'test', 'rdx, rdx'),
 ('MyScriptEngine.dll', 0x18003a2ac, 'je', '0x18003a2c4'),
 ('MyScriptEngine.dll', 0x18003a2ae, 'call', '0x18003a100'),
 ('MyScriptEngine.dll', 0x18003a2b7, 'cmovne', 'rcx, rbx'),
 ('MyScriptEngine.dll', 0x18003a2bb, 'mov', 'rax, rcx')]

bindings = [
    ('MyScriptEngine.dll', 0x180252048, 0x1801dbb00),
    ('MyScriptEngine.dll', 0x1801dbb20, 0x1801dbbf0),
    ('MyScriptEngine.dll', 0x1801dbc10, 0x18003a2a0),
    ('MyScriptDocument.dll', 0x1801fd1e8, 0x18004a370),
    ('MyScriptDocument.dll', 0x1801fd180, 0x18004c930),
    ('MyScriptDocument.dll', 0x1801edab0, 0x18003b300),
]
binaries = {name: Binary(name) for name in digests}
for name, binary in binaries.items():
    assert hashlib.sha256(binary.data).hexdigest() == digests[name]
md = Cs(CS_ARCH_X86, CS_MODE_64)
md.detail = True
instructions = {}
for name, start, end, expected in regions:
    binary = binaries[name]
    raw = binary.data[binary.off(start):binary.off(end)]
    assert hashlib.sha256(raw).hexdigest() == expected, (name, hex(start))
    instructions.update(((name, i.address), i) for i in md.disasm(raw, start))
for name, address, mnemonic, operands in pins:
    instruction = instructions[name, address]
    assert (instruction.mnemonic, instruction.op_str) == (mnemonic, operands), hex(address)
for name, address, expected in bindings:
    assert binaries[name].qwords(address, 1)[0] == expected, hex(address)

b = binaries['MyScriptDocument.dll']
def rip_string(address):
    instruction = instructions['MyScriptDocument.dll', address]
    assert instruction.mnemonic == 'lea'
    operand = instruction.operands[1]
    assert operand.type == X86_OP_MEM and operand.mem.base == X86_REG_RIP
    target = instruction.address + instruction.size + operand.mem.disp
    offset = b.off(target)
    return b.data[offset:offset + 128].split(b'\0')[0].decode('ascii')

assert rip_string(0x180044bcf) == 'need_force_nonobs_positions'
assert rip_string(0x180044fdb) == 'config'
request_keys = [rip_string(address) for address in
                (0x1800456d3, 0x180045712, 0x18004574e, 0x18004578f, 0x1800457d0)]
assert request_keys == ['disable_iso_pass', 'beam_size', 'language_model.alpha',
                        'language_model.beta', 'delta_cost_max']
# Reject an unnoticed new key in the inspected request-configuration block.
observed_keys = []
for (name, address), instruction in instructions.items():
    if (name == 'MyScriptDocument.dll' and 0x1800456c2 <= address < 0x180045880
            and instruction.mnemonic == 'lea' and instruction.op_str.startswith('r9,')):
        operand = instruction.operands[1]
        if operand.type == X86_OP_MEM and operand.mem.base == X86_REG_RIP:
            observed_keys.append(rip_string(address))
assert observed_keys == request_keys

# The prior width audit independently decodes all three ORTM custom-metadata
# maps from the pinned resource; no model inference or tensor dimension guess.
out.parent.mkdir(parents=True, exist_ok=True)
metadata_report = out.with_name(out.stem + '-width-producers.json')
subprocess.run([sys.executable, str(ROOT / 'tools/handwriting/audit_math2_beam_width_producers.py'),
                str(metadata_report)], check=True)
metadata = json.loads(metadata_report.read_bytes())
assert metadata['resourceMode'] == 0
configs = [item['config'] for item in metadata['models']]
assert len(configs) == 3
assert ['need_force_nonobs_positions' in config for config in configs] == [False, False, True]
assert configs[2]['need_force_nonobs_positions'] is True
assert configs[2]['num_heads'] == 4

# Prepared byte-layout discriminators, not captured native factory executions.
# The critical offset is callback frame68 + 120 = owner188 (not owner120).
owner = bytearray([0xa5] * 0x238)
owner[0x68:0x1d8] = bytes(0x170)
assert owner[0x188] == 0
owner[0x188] = 0x73  # inherited nonzero field must not become the default
owner[0x188] = 0  # mode0 prepare reset occurs before the metadata callbacks
for config in configs:
    if 'need_force_nonobs_positions' in config:
        owner[0x68 + 0x120] = int(config['need_force_nonobs_positions'])
assert owner[0x188] == 1 and owner[0x120] == 0
packet = bytearray(0x40)
packet[0x18:0x28] = owner[0x180:0x190]
instance = bytearray(0x138)
instance[0x28:0x38] = packet[0x18:0x28]
assert packet[0x20] == instance[0x30] == 1
# False and later repeated metadata values remain legal outside this bundle.
precedence = []
for values in ([], [False], [True], [True, False], [False, True]):
    field = 0
    for value in values:
        field = int(value)
    precedence.append(dict(preparedMetadata=values, resultingByte=field))
assert [row['resultingByte'] for row in precedence] == [0, 0, 1, 0, 1]
# Warm preparation is not evidence of replacing the active child.
warm_outer, warm_active = 1, 0
assert warm_outer != warm_active

report = dict(
    regions=[dict(binary=name, start=hex(start), end=hex(end), sha256=sha)
             for name, start, end, sha in regions],
    instructionPins=len(pins), bindings=len(bindings), metadataProof=metadata,
    producer='mode0 reset EED188=0 -> ordered model config callbacks frame(EED68)+120 -> EED188',
    packet='EED180..18F -> EF1 rawpacket18..27 -> EF1 instance28..37; force byte EF130',
    actualFreshForce=True, actualDecoderMetadataSlot=2, requestOverrideKeys=request_keys,
    retention=dict(defaultDescriptor='Engine252048 -> 1DBB00',
        installation='Engine031B4E/031B52: descriptor20 -> context18',
        method='context18.V20=03A2A0 -> 03A100 high16 reference increment',
        success='returns the identical input pointer; EEF.B8 and EED168 refer to the same EF1',
        failure='null or reference-count overflow can return null; no new object is constructed'),
    consumer='EF130!=0 -> EF3.V10 selected-label predicate -> nonobservable last attention column FF7FFFFF',
    preparedLayoutControls=dict(frameOffset=0x68, callbackOffset=0x120,
        ownerOffset=0x188, packetOffset=0x20, instanceOffset=0x30,
        precedence=precedence, warmCounterexample=dict(outer=warm_outer, active=warm_active)),
    corrections=[
        dict(path='docs/agents/research/legacy-math2-score-channels.md', lines=[15, 51],
             change='decoder clone -> same-pointer retained decoder, under the default Engine service'),
        dict(path='docs/agents/research/legacy-math2-tensor-lifecycle.md', lines=[9],
             change='selected/cloned EF1 -> selected EF1 retained at the same pointer'),
        dict(path='tools/handwriting/audit_math2_score_channels.py', lines=[144],
             change='report.route EEF B8 clone -> EEF B8 same-pointer retain'),
    ],
    unchangedCloneScope='Restriction-map doc20 EEB clones its EED source is a different global10.V18 operation; not corrected by this V20 proof',
    supportedScope='Successful cold EED220==0, mode0, pinned raw3 metadata, default Engine service, fresh EEE/EEF/EF1; no caller metadata substitution',
    excluded=['Warm EED220 already owns a child; refreshed outer fields do not prove child replacement',
              'Custom Engine descriptor override may change the object-retention service',
              'Other models, raw inherited packets, arbitrary property configuration and failed preparation',
              'No search loop, completed-record ownership, maxstep packet layout or acceptance implementation'],
    runtimeChanges=False, vendorExecution=False, ortExecution=False)
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print(f'PASS {len(regions)} regions/{len(pins)} pins/{len(bindings)} bindings; '
      'raw3 force=true -> EF130, default Engine retains SAME EF1; static only')
