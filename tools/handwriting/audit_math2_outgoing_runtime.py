"""Static native transfer evidence plus bounded library runtime consumption.
No vendor DLL execution or original-page acceptance claim.
"""
from pathlib import Path
import argparse,json,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,default=Path(__file__).resolve().parents[2]);a=p.parse_args()
root=Path(__file__).resolve().parents[2]
proof=Path(str(a.output)+'.native.json')
subprocess.run([sys.executable,str(Path(__file__).with_name('audit_math2_original_transfer.py')),str(proof),'--native-repo',str(a.native_repo)],check=True)
publisher=(root/'src/InkRecognition.Math/Math2Mode0FreshMathPublisher.cs').read_text(encoding='utf8')
runtime=(root/'src/InkRecognition.Math/Math2Mode0OutgoingTransfer.cs').read_text(encoding='utf8')
probe=(root/'tools/LegacyMathOutgoingTransfer.Probe/Program.cs').read_text(encoding='utf8')
accessor=publisher[publisher.index('internal T WithCompletedTransferInput'):publisher.index('private void Cleanup()')]
assert accessor.index('receipt.Validate(')<accessor.index('WithFreshPublicationInput(')<accessor.index('lock(gate)')
assert accessor.count('receipt.Validate(')==1
rules=[(accessor,t)for t in('input.Check(cancellation)','!complete||failed||headerLive','receipt.Field.OriginalInkIdentity','thirdInk','temporary.Names','temporary.Nodes.Count','registry,next,reservation,Check')]
rules += [(runtime,t)for t in('ConditionalWeakTable<Math2Mode0FreshMathPublisher,Claim>','publisher.WithCompletedTransferInput(input=>','Math2Mode0OutgoingKernel.Plan(input,budget,token)','Interlocked.CompareExchange(ref claim.Entered,1,0)','ReferenceEquals(originalInk,input.OriginalInk)','ReferenceEquals(selectedInk,input.Field.SelectedInk)','ReferenceEquals(thirdInk,input.ThirdInk)','ReferenceEquals(input.Names,input.Field.Names)','input.NodeCount is<1 or>65536','++depth>256','32L*1024*1024','Actual selected prefix changed.','Fresh actual field INPUT prefix/result required.','plan.Children.TryGetValue','names.Intern(source.Name)','Origin=source.Id','dom=new(2,[],ordinal,payloadRoot,names,writer);domExported=true','OrderBy(r=>r.Id)','entry.Ranges.ToArray()','input.Field.Next<input.FinalNext','input.Field.Next,input.FinalNext,false','source=new(commands);sourceExported=true','pending=new(-1,-1,dom,null,source);handedOff=true','target.failed=true','if(observer is null)target.Dispose()','commands.Select(c=>c with{Ranges=c.Ranges.ToArray()})','if(released)return;released=true;cleanupCount++','LegacyOwnedRawInkRangeOperations.Contains(input.Field.InputRanges,range)','actualTags.Count!=input.Registry.Count-1','planned>budget')]
rules += [(probe,t)for t in('new[]{1,3,16}','foreach(var step in events)','new[]{false,true}','Concurrent one-use claim.','same.CopyResult().Reservation!=exact','Outgoing transfer modified retained publisher.','Outgoing snapshot aliases retained payload.','Closure crossed mutation boundary.','p.Receipt.Validate(p.Receipt.Copy,p.Field)')]
for source,rule in rules:assert rule in source,rule
assert runtime.index('Math2Mode0OutgoingKernel.Plan(input,budget,token)')<runtime.index('Interlocked.CompareExchange')<runtime.index('target.Build(')
assert runtime.index('dom=new(2,[],ordinal,payloadRoot,names,writer);domExported=true')<runtime.index('foreach(var entry in input.Registry')<runtime.index('source=new(commands);sourceExported=true')<runtime.index('pending=new(-1,-1,dom,null,source);handedOff=true')
assert 'LegacyOwnedFreshTextSelection'not in runtime and 'LegacyLibraryOriginalSourceHistory'not in runtime
report=dict(native=json.loads(proof.read_text()),consumptionChecks=len(rules)+4,vendorExecution=False,scope='Owning outgoing library transfer only; no original source or DOM mutation')
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(f"PASS {report['consumptionChecks']} runtime ownership/order checks +native transfer proof; no original-page acceptance.")
