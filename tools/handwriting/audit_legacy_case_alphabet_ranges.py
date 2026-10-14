"""Read-only evidence for stored/native per-symbol predicate and range metadata."""
import argparse,hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True,type=Path);a=p.parse_args()
b=Binary('MyScriptEngine.dll');assert hashlib.sha256(b.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
f=functions('MyScriptEngine.dll');reader=f['180053f20'];ctor=f['180052260'];predicate=f['180059a70'];ranges=f['180058200']
assert 'param_4 < 0x104' in reader and '*(short *)(param_2 + 0x6a) != 0' in reader and 'param_2 + 0x18,1,1' in reader
assert 'FUN_180058200(param_1,param_2)' in reader and 'param_2 + 0xd0) = 0xffffffffffffffff' in ctor
for token in ('param_2[0xe]','param_2[0xf]','param_2[0x10]','iVar7 != -1','iVar7 != 0x18','(uVar3,0,0)'):
 assert token in predicate,token
assert "(char)param_2[3] == '\\0'" in ranges
assert 'param_2 + 0xd2) = uVar3 - 1' in ranges and 'param_2 + 0xd6) = uVar3 - 1' in ranges and 'param_2 + 0xd6) = uVar3;' in ranges
assert 'after the ranges [%d, %d]U[%d, %d]' in ranges and 'return 0' in ranges
r=dict(reader='053F20 versions103/104 set18 metadata then058200',constructor='052260 D0..D6 FFFF',predicate='059A70 stored-map presence per symbol; Unicode fallback only if all three maps NULL',ranges='058200 literal two-run state and third-run refusal; native sentinel and one-past-end retained',scope='Portable post-load case range metadata; other derived alphabet metadata and wrapper292 live-query still unfinished',vendorExecution=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(r)+'\n',encoding='utf8');print(r)
