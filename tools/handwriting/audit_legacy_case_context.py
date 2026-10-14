"""Read-only static proof of default05F8E0 records and04F170 row selector."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True,type=Path);a=p.parse_args()
b=Binary('MyScriptEngine.dll');assert hashlib.sha256(b.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert b.qwords(0x180202a70+0x88,1)[0]==0x18005f8e0
model=json.loads((root/'models/text-unicode/legacy-normalization-data.json').read_text(encoding='utf8'))
actual=[list(struct.unpack_from('<3i',b.data,b.off(0x1801f8560)+12*i)) for i in range(517)]
assert actual==model['combiningRanges']
soft=struct.unpack_from('<46i',b.data,b.off(0x1801fd860))
def ccc(cp):return next((v for lo,hi,v in actual if lo<=cp<=hi),-1)
assert len(set(soft))==46 and all(ccc(cp)==0 for cp in (*soft,0x49))
f=functions('MyScriptEngine.dll');s=f['18005f8e0']
for token in ('bVar11 = iVar10 == 0x49','uVar6 = 0x2e','uVar1 = 0x205','puVar9[-1] = 0','*puVar9 = 0','puVar9[1] = bVar11','puVar9[2] = uVar8','iVar4 != 0xe6','*param_1 == 0x307','bVar11 = false','uVar8 = 0'):
 assert token in s,token
#Static assembly has no loop write re-enabling r14b/sil. Their only live1
#initializations precede the scalar loop; CCC0/230 clears them at05FA3A.
assert b.data[b.off(0x18005fa3a):b.off(0x18005fa40)].hex()=='4532f64032f6'
selector=f['18004f170']
for token in ('param_7 != 0','iVar13 == param_7','piVar12[-1] == iVar5','iVar2 == 1','(param_4 & 2) != 0','cVar3 = pcVar16[3]','cVar3 = *pcVar16','cVar3 = pcVar16[2]','bVar18 = pcVar16[1]'):
 assert token in selector,token
r=dict(nativeContext='05F8E0',cccRanges=517,softDottedEntries=46,flags6And7='provably zero for exact default data: initialization firstscalar only, all initial true cases CCC0 then cleared',selection='04F170 language fallback/ordered context predicates; full normalized alphabet lookup remains separate',vendorExecution=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(r)+'\n',encoding='utf8');print(r)
