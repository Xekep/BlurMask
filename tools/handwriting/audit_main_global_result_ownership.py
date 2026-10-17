"""Pinned static global TOP273/capture ownership. Never loads vendor code."""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
from capstone.x86 import X86_OP_MEM, X86_REG_RDI

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary()
f = functions()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x180157ccc, 0x180157d7b, '767792576389d4c942c7dc76287f025d20a69393e2c0070cf10638ed3d3f9777'),
    (0x18015855b, 0x180158587, '959b885b7990eccf0d14331731f6394a5681c3af22ac93d3e615f95f39452ca5'),
    (0x18022733f, 0x180227388, '906a5fa33087e19d22221961ff3e7c64081e33fe66226fc968caf86600e7017f'),
    (0x18013bb50, 0x18013bd40, '6027e9ca6dfc3f2c0f239338e12fd5adae41e358fae3da8b81b3d31165b04cc7'),
    (0x18013abf0, 0x18013ac80, 'b58ada3340f5e45aa09591d4bcabf733deffc722dd75209bcb98907370ffd070'),
    (0x18020c8c0, 0x18020c9d0, '83fb4f5184c36e45f8494abc50864677ddfd99152e49e6b02fa2a694bf10da74'),
]
for a, z, expected in regions:
    assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest() == expected
pins = [
    (0x180157d29, '48898424b0000000'),  #Descriptor at local TOP rsp+B0.
    (0x180157d36, '410fb68620020000'), #Actual driver capture220.
    (0x180157d41, '88842458020000'),
    (0x180157d61, '4c8d842458020000'),
    (0x180157d69, '488d9424b0000000'), #Construct the same local TOP.
    (0x180157d78, '41ffd2'),
    (0x18015855e, '4c8d8424b0000000'), #Pass that exact TOP to main240.V80.
    (0x180158566, '48c744242800000000'),
    (0x18015856f, '41b101'),           #Reset flag1, retry/extra argument0.
    (0x180158575, 'c644242000'),
    (0x18015857d, '4c8b9080000000'),
    (0x180158584, '41ffd2'),
    (0x180227351, 'ba73020000'),      #273 reset dispatcher, no replacement ctor.
    (0x180227361, '4c8b4030'),
    (0x180227368, '488bd7'),
    (0x180227371, '41ffd0'),
    (0x18013bb65, '488bfa'),
    (0x18000c8f3, '668987c4000000'), #Fresh configuration C4/C5=0.
]
for a, raw in pins:
    assert b.data[b.off(a):b.off(a)+len(raw)//2].hex() == raw
assert b.qwords(0x1802e7ab8+8, 1)[0] == 0x1802e7cb0
assert b.qwords(0x1802e7cb0+0x80, 1)[0] == 0x1802271f0
assert b.qwords(0x1802dc6f8+8, 1)[0] == 0x1802dc730
assert b.qwords(0x1802dc730, 3) == (0x18013b9e0, 0x1802dc7b0, 0x18013bb50)
assert b.qwords(0x1802dc7f8+0x10, 1)[0] == 0x18013b870
assert b.qwords(0x1802dc680, 1)[0] == 0x18013abf0
for name in ('180155da0', '180155ef0'):
    assert '*(undefined1 *)(param_2 + 0x220) = 0;' in f[name]
assert '*(undefined1 *)(param_2 + 0x41) = *param_3;' in f['18013b870']
assert '(param_1,uVar2,param_2 + 0x41,1)' in f['18013b870']
#273 reset keeps both its capture byte and its existing274 owner. The full
#pinned body also covers the indirect+38 store and all called cleanup branches.
md = Cs(CS_ARCH_X86, CS_MODE_64)
md.detail = True
ins = list(md.disasm(b.data[b.off(0x18013bb50):b.off(0x18013bd31)], 0x18013bb50))
writes = [(o.mem.disp, o.size) for i in ins for o in i.operands
          if o.type == X86_OP_MEM and o.access & 2 and o.mem.base == X86_REG_RDI]
assert writes == [(0x18,8),(0x20,8),(0x2c,4),(0x30,8),(0x3c,4),(0x40,1),
                  (0x28,4),(0x60,2),(0x78,8),(0x80,4),(0x88,8),(0x90,8),
                  (0x98,1),(0xb5,2),(0xb4,1)]
assert all(not (a <= 0x41 < a+n or a <= 0x48 < a+n) for a,n in writes)
pool_reset = f['18013abf0']
assert 'uVar1 = *(undefined1 *)(param_2 + 0x10);' in pool_reset
assert pool_reset.count('*(undefined1 *)(param_2 + 0x10) = uVar1;') == 2
assert b.qwords(0x1802e5a20+0x68, 1)[0] == 0x1801d62a0
assert 'param_2 + 0xb9,1' in f['1801d4950']
refresh = f['18020c8c0']
at = refresh.index('*(undefined1 *)(param_2 + 0x22f9) = uVar1;')
assert '*plVar3 + 0x68' in refresh[at-90:at]
assert '*(undefined1 *)(param_2 + 0x22f9)' in f['18021fe30']
report = dict(regions=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in regions],
              exactPins=len(pins), sameTop='driver157C60 rsp+B0 ctor ->actual240.V80=2271F0 argument3 ->273 reset13BB50',
              capture='fresh driver220 ->27341 ->27410; main reset retains capture and pool identity',
              sourceFormat='actual247.V68 resourceB9 ->main20C8C0 controller22F9 ->21FE30 publisher argument',
              resetDirectOwnerWrites=writes,
              excluded=['reused/property-configured driver','trace','associated source','generic source format','full global result execution'],
              noVendorExecution=True)
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS6 native regions/18 exact pins/actual240.V80 same TOP273/reset preserves capture and274 owner/main B9-to22F9; static only')
