"""Pinned static PE proof for field0, never loads the vendor binary."""
import hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions();cs=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
blocks=[(0x1801be530,0x1801be6a8,'9f6d27f219b191689f8e557b466f55292bfb52b3e32776dda918b62c3d18624a'),
 (0x1801c1260,0x1801c12f6,'31660a4020f650700e58d2e0ec9b2705bfee5c584d3a6415ece6ab5da30915ac'),
 (0x1801529a0,0x1801529b8,'4727450093d0ce0d70ef4a33af3b9a8b7d5f8fd29c4920669c82590119af526d')]
for a,z,sha in blocks:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==sha
def asm(a,z):return [(i.mnemonic,i.op_str) for i in cs.disasm(b.data[b.off(a):b.off(z)],a)]
assert b.qwords(0x1802de610+0x150,1)[0]==0x1801529a0
assert b.qwords(0x1802e4180+0x78,1)[0]==0x1801c1260
assert b.qwords(0x1802e4180+0x80,1)[0]==0x1801c1300
assert asm(0x1801529a0,0x1801529b8)[:3]==[('mov','rdx, qword ptr [rdx + 0x78]'),('mov','rax, qword ptr [rdx]'),('mov','r10, qword ptr [rax + 0x78]')]
assert asm(0x1801be55b,0x1801be561)==[('mov','dword ptr [rax], 0x3f800000')]
assert struct.unpack_from('<f',b.data,b.off(0x1802ed440))==(8192.0,)
assert struct.unpack_from('<I',b.data,b.off(0x1802ed7a0))==(0x80000000,)
assert asm(0x1801be5dc,0x1801be5f7)==[('movd','xmm0, dword ptr [r15 + rbx + 4]'),('cvtdq2ps','xmm0, xmm0'),('xorps','xmm0, xmm8'),('divss','xmm0, xmm7'),('cvtss2sd','xmm0, xmm0'),('call','0x18024e970')]
assert asm(0x1801be600,0x1801be613)==[('inc','r12'),('movss','dword ptr [r15 + rbx], xmm1'),('mov','rax, qword ptr [rbp + 0x20]'),('addss','xmm6, dword ptr [r15 + rax]')]
assert ('divss','xmm0, xmm6') in asm(0x1801be67c,0x1801be68a)
assert ('movss','xmm0, dword ptr [rax + rcx*8]') in asm(0x1801c12cc,0x1801c12e8)
assert ('xorps','xmm0, xmm0') in asm(0x1801c12e8,0x1801c12f6)
assert 'FUN_1801be530' in f['1801be930']
assert 'if ((fVar14 != 0.0) && (param_5 != 1))' in f['1801bc900']
assert '*(undefined4 *)(param_3 + 0x1c) = 0x3f800000;' in f['1801bc900']
resource=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(resource).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
# Independently consume the directional prefix and variable cache tail; do not
# locate the configuration by searching repeated zero bytes.
p=189734+16
def u8():
    global p
    value=resource[p];p+=1;return value
def u16():
    global p
    value=struct.unpack_from('<H',resource,p)[0];p+=2;return value
def network():
    global p
    magic,version,length=struct.unpack_from('<HHI',resource,p)
    assert (magic,version)==(0xfea0,0x106)
    p+=8+length
def shorts(copies):
    global p
    count=u16();p+=count*2*copies
p+=4;assert u8()==1;network();p+=3*4+2+2+4+1+1+2
assert u8()==1;p+=2+9*4
assert u8()==1 and u8()==0
p+=2+4+5*2+7*4
shorts(1);shorts(2);shorts(2);p+=2+2+4+4+2+2
assert u8()==0
if u8():shorts(2)
for _ in range(7):
    if u8():shorts(2)
    p+=2;mode=u8();assert mode<=2
    if mode==0:p+=4
    for _ in range(3):
        if u8():network()
tail=p
p=tail+1+4+1+8
n=struct.unpack_from('<H',resource,p)[0];p+=2
for _ in range(n):count=struct.unpack_from('<H',resource,p)[0];p+=2+count*2
p+=4+5*4+1+8+1+2+4+2+2+12+1
assert struct.unpack_from('<f',resource,p)==(0.0,)
report=dict(blocks=[dict(start=hex(a),end=hex(z),sha256=s) for a,z,s in blocks],
 bindings=['frontend150=1529A0 -> ordinaryV78=1C1260 -> candidate24.field0','frontend1B8 -> V80=1C1300 -> vector10[channel] is separate'],
 arithmetic='singleton1; first20 exp(double(float(-float(cost)/float(scale*8192)))), sequential float sum and float division; laterzero; no exponent clamp',
 actualRaw100Offset=p,actualRaw100=0,actualGroupScale=1,
 scope='same owning sampled main DIRECT/case scorer; no winner or handwriting accuracy claim',noVendorExecution=True)
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS static3 executable blocks, frontend/V78 vsV80, float instruction ordering, actual raw100=0 scale1')
