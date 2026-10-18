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
bind(I,0x1800e5050,0xe0,0x18006be30)
bind(I,0x1800e78a0,0x38,0x18009ee20)
bind(I,0x1800e78a0,0x48,0x18009f380)
bind(I,0x1800e78a0,0x18,0x18009eca0)
bind(I,0x1800e78a0,0x28,0x18009ed40)
bind(E,0x180227000,0,0x1800ebd50)
for slot,target in [(0,0x180029770),(0x78,0x18002aae0),(0x80,0x18002ac70),(0x88,0x18002ae90),(0x128,0x18002bd60)]:bind(E,0x18023c7e0,slot,target)
for slot,target in [(0x90,0x1800797f0),(0x98,0x180079a10),
                    (0xb0,0x180079f90),(0xb8,0x18007a120)]:bind(I,0x1800e5860,slot,target)
for slot,target in [(0xb0,0x18013d1b0),(0xb8,0x18013c990)]:bind(D,0x18020b1c0,slot,target)
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
# Forward history rows are generated by original observers, not incoming C4D.
pin(I,0x1800797f0,0x180079a03,[
 (0x180079810,'cmp','byte ptr [rdx + 0x108], sil'),(0x180079817,'je','0x1800799f0'),
 (0x18007982f,'mov','r8d, 4'),(0x180079843,'mov','qword ptr [rdi + 0x110], rax'),
 (0x180079861,'mov','r8d, 4'),(0x18007986d,'call','0x180077230'),
 (0x1800798f5,'mov','qword ptr [rsi + 0x20], rax'),
 (0x180079929,'mov','qword ptr [rsi + 0x30], rax'),
 (0x18007994f,'mov','dword ptr [rsi + 0x40], eax'),
 (0x180079968,'mov','qword ptr [rdi + 0x130], rsi'),
 (0x1800799d5,'mov','qword ptr [rdi + 0x110], 0'),
 (0x1800799e0,'mov','qword ptr [rdi + 0x118], 0'),(0x180079a02,'ret','')])
# An observer returns void. Its own failed history allocation does NOT abort
# the outer registration. The distinct executor temporary-C22 failure is not
# interchangeable with this event or with C51's own registration result.
pin(I,0x1800653d0,0x18006547c,[
 (0x180065424,'mov','r10, qword ptr [rax + 0x90]'),
 (0x180065450,'call','r10'),(0x180065453,'add','rbx, 0x18'),
 (0x180065457,'sub','rdi, 1'),(0x18006545b,'jne','0x180065420')])
pin(I,0x18006bffe,0x18006c105,[
 (0x18006c020,'call','0x1800653d0'),
 (0x18006c025,'mov','rbx, qword ptr [rbp + 0x80]'),
 (0x18006c062,'mov','r10, qword ptr [rax + 0x38]'),
 (0x18006c0d1,'call','r10'),(0x18006c0d4,'cmp','rax, rbx'),
 (0x18006c100,'call','0x180065480')])
pin(I,0x180065480,0x18006555b,[
 (0x1800654f4,'mov','r10, qword ptr [rax + 0x98]'),
 (0x180065524,'call','r10'),(0x180065527,'mov','rcx, qword ptr [rsp + 0x70]'),
 (0x18006552c,'sub','rbx, 0x18')])
pin(I,0x180079a10,0x180079b6a,[
 (0x180079a1a,'cmp','qword ptr [rdx + 0x110], 0'),
 (0x180079a2e,'cmp','byte ptr [rdx + 0x108], 0'),
 (0x180079a45,'je','0x180079b23'),
 (0x180079a56,'mov','qword ptr [rdx + 0x130], rax'),
 (0x180079a66,'mov','qword ptr [rcx + 0x18], rax'),
 (0x180079a8e,'mov','qword ptr [rcx + 0x10], rax'),
 (0x180079a99,'mov','qword ptr [rax + 0x10], rcx')])
