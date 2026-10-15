"""Fresh ordinary frontend and first-word metadata dispatch; static PE only."""
import hashlib,json,sys
from pathlib import Path
import capstone
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);pins=[]
for address,mnemonic,operands in [
    (0x180204323,'mov','rdx, qword ptr [r13 + 0x20]'),
    (0x180204331,'mov','r9, qword ptr [rcx + 0x280]'),
    (0x18020433b,'call','r9'),
    (0x180155710,'mov','rdx, qword ptr [rdx + 0x70]'),
    (0x180155717,'mov','r9, qword ptr [rax + 0x1c8]'),
    (0x18015571e,'jmp','r9'),
    (0x18005a400,'xor','al, al'),(0x18005a402,'ret',''),
    (0x180204d86,'mov','byte ptr [rsp + 0x28], 0'),
    (0x180204d8b,'mov','r9, rsi'),
    (0x180204d91,'mov','word ptr [rsp + 0x20], r12w'),
    (0x180204d9a,'mov','r10, qword ptr [rax + 0x70]'),
    (0x180204d9e,'call','r10'),
    (0x180204da5,'movups','xmm7, xmmword ptr [rax]'),
    (0x180205368,'movups','xmmword ptr [rbp + 0x188], xmm7')]:
    i=next(cs.disasm(b.data[b.off(address):b.off(address)+16],address))
    assert (i.mnemonic,i.op_str)==(mnemonic,operands),(hex(address),i.mnemonic,i.op_str)
    pins.append([hex(address),mnemonic,operands])
tables=[]
for table,offset,target in [(0x1802de610,0x280,0x180155710),(0x1802e1ff0,0x1c8,0x18005a400)]:
    assert b.qwords(table+offset,1)[0]==target
    tables.append([hex(table),hex(offset),hex(target)])
out=dict(instructions=pins,tables=tables,scope='Fresh ordinary frontend280=false and source70 whole-token dispatch; not full case scheduler or OCR')
if len(sys.argv)>1:
    p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
    p.write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
print('PASS static PE-only15 instruction pins/2 table bindings; ordinary frontend280 false independently of associated-source state')
