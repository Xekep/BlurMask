"""Read-only worker-selected operation4/7 replay and C51 counter evidence."""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
dep=Path(str(a.output)+'.registered.json')
subprocess.run([sys.executable,str(Path(__file__).with_name('audit_registered_text_patch_apply.py')),str(dep)],cwd=root,check=True)
producer_proofs={}
for name in ('audit_fresh_text_source_context.py','audit_document_counter_handoff.py'):
    output=Path(str(a.output)+'.'+name+'.json')
    subprocess.run([sys.executable,str(Path(__file__).with_name(name)),str(output)],cwd=root,check=True)
    producer_proofs[name]=json.loads(output.read_text())
b=Binary('MyScriptInk.dll');assert hashlib.sha256(b.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];bindings=[];pins=0
def bind(table,slot,target):
    assert b.qwords(table+slot,1)==(target,)
    bindings.append(dict(table=hex(table),slot=hex(slot),target=hex(target)))
def pin(start,end,expected,binary=b):
    global pins
    raw=binary.data[binary.off(start):binary.off(end)];xs=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
    for v in expected:assert v in xs,(hex(start),v)
    pins+=len(expected);regions.append(dict(dll=binary.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
bind(0x1800e5050,0xe0,0x18006be30)
bind(0x1800e5050,0xf8,0x18006a100)
bind(0x1800e78a0,0x20,0x18008e870)
bind(0x1800e78a0,0x28,0x18009ed40)
bind(0x1800e78a0,0x38,0x18009ee20)
pin(0x180078380,0x1800783fa,[
 (0x1800783b7,'lea','r8, [rbp - 0x39]'),(0x1800783bb,'mov','r9d, 0x18'),
 (0x1800783c4,'call','rbx'),(0x1800783ee,'call','r8'),(0x1800783f4,'je','0x18007864a')])
pin(0x1800783fa,0x180078535,[
 (0x1800783fa,'mov','rbx, qword ptr [r14 + 0x18]'),(0x18007843b,'mov','r9, rbx'),
 (0x18007844b,'mov','r10, qword ptr [rax + 0xe0]'),(0x180078452,'mov','eax, dword ptr [r14 + 0x40]'),
 (0x18007845e,'mov','qword ptr [rsp + 0x40], r9'),(0x180078463,'mov','r9, r12'),
 (0x18007847a,'call','r10'),(0x18007852e,'cmp','rbx, rax'),(0x180078531,'sete','dil')])
pin(0x1800786f9,0x180078772,[
 (0x180078708,'mov','rdx, qword ptr [rsi + 0x40]'),(0x18007871d,'mov','r8, qword ptr [rax + 0x20]'),
 (0x180078729,'cmp','qword ptr [r14 + 0x20], -1'),(0x180078730,'cmp','qword ptr [r14 + 0x18], -1'),
 (0x18007873d,'mov','r8, qword ptr [r14 + 0x18]'),(0x180078744,'cmp','r8, rax'),
 (0x180078747,'jle','0x18007864d'),(0x180078763,'mov','r10, qword ptr [rax + 0xf8]'),(0x18007876a,'call','r10')])
pin(0x18009f1ba,0x18009f23f,[
 (0x18009f1d1,'call','0x18009bb20'),(0x18009f1dc,'je','0x18009f26a'),
 (0x18009f1e4,'cmp','r12, qword ptr [rbp + 0x20]'),(0x18009f1ea,'mov','qword ptr [rbp + 0x20], r12'),
 (0x18009f1ee,'mov','byte ptr [rbp + 0x28], r14b'),(0x18009f201,'cmp','r15, rax'),
 (0x18009f204,'jl','0x18009f20e'),(0x18009f206,'lea','rax, [r15 + 1]'),
 (0x18009f20a,'mov','qword ptr [rbp + 0x18], rax'),(0x18009f239,'mov','dword ptr [r14 + 0x18], edi')])
pin(0x18006a112,0x18006a17f,[
 (0x18006a12e,'mov','r8, qword ptr [rax + 0x18]'),(0x18006a132,'call','r8'),
 (0x18006a135,'cmp','rax, rsi'),(0x18006a138,'jl','0x18006a13e'),(0x18006a13a,'lea','rsi, [rax + 1]'),
 (0x18006a162,'mov','r8, rsi'),(0x18006a178,'mov','r9, qword ptr [rax + 0x28]'),(0x18006a17c,'call','r9')])
pin(0x18009edce,0x18009ede0,[
 (0x18009edce,'cmp','qword ptr [r11 + 0x20], rdi'),(0x18009edd2,'jl','0x18009edd8'),
 (0x18009edd8,'mov','qword ptr [r11 + 0x18], rdi')])
d=Binary('MyScriptDocument.dll');assert hashlib.sha256(d.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
pin(0x180115c39,0x180115c69,[
 (0x180115c57,'mov','r10, qword ptr [rax + 0x188]'),(0x180115c5e,'call','r10'),
 (0x180115c61,'test','al, al'),(0x180115c63,'je','0x180115d4a')],d)
pin(0x180115d82,0x180115dbb,[
 (0x180115d9b,'mov','r10, qword ptr [rax + 0x218]'),(0x180115dac,'call','r10'),
 (0x180115daf,'test','al, al'),(0x180115db1,'je','0x180115db8'),(0x180115db8,'xor','dil, dil')],d)
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_bytes((json.dumps(dict(regions=regions,bindings=bindings,pins=pins,
 registeredProof=json.loads(dep.read_text()),pendingProducers=producer_proofs,vendorExecution=False,
 scope='library-owned selected registry overlay only; no CBB receiver, DOM apply or page acceptance'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} PE regions/{len(bindings)} bindings/{pins} pins +registered worker-selected lifecycle dependency.')
