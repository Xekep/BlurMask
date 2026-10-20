"""Static next global024F30 source-wrapper plan; no runtime or vendor execution."""
import json,hashlib,sys,runpy
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('REPORT.json')
target=Path(sys.argv[1]);b=Binary();f=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[(6442602288, 6442604752, '108097dbb0b17e9102cae2b312d9f33501de7aae9a4fd1a053d05a3f1ba589fd'), (6442601792, 6442602285, 'aca8de5b831acf4d62b42437c80c11cc860eebe87c04fcac4b3f0bffcae7f0db'), (6442588000, 6442588848, '807e72beb5456f94ba6945fd79d7a75853f3e2a91ab06e973761c8433528ac76'), (6442840144, 6442840368, 'e1537bf14ba797fa0230cfde5f5a52aeb99c791e46fb5bae4f934c490dc24937'), (6442562784, 6442563872, 'e69a2710de7096015ac7f703d2cf968916b68a0b9f352908f007f3403181cf4b')]
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={}
for a,z,h in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
pins={
 0x180025090:('call','r9'),0x1800250d3:('movzx','eax, word ptr [rdx + 4]'),
 0x1800250db:('cmp','ax, 1'),0x1800250e5:('mov','edx, 0x7ffc'),
 0x1800250ed:('jbe','0x180025125'),0x1800250f1:('cmp','al, 3'),
 0x1800250f5:('mov','byte ptr [rcx], 1'),0x18002510c:('movzx','eax, word ptr [rax + 0x4c]'),
 0x180025116:('cmp','word ptr [rdx + 8], ax'),0x180025174:('call','r10'),
 0x180025252:('mov','r10, qword ptr [rax + 0xc0]'),
 0x180025259:('mov','rax, qword ptr [rbp + 0x77]'),
 0x18002525d:('mov','r9d, dword ptr [rax + 0x28]'),
 0x180025261:('mov','r8, qword ptr [rax + 0x20]'),0x180025265:('call','r10'),
 0x1800252a0:('mov','dword ptr [rbp - 0x21], r8d'),
 0x180025612:('call','0x18001b4e0'),0x180025623:('movzx','eax, byte ptr [rbp - 0x65]'),
 0x180025643:('je','0x1800256b2'),0x180025675:('call','0x1800213a0'),
 0x18002569e:('call','0x180024d40'),0x1800256d8:('call','0x180021760')}
for a,v in pins.items():assert ins[a]==v,(hex(a),ins.get(a))
outer=f['180024f30'];source=f['18005f050']
assert 'local_78 = param_3[2];'in outer
assert '(param_1,*(undefined8 *)(local_78 + 0x20),*(undefined4 *)(local_78 + 0x28),'in outer
assert 'param_2[2] = lVar1;'in source and '*(undefined8 *)(param_3 + 0x40)'in source
assert '*(undefined8 *)(param_5 + 0x20)'in outer
assert outer.index('*(undefined8 *)(param_5 + 0x20)')<outer.index('while (uVar23 = uVar10, cVar7')
assert outer.index('FUN_18001b4e0')<outer.index('FUN_180021760')<outer.index('FUN_1800213a0')
assert 'FUN_1800213a0(param_1,param_2,param_3,'in outer
completion=(root/'src/InkRecognition/LegacyFreshMainGlobalNoCandidateSourceCompletion.cs').read_text()
for term in ['var sourceRanges=Snapshot.SourcePolicy.Owner.Acoustic.Geometry.Context.Source;',
             'sourceRanges.ResolveForwardWithResiduals((uint)Word.Span.First,(uint)Word.Span.Last,token)',
             'var doc=scope.Document;var map=new LegacyNativeTextSourceInputMap(doc.CopySelectedInputs(),[],0);',
             'LegacyNativeTextSourcePositionPublication.Project(map,projection,true,original,token)',
             'references=filled;ranges=positions;',
             'public LegacyNativeInkPositionRange[] CopyRanges(){Validate(default);return ranges.ToArray();}']:
 assert term in completion,term
components=[];saved=sys.argv
for name in ['audit_main_text_source_wrapper.py','audit_main_text_source_interner.py','audit_main_text_glyph_reuse.py','audit_fresh_text_source_context.py','audit_global_text_glyph_choices_runtime.py']:
 p=Path(__file__).with_name(name);out=target.with_name(target.stem+'.'+name[6:-3]+'.json');sys.argv=[str(p),str(out)]
 runpy.run_path(str(p),run_name='__main__');components.append(dict(script=name,sha256=hashlib.sha256(out.read_bytes()).hexdigest()))
sys.argv=saved
report=dict(vendorExecution=False,runtimeImplemented=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(v)for a,v in pins.items()},components=components,
 sourceRhs='Two distinct proven operands: first-pass intersection RHS is actual completed WORD source26N projected ranges (param5+20/count28); degenerate Contains118 RHS is full original source collection (param3[2]+20/count28), copied by05F050 from selected source+40. Raw full-root construction is a bounded library adaptation requiring exact full selected domain, never a general C22=collection identity.',
 interleaving='Original descriptor snapshot immediately before EACH own refresh; ALL eligible refs refresh/project/clip before any select. Then per glyph select -> sourceprocess -> same3E/sourceinterner -> NEW213A0+024D40 OR existing21760; never emit all first.',
 typedReceipt='NEW sealed global processed-source receipt retains exact wrapper/scope/Document/Seed/shared3E/completion/selection/V140/WORDclip/originaldomain. No production Prepared node or caller positive flag.',
 seams=['LegacyFreshMainGlobalTextGlyphChoices.cs internal typed claim/firstpass/select/freshEmit/fault joins, publicPublish unchanged',
 'LegacyFreshMainGlobalGlyphDocumentPositions.cs same projection context full-domain snapshot accessor',
 'LegacyOwnedMainTextSourceInterner.cs named actual global receipt entry and explicit global provenance sidecar; old getters strict',
 'NEW LegacyOwnedFreshTextSourceContext.Global.cs partial with named validated global CHAR registration -> existing privateCreateCore',
 'LegacyPreparedMainTextGlyphReuse.cs named internal global factory/ApplyGlobal, exact existing Process body unchanged, old Emission/API strict'],
 nextScope='one actual forward ordinary WORD, fresh selected input, same global scope/preNN3E/Seed; no WORD/TEXT registration or document acceptance yet')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print('PASS5 native regions/22 exact instructions/5 dependency closures; typed global024F30 next proposal only')
