"""Read-only20E6A0 dispatcher/owner contract audit; never loads vendor code.

The mixed-stream and actual CASE byte oracles are separate from these static
pins. Source headers, owning rows and general geometry operations remain
explicit prepared inputs rather than invented inference producers.
"""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
start, end = 0x18020e6a0, 0x180210533
region_sha = '03f550eeff2a823efc7fd4ce029595d3d50b079df13817ecabe54013776fcd79'
assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == region_sha
pins = [
    (0x18020e6cd, '6683ba0224000001'),  #unsigned primary count >1
    (0x18020e6f3, '770d'),
    (0x18020e6f5, '6641833b01'),       #otherwise secondary count >1
    (0x18020e6fa, '0f860e1e0000'),     #no-op before trace/source/row access
    (0x18020e707, '450fb69668230000'), #trace read occurs only in active scope
    (0x18020e72f, '3902'),
    (0x18020e731, '0f86be1d0000'),     #empty source bucket skips row body
    (0x18020e740, '448bf8'),
    (0x18020e743, '49c1e707'),         #physical row index *128, not pool ordinal
    (0x18020e747, '4c037a10'),
    (0x18020e74b, '450fbe4720'),       #signed span byte
    (0x18020e753, '412bd8'),
    (0x18020e75b, 'ffcb'),             #previous=current-signed span-1
    (0x18020e762, '450fb74f1e'),       #original previous row ordinal
    (0x18020e76b, '458b6f58'),         #actual pool ordinal58
    (0x18020e7a0, '410fafc5'),
    (0x18020e7aa, '498b8628240000'),   #primary alternatives2428
    (0x18020e7e0, '410fafc5'),
    (0x18020e7ea, '498b8630240000'),   #secondary alternatives2430
    (0x18020e8ab, '83fbff'),
    (0x18020e8ae, '0f855f020000'),     #root -1 before other predecessor scopes
    (0x18020e8c1, 'e86a95feff'),       #mapped context1F7E30
    (0x18020e8d6, 'e81597feff'),       #raw context1F7FF0
    (0x18020eb1b, '66453b8e0a230000'), #secondary threshold230A, unsigned ordinal
    (0x18020eb23, '0f83c2080000'),
    (0x18020eb29, '498b96e0230000'),   #stored88 actual owning node23E0
    (0x18020eb44, '496bc958'),
    (0x18020f3eb, '4180bef622000000'), #reset only mode0
    (0x18020f3f8, '0f8572030000'),
    (0x18020f3fe, '498b86e0220000'),   #actual source descriptor header table22E0
    (0x18020f40b, '241c'),
    (0x18020f40d, '3c08'),
    (0x18020f40f, '0f825b030000'),     #(header&1C)<8 takes general branch
    (0x18020f415, '410fb786ea240000'),
    (0x18020f41d, '6641394724'),
    (0x18020f422, '0f8448030000'),     #label24==controller24EA takes general
    (0x18020fc48, '4c8b8818010000'),   #initial full-packet118 then140
    (0x18020fc68, '4c8b8840010000'),
    (0x18020fefc, '450fb78e0a230000'), #alternative predecessor scan starts230A
    (0x18020ff0d, '498b86f0230000'),   #ends23F0, NOT retained23F8
    (0x18020ffc2, '4c8b88f0000000'),   #source successor count/list
    (0x180210100, '4c8b8818010000'),   #per-candidate118/140
    (0x180210120, '4c8b8840010000'),
    (0x1802102eb, 'e8d084feff'),       #existing1F87C0 workspace merge
    (0x180210309, '664189471e'),       #selected true predecessor ordinal writeback
    (0x1802103ef, '498b86f0230000'),
    (0x18021050e, 'b001'),
    (0x180210532, 'c3'),
    #The special reset symbol comes from lookup of U+0003, not AL header0E.
    (0x18020c347, '4c8d05fa070e00'),
    (0x18020c37b, 'e8f0cbe0ff'),
    (0x18020c380, '668983ea240000'),
    (0x1802ecb48, '03000000'),
]
for address, expected in pins:
    assert b.data[b.off(address):b.off(address) + len(expected) // 2].hex() == expected, hex(address)
md = Cs(CS_ARCH_X86, CS_MODE_64)
decoded = list(md.disasm(b.data[b.off(start):b.off(end)], start))
assert decoded[-1].address == end - 1 and decoded[-1].mnemonic == 'ret'
assert 0x18020c347 + 7 + int.from_bytes(bytes.fromhex('fa070e00'), 'little', signed=True) == 0x1802ecb48
body = functions()['18020e6a0']
assert body.index('if ((1 < *(ushort *)(param_2 + 0x2402))') < body.index('local_448 =')
assert body.index('if (local_444 == 0xffffffff)') < body.index('if (uVar17 < *(ushort *)(param_2 + 0x230a))')
assert '(param_2 + 0x23f0)' in body
assert 'uint local_3f8;' in body and 'byte local_3af;' in body and 'int local_3a8;' in body
#One native local80 request spans3F8..3A9 and is not re-zeroed per heap row.
#The following managed pins check bridge ownership; behavioral byte oracles
#separately verify request scratch carry across interleaved route kinds.
managed = (ROOT / 'src/InkRecognition/LegacyNativePrimaryWordRecombination.cs').read_text()
managed_pins = [
    'if (pc == 1 && sc == 1 || activeCount == 0)',
    'configuration.Trace2368 != 0', 'ArgumentNullException.ThrowIfNull(owningNodeRowsLookup)',
    'uint previous = unchecked(currentNode - (uint)(int)(sbyte)slot[32] - 1);',
    'int ordinal = I32(slot, 88)', 'parentOrdinal < configuration.PrimaryArcLimit230a',
    'result.HeapSlot.Span.CopyTo(slot)', 'requestResidue = result.LastRequest.ToArray()',
    'snapshots.Add(node, rows)', 'new(row.Record.ToArray(), row.PrimaryAlternatives.ToArray(), row.SecondaryAlternatives.ToArray())',
]
for needle in managed_pins:
    assert needle in managed, needle
assert managed.index(managed_pins[0]) < managed.index(managed_pins[1]) < managed.index(managed_pins[2])
report = dict(
    region=dict(start=hex(start), end=hex(end), sha256=region_sha), instructionPins=len(pins),
    managedOwnershipPins=len(managed_pins),
    branches=['root previous=-1', 'primary parent ordinal<230A',
              'higher reset: mode0,header&1C>=8,label24!=24EA', 'remaining full80 higher geometry'],
    sourceOwnership='actual computed predecessor node; all source23F0 stored88 rows plus configured40-byte alternatives',
    ordinals='physical heap slot and row58 pool ordinal remain distinct; inactive bytes survive',
    scratch='same local80 request is carried between general rows; other routes do not zero it',
    preparedHigherOnly=True, actualResourceConsumer='CASE D0/mainNN/cache acoustic root/ordinary-parent prefix; no intervening LM phase',
    limits=['not connected to shared2144F0 scheduler', 'no inferred secondary predecessor in fresh no-associated fixtures',
            'general geometry requires explicit118/140/F0F8 and secondaryCount>=2',
            'trace side tables', 'bounded1..16 counts/4096 heap slots', 'native allocation/failure-order orchestration'],
    noVendorExecution=True)
Path(sys.argv[1]).write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print(f'PASS full20E6A0 region/{len(pins)} PE pins/{len(managed_pins)} ownership pins; static reads only.')
