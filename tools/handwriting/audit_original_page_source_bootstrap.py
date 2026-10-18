"""Read-only original page/CB7/BE2 owners and completed field revision producer."""
import argparse, hashlib, json, subprocess, sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
dep=Path(str(a.output)+'.history.json')
subprocess.run([sys.executable,str(Path(__file__).with_name('audit_registered_text_patch_history.py')),str(dep)],cwd=root,check=True)
d,e=Binary('MyScriptDocument.dll'),Binary('MyScriptEngine.dll')
for binary,sha in [(d,'17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'),
                   (e,'d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c')]:assert hashlib.sha256(binary.data).hexdigest()==sha
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];bindings=[];literals=[];pins=0
def bind(b,table,slot,target):
    assert b.qwords(table+slot,1)==(target,)
    bindings.append(dict(dll=b.name,table=hex(table),slot=hex(slot),target=hex(target)))
def region(b,start,end,sha,expected):
    global pins
    raw=b.data[b.off(start):b.off(end)];assert hashlib.sha256(raw).hexdigest()==sha
    rows=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
    for row in expected:assert row in rows,(b.name,row)
    pins+=len(expected);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=sha))
def literal(va,text):
    raw=d.data[d.off(va):].split(b'\0',1)[0];assert raw==text.encode('utf-8'),(hex(va),raw)
    literals.append(dict(address=hex(va),utf8=text))
for b,t,s,x in [(d,0x180209e00,0,0x1801243c0),(d,0x180209e00,8,0x180126cb0),
 (d,0x180209248,0x10,0x1801114c0),(d,0x18020b170,0x10,0x1801363a0),
 (d,0x18020b0f0,0,0x18013d3a0),(d,0x18020b1c0,0,0x18013c870),
 (e,0x180227010,0x10,0x1800ec260),(e,0x1802264b0,0,0x1800e0e20)]:bind(b,t,s,x)
for va,text in [(0x18020ca38,'page'),(0x18020c7c4,'id'),(0x18020d2cf,'')]:literal(va,text)
region(d,0x1801101a0,0x1801105e0,'65ec30aa8d3e0eda79205fca6644bdd5658d5ccf76d240fd27044b12c1bc836c',[
 (0x180110349,'call','0x18010fb80'),(0x180110364,'call','0x180110ae0'),
 (0x180110555,'mov','edx, 0xcb5'),(0x180110561,'call','r8'),
 (0x180110564,'mov','dl, 1'),(0x180110569,'mov','r8, qword ptr [rax]'),
 (0x18011056c,'call','r8'),(0x1801105a0,'call','r10'),(0x1801105c3,'call','0x18010fd60')])
region(d,0x1801243c0,0x180124590,'885820af2adcd26173be4885b7f5b8e208eb1c3d93c7b44a0ec336dd0d3ec829',[
 (0x1801243e1,'mov','byte ptr [rsp + 0x40], dl'),
 (0x1801243e9,'mov','qword ptr [rsp + 0x48], r14'),
 (0x180124400,'mov','edx, 0xbcc'),(0x18012440a,'lea','r8, [rsp + 0x40]'),
 (0x18012440f,'mov','r9d, 0x10'),(0x180124418,'call','rbx'),
 (0x180124485,'mov','dword ptr [rsp + 0x30], 5'),
 (0x1801244a0,'mov','r10, qword ptr [rax + 0x118]'),
 (0x1801244a7,'call','r10'),(0x18012450c,'call','r10'),
 (0x180124525,'call','r10'),(0x18012454b,'call','rbx')])
region(e,0x1800ec260,0x1800ec2b0,'c7ba58a519b841ed7e3ec85e29e95e38d9177d5a9df297412d00af0847073bfd',[
 (0x1800ec264,'cmp','r9, 0x10'),(0x1800ec29a,'mov','r9, qword ptr [r8 + 8]'),
 (0x1800ec29e,'movzx','r8d, byte ptr [r8]'),(0x1800ec2a6,'jmp','0x1800f1e40')])
region(e,0x1800f1e40,0x1800f21a0,'331e756da6a96c2a2e53766c4a7b424e5ff02befe6009713b4adb334a0f6ef18',[
 (0x1800f1e60,'movzx','ebp, r8b'),(0x1800f20fb,'test','bpl, bpl'),
 (0x1800f20fe,'je','0x1800f217a'),(0x1800f2104,'mov','edx, 0xbe2'),
 (0x1800f2127,'call','rbx'),(0x1800f2131,'xor','r9d, r9d'),
 (0x1800f2134,'mov','byte ptr [rsp + 0x20], 1'),
 (0x1800f2139,'mov','r8, rax'),(0x1800f213c,'mov','rdx, r14'),
 (0x1800f2142,'call','0x1800ea0d0')])
