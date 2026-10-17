"""Static PE evidence for213C00, its2144F0 caller and fresh ordinary kind7.

Only reads vendor PE bytes; does not load or execute any vendor library.
"""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
blocks = [
    (0x180213c00, 0x180214250, '92ee67346af5b9064308d0c27f05136ce50668104c6f235bcef0068ebc50e77a'),
    (0x1801a83c0, 0x1801a8670, '0f60f64be2a2bff280ae6186e5c6daae95cdd9f379298927bcd61845a78aae23'),
    (0x1801fcf40, 0x1801fd240, '53e84b3af4a85b9dc3a66bdff89eb87b50013b3057d49b2a420cce69bd78d01e'),
]
for start, end, expected in blocks:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == expected
pins = {
    #Native caller owns LIMIT34; this is NOT the pointed costcell initializer.
    0x180214529: '488d4c2460', 0x18021452e: '48894df8', 0x180214535: 'c7450400000004',
    #Source eligibility and frontend110 precede all request/context work.
    0x180213c81: '81e1001e0000', 0x180213c8e: '83fa06', 0x180213cbf: '3d000a0000',
    0x180213d32: '4c8b9010010000', 0x180213d39: '41ffd2', 0x180213d3e: '0f84e3040000',
    #Soft request label versus distinct ASCII-dash geometry/record label.
    0x180213dd2: '66894f24', 0x180213de2: '410fb797c0240000', 0x180213df2: '0fb6140a',
    0x180213df6: '885712', 0x180213e01: '48c7471403000000', 0x180213e09: '4488771c',
    0x180213e29: '894f04', 0x180213e4a: '6689570e', 0x180213e62: '66894f10',
    0x180213e7a: '66894f26', 0x180213e95: '885749', 0x180213ec2: '884f13',
    0x180213ed6: '4438b0a0000000', 0x180213edd: '0f44ca', 0x180213f23: 'e8a8d9e7ff',
    #Context lookup precedes F0, then per-arc increment/Cost reset and118/140/128.
    0x180213f5e: '4c8b88f0000000', 0x180213f65: '41ffd1',
    0x180213fcf: '894730', 0x180213fd8: '4c8b90f8000000',
    0x180213fe2: '6689470c', 0x180213ff0: '8930',
    0x18021400d: '4c8b8818010000', 0x180214038: '4c8b8840010000', 0x180214064: '4c8b8828010000',
    0x1802140f0: '450fb797c0240000', 0x180214194: '410fb6040a',
    0x1802141dd: 'c644243001', 0x1802141f0: 'e86b4bfeff',
    #Actual caller E8 and parent flag gates before213C00, before201270.
    0x1802150f3: '3bc7', 0x180215112: 'f644080601', 0x180215119: '837c247001',
    0x180215125: '663986c2240000', 0x18021512e: '663986c0240000', 0x18021514e: 'e8adeaffff',
    #Continuation parent flag selects REQUEST kind7, not query kind.
    0x180201818: '42f644100601', 0x180201820: '41b007',
    #1A83C0 flag10 before flag8/context;195C40 rejects native statuses0/2.
    0x1801a83f8: '41f6420610', 0x1801a8403: 'c70000000004', 0x1801a8417: '41f6420608',
    0x1801a84c8: '803c0100', 0x1801a84cc: '0f8564010000',
    0x180195e0f: '488b8510010000', 0x180195e25: 'a9fdffffff', 0x180195e30: '83f804',
    #1FCF40 follows actual parent links and excludes rootFFF9..FFFC withushort wrap.
    0x1801fcfdf: '4438b1a0000000', 0x1801fd012: '488b01', 0x1801fd020: '0fb74220',
    0x1801fd024: '6683c007', 0x1801fd028: '6683f803', 0x1801fd035: '418d43fa',
}
for address, expected in pins.items():
    assert b.data[b.off(address):b.off(address)+len(expected)//2].hex() == expected, hex(address)
assert b.qwords(0x1802e1ff0 + 0x110, 1)[0] == 0x1801a83c0
md = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {i.address: (i.mnemonic, i.op_str) for start,end,_ in blocks
                for i in md.disasm(b.data[b.off(start):b.off(end)], start)}
assert instructions[0x180213f23] == ('call', '0x1800918d0')
assert instructions[0x1802141f0] == ('call', '0x1801f8d60')
source = ROOT / 'src/InkRecognition'
driver = (source / 'LegacyOwnedMainDirectWordPass.cs').read_text()
assert driver.index('WriteInt32LittleEndian(flagResidue.AsSpan(52), Impossible)') < driver.index('flagPrepass.BindSharedCallerFrame()')
body = driver.split('flagAllocations += flagPrepass.Process(',1)[1]
assert body.index('softHyphenPrepass.Process(') < body.index('ApplyContinuation(')
assert 'owner.ValidateSpan(currentSpan, cancellationToken)' in body
assert 'global.ResolveGroupSource(arcs[ordinal].GroupId, token).Primary.LineId' in driver
span = (source / 'LegacyOwnedMainWordSpanPreparation.cs').read_text()
assert 'single.GetSourceLineIdForArc(node, ordinal)' in span and 'source.Line!.Owner.SourceLineId' in span
flag = (source / 'LegacyNativeMainWordFlagPrepass.cs').read_text()
assert 'sharedCallerFrame?.CopyBytes() ?? residue' in flag and 'finally { sharedCallerFrame?.Commit(request); }' in flag
packet = (source / 'LegacyNativeWordSoftHyphenRequestFactory.cs').read_text()
assert 'cost.Value =' not in packet and 'W32(bytes, 52,' not in packet
report = dict(scope='Static213C00 plus fresh ordinary C00 kind7, no trace/associated/custom source claim',
              textSha256=hashlib.sha256(b.data).hexdigest(), blocks=blocks, pePins=len(pins),
              vtableTargets=1, producerConsumerOrder=True, costCellInitialization='explicit caller value, not native LIMIT34')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n')
print(f'PASS {len(pins)} PE pins/{len(blocks)} full blocks/1vtable/typed driver source/order guards; vendor not executed')
