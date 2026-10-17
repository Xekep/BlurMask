"""BE2 queue4-before-V30, queue2-after-V30 and destructive replace prefix."""
import argparse,hashlib,json,runpy,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64

root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
reports=[]
for name in ['audit_library_text_dom_target.py','audit_main_document_be2_transport.py']:
 path=a.output.with_name(a.output.stem+'-'+name.removeprefix('audit_').removesuffix('.py')+'.json')
 old=sys.argv;sys.argv=[name,str(path)]
 try:runpy.run_path(str(root/'tools/handwriting'/name),run_name='__main__')
 finally:sys.argv=old
 reports.append(dict(path=str(path),sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
e=Binary('MyScriptEngine.dll');t=Binary('MyScriptText.dll');md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];pins=0
def region(b,start,end,expected):
 global pins
 raw=b.data[b.off(start):b.off(end)];rows=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
 for x in expected:assert x in rows,(b.name,x)
 pins+=len(expected);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
region(t,0x180061510,0x180061680,[
 (0x1800615ed,'call','r10'),(0x180061614,'call','r9'),(0x180061653,'call','r10')])
region(t,0x1800658e0,0x180065d00,[
 (0x180065908,'xor','r13d, r13d'),(0x18006592d,'je','0x180065c7a'),
 (0x180065c83,'mov','qword ptr [rsp + 0x48], r13')])
region(e,0x1800e1ea0,0x1800e1f90,[
 (0x1800e1ec0,'mov','r8d, 4'),(0x1800e1ecd,'call','0x1800e4440'),
 (0x1800e1ef5,'cmp','qword ptr [rdi + 0x10], rdx'),
 (0x1800e1f34,'mov','r10, qword ptr [rax + 0x88]'),(0x1800e1f3b,'call','r10')])
region(e,0x1800e2090,0x1800e2180,[
 (0x1800e20b0,'mov','r8d, 2'),(0x1800e20bd,'call','0x1800e4440'),
 (0x1800e20de,'cmp','qword ptr [rdi + 0x18], rdx')])
region(e,0x1800e3180,0x1800e31d0,[
 (0x1800e3196,'mov','r9, qword ptr [rax + 0xe8]'),
 (0x1800e31ac,'mov','r9, qword ptr [rax + 0x90]')])
#Undefined clone-null apply has no success gate before the Append call.
region(e,0x1800e2fa8,0x1800e3000,[
 (0x1800e2fc5,'call','r10'),(0x1800e2fd0,'mov','r9, rbx'),
 (0x1800e2fde,'mov','rax, qword ptr [rax + 0xa8]'),(0x1800e2ffc,'call','rax')])
after=[(x.mnemonic,x.op_str)for x in md.disasm(e.data[e.off(0x1800e2fc8):e.off(0x1800e2ffc)],0x1800e2fc8)]
assert ('test','rbx, rbx')not in after and ('test','rax, rax')not in after
a.output.write_bytes((json.dumps(dict(prerequisites=reports,regions=regions,pins=pins,vendorExecution=False,
 scope='Explicit library empty-root insert/replacement only; no SDK/C8F/C20 acceptance'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} insertion regions/{pins} pins plus frozen target/transport prerequisites.')
