"""Read-only registered C8F -> C8E patch callback and concrete CBB boundary.

No native DLL execution. This does not bind a library BCC root to the page,
store/history or revision owners required by these native callbacks.
"""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
# Existing closed ordinary C4A/C51/C22/no-external-observer constructor and
# full-copy scope. It must accompany the newly pinned caller history flag.
dependency=Path(str(a.output)+'.input-context.json')
subprocess.run([sys.executable,str(Path(__file__).with_name('audit_textfield_input_context.py')),str(dependency)],cwd=root,check=True)
d=Binary('MyScriptDocument.dll');ink=Binary('MyScriptInk.dll')
assert hashlib.sha256(d.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
assert hashlib.sha256(ink.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];bindings=[];count=0
def bind(b,table,slot,target):
    assert b.qwords(table+slot,1)==(target,)
    bindings.append(dict(dll=b.name,table=hex(table),slot=hex(slot),target=hex(target)))
def pin(b,start,end,expected):
    global count
    raw=b.data[b.off(start):b.off(end)];instructions=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
    for v in expected:assert v in instructions,(b.name,hex(start),v)
    count+=len(expected);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
bind(d,0x1802095b0,0x10,0x18011baa0)
bind(d,0x1802095b0,0x28,0x18011b680)
bind(d,0x1801b6330,0x70,0x18000f110)
bind(d,0x180208f90,0x78,0x1801105e0)
bind(d,0x180208f90,0x110,0x180115800)
bind(ink,0x1800e5050,0x188,0x18006ed30)
bind(ink,0x1800e5050,0x170,0x18006e120)
bind(ink,0x1800e4ed8,0x10,0x180063ed0)
bind(ink,0x1800e5860,0,0x1800788d0)
pin(d,0x18011b680,0x18011b6b7,[
    (0x18011b694,'mov','r8d, 1'),(0x18011b6a4,'call','r9'),
    (0x18011b6aa,'mov','qword ptr [rbx + 0x18], rax'),(0x18011b6ae,'setne','al')])
pin(d,0x18011baa0,0x18011bb17,[
    (0x18011baaa,'test','byte ptr [rdx + 0x20], 2'),(0x18011bab6,'mov','eax, 1'),
    (0x18011bac6,'mov','rdx, qword ptr [rdx + 0x18]'),(0x18011bade,'mov','r9, qword ptr [rax + 0x18]'),
    (0x18011bae2,'call','r9'),(0x18011baf3,'mov','r9, qword ptr [rcx + 0x70]'),(0x18011bb04,'jmp','r9')])
pin(d,0x1800128ff,0x180012937,[
    (0x180012906,'mov','rdx, qword ptr [r13 + 0x40]'),(0x18001291f,'mov','qword ptr [r14 + 0x28], rax'),
    (0x180012928,'mov','rax, qword ptr [r13 + 0x48]'),(0x180012931,'mov','qword ptr [r14 + 0x30], rax')])
pin(d,0x180012df6,0x180012e26,[
    (0x180012e04,'mov','r9, qword ptr [rax + 0x28]'),(0x180012e08,'call','r9'),
    (0x180012e0d,'je','0x180012e26'),(0x180012e0f,'mov','r8, qword ptr [r13 + 0x38]')])
pin(d,0x18000f2e2,0x18000f421,[
    (0x18000f2e2,'mov','rdx, qword ptr [r12 + 0x10]'),(0x18000f2f2,'mov','r10, qword ptr [rax + 0x78]'),
    (0x18000f2f6,'call','r10'),(0x18000f33b,'mov','r10, qword ptr [rax + 0x1e0]'),
    (0x18000f3ff,'call','0x18000c180'),(0x18000f410,'mov','rcx, qword ptr [rax + 0x48]'),
    (0x18000f414,'cmp','qword ptr [r12 + 0x30], rcx'),(0x18000f419,'jne','0x18000f6b7')])
pin(d,0x18000f491,0x18000f56b,[
    (0x18000f4b6,'cmp','qword ptr [r8 + 0x18], 0'),(0x18000f4bd,'cmp','qword ptr [r8 + 0x20], 0'),
    (0x18000f4c7,'mov','r9, r12'),(0x18000f4ca,'mov','r8, qword ptr [r14 + 0x98]'),
    (0x18000f4d4,'mov','r10, qword ptr [rax + 0x110]'),(0x18000f4ef,'call','r10'),
    (0x18000f559,'mov','r9, qword ptr [rax + 0x20]'),(0x18000f55d,'call','r9')])
pin(d,0x1801105e0,0x180110651,[
    (0x1801105f8,'call','0x1801101a0'),(0x180110605,'mov','rcx, qword ptr [rax + 0x38]'),
    (0x18011060e,'cmp','qword ptr [rax + 0x30], 0'),(0x180110628,'mov','rdx, qword ptr [rcx + 0x30]')])
pin(d,0x1801158b0,0x180115a13,[
    (0x1801158ba,'cmp','qword ptr [rax + 0x38], 0'),(0x1801158c5,'cmp','qword ptr [rax + 0x30], 0'),
    (0x18011592c,'mov','rdx, qword ptr [rax + 0xb8]'),(0x180115936,'mov','r10, qword ptr [rax + 0x20]'),
    (0x18011593a,'call','r10'),(0x18011595e,'mov','r8, qword ptr [rax + 0x78]'),
    (0x1801159b1,'mov','rax, qword ptr [rdx + 0x38]'),(0x1801159be,'mov','r14, qword ptr [rax + r13 + 0x20]')])
pin(d,0x180115b02,0x180115b82,[
    (0x180115b31,'mov','r10, qword ptr [rax + 0x180]'),(0x180115b38,'call','r10')])
pin(d,0x180115c39,0x180115c68,[
    (0x180115c39,'mov','byte ptr [rsp + 0x20], 0'),
    (0x180115c57,'mov','r10, qword ptr [rax + 0x188]'),(0x180115c5e,'call','r10')])
pin(d,0x180115d82,0x180115db8,[
    (0x180115d9b,'mov','r10, qword ptr [rax + 0x218]'),(0x180115dac,'call','r10')])
pin(ink,0x180063ec0,0x180063ecb,[
    (0x180063ec0,'mov','r9b, 1'),(0x180063ec3,'xor','r8d, r8d'),(0x180063ec6,'jmp','0x1800638a0')])
# The original is history-enabled, but the registered worker's TWO selected
# copies pass LAST argument1. That caller-produced policy suppresses history.
pin(d,0x18000d68f,0x18000d705,[
    (0x18000d68f,'mov','byte ptr [rsp + 0x30], 1'),(0x18000d6ac,'mov','byte ptr [rsp + 0x28], 1'),
    (0x18000d6b7,'mov','r10, qword ptr [rax + 0x170]'),(0x18000d6c8,'call','r10'),
    (0x18000d6f6,'mov','qword ptr [rbx + 0x60], r14'),(0x18000d700,'mov','qword ptr [rbx + 0x78], r14')])
pin(ink,0x18006e13e,0x18006e1b2,[
    (0x18006e153,'cmp','byte ptr [rsp + 0xa0], sil'),(0x18006e15b,'jne','0x18006e167'),
    (0x18006e15d,'cmp','qword ptr [rdx + 0x58], rsi'),(0x18006e167,'xor','al, al'),
    (0x18006e169,'mov','byte ptr [rsp + 0x48], al'),(0x18006e19a,'lea','r8, [rsp + 0x40]'),
    (0x18006e19f,'mov','r9d, 0x10'),(0x18006e1a8,'call','rbx')])
pin(ink,0x180063ed0,0x180063f00,[
    (0x180063ee3,'cmp','r9, 0x10'),(0x180063ee9,'movzx','r9d, byte ptr [r8 + 8]'),
    (0x180063eee,'mov','r8, qword ptr [r8]'),(0x180063efb,'jmp','0x1800638a0')])
pin(ink,0x1800638a0,0x180063b50,[
    (0x1800638bb,'movzx','r15d, r9b'),(0x1800638fc,'xor','r12d, r12d'),
    (0x180063960,'mov','qword ptr [rsi + 0x58], r12'),
    (0x180063a44,'test','r15b, r15b'),(0x180063a47,'je','0x180063ae2'),
    (0x180063a51,'mov','edx, 0xc4d'),(0x180063a92,'call','0x180064da0')])
# The ORIGINAL's own default history target is not guessed either:
# C43 attach receives the exact C20 target from0648A0 and writesF8.
pin(ink,0x180064a90,0x180064af1,[
    (0x180064a9d,'mov','rdx, qword ptr [r14 + 0x58]'),(0x180064ad2,'mov','qword ptr [r14 + 0x58], rax'),
    (0x180064ada,'mov','r8, r14'),(0x180064aed,'call','r9')])
pin(ink,0x1800788d0,0x18007893c,[
    (0x1800788e2,'mov','rsi, r8'),(0x180078901,'mov','qword ptr [rbx + 0xf8], rsi'),
    (0x18007890f,'mov','rdx, qword ptr [rsi + 0xd8]'),(0x18007892a,'mov','qword ptr [rbx + 0x100], rax')])
rows=[(x.mnemonic,x.op_str)for x in md.disasm(ink.data[ink.off(0x18006ed30):ink.off(0x18006ede4)],0x18006ed30)]
for v in [('mov','rsi, qword ptr [rdx + 0x58]'),('mov','r10, qword ptr [rax + 0x40]'),('mov','r10, qword ptr [rax + 0x38]')]:assert v in rows
regions.append(dict(dll=ink.name,start='0x18006ed30',end='0x18006ede4',sha256=hashlib.sha256(ink.data[ink.off(0x18006ed30):ink.off(0x18006ede4)]).hexdigest()));count+=3
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_bytes((json.dumps(dict(regions=regions,bindings=bindings,pins=count,inputOwnerProof=json.loads(dependency.read_text()),vendorExecution=False,
    scope='registered C8F patch callback; ORIGINAL history-enabled, worker-selected clone history-disabled; no CBB/page acceptance runtime'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} PE regions/{len(bindings)} bindings/{count} pins; registered C8F->C8E apply callback, no guessed page/history owner.')
