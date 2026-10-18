"""Read-only CBB patch preflight/original C4D target/CB7 commit boundary."""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
dep=Path(str(a.output)+'.registered.json')
subprocess.run([sys.executable,str(Path(__file__).with_name('audit_registered_text_patch_apply.py')),str(dep)],cwd=root,check=True)
d=Binary('MyScriptDocument.dll');i=Binary('MyScriptInk.dll');e=Binary('MyScriptEngine.dll')
for b,h in [(d,'17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'),
 (i,'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'),
 (e,'d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c')]:assert hashlib.sha256(b.data).hexdigest()==h
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];bindings=[];pins=0
def bind(b,table,slot,target):
    assert b.qwords(table+slot,1)==(target,)
    bindings.append(dict(dll=b.name,table=hex(table),slot=hex(slot),target=hex(target)))
def pin(b,start,end,expected):
    global pins
    raw=b.data[b.off(start):b.off(end)];xs=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
    for v in expected:assert v in xs,(b.name,hex(start),v)
    pins+=len(expected);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
for slot,target in [(0x78,0x1801105e0),(0x110,0x180115800)]:bind(d,0x180208f90,slot,target)
for slot,target in [(0x10,0x180136980),(0x20,0x18013bce0),(0x28,0x18013c500)]:bind(d,0x18020b070,slot,target)
for slot,target in [(0x180,0x18006ec60),(0x188,0x18006ed30)]:bind(i,0x1800e5050,slot,target)
for slot,target in [(0x20,0x18007e2e0),(0x30,0x180077850),(0x38,0x18007e210)]:bind(i,0x1800e5980,slot,target)
for table in [0x180227080,0x1802273a0]:
    bind(e,table,0x210,0x1800efa70);bind(e,table,0x218,0x1800efb10)
bind(e,0x1802265e0,8,0x1800e2870);bind(e,0x1802265e0,0x10,0x1800e2d70)
bind(i,0x1800e5ac0,0x120,0x180081d90)
pin(i,0x18006ec60,0x18006ed2a,[
 (0x18006ec75,'mov','r14, qword ptr [rdx + 0x58]'),(0x18006ec82,'test','r8, r8'),
 (0x18006ec85,'jne','0x18006ec9c'),(0x18006ec8c,'mov','byte ptr [r9], 1'),
 (0x18006ecd7,'mov','rbx, qword ptr [rbx + 0x30]'),(0x18006ecdb,'mov','rdx, r14'),
 (0x18006ece1,'mov','r8, qword ptr [rax + 0x20]'),(0x18006ece5,'call','r8'),
 (0x18006ece8,'mov','rdx, rax'),(0x18006ecfd,'mov','r8, rsi'),(0x18006ed0d,'call','rbx')])
pin(i,0x18007e2e0,0x18007e2e8,[(0x18007e2e0,'mov','rax, qword ptr [rdx + 0xf8]'),(0x18007e2e7,'ret','')])
pin(d,0x18011584e,0x1801158b0,[
 (0x18011585e,'mov','rbx, r8'),(0x18011588b,'mov','rcx, qword ptr [rax + 0x38]'),
 (0x18011588f,'mov','r13, qword ptr [rax + 0x30]'),(0x1801158a9,'call','0x1801101a0')])
pin(d,0x18011591a,0x180115945,[
 (0x18011591e,'xor','r9d, r9d'),(0x180115921,'xor','r8d, r8d'),
 (0x180115924,'mov','byte ptr [rsp + 0x20], 1'),(0x18011592c,'mov','rdx, qword ptr [rax + 0xb8]'),
 (0x180115936,'mov','r10, qword ptr [rax + 0x20]'),(0x18011593a,'call','r10')])
pin(d,0x18013bce0,0x18013bd6e,[
 (0x18013bd13,'cmp','byte ptr [rsp + 0x50], al'),(0x18013bd17,'jne','0x18013bd20'),
 (0x18013bd19,'mov','byte ptr [rdx + 0x1b0], r10b'),(0x18013bd22,'mov','dword ptr [rdx + 0x1b4], eax'),
 (0x18013bd32,'jae','0x18013bd53'),(0x18013bd43,'call','0x18013bba0'),(0x18013bd5f,'call','0x18013b9c0')])
