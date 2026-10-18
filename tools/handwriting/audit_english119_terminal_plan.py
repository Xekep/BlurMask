"""EN119 terminal273 invocation/ownership proof. Static PE only."""
from pathlib import Path
import hashlib,json,runpy,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True);saved=sys.argv
components=[]
for name,flags in [('audit_english119_context_plan.py',False),('audit_source_frame_score_mode.py',True),('audit_english_positive24c_plan.py',False),('audit_fresh_first_text_policy.py',True)]:
 p=Path(__file__).with_name(name);out=target.with_name(target.stem+'.'+name.removeprefix('audit_').removesuffix('.py')+'.json')
 sys.argv=[str(p)]+(['--output',str(out)]if flags else[str(out)])
 g=runpy.run_path(str(p));components.append(dict(auditor=name,report=str(out),sha256=hashlib.sha256(out.read_bytes()).hexdigest()))
 if name=='audit_english119_context_plan.py':b=g['b'];f=g['f']
sys.argv=saved;md=Cs(CS_ARCH_X86,CS_MODE_64)
regions=[
 (0x1801edb60,0x1801edc67,'6d3388ecc207e0ef05fd2481100843f8670ec47822319190a1c4f1b4a55250e8'),
 (0x180158a66,0x180158ae5,'40ec6eb9234755df969dc381e9422fa23bbbee83afcc87bd55fabf6a42ebb005'),
 (0x1801f482b,0x1801f485a,'ef59e1c9238508bdcc392022ce36ae5b902ce0f1c6fc56307690aeb46853f1eb'),
 (0x18013b870,0x18013b9e0,'8182c06267944f7182f81d5e06ea083f02f585491f5acbed0c4a6ba2e663cfc9'),
 (0x18013bb50,0x18013bd40,'6027e9ca6dfc3f2c0f239338e12fd5adae41e358fae3da8b81b3d31165b04cc7'),
 (0x1801f2440,0x1801f2460,'1133e765edbe9c835fd5bc5a46e272d47764bc041248b748b235fa2946680c6a')]
#The last range is a comparison aid, not a guessed function-end claim.
for a,z,h in regions:
 actual=hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()
 if h is not None:assert actual==h
pins={
 0x1801edc41:('mov','dword ptr [rdx + 0xf4], 0xa000'),
 0x1801edc52:('mov','dword ptr [rdx + 0xfc], ecx'),
 0x18000c8a8:('xor','eax, eax'),
 0x18000c8cb:('mov','qword ptr [rdi + 0x98], rax'),
 0x180158a87:('call','0x180156630'),
 0x180158a94:('mov','rcx, qword ptr [r13 + 0x210]'),
 0x180158aa1:('movzx','eax, byte ptr [rdi + 0x150]'),
 0x180158aa8:('mov','byte ptr [rcx + 0xa8], al'),
 0x180158ad8:('mov','r8, qword ptr [rcx + 0x60]'),
 0x180158adf:('call','r8'),
 0x1801f4723:('add','r12d, dword ptr [r13 + 0xf4]'),
 0x1801f4833:('cmp','byte ptr [rax + 0x99], 0'),
 0x1801f483c:('cmp','byte ptr [rax + 0xa8], 0'),
 0x1801f4856:('mov','qword ptr [rdi + 0x78], rax'),
 0x18013b97c:('mov','qword ptr [rdi + 0x78], rbp'),
 0x18015855e:('lea','r8, [rsp + 0xb0]'),
 0x18015856f:('mov','r9b, 1'),
 0x18015857d:('mov','r10, qword ptr [rax + 0x80]'),
 0x180158584:('call','r10')}
for a,p in pins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==p,(hex(a),i.mnemonic,i.op_str,p)
assert b.qwords(0x1802e7968,1)[0]==0x1801edb60
assert b.qwords(0x1802e79d0+0x70,1)[0]==0x1801f2440
assert b.qwords(0x1802e79d0+0x80,1)[0]==0x1801f42d0
assert 'FUN_1801ef070(param_1,param_2,0,local_res18)'in f['1801f2440']
assert 'plVar8 = *(longlong **)(param_2 + 0x208)'in f['180157c60']
assert '(param_1,plVar8,local_188,1,'in f['180157c60']
assert '*(undefined8 *)(param_2 + 0x78) = 0;'in f['18013bb50']
assert '*(undefined1 *)(param_2 + 0x99)'not in f['180156630']
#Native no-Arabic resolver produces five zero traits; terminal scan consumes
#those actual record bytes. The scoped owner cannot import property events.
assert 'param_2 + 0xf4'not in f['1801edde0']
report=dict(staticOnly=True,vendorExecution=False,components=components,
 regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions if h is not None],
 pins={hex(a):' '.join(p)for a,p in pins.items()},
 registered23F=dict(constructor='2E7968->1EDB60',prepare='2E79D0+70->1F2440->1EF070',terminal='2E79D0+80->1F42D0'),
 selectedInvocation='158970 runs156630 then copies same source secondary150 to child210+A8. New EN raw-only owner derives150=0 through16A130,24A,24C exclusiveXYstores and265; no reused/cloned/property-event source is accepted.',
 configuration99='NEW2B3 constructor00C8CB zeros98..9F. Scoped156200/156630 selected binding has no external property events;2458B0 temporary99 wrapper is outside this owner. No universal zero assertion for arbitrary native invocations.',
 scalars='FIRST ctor1EDC41 F4=0xA000,FC=0. TOP273 ctor13B97C and reset13BB50 clear78. Terminal relative-cost/reference branch uses actual99/A8; E6 comes from same acoustic alphabet shadda lookup.',
 currentScope='Owned final B8 terminal selection and exact predecessor trace only. No rich273/274 allocation, WORD276/275, source720, accepted text, default recognizer, or English accuracy claim.',
 next='NEW typed EN119 terminal owner validates same completed Context/Primary/Policy/cache/source references, derives policy without caller flags, uses existing selector and trace. Independent raw-resource/capturedB8 scalar/backtrack oracle; no NN reruns. Actual WORD join remains a separate prerequisite.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS6 pinned terminal regions/{len(pins)} instructions/23F table plus4 composed ownership audits; no vendor execution')
