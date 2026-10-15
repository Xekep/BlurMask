"""Pin both native cache keys and actual startup/continuation key writers. PE only."""
import hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);pins=[]
def pin(a,m,o):
 i=next(cs.disasm(b.data[b.off(a):b.off(a)+16],a));assert (i.mnemonic,i.op_str)==(m,o),(hex(a),i.mnemonic,i.op_str)
 pins.append([hex(a),m,o])
for a,m,o in [
 (0x1801fd366,'mov','dword ptr [r14 + 0x1664], ebx'),
 (0x1801fd36d,'mov','dword ptr [r14 + 0x1668], edx'),
 (0x18020147f,'mov','dword ptr [r12 + 0x1664], r11d'),
 (0x180201487,'mov','dword ptr [r12 + 0x1668], r9d'),
 (0x180233192,'cmp','dword ptr [r15 + 0x1c], eax'),
 (0x180233196,'jne','0x1802331c0'),
 (0x18023319e,'cmp','dword ptr [r15 + 0x20], eax'),
 (0x1802331a2,'jne','0x1802331c0'),
 (0x180093c2c,'cmp','dword ptr [rdi + 0x1c], eax'),
 (0x180093c2f,'jne','0x180093c5f'),
 (0x180093c37,'cmp','dword ptr [rdi + 0x20], eax'),
 (0x180093c3a,'jne','0x180093c5f')]:pin(a,m,o)
for a,m,o in [
 (0x1802047b6,'mov','dword ptr [r13 + 0x1664], r8d'),
 (0x1802047c1,'movsx','ecx, byte ptr [rcx + rax + 0x20]'),
 (0x1802047c6,'mov','dword ptr [r13 + 0x1668], ecx'),
 (0x180093c56,'cmp','r12b, byte ptr [rbx + 0x41c]'),
 (0x180093c5d,'je','0x180093ca7'),
 (0x180093ca0,'mov','byte ptr [rbx + 0x41c], r12b'),
 (0x180093ca7,'test','r12b, r12b'),
 (0x180093caa,'jne','0x180093cbc'),
 (0x180093cb0,'mov','r14, qword ptr [rsi + 0x50]'),
 (0x180093cb6,'mov','r14, qword ptr [rsi + 0x58]'),
 (0x180093cbc,'mov','r14, qword ptr [rsi + 0x60]')]:pin(a,m,o)
assert b.qwords(0x1802e85b0+0x48,1)[0]==0x180231ae0
assert b.qwords(0x1802b4360+0x48,1)[0]==0x180093f50
for address,digest in [
 ('180093f80','7e1117d564e41799ec03aa48d31f34036b1d63d1310b2b951d0851802831e4d8'),
 ('180231ae0','7132945bea86f341c6e3e21f0aa06d0aacf9abe1b9d5aa8849daaa7ea8e8097b')]:
 assert hashlib.sha256(f[address].encode()).hexdigest()==digest,address
assert 'param_2 + 0x60' not in f['180093f80']
for k in ['1801fd240','180201270']:
 assert '*(uint *)(param_2 + 0x1668) = param_4;' in f[k]
 assert ('*(undefined4 *)(param_2 + 0x1664) = param_3;' in f[k] or '*(uint *)(param_2 + 0x1664) = param_3;' in f[k])
report={'pins':pins,'queryBase':'1648; +1C1664=currentNode; +201668=currentSpan',
 'scope':'Normal query: actual sourceV80 epoch and increasing D0 keys; auxiliary: immutable60, signed language span and inner mode key, no normal wrapper cache',
 'notClaimed':'Full240/290 scheduler, arbitrary repeated or reordered spans, persistent requests outside proven keys'}
if len(sys.argv)>1:
 out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,indent=2)+'\n')
print('PASS static PE-only23 writer/comparison/mode/map pins and2 auxiliary dispatch bindings: normal D0 and auxiliary signed-span keys are distinct')