pin(d,0x18013b9c0,0x18013b9f2,[
 (0x18013b9db,'test','r9d, r9d'),(0x18013b9de,'jne','0x18013b9f2'),(0x18013b9e0,'mov','al, 1')])
pin(d,0x18010fce3,0x18010fd58,[
 (0x18010fcf2,'mov','byte ptr [rsp + 0x20], 1'),(0x18010fcf7,'mov','r8, qword ptr [r8 + 0xb8]'),
 (0x18010fcfe,'mov','r10, qword ptr [rax + 0x18]'),(0x18010fd02,'call','r10'),
 (0x18010fd4f,'mov','qword ptr [rcx + 0x38], rsi')])
pin(d,0x180110144,0x180110198,[
 (0x180110148,'xor','r9d, r9d'),(0x180110154,'mov','byte ptr [rsp + 0x20], 1'),
 (0x180110159,'mov','r8, qword ptr [r8 + 0xb8]'),(0x180110160,'mov','r10, qword ptr [rax + 0x10]'),
 (0x180110164,'call','r10'),(0x180110178,'mov','r8, qword ptr [r8 + 0x38]'),
 (0x18011017c,'call','0x180116ff0'),(0x18011018f,'mov','qword ptr [rax + 0x30], r14')])
pin(d,0x1801159a0,0x180115a13,[
 (0x1801159b1,'mov','rax, qword ptr [rdx + 0x38]'),(0x1801159b9,'mov','rcx, qword ptr [rax + r13 + 0x10]'),
 (0x1801159be,'mov','r14, qword ptr [rax + r13 + 0x20]'),(0x1801159d1,'cmp','qword ptr [rax + r13], rbx'),
 (0x1801159d5,'je','0x180115b02'),(0x1801159db,'jg','0x180115e01'),
 (0x1801159f4,'mov','byte ptr [rsp + 0x42], 1'),(0x180115a03,'add','r13, 0x28')])
pin(d,0x180115b02,0x180115b8f,[
 (0x180115b31,'mov','r10, qword ptr [rax + 0x180]'),(0x180115b38,'call','r10'),
 (0x180115b3d,'je','0x180115dea'),(0x180115b5a,'mov','r10, qword ptr [rax + 0x210]'),
 (0x180115b6b,'call','r10'),(0x180115b70,'je','0x180115dea'),
 (0x180115b76,'cmp','byte ptr [rsp + 0x43], 0'),(0x180115b7e,'je','0x180115ded'),
 (0x180115b84,'cmp','byte ptr [rsp + 0x44], 0'),(0x180115b89,'je','0x180115ded')])
pin(d,0x180115e3f,0x180115ece,[
 (0x180115e57,'mov','r9, qword ptr [rax + 0x28]'),(0x180115e5b,'call','r9'),
 (0x180115e69,'cmove','ebx, eax'),(0x180115e73,'je','0x180115eab'),
 (0x180115e78,'jne','0x180115eab'),(0x180115e9c,'mov','r10, qword ptr [rax + 0x80]'),
 (0x180115ea3,'call','r10'),(0x180115ec4,'mov','r8, qword ptr [rax + 0x88]'),(0x180115ecb,'call','r8')])
pin(d,0x18013c500,0x18013c597,[
 (0x18013c521,'call','0x18013b530'),(0x18013c526,'sub','dword ptr [rbx + 0x1b4], 1'),
 (0x18013c52d,'jne','0x18013c58a'),(0x18013c53c,'call','0x18013bd90'),
 (0x18013c54b,'call','0x18013bfd0'),(0x18013c558,'mov','dword ptr [rbx + 0x1c0], ecx'),
 (0x18013c55e,'mov','dword ptr [rbx + 0x1d0], ecx')])
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_bytes((json.dumps(dict(regions=regions,bindings=bindings,pins=pins,registeredProof=json.loads(dep.read_bytes()),
 vendorExecution=False,scope='original receiver preflight and CB7 transaction boundary; no selected-clone/page coercion or acceptance runtime'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} focused regions/{len(bindings)} bindings/{pins} pins +registered caller/clone/F8 dependency; no vendor execution.')
