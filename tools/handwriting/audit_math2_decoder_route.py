"""Static Math2 decoder ownership/configuration, not a restored beam search."""
import hashlib
import json
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root/'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions = [
    (0x1800450c0, 0x180045460, '155f71ec1b5fea2831e30c94fa71dfcd3d3c8753d41bfe22dd27df8b9eb51693'),
    (0x18004a1f0, 0x18004a2e5, '50f162c27e94c5da66106cc94c70ad34858cf2ed604c024b99faa1e5849cada5'),
    (0x18004a370, 0x18004a420, '52e98f2cfdce3dacfb12524073e7d6edc1c3fd51eb69b9efb5655477de5bdc9a'),
]
for lo, hi, expected in regions:
    assert hashlib.sha256(b.data[b.off(lo):b.off(hi)]).hexdigest() == expected
md = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {i.address: (i.mnemonic, i.op_str) for lo, hi in
                [(0x1800450c0, 0x180045460), (0x180047c50, 0x1800485b0)]
                for i in md.disasm(b.data[b.off(lo):b.off(hi)], lo)}
pins = {
    0x1800450e2: ('mov', 'r14d, dword ptr [r8]'),
    0x180045119: ('jg', '0x180045127'),
    0x18004511b: ('mov', 'r14d, dword ptr [rdi + 0x90]'),
    0x180045125: ('jle', '0x180045168'),
    0x180045127: ('cmp', 'r14d, 0x10'),
    0x18004512b: ('jbe', '0x18004516e'),
    0x180045143: ('mov', 'edx, 0x3080003'),
    0x180045168: ('mov', 'r14d, 1'),
    0x1800451d1: ('cmp', 'dword ptr [rdi + 0x178], 0'),
    0x180045210: ('jne', '0x1800452b1'),
    0x180045227: ('mov', 'qword ptr [rsp + 0x88], r13'),
    0x18004523b: ('mov', 'edx, 0xef1'),
    0x180045260: ('mov', 'r9d, 0x40'),
    0x1800452e0: ('mov', 'edx, 0xef2'),
    0x180045305: ('mov', 'r9d, 0x58'),
    0x180048106: ('mov', 'rdx, qword ptr [r13 + 0x220]'),
    0x180048127: ('mov', 'rax, qword ptr [rdx]'),
    0x180048144: ('mov', 'r10, qword ptr [rax + 0x28]'),
    0x180048168: ('call', 'r10'),
}
for address, expected in pins.items():
    assert instructions[address] == expected, (hex(address), instructions[address])
bindings = [
    (0x1801fd190+0x18, 0x1801fd118), #EF1 factory table, not its primary VT.
    (0x1801fd118+8, 0x1801fd150),
    (0x1801fd190+0x48, 0x18004a2f0),
    (0x1801fd190+0x58, 0x18004a370),
    (0x1801fd150+0x20, 0x18004a710),
    (0x1801fd150+0x28, 0x18004b9e0),
    (0x1801fd150+0x30, 0x18004c930),
    (0x1801ed9a0+0x18, 0x1801ed960),
    (0x1801ed960+8, 0x1801ed930),
    (0x1801ed930+0x28, 0x18003a260),
]
for address, target in bindings:
    assert b.qwords(address, 1)[0] == target
report = dict(
    pins=len(pins), tableBindings=len(bindings),
    regions=[dict(start=hex(lo), end=hex(hi), sha256=h) for lo, hi, h in regions],
    beam='signed requested>0 else signed saved90>0 else1; selected unsigned width must be<=16',
    decoder='owner178==0 ->EF1 initialized with64B packet; other modes ->EF2 with88B packet',
    driver='EED047C50 ->same EEE owner220.V28=03A260; EF1 tensor/state callbacks remain distinct',
    notPorted=['EEE beam driver03A260', 'candidate scoring/pruning and constraint producers',
               'EF1 tensor/state ownership', 'native model/user beam-width producer',
               'final grammar and structural acceptance'],
    appChange=False, noVendorExecution=True)
output = Path(sys.argv[1])
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print(f'PASS {len(pins)} Math2 decoder instruction pins/{len(bindings)} table bindings/3 regions; static route only, no beam implementation claim')
