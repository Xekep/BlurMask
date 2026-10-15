"""Static PE/source proof for the bounded ordinary assigned1AAE70 path."""
import hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions();cs=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
blocks=[(0x1801aae70,0x1801ac0d5,'1e05e24372dfe33d1aca0d83a0e18c97615f464d052fb32368fc58db796b6228'),
 (0x18019c5a0,0x18019c5e0,'d09247d09cf9513d725cf69956002d73c2c110b799860c00dce91626183f72bf'),
 (0x1801abdfb,0x1801abe2b,'5c1d0b481203b16092ce624de5f7c1b6553ad18c81dcebce0cb51a0601a94e3c'),
 (0x1802e1f20,0x1802e1fe4,'9312d04ad8f6868b591dd94040a7cd3b0e193b05b556d5243d21a5d86e8d0da6')]
for a,z,s in blocks:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==s
def asm(a,z):return [(i.mnemonic,i.op_str) for i in cs.disasm(b.data[b.off(a):b.off(z)],a)]
assert asm(0x18019c5b5,0x18019c5c3)==[('movzx','eax, byte ptr [rdi + 0x51]'),('mov','byte ptr [rdx + 4], al'),('movzx','eax, byte ptr [rdi + 0x52]'),('mov','byte ptr [rdx + 5], al')]
assert asm(0x1801abdfb,0x1801abe00)==[('movss','xmm1, dword ptr [rax + 0x30]')]
assert asm(0x1801abe18,0x1801abe1c)==[('minss','xmm0, xmm1')]
assert struct.unpack_from('<f',b.data,b.off(0x1802e1e88))==struct.unpack('<f',struct.pack('<f',1.87))
assert struct.unpack_from('<f',b.data,b.off(0x1802e1ea0))==struct.unpack('<f',struct.pack('<f',2.95))
assert struct.unpack_from('<f',b.data,b.off(0x1802e1eac))==struct.unpack('<f',struct.pack('<f',2.35))
s=f['18019f850'];assert 'bStack_107 = false;' in s and "cVar13 != '\\0' || bStack_107 != false" in s
assert '0x170) + 0x70' in s
merge=f['1801a3b30'];assert '*(undefined1 *)(lVar17 + 0x51 + lVar16) = uVar12' in merge
assert 'uVar12 = 0;' in merge and 'uVar12 = 1;' in merge
ctor=f['18019c970'];assert '*(undefined4 *)(param_2 + 0x1d9) = 0;' in ctor
init=f['1801a6980'];assert "*(char *)(param_2 + 0x1d8) = param_3[3]" in init and '0x1da' not in init
frontend=f['180150d00'];assert '0xc4)' in frontend and 'iVar10 != 0x137' in frontend and 'DAT_1802eb3a0' in frontend
assert 'CONCAT13(*(undefined1 *)(param_2 + 0x91)' in frontend
assert 'if (param_4 < 0x107)' in f['180196500'] and '*(undefined1 *)(param_2 + 0x134) = 0;' in f['180196500']
data=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(data).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert data[189396]==1 and struct.unpack_from('<H',data,188157+14)==(0x106,)
report=dict(blocks=[dict(start=hex(a),end=hex(z),sha256=s) for a,z,s in blocks],
 actualGraph9=1,source='same actual ordinary assigned descriptor135; fresh C4=0, noF0009; no external/property overrides',
 compact='4=wide51 (physicalV70 OR, zero fresh; merges OR);5=wide52 endpointnibbles zero;7=wide7 kind0',
 categories='FirstIds[FirstMap[symbol]] and table7x7, NOT composite geometry category',
 thresholds='request30 readMOVSS, positive-gap punctuation MIN(.55,.12); ordinary max(guideHeight*2.35,gapInput*1.87) or height*2.95',
 scope='kind1 prepared64 request and actualownedgeometry; no unassigned, known-input, custom metadata, dynamichigherarcs or winning-request production',noVendorExecution=True)
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS static4 pinned blocks, compactfieldlineage, main9=1,1D8shortcut,1DAzero,discretecategories and floatthresholds')
