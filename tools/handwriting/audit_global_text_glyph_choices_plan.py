"""Scoped static global glyph-choice plan. Reads vendor bytes; never executes them."""
import hashlib,json,sys,runpy
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);b=Binary();f=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[(6443747040, 6443747392, '6c2600caf2d7b6b388aedfe41f0be739896e5adbb72a5413464c07f006f2b176'), (6442587040, 6442588000, '1b14a995679d2234a4d35b1042b2e21c59b44212bfd2c6b74b7cb87b8bbe7e2f'), (6442615728, 6442616608, '4c249b787ffcabc324dcc9fa26a6db9ba74ba006abb301143f039f77dfd08d23'), (6442602288, 6442604752, '108097dbb0b17e9102cae2b312d9f33501de7aae9a4fd1a053d05a3f1ba589fd'), (6442823584, 6442825648, '6ca505eecff0b55186b5eaa3a068a760924b0be90596c272e7c73081087c45c9')]
ins={};md=Cs(CS_ARCH_X86,CS_MODE_64)
for a,z,h in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
pins={
 0x18013c717:('sar','rax, 0x28'),0x18013c71e:('movzx','ecx, ax'),
 0x18013c721:('bt','rax, 0xf'),0x18013c726:('lea','eax, [rcx - 0x10000]'),
 0x18013c72c:('cmovae','eax, ecx'),0x18013c733:('mov','dword ptr [rsp + 0x50], eax'),
 0x18013c7d6:('mov','r10, qword ptr [rax + 0x60]'),0x18013c7da:('lea','rax, [rsp + 0x50]'),
 0x18013c7e4:('call','r10'),0x18013c80d:('mov','dword ptr [rdx], eax'),
 0x180028458:('xorps','xmm0, xmm0'),0x180028464:('movups','xmmword ptr [rbp + 0x10], xmm0'),
 0x1800285a6:('mov','rax, qword ptr [rcx + 0x10]'),0x1800285aa:('mov','rdi, qword ptr [rax + 0x10]'),
 0x1800285ae:('test','rdi, rdi'),0x1800285b1:('je','0x18002870d'),
 0x1800285ba:('lea','r9, [rbp + 0x18]'),0x1800285c6:('mov','r8d, 0x12d0065'),
 0x1800285e0:('call','r10'),0x1800214b1:('cmp','byte ptr [rax + 8], 0'),
 0x1800214b5:('je','0x1800214cf'),0x1800215b0:('cmp','eax, edi'),
 0x1800215b2:('jle','0x1800215b8'),0x1800215b4:('dec','eax'),
 0x1800216c6:('call','0x180021000')}
for a,v in pins.items():assert ins[a]==v,(hex(a),ins.get(a))
caller=f['1800213a0'];outer=f['180024f30'];wrapper=f['1800283b0']
assert "(char)param_3[1] == '\\0'"in caller
assert 'FUN_1800213a0(param_1,param_2,param_3,'in outer
assert 'FUN_180024f30(param_1,param_2,&local_138,'in wrapper
assert 'uStack_130 = 0;'in wrapper and '0x12d0065,&uStack_130,1' in wrapper
assert wrapper.index('uStack_130 = 0;')<wrapper.index('0x12d0065,&uStack_130,1')
assert outer.index('iVar9 = (**(code **)(lVar13 + 0x40))')<outer.index('while (uVar23 = uVar10, cVar7')
components=[];saved=sys.argv
for script,flag in [('audit_glyph_alternative_selection.py',False),('audit_legacy_text_glyph_caller.py',True),('audit_main_text_source_positions.py',False),('audit_global_ordinary_refresh_plan.py',False)]:
 path=Path(__file__).with_name(script);out=target.with_name(target.stem+'.'+script[6:-3]+'.json')
 sys.argv=[str(path)]+(['--output',str(out)]if flag else[str(out)])
 runpy.run_path(str(path),run_name='__main__');components.append(dict(script=script,sha256=hashlib.sha256(out.read_bytes()).hexdigest()))
sys.argv=saved
report=dict(vendorExecution=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(v)for a,v in pins.items()},components=components,
 marker='signed16 from original reference bits40..55, successful13A830 adjusted marker then0213A0 empty removal; no fallback',
 filter='THIRD argument byte8. 0283B0 local138+8=uStack130 initialized0; prior result property12D0065 may replace it. Fresh original bootstrap has no prior result. Knowledge99 is NOT the direct producer.',
 geometry='same original Document.CopySelectedInputs value map; same actual V140 references24N and residual2N; group0 ordinary noC25, one closed original root interval,05AFA0 ordered append without coalescing',
 runtimePlan='NEW global choices and NEW global document position type; ZERO old seams; actual scope/completion/shared3E. Refresh ALL actual WORD refs before any selection/emission; then existing13A830 and0213A0 kernels.',
 excluded=['024F30 source-node interning','CHAR/WORD/TEXT registration','negative/span/protocol sources','associated/cached/C25','properties/prior result reuse','native failed4000 replay','library document acceptance','Russian quality claim'])
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print('PASS5 native regions/25 exact instructions/4 dependency closures; fresh wrapper byte8 producer and signed marker; static-only global choices proposal')
