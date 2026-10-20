"""Actual global post026EF0 counter owner and unchanged07AAF0 append composition."""
from pathlib import Path
import argparse,hashlib,json,runpy,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[2];dep=a.output.with_name(a.output.stem+'-native.json');saved=sys.argv
try:
 sys.argv=['audit_document_counter_handoff.py',str(dep)];runpy.run_path(str(a.native_repo/'tools/handwriting/audit_document_counter_handoff.py'),run_name='__main__')
finally:sys.argv=saved
s=(root/'src/InkRecognition/LegacyFreshMainGlobalCounterHandoff.cs').read_text(encoding='utf8');kernel=root/'src/InkRecognition/LegacyFreshMainDocumentCounterHandoff.cs'
rules=['public sealed partial class LegacyOwnedFreshTextSourceContext',
 'transforms.ValidateCurrent(token);var parent=transforms.Cleanup.State.Publication',
 'ReferenceEquals(transforms.Seed.PendingSelections,this)','ReferenceEquals(InputBinding,transforms.Seed.InputBinding)',
 'var prefix=parent.CopyPendingSelections();','initial=transforms.Seed.InputBinding.Next',
 'ReferenceEquals(prefix[^1],parent.Text.Selection)','initial<0||expectedNext<initial',
 'if(failed||next!=expectedNext||commands.Count!=prefix.Count','ReferenceEquals(c,prefix[i])',
 'return AppendCounterCore(initial,next,preparedAllocationFailure,token)',
 'FinalPendingCounter=append.Command?.NewCounter18??InitialPendingCounter',
 'result.ValidateCurrent(token);return result','TransactionOwner.NextSelectionId!=FinalPendingCounter',
 'ReferenceEquals(c.Selection,Commands[i].Selection)','ReferenceEquals(c.Counter,Commands[i].Counter)',
 'c.NextCommandIdentity!=Commands[i].NextCommandIdentity']
for rule in rules:assert rule in s,rule
assert 'LegacyFreshMainDocumentInputTransforms'not in s
normalized=kernel.read_text(encoding='utf-8-sig').replace('\r\n','\n').encode()
assert b'private LegacyPendingCounterAppend AppendCounterCore('in normalized
native=kernel.read_bytes();assert native==(a.native_repo/'src/InkRecognition/LegacyFreshMainDocumentCounterHandoff.cs').read_bytes()
a.output.write_text(json.dumps(dict(status='PASS actual global pending-only07AAF0 handoff',contracts=len(rules)+2,unchangedNumericOwnerSha256=hashlib.sha256(normalized).hexdigest(),nativeProofSha256=hashlib.sha256(dep.read_bytes()).hexdigest(),vendorExecution=False,notExecuted=['C20VF8','selected registry replay','2E7','DOM adoption','original acceptance']),indent=2)+'\n')
print('PASS genuine global source counter/prefix ownership and unchanged private07AAF0 numeric append; no old configured owner.')
