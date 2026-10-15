"""Fresh no-property-event20C9D0/prepare binding; static PE only."""
import hashlib,json,re,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise ValueError('output.json')
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
ctor=f['18000c860'];assert '*(undefined8 *)(param_2 + 0x90) = 0xffffffffffffffff' in ctor and '*(undefined2 *)(param_2 + 0xc4) = 0' in ctor
assert '*(undefined1 *)(param_2 + 0x2368) = 0' in f['18020baa0']
source=f['18020c9d0']
for token in ['param_2 + 0x18) + 0x90','uVar17 == 0xffffffff','(uint)local_res18 & 7','uVar17 == 1','param_1,0x292','param_1,0x2b7','uVar17 = (uint)local_res18 >> 3 & 3','param_1,0x293','param_1,0x294','param_1,0x2b5']:
 assert token in source,token
prepare=f['18020d8f0'];assert '*(undefined1 *)(param_2 + 0x2368)' in prepare and '**(longlong **)(param_2 + 0x30) + 0x10' in prepare
case=f['1802324e0'];assert '*(undefined1 *)(param_2 + 0x18) = param_6' in case and '*(undefined1 *)(param_2 + 0x9030) = *(undefined1 *)(param_3[1] + 0xc4)' in case
propertySetter=f['180215f30'];assert 'iVar7 == 0x12d0081' in propertySetter and '*(char *)(param_2 + 0x2368) = cVar3' in propertySetter
#Source policy does not overwritecontainer90/C4 duringselectedresource scalar refresh.
selected=f['1801d60d0'];assert not re.search(r'\+ 0x(?:90|c4)\)\s*=(?!=)',selected)
query=f['180233090'];assert "*(char *)(param_2 + 0x18) != '\\0'" in query and 'lVar23 = param_3[6]' in query
ak=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
name=bytes(value^[20,18,25,119][i%4] for i,value in enumerate(ak[24:80])).split(b'\0')[0]
assert name==b'ru_RU-ak-cur.lite'
out={'freshscope':'new00C860container90=FFFFFFFF,C4=0;new20BAA0 capture2368=0;no external/reusedpropertyevents acceptedbynewAPI',
 'mode':'20C9D0 lexicalALcasepredicate;container90FFFF→resourceField68&7;actualRUField68=1→292 over2B7',
 'capture':'20D8F0 sourceV10 sixtharg240+2368→2324E0 runtime18;querysidevectors conditionalruntime18;12D0081 canchangeitoutsidefreshscope',
 'ignoreCase':'2324E0 copiesactualcontainerC4→292+9030;fresh0;20C9D0 C4falsealsoskips2B5',
 'ownership':'casebinding enumeration+permissionrefresh+normal/aux/terminal/restart shareoneSessionbackend;main240wholeloop/winner notclaimed',
 'resource':'pinnedRUAK/LK/checkedsampledmain0/referenceownedgraph;no293 Field68bits3..4=0,noArabicmap294,noAsian/Germantransform inexactRUfreshscope',
 'vendorExecution':False}
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(out,indent=2)+'\n',encoding='utf8');print('PASS fresh20C9D0 mode/C4/prepare-capture2368 bindings, nativepropertyoverride boundary andsharedsource scope')
