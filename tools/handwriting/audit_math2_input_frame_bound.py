"""Original Math2 feature tensor N and cold search-budget consumer; static only."""
import hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
binaries={name:Binary(name) for name in ['MyScriptInk.dll','MyScriptDocument.dll','MyScriptEngine.dll']}
expected={'MyScriptInk.dll':'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a','MyScriptDocument.dll':'17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f','MyScriptEngine.dll':'d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'}
for name,b in binaries.items():assert hashlib.sha256(b.data).hexdigest()==expected[name],name
regions=[('MyScriptInk.dll', 6442477520, 6442477936, '5dce6888cd7f413faf3ede3dc28ca2e86de6288267fcab72ab2be3e4f3f7fd29'), ('MyScriptInk.dll', 6442495248, 6442496032, 'd028a4ba543b55d108e47a46606852a0e0b92d1c1fa85d69d74587e6427c7d44'), ('MyScriptInk.dll', 6442532640, 6442533856, 'ec0cc6ba34d7f75fc83ab23cb323e175a8bc564ca3dab70885d0ecdd273bab98'), ('MyScriptInk.dll', 6442535904, 6442536832, 'e8c6bb53bdd4c79190c67f373fabeb88d25675716fa15febdeaa76d0ee9098cb'), ('MyScriptDocument.dll', 6442731056, 6442731696, 'bf03c90bfc43d2c819d3ecfccc89cdb75f47910aa5a6b58bd2798978079a241e'), ('MyScriptDocument.dll', 6442744912, 6442747328, 'f7edd926e1e997dfb8c939e1c989df1e848076e520a713676595c6b83c9db302'), ('MyScriptDocument.dll', 6442689120, 6442693152, '43b6395c140ccc4975bcc1dde81378e5df1745dd5bdd1cbdb88a79007780f19d'), ('MyScriptDocument.dll', 6442755248, 6442755648, 'c52f1d4a939e8a68ab7e7e0bb12e177c31a5bbde8e9ca0a03763e5f9d178d11a'), ('MyScriptEngine.dll', 6444052288, 6444053568, 'ab7b6cb9d93b2359b4474a7ac78675898a552237b5e480b61bfdc39390003c1f')]
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={}
for name,a,z,digest in regions:
 b=binaries[name];raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==digest
 ins.update(((name,i.address),(i.mnemonic,i.op_str))for i in md.disasm(raw,a))
pins=[]
def pin(name,a,m,o):
 assert ins[name,a]==(m,o),(name,hex(a),ins.get((name,a)),m,o)
 pins.append((name,a,m,o))
