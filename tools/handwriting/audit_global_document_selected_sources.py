"""Static actual-global01FD40/020000 owner/guard/write composition.
Numerical outputs are checked by the separate portable selected-source oracle.
"""
import argparse,hashlib,json,runpy,sys
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path)
p.add_argument('--native-repo',type=Path,required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[2];dependencies={}
for index,name in enumerate(['audit_main_document_selected_sources','audit_global_document_coverage','audit_global_raw_object_kinds']):
 out=a.output.with_name('gsel-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]+'-'+str(index)+'.json');saved=sys.argv
 try:
  sys.argv=[name,str(out)]+(['--native-repo',str(a.native_repo)]if index else[])
  runpy.run_path(str((root if index else a.native_repo)/'tools/handwriting'/(name+'.py')),run_name='__main__')
 finally:sys.argv=saved
 dependencies[name]=hashlib.sha256(out.read_bytes()).hexdigest()
source=(root/'src/InkRecognition/LegacyFreshMainGlobalDocumentSelectedSources.cs').read_text(encoding='utf-8-sig')
rules=[
 'actual.Validate(token);var parent=actual.Publication',
 'parent.SelectedWordIndex!=0||parent.AlternativeCount!=1||parent.WordSource.Field40!=-1',
 'parent.Scope.SharedState.Selections.Failed',
 'LegacyGlobalRawObjectKinds? rawKinds=null',
 'LegacyGlobalRawObjectKinds.ComputeMetadata(actual,s.Source,s.Scalars,s.Key,ref rawKinds,token)',
 'rawKinds?.Validate(token)',
 'ReferenceEquals(parent.Scope.Seed,parent.Scope.Context.Result.PublicationSeed)',
 'ReferenceEquals(parent.Scope.Seed.PendingSelections,parent.Scope.SharedState.Selections)',
 'ReferenceEquals(source.Owner,parent.Wrapper)',
 'ReferenceEquals(node.Owner,parent.Wrapper.CharacterInterner)',
 'ReferenceEquals(node.FirstGlobalSource,source.Source)',
 'nodes.Count!=payloads.Count||tree.Length!=nodes.Count',
 'ReferenceEquals(interner.Document,parent.Scope.Document)',
 'bytes>4_194_304','source.ReuseKernel?.RecordCount',
 'stack.Count==64||!shape.TryGetValue(cursor,out var node)||!entered.Add(cursor)',
 'if((flags&0x10600000)!=0)return',
 'if((flags&0x20000000)!=0)',
 'if(selection!=-1)throw new NotSupportedException',
 'if(count!=1)return','if(key.Length!=16)throw',
 'BinaryPrimitives.ReadUInt32LittleEndian(key[4..])==BinaryPrimitives.ReadUInt32LittleEndian(key[12..])',
 'BinaryPrimitives.ReadUInt16LittleEndian(key[2..])==0&&key[1]==0',
 'BinaryPrimitives.ReadUInt16LittleEndian(key[10..])==3&&key[9]==255',
 'ComputeWordMetadata(originalWordFlags,links.Select(l=>metadata[ordinal[l.Source.Node.Identity]]).ToArray())',
 'if((flags&0x11004000)==0)',
 '(original&0xfeffffffu)|(all?0x01000000u:0)',
 'LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(flags,indices,',
 'Scope.SharedState.Selections.NextSelectionId!=next',
 '!word.CopyChoiceScalar48().AsSpan().SequenceEqual(word48)',
 'Source.CopyAssignedScalars()!=Scalars',
 'ReferenceEquals(rows[i].Text,Texts[i])',
]
for rule in rules:assert rule in source,rule
assert source.index('LegacyGlobalRawObjectKinds.ComputeMetadata(')<source.index('uint wordMetadata=')
assert source.index('rawKinds?.Validate(token)')<source.index('var result=new LegacyFreshMainGlobalDocumentSelectedSources')
assert source.index('if((flags&0x10600000)!=0)return')<source.index('if((flags&0x20000000)!=0)')<source.index('if(count!=1)return')<source.index('bool lookup=')
assert source.index('if(bytes>4_194_304)')<source.index('var captured=')
assert 'LegacyOwnedMainWordSourceChoicesResult'not in source and 'LegacyLibraryTextPublicationScope'not in source
old=(root/'src/InkRecognition/LegacyFreshMainDocumentSelectedSources.cs').read_text(encoding='utf-8-sig')
start=old.index('{',old.index('internal static void ApplySelectedCharacters'));depth=1;end=start+1
while depth:depth+=(old[end]=='{')-(old[end]=='}');end+=1
bodyhash=hashlib.sha256(old[start:end].encode()).hexdigest();assert bodyhash=='d67baa317be52dfb695ad12214acf34c1e733fa37b905f76df0b41959f9d6685'
a.output.write_text(json.dumps(dict(status='PASS bounded actual-global selected-source overlay',vendorExecution=False,
 dependencies=dependencies,runtimeRules=len(rules)+4,oldNumericBodySha256=bodyhash,
 scope='same global coverage/Seed/fullCHARAVL/selectedWORD; lazy reached prior refusal/actual typed C49 raw-slot kind lookup; no020920/DOM/acceptance'),indent=2)+'\n')
print('PASS actual global selected-source lifecycle/native kind receipt/fullAVL64/link-order composition; one reviewed Prepare metadata seam.')