# Actual delete is before observers -> C51 removal -> reverse after observers.
pin(I,0x180069630,0x1800698a7,[
 (0x180069659,'mov','r9, qword ptr [rax + 0x30]'),
 (0x180069666,'jne','0x180069677'),(0x180069668,'mov','al, 1'),
 (0x18006967e,'mov','rdx, qword ptr [r14 + 8]'),
 (0x1800696b8,'test','r8, r8'),(0x1800696bb,'je','0x180069709'),
 (0x180069732,'inc','dword ptr [rbp + 0x68]'),
 (0x180069754,'mov','r10, qword ptr [rax + 0xb0]'),
 (0x18006977a,'add','rbx, 0x18'),
 (0x18006979d,'mov','r10, qword ptr [rax + 0x48]'),(0x1800697a1,'call','r10'),
 (0x1800697d4,'mov','r10, qword ptr [rax + 0xb8]'),
 (0x18006981d,'sub','rbx, 0x18'),(0x180069825,'jns','0x1800697d0'),
 (0x180069827,'sub','dword ptr [rbp + 0x68], 1'),
 (0x180069840,'call','0x180064610')])
pin(I,0x18009f380,0x18009f6f4,[
 (0x18009f3aa,'mov','rax, qword ptr [rsi + 0xd0]'),
 (0x18009f3f0,'xor','r15b, r15b'),
 (0x18009f403,'mov','qword ptr [r14], rbx'),
 (0x18009f427,'mov','qword ptr [r14 + 8], rbx'),
 (0x18009f49f,'call','0x18009b060'),(0x18009f4a6,'je','0x18009f3f0'),
 (0x18009f4ea,'mov','rax, qword ptr [rsi + 0xf8]'),
 (0x18009f551,'call','0x18009b060'),(0x18009f558,'je','0x18009f3f0'),
 (0x18009f5ce,'call','0x18009c380'),
 (0x18009f60f,'call','0x18009b060'),(0x18009f616,'je','0x18009f3f0'),
 (0x18009f62b,'mov','r15b, 1'),(0x18009f66e,'call','0x18009d700'),
 (0x18009f694,'mov','r9, qword ptr [rax + 0x40]'),
 (0x18009f6c7,'dec','dword ptr [rsi + 0x3c]'),
 (0x18009f6d7,'cmp','rbp, qword ptr [rsi + 0x20]'),
 (0x18009f6dd,'mov','byte ptr [rsi + 0x28], 1'),
 (0x18009f6e1,'movzx','eax, r15b')])
delete_instructions=list(md.disasm(bs[I].data[bs[I].off(0x18009f380):bs[I].off(0x18009f6f4)],0x18009f380))
assert not any(x.mnemonic.startswith(('mov','inc','dec','add','sub')) and
              x.op_str.split(',')[0]=='qword ptr [rsi + 0x18]' for x in delete_instructions)
pin(I,0x18009eca0,0x18009ed3e,[
 (0x18009eca4,'cmp','byte ptr [rdx + 0x28], 0'),
 (0x18009ecce,'nop',''),(0x18009ece2,'mov','rcx, qword ptr [r10 + 8]'),
 (0x18009ed20,'mov','qword ptr [r11 + 0x20], rbx'),
 (0x18009ed2b,'mov','byte ptr [r11 + 0x28], 0')])
# Rollback recording108=0 bypasses BOTH original C4D delete observers.
pin(I,0x180079f90,0x18007a089,[
 (0x180079fa5,'cmp','byte ptr [rdx + 0x108], dil'),
 (0x180079fac,'je','0x18007a083'),(0x180079fca,'cmp','qword ptr [rdx + 0x110], rdi')])
pin(I,0x18007a120,0x18007a36f,[
 (0x18007a142,'cmp','qword ptr [rdx + 0x110], r15'),
 (0x18007a149,'je','0x18007a35e'),(0x18007a14f,'cmp','byte ptr [rdx + 0x108], sil'),
 (0x18007a156,'je','0x18007a35e'),(0x18007a164,'je','0x18007a1fc'),
 (0x18007a1d5,'mov','qword ptr [rsi + 0x30], rax'),
 (0x18007a1f7,'mov','dword ptr [rsi + 0x40], eax'),
 (0x18007a266,'mov','qword ptr [rsi + 0x10], rax')])
