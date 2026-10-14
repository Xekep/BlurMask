"""Pinned static source/provider/2E3 proof. Reads PE and resource bytes only."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
parser=argparse.ArgumentParser();parser.add_argument('--output',required=True);args=parser.parse_args()
b=Binary('MyScriptText.dll');fs=functions('MyScriptText.dll');checks={}
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def includes(addr,*terms):
    text=fs[f'{addr:x}'];assert all(t in text for t in terms),(hex(addr),terms)
    checks[f'{addr:x}']=list(terms)
def slot(base,offset,target):
    assert b.qwords(base+offset,1)[0]==target
    checks[f'{base:x}+{offset:x}']=f'{target:x}'
base=0x1802771a0
for offset,target in [(0x20,0x180076ba0),(0x38,0x180077120),(0x48,0x180078b10),(0x50,0x1800800c0),(0x58,0x180080360),(0x68,0x180080400)]:slot(base,offset,target)
includes(0x18015a1c0,'lVar5 + 0xc1','lVar5 + 0x70','0x2ab','0x2a9','lVar5 + 9','uVar9 = 0x2e3','param_2 + 8')
includes(0x180163970,'*(undefined8 *)(param_2 + 0x28) = param_3[4]','*(undefined8 *)(param_2 + 0x38) = param_3[6]')
includes(0x1801680d0,'FUN_18015af40','param_2 + 0x29','FUN_18015b260','FUN_18015f050')
includes(0x18015b260,'*param_3 + 0x38','+ 0xb0','+ 0xb8','*param_3 + 0x48','lVar14 + 4 + lVar26) != -1','*param_3 + 0x50','*param_3 + 0x58','*param_3 + 0x68','uVar10 < 2')
includes(0x180076ba0,'param_2 + 0x1c','param_3 + 0xc','param_2 + 0x80) = 0x3f4ccccd','DAT_18029e9d0','DAT_180277280')
includes(0x180076780,'param_2 + 0x94) = 0x40a00000','param_2 + 0x90) = 0x3f400000','param_2 + 0x98) = 0')
includes(0x180078df0,'9 < iStack_30','FUN_180078ca0','FUN_1800720c0','param_2 + 0x1c')
includes(0x18007faf0,'FUN_18007d7c0','FUN_18007a850','*(int *)(param_2 + 0x18)','iVar4 == 0x134')
#Actual fresh receiver flag writer077910 is different from source association+4.
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
instructions=list(md.disasm(b.data[b.off(0x180077910):b.off(0x180077910)+230],0x180077910))
assert any(i.address==0x180077974 and i.mnemonic=='mov' and i.op_str.endswith(', 0') for i in instructions)
checks['077974-fresh-halfX']=[f'{i.address:x} {i.mnemonic} {i.op_str}' for i in instructions if i.address==0x180077974]
ak=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert len(ak)==572763 and hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IB',ak,5233)==(0x135,2)
assert struct.unpack_from('<4f',ak,5285)==tuple(map(lambda v:struct.unpack('<f',struct.pack('<f',v))[0],[.96,.85,3,10]))
assert ak[5301:5305]==bytes([0,1,1,1]) and struct.unpack_from('<f',ak,5305)[0]==.25
for offset,kind,size,version in [(5309,0x2ab,127,0x101),(5325,0x2a9,71,0x100)]:
    assert struct.unpack_from('<IIIHH',ak,offset)==(0x101fe90,size,kind,0,version)
assert ak[5350]==0
models=[]
for filename,size,sha,address in [('line-2e3-30.res',85812,'10f4b43cd02873f1995f543dece70e70c1f22cbdb7dcbf63e1c5c205966ac70e',0x18029e9d0),('line-2e3-50.res',161332,'01f9fd0bf4b5c0c3c1acc170ee82938e24a132696acbfbcfb34573a9d0550c7e',0x180277280)]:
    data=(ROOT/'models/line-geometry'/filename).read_bytes()
    assert len(data)==size and hashlib.sha256(data).hexdigest()==sha and data==b.data[b.off(address):b.off(address)+size]
    models.append(dict(path=filename,sha256=sha,bytes=size,embeddedVA=hex(address)))
report=dict(checks=checks,sourceSHA256=hashlib.sha256(ak).hexdigest(),models=models,
    scope='Fresh main0 direct source, source15B260 unknown memberships only; descriptor splitting and15F050 physical order remain incomplete')
Path(args.output).write_bytes((json.dumps(report,indent=2)+'\n').encode('utf8'))
print(f'PASS {len(checks)} pinned source/provider/receiver checks +actualresource +2 embedded models; no DLL execution')
