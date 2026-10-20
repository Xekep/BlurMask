"""Actual original FIELD runtime ownership and full Ghidra body-range closure.
Static analysis only: vendor DLLs are never loaded or executed.
"""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path)
p.add_argument('--native-repo',type=Path,default=Path(__file__).resolve().parents[2]);a=p.parse_args()
root=Path(__file__).resolve().parents[2];native=a.native_repo.resolve()
sys.path.insert(0,str(native/'research/myscript/scripts'))
from inspect_text_features import Binary
a.output.parent.mkdir(parents=True,exist_ok=True)
dep=Path(str(a.output)+'.field.json')
subprocess.run([sys.executable,str(Path(__file__).with_name('audit_original_field_dom_join.py')),str(dep),'--native-repo',str(native)],cwd=root,check=True)
BODIES=[('MyScriptDocument.dll',
  6442504384,
  6442505525,
  1141,
  '84ae59f96b88d8ac8bbc037ed4920defb09ca5bb32eca0827d2280e9d6fddcb3'),
 ('MyScriptDocument.dll',
  6442512656,
  6442514199,
  1543,
  'ff9af4627202cfcdc8684044cf3520a9fe07cfac5ff790e935f2a0b310a5315d'),
 ('MyScriptDocument.dll',
  6443587584,
  6443589407,
  1823,
  'd2359a9a06ccfd2d6a88cfd883aeccbeae59ca885c9ca3d601b459410c5f3ecd'),
 ('MyScriptDocument.dll',
  6443587296,
  6443587572,
  276,
  'e824bb2f221a86289456c5f79a482a2725ffc5b95f17d6ce412ba6913c1f19dd'),
 ('MyScriptEngine.dll',
  6443378800,
  6443380076,
  1276,
  '5c479b6cd87e9dd0a33873835091853492c559791431816c44b86e40e781a230'),
 ('MyScriptEngine.dll',
  6443380080,
  6443381986,
  1906,
  'fd423dd09ec4a41f78cb6e57ae4a305f04191f3fa722bae6cae851f9417957a9'),
 ('MyScriptEngine.dll',
  6443390704,
  6443390884,
  180,
  '01340bcbc53c0181818ebb0871482de1e41d6cde21b9d30295a997ee8cafa991'),
 ('MyScriptEngine.dll',
  6443432560,
  6443432711,
  151,
  '31eef48380075d4fa3740ea4ac258c366451cfac70218c2b477b7d3f079a53ee'),
 ('MyScriptEngine.dll',
  6443432720,
  6443432874,
  154,
  'c421a4854709890ec1ad795645bb8a81839412b87efa146a7dc23a3c6fe71815'),
 ('MyScriptText.dll',
  6442849552,
  6442849917,
  365,
  '35597ecfa0c5a04e8634babd586bfe7548bdc318d06c63d50d9e65d4db59a937')]
body_report=[]
for dll,start,end,count,sha in BODIES:
 b=Binary(dll);raw=b.data[b.off(start):b.off(end)]
 assert end-start==count and hashlib.sha256(raw).hexdigest()==sha,(dll,hex(start))
 body_report.append(dict(dll=dll,start=hex(start),endExclusive=hex(end),bytes=count,sha256=sha))
def read(name):return(root/'src/InkRecognition'/name).read_text(encoding='utf8')
history=read('LegacyLibraryOriginalSourceHistory.cs');document=read('LegacyLibraryTextPublicationDocument.cs')
owner=read('LegacyLibraryOriginalFieldDom.cs');kernel=read('LegacyOriginalFieldDomKernel.cs')
assert history.index('Owner.FieldDom?.ValidateBeforeSourceRun(this,token)')<history.index('entered=true;kernel.Trace.Add("library-source-frame:begin")')
assert history.index('Owner.FieldDom?.ValidateSourceRollback(token)')<history.index('rollbackEntered=true;kernel.Recording=false;')
assert 'if(selected is not null||fieldDom is not null)'in history
assert 'ReferenceEquals(fieldDom.Input,actualInput)'in history
assert 'ReferenceEquals(originalSourceApplication,actual.SourceApplication)'in document
assert document.index('originalFieldDom is not null&&!originalFieldDom.State.Succeeded')<document.index('applied=target.ApplyPrepared')
assert 'handoff is null||applied is not null'in document # Existing kind4 validation remains strict.
assert owner.index('LegacyOriginalFieldDomKernel.MeasurePayload(payload,token)')<owner.index('state=Capture(false,true,false,false,[])')
assert 'kernel.Nodes.Count+fieldCount+2L*size.Nodes'in owner
pre=owner[owner.index('private LegacyLibraryOriginalFieldDomPublicationState PreflightCore'):owner.index('public LegacyLibraryOriginalFieldDomPublicationState Apply(')]
assert pre.index('scratch=kernel.CloneField(token)')<pre.index('kernel.Step("map-all-parents",token)')<pre.index('kernel.ClonePayload(payload,token)')<pre.index('kernel.Append(scratch')
live=owner[owner.index('private LegacyLibraryOriginalFieldDomPublicationState ApplyCore'):owner.index('internal static LegacyLibraryOriginalFieldDomIdentityPair[]')]
assert live.index('SourceApplication.State.IsSafeForLibraryApply')<live.index('ValidateIdentityMap(Owner,SourceApplication,registry,token)')<live.index('applyEntered=true')<live.index('kernel.ClonePayload(payload,token)')<live.index('kernel.Append(kernel.Field')<live.index('ValidateTags(kernel.Field')
assert 'fieldDepth+1+size.Depth>LegacyOriginalFieldDomKernel.MaxDepth'in owner
assert 'for(var parent=kernel.Field.Parent;parent is not null;parent=parent.Parent)'in owner
mapping=owner[owner.index('internal static LegacyLibraryOriginalFieldDomIdentityPair[] ValidateIdentityMap'):owner.index('internal static void ValidateTags')]
assert mapping.count('token.ThrowIfCancellationRequested()')==3
for pin in ['e.RegistryId!=source.SelectionId','ReferenceEquals(e.SourceCommand,source)','e.CopyRanges().AsSpan().SequenceEqual(source.CopyRanges())','LegacyOwnedRawInkRangeOperations.Contains(domain,r)','ChildOrdinal=checked((uint)kernel.Field.Children.Count)','SourceApplication.State.HasEntered']:
 assert pin in owner,pin
assert 'a.Tag!=4||a.Value.Length!=8'in kernel and 'SourceName="./page/content/textField[@id='in owner
assert 'Check(original.RootIdentity,0,0)'in kernel and 'if(seen.Count!=source.Count)'in kernel
report=dict(completeGhidraBodies=body_report,dependency=json.loads(dep.read_bytes()),vendorExecution=False,
 scope='bounded explicit library original FIELD publication; genuine payload/source identities, preflight before source Run, kind3 scratch and full-PAGE overlay; no CBB/revision/CB7/native combined rollback')
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print('PASS10 full contiguous Ghidra body hashes +10-region/8-binding/147-pin/5-literal native join and original FIELD runtime ownership/order gates; no vendor execution.')
