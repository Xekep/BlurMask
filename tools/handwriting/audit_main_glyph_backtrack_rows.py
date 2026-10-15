"""Read-only21FFD0 numeric row/key producer audit, not full backtracking."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('output.json')
b=Binary();fs=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
checks=0;pins=[]
def pin(a,z,sha):
    value=hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest();assert value==sha;pins.append([hex(a),hex(z),value])
def exact(a,z,expected):
    global checks
    actual=[(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(a):b.off(z)],a)];assert actual==expected,(hex(a),actual)
    checks+=len(actual)
pin(0x180216610,0x180216858,'20f5badb4c2e42629866807ed22c65cf2289e0b6dfd0f453cbe1113f6c15cc14')
pin(0x180220b8f,0x180220dd8,'3c11d49e9d919f98060548ea8c2d5efec91c52faacb0e53f5d025a0d5fecd808')
pin(0x1801a7bf0,0x1801a7cee,'333812bfb80656868465e11eb5c42de805448f74c87eea0f1fd0718925c4c04b')
exact(0x180220bfc,0x180220c01,[('call','0x180216610')])
exact(0x180220c58,0x180220c94,[
 ('movzx','ecx, byte ptr [rdx + r8*8 + 0x20]'),('movzx','eax, cl'),('mov','word ptr [rbp + 0x6c], r15w'),
 ('mov','r15, qword ptr [rbp - 0x70]'),('and','al, 7'),('mov','byte ptr [rbp + 0x8c], al'),
 ('movzx','eax, byte ptr [rdx + r8*8 + 0x21]'),('mov','byte ptr [rbp + 0x8e], al'),
 ('movzx','eax, byte ptr [rdx + r8*8 + 7]'),('shr','cl, 6'),('and','cl, 1'),('shr','al, 1'),('and','al, 1'),('mov','byte ptr [rbp + 0x8d], cl')])
exact(0x180220cb7,0x180220cc8,[('mov','dword ptr [rbp + 0x90], 0'),('mov','byte ptr [rbp + 0x94], 0')])
exact(0x180220d42,0x180220d77,[
 ('movaps','xmm0, xmmword ptr [rbp + 0x50]'),('movaps','xmm1, xmmword ptr [rbp + 0x60]'),
 ('movups','xmmword ptr [rdx], xmm0'),('movaps','xmm0, xmmword ptr [rbp + 0x70]'),('movups','xmmword ptr [rdx + 0x10], xmm1'),
 ('movaps','xmm1, xmmword ptr [rbp + 0x80]'),('movups','xmmword ptr [rdx + 0x20], xmm0'),
 ('movsd','xmm0, qword ptr [rbp + 0x90]'),('movups','xmmword ptr [rdx + 0x30], xmm1'),('movsd','qword ptr [rdx + 0x40], xmm0'),
 ('inc','dword ptr [rsi + 0x24a0]')])
exact(0x18021681d,0x18021683d,[('mov','eax, dword ptr [rsp + 0x58]'),('movzx','ecx, word ptr [rsp + 0x70]'),('and','eax, 0xffffff'),('shl','rcx, 0x18'),('or','rax, rcx'),('mov','rcx, qword ptr [rsp + 0x80]'),('mov','byte ptr [rcx], 0')])
exact(0x1802167fa,0x18021681d,[('sub','eax, r14d'),('movabs','rcx, 0x100000000'),('movzx','eax, ax'),('and','r14d, 0xffffff'),('or','rax, rcx'),('shl','rax, 0x18'),('or','rax, r14'),('jmp','0x18021683d')])
assert b.qwords(0x1802de610+0x168,1)[0]==0x180152a10
assert b.qwords(0x1802de610+0x170,1)[0]==0x180152a30
assert b.qwords(0x1802e1ff0+0xb8,1)[0]==0x1801a7bf0
assert b.qwords(0x1802e1ff0+0xc0,1)[0]==0x1801a7c70
s=fs['18021ffd0']
for term in ['local_230 = 0;', 'local_22c = 0;', 'local_c0 = 1;', 'local_290 = sVar24;',
             'uStack_194 = local_240;', 'uStack_190 = local_280;', 'local_1b8 = local_2a0;',
             'iStack_1a4 = local_268;', 'local_198 = iVar14;', 'bStack_17c = bVar3 & 7;',
             'bStack_17b = bVar3 >> 6 & 1;', 'bStack_173 = *(byte *)(lVar16 + 7 + lVar26 * 0x28) >> 1 & 1;',
             'local_240 = local_270;', 'local_280 = local_292;', 'local_270 = local_2a0;']:
    assert term in s,term
result=dict(exactInstructions=checks,pins=pins,bindings=4,
 scope='actual owned DIRECT compact winner -> numeric72 append events; ordinary graph subtypes0..3',
 unresolved=['word-segment callbacks and summary packets','217DB0 residual association','218560 linked glyph publication','complete21FFD0'])
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(result,indent=2)+'\n').encode())
print(f'PASS {checks} exact instructions,3 native ranges,4 source-boundary bindings and15 numeric/initial-callback contracts; no vendor execution.')
