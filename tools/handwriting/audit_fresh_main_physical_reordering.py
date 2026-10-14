"""Static PE audit of fresh nullE8 and15F050 scale/reset/interval bindings."""
import argparse,hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
parser=argparse.ArgumentParser();parser.add_argument('--output',required=True);args=parser.parse_args()
b=Binary('MyScriptText.dll');fs=functions('MyScriptText.dll');checks={}
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def includes(addr,*terms):
    text=fs[f'{addr:x}'];assert all(t in text for t in terms),(hex(addr),terms)
    checks[f'{addr:x}']=list(terms)
includes(0x180163970,'*(undefined8 *)(param_2 + 0xe0) = 0','*(undefined8 *)(param_2 + 0xe8) = 0')
includes(0x1801680d0,'*(longlong **)(param_2 + 0xe8) == (longlong *)0x0','**(longlong **)(param_2 + 0xe8) + 0x18','FUN_180161120','FUN_18015eaa0','FUN_18015f050')
includes(0x180076ad0,'FUN_180072030','param_2 + 0x120','*(undefined8 *)(param_2 + 0x98) = 0','*(undefined4 *)(param_2 + 0xbc) = 0')
assert '0x190' not in fs['180076ad0'] and '0x198' not in fs['180076ad0'] and 'param_2 + 0x12)' not in fs['180076ad0']
checks['076ad0-preserves19019812']='reset body does not write scale or scale-active flag'
includes(0x180161120,'*(int *)(lVar27 + 0x20 + lVar13) != 0x2000002','if (bVar20)','FUN_18016aa00')
includes(0x18015eaa0,'sVar22 = -1','if (sVar5 < sVar22)','return 1')
includes(0x18015d360,'*pbVar2 = *pbVar2 & 0xfe')
includes(0x18015f050,'*param_3 + 0x10','*param_3 + 0x48','*param_3 + 0x50','*param_3 + 0x58','*param_3 + 0x68','FUN_1800722c0','(double)*(float *)(param_2 + 0x2c)','+ 0xb0','pfVar15[2]','*(char *)(local_f8 + 0x48 + local_100)','FUN_18016aa00')
assert '*param_3 + 0x38' not in fs['18015f050']
checks['15f050-no-scale38']='does not reinitialize provider from source scales between descriptor calls'
includes(0x180078df0,'9 < iStack_30','FUN_180078ca0')
includes(0x18007d7c0,'FUN_18007ca80','FUN_180078df0')
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
setter=list(md.disasm(b.data[b.off(0x180163f50):b.off(0x180163f50)+8],0x180163f50))
assert [(i.mnemonic,i.op_str) for i in setter]==[('mov','qword ptr [rdx + 0xe8], r8'),('ret','')]
assert b.qwords(0x1802df0a0+0x28,1)[0]==0x180163f50
checks['219metadata28-sourceE8externalsetter']=[f'{i.address:x} {i.mnemonic} {i.op_str}' for i in setter]
scale=list(md.disasm(b.data[b.off(0x180077120):b.off(0x180077120)+90],0x180077120))
assert any(i.mnemonic=='movss' and i.op_str=='dword ptr [rdx + 0x190], xmm2' for i in scale)
assert not any(i.mnemonic=='movss' and '0x198]' in i.op_str for i in scale[:9]) #slot38 ignores its secondary argument
assert any(i.mnemonic=='mov' and i.op_str=='byte ptr [rdx + 0x12], 1' for i in scale)
checks['provider38-active-scale']=[f'{i.address:x} {i.mnemonic} {i.op_str}' for i in scale if any(s in i.op_str for s in ['0x190','0x198','0x12]'])]
Path(args.output).write_bytes((json.dumps(dict(checks=checks,scope='Fresh direct219 ctor/noexternalsetter/propertyevents/prior/pageguides; not general SDK or loaded249 callback state'),indent=2)+'\n').encode('utf8'))
print(f'PASS {len(checks)} pinned fresh nullE8/reset/carry/interval checks; no vendor execution')
