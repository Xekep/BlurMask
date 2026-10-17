"""Static21FE30->218560->217DB0 ordering and callback-success gate."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions();m=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins=[];count=0
for a,z,sha in [(0x18021fe30,0x18021ffd0,'a347dbe66669f21e1b04b6e58126c3f277972ea29ac7e3fb8997b14bc9e38f6f'),
 (0x180218560,0x1802188e1,'9b322a862d1f0a9b22edef7d3eff93c8ff94be6c7853d13c69e71dba89468744'),
 (0x180217db0,0x180218560,'5cfc62caadf652d8b89b314509b6b280e3733407e4344096f606b09cd08fbdcd')]:
 assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==sha;pins.append([hex(a),hex(z),sha])
def exact(a,z,wanted):
 global count
 got=[(i.mnemonic,i.op_str) for i in m.disasm(b.data[b.off(a):b.off(z)],a)]
 assert got==wanted,(hex(a),got);count+=len(got)
exact(0x1802185d1,0x1802185e9,[('call','0x180217db0'),('test','al, al'),('je','0x1802188cf'),
 ('lea','ecx, [rbx - 1]'),('test','ecx, ecx'),('js','0x18021889c')])
exact(0x1802211ab,0x1802211c4,[('call','qword ptr [rbp - 0x58]'),('movzx','edi, byte ptr [rsp + 0x60]'),
 ('test','al, al'),('je','0x18022127f'),('mov','ebx, dword ptr [rbp + 0xe4]'),('mov','r12b, 1')])
exact(0x18021fee4,0x18021fee9,[('call','0x180218560')])
exact(0x18021feb4,0x18021feb8,[('movups','xmmword ptr [rbp + 0x44], xmm0')])
body=f['18021fe30'];assert body.index('*(undefined4 *)(lVar1 + 0x44)')<body.index('FUN_180218560(')
body=f['180218560'];assert body.index('FUN_180217db0(')<body.index('iVar1 = iVar1 + -1;')
caller=f['180221840']
for item in ['local_c0 = *(undefined2 *)(param_10 + 0x32);','local_b8 = *(undefined4 *)(param_10 + 0x3c);',
 'puVar2 = (undefined8 *)(param_10 + 0x44);']:assert item in caller,item
# Kernel success publishes a remaining count of0. Its retained storage tail is
# not a live prefix to be fed into the next invocation. The prepared kernel
# already has independent partial-failure and callback-order controls.
residual=f['180217db0'];assert '0x24b0' in residual and '0x24a0' in residual
report=dict(pins=pins,exactInstructions=count,ordering='packet stores ->217DB0 ->glyph rows ->typed publication acknowledgement',
 wordSeed='initial actual WORD32/3C/44..53; never fake zero metadata',
 scope='owning segmented association/seed bridge; actual acceptance callback is separate',
 unresolved=['genuine nonzero residual trace fixture','actual nonempty source70 event','complete2271F0 result alternatives'])
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS {count} exact instructions/3 native ranges/WORD seed and callback gates; static reads only.')
