"""Exact219580 caller/218560 result/zero-residual217DB0 gates. PE reads only."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
start,end=0x180218560,0x1802188e1
h=hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()
assert h=='9b322a862d1f0a9b22edef7d3eff93c8ff94be6c7853d13c69e71dba89468744'
pins={
 0x180219b1a:('mov','byte ptr [rsp + 0x48], 0'),
 0x180219b23:('mov','byte ptr [rsp + 0x40], 1'),
 0x180219b33:('mov','qword ptr [rsp + 0x38], rax'),
 0x180219b3b:('mov','dword ptr [rsp + 0x30], 0xffffffff'),
 0x180219b46:('mov','dword ptr [rsp + 0x28], 0xffffffff'),
 0x180219b4e:('mov','word ptr [rsp + 0x20], 0xffff'),
 0x180219b55:('call','0x180218560'),
 0x180217e05:('cmp','dword ptr [rdx + 0x24b0], 0'),
 0x180217e1c:('je','0x180218501'),
 0x180217e26:('cmp','byte ptr [rax + 0x99], 0'),
 0x180217e2d:('jne','0x180218501'),
 0x180218848:('call','0x1802173e0'),
 0x18021884d:('test','al, al'),
 0x18021884f:('je','0x1802188cf'),
}
for address,wanted in pins.items():
    i=next(md.disasm(b.data[b.off(address):b.off(address)+16],address));assert(i.mnemonic,i.op_str)==wanted,(hex(address),i.mnemonic,i.op_str)
residual=f['180217db0'];assert residual.index('param_2 + 0x24b0')<residual.index('param_2 + 0x20')
assert residual.rstrip().endswith('}') and 'return 1;' in residual
chain=f['180218560'];out=chain.index('*param_8 = uVar11')
assert chain.rfind('FUN_1802173e0',0,out)>chain.rfind("if (local_res8[0] == '\\0')",0,out)
assert 'if ((!bVar3) && (param_8 !=' in chain[:out]
report=dict(fullBody=[hex(start),hex(end),h],exactPins=len(pins),
 connection=dict(previousSymbol=65535,topAcBits='FFFFFFFF',topB0Bits='FFFFFFFF',conversion=1,initialForceDuplicate=False),
 scope='actual218A30 numeric ordinary/base rows, same TOP and scorers, zero residual; no219580 eligibility/intern/WORD cost update',vendorExecution=False)
Path(sys.argv[1]).write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS full218560 hash/{len(pins)} exact caller/gate pins; first-new success-only and zero-residual bypass.')
