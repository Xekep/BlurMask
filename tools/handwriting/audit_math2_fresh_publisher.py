"""Static native prerequisites and narrow consuming fresh Math publisher joins.
No vendor execution; numerical output/DOM/prefix oracle is separate.
"""
from pathlib import Path
import argparse,hashlib,json,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,default=Path(__file__).resolve().parents[2]);a=p.parse_args()
root=Path(__file__).resolve().parents[2];native=root if a.native_repo is None else a.native_repo
report=Path(str(a.output)+'.native.json')
subprocess.run([sys.executable,str(native/'tools/handwriting/audit_math2_temporary_patch_application.py'),str(report),'--native-repo',str(native)],check=True)
proof=json.loads(report.read_text());assert proof['staticOnly']and not proof['vendorExecution']
assert len(proof['regions'])==18 and proof['pins']==435 and len(proof['bindings'])==17
checks={
 'src/InkRecognition.Math/Math2Mode0OriginalMathFieldSource.cs':[
  'internal T WithFreshPublicationInput<T>(', 'lock (gate)', 'ReferenceEquals(receipt.Field, this)',
  'Validate(activeBinding, this); receipt.Copy.ValidateForOuterCopy(cancellation)',
  'selectedInk.Registry.Count != 1 || selectedInk.Next != 1',
  'entry.Category != 102', 'entry.Name != "INPUT"', 'ReferenceEquals(entry.C22, selectedC22)',
  'entry.C22.Ranges.SequenceEqual(domain)', 'selected.Any(n => n.Name == "mathResult")',
  'selected.SingleOrDefault(n => n.Kind == 3 && n.Name == "mathField"',
  'a.StringValue == FieldId', 'a.Parent == input.Id && a.ValueTag == 4 && a.NumericValue == entry.Id',
  'selectedInk.Next, entry.C22.Ranges, selectedInk, Check)'],
 'src/InkRecognition.Math/Math2Mode0RawOnlySource.cs':[
  'PrepareWithPreparedSearchAndSelectedMathField(',
  'PrepareCore(runtime, bundle, run, actualField.Document.Strokes, width, true, actualField, token)'],
 'src/InkRecognition.Math/Math2Mode0FreshMathPublisher.cs':[
  'ConditionalWeakTable<Math2Mode0SelectedMathFieldCopy, Claim>',
  'receipt.Validate(receipt.Copy,receipt.Field,token)', 'receipt.Field.WithFreshPublicationInput(receipt,input =>',
  'receipt.Copy.WithSymbolRowsForOuterCopy(', 'rows.Length is <1 or >65535',
  'if(planned>budget)', 'Interlocked.CompareExchange(ref claim.Entered,1,0)',
  'Math2Mode0OuterSymbolVector.CreateSingle(receipt.Copy,token)',
  'registry.Add(new(input.InputId,"INPUT",102,input.InputRanges.ToArray()))',
  'candidate=new(input.Names);temporary=new(input.Names)', 'Encoding.UTF8.GetByteCount(text)<16',
  'var tRoot=temporary.New(4,"");var tField=CloneSelected(input.FieldIdentity);Arena.Attach(tRoot,tField)',
  'text.Length==0?(byte)3', 'long id=temporaryNext++',
  'pending.Count==failSelection?null:ranges.ToArray()', 'pending.Add(p)',
  'LegacyNativeSourceRangeUtf8.Format(ranges,operations,token:token)',
  'RegisterPending("MATH_SYMBOL",row.Ranges)', 'Attribute(candidate,symbol,"tag",4,number:-id)',
  'row.Alternatives[0].Text.ReadBytes()', 'new UTF8Encoding(false,true)',
  'Attribute(candidate,choice,"nrs",10,number:0)', 'question.Text="?"',
  'Attribute(candidate,terminal,"selected",7,number:0)', 'RegisterPending("MATH_NODE",input.InputRanges)',
  'result.Children.Count!=3||oldRoot.Children.Count!=0||oldRoot.Attributes.Count!=0',
  'var payload=temporary.Clone(child)', 'patch.Add(new(i,child,payload,oldRoot))',
  'Arena.Attach(action.Parent,action.Payload);action.Applied=true',
  'headerLive=false;Observe(Math2Mode0PublishStage.HeaderFreed)',
  'foreach(var node in Walk(oldRoot))foreach(var attribute in node.Attributes)',
  'pending.SingleOrDefault(p=>p.Id==-tag)', 'long id=next++;registry.Add(new(id,p.Name,102,p.Ranges.ToArray()));p.Resolved=id',
  'attribute.Number=p.Resolved', 'newTags.Add(attribute.Number!.Value)',
  'if(oldTags.Count!=0)', 'outer?.Dispose()', 'catch{target.failed=true;if(observer is null)target.Dispose();throw;}']}
text={};pins=0
for path,expressions in checks.items():
 s=(root/path).read_text(encoding='utf-8-sig');text[path]=s
 for expression in expressions:assert expression in s,(path,expression);pins+=1
field=text['src/InkRecognition.Math/Math2Mode0OriginalMathFieldSource.cs']
seam=field.split('internal T WithFreshPublicationInput<T>')[1].split('private void CheckScope()')[0]
assert 'receipt.Validate('not in seam and 'receipt.Dispose('not in seam
publisher=text['src/InkRecognition.Math/Math2Mode0FreshMathPublisher.cs']
assert publisher.index('receipt.Validate(receipt.Copy,receipt.Field,token)')<publisher.index('receipt.Field.WithFreshPublicationInput')
assert publisher.index('if(planned>budget)')<publisher.index('Interlocked.CompareExchange')<publisher.index('target.Build(')
assert publisher.index('var payload=temporary.Clone(child)')<publisher.index('patch.Add(new(i,child,payload,oldRoot))')<publisher.index('foreach(var action in patch)')
assert publisher.index('Arena.Attach(action.Parent,action.Payload)')<publisher.index('headerLive=false;Observe(Math2Mode0PublishStage.HeaderFreed)')<publisher.index('pending.SingleOrDefault')
assert publisher.index('p.Resolved=id;')<publisher.index('Math2Mode0PublishStage.Registered,')<publisher.index('attribute.Number=p.Resolved')
assert 'receipt.Dispose('not in publisher and 'receipt.Copy.Dispose('not in publisher
assert 'LegacyLibraryOriginalTextFieldBootstrap'not in publisher
a.output.write_text(json.dumps(dict(staticOnly=True,vendorExecution=False,nativeReport=str(report),nativeRegions=18,nativePins=435,nativeBindings=17,consumptionPins=pins,orderedJoins=5,
 runtimeSha256={p:hashlib.sha256((root/p).read_bytes()).hexdigest()for p in checks},
 scope='Library actual fresh produced receipt/third registry/candidate/temp DOM/3op2 cloned ordered patch/late CB8 tags; same-field locks without field->receipt inversion, oneuse CWT, borrowed firstcopy. No native edit ABI or original-page/history acceptance.'),indent=2)+'\n',encoding='utf8')
print(f'PASS18 complete native patch bodies/435pins/17bindings +{pins} runtimeconsumption checks/5orderedjoins; no vendor execution')