region(e,0x1800e0e20,0x1800e0e60,'3d2d885d84a5c3cdfc184eed624da6eb72f7fb6ca42ecb13ab94c1e43a4aaa1d',[
 (0x1800e0e30,'mov','rdx, qword ptr [rdx + 0xf8]'),
 (0x1800e0e46,'call','r9'),(0x1800e0e49,'mov','qword ptr [rbx + 0xf8], rdi')])
region(d,0x1801114c0,0x1801125f0,'da2226b8a7d3ac6d0661c5b9289c908f6be63995e96125cdfcb157bc0fc275f9',[
 (0x180111a11,'mov','edx, 0xcb7'),(0x180111a59,'mov','qword ptr [rcx + 0xb8], rax'),
 (0x180111a78,'mov','r8, qword ptr [rdx + 0xb8]'),
 (0x180111a7f,'mov','rdx, qword ptr [rdx + 0x88]'),
 (0x180111a89,'mov','r9, qword ptr [rax + 0x128]')])
region(d,0x1801363a0,0x180136610,'1a31819926eb4d8bf653084e4aea14ef9edd8cfbf95f15d5e43ce72461a3834c',[
 (0x1801363ee,'mov','qword ptr [rbx + 0x90], rax')])
region(d,0x18013fdf0,0x18013fe20,'98aceb38e696658763694396951a9011a7528ff2faa18205909578993583cfcf',[
 (0x18013fdf0,'sub','edx, 0x3f'),(0x18013fdf3,'je','0x18013fe15'),
 (0x18013fdf5,'sub','edx, 0xb9b'),(0x18013fdfb,'je','0x18013fe0d'),
 (0x18013fdfd,'cmp','edx, 0x69'),(0x18013fe00,'je','0x18013fe05'),
 (0x18013fe05,'lea','rax, [rip + 0xcb3b4]'),
 (0x18013fe0d,'lea','rax, [rip + 0xcb2dc]'),
 (0x18013fe15,'lea','rax, [rip + 0xcb214]')])
region(d,0x18000d0c0,0x18000d540,'c401a3633b1e996b0bb607a971bf2d0081613fbf5169c6125e26a3e4c12651e2',[
 (0x18000d1e9,'call','rdx'),(0x18000d1eb,'mov','qword ptr [rsi + 0x48], rax'),
 (0x18000d376,'mov','qword ptr [rsi + 0x40], rax'),
 (0x18000d3ec,'mov','rdx, qword ptr [rsi + 0x40]'),
 (0x18000d40c,'mov','qword ptr [rcx + 0x28], rax'),
 (0x18000d425,'mov','rax, qword ptr [rsi + 0x48]'),
 (0x18000d429,'mov','qword ptr [rcx + 0x30], rax'),
 (0x18000d4b2,'mov','rax, qword ptr [rsi + 0x48]'),
 (0x18000d4b6,'mov','qword ptr [rcx + 0x30], rax'),
 (0x18000d4e0,'test','dl, dl'),(0x18000d4e2,'je','0x18000d513'),
 (0x18000d4e4,'mov','qword ptr [rsi + 0x98], 0xffffffffffffffff'),
 (0x18000d4ef,'mov','qword ptr [rsi + 0xa0], 0xffffffffffffffff'),
 (0x18000d4fa,'mov','qword ptr [rsi + 0xa8], 0xffffffffffffffff'),
 (0x18000d50c,'mov','byte ptr [rsi + 0xd1], 1')])
region(d,0x18000bb20,0x18000c180,'40a7dcd46698f9af5c9b7cee847e60657c4f6cff46f4cdef16f90114bb66d942',[
 (0x18000be21,'cmp','byte ptr [rsp + 0xd8], 0'),
 (0x18000be2e,'je','0x18000c06a'),(0x18000be34,'add','r13, 0xa8'),
 (0x18000be3f,'mov','r8, qword ptr [r13]'),(0x18000be56,'lea','r9, [r8 + 1]'),
 (0x18000be63,'call','r10'),(0x18000be6c,'inc','qword ptr [r13]'),
 (0x18000c074,'mov','r8, qword ptr [r13 + 0xa8]'),
 (0x18000c083,'mov','r9, r8'),(0x18000c094,'call','r10')])
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_bytes((json.dumps(dict(regions=regions,bindings=bindings,literals=literals,pins=pins,
    dependency=json.loads(dep.read_bytes()),vendorExecution=False,
    scope='original page/history factory and completed source metadata producer; no SDK/page acceptance runtime'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} PE regions/{len(bindings)} bindings/{pins} pins/{len(literals)} literals + original/history dependencies; no vendor execution.')
