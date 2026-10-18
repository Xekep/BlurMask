"""Static math symbol/result DOM caller contracts; no vendor execution."""
from pathlib import Path
import hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
d=Binary('MyScriptDocument.dll');e=Binary('MyScriptEngine.dll')
assert hashlib.sha256(d.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
regions=[
 (0x180027e20,0x180028070,'2ccfdea7db43b2090c4bc0c313d37cdeb631d32e9e1331756ba5a60b55f68914'),
 (0x180028070,0x180028a80,'a562c726af51e297760f00aab733045a4acf563ab49c34e6899cf3382e594cac'),
 (0x18002af50,0x18002b3c0,'e419fb396004c5f90e36eba1b8369709329681ae278b05dfcc7aeeadbc743c98')]
md=Cs(CS_ARCH_X86,CS_MODE_64);instructions={}
for a,z,h in regions:
 raw=d.data[d.off(a):d.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 instructions.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
pins={
 0x18002b28d:('lea','rax, [rbp - 0x11]'),
 0x18002b294:('mov','qword ptr [rsp + 0x28], rax'),
 0x18002b299:('lea','r9, [rbp - 1]'),
 0x18002b2a0:('mov','qword ptr [rsp + 0x20], r13'),
 0x18002b2a8:('call','0x180028070'),
 0x18002834d:('lea','rcx, [rdi + rdi*4]'),
 0x180028356:('lea','r9, [rax + rcx*8]'),
 0x18002836a:('mov','qword ptr [rsp + 0x28], r12'),
 0x18002836f:('mov','qword ptr [rsp + 0x20], rax'),
 0x180028374:('call','0x180027e20'),
 0x18002837c:('je','0x1800287d0'),
 0x180027e2e:('mov','r14, r8'),
 0x180027e31:('mov','r12, r9'),
 0x180027e57:('mov','r8d, dword ptr [r12 + 8]'),
 0x180027e91:('mov','rdx, qword ptr [r12]'),
 0x180027e99:('mov','r10, qword ptr [rax + 0x180]'),
 0x180027ef2:('mov','r9, qword ptr [r12]'),
 0x180027efc:('mov','r10, qword ptr [rax + 0x28]'),
 0x180027f00:('mov','eax, dword ptr [r12 + 8]'),
 0x180027f0c:('neg','rax'),
 0x180027f11:('je','0x18002802d'),
 0x180027f35:('mov','r10, qword ptr [rcx + 0x128]'),
 0x180027fe0:('cmp','dword ptr [r12 + 0x18], ebx'),
 0x180027fe7:('mov','rax, qword ptr [r12 + 0x10]'),
 0x180027fec:('mov','r8, qword ptr [rax]'),
 0x180028000:('mov','r10, qword ptr [rax + 0x80]'),
 0x180028039:('mov','r9, qword ptr [rax + 0x90]'),
 0x18002887a:('mov','r10, qword ptr [rax + 0x10]'),
 0x180028896:('mov','r9, qword ptr [rcx + 0x228]'),
 0x1800288b8:('mov','r9, qword ptr [rax + 0x40]'),
 0x1800288d7:('mov','r10, qword ptr [rax + 0x48]'),
 0x1800288f0:('mov','r8, qword ptr [rax + 0x50]'),
 0x180028930:('mov','r10, qword ptr [rax + 0x208]'),
 0x180028944:('je','0x1800289c1'),
 0x180028963:('mov','r10, qword ptr [rcx + 0x178]'),
 0x180028973:('je','0x1800289c1'),
 0x18002899b:('mov','r10, qword ptr [rcx + 0x18]'),
 0x180028a1a:('mov','r9, qword ptr [rax + 0xe8]'),
 0x180028a2d:('mov','r9, qword ptr [rcx + 0x90]'),
 0x18002b312:('add','rbx, 0x20')}
for a,p in pins.items():assert instructions[a]==p,(hex(a),instructions[a],p)
bindings=[]
def bind(binary,table,slot,target):
 assert binary.qwords(table+slot,1)==(target,)
 bindings.append(dict(dll=binary.name,table=hex(table),slot=hex(slot),target=hex(target)))
#Registered CB8 constructor metadata and actual object table, not an inferred
#interface selected because its numerical slot happens to have the same value.
bind(d,0x1801ed890,0x18,0x1801ed8d8)
bind(d,0x1801ed8d8,8,0x1801ed830)
for slot,target in [(0x10,0x180033040),(0x28,0x180033560),(0x40,0x1800337e0),(0x48,0x180033920),(0x50,0x1800339d0)]:bind(d,0x1801ed830,slot,target)
for table in (0x180227080,0x1802273a0):
 for slot,target in [(0x68,0x1800ed3a0),(0x80,0x1800ed510),(0x90,0x1800ea480),(0xa8,0x1800eaaf0),(0x110,0x1800ebaf0),(0x128,0x1800eda40),(0x148,0x1800edca0),(0x1b8,0x1800ef340),(0x208,0x1800efa00),(0x228,0x1800efc40),(0x230,0x1800efc90)]:bind(e,table,slot,target)
literals={0x18020c7dc:b'tag',0x18020d700:b'nrs',0x18020d704:b'char',0x18020d654:b'name',0x18020d034:b'ref',0x18020d2cf:b'',0x18020d6a4:b'symbol',0x18020d6b0:b'MATH_SYMBOL'}
for a,s in literals.items():assert d.data[d.off(a):d.off(a)+len(s)+1]==s+b'\0'
report=dict(staticOnly=True,vendorExecution=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(p)for a,p in pins.items()},bindings=bindings,literals={hex(a):s.decode()for a,s in literals.items()},
 contract='028070->027E20 supplies actual40B symbol rows; separate32B freeze rows have flag1C. Ranges, alternate strings, CB8 negative tags, DAF transforms, DOM patch and matching original source edits are separate owners.',
 boundary='No equality of shape with completed168B records, units50/runs68/alternatives78, or library40B output rows establishes their missing producer/transport join. Original field/source selection and native result acceptance are not implemented by this audit.')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} complete caller/helper regions/{len(pins)} pins/{len(bindings)} bindings/{len(literals)} literals; symbol DOM transport only, no runtime acceptance.')
