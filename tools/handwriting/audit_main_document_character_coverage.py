"""Static01C740 temporary preferred-source subtraction and split gate."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary();ink=Binary('MyScriptInk.dll');f=functions();m=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(ink.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
def dis(image,s,e):return[(i.mnemonic,i.op_str)for i in m.disasm(image.data[image.off(s):image.off(e)],s)]
def pins(image,s,e,expected):
 rows=dis(image,s,e)
 for x in expected:assert x in rows,(hex(s),x)
 return rows
caller=pins(b,0x1800296eb,0x18002978e,[('test','dword ptr [r14 + 0x30], 0x200000'),
 ('mov','ebx, dword ptr [r14 + 0x70]'),('dec','ebx'),('js','0x18002978e'),
 ('mov','r8, qword ptr [rax + 0x78]'),('call','r8'),('mov','dword ptr [rsp + 0x20], ebx'),
 ('lea','r9, [r14 + 0x18]'),('call','0x18001c740'),('cmp','dword ptr [rbp - 0x58], 0'),
 ('je','0x180029783'),('cmp','dword ptr [r14 + 0x70], 1'),('je','0x18002978e'),('call','0x180024a90')])
assert caller.index(('call','r8'))<caller.index(('call','0x18001c740'))<caller.index(('call','0x180024a90'))
pins(b,0x18001c75f,0x18001c7bb,[('mov','r15d, 0x20'),('mov','eax, 0x30'),
 ('cmp','dword ptr [r9 + 0x38], 0'),('cmove','eax, r15d'),('movups','xmm1, xmmword ptr [rax + r9]'),
 ('mov','r10, qword ptr [rax + 0x88]'),('call','r10')])
pins(b,0x18001c7c0,0x18001c828,[('xor','ebx, ebx'),('cmp','ebx, dword ptr [rdi + 0x18]'),
 ('mov','rdx, qword ptr [rax + rcx*8 + 8]'),('mov','eax, 0x30'),('cmp','dword ptr [rdx + 0x38], 0'),
 ('cmove','rax, r15'),('movups','xmm1, xmmword ptr [rax + rdx]'),('mov','r8, r14'),
 ('mov','r10, qword ptr [rax + 0xd0]'),('call','r10'),('inc','ebx'),('jmp','0x18001c7c2')])
for offset,target in[(0x60,0x18007f7f0),(0x78,0x18007f820),(0x88,0x18007fad0),(0xd0,0x1800812d0)]:assert ink.qwords(0x1800e5ac0+offset,1)[0]==target
assert dis(ink,0x18007f820,0x18007f828)==[('mov','dword ptr [rdx + 8], 0'),('ret','')]
assert dis(ink,0x18007fad0,0x18007fad5)==[('jmp','0x18007f830')]
pins(ink,0x18007f8b3,0x18007f903,[('shl','rcx, 4'),('add','rcx, qword ptr [rdi]'),
 ('mov','r8, rsi'),('mov','rdx, rbp'),('shl','r8, 4'),('call','0x1800d57e0'),('add','dword ptr [rax], esi')])
#Fresh024D40 initializes CHAR payload30 via the actualC21 constructor; the
#raw/no-C25 scope excludes the later cached-source021D10 producer.
pins(b,0x180024dd3,0x180024e11,[('lea','rdx, [rbx + 0x30]'),('mov','r8, qword ptr [rax + 0x60]'),('call','r8')])
assert dis(ink,0x18007f7f0,0x18007f7fa)==[('xor','eax, eax'),('mov','qword ptr [rdx], rax'),('mov','qword ptr [rdx + 8], rax'),('ret','')]
assert 'puVar7[7] != 0xc25'in f['180021d10']
regions=[(b,0x18001c740,0x18001c860),(b,0x1800296eb,0x18002979e),(b,0x180024dd3,0x180024e11),
 (ink,0x18007f7f0,0x18007f7fa),(ink,0x18007f820,0x18007f828),(ink,0x18007fad0,0x18007fad5),
 (ink,0x18007f830,0x18007f91e),(ink,0x1800812d0,0x180081380)]
report=dict(regions=[dict(binary=x.name,start=hex(s),end=hex(e),sha256=hashlib.sha256(x.data[x.off(s):x.off(e)]).hexdigest())for x,s,e in regions],
 bindings=4,order='WORD choices reverse, CHAR links forward; preferred30/38 elseoriginal20/28',
 temporary='V78 clears count;V88 appends exact16N bytes;VD0 subtracts using original document pointcounts',
 split='remainingCount>0 andWORDchoiceCount!=1 requires024A90; unsupported',scope='actual immutable fresh sharedWORD/no cachedCHARprior/no document acceptance',vendorExecution=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS8 static regions/4 virtual bindings/reverseWORD-forwardCHAR/preferredsourcekeys/clear-append-subtract/native024A90guard.')
