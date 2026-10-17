"""Static existing glyph-reference/token contracts used by owning WORD projection."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions();m=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins=[];count=0
for a,z,sha in [(0x18013ae90,0x18013af30,'85f2f740ef56504887346ef000b5b69ff96dd6d2371402d82c2e16742dcf9529'),
 (0x180139c90,0x180139d90,'1c2f395d1d985d49623c8b480ea2708f7c2a49b5b0fe8671861e458f77db94f4'),
 (0x18013c840,0x18013ca20,'cc898b957c136d8d304de687f7a8fbeffa9bc099320489cb068b5eb9f2e87758')]:
 assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==sha;pins.append([hex(a),hex(z),sha])
def exact(a,z,wanted):
 global count
 got=[(i.mnemonic,i.op_str) for i in m.disasm(b.data[b.off(a):b.off(z)],a)]
 assert got==wanted,(hex(a),got);count+=len(got)
exact(0x18013ae90,0x18013aed5,[('mov','r9, r8'),('mov','r10, r8'),('sar','r10, 0x38'),('mov','rax, r8'),
 ('and','r10d, 1'),('sar','rax, 0x18'),('shl','r10, 0x20'),('and','r9d, 0xffffff'),('sar','r8, 0x28'),('movzx','ecx, ax'),
 ('or','r10, rcx'),('movzx','eax, r8w'),('shl','r10, 0x18'),('or','r10, r9'),('bt','r8, 0xf'),('lea','r8d, [rax - 0x10000]'),('cmovae','r8d, eax')])
exact(0x18013af0e,0x18013af1e,[('movsxd','rcx, r8d'),('lea','rdx, [rcx + rcx*4]'),('mov','rcx, qword ptr [rax + 0x20]'),('lea','rax, [rcx + rdx*8]'),('ret','')])
exact(0x180139cb9,0x180139cd7,[('movzx','ecx, word ptr [r8 + 8]'),('mov','rax, qword ptr [rsp + 0x50]'),('and','cx, 3'),
 ('sub','cx, 7'),('mov','word ptr [rax], cx'),('mov','byte ptr [rax + 2], 0'),('add','rsp, 0x28'),('ret','')])
s=f['180218560'];assert 'piVar13[2] - *(int *)' in s and '0x2478' in s
s=f['180139c90'];assert '*(undefined8 *)(param_4 + 0x18)' in s
s=f['18013c920'];assert 'param_2 + 0x20' in s and 'FUN_18013c840' in s
report=dict(pins=pins,exactInstructions=count,scope='actual published WORD275 references -> ordinary raw glyph UTF8 and canonical source keys',
 ownerBinding='actual40-byte pool records; same-associated-row key/relativecompactdescriptor; ordinary originalNNsource checked',
 exclusions=['not0213A0 wrapper filtering','not021000 accepted text-source records','no invented control/alternate owner','protocol span keys are not NN group arcs','not original document point IDs'])
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS {count} exact instructions/3 existing native ranges; source identity and distinct control/alternate owners retained.')
