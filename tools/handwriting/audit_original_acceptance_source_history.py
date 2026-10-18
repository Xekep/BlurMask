"""Read-only original field target, A8 history parents and source rollback.

No library selected replay or kind4 target is substituted for a CBB receiver.
"""
import argparse,hashlib,json,struct,subprocess,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
a.output.parent.mkdir(parents=True,exist_ok=True)
dep=Path(str(a.output)+'.bootstrap.json')
subprocess.run([sys.executable,str(Path(__file__).with_name('audit_original_page_source_bootstrap.py')),str(dep)],cwd=root,check=True)
shas={
 'MyScriptDocument.dll':'17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f',
 'MyScriptInk.dll':'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a',
 'MyScriptEngine.dll':'d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'}
bs={n:Binary(n)for n in shas}
for n,b in bs.items():assert hashlib.sha256(b.data).hexdigest()==shas[n],n
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];bindings=[];count=0
def pin(n,s,e,expected):
 global count
 b=bs[n];raw=b.data[b.off(s):b.off(e)];xs=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(raw,s)]
 for v in expected:assert v in xs,(n,hex(s),v)
 count+=len(expected);regions.append(dict(dll=n,start=hex(s),end=hex(e),sha256=hashlib.sha256(raw).hexdigest()))
def bind(n,t,slot,target):
 assert bs[n].qwords(t+slot,1)==(target,)
 bindings.append(dict(dll=n,table=hex(t),slot=hex(slot),target=hex(target)))
D,I,E='MyScriptDocument.dll','MyScriptInk.dll','MyScriptEngine.dll'
bind(I,0x1800e4f38,0,0x18006c990)
bind(I,0x1800e59f0,0x18,0x180077570)
bind(I,0x1800e5980,0x38,0x18007e210)
bind(I,0x1800e5050,0xe8,0x180069630)
bind(I,0x1800e78a0,0x48,0x18009f380)
bind(I,0x1800e78a0,0x18,0x18009eca0)
bind(I,0x1800e78a0,0x28,0x18009ed40)
bind(E,0x180227000,0,0x1800ebd50)
for slot,target in [(0,0x180029770),(0x78,0x18002aae0),(0x80,0x18002ac70),(0x88,0x18002ae90),(0x128,0x18002bd60)]:bind(E,0x18023c7e0,slot,target)
pin(I,0x1800707c0,0x180070800,[
 (0x1800707c0,'sub','edx, 0xa8'),(0x1800707c6,'je','0x1800707f8'),
 (0x1800707f8,'lea','rax, [rip + 0x74739]')])
assert 0x1800707ff+0x74739==0x1800e4f38
pin(I,0x18006c990,0x18006c995,[(0x18006c990,'mov','rax, qword ptr [rdx + 0x58]'),(0x18006c994,'ret','')])
pin(E,0x1800f04e0,0x1800f0513,[
 (0x1800f04e0,'sub','edx, 0xa8'),(0x1800f04e6,'je','0x1800f050b'),
 (0x1800f050b,'lea','rax, [rip + 0x136aee]')])
assert 0x1800f0512+0x136aee==0x180227000
pin(E,0x1800ebd50,0x1800ebe3e,[
 (0x1800ebd6e,'mov','edx, 0xbe2'),(0x1800ebdac,'cmp','dword ptr [rcx + 0xd8], ebx'),
 (0x1800ebdc0,'mov','rdx, qword ptr [rcx + 0xd0]'),(0x1800ebdd7,'shl','rdi, 5'),
 (0x1800ebde6,'call','r9'),(0x1800ebdef,'jne','0x1800ebdfd'),
 (0x1800ebe04,'mov','r14, qword ptr [rdi + rax]'),(0x1800ebe2c,'mov','rax, r14')])
pin(D,0x18010fb80,0x18010fcdb,[
 (0x18010fbe1,'mov','r8d, 0xa8'),(0x18010fc08,'call','r8'),
 (0x18010fc0b,'mov','qword ptr [rbx + 0x80], rax'),
 (0x18010fc4c,'mov','rax, qword ptr [rdx + 0x60]'),
 (0x18010fc53,'je','0x18010fcc0'),(0x18010fc55,'cmp','rax, qword ptr [r8 + 0x88]'),
 (0x18010fc78,'mov','edx, 0x1000000')])
