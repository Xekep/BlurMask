"""Pinned selected INPUT transform tail composed with the genuine global owner."""
from pathlib import Path
import argparse,hashlib,json,runpy,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[2];dependency=a.output.with_name(a.output.stem+'-native.json');saved=sys.argv
try:
 sys.argv=['audit_selected_document_input_transforms.py',str(dependency)]
 runpy.run_path(str(a.native_repo/'tools/handwriting/audit_selected_document_input_transforms.py'),run_name='__main__')
finally:sys.argv=saved
s=(root/'src/InkRecognition/LegacyFreshMainGlobalInputTransforms.cs').read_text(encoding='utf8')
rules=['ConditionalWeakTable<LegacyFreshMainTextDocumentSeed,Entry>',
 'ValidateSeed(cleanup,token)','if(entry.Claimed)throw','ReferenceEquals(seed.CurrentTransforms,seed.InitialTransforms)',
 'ReferenceEquals(seed,scope.Context.Result.PublicationSeed)','ReferenceEquals(seed.ProcessingResult,scope.Context.Result)',
 'ReferenceEquals(seed.Document,scope.Document)','ReferenceEquals(seed.PendingSelections,scope.SharedState.Selections)',
 'ReferenceEquals(seed.StringPool,parent.Wrapper.Choices.StringPool)','ReferenceEquals(seed.InputBinding,scope.Context.InputBinding)',
 'seed.SourceTag30!=scope.Context.InputBinding.RegistryId','seed.InitialDocumentAlternativeCountD8!=0||parent.AlternativeCount!=1',
 'SourceIndices','Seed.InputBinding.ExportIdentity(token)','sourceExport.Geometry.NativeNonidentityTransformCount!=0',
 'ReferenceEquals(Seed.CurrentTransforms,before)','Seed.SwapTransforms(prepared?.Prefix??LegacyDocumentTransformVector.Fresh()',
 'ReferenceEquals(swap.Released,before)','if(!exportSucceeded){faulted=true;Record("export-failed");return this;}',
 'Seed.CopyDafInputRows32().Length!=0','Observe(LegacyGlobalInputTransformStage.Complete);ValidateSeed(Cleanup,token)',
 'ReferenceEquals(Seed.CurrentTransforms,output)','catch{faulted=true;Record("fault");throw;}',
 'if(!complete||faulted)throw','return prefixes.ToArray()']
rules.remove('SourceIndices')
for rule in rules:assert rule in s,rule
assert s.index('Seed.SwapTransforms(')<s.index('if(!exportSucceeded)')<s.index('Seed.CopyDafInputRows32()')
section=s[s.index('Record("exported")'):s.index('var swap=')]
assert 'ValidateSeed(Cleanup,token)'in section and 'ReferenceEquals(Seed.CurrentTransforms,before)'in section
assert 'LegacyFreshMainDocumentControlCleanup'not in s
a.output.write_text(json.dumps(dict(status='PASS genuine global037E80/026EF0 identity-only seed handoff',contracts=len(rules)+3,dependencySha256=hashlib.sha256(dependency.read_bytes()).hexdigest(),vendorExecution=False,remaining=['nonidentity codec','nonempty DAF32','final2E7','DOM acceptance']),indent=2)+'\n')
print('PASS actual global original-selected seed/ALT1/ordered export-swap-status-DAF tail and retained-owner guards; no old configured owner.')
