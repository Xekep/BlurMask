"""Read-only scratch mutators: VB8 inserts before; VE8 detaches, not frees."""
import argparse, hashlib, json, subprocess, sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
dependency=Path(str(a.output)+'.preflight.json')
subprocess.run([sys.executable,str(Path(__file__).with_name('audit_library_dom_patch_preflight.py')),str(dependency)],cwd=root,check=True)
b=Binary('MyScriptEngine.dll')
assert hashlib.sha256(b.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
bindings=[];regions=[];pins=0
for table in [0x180227080,0x1802273a0]:
    for slot,target in [(0xb8,0x1800eadd0),(0xe8,0x1800ea9d0)]:
        assert b.qwords(table+slot,1)==(target,)
        bindings.append(dict(table=hex(table),slot=hex(slot),target=hex(target)))
md=Cs(CS_ARCH_X86,CS_MODE_64)
def region(start,end,sha,expected):
    global pins
    raw=b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest()==sha
    rows=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
    for row in expected:assert row in rows,row
    pins+=len(expected);regions.append(dict(start=hex(start),end=hex(end),sha256=sha))
region(0x1800eadd0,0x1800eaf30,'a0c5f4065e45c63e905560b8e52fd8b6d94a52f35043c4fe239bc26f319ceeaf',[
    (0x1800eade8,'mov','rsi, qword ptr [r8 + 8]'), #old parent
    (0x1800eadf3,'mov','rbx, r9'), #new
    (0x1800eadf6,'mov','r14, r8'), #old child
    (0x1800eaea7,'mov','qword ptr [rbx + 8], rsi'),
    (0x1800eaeab,'mov','rax, qword ptr [r14 + 0x10]'),
    (0x1800eaeaf,'mov','qword ptr [rbx + 0x10], rax'),
    (0x1800eaeb3,'mov','qword ptr [rbx + 0x18], r14'), #new.next=old
    (0x1800eaeb7,'mov','qword ptr [r14 + 0x10], rbx'), #old.prev=new
    (0x1800eaebb,'cmp','qword ptr [rsi + 0x20], r14'),
    (0x1800eaec1,'mov','qword ptr [rsi + 0x20], rbx'),
    (0x1800eaecb,'mov','qword ptr [rax + 0x18], rbx'),
    (0x1800eaf25,'ret','')])
region(0x1800ea9d0,0x1800eaa20,'9f8b76d4ec144f96da8cd5c9dd45ecb4265f43fe0da42acdc86f627e0d494f69',[
    (0x1800ea9ef,'dec','al'),(0x1800ea9f1,'cmp','al, 2'),
    (0x1800eaa01,'test','rcx, rcx'),
    (0x1800eaa06,'mov','r9, r8'),(0x1800eaa09,'mov','r8, rcx'),
    (0x1800eaa0f,'jmp','0x1800eb090')])
region(0x1800eb090,0x1800eb3b0,'cec14eb9aa3dfd4e607a20fe5916d2ea0894da43df37a3ee083874214f8b8ea2',[
    (0x1800eb242,'mov','rax, qword ptr [r15 + 0x18]'),
    (0x1800eb246,'mov','qword ptr [rsi + 0x20], rax'),
    (0x1800eb266,'mov','qword ptr [rcx + 8], rax'),
    (0x1800eb270,'mov','qword ptr [rsi + 0x28], rax'),
    (0x1800eb274,'mov','qword ptr [r15 + 8], rbx'),
    (0x1800eb278,'mov','qword ptr [r15 + 0x10], rbx'),
    (0x1800eb27c,'mov','qword ptr [r15 + 0x18], rbx')])
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_bytes((json.dumps(dict(regions=regions,bindings=bindings,pins=pins,
    dependency=json.loads(dependency.read_bytes()),vendorExecution=False,
    scope='scratch mutators only; insert-before preserves the existing child; no native page acceptance'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} complete mutator regions/{len(bindings)} bindings/{pins} pins +83-pin preflight/original/history proofs; no vendor execution.')
