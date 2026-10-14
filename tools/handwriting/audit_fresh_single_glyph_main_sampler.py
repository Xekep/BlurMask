"""Pinned PE-only main sampler/point-map/line-field audit. Never executes DLLs."""
import argparse, hashlib, json, struct, sys
from pathlib import Path
import capstone

ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary, functions

parser=argparse.ArgumentParser();parser.add_argument('--output',required=True);args=parser.parse_args()
b=Binary('MyScriptText.dll');fs=functions('MyScriptText.dll')
sha=hashlib.sha256(b.data).hexdigest()
assert sha=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
checks={}
def includes(address,*terms):
 text=fs[f'{address:x}']
 assert all(term in text for term in terms),(hex(address),terms)
 checks[hex(address)]=list(terms)
def slot(base,offset,target):
 assert b.qwords(base+offset,1)[0]==target,(hex(base),hex(offset))
 checks[f'{base:x}+{offset:x}']=hex(target)
slot(0x1802df340,0x30,0x180168b40)
slot(0x1802df340,0x40,0x180168b90)
slot(0x1802df340,0x70,0x180168c40)
slot(0x1802df340,0x80,0x180168ca0)
slot(0x1802e1ff0,0x10,0x1801a6980)
slot(0x1802e1490,0x18,0x18018d920)
slot(0x1802e1490,0x38,0x180193380)
slot(0x1802de610,0xe8,0x180152280)
includes(0x18018d400,'*(undefined4 *)(param_2 + 0x244) = 0','*(undefined1 *)(param_2 + 0x242) = 0')
includes(0x18018d590,'FUN_18018d400','lVar9 = 3','puVar5 = (undefined8 *)(param_3 + 8)')
assert '0x244' not in fs['18018d590'] and '0x244' not in fs['18018d920']
includes(0x180196500,'if (param_4 < 0x107)','*(undefined1 *)(param_2 + 0x134) = 0','*(undefined8 *)(param_2 + 0x138) = 0','if (0x107 < param_4)')
includes(0x180192c80,"*(char *)(param_2 + 0x242) == '\\0'",'return 1')
includes(0x180193380,'FUN_180191950','*puVar4,puVar4[1]','&local_48,&local_58,0')
includes(0x180191950,'*(int *)(param_2 + 0x244) != 1','param_2 + 0x1c0) + 0x60')
includes(0x180152280,'+ 0x68','+ 0x78','return *(undefined4 *)(lVar3 + 0x10)')
includes(0x18019f850,'local_158._6_2_ = 0','local_158._2_2_ = 0xffff')
includes(0x180150d00,'0x23f','local_b8[0xc]','cVar4 !=')
includes(0x1801a6980,'local_37 = param_3[5]')
includes(0x18018d920,'*(undefined1 *)(param_2 + 0x210) = *(undefined1 *)(param_3 + 1)')
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
instructions=list(md.disasm(b.data[b.off(0x180168b40):b.off(0x180168b40)+39],0x180168b40))
assert any(i.mnemonic=='mov' and i.op_str=='dword ptr [rsp + 0x28], 1' for i in instructions)
assert any(i.mnemonic=='call' and i.op_str=='0x18016aa00' for i in instructions)
checks['168b40-map-factor1']=[f'{i.address:x} {i.mnemonic} {i.op_str}' for i in instructions]
ak=ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res';data=ak.read_bytes()
assert hashlib.sha256(data).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert data[188173:188222].hex()=='0000003f3333b33ecdcc4c3e6666e63f0001003333b33ecdcc4c3e0000803e0000003f0000003fcdcccc3d060000804001'
assert struct.unpack_from('<H',data,188171)[0]==0x106
assert data[189750:189752]==b'\xff\xff' and struct.unpack_from('<H',data,189752)[0]==100
report=dict(textSHA256=sha,akSHA256=hashlib.sha256(data).hexdigest(),checks=checks,
 actualMainSampler=188157,actualMainCategories=188222,actualMainGraph=189379,actualMainAcoustic=189734,
 scope='fresh single physical/single source720, no overrides/prior, nonclosed physical; controller240 selection remains external')
Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS {len(checks)} pinned static main sampler/source-map/line-ID checks; no vendor execution')