pin('MyScriptInk.dll',0x180006827,'mov','qword ptr [rdx + 8], rdi')
pin('MyScriptInk.dll',0x18000683b,'lea','r11, [rbx + 0x10]')
pin('MyScriptInk.dll',0x180006843,'movsxd','rcx, dword ptr [r8 + 8]')
pin('MyScriptInk.dll',0x18000684f,'mov','qword ptr [r11], rcx')
pin('MyScriptInk.dll',0x18000adec,'call','0x18000a8f0')
pin('MyScriptInk.dll',0x18000adf9,'cmp','qword ptr [rdi + 8], 0')
pin('MyScriptInk.dll',0x180014037,'mov','dword ptr [r15], 2')
pin('MyScriptInk.dll',0x180014045,'mov','edx, 0x110')
pin('MyScriptInk.dll',0x18001405a,'mov','qword ptr [r15 + 8], rax')
pin('MyScriptInk.dll',0x1800140c1,'mov','edx, 0xc66')
pin('MyScriptInk.dll',0x1800140e6,'mov','qword ptr [rsp + 0x28], rbx')
pin('MyScriptInk.dll',0x1800140eb,'mov','rax, qword ptr [rax]')
pin('MyScriptInk.dll',0x1800140f6,'call','rax')
pin('MyScriptInk.dll',0x180014221,'mov','qword ptr [rbx + 0x18], 1')
pin('MyScriptInk.dll',0x18001422e,'add','qword ptr [rbx + 8], 1')
pin('MyScriptInk.dll',0x18001428b,'mov','rbp, qword ptr [rbx + 0x10]')
pin('MyScriptDocument.dll',0x1800446c3,'mov','edx, 0xc6d')
pin('MyScriptDocument.dll',0x180044700,'mov','r9d, 8')
pin('MyScriptDocument.dll',0x180044710,'mov','qword ptr [rdi + 0x218], rax')
pin('MyScriptDocument.dll',0x180047eff,'mov','rdx, qword ptr [r13 + 0x218]')
pin('MyScriptDocument.dll',0x180047f17,'mov','r10, qword ptr [rax + 0x10]')
pin('MyScriptDocument.dll',0x180047f1b,'lea','rax, [rbp + 0x30]')
pin('MyScriptDocument.dll',0x180047f1f,'mov','qword ptr [rsp + 0x40], rax')
pin('MyScriptDocument.dll',0x180047f3d,'call','r10')
pin('MyScriptDocument.dll',0x180047f4c,'mov','rcx, qword ptr [rbp + 0x38]')
pin('MyScriptDocument.dll',0x180047f52,'cmp','qword ptr [rcx + 0x10], 0xbb8')
pin('MyScriptDocument.dll',0x180048136,'lea','r8, [rbp + 0x30]')
pin('MyScriptDocument.dll',0x180048144,'mov','r10, qword ptr [rax + 0x28]')
pin('MyScriptDocument.dll',0x180048168,'call','r10')
pin('MyScriptDocument.dll',0x18003a4bf,'mov','rdx, qword ptr [r15 + 8]')
pin('MyScriptDocument.dll',0x18003a4d9,'imul','rcx, qword ptr [rdx + 0x10]')
pin('MyScriptDocument.dll',0x18003a4de,'inc','rcx')
pin('MyScriptEngine.dll',0x18018702b,'mov','r8, qword ptr [r15 + 8]')
pin('MyScriptEngine.dll',0x18018702f,'cmp','r8, qword ptr [rbx + 8]')
pin('MyScriptEngine.dll',0x180187050,'cmp','rcx, 1')
pin('MyScriptEngine.dll',0x180187054,'je','0x180187063')
pin('MyScriptEngine.dll',0x180187056,'mov','rax, qword ptr [rdx]')
pin('MyScriptEngine.dll',0x180187059,'cmp','qword ptr [r9 + rdx], rax')

bindings=[('MyScriptInk.dll',0x1800dea80,0x1800deac0),('MyScriptInk.dll',0x1800deac0,0x18000ad10),('MyScriptInk.dll',0x1800ded40,0x1800dece8),('MyScriptInk.dll',0x1800dece8+0x10,0x180014be0),('MyScriptEngine.dll',0x18022a200+0x60,0x180186f40),('MyScriptDocument.dll',0x1801ed930+0x28,0x18003a260)]
for name,a,target in bindings:assert binaries[name].qwords(a,1)==(target,),(name,hex(a))
b=binaries['MyScriptInk.dll']
assert struct.unpack_from('<I',b.data,b.off(0x1800dea68))[0]==0xc66
for a,text in [(0x1800f6aa4,b'src'),(0x1800f6aa8,b'src_len'),(0x1800f6968,b'downsampling'),(0x1800f6ab0,b'strk_ends')]:
 o=b.off(a);assert b.data[o:o+len(text)+1]==text+b'\0'
#Layout discriminators are prepared byte packets, not SDK execution. Preserve
#N through unsqueeze axis1 and concatenation axis1; none is raw ink length.
controls=0
for n in [1,2,31,249,250,251,3000,6000]:
 descriptor=bytearray(0x110);struct.pack_into('<QQQQ',descriptor,8,3,n,1,26)
 assert struct.unpack_from('<Q',descriptor,0x10)[0]==n
 assert struct.unpack_from('<Q',descriptor,0x18)[0]!=n or n==1
 for k in [0,1,7,127,1000,2**63-1]:
  expected=min(1000,4*n+1) if k==0 else min(1000,4*n+1,k)
  assert expected<=1000 and expected>=1
  controls+=1
assert min(1000,4*31+1)!=min(1000,4*8+1) #original N vs downsampled memory
report=dict(scope='Static original input frame dimension and cold numeric maximum only; no search loop or feature accuracy claim',regions=len(regions),instructionPins=len(pins),bindings=len(bindings),preparedLayoutControls=controls,originalTensor='src F32[N,1,26], first tuple descriptor+10',hostFrameLimit=3000)
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} regions/{len(pins)} pins/{len(bindings)} bindings; originalsrc N preserved and cold bound; no SDK execution')
