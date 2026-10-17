"""Pinned static-only EN FIRST297/108 cut/seed join proof; never loads a DLL."""
from pathlib import Path
import ast,base64,hashlib,json,struct,sys
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions()
regions=[
(0x18018d400,0x18018d590,'c609b30fab12220bea1e5b6fc65bdec6ed67af4e5b79ba905707f93b07bbc42d'),
(0x18018d590,0x18018d6e0,'c4fe31d8b959bfd73fec54f74fc0ddc6b25da3e124144757a6f06ce2c01fd783'),
(0x18018d920,0x18018db60,'ede92e1909c679dfa0ac42e3739505500a693ff538d59da0bbdf87eed25b3ef9'),
(0x180192c80,0x180192ef0,'0f549111b2729fbebdc4e1bd5df1f2244d43619c9285c398bba7441176ac9936'),
(0x180193380,0x1801935d0,'acd730750381cb3e9e56e07838880d7805fc05873a48323d61c33e9042636467'),
(0x180196500,0x180196f40,'d70df666938d2f080c39a0e0c2650668ac688cfae7c0c357083bc6609dc8dd83'),
(0x180168c40,0x180168ca0,'d475e6654f9a250f0d9fe4fe58d791adf1b8240a159ea7ffd645ed096fa82967'),
(0x180168b40,0x180168b70,'687dc73ea16c352dfb099512ef77141f5ef3d9df264374a5e3e88c31c8cd833e')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
terms={
'18018d400':['*(undefined4 *)(param_2 + 0x244) = 0','*(undefined8 *)(param_2 + 0x1d0) = 0'],
'18018d590':['*(undefined1 *)(param_2 + 0x242) = *(undefined1 *)(param_2 + 0x134)','puVar5 = (undefined8 *)(param_3 + 8)'],
'18018d920':["*(char *)(param_2 + 0x134) == '\\0'",'*(undefined8 *)(param_2 + 0x1c8) = *(undefined8 *)(param_3 + 0x18)'],
'180192c80':["*(char *)(param_2 + 0x242) == '\\0'",'return 1'],
'180193380':['FUN_180191950','*puVar4,puVar4[1]','&local_48,&local_58,0','*(longlong *)(param_2 + 0x1d0) != 0'],
'180196500':['if (0x107 < param_4)','param_2 + 0x134','param_2 + 0x1d0'],
'180191430':['iVar8 = -1;','*(longlong **)(param_2 + 0x1c8)'],
'1801d4950':['if (0x10f < param_4)','*(undefined8 *)(param_2 + 0x148) = 0;']}
for address,values in terms.items():
 for value in values:assert value in f[address],(address,value)
assert '0x244'not in f['18018d590']and'0x244'not in f['18018d920']
bindings={0x1802e1490+0x18:0x18018d920,0x1802e1490+0x38:0x180193380,
 0x1802df340+0x30:0x180168b40,0x1802df340+0x70:0x180168c40,0x1802e5a20+0xa0:0x1800c2430}
for at,target in bindings.items():assert b.qwords(at,1)[0]==target
# Independent tracked raw parser; absolute category alignment is preserved.
path=ROOT/'tools/LegacySource247Wire.Probe/audit_output.py';tree=ast.parse(path.read_text(encoding='utf-8-sig'))
ns={'struct':struct,'base64':base64};exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.ClassDef,ast.FunctionDef))],type_ignores=[]),str(path),'exec'),ns)
data=(ROOT/'models/en-US/en_US-ak-cur.lite.res').read_bytes()
sampler,refusal=ns['sampler'](data,dict(Offset=16952,End=18340,Version=0x108),217);assert refusal is None
fields={x['destination']:base64.b64decode(x['raw'])for x in sampler['fields']}
assert fields['134.present']==fields['1D0.present']==b'\0'
prefix=bytes.fromhex('0000003f3333b33ecdcc4c3e6666e63f0001003333b33ecdcc4c3e0000803e0000003f0000003fcdcccc3d060000804001')
assert data[16968:17017]==prefix and data[181996]==0
assert data[18340+16:18340+18]==b'\7\1'and data[18340+306:18340+309]==b'\1\0\0'
assert struct.unpack_from('<H',data,18695+14)[0]==0x119
assert struct.unpack('<f',fields['68'])[0]>0 and struct.unpack('<f',fields['114'])[0]==1
report=dict(noVendorExecution=True,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],
 bindings={hex(a):hex(z)for a,z in bindings.items()},terms=terms,
 actual=dict(source=8843,sampler=16952,category=17017,graph=18340,acoustic=18695,alphabet=217,spanCap=7,insertionShift=0,
 closure134=False,sourceName1D0=False,optional148=False,fields=sampler['fields']),
 scope='Fresh raw FIRST ENlite single ordinary descriptor; shared191950 scalar arithmetic, actual probabilities and chronology; no21D119 cache/controller/winner claim.')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print('PASS8 pinned ranges/5bindings/actual108 closed-tail gates/217categories/7atom shift0 graph; static only')
