"""Pinned source240/main21D policy bindings, read-only PE/resource audit."""
import argparse, hashlib, json, struct, sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary, functions
parser=argparse.ArgumentParser();parser.add_argument('--output',required=True);args=parser.parse_args()
b=Binary('MyScriptText.dll');f=functions('MyScriptText.dll');engine=Binary('MyScriptEngine.dll');ef=functions('MyScriptEngine.dll')
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
checks={}
def includes(table,address,*terms):
 text=table[f'{address:x}'];assert all(t in text for t in terms),(hex(address),terms)
 checks[hex(address)]=list(terms)
assert engine.qwords(0x18020f290+0x58,1)[0]==0x180089ad0
assert b.qwords(0x1802e4180+0x18,1)[0]==0x1801b0760
includes(f,0x180150d00,'*local_res20 + 0x18','*(longlong *)(param_2 + 0x28) + 0x14')
includes(f,0x1801b0760,'local_90 = param_4','&DAT_1802eb358','param_2[0x5a] = lVar15')
includes(ef,0x180089ad0,'strlen(param_2)','param_4[lVar3] != *param_4','return sVar1 == sVar2')
includes(f,0x18000c860,'*(undefined4 *)(param_2 + 0x18) = 0x20dffff','*(undefined8 *)(param_2 + 0xec) = 0')
includes(f,0x1801d60d0,'0x21703','~uVar2','*(int *)(param_3 + 0x1c) == -1')
includes(f,0x1801b0760,'*(undefined2 *)((longlong)param_2 + 0x1a4) = 0','*(undefined1 *)((longlong)param_2 + 0x1a6) = 0','*(undefined8 *)(lVar31 + 0x50) = 0','*(bool *)((longlong)param_2 + 0x271)','*(bool *)((longlong)param_2 + 0x272)')
includes(f,0x1801b75b0,'*(longlong *)(param_7 + 0x50) == 0','param_7 + 0xa0','param_7 + 0xa1')
includes(f,0x1801c9870,'*(undefined2 *)(param_2 + 0x82) = 0','uVar27 = *(int *)(pbVar17 + 0x24) + 1','for (; uVar27 <= uVar28;')
includes(f,0x1801be930,'*(undefined8 *)(param_2 + 0x2c0)','*(undefined8 *)(param_2 + 0x2c8)','*(undefined8 *)(param_2 + 0x2d8)','*(undefined8 *)(param_2 + 0x2e0)','FUN_1801bb020','FUN_1801bbf80','FUN_1801bc900')
assert b.data[b.off(0x1802eb358):b.off(0x1802eb358)+4]==b'zh_\0'
data=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(data).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
name=bytes(x^(0x14,0x12,0x19,0x77)[i%4] for i,x in enumerate(data[24:80])).split(b'\0')[0]
assert name==b'ru_RU-ak-cur.lite' and not name.startswith(b'zh_')
metadata=data[572577:572763]
flags=0x21703&~struct.unpack_from('<I',metadata,13*4)[0]
assert flags==0x21400 and metadata[128]==16
assert struct.unpack_from('<H',metadata,64+13*2)[0]==125
assert struct.unpack_from('<H',metadata,162+2*2)[0]==20
assert struct.unpack_from('<3f',data,195746)==(0.,1.,1.)
assert struct.unpack_from('<h',data,195811)[0]!=0 and struct.unpack_from('<h',data,195817)[0]//3==10
report=dict(checks=checks,sourceIdentifier=name.decode(),zhPrefix=False,mainFlags=flags,mode=13,variant=2,
 sourceArcs=125,sourceAlternatives=20,prune=True,penupCap=10,
 scope='prepared fresh ordinary240, no prior/events; actual mainNN/cache; linguistic symbol masks remain prepared input')
Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS {len(checks)} static main NN/context/cache/name/flag bindings; no vendor execution')
