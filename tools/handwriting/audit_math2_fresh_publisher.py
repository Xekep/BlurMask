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
# The missing-ID success branch is separate from the Document CB8 bodies.
# Authenticate its frozen path against the pinned Ink PE without executing it.
sys.path.insert(0,str(native/'research/myscript/scripts'))
from inspect_text_features import Binary
absence=json.loads((root/'tools/handwriting/ports/cb8-owner-20261028/native-absent-removal.json').read_text())
ink=Binary('MyScriptInk.dll')
assert not absence['vendorExecution'] and hashlib.sha256(ink.data).hexdigest()==absence['binarySha256']
for instruction in absence['instructions']:
 address=int(instruction['va'],16);expected=bytes.fromhex(instruction['bytes'])
 assert ink.data[ink.off(address):ink.off(address)+len(expected)]==expected,instruction
assert any(i['va']=='0x180069668' and i['bytes']=='b001' for i in absence['instructions'])
checks={
 'src/InkRecognition.Math/Math2Mode0OriginalMathFieldSource.cs':[
  'internal T WithFreshPublicationInput<T>(', 'lock (gate)', 'ReferenceEquals(receipt.Field, this)',
  'Validate(activeBinding, this); receipt.Copy.ValidateForOuterCopy(cancellation)',
  'selectedInk.Registry.Count != 1 || selectedInk.Next != 1',
  'entry.Category != 102', 'entry.Name != "INPUT"', 'ReferenceEquals(entry.C22, selectedC22)',
  'entry.C22.Ranges.SequenceEqual(domain)', 'selected.Any(n => n.Name == "mathResult")',
  'selected.SingleOrDefault(n => n.Kind == 3 && n.Name == "mathField"',
  'a.StringValue == FieldId', 'a.Parent == input.Id && a.ValueTag == 4 && a.NumericValue == entry.Id',
  'selectedInk.Next, entry.C22.Ranges, selectedInk, () => { Check(token); return CloneSelectedInk(); }, Check)',
  'private sealed class RetainedInk(Math2Mode0OriginalMathFieldSource sourceField, InkOwner ink)',
  'private Math2Mode0RetainedInk CloneSelectedInk()',
  'var clone = new InkOwner(Document) { Next = selectedInk.Next }',
  'ReferenceEquals(entry.Owner, selectedInk)', 'ReferenceEquals(entry.C22.Owner, selectedInk)',
  'new Selection(clone, entry.C22.Ranges.ToArray(), entry.C22.Storage is null)',
  'new(entry.Id, entry.Name, entry.Category, clone, selection)',
  'Monitor.IsEntered(sourceField.gate)', 'ReferenceEquals(actual.Owner, ink)',
  'ReferenceEquals(actual.Owner, Names)', 'LegacyOwnedRawInkRangeOperations.Equal',
  'ink.Registry.Remove(id); return true;',
  'ink.Registry.Add(id, new(id, canonicalName.Text, 102, ink, canonical))'],
 'src/InkRecognition.Math/Math2Mode0RawOnlySource.cs':[
  'PrepareWithPreparedSearchAndSelectedMathField(',
  'PrepareCore(runtime, bundle, run, actualField.Document.Strokes, width, true, actualField, token)'],
 'src/InkRecognition.Math/Math2Mode0FreshMathPublisher.cs':[
  'ConditionalWeakTable<Math2Mode0SelectedMathFieldCopy, Claim>',
  'receipt.Validate(receipt.Copy,receipt.Field,token)', 'receipt.Field.WithFreshPublicationInput(receipt,input =>',
  'receipt.Copy.WithSymbolRowsForOuterCopy(', 'rows.Length is <1 or >65535',
  'if(planned>budget)', 'Interlocked.CompareExchange(ref claim.Entered,1,0)',
  'Math2Mode0OuterSymbolVector.CreateSingle(receipt.Copy,token)',
  'retainedInk=input.CloneSelectedInk()',
  'ReferenceEquals(retainedInk.Identity,input.SelectedInk)',
  '!ReferenceEquals(retainedInk.Names,input.Names)||retainedInk.SnapshotNext!=input.Next',
  'candidate=new(input.Names);temporary=new(input.Names)', 'Encoding.UTF8.GetByteCount(text)<16',
  'var tRoot=temporary.New(4,"");var tField=CloneSelected(input.FieldIdentity);Arena.Attach(tRoot,tField)',
  'text.Length==0?(byte)3', 'long id=temporaryNext++',
  'pending.Count==failSelection?null:retainedInk!.CreateSelection(ranges)', 'pending.Add(p)',
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
  'pending.SingleOrDefault(p=>p.Id==-tag)', 'long id=retainedInk!.Register(p.Name,p.Selection);p.Resolved=id',
  'retainedInk?.CopyRegistry()??[]', 'retainedInk.BorrowRegistry(),retainedInk.SnapshotNext',
  'retainedInk?.Dispose()',
  'var pendingView=new PendingView(pending)',
  'entries[entries.Count-1-index]',
  'Math2DomCb8EditMatcher.Matches(2,action.Parent.Kind,null,action.Parent.Name,tagName',
  'default,default,pendingView,retainedInk!,token:token)',
  'Math2DomCb8TagCleanup.RemoveStale(CollectionsMarshal.AsSpan(oldTags),CollectionsMarshal.AsSpan(newTags),retainedInk!,token)',
  'attribute.Tag!=4||attribute.Number is not long actualTag',
  'long tag=Math2DomValueConversion.RegistrationTag(actualTag)',
  'attribute.Number=p.Resolved', 'newTags.Add(Math2DomValueConversion.CollectedTag(attribute.Number!.Value))',
  'if(oldTags.Count!=0)', 'outer?.Dispose()', 'catch{target.failed=true;if(observer is null)target.Dispose();throw;}'],
 'src/InkRecognition.Math/Math2DomValueConversion.cs':[
  'internal static long Signed64(long actualValue) => actualValue',
  'internal static long RegistrationTag(long actualValue)',
  'if (tag == long.MaxValue) throw new NotSupportedException(',
  'internal static long CollectedTag(long actualValue)',
  'if (tag == -1) throw new NotSupportedException(']}
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
assert publisher.index('attribute.Tag!=4||attribute.Number is not long actualTag')<publisher.index('Math2DomValueConversion.RegistrationTag(actualTag)')<publisher.index('pending.SingleOrDefault')
assert 'receipt.Dispose('not in publisher and 'receipt.Copy.Dispose('not in publisher
assert 'LegacyLibraryOriginalTextFieldBootstrap'not in publisher
a.output.write_text(json.dumps(dict(staticOnly=True,vendorExecution=False,nativeReport=str(report),nativeRegions=18,nativePins=435,nativeBindings=17,consumptionPins=pins,orderedJoins=6,
 absentRemovalInstructions=len(absence['instructions']),inkSha256=absence['binarySha256'],
 runtimeSha256={p:hashlib.sha256((root/p).read_bytes()).hexdigest()for p in checks},
 scope='Library actual fresh produced receipt/third registry/candidate/temp DOM/3op2 cloned ordered patch/late CB8 tags; same-field locks without field->receipt inversion, oneuse CWT, borrowed firstcopy. No native edit ABI or original-page/history acceptance.'),indent=2)+'\n',encoding='utf8')
print(f'PASS18 complete native patch bodies/435pins/17bindings +{pins} runtimeconsumption checks/6orderedjoins; no vendor execution')
