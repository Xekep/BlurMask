"""Actual owning EE4 handoff and supplemental literal branch pins.

Vendor files are read as data. Runtime numerical ownership is audited separately.
"""
import json
from pathlib import Path
import runpy
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv) != 2:
    raise SystemExit('report.json')
root = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1]).resolve(); out.parent.mkdir(parents=True, exist_ok=True)
old = sys.argv[:]
try:
    sys.argv = [str(root/'tools/handwriting/audit_math2_ee4_formatter.py'), str(out.with_suffix('.parser.json'))]
    runpy.run_path(sys.argv[0], run_name='__main__')
finally:
    sys.argv = old
parser = json.loads(out.with_suffix('.parser.json').read_bytes())
sys.path.insert(0, str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b = Binary('MyScriptDocument.dll')
ins = {}
for region in parser['regions']:
    start, end = int(region['start'], 16), int(region['end'], 16)
    ins.update({item.address: (item.mnemonic, item.op_str) for item in
        Cs(CS_ARCH_X86, CS_MODE_64).disasm(b.data[b.off(start):b.off(end)], start)})
pins = [
    (0x180023e6e, 'cmp', 'eax, 2'), (0x180023e73, 'cmp', 'eax, 3'),
    (0x180023e94, 'mov', 'byte ptr [rcx + rdx + 0x20], al'),
    (0x180023ed3, 'mov', 'byte ptr [rax + rcx + 0x20], 1'),
    (0x180024586, 'mov', 'eax, 0xc408'), (0x18002458b, 'bt', 'eax, ecx'),
    (0x180025bda, 'inc', 'word ptr [r14 + rax*2]'),
    (0x180025c0d, 'cmp', 'eax, 1'), (0x180025c12, 'cmp', 'eax, 4'),
    (0x180025c1c, 'mov', 'ebx, edi'),
    (0x180025e90, 'mov', 'word ptr [r15 + rax*4], 0xffff'),
    (0x1800254e9, 'cmp', 'dx, cx'),
    (0x180025545, 'mov', 'word ptr [rbp - 0x28], ax'),
    (0x18002566d, 'mov', 'word ptr [rbp - 0x24], ax'),
    (0x1800258c8, 'mov', 'word ptr [rcx], r12w'),
    (0x18002591f, 'mov', 'dword ptr [rdx], r15d'),
    (0x180025fe3, 'call', '0x180024a20')]
for address, mnemonic, operands in pins:
    assert ins[address] == (mnemonic, operands), (hex(address), ins.get(address))
source = (root/'src/InkRecognition.Math/Math2Mode0RawOnlyOutput.cs').read_text(encoding='utf-8-sig')
policy = (root/'src/InkRecognition.Math/Math2Mode0RawOnlyOutputPolicy.cs').read_text(encoding='utf-8-sig')
assert 'FormatFullCold(CancellationToken' in source and 'FormatOrdinary(CancellationToken' in source
assert source.index('PlanOutput(this, retained') < source.index('outputClaimed = true')
assert source.count('private bool outputClaimed;') == 1
assert source.index('policy.Format(row.Units') < source.index('owner.Ef4.StoreOutputRuns(') < source.index('policy.NormalizeDecimalPrepared(') < source.index('row.Library.InternOutput(')
assert 'formatter == null && grammar.Contains(label)' in policy
assert 'internal LegacyMath2Ee4Formatter.Projection Format(' in policy
assert 'MaximumFormattedUnits' in source and 'FormatterWorkspace' in source and 'FullStringEnvelope' in source
assert 'if (!policy.HasFullFormatter) owner.Ef4.StoreOutputUnits' in source
assert 'row.Library.InternOutput' in source and 'new LegacyNativeUtf8StringPool' not in source
out.write_text(json.dumps(dict(parser=parser, additionalPins=len(pins),
    scope='Mode0/null external eligibility; actual raw-only same AL; full14-rule parser -> source-position map/runs -> decimal -> SAME Library intervals; ordinary guards unchanged',
    allocation='Portable managed syntax/frame/vector representation and bounded payload; no native ABI or allocator-order equivalence',
    vendorExecution=False), indent=2)+'\n', encoding='utf-8')
print(f'PASS owning EE4 {len(pins)} supplemental pins +13 complete parser regions/45field/70call pins +complete output route')
