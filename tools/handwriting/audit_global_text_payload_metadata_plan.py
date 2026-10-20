"""Readonly global024D40 metadata producer and failure-prefix closure."""
import json,sys,hashlib,runpy
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('REPORT.json')
target=Path(sys.argv[1]);b=Binary();f=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[(6442601792, 6442602285, 'aca8de5b831acf4d62b42437c80c11cc860eebe87c04fcac4b3f0bffcae7f0db'), (6442580704, 6442580921, 'f46860776d9338210186baa07002d2f6900e7c81499088f61ef5607cb4f440e4'), (6442589456, 6442590832, 'ea3a7540b11a3d0bde1f4fea50183710db2aff10c3dedf79ced0d940213b0e5c'), (6444185552, 6444185677, 'b0977a186cedd4570a3d12effa15e3d4097c71431c6d83b886b5e0ba1cad4d64'), (6444185888, 6444185930, '7023a46340536c1bfad6679640f9f58a3a48264bcc48291e650b937a9f043bca'), (6443928272, 6443928291, '8637006231d90d1ea11f327b5a109840c41bd5f65c3f30bb11e9938bc5d340df'), (6443279920, 6443279925, '0a6f8b667b1b596d05bbe6a2099e2a0034bf94c7c4546d0626e3745e425f1f3f'), (6443839728, 6443839733, '95bb3d00d0b6135c3018b9307c36b67e652de16f836ff06f95d610cd37812927'), (6442616054, 6442616133, '84c15bfd6ca0d192d794251d27e7b3d8f0472701a12c31bfbcf3c91a1c4468aa'), (6442617049, 6442617103, '6f504b0649df60987ae7efdaab88734a75466293dc450f6f11d31dddebea4dbf'), (6444184512, 6444184944, '9b1648d8746a766d78f325e74721966f766d7fa5ce8ccd4baf1c289d3c31eafe')]
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={}
for a,z,h in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
bindings={0x1802de610+0x248:0x1800ca630,0x1802de610+0x250:0x1801530f0,
          0x1802e1ff0+0x20:0x1801a73c0,0x1802e1ff0+0x68:0x1801a77d0,
          0x1802e1ff0+0x78:0x1801a7920,0x1802df340+0x10:0x180168ad0}
for a,v in bindings.items():assert b.qwords(a,1)[0]==v
pins={
 0x1800ca630:('mov','rax, qword ptr [rdx + 0x70]'),
 0x1801530f0:('mov','rax, qword ptr [rdx + 0x60]'),
 0x1800284f9:('mov','r8, qword ptr [rax + 0x250]'),
 0x18002850f:('mov','qword ptr [rbp + 0x38], rax'),
 0x180028513:('mov','r8d, 0x218'),
 0x18002852a:('mov','qword ptr [rbp + 0x40], rax'),
 0x180028537:('mov','r8, qword ptr [rax + 0x248]'),
 0x180028541:('mov','rdx, qword ptr [rbp + 0x50]'),
 0x1801a7940:('imul','r9, rcx'),0x1801a7944:('movzx','eax, word ptr [r9 + rax]'),
 0x180168ad0:('mov','rcx, qword ptr [rdx + 0x120]'),0x180168ada:('imul','rax, rax, 0x58'),
 0x180168ade:('add','rax, qword ptr [rcx + 0x18]'),
 0x1800288e0:('xor','r11d, r11d'),0x180028902:('mov','dword ptr [rbp + 0x78], r11d'),
 0x180024dbf:('call','0x1800210c0'),0x180024dcc:('mov','qword ptr [rcx], 0xffffffffffffffff'),
 0x180024dd3:('mov','dword ptr [rbx + 8], 0xffffffff'),0x180024dde:('mov','qword ptr [rbx + 0x40], 0xffffffffffffffff'),
 0x180024de9:('mov','qword ptr [rbx], r15'),0x180024def:('mov','dword ptr [rbx + 0x1c], r15d'),
 0x180024df3:('mov','byte ptr [rbx + 0x64], r15b'),0x180024df7:('mov','dword ptr [rbx + 0x48], 0xffffffff'),
 0x180024e41:('call','0x180021d10'),0x180024e4c:('mov','dword ptr [rbx + 0x60], 0xffffffff'),
 0x180024e56:('je','0x180024f0e'),0x180024e64:('jne','0x180024ef4'),
 0x180024e6e:('je','0x180024ef4'),0x180024e82:('call','r8'),
 0x180024e87:('jle','0x180024ed5'),0x180024eba:('call','r10'),
 0x180024ece:('call','r9'),0x180024ee8:('call','r10'),
 0x180024eeb:('mov','ecx, dword ptr [rax + 0x10]'),0x180024eee:('add','ecx, dword ptr [rdi + 0x68]'),
 0x180024ef1:('mov','dword ptr [rbx + 0x60], ecx'),0x180024f01:('call','0x18001fae0')}
