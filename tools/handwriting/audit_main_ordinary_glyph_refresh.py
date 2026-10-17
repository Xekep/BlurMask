"""Static ordering evidence for the bounded actual ordinary217670 refresh."""
import argparse
import hashlib
import json
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('output', type=Path)
args = parser.parse_args()
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
start, end = 0x180217670, 0x180217DB0
assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == '8321a0ed9770f4e6f4ff036643ff8d1490ad6989dd616084b739dd5a2c0b176b'
assert b.qwords(0x1802E7CB0 + 0x90, 1)[0] == start
m = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {i.address: (i.mnemonic, i.op_str) for i in m.disasm(b.data[b.off(start):b.off(end)], start)}
pins = [
    (0x180217920, 'movss', 'dword ptr [rbp + 0x1c], xmm0'),
    (0x1802179D6, 'call', 'r10'),
    (0x180217A5E, 'call', '0x180216860'),
    (0x180217AAC, 'call', 'r10'),
    (0x180217B04, 'movss', 'dword ptr [rdi + 0x10], xmm0'),
    (0x180217BCC, 'mov', 'dword ptr [rdi + 0x14], eax'),
    (0x180217BDB, 'movzx', 'ecx, byte ptr [rax + r14*8 + 0x21]'),
    (0x180217BED, 'divss', 'xmm0, xmm6'),
    (0x180217BF1, 'movss', 'dword ptr [rdi + 0xc], xmm0'),
    (0x180217C16, 'mov', 'word ptr [rdi + 4], cx'),
]
for address, mnemonic, operands in pins:
    assert instructions[address] == (mnemonic, operands)
code = (root / 'src/InkRecognition/LegacyOwnedMainOrdinaryGlyphRefresh.cs').read_text(encoding='utf-8')
order = ['state.Source = SourceMetadata.Read(', 'state.SourceAllocation26 =',
         'state.SourceScoreBits =', 'pool.AppendDescriptor(', 'resolver.Resolve(',
         'LegacyNativeGlyphSymbolUpdate.Apply(', 'Scoring.ReadSymbolScoreBits(',
         'record.AsSpan(0x10)', 'Scoring.ReadPreparedSymbol(', 'record.AsSpan(0x14)',
         'record.AsSpan(0x0c)']
positions = [code.index(part) for part in order]
assert positions == sorted(positions)
assert 'index - first, false, allocatedRecordStorage40' in code
assert 'state.Source.BaseSourceKey) continue;' in code
assert 'progress.Add(canonicalOwnerKey, state)' in code
report = dict(function=[hex(start), hex(end)], pins=len(pins), binding='ordinary240.V90=217670',
              managedOrder=order, actualScope='Nonnegative ordinary CASE source owners; native scalar/pool order preserved',
              remaining=['negative/span sources', 'native retry behavior after a failed4000 refresh', 'full final string-wrapper acceptance'],
              noVendorExecution=True)
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print('PASS10 ordered native instructions/actual240.V90 binding/11 managed stage-order pins; no full-wrapper claim.')
