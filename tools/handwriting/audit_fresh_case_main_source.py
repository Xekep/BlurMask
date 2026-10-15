"""Fresh20E13B-only factory→noAssociated/sourceV60→no1FE740. Static PE only."""
import hashlib,json,struct,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);pins=[]
def pin(a,m,o):
 i=next(cs.disasm(b.data[b.off(a):b.off(a)+16],a));assert (i.mnemonic,i.op_str)==(m,o),(hex(a),i.mnemonic,i.op_str);pins.append([hex(a),m,o])
def slot(table,offset,target):assert b.qwords(table+offset,1)[0]==target;pins.append([hex(table),hex(offset),hex(target)])
parents={}
for t,parent in [(0x13b,0x139),(0x139,0x27b),(0x27b,0x138),(0x13d,0x138)]:
 address,q=b.descriptor(t);assert q[1]>>32==parent;parents[hex(t)]=[hex(address),hex(parent)]
#Registered20E constructor, not merely a nearby unreferenced initializer.
slot(0x1802e73d0,0x18,0x1802e7450);slot(0x1802e7450,0x10,0x1802e73b8);slot(0x1802e73b8,0,0x1801eada0)
for a,m,o in [
 (0x1801eada0,'xor','eax, eax'),(0x1801eadb2,'mov','qword ptr [rdx + 0x28], rax'),
 (0x1801eaed5,'mov','edx, 0x27b'),(0x1801eaf02,'je','0x1801eaf81'),
 (0x1801eaf77,'mov','qword ptr [rdx], rbp'),(0x1801eaf7a,'inc','dword ptr [rdi + 0x10]'),
 (0x1801eaf7d,'jmp','0x1801eaff2'),(0x1801eaf85,'mov','edx, 0x13d'),
 (0x1801eafee,'mov','qword ptr [rdi + 0x28], rbp'),
 (0x18020baba,'xor','esi, esi'),(0x18020bb68,'mov','qword ptr [rbx + 0x2300], rsi'),
 (0x18020bd00,'mov','byte ptr [rbx + 0x24b8], sil'),
 (0x18020c8b0,'mov','rax, qword ptr [rdx + 0x2300]'),
 (0x1801eb5e1,'mov','rbx, qword ptr [rdi + 0x28]'),(0x1801eb5e8,'mov','r9, qword ptr [rax + 0x50]'),
 (0x1801eb5ef,'cmp','rax, rbx'),(0x1801eb5f2,'je','0x1801eb60b'),
 (0x1801eb600,'mov','r9, qword ptr [rax + 0x48]'),(0x1801eb604,'call','r9'),
 (0x18005a400,'xor','al, al'),(0x18005a402,'ret','')]:pin(a,m,o)
slot(0x1802e7cb0,0x48,0x18020c7f0);slot(0x1802e7cb0,0x50,0x18020c8b0)
slot(0x1802e85b0,0x60,0x18005a400);slot(0x1802e8c60,0x60,0x18005a400);slot(0x1802e8bb0,0x60,0x180236c20)
assert '*(longlong *)(param_2 + 0x2300) = param_3;' in f['18020c7f0']
assert '(param_1,0x27b)' in f['1801eaeb0'] and '(param_1,0x13d)' in f['1801eaeb0']
assert '**(longlong **)(param_2 + 0x30) + 0x60' in f['1802144f0'] and 'FUN_1801fe740' in f['1802144f0']
lk=(ROOT/'models/ru-RU/ru_RU-lk-text.lite.res').read_bytes();assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
assert struct.unpack_from('<I',lk,606964+8)[0]==0x13b
report={'pins':pins,'parents':parents,'selectedWire13BOffset':606964,
 'factory':'NEW20E28zero; onlyactual13B source is139→27B, firstAddSource branch writesconfiguredlist notassociated28;1EB340 V50matches0 so noV48change2300',
 'gate':'292/290.V60false;28D ORonly292 false;2300zero→2144 first1FE740 gatefalse',
 'scope':'No external/reused/13D/configuration-event sources; no fullcase controller or winner claim'}
if len(sys.argv)>1:
 out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,indent=2)+'\n')
print(f'PASS static PE-only freshcase noAssociated: {len(pins)} instruction/table pins,4 registeredparent links,actual13B wire')
