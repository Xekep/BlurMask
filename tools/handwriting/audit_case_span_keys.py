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
for k in ['1801fd240','180201270']:
 assert '*(uint *)(param_2 + 0x1668) = param_4;' in f[k]
 assert ('*(undefined4 *)(param_2 + 0x1664) = param_3;' in f[k] or '*(uint *)(param_2 + 0x1664) = param_3;' in f[k])
report={'pins':pins,'queryBase':'1648; +1C1664=currentNode; +201668=currentSpan',
 'scope':'First actual sourceV80 epoch; monotonically increasing supplied D0 spans within next node; same source/model/context owner; no external query/refreshed map',
 'notClaimed':'Full240/290 scheduler, arbitrary repeated or reordered spans, persistent requests outside proven keys'}
if len(sys.argv)>1:
 out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,indent=2)+'\n')
print('PASS static PE-only12 writer/comparison/conditional-jump pins: actual node/span keys invalidate BOTH292 and2B7 query slots')
