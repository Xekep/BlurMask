"""Native prerequisites and separately named library selected-input adoption checks."""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
deps=[]
for name in ['audit_original_selected_field.py','audit_text_publication_adoption.py']:
 script=Path(__file__).with_name(name);out=Path(str(a.output)+'.'+name+'.json')
 subprocess.run([sys.executable,str(script),str(out)],cwd=root,check=True)
 deps.append(dict(path='tools/handwriting/'+name,sha256=hashlib.sha256(script.read_bytes()).hexdigest(),report=json.loads(out.read_text())))
def read(name):return (root/'src/InkRecognition'/name).read_text(encoding='utf8')
s=read('LegacyLibraryTextPublicationScope.cs').split('public static LegacyLibraryTextPublicationScope PrepareConfiguredOriginalSelected(',1)[1]
order=['LegacyLibraryTextProcessingContext.CreateForOriginalSelected(actualSelectedInput,token)',
 'context.PrepareInput(token);lease=context.RetainForPublication()',
 'scope=new(context,lease,models,table,publication)',
 'models.PrepareConfiguredDocument(context.Document,resolution,table,caseData,publication,token)']
positions=[s.index(v)for v in order];assert positions==sorted(positions)
binding=read('LegacyLibrarySelectedInputBinding.cs')
for value in ['internal sealed class LegacyLibrarySelectedInputBinding','internal object Identity',
 'input.State.Succeeded','input.HasNativeReaderContext','input.NativeHistoryEnabled',
 'ReferenceEquals(e.OriginalEntry, input.OriginalInput)','OriginalSelected!.State.Registry.Single(e => e.RegistryId == RegistryId)',
 'LegacyOrThrow','input.Children.Count != 0']:
 assert value in binding,value
ctx=read('LegacyLibraryTextProcessingContext.cs')
for value in ['boundInput = Owner.InputBinding','sourceTag30 = request.ReadInputTag()',
 'ranges = Owner.InputBinding.CopyRanges()','ReferenceEquals(seed.InputBinding,boundInput)',
 'var imported = Arena.ImportSelectedPage(selected, token)',
 'if(!disposed||publicationLeases!=0||resourcesReleased)return']:
 assert value in ctx,value
arena=read('LegacyLibraryTextDomArena.cs')
assert 'ReferenceEquals(Document,source.Document)' in arena
assert 'copy.NamespacePrefix=n.NamespacePrefixName?.Text' in arena
assert 'copy.NamespaceUri=n.NamespaceUri' in arena
assert 'NamePool.Intern(copy.NamespacePrefix);NamePool.Intern(copy.NamespaceUri)' in arena
seed=read('LegacyFreshMainTextDocumentSeed.cs')
assert 'ReferenceEquals(actualWords.InputBinding,InputBinding)' in seed
assert 'dafInputRows32=actualSelectedInput.CopyDafInputRows32()' in seed
assert 'SourceTag30=>InputBinding.RegistryId' in seed
# EarlyUnsupported gates are before either CWT ownership claim. These are
# library supported-scope guards, never assertions of a native rejection.
doc=read('LegacyLibraryTextPublicationDocument.cs')
assert doc.index('actualScope.Context.OriginalSelectedInput is not null')<doc.index('claims.GetValue(')
transform=read('LegacyFreshMainDocumentInputTransforms.cs')
assert transform.index('seed.OriginalSelectedInputContext is not null')<transform.index('lifetime.GetValue(')
a.output.write_bytes((json.dumps(dict(nativePrerequisites=deps,portableOrderedSteps=order,
 portableSameOwnerAndGuardChecks=25,vendorExecution=False,
 scope='Library original-selected input2C1/import/shared3E/Seed before configured2 NN; downstream transform/transport/replay explicitly refused; no CBB acceptance'),indent=2)+'\n').encode())
print('PASS2 native prerequisite audit trees +4 ordered library adoption steps/25 owner and early guard checks; no vendor execution')
