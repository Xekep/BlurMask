"""Read-only pins for LIVE207CB0 ordering and fresh292 V58 cache isolation.

This inspects PE bytes only. No vendor code is imported or executed.
"""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
from capstone.x86 import X86_OP_MEM

if len(sys.argv) != 2:
    raise SystemExit('output.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x180231ed0, 0x180232090, '13b9105f8190b38d2af5242b01a1b6f49a9b3bd15165660456f5b60526968e8c'),
    (0x180233090, 0x180233f70, '3c0fe0202d8ef17305507261c8b42249bbf6caf53f179a54331fe977580d01f1'),
    (0x180233fc0, 0x180234840, 'dcf8c0b4d64a3660bf05016c91975c9cf984efa6e9305ff48f7f59cc6cda582f'),
    (0x180215dc0, 0x180215de3, '9457abb39c680f9580ee97a76c32f2136d89c2f02fd008f82f4eae20c08ee7c8'),
]
for start, end, sha in regions:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == sha, hex(start)
pins = [
    (0x18023203e, '48c786c08f0000ffffff7f'),  #V80 reset: cursor INT_MAX, count zero.
    (0x180233134, '49639278830000'),         #NormalV40 slot index.
    (0x18023313b, '4869da68100000'),         #NormalV40 slot stride1068.
    (0x180233142, '4883c3384903da'),         #Slot base38+actual292 object.
    (0x18023314b, '81e207000080'),           #Ring index mod8 from resetzero.
    (0x18023409e, '48c783c08f0000ffffff7f'), #V58 key change resets its own cache.
    (0x180234118, '8b83c48f0000'),           #V58 cached count.
    (0x180234125, '448babc08f0000'),         #V58 cursor.
    (0x18023412c, '443be87c46'),             #cursor<count skips fetch; equality fetches.
    (0x18023413a, '488b5308'),               #Exact inner backend, not a new query source.
    (0x180234144, '4c8b4858'),               #InnerV58.
    (0x180234148, '41ffd1'),
    (0x18023414b, '8983c48f0000'),           #Store inner return as cache count.
    (0x180234154, '4489b3c08f0000'),         #Store cursor zero.
    (0x18005a3f0, '33c0c3'),                #Actual inner2B7.V58 returns0.
    (0x180215dde, 'e8cd1effff'),             #LIVE207CB0 before the following V80.
    (0x180215e75, '488b5630'),               #Same source30, after publication/progress.
    (0x180215e8b, '4c8b9080000000'),         #SourceV80.
    (0x180215e92, '41ffd2'),
    (0x180208657, '418b8e90240000'),         #Each new2488 entry = publication start-1.
    (0x180208665, 'ffc942890cb8'),
    (0x18020ba17, '890491'),                 #2478[end+1] = global2470.
    (0x18020ba1a, '418d4dff'),               #2488 sentinel = end.
    (0x18020ba25, '4589ae90240000'),         #2490 = end+1.
    (0x18020ba2c, '890c90'),
]
for address, expected in pins:
    assert b.data[b.off(address):b.off(address) + len(expected) // 2].hex() == expected, hex(address)
assert b.qwords(0x1802e85b0 + 0x40, 1)[0] == 0x180233090
assert b.qwords(0x1802e85b0 + 0x58, 1)[0] == 0x180233fc0
assert b.qwords(0x1802b4360 + 0x58, 1)[0] == 0x18005a3f0
#Normal233090 owns eight1068-byte request slots starting38. They end exactly
#at8378, before the separate V58 request8F80/cursor8FC0/count8FC4. Its context
#rewrite helper232F10 gets context factory/AL arguments, not the292 object.
assert 0x38 + 8 * 0x1068 == 0x8378
md = Cs(CS_ARCH_X86, CS_MODE_64)
md.detail = True
normal = list(md.disasm(b.data[b.off(0x180233090):b.off(0x180233f16)], 0x180233090))
assert normal and normal[-1].address == 0x180233f15 and normal[-1].mnemonic == 'ret'
assert not any(op.type == X86_OP_MEM and 0x8f80 <= op.mem.disp <= 0x8fd0
               for ins in normal for op in ins.operands)
report = dict(
    regions=[dict(start=hex(s), end=hex(e), sha256=h) for s, e, h in regions],
    exactPins=len(pins), normalSlotEnd='0x8378', normalCacheReferences=0,
    bindings=['292.V40=233090', '292.V58=233FC0', '2B7.V58=05A3F0'],
    proof='Prior-node V80 resets count0/cursorINT_MAX; normal V40 uses separate bounded slots; V58 fetches same inner0, then retains count0/cursor0.',
    excluded=['nonempty/custom inner V58', 'reused wrapper with external V58 injection', 'associated source expansion'])
Path(sys.argv[1]).write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print(f'PASS {len(regions)} whole regions/{len(pins)} pins, three VTables and isolated fresh292 V58 cache; no vendor execution.')
