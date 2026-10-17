"""Read-only21760 proof, including the overwritten intern OUT-new argument."""
import argparse
import hashlib
import json
from pathlib import Path
import sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64

root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions

p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
t=Binary();e=Binary('MyScriptEngine.dll');f=functions();ef=functions('MyScriptEngine.dll')
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
md=Cs(CS_ARCH_X86,CS_MODE_64)
def dis(b,start,end):
    return [(i.mnemonic,i.op_str)for i in md.disasm(b.data[b.off(start):b.off(end)],start)]
def pin(start,end,expected):assert dis(t,start,end)==expected,(hex(start),dis(t,start,end))
assert e.qwords(0x18023cfc8,3)==(0x180031390,0x18023cf80,0x1800315e0)
assert '0x3e' in f['18001c010'] and '0x118' in f['18001c010']
assert 'FUN_180030e60(param_1,uVar1,&local_18,uVar2,param_5)' in ef['1800315e0']
assert '*param_5 = 0' in ef['180030e60'] and '*param_5 = 1' in ef['180030e60']
#The fifth21760 stack slot is overwritten by the string service. It is NOT
#force-append input policy, despite the decompiler's inferred parameter name.
pin(0x1800218f4,0x180021910,[('lea','rax, [rsp + 0xb0]'),
    ('mov','qword ptr [rsp + 0x20], rax'),('call','r10'),('mov','r8, rax'),
    ('test','rax, rax'),('je','0x180021a58')])
assert ('cmp','byte ptr [rsp + 0xb0], 0') in dis(t,0x180021910,0x18002191d)
assert ('mov','byte ptr [rax], 0') in dis(e,0x180030fa0,0x180030fb8)
assert ('mov','byte ptr [rax], 1') in dis(e,0x180031020,0x180031040)
#Old-count snapshot is held inR12; only that fixed prefix is searched.
pin(0x1800217ac,0x1800217b0,[('movsxd','r12, dword ptr [r13 + 0x58]')])
pin(0x180021930,0x180021943,[('cmp','qword ptr [rax], r8'),('je','0x180021943'),
    ('inc','edx'),('inc','rcx'),('add','rax, 0x28'),('cmp','rcx, r12'),('jl','0x180021930')])
#A fresh row is zeroed and count incremented before021000. A failed021000
#skips aggregateFlags; cleanup still writes the current marker mapping.
pin(0x1800219ad,0x1800219bb,[('movups','xmmword ptr [rdx], xmm0'),
    ('movups','xmmword ptr [rdx + 0x10], xmm0'),('mov','qword ptr [rdx + 0x20], rax'),
    ('inc','dword ptr [rbx + 8]')])
pin(0x1800219f4,0x1800219fd,[('call','0x180021000'),('test','al, al'),('je','0x180021a58')])
pin(0x180021a27,0x180021a2e,[('mov','eax, dword ptr [rdi + 8]'),('or','dword ptr [r13 + 0x18], eax')])
pin(0x180021a7c,0x180021a84,[('cmp','r15d, dword ptr [rax]'),('je','0x180021a84'),('mov','dword ptr [rax], r15d')])
#The scalar/intern prefix inside021000 precedes classification.
publication=dis(t,0x180021000,0x1800210b1)
ordered=[('mov','dword ptr [r8 + 0x20], eax'),('xor','dword ptr [r8 + 8], r9d'),
    ('call','r10'),('mov','qword ptr [rsi + 0x10], rax'),('call','0x180019030')]
indices=[publication.index(v)for v in ordered];assert indices==sorted(indices)
reuse=dis(t,0x180021760,0x180021ab0)
direct=[op for mn,op in reuse if mn=='call' and op.startswith('0x')]
assert direct==['0x180021000'] #No fresh213A0 smiley/filter/remove pass.
regions=[]
for b,start,end in [(t,0x180021760,0x180021ab0),(t,0x180021000,0x1800210b1),
    (e,0x1800315e0,0x180031650),(e,0x180030e60,0x180031200)]:
    regions.append(dict(binary=b.name,start=hex(start),end=hex(end),
        sha256=hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()))
report=dict(regions=regions,oldPrefixCount='signed sourcePayload58 capturedR12 atentry',
    internedNewFlag='Engine0315E0 ->030E60 OUT-new overwrites21760 fifthstackslot; no callerpolicy',
    orderedPublicationPins=len(ordered),nativeMarkerCleanup=True,nativeAppendBeforePublication=True,
    scope='021760 reuse kernel over explicitexisting source-node state and actual sameTOP emission; not024F30 intern/join',
    noVendorExecution=True)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS4 PE regions, actualinternOUT-new, fixed-prefixscan, fullrowzero/count-before-publication, partial021000 andmarker cleanup instructions.')
