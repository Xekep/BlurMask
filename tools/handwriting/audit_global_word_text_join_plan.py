"""Static global CHAR -> WORD/TEXT proposal; no runtime or vendor execution.

Composes existing native proofs, pins the actual parent-before-child ordering
and records the real typed/document boundary. It does not approve runtime edits.
"""
import argparse,hashlib,json,runpy,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--capture',type=Path);a=p.parse_args()
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
 (0x1800283b0,0x180029990,'fa2576435b07665bfe72ead6e2da50229aadea857719d8c27ab507ef9b8ab195'),
 (0x180024f30,0x1800258d0,'108097dbb0b17e9102cae2b312d9f33501de7aae9a4fd1a053d05a3f1ba589fd'),
 (0x180021ab0,0x180021cd0,'1b3841a02c3bf5494258e2db1ede3d8bc047d8739b5a8a4d3e0e7270588c6540'),
 (0x1800210c0,0x18002139a,'6a2a59021b959b9179b0ae4ec01dce67d6f64597d619f7eabed22dee8d5cfc1b'),
 (0x180025e40,0x180026088,'66b6b63bb35b0734e9b47edbe8113fe135cd54c93c6db4f6c39184edf94af326'),
 (0x180025940,0x180025e33,'bd33ccf981b7f23a04af03b4366a3dbf1499618b8028a9e0cf7df133c90ab8be')]
ins={}
for s,e,h in regions:
 raw=b.data[b.off(s):b.off(e)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,s)})
pins={
 0x180028e25:('call','0x18001b4e0'),0x180028e4f:('lea','rdi, [rax + 0x18]'),
 0x180028e55:('je','0x180029158'),0x180028e62:('mov','qword ptr [rdi + 0x50], rdx'),
 0x180028e66:('mov','qword ptr [rdi + 0x58], 0'),0x180028e75:('mov','qword ptr [rdi + 0x18], 0'),
 0x180028e7d:('mov','dword ptr [rdi + 8], 0xffffffff'),
 0x180028e9c:('movabs','rax, 0x7fffffffffffffff'),0x180028eaf:('mov','qword ptr [rdi + 0x10], rax'),
 0x180028eb7:('mov','qword ptr [rdi + 0x40], 0xffffffffffffffff'),
 0x180028ebf:('mov','dword ptr [rdi + 0x48], 0xffffffff'),
 0x180028f94:('call','0x18025ee50'),0x180028f99:('add','dword ptr [rdi + 0x58], r15d'),
 0x180028fa0:('btr','eax, 0x17'),0x180028fa7:('bts','eax, 0x15'),
 0x18002901d:('call','0x180024f30'),0x180029031:('add','r15, 0x30'),0x180029035:('add','r14, 0x78'),
 0x180029080:('or','eax, dword ptr [rcx]'),0x180029082:('lea','rcx, [rcx + 0x30]'),
 0x18002908c:('or','eax, dword ptr [rdi + 0x18]'),0x18002909d:('bt','eax, 0x15'),
 0x1800290ac:('mov','qword ptr [rcx], 0xffffffffffffffff'),0x1800290bf:('lea','rbx, [rdi + 0x20]'),
 0x1800290cf:('mov','qword ptr [rsp + 0x20], rbx'),0x1800290d4:('mov','r9d, 1'),
 0x1800290da:('call','0x1800210c0'),0x1800290e5:('cmp','dword ptr [rbx + 8], 1'),
 0x180029123:('cmp','r9b, r10b'),0x180029128:('cmp','r9b, 0xff'),0x18002912e:('cmp','r10b, 0xff'),
 0x180029134:('mov','ecx, 0x800000'),0x180029145:('btr','eax, 0x17'),
 0x180029316:('mov','dword ptr [rbp + 0xb8], r12d'),0x180029329:('mov','byte ptr [rbp + 0xbc], 0'),
 0x18002933f:('or','edx, dword ptr [rcx + rax + 8]'),0x180029343:('lea','rax, [r9 + 0x18]'),
 0x180029347:('and','edx, 0x3ff'),0x18002934d:('mov','qword ptr [rbp + 0xb0], rax'),
 0x18002936b:('mov','r8d, 0x18'),0x1800293d1:('inc','dword ptr [rdi + rsi + 0x18]'),
 0x180029411:('call','0x180021ab0'),0x18002960c:('lea','rcx, [r14 + 0xc0]'),
 0x180029624:('mov','r9d, 2'),0x18002962a:('call','0x1800210c0'),0x180029647:('call','0x180025e40'),
 0x1800257d7:('mov','eax, dword ptr [rdx + 0x40]'),0x1800257dc:('jns','0x1800257e6'),
 0x1800257de:('mov','eax, r14d'),0x1800257f9:('or','dword ptr [r12 + 8], ecx'),
 0x18002581f:('btr','r8d, 0x14'),0x18002582f:('or','edx, dword ptr [rax + rcx*8 + 8]'),
 0x180025833:('and','edx, 0x100000'),0x180025839:('xor','edx, r8d')}