for a,v in pins.items():assert ins[a]==v,(hex(a),ins.get(a))
payload=f['180024d40'];prior=f['180021d10'];flags=f['18001fae0'];wrapper=f['1800283b0']
assert payload.index('FUN_1800210c0')<payload.index('*(undefined4 *)(param_4 + 1)')<payload.index('FUN_180021d10')<payload.index('*(undefined4 *)(param_4 + 0xc)')<payload.index('FUN_18001fae0')
assert "*param_6 = '\\x01';"in prior and "*param_6 = '\\0';"in prior and 'puVar7[7] != 0xc25'in prior
assert 'if (bVar4)'in prior and 'lVar8 == 0'in prior
assert 'param_4 & 1' in flags and 'uVar3 & 0xce1fffff' in flags and '0x800000' in flags and '0xfdffffff' in flags
assert 'uStack_d0 = 0;' in wrapper
group=f['1801a77d0'];assert 'param_2 + 0x2c8'in group and 'param_2 + 0x248'in group and '0xffffff'in group
cuts=(root/'src/InkRecognition/LegacyFreshMainDescriptorCutSource.cs').read_text()
compact=(root/'src/InkRecognition/LegacyFreshMainCompactOrdinaryGraph.cs').read_text()
globalSource=(root/'src/InkRecognition/LegacyFreshMainGlobalOrdinaryState.cs').read_text()
metadata=(root/'src/InkRecognition/LegacyFreshMainGlobalGlyphSourceMetadata.cs').read_text()
for term in ['DescriptorOrdinal=f.DescriptorOrdinal','SecondaryDescriptorOrdinal=f.SecondaryDescriptorOrdinal']:assert term in compact
assert 's.DescriptorOrdinal==f.DescriptorOrdinal&&!s.IsPendingSeparator'in globalSource
assert 'line=I(d,16)'in cuts and 'LineId=line;'in cuts
assert 'Graph.State.GetNode(source.Node)[source.Arc].GroupId'in metadata
assert 'SourceOverlay.ResolveBaseReference(targetKey,token)'in metadata
saved=sys.argv;components=[]
for name in ['audit_global_text_source_wrapper_plan.py','audit_main_global_word_pass.py','audit_main_residual_source.py','audit_main_owned_source_lines.py']:
 path=Path(__file__).with_name(name);out=target.with_name(target.stem+'.'+name[6:-3]+'.json')
 sys.argv=[str(path)]+(['--output',str(out)]if name=='audit_main_global_word_pass.py'else[str(out)])
 runpy.run_path(str(path),run_name='__main__');components.append(dict(script=name,sha256=hashlib.sha256(out.read_bytes()).hexdigest()))
sys.argv=saved
report=dict(vendorExecution=False,runtimeImplemented=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],bindings={hex(a):hex(v)for a,v in bindings.items()},pins={hex(a):' '.join(v)for a,v in pins.items()},components=components,
 sourceLine='same frontend219.V248->graph21B.V68(target resolves higher originalLast)->groupID;V78->sourceDescriptorOrdinal; frontendV250 actualsource cast218.V10->descriptor88; I32+10 +peralternative wrapper68. Not a guide40 field, constant or count1 sampler.',
 lineBranches='native graphV20<=0 uses descriptor0; negative arc or NULLsource keeps initialized-1. Supported actualglobal metadata requires same live nonempty ordinary graph/source; decline any unsupported fallback owner.',
 prior='021D10 startsOUT1. Each visited fresh ordinary C4A/no cachedpayload clears0; no visited input retains1. Same raw scope has no C25 and no payload, so priorC21 remains empty. Nonordinary bypasses021D10 and keeps initial0.',
 flags='(((prior&1)<<24)^((ordinary?0:0x200000)|(OR(all213A0.flags)&0xCE1FFFFF)|(ordinary&&oneSameCoordinate?0x800000:0)))&0xFDFFFFFF',
 failures='CHAR registration attempted first, may consume cursor. Basic fields and priorC21 initialized even on registration failure; line60=-1 written; failure skips computed flags. Do not synthesize zero for an unwritten selectionID/flags or implicit rollback.',
 scope='same actual global scope/Document/preNN3E/Seed; one fresh ordinary source domain; proposed typed payload only, WORD/TEXT and acceptance remain later')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print('PASS11 static metadata regions/6 exact bindings/37 instruction pins/4 dependency closures; no runtime or vendor execution')
