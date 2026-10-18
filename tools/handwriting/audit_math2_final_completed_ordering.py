"""Final038610/035000 owning-tail prerequisites; vendor bytes read only."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv) != 2:
    raise SystemExit('report.json')
root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary

binary = Binary('MyScriptDocument.dll')
assert hashlib.sha256(binary.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions = [
    (0x180034d10, 0x180034ea0, '13c99fa9bbedf8763901cd6d22410ee60c8e6a7fe0a750499d410bd850385232'),
    (0x180035000, 0x180035110, 'bde2b83414f859683bf2fea706f024d5772c91d7a9e4946169384dcd5c649fd4'),
]
instructions = {}
for start, end, expected in regions:
    raw = binary.data[binary.off(start):binary.off(end)]
    assert hashlib.sha256(raw).hexdigest() == expected
    instructions.update({i.address: (i.mnemonic, i.op_str)
                         for i in Cs(CS_ARCH_X86, CS_MODE_64).disasm(raw, start)})
pins = [
    (0x180035011, 'mov', 'ecx, dword ptr [rdx + 8]'),
    (0x18003501a, 'cmp', 'r8d, ecx'), (0x18003501d, 'jae', '0x1800350fb'),
    (0x180035023, 'test', 'r9d, r9d'), (0x180035026, 'je', '0x1800350fb'),
    (0x180035031, 'sub', 'ecx, r15d'),
    (0x180035037, 'imul', 'rdx, r15, 0xa8'), (0x180035043, 'add', 'rbx, rdx'),
    (0x180035050, 'cmp', 'ecx, ebp'), (0x180035052, 'ja', '0x18003507e'),
    (0x180035060, 'mov', 'rdx, rbx'), (0x180035066, 'call', '0x180034d10'),
    (0x18003506b, 'add', 'rbx, 0xa8'), (0x180035072, 'sub', 'rdi, 1'),
    (0x180035076, 'jne', '0x180035060'),
    (0x180035078, 'mov', 'dword ptr [r14 + 8], r15d'),
    (0x18003507c, 'jmp', '0x1800350ec'),
    (0x180035090, 'mov', 'rdx, rbx'), (0x180035096, 'call', '0x180034d10'),
    (0x1800350e3, 'call', '0x1801ac820'),
    (0x180034d26, 'mov', 'rdx, qword ptr [rdx + 8]'),
    (0x180034d42, 'mov', 'rdx, qword ptr [rbp + 0x18]'),
    (0x180034d5f, 'call', '0x180026a80'),
    (0x180034d84, 'mov', 'rdx, qword ptr [rbp + 0x40]'),
    (0x180034d9d, 'mov', 'rdx, qword ptr [rbp + 0x50]'),
    (0x180034db6, 'mov', 'rdx, qword ptr [rbp + 0x68]'),
    (0x180034e41, 'mov', 'rdx, qword ptr [rbp + 0x88]'),
    (0x180034e54, 'call', 'r8'),
    (0x180034e5e, 'mov', 'rdx, qword ptr [rbp + 0x98]'),
    (0x180034e7a, 'mov', 'rdx, qword ptr [rbp + 0x90]'),
]
for address, mnemonic, operand in pins:
    assert instructions[address] == (mnemonic, operand), (hex(address), instructions.get(address))
out = Path(sys.argv[1]).resolve()
out.parent.mkdir(parents=True, exist_ok=True)
dependency = out.with_name(out.stem + '-caller-numeric.json')
subprocess.run([sys.executable, str(root / 'tools/handwriting/audit_math2_initial_completed_ordering.py'),
                str(dependency)], check=True)
caller = json.loads(dependency.read_bytes())
assert caller['instructionPins'] == 33
report = dict(regions=[dict(start=hex(a), end=hex(b), sha256=h) for a, b, h in regions],
    instructionPins=len(pins), callerNumericDependency=str(dependency),
    finalCaller='039BF2 count>1 ->039C0C036810 ->count>sameSavedWidth ->039C26035000(width,count-width)',
    release='Full-tail branch visits ascending168B records then writes count=width once; no memmove on this branch',
    payloadOrder='Primary8,secondary18,nested30,40,units50,runs68,sidecars78,Library88,aux98/90',
    scope='Success-only raw-only EF4 token; failure-exit native final-tail orchestration remains separate',
    representation='Ordinal168 projection is portable non-ABI; actual Record/Source/Library and EF4 payload pair by identity',
    vendorExecution=False)
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print(f'PASS2 release regions/{len(pins)} pins +caller33/numeric036810; no native execution')