for at,value in pins.items():assert ins[at]==value,(hex(at),ins.get(at),value)
outer=f['1800283b0'];child=f['180024f30']
assert outer.index('FUN_18001b4e0')<outer.index('FUN_180024f30')<outer.index('FUN_180021ab0')<outer.index('FUN_180025e40')
assert '(longlong *)(lVar10 + 0x38)'in outer and '(undefined8 *)(lVar10 + 0x28)'in outer
assert 'FUN_1800210c0(param_1,local_c8,&local_128,2,uVar23,param_2 + 0xb0)'in outer
assert '*(int *)(lVar12 + 0x50) == 0'in child and '(lVar12 + 0x38)'in child and '(lVar12 + 0x48)'in child
dependencies={}
# Nested proof reports append their own descriptive suffixes. Keep this outer
# name bounded so the deepest report stays below Windows' component limit.
report_id=hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]
for dependency_index,name in enumerate(['audit_main_word_source_choices','audit_main_owned_source_lines','audit_main_document_text_selection',
             'audit_main_document_line_consistency','audit_global_text_source_wrapper_runtime']):
 path=a.output.with_name(f'gwt-{report_id}-{dependency_index}.json');saved=sys.argv
 try:sys.argv=[name,str(path)];runpy.run_path(str(Path(__file__).with_name(name+'.py')),run_name='__main__')
 finally:sys.argv=saved
 dependencies[name]=hashlib.sha256(path.read_bytes()).hexdigest()
def code(name):return(root/'src/InkRecognition'/name).read_text(encoding='utf-8-sig')
# Actual global producer and exact prior/default ownership, not prepared old owners.
contracts={
 'LegacyLibraryGlobalTextPublicationScope.cs':['context.PrepareInput(token);lease=context.RetainForPublication()',
 'scope=new(context,lease,models)','shared.BindActualGlobalTop(result.Session!.Ownership)'],
 'LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.cs':['ActiveAlternativeCount60 => 1','SourceNodeCount2310'],
 'LegacyFreshMainGlobalTextSourceWrapper.cs':['CharacterInterner.Count!=0','Choices.ClaimSourceWrapper(this,token)',
 'operations.Intersect(projected.CopyRanges(token),wordClip,token)','firstPassComplete=true',
 'CharacterInterner.InternGlobalProcessed(currentSource,token)','if(key.Inserted)',
 'Choices.EmitFreshForWrapper(currentSelection,token)','payload.Initialize(preparedCharFailure,token)',
 'payload.Reuse(currentSelection,token)','selectedFlags|=rows[marker].Flags'],
 'LegacyFreshMainGlobalTextSourcePayload.cs':['PriorRangeCount=>0','basic?-1L:null','SourceLineId=Owner.ReadSourceLine(Source,token)'],
 'LegacyFreshMainDescriptorCutSource.cs':['var descriptors=lines.CopyDescriptors();var guides=lines.CopyGuides()',
 'lines.Owner.Controller.SourceOffset!=0','CopyDescriptors()=>descriptors.Select(d=>d.ToArray()).ToArray()',
 'CopyGuides()=>guides.Select(g=>g.ToArray()).ToArray()'],
 'LegacyFreshMainTextDocumentSeed.cs':['InputBinding.RegistryId','PendingSelections=>shared.Selections',
 'ReferenceEquals(actualWords.SelectionContext,PendingSelections)'],
}
for name,needles in contracts.items():
 text=code(name)
 for needle in needles:assert needle in text,(name,needle)
