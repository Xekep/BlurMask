"""Native selected-C22 continuation plus consumption/lifetime joins; no DLL execution."""
import argparse, hashlib, json, subprocess, sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('output',type=Path)
p.add_argument('--native-repo',type=Path,default=root)
a=p.parse_args()
dependency=Path(str(a.output)+'.native.json')
subprocess.run([sys.executable,str(a.native_repo/'tools/handwriting/audit_original_selected_continuation.py'),str(dependency)],cwd=a.native_repo,check=True)
checks={
 'LegacyLibrarySelectedInputBinding.cs':[
  'OriginalSelectedEntry => OriginalSelected?.State.Registry.Single(e => e.RegistryId == RegistryId)',
  'OriginalSelected!.State.Registry.Select(e => e.RegistryId).ToArray()',
  'ReferenceEquals(entry.OriginalEntry, selected.OriginalInput)',
  'selected.ExportInputGeometry(entry, token)',
 ],
 'LegacyFreshMainDocumentInputTransforms.cs':[
  'public static LegacyFreshMainDocumentInputTransforms ApplyOriginalSelected(',
  'var export=seed.InputBinding.ExportIdentity(token)',
  'ReferenceEquals(export.Binding,seed.InputBinding)',
  'sourceExport.OriginalSelectedInput',
  'var swapped=seed.SwapTransforms(',
 ],
 'LegacyFreshMainDocumentCounterHandoff.cs':[
  'AppendCounterCore(actualTransforms.Seed.InputBinding.Next,next,false,token)',
  'InitialPendingCounter=>Transforms.Seed.InputBinding.Next',
 ],
 'LegacyLibraryTextDomTransportCache.cs':[
  'ReferenceEquals(incoming.InputBinding,InputBinding)',
  'ReferenceEquals(transaction.InputBinding,source.Seed.InputBinding)',
 ],
 'LegacyLibrarySelectedInputRegistryReplay.cs':[
  'InputBinding=scope.Seed.InputBinding',
  'ReferenceEquals(TransactionOwner.InputBinding,InputBinding)',
  'var initialIds=InputBinding.CopyInitialRegistryIds()',
  'total>1048576||InputBinding.Next<=InputBinding.Maximum',
  'long next=InputBinding.Next,maximum=InputBinding.Maximum',
  'OriginalSelectedTargetContext=>Owner.OriginalSelectedInitialContext',
 ],
 'LegacyLibraryOriginalInputPatchCompatibility.cs':[
  'OriginalBootstrap=selected.Original;OriginalBootstrapInput=selected.OriginalInput',
  'ReferenceEquals(entry.OriginalEntry,originalInput)',
  'ReferenceEquals(replay.Scope.Context.InputBinding,replay.InputBinding)',
  '!built.AsSpan().SequenceEqual(replay.InputBinding.CopyRanges())',
 ],
 'LegacyLibraryTextPublicationDocument.cs':[
  'public static LegacyLibraryTextPublicationDocument PrepareOriginalSelected(',
  'LegacyFreshMainDocumentInputTransforms.ApplyOriginalSelected(cleanup,token)',
  'ReferenceEquals(row.Owner.InputBinding,scope.Seed.InputBinding)',
  'registryReplay is not null&&!registryReplay.State.Succeeded',
  'originalCompatibility is not null&&!originalCompatibility.State.IsReady',
 ],
}
source={};count=0
for name,needles in checks.items():
 path=root/'src/InkRecognition'/name;text=path.read_text(encoding='utf-8')
 for value in needles:assert value in text,(name,value)
 count+=len(needles);source[name]=dict(sha256=hashlib.sha256(path.read_bytes()).hexdigest(),checks=len(needles))
t=(root/'src/InkRecognition/LegacyFreshMainDocumentInputTransforms.cs').read_text()
assert t.index('entry.Claimed=true')<t.index('seed.InputBinding.ExportIdentity(token)')<t.index('seed.SwapTransforms(')<t.index('if(!swapped.ExportSucceeded)')<t.index('byte[] daf=seed.CopyDafInputRows32()')
replay=(root/'src/InkRecognition/LegacyLibrarySelectedInputRegistryReplay.cs').read_text()
assert replay.index('if(commands.Length>65537)')<replay.index('var initialIds=')<replay.index('for(int i=0;i<commands.Length;i++)')<replay.index('state=new(this,InputBinding.Next')
d=(root/'src/InkRecognition/LegacyLibraryTextPublicationDocument.cs').read_text()
assert d.index('actualScope.Seed.Validate(actualWords.Owner,token)')<d.index('claims.GetValue(actualWords')
assert d.index('var transforms=scope.Seed.OriginalSelectedInputContext')<d.index('LegacyFreshMainDocumentCounterHandoff.Apply')<d.index('LegacyLibraryTextDomTransfer.Prepare')
a.output.write_bytes((json.dumps(dict(nativeProof=json.loads(dependency.read_bytes()),runtimeSources=source,
 consumptionChecks=count,orderedJoins=4,vendorExecution=False,
 scope='same original-selected binding/C22 registry-all overlay and library DOM transport; no CBB/page/history acceptance'),indent=2)+'\n').encode())
print(f'PASS native continuation plus {len(source)} runtime files/{count} consumption checks/4 ordered joins; no vendor execution.')
