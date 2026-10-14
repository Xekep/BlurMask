"""Optional pinned static PE evidence; no vendor code loading or execution."""
import argparse,hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True,type=Path);a=p.parse_args()
e=Binary('MyScriptEngine.dll');t=Binary('MyScriptText.dll')
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert e.qwords(0x1801de1b0+0x58,1)[0]==0x180052bd0
#Exact052BD0 assembly: three non-null stored maps return1; all NULL branch04FFE0.
assert e.data[e.off(0x180052bd0):e.off(0x180052bf2)].hex()=='48837a7000751548837a7800750e4883ba80000000000f84f4d3ffffb801000000c3'
ef=functions('MyScriptEngine.dll');tf=functions('MyScriptText.dll')
scan=ef['18004ffe0'];scalar=ef['18005f4c0']
assert '*param_2 + 0x38' in scan and '*param_2 + 0x30' in scan and 'iVar3 != 0x18' in scan and '(uVar4,0,0)' in scan
for token in ('param_1 == 0xb5','uVar8 = 0x82','uVar6 = 0x3a','uVar6 = 0x22','uVar6 = 0x59','uVar6 = 0x23','uVar6 = 0x6e','uVar6 = 8','uVar11 * 0x48','puVar15[5]','puVar15[6]'):
 assert token in scalar,token
factory=tf['18020c9d0'];assert 'uVar17 = (uint)local_res18 & 7' in factory and 'uVar17 == 1' in factory and '(param_1,0x292)' in factory
assert t.qwords(0x1802e85b0+0x30,1)[0]==0x180232ed0
assert 'FUN_180232c80' in tf['180232ed0']
report=dict(checks=18,scope='052BD0 stored-map/04FFE0 fallback scan and05F4C0 scalar alternatives; default20C9D0 case292 frontier identified, not full292 query port',vendorExecution=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(report)+'\n',encoding='utf8');print(report)
