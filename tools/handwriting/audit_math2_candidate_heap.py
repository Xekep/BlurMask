"""Pinned0351E0 heap and actual unrestricted caller; static bytes only."""
from pathlib import Path
import sys,hashlib,json
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions=[
 (0x1800351e0,0x180035330,'6af1234dec27e9848d9ce2b22ec3c110ba9829ba77b0e76f68073483a1da48a5'),
 (0x180038610,0x18003a260,'180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'),
]
pins=[
 (0x1800351e2,'mov','ecx, dword ptr [rdx + 4]'),
 (0x1800351ea,'je','0x180035324'),
 (0x1800351f0,'mov','eax, dword ptr [rdx]'),
 (0x1800351f4,'jne','0x1800352c0'),
 (0x1800351fe,'comiss','xmm2, dword ptr [r10]'),
 (0x180035202,'jae','0x180035324'),
 (0x180035220,'lea','r9d, [r11*2 + 1]'),
 (0x180035245,'comiss','xmm0, dword ptr [r10 + rax*8]'),
 (0x18003524a,'cmovae','r9d, r8d'),
 (0x180035255,'comiss','xmm2, dword ptr [r10 + rdx*8]'),
 (0x18003525a,'jae','0x180035293'),
 (0x180035272,'movups','xmmword ptr [r10 + rcx*8], xmm0'),
 (0x18003527d,'movups','xmmword ptr [r10 + rcx*8 + 0x10], xmm1'),
 (0x180035289,'movups','xmmword ptr [r10 + rcx*8 + 0x20], xmm0'),
 (0x180035291,'jb','0x180035220'),
 (0x1800352af,'movups','xmmword ptr [rax], xmm0'),
 (0x1800352b2,'movups','xmmword ptr [rax + 0x10], xmm0'),
 (0x1800352b6,'movups','xmmword ptr [rax + 0x20], xmm0'),
 (0x1800352ba,'movss','dword ptr [rax], xmm2'),
 (0x1800352c5,'mov','dword ptr [rdx], eax'),
 (0x1800352e6,'comiss','xmm0, xmm2'),
 (0x1800352e9,'jae','0x180035298'),
 (0x1800352fd,'movups','xmmword ptr [r8 + rcx*8], xmm0'),
 (0x180035308,'movups','xmmword ptr [r8 + rcx*8 + 0x10], xmm1'),
 (0x180035314,'movups','xmmword ptr [r8 + rcx*8 + 0x20], xmm0'),
 (0x18003531d,'jne','0x1800352d0'),
 (0x180039170,'movss','xmm2, dword ptr [r12 + rbx*4]'),
 (0x18003917d,'call','0x1800351e0'),
 (0x180039187,'mov','dword ptr [rax + 0x28], edi'),
 (0x18003918a,'mov','qword ptr [rax + 8], rbx'),
 (0x18003918e,'mov','qword ptr [rax + 0x10], r13'),
 (0x180039192,'mov','dword ptr [rax + 0x18], r15d'),
 (0x180039196,'mov','qword ptr [rax + 0x20], 0xffffffffffffffff'),
 (0x1800391a4,'jb','0x180039170'),
 ]
md=Cs(CS_ARCH_X86,CS_MODE_64);actual={}
for lo,hi,digest in regions:
 raw=b.data[b.off(lo):b.off(hi)];assert hashlib.sha256(raw).hexdigest()==digest
 actual.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,lo)})
for address,mnemonic,operands in pins:assert actual[address]==(mnemonic,operands),hex(address)
report=dict(regions=len(regions),pins=len(pins),recordBytes=48,
 comparisons='COMISS/JAE ordered >=; unordered falls through; equal children select right',
 mutation='count increments before parent moves; helper zeroes entire returned48B and writes cost; caller then writes28,8,10,18,20',
 boundary='Prepared owned heap only; no actual logits/alias/constraint/EOS beam integration or recognition claim',vendorExecution=False)
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} static regions/{len(pins)} heap/caller instruction pins; no SDK execution')