pin(D,0x18010fd60,0x180110198,[
 (0x18011007e,'mov','qword ptr [rbx + 0x78], rax'),
 (0x1801100b6,'mov','rax, qword ptr [rdx + 0x60]'),
 (0x1801100bf,'cmp','rax, qword ptr [r8 + 0x88]'),
 (0x1801100e2,'mov','edx, 0x1020003'),
 (0x18011012c,'mov','r8, qword ptr [r8 + 0x88]'),(0x18011013d,'call','r9')])
pin(D,0x180115800,0x180115945,[
 (0x1801158e6,'mov','rcx, qword ptr [rax + 0x48]'),
 (0x1801158ef,'mov','qword ptr [rbp - 0x28], rax'),
 (0x1801158fe,'mov','r10, qword ptr [rax + 0x1e0]'),
 (0x180115905,'mov','rax, qword ptr [rdi + 0x28]'),
 (0x180115909,'mov','qword ptr [rsp + 0x20], rax'),
 (0x18011590e,'call','r10'),(0x180115911,'cmp','eax, 1'),
 (0x180115914,'jne','0x180115e0b')])
pin(D,0x180115b02,0x180115b8f,[
 (0x180115b4f,'mov','r8, qword ptr [rsp + 0x60]'),
 (0x180115b5a,'mov','r10, qword ptr [rax + 0x210]'),(0x180115b6b,'call','r10'),
 (0x180115b76,'cmp','byte ptr [rsp + 0x43], 0'),(0x180115b84,'cmp','byte ptr [rsp + 0x44], 0')])
pin(D,0x180115d85,0x180115dbc,[
 (0x180115d90,'mov','r8, qword ptr [rsp + 0x60]'),
 (0x180115d9b,'mov','r10, qword ptr [rax + 0x218]'),(0x180115dac,'call','r10')])
pin(I,0x18007e210,0x18007e2d9,[
 (0x18007e241,'mov','r10, qword ptr [rax + 0x78]'),(0x18007e245,'call','r10'),
 (0x18007e24a,'je','0x18007e291'),(0x18007e24c,'mov','rdx, qword ptr [rsi + 0xf8]'),
 (0x18007e260,'lea','r8, [r14 + 8]'),(0x18007e274,'mov','byte ptr [rsi + 0x108], dil'),
 (0x18007e27b,'call','0x180078090'),(0x18007e280,'mov','byte ptr [rsi + 0x108], dil'),
 (0x18007e298,'je','0x18007e2bb'),(0x18007e2ae,'mov','r10, qword ptr [rcx + 0x80]'),
 (0x18007e2b8,'call','r10')])
pin(I,0x180077570,0x180077824,[
 (0x180077595,'cmp','qword ptr [rdx + 0xf8], 0'),
 (0x1800775a9,'mov','byte ptr [rdx + 0x108], 0'),
 (0x1800775b0,'mov','rbx, qword ptr [r8 + 0x10]'),
 (0x1800775dc,'mov','ecx, dword ptr [rbp + rax*4 + 0x77824]'),
 (0x1800776c0,'mov','r9, qword ptr [rax + 0xe8]'),
 (0x1800777a2,'mov','r8, qword ptr [rax + 0x18]'),
 (0x1800777a9,'mov','r8, qword ptr [rbx + 0x20]'),
 (0x1800777ba,'cmp','rax, r8'),(0x1800777bd,'jge','0x180077806'),
 (0x1800777d9,'mov','r9, qword ptr [rax + 0x28]'),
 (0x180077806,'mov','rbx, qword ptr [rbx + 0x10]'),
 (0x180077813,'mov','byte ptr [rdi + 0x108], 1')])
for opcode,target in [(4,0x776af),(7,0x77788)]:
 assert struct.unpack_from('<I',bs[I].data,bs[I].off(0x180077824)+opcode*4)[0]==target
pin(E,0x180003200,0x18000327d,[
 (0x180003210,'mov','qword ptr [rcx + 0x28], rax'),
 (0x180003240,'mov','rax, qword ptr [rip + 0x239769]'),(0x180003247,'mov','qword ptr [rcx], rax')])
assert 0x180003247+0x239769==0x18023c9b0
assert bs[E].qwords(0x18023c9b0,1)==(0x18023c7e0,)
a.output.write_bytes((json.dumps(dict(peSha256=shas,regions=regions,bindings=bindings,pins=count,
 nativeBootstrap=json.loads(dep.read_bytes()),vendorExecution=False,
 scope='original field query/A8 history-parent producers and C4D rollback; no CBB or page acceptance runtime'),indent=2)+'\n').encode())
print(f'PASS {len(regions)} native regions/{len(bindings)} bindings/{count} pins +original bootstrap proof; no vendor execution.')
