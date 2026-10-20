"""Static raw C49 leaf receivers used by the cropped-view factory.
These exact ret-terminated instruction extents are not Ghidra full-body claims.
"""
from pathlib import Path
import argparse,hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,default=Path(__file__).resolve().parents[2]);a=p.parse_args()
sys.path.insert(0,str(a.native_repo/'research/myscript/scripts'));from inspect_text_features import Binary
b=Binary('MyScriptInk.dll');assert hashlib.sha256(b.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
md=Cs(CS_ARCH_X86,CS_MODE_64)
rows=[(0x1800e65d0,0x180052740,0x180052744,[('mov','rax, rdx'),('ret','')]),
 (0x1800e65d8,0x18008e000,0x18008e015,[('cmp','dword ptr [r8 + 4], -1'),('jne','0x18008e014'),
 ('mov','rax, qword ptr [rdx + 0x28]'),('mov','ecx, dword ptr [rax + 8]'),('dec','ecx'),('mov','dword ptr [r8 + 4], ecx'),('ret','')])]
report=[]
for slot,start,end,expected in rows:
 assert b.qwords(slot,1)[0]==start
 raw=b.data[b.off(start):b.off(end)];xs=list(md.disasm(raw,start))
 assert [(i.mnemonic,i.op_str)for i in xs]==expected and sum(i.size for i in xs)==len(raw)
 report.append(dict(slot=hex(slot),start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest(),instructions=expected))
a.output.write_text(json.dumps(dict(vendorExecution=False,leafReceivers=report,
 scope='C49 runtime V20 returns exact original object; V28 replaces only end=-1 with stored point-count minus1. C1E service table is distinct from this runtime object table; not a new full Ghidra or C4C/DOM admission claim.'),indent=2)+'\n',encoding='utf8')
print('PASS2 exact C49 leaf receivers/9 instructions/runtime V20+V28 bindings; sentinel endpoint only, no hidden coordinate normalization.')
