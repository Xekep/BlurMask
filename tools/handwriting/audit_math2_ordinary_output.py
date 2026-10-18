"""Static no-rule map/full-unit copy + semantic unit-row/interner ownership.

Extends the complete output-route proof; vendor binaries are read as data only.
"""
import hashlib
import json
from pathlib import Path
import runpy
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv) != 2:
    raise SystemExit('report.json')
root = Path(__file__).resolve().parents[2]
output = Path(sys.argv[1]).resolve()
output.parent.mkdir(parents=True, exist_ok=True)
original = sys.argv[:]
try:
    sys.argv = [str(root/'tools/handwriting/audit_math2_output_route.py'), str(output.with_suffix('.route.json'))]
    runpy.run_path(sys.argv[0], run_name='__main__')
finally:
    sys.argv = original
route = json.loads(output.with_suffix('.route.json').read_bytes())
sys.path.insert(0, str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b = Binary('MyScriptDocument.dll')
starts = [(0x1800251e0, 0x180025980), (0x180043650, 0x180043740), (0x180045c60, 0x1800463e0)]
ins = {}
for start, end in starts:
    ins.update({item.address: (item.mnemonic, item.op_str) for item in
        Cs(CS_ARCH_X86, CS_MODE_64).disasm(b.data[b.off(start):b.off(end)], start)})
pins = [
    (0x180025828, 'movzx', 'ecx, byte ptr [rbp - 0x4a]'),
    (0x18002582c, 'cmp', 'byte ptr [rbp - 0x52], cl'),
    (0x180025835, 'cmp', 'dx, ax'),
    (0x1800258c8, 'mov', 'word ptr [rcx], r12w'),
    (0x1800258db, 'inc', 'r10w'),
    (0x18002591f, 'mov', 'dword ptr [rdx], r15d'),
    (0x180025922, 'inc', 'dword ptr [rsi + 8]'),
    (0x180043676, 'mov', 'r8d, 0x28'),
    (0x1800436d0, 'mov', 'qword ptr [rbx - 8], r15'),
    (0x1800436d8, 'mov', 'qword ptr [rbx], r15'),
    (0x1800436ea, 'mov', 'r8, qword ptr [rax + 0x60]'),
    (0x1800436ee, 'call', 'r8'),
    (0x1800436f1, 'mov', 'byte ptr [rbx + 8], r15b'),
    (0x1800436f5, 'mov', 'dword ptr [rbx + 0xc], 0xffffffff'),
    (0x180045e22, 'mov', 'rax, qword ptr [r15]'),
    (0x180045e25, 'mov', 'rdx, r15'),
    (0x180045e30, 'mov', 'r8, qword ptr [rsp + 0x48]'),
    (0x180045e35, 'add', 'r9, r8'),
    (0x180045e41, 'mov', 'r10, qword ptr [rax + 0x10]'),
    (0x180045e45, 'call', 'r10'),
    (0x180045e78, 'movaps', 'xmm6, xmm8'),
    (0x180045e95, 'mov', 'r8d, 0x10'),
    (0x180045ee9, 'mov', 'qword ptr [rdx], r15'),
    (0x180045eec, 'mov', 'dword ptr [rdx + 8], r14d'),
    (0x180045ef0, 'movss', 'dword ptr [rdx + 0xc], xmm6'),
    (0x180045ef5, 'inc', 'dword ptr [rdi + rsi*8 + 0x18]'),
]
for address, mnemonic, operands in pins:
    assert ins[address] == (mnemonic, operands), (hex(address), ins.get(address))
# Verify the runtime policy's complete supported grammar-ID refusal set against
# the actual table, without substituting a guessed observable/Unicode rule.
policy = (root/'src/InkRecognition.Math/Math2Mode0RawOnlyOutputPolicy.cs').read_text(encoding='utf-8-sig')
for rule in route['rules']:
    for name in rule['names']:
        escaped = json.dumps(name, ensure_ascii=False)
        if name == '\u2402': escaped = '"\\u2402"'
        if name == '\u2403': escaped = '"\\u2403"'
        assert escaped in policy, (name, escaped)
source = (root/'src/InkRecognition.Math/Math2Mode0RawOnlyOutput.cs').read_text(encoding='utf-8-sig')
assert source.index('PlanOrdinaryOutput(this, retained') < source.index('outputClaimed = true')
assert source.index('CopyIdentityMapPrepared(row.Units)') < source.index('NormalizeDecimalPrepared(units') < source.index('row.Library.InternOutput(')
assert 'new LegacyNativeUtf8StringPool' not in source
output.write_text(json.dumps(dict(dependency=route, additionalPins=len(pins),
    scope='Whole-vector ordinary no-rule-hit EE4 identity map/full32 unit copy; decimal low16 rewrite; SAME Library88 interval interning and semantic40/16 rows',
    representation='Portable typed identities/known semantic fields only; EngineContext20.V60 opaque row lane and ABI padding are not manufactured',
    vendorExecution=False), indent=2)+'\n', encoding='utf-8')
print(f'PASS ordinary output {len(pins)} additional pins + complete15region/83pin/14rule route')