# CB7 B8 is an active tailcall, not a constructor/no-op shortcut.
pin(D,0x18013d1b0,0x18013d394,[
 (0x18013d1ce,'cmp','qword ptr [rdx + 0x98], 0'),
 (0x18013d205,'call','0x180136050'),(0x18013d20a,'inc','dword ptr [rdi + 0x1e8]'),
 (0x18013d213,'mov','eax, dword ptr [r15 + 0x18]'),
 (0x18013d238,'mov','qword ptr [rsp + 0x40], 2'),
 (0x18013d241,'mov','dword ptr [rsp + 0x48], 0x7f7fffff'),
 (0x18013d249,'mov','dword ptr [rsp + 0x50], 0xff7fffff'),
 (0x18013d261,'call','0x18013c610'),(0x18013d268,'je','0x18013d2bc'),
 (0x18013d2d0,'call','0x1800f7580'),
 (0x18013d371,'mov','qword ptr [rdi + 0x188], rax')])
pin(D,0x18013c990,0x18013c99f,[
 (0x18013c990,'cmp','qword ptr [rdx + 0x98], 0'),
 (0x18013c998,'jne','0x18013c700'),(0x18013c99e,'ret','')])
pin(D,0x18013c610,0x18013c6f1,[
 (0x18013c62d,'mov','eax, dword ptr [rbx + 0x20]'),
 (0x18013c630,'cmp','eax, -1'),(0x18013c635,'add','eax, -0xa'),
 (0x18013c638,'cmp','eax, 2'),(0x18013c63b,'ja','0x18013c64f'),
 (0x18013c64f,'xor','al, al'),(0x18013c660,'ret','')])
pin(D,0x18013c700,0x18013c868,[
 (0x18013c70b,'add','dword ptr [rdx + 0x1e8], -1'),
 (0x18013c71e,'cmp','byte ptr [rdx + 0x1b0], 0'),
 (0x18013c749,'cmp','dword ptr [rdx + 0x1b4], 0'),
 (0x18013c79a,'mov','rbx, qword ptr [rcx + 0x78]'),
 (0x18013c7b9,'call','0x18013b240'),(0x18013c7ca,'call','0x18013acb0'),
 (0x18013c81e,'mov','rbx, qword ptr [r8 + 0x80]'),
 (0x18013c852,'call','0x180136f20')])
pin(D,0x18013b240,0x18013b52e,[
 (0x18013b29c,'mov','byte ptr [r14], 1'),
 (0x18013b308,'mov','eax, dword ptr [rdi + 0x40]'),
 (0x18013b30b,'cmp','eax, 0xc'),(0x18013b310,'cmp','eax, 0xa'),
 (0x18013b315,'cmp','eax, 0xb'),(0x18013b31e,'mov','byte ptr [r14], 0')])
pin(D,0x18013acb0,0x18013b233,[
 (0x18013ad30,'cmp','dword ptr [rdx + 0x28], 0'),
 (0x18013adb9,'call','0x180136110'),
 (0x18013b071,'call','0x18013ac10'),(0x18013b081,'call','0x18013ab50'),
 (0x18013b0ef,'call','0x18013ac10'),(0x18013b166,'call','0x18013a850'),
 (0x18013b188,'call','0x18013ab50'),(0x18013b1db,'call','0x180136050')])
pin(D,0x18013ab50,0x18013acac,[
 (0x18013abb6,'call','0x18013a850'),(0x18013abd8,'call','0x18013ab50'),
 (0x18013ac3e,'mov','r10, qword ptr [rax + 0x1b0]'),
 (0x18013ac6f,'mov','r10, qword ptr [rcx + 0xf0]'),
 (0x18013ac8a,'call','0x18013a850')])
pin(D,0x18013a850,0x18013ab50,[
 (0x18013a86c,'call','0x18005d050'),(0x18013a877,'js','0x18013ab37'),
 (0x18013a895,'sub','rax, r8'),(0x18013a898,'je','0x18013a8c7'),
 (0x18013a8ad,'mov','al, 1'),(0x18013a8c6,'ret',''),
 (0x18013a8c7,'cmp','qword ptr [rbx + 0x20], r8'),
 (0x18013a8cb,'jne','0x18013a8ad')])
a.output.write_bytes((json.dumps(dict(peSha256=shas,regions=regions,bindings=bindings,pins=count,
 nativeBootstrap=json.loads(dep.read_bytes()),vendorExecution=False,
 baseCommit='019d214',deletionNextPreserved=True,
 scope='original field query/A8 history-parent producers, generated C4D rows, deletion observers and nonmatching-tag rollback; no CBB or page acceptance runtime'),indent=2)+'\n').encode())
print(f'PASS {len(regions)} native regions/{len(bindings)} bindings/{count} pins +original bootstrap proof; no vendor execution.')
