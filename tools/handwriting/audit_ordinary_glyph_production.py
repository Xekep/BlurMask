"""Pinned ordinary216BC0 scalar/caller/score contract, static PE reads only."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('output.json')
b=Binary();engine=Binary('MyScriptEngine.dll');fs=functions();efs=functions('MyScriptEngine.dll');md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
instructions=0;hashes=[]
def exact(start,end,wanted):
    global instructions
    actual=[(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(start):b.off(end)],start)]
    assert actual==wanted,(hex(start),actual)
    instructions+=len(actual)
def pin(start,end,wanted):
    raw=b.data[b.off(start):b.off(end)];value=hashlib.sha256(raw).hexdigest();assert value==wanted
    hashes.append(dict(start=hex(start),end=hex(end),sha256=value))
pin(0x180216bc0,0x1802173e0,'84da9213fea4000119cd81f85d9f1f73376012b0362565b8e2435ad1a4b2f70f')
pin(0x180216860,0x180216bbb,'49f0d99b8e9e3c190bdc590e98874885a980244b394780689ba0d34ef3098b25')
pin(0x180152f50,0x180152f8a,'9bf0bca1cfbc07b656bc337cb6350c0d08b02f107f6b5a6f2be0be69b96e3ee0')
pin(0x1801c1300,0x1801c13a0,'d553ea2e1172201343c1b0a18510f24bdd96dc4f3407750aa0695bacf7d88113')
pin(0x1801c13a0,0x1801c1520,'b9319d885c0b3698680031ce1204f244b2fd3be28a8c2ff3582c388a74b932fa')
exact(0x180216cbd,0x180216ccd,[('mov','byte ptr [rbx], 0'),('mov','dword ptr [rsp + 0x78], ecx'),('mov','word ptr [rsp + 0x74], r8w'),('mov','byte ptr [rax], 0')])
exact(0x180216d19,0x180216d27,[('lea','eax, [r14 + 6]'),('cmp','ax, 2'),('ja','0x180217088')])
exact(0x1802170d6,0x1802170df,[('dec','dword ptr [rax]'),('movsxd','rax, dword ptr [rax]'),('lea','rcx, [rcx + rax*2]')])
exact(0x18021709c,0x1802170c2,[('movsxd','rcx, dword ptr [rbp - 0x6c]'),('movzx','r9d, ax'),('cmp','ecx, -1'),('je','0x1802170bc'),('lea','rdx, [rcx + rcx*4]'),('mov','rcx, qword ptr [rdi + 0x2460]'),('movzx','r8d, word ptr [rcx + rdx*8 + 0x1c]'),('jmp','0x1802170c2'),('mov','r8d, 0xffff')])
exact(0x180217126,0x180217138,[('call','0x180216860'),('test','al, al'),('jne','0x18021706a'),('jmp','0x1802173aa')])
exact(0x18021713c,0x18021715e,[('cmp','r14w, word ptr [rbx + 0xe]'),('je','0x180217289'),('test','r15b, r15b'),('jne','0x180217274'),('lea','eax, [r14 + 9]'),('cmp','ax, 5'),('jbe','0x180217289')])
exact(0x18021715e,0x18021717b,[('movzx','eax, byte ptr [r12 + 0x3e]'),('movd','xmm0, eax'),('mov','rax, qword ptr [rbp - 0x30]'),('cvtdq2ps','xmm0, xmm0'),('divss','xmm0, dword ptr [rip + 0xd62c1]'),('movss','dword ptr [rax], xmm0')])
exact(0x1802171bb,0x1802171cd,[('mov','r10, qword ptr [rax + 0x1b8]'),('call','r10'),('mov','rax, qword ptr [rbp - 0x28]'),('movss','dword ptr [rax], xmm0')])
exact(0x1802171e4,0x18021722d,[
 ('cmp','bx, word ptr [rdi + 0x24dc]'),('jne','0x1802171f1'),('mov','al, 0xff'),('jmp','0x18021722d'),
 ('cmp','byte ptr [r12 + 0x3d], 0'),('mov','r11, qword ptr [rbp - 0x60]'),('je','0x180217218'),
 ('mov','rax, qword ptr [r11 + 0x58]'),('movzx','ecx, bx'),('movsx','rdx, byte ptr [rcx + rax]'),('cmp','dl, 0xff'),('je','0x180217218'),
 ('mov','rax, qword ptr [r11 + 0x78]'),('movzx','eax, byte ptr [rdx + rax]'),('jmp','0x18021722d'),
 ('mov','rax, qword ptr [r11 + 0x50]'),('movzx','ecx, bx'),('movsx','r8, byte ptr [rax + rcx]'),('mov','rax, qword ptr [r11 + 0x78]'),('movzx','eax, byte ptr [r8 + rax]')])
exact(0x18021722d,0x180217265,[
 ('mov','ecx, dword ptr [rsp + 0x78]'),('lea','r8, [rbp - 0x10]'),('mov','dword ptr [rbp - 0x10], ecx'),('mov','r11d, 0xffff'),
 ('movzx','ecx, word ptr [rsp + 0x74]'),('mov','rdx, r9'),('mov','byte ptr [rbp - 6], al'),('xor','eax, eax'),
 ('mov','word ptr [rbp - 0xc], cx'),('mov','rcx, r13'),('mov','word ptr [rbp - 0xa], r11w'),('mov','word ptr [rbp - 8], bx'),
 ('mov','byte ptr [rbp - 5], 0'),('mov','dword ptr [rbp - 4], eax'),('call','r10')])
exact(0x180217289,0x1802172af,[('mov','rax, qword ptr [rbp - 0x30]'),('mov','dword ptr [rax], 0x3f800000'),('mov','rax, qword ptr [rbp - 0x28]'),('mov','dword ptr [rax], 0x3f800000'),('mov','eax, r11d'),('mov','rcx, qword ptr [rbp - 0x20]'),('mov','dword ptr [rcx], eax'),('test','rsi, rsi'),('je','0x1802173a8')])
exact(0x1802172af,0x1802172b7,[('mov','word ptr [rsi], r11w'),('cmp','r15b, 1')])
exact(0x1802172f9,0x18021730b,[('mov','r9, qword ptr [rax + 0x280]'),('call','r9'),('test','al, al'),('je','0x1802173a8')])
assert b.qwords(0x1802de610+0x280,1)[0]==0x180155710
assert b.qwords(0x1802e1ff0+0x1c8,1)[0]==0x18005a400
description,words=b.descriptor(0x21d);table=0x1802e4180
assert b.qwords(table+8,1)[0]==description and words[1]&0xffffffff==0x21d
assert b.qwords(table+0x80,1)[0]==0x1801c1300 and b.qwords(table+0x88,1)[0]==0x1801c13a0
assert '*local_b8 = (float)*(byte *)((longlong)param_3 + 0x3e) / 255.0' in fs['180216bc0']
assert '*(undefined1 *)(param_2 + 0x12) = 0' in fs['18000c860']
assert '*(undefined8 *)(param_2 + 0x84) = 0' in fs['18000c860']
assert '*(undefined8 *)(param_2 + 0x98) = 0' in fs['18000c860']
for offset in ['8','10','0xc','0xe','0x10','0x12','0x14','0x1a']:
    assert 'param_2 + '+offset in efs['180053f20']
result=dict(textSha256=hashlib.sha256(b.data).hexdigest(),engineSha256=hashlib.sha256(engine.data).hexdigest(),
 exactInstructions=instructions,hashes=hashes,bindings=4,
 scope='actual fresh Russian case owner;prepared72;ordinary subtype0;same graph/cache;no vendor execution',
 remaining='actual72 producer;protocol/multi-output;FFFDfrontend1A8;full218560/21FFD0')
output=Path(sys.argv[1]);output.parent.mkdir(parents=True,exist_ok=True);output.write_bytes((json.dumps(result,indent=2)+'\n').encode())
print(f'PASS {instructions} exact instructions,5 pinned ranges,4 graph/frontend/cache bindings; prepared72 ordinary path only.')
