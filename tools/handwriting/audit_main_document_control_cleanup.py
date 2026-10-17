"""Read-only023260 no-control pass and following live dependency boundaries."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args();b=Binary();d=Binary('MyScriptDocument.dll');md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(d.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
def pins(binary,s,e,expected):
 rows=[(i.mnemonic,i.op_str)for i in md.disasm(binary.data[binary.off(s):binary.off(e)],s)]
 for pin in expected:assert pin in rows,(hex(s),pin)
 return rows
word=pins(b,0x180023260,0x1800232fa,[('mov','rbp, qword ptr [rdx + 0x80]'),('cmp','edi, dword ptr [rbp + 0x70]'),
 ('test','dword ptr [rax + rcx*8 + 8], 0x100000'),('call','0x180022960'),('and','dword ptr [rbp + 0x30], 0xffefffff'),
 ('mov','rbp, qword ptr [rbp + 0x88]'),('mov','r14, qword ptr [r15 + 0x50]')])
assert word.index(('call','0x180022960'))<word.index(('and','dword ptr [rbp + 0x30], 0xffefffff'))<word.index(('mov','r14, qword ptr [r15 + 0x50]'))
pins(b,0x1800232fa,0x1800234cc,[('test','dword ptr [r14 + 0x30], 0x100000'),('je','0x18002349b'),
 ('cmp','dword ptr [rax + rbp + 0x20], 4'),('cmp','byte ptr [rcx], 0xf3'),('cmp','byte ptr [rcx + 1], 0xb0'),
 ('cmp','byte ptr [rcx + 2], 0x80'),('cmp','byte ptr [rcx + 3], 0x81'),('jg','0x18002348d'),
 ('cmp','byte ptr [r15 + 0x1f0], 0'),('call','0x180034d90'),('mov','r14, qword ptr [r14 + 0x88]')])
calls=pins(b,0x1800297cd,0x180029886,[('call','0x180020920'),('call','0x180023260'),('call','0x180037e80'),('call','0x180026ef0'),
 ('mov','r8, qword ptr [r14 + 0x30]'),('cmp','r8, -1'),('mov','r9, qword ptr [rax + 0x128]'),('mov','byte ptr [rsp + 0x20], 0')])
assert calls.index(('call','0x180020920'))<calls.index(('call','0x180023260'))<calls.index(('call','0x180037e80'))<calls.index(('call','0x180026ef0'))
#Source30 is a live copied value: a new result constructor alone cannot
#prove the later transform vector empty. Preserve this explicit boundary.
pins(b,0x1800283b0,0x180028800,[('mov','r15, qword ptr [rax + 0x30]'),('mov','qword ptr [r14 + 0x30], r15')])
pins(b,0x180037e80,0x180037ef0,[('mov','edi, dword ptr [r9 + 8]'),('test','edi, edi'),('je','0x1800380fb'),('cmp','dword ptr [rdx + 0xd8], 0')])
pins(b,0x180026ef0,0x180026f1a,[('cmp','dword ptr [r8 + 8], 0'),('jne','0x180026f1a'),('mov','al, 1'),('ret','')])
assert d.qwords(0x1801ed7e0,1)[0]==1 and d.qwords(0x1801ed7e0+8,1)[0]==0xffffffff00000daf
assert d.qwords(0x1801ed7e0+32,1)[0]==0x1801ed790
for offset,target in [(0x28,0x180030d40),(0x30,0x180030e20),(0x38,0x180030fb0)]:assert d.qwords(0x1801ed790+offset,1)[0]==target
assert b.qwords(0x180276c20,1)[0]==1 and b.qwords(0x180276c20+8,1)[0]==0xffffffff000002e7
assert b.qwords(0x180276c20+32,1)[0]==0x180276c68 and b.qwords(0x180276c68,1)[0]==0x1800658e0
regions=[(0x180023260,0x1800234cc),(0x1800297cd,0x180029886),(0x1800283b0,0x180028800),(0x180037e80,0x180037ef0),(0x180026ef0,0x180026f1a)]
report=dict(regions=[dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest())for s,e in regions],
 virtualBindings=4,scope='actualallWORD48 flags beforepayload100000clear; actualCHARinsertionlistflags gate; activecontrol expansion/deletion refuses',
 partial='unsupportedservice is refused beforecalling; cancellation/managedfailure keeps modeled prefix; no nativeallocatorfailure simulation',
 pending=['copied source30/priorV128 transform owner for037E80','DAF030FB0 actualdocumentinput32Bspans for026EF0','priorcache/C4Dreplacement and2E70658E0/BE2publication'],vendorExecution=False,documentAcceptance=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS5 native PE regions/4DAF+2E7 service bindings/WORDthenCHAR order/actualflag predicates/live copiedtransform boundary; no vendor execution.')
