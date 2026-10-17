"""Read-only native279 list -> published275 chain ->21FBF0 evidence."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
 (0x18021fbf0,0x18021fe04,'b90b1c22b0c468c1b2dd6d4cacf647e5e6d3209b500b8cff246136304cd21d89'),
 (0x18013f180,0x18013f1ee,'e02ddf2860efd75ac888cb8aec79af55ff3162544c8d616b3f8b4b60a45bf8fc')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
assert b.qwords(0x1802dc900+0x10,1)[0]==0x18013f180
body=f['18021fbf0']
for text in ['plVar1 = *(longlong **)(param_4 + 0x18);',
 '(longlong)(int)plVar1[1] * 0x78 + *(longlong *)(*plVar1 + 0x18)',
 'lVar6 = *(longlong *)(lVar8 + 8);','plVar1 = (longlong *)plVar1[4];',
 '*(undefined2 *)(lVar5 + 8) = uVar4;',
 '*(undefined2 *)(lVar6 + 8) = *(undefined2 *)(param_2 + 0x24ba);']:
 assert text in body,text
caller=f['180221840']
assert caller.index('FUN_18021fbf0(')<caller.index('FUN_18021f320(')
last=f['18013f180']
assert 'lVar1 = *(longlong *)(param_2 + 8);' in last
assert 'lVar1 = *(longlong *)(lVar2 + 0x20);' in last
assert 'return lVar3;' in last
Path(sys.argv[1]).write_text(json.dumps(dict(
 regions=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in regions],
 list='279.Head18 -> node owner275/index*120 -> WORD.Head08; Next20',
 last='common WORD V10/13F180 follows pool record20 to the last reference',
 writes='glyph ushort08 only; original two four-byte caller scratch slots retained',
 caller='221840 successful publication ->21FBF0 ->separate21F320',
 limits=['actual synthetic quote NN fixture not established','nonempty control owner not supplied',
         'full alternative assembly/string wrapper and text acceptance separate'],
 noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS2 pinned regions/279 published-list and common WORD last-reference binding; no vendor execution')