# Real typed/configured2 blockers must remain visible; this proposal does not
# silently weaken those old gates to get a one-alternative global owner through.
blockers={
 'LegacyFreshMainWordSourceChoices.cs':['candidates.Length!=2','LegacyOwnedMainCandidateSource.FromConfigured'],
 'LegacyFreshMainSourceLinePublication.cs':['actualWordChoices.Alternatives.Count!=2','candidate.Sampled'],
 'LegacyFreshMainDocumentTextSelection.cs':['actualSourceLines.Alternatives.Count!=2','actualSourceLines.LineCount!=2'],
 'LegacyFreshMainDocumentLineConsistency.cs':['actualText.SourceLines.LineCount!=2','words.Owner.Sources.CandidateSource.Sampled'],
 'LegacyFreshMainDocumentSourceCoverage.cs':['words.Alternatives.Count!=2','LegacyOwnedRawInkRangeOperations.Equal(union,root)'],
 'LegacyFreshMainDocumentCharacterCoverage.cs':['LegacyOwnedMainTextSourcePayload Source','SourceNode.FirstProcessedSource'],
 'LegacyFreshMainDocumentSelectedSources.cs':['LegacyOwnedMainTextSourcePayload OriginalSource','LegacyOwnedMainWordSourcePayload OriginalWordSource'],
 'LegacyFreshMainDocumentSourceIndices.cs':['ConditionalWeakTable<LegacyFreshMainWordSourceChoices,Entry>','FirstProcessedSource?.CopyRanges()'],
 'LegacyFreshMainDocumentInputTransforms.cs':['if(alternatives!=2)','seed.Validate(owner,token)'],
 'LegacyFreshMainSourceRangeUtf8.cs':['LegacyOwnedMainTextSourcePayload? CharacterSource','LegacyOwnedMainWordSourcePayload? WordSource'],
 'LegacyFreshMainDetachedTextDom.cs':['words.Alternatives.Count!=2','Dictionary<LegacyOwnedMainTextSourcePayload'],
 'LegacyLibraryTextPublicationDocument.cs':['LegacyLibraryTextPublicationScope scope','LegacyOwnedMainWordSourceChoicesResult words'],
}
for name,needles in blockers.items():
 text=code(name)
 for needle in needles:assert needle in text,(name,needle)
evidence=None
if a.capture:
 d=json.loads(a.capture.read_bytes());assert d['success']==13 and d['refused']==d['actualReuse']==0
 equal=[];unequal=[]
 for sample in d['samples']:
  assert sample['stage']=='complete'
  (equal if sample['wordClip']==sample['original']else unequal).append(sample['name'])
 evidence=dict(sha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),actualSources=13,
   exactWordTextKeyMatches=equal,requiresUncoveredSourceBranch=unequal,naturalDuplicateKeys=0)
a.output.write_text(json.dumps(dict(status='STATIC PROPOSAL ONLY',vendorExecution=False,regions=len(regions),
 pins=len(pins),dependencyReports=dependencies,globalContracts=sum(map(len,contracts.values())),
 currentOldDocumentBlockers=blockers,naturalEvidence=evidence,
 proposedFirstBatchOldSeams=['LegacyFreshMainGlobalTextSourceWrapper.cs','LegacyOwnedMainTextSourceInterner.cs'],
 proposedNewSelectionPartial='LegacyOwnedFreshTextSourceContext.GlobalWordText.cs',
 firstBatchScope='native-order global1/oneWORD parent receipt -> actualCHAR -> WORD registration -> actual source lines -> NULL-projected TEXT; no existing document-route adoption',
 nextDocumentBoundary='requires separately reviewed typed contract and12 old concrete consumers; never manufacture configured2/count1 owners',
 missingBranches=['different-span WORD remapping','cached/prior WORD payload','238B0 uncovered sources',
 '025940/024A90 split/insertion','full existing document/DOM bridge','recognition quality']),indent=2)+'\n')
print(f'PASS static6 complete regions/{len(pins)} exact pins/5 native compositions; global WORD/TEXT owner proposal, actual document blockers explicit.')
