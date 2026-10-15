"""21FFD0 predecessor tail and initialization. Static PE/decomp inspection only."""
import hashlib,json,sys
from pathlib import Path
import capstone
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(f['18021ffd0'].encode()).hexdigest()=='17f379f2c815899fd745e3abfacef52039c9ffbc4bc8728c8ede05dc1bdcb2d4'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
start,end=0x1802213f5,0x1802214ca
raw=b.data[b.off(start):b.off(end)]
assert hashlib.sha256(raw).hexdigest()=='cd79f0a46c9efb29e7441c696811c8b71fcde968c5498d8ff8d1e116387486d0'
tail=list(cs.disasm(raw,start));assert sum(i.size for i in tail)==len(raw)
pins=[]
for address,mnemonic,operands in [
 (0x18022018b,'mov','rax, qword ptr [rsi + 0x2478]'),
 (0x1802201aa,'mov','r14d, dword ptr [rax + rcx*4]'),
 (0x1802201ae,'add','r14d, edi'),
 (0x1802201d5,'mov','rax, qword ptr [rdx + r8*8 + 8]'),
 (0x1802201da,'sar','rax, 0x18'),
 (0x1802201ff,'cmp','word ptr [rbp + 0x230], r9w'),
 (0x180220207,'jne','0x180220216'),
 (0x180220209,'movzx','eax, word ptr [rdx + r8*8 + 0x1e]'),
 (0x18022020f,'mov','word ptr [rbp + 0x230], ax'),
 (0x180220572,'movsxd','r14, dword ptr [rdx + rcx*8 + 0x10]'),
 (0x18022057c,'cmp','r14d, -1'),
 (0x180220580,'jne','0x1802213f5'),
 (0x180220586,'mov','eax, r14d'),
 (0x180220589,'movzx','edx, r8w'),
 (0x18022058d,'jmp','0x1802214c2'),
 (0x180220447,'mov','r11d, 0xfffd'),
 (0x1802204fb,'mov','r11d, 0xfffd'),
 (0x18022052d,'mov','r11d, 0xfffd'),
 (0x180220f88,'mov','r11d, 0xfffd'),
 (0x1802213ca,'mov','r11d, 0xfffd')]:
    i=next(cs.disasm(b.data[b.off(address):b.off(address)+16],address))
    assert (i.mnemonic,i.op_str)==(mnemonic,operands),(hex(address),i.mnemonic,i.op_str)
    pins.append([hex(address),mnemonic,operands])
out=dict(tail=[(hex(i.address),i.mnemonic,i.op_str) for i in tail],bootstrap=pins,
    scope='Prepared compact predecessor cursor only; no207CB0 scheduler,21FFD0 glyph/callback/blank stages or recognized text')
if len(sys.argv)>1:
    p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
    p.write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
print(f'PASS static full{len(tail)}-instruction predecessor tail/20 bootstrap-threshold pins and complete21FFD0 decomp hash; no vendor execution')
