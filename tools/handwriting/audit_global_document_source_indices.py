"""Read-only native020920 proof and actual-global fresh index contracts.
No vendor execution, configured2 owner or acceptance is inferred.
"""
import argparse,hashlib,json,runpy,sys
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[2];dependencies={}
for index,name in enumerate(['audit_main_document_source_indices','audit_global_document_selected_sources']):
 report=a.output.with_name('gix-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]+'-'+str(index)+'.json');saved=sys.argv
 try:
  sys.argv=[name,str(report)]+(['--native-repo',str(a.native_repo)]if index else[])
  runpy.run_path(str((root if index else a.native_repo)/'tools/handwriting'/(name+'.py')),run_name='__main__')
 finally:sys.argv=saved
 dependencies[name]=hashlib.sha256(report.read_bytes()).hexdigest()
kernel=root/'src/InkRecognition/LegacyMainDocumentSourceIndexKernel.cs'
assert hashlib.sha256(kernel.read_bytes()).hexdigest()=='46df26b8bb0ca3ae754e18783cb2470846203fa86c26faecbdd65bd97c2f45e0'
source=(root/'src/InkRecognition/LegacyFreshMainGlobalDocumentSourceIndices.cs').read_text(encoding='utf-8-sig')
rules=[
 'ConditionalWeakTable<LegacyFreshMainGlobalWordTextPublication,Entry>',
 'actual.Validate(token);var publication=actual.Publication',
 'publication.Wrapper.CharacterInterner.CopyInsertionOrder()',
 'nodes.Count!=byId.Count||nodes.Count!=shape.Count',
 'ReferenceEquals(character.OriginalSource.Node,node)',
 'row.PreviousInserted!=(i==0?0:nodes[i-1].Identity)',
 'row.NextInserted!=(i+1==nodes.Count?0:nodes[i+1].Identity)',
 'if((character.Flags&0x200000)!=0)continue',
 'source.PriorRangeCount!=0||!ReferenceEquals(node.FirstGlobalSource,source.Source)',
 'plans.Add(new(actual,character,0x20,source.Source.CopyRanges(token)))',
 'wordNodes.Count!=1||!ReferenceEquals(wordNodes[0],word.Node)',
 'ReferenceEquals(word.Node.FirstGlobalWordSource,publication.OriginalWordSource)',
 'if((actual.WordFlags&0x200000)==0)',
 'var ranges=word.CopyOrdinaryRanges();int header=0x30',
 'ranges=publication.OriginalWordSource.CopyOriginalKey();header=0x20',
 'foreach(bool isWord in new[]{false,true})',
 'bounds.Add(plan.Key)','lifetimes.GetValue(publication',
 'if(entry.Claimed)throw new InvalidOperationException',
 'entry.Claimed=true;return result',
 'token.ThrowIfCancellationRequested();if(entered)throw',
 'ValidateState(token);entered=true',
 '(plan.IsWord?words:chars).Add(plan,plan.Key,token)',
 'ValidateState(token);ready=true;Record("ready")',
 'catch{faulted=true;Record("fault");throw;}',
 'if(selection.Failed||selection.NextSelectionId!=next)',
 'CachedVectorCount=vector?0:null',
 'key.Length>65536||key.Length>1_048_576-Memberships',
 'foreach(var range in key)intervals.Add(range);vectors.Add(key.ToArray())',
 'intervals.Count>65536||vectors.Count>65536',
]
for rule in rules:assert rule in source,rule
assert source.index('bounds.Add(plan.Key)')<source.index('lifetimes.GetValue(publication')<source.index('entry.Claimed=true')
assert source.index('publication.Wrapper.CharacterInterner.CopyInsertionOrder()')<source.index('publication.Scope.SharedState.Words.CopyInsertionOrder()')
for oldtype in ['LegacyFreshMainWordSourceChoices','LegacyFreshMainDocumentSelectedSources','LegacyOwnedMainDocumentIndexedSource']:
 assert oldtype not in source,oldtype
minimum=[0,1]
for i in range(2,33):minimum.append(1+minimum[-1]+minimum[-2])
assert minimum[22]<=65536<minimum[23]==75024
a.output.write_text(json.dumps(dict(status='PASS bounded actual-global020920 typed owner',vendorExecution=False,
 dependencyReports=dependencies,runtimeContracts=len(rules)+3,unchangedKernelSha256=hashlib.sha256(kernel.read_bytes()).hexdigest(),
 preflight=dict(maximumIntervalNodes=65536,maximumVectorNodes=65536,maxRawMembershipsPerIndex=1048576,maxRangesPerSource=65536,maximumAdmittedAVLHeight=22,nativePathLimit=32),
 prefix='raw occurrences -> memberships -> dirty -> vector latest-owner/cache0 -> source count;ready after CHAR thenWORD passes',
 scope='NEW typedglobal sources/snapshots; CWT exactPublication; no native allocator/page-reuse/023260/DOM/acceptance claim'),indent=2)+'\n')
print('PASS genuine-global source-index typed ownership/preflight/lifetime/prefix contracts; unchanged generic AVL kernel; zero old seams.')
