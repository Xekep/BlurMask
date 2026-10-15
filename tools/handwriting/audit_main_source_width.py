"""Pinned2188F0 filtering/float-width/double-sum and221840 mean, static only."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x1802188f0,0x180218a2b
h='7f3a9a401d16952302a2d935d87b15bcf5dff2dc1ac42797f214563a3b583b3c'
assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins=[(0x18021893b,'8d43ff'),(0x180218976,'3b8670240000'),
 (0x18021898e,'80e207'),(0x180218991,'80fa02'),(0x180218996,'80ea04'),
 (0x180218999,'f6c2fd'),(0x18021899e,'6645397c1838'),
 (0x1802189a6,'664183c206'),(0x1802189ab,'664183fa02'),
 (0x1802189df,'f30f10442438'),(0x1802189e5,'f30f5c442430'),
 (0x1802189eb,'ff4508'),(0x1802189ee,'0f5ac8'),(0x1802189f1,'f20f584d00'),
 (0x1802189fb,'4883eb48')]
for addr,raw in pins:assert b.data[b.off(addr):b.off(addr)+len(raw)//2].hex()==raw
caller=f['180221840']
assert 'local_58 = 0.0;' in caller and 'uStack_50 = 0;' in caller
assert 'if (0 < (int)uStack_50)' in caller
assert '*(float *)(param_3 + 0xb0) = (float)(local_58 / (double)(int)uStack_50);' in caller
assert '*(undefined4 *)(param_3 + 0xb0) = 0;' in caller
Path(sys.argv[1]).write_text(json.dumps(dict(start=hex(a),end=hex(z),sha256=h,pins=len(pins),
 order='reverse within each native flush segment; do not flatten complete trace',
 arithmetic='float maxX-minX, increment signed32 count, widen width and add double; positive-count mean to float TOP.B0',
 scope='prepared single-segment rows with actual compact/source geometry; native segment producer separate',noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS pinned2188F0 block/15 instructions and221840 initial/mean branches; static reads only')
