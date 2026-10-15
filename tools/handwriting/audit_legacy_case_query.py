"""Pinned static wrapper292 policies, ordinary owner, source mode and query contract."""
import hashlib,json,re,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise ValueError('output.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions();source=(ROOT/'src/InkRecognition/LegacyNativeCasePathPolicy.cs').read_text(encoding='utf-8')
for version,count,policy,selector in [(0,14,0x1802e84d0,0x1802e8660),(512,28,0x1802e86d0,0x1802e8880)]:
 parsed=re.search(rf'\b{version} => new\(version,"([0-9a-f]+)","([0-9a-f]+)"\)',source)
 assert parsed
 for payload,address,size in [(parsed[1],policy,count*15),(parsed[2],selector,count*8)]:
  assert bytes.fromhex(payload)==b.data[b.off(address):b.off(address)+size]
cats=re.search(r'categories200=Convert.FromHexString\("([0-9a-f]+)"\)',source)[1]
assert bytes.fromhex(cats)==b.data[b.off(0x1802e8998):b.off(0x1802e8998)+32]
for term in ['+ 0x8378','* 0x1068','0x1058','+ -4','iVar20 == -1','FUN_180232f10','local_a8[7] & 3','local_98._0_4_ + 0x2000']:
 assert term in f['180233090'],term
assert '0x7c' in f['180233f70'] and 'param_3 + 5) & 4' in f['180233f70']
assert b.qwords(0x1802de610+0x280,1)[0]==0x180155710
assert b.data[b.off(0x180155710):b.off(0x180155710)+17]==bytes.fromhex('488b5270488b024c8b88c801000049ffe1')
assert b.qwords(0x1802e1ff0+0x1c8,1)[0]==0x18005a400
assert b.qwords(0x1802b4360+0x58,1)[0]==0x18005a3f0
assert b.data[b.off(0x18005a3f0):b.off(0x18005a3f0)+3]==bytes.fromhex('33c0c3')
assert b.qwords(0x1802baff0+0x40,1)[0]==0x1801d6dd0
assert b.data[b.off(0x1801d6dd0):b.off(0x1801d6dd0)+9]==bytes.fromhex('8b4268418900b001c3')
assert b.qwords(0x1802baff0+0x88,1)[0]==0x1801d6de0
assert b.qwords(0x1802b4360+0x50,1)[0]==0x180093f60
assert '*(undefined8 *)(param_2 + 0x90) = 0xffffffffffffffff' in f['18000c860']
factory=f['18020c9d0'];assert 'uVar17 = (uint)local_res18 & 7' in factory and 'param_3[0x11] + 0x38' in factory and '0x292' in factory
for term in ['+ 0x9028','cVar1 ==','param_2 + 0x18','param_4 >> 0x3a']:
 if term=='param_2 + 0x18':continue
 assert term in f['180234970'],term
report={'normalQuery':'233090 ordinary frontend,8slots/128rows,cap-4 slack,full-row preskip',
 'policies':'0:14x15,200:28x15;selectors8bytes;wordcategory200:32indexedbytes(includingterminal31)',
 'ordinaryOwner':'222.V280=155710->21B.V1C8=05A400 false',
 'actualDictionaryAux24':'2B7.V58=05A3F0 constant0;not nonempty28C',
 'mode':'13B.V40=1D6DD0 readsField68;explicitcontainer90 overrides,FFFF derivesField68&7',
 'constructorOnly':'00C860 defaults90/94FFFF;laterpropertiesarenotassumedabsent',
 'restart':'233F70 transportscasebits58..62 onlyifheader05.bit2',
 'terminalCategory':'234970 changesmetadata8;wordidentity4andotherbytesremain',
 'remaining':'generic nonempty24 backend;actualcontrollercase-setting/side-table/metadataC4owner;no native finalwinner'}
Path(sys.argv[1]).parent.mkdir(parents=True,exist_ok=True);Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS pinned292 state policies/ordinary owner/source mode/empty actual24/restart/terminalcategory; no vendor execution')
