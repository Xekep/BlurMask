"""Exact arithmetic/order preservation checks for the seven approved seams."""
from pathlib import Path
import argparse,subprocess,json,hashlib,re
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();base='2f539ad4bf1acb9629e3aa8cd899d8350febb739';checked=[]
def source(name):
 path='src/InkRecognition/'+name;old=subprocess.check_output(['git','show',base+':'+path],cwd=a.repo).decode('utf-8-sig');new=(a.repo/path).read_text(encoding='utf-8-sig');return old,new
def body(s,marker):
 lo=s.index('{',s.index(marker));depth=0
 for i in range(lo,len(s)):
  if s[i]=='{':depth+=1
  elif s[i]=='}':
   depth-=1
   if depth==0:return s[lo:i+1]
 raise AssertionError(marker)
def eq(name,old,new):
 # Formatting is irrelevant; tokens/comments/strings and statement order stay.
 canonical=lambda s:re.sub(r'\s+','',s)
 assert canonical(old)==canonical(new),name
 checked.append({'section':name,'normalizedSha256':hashlib.sha256(canonical(new).encode()).hexdigest()})
o,n=source('LegacyEnglishMainOnlyAcousticScorer.cs');x=body(o,'private int[] Combine(').replace('Masks.Source.Alphabet','knowledge.Source.Alphabet').replace('Masks.Knowledge.','knowledge.');eq('weighted ensemble/positive80 before division',x,body(n,'internal static int[] CombineCore('))
o,n=source('LegacyEnglishMainClassCacheKernel.cs');eq('entire220-class cache arithmetic/order',body(o,'internal LegacyOrdinaryAcousticClassCacheResult Build(').replace('masks.','inputs.'),body(n,'internal LegacyOrdinaryAcousticClassCacheResult Build('))
o,n=source('LegacyEnglishMainGlyphFeatures.cs');eq('full group geometry/four feature vectors',body(o,'internal static LegacyEnglishMainGlyphFeatures Prepare(').replace('snapshot.Masks.Source','snapshot.Inputs.Source'),body(n,'internal static LegacyEnglishMainGlyphFeatures Prepare('))
o,n=source('LegacyEnglishMainAcousticModel.cs');eq('whole immutable107 NN implementation',o[o.index('internal sealed class LegacyEnglishMainNetwork'):],n[n.index('internal sealed class LegacyEnglishMainNetwork'):]);x=body(o,'private LegacyEnglishMainAcousticModel(');y=body(n,'private LegacyEnglishMainAcousticModel(LegacyEnglishMainAcousticInputs');marker='if(!Field("profile0","14..44.raw"';eq('all profile/model numerical construction',x[x.index(marker):],y[y.index(marker):]);oldPrepare=body(o,'internal static LegacyEnglishMainAcousticModel Prepare(');newPrepare=body(n,'internal static LegacyEnglishMainAcousticModel Prepare(');newPrepare=newPrepare.replace('_=LegacyEnglishMainAcousticInputs.FromIndependent(masks); // Validate policy even if case preparation populated Shared first.','');eq('old per-call Prepare plus mandatory policy validation',oldPrepare,newPrepare)
o,n=source('LegacyNativeCasePermissions.cs');x=body(o,'public byte[] ExpandStoredPermissions(').replace('Alphabet.LabelCount','alphabetCount').replace('foreach(var row in _triplets)','foreach(var row in triplets)').replace('LanguageModel.CasePathVersion','casePathVersion');y=body(n,'internal static byte[] ExpandStoredPermissionsCore(');y=y[y.index('if(permissions.Length'):];eq('literal ordered case byte writes',x[1:],y)
o,n=source('LegacyPreparedAcousticDictionaryPermissions.cs');x=body(o,'public static LegacyPreparedAcousticPermissionResult BuildAndRefresh(');y=body(n,'internal static LegacyOrdinaryAcousticPermissionBytes BuildBytes(');start='cancellationToken.ThrowIfCancellationRequested();';x=x[x.index(start):x.rindex('return new(true, permission,')];y=y[y.index(start):y.rindex('return new(true,permission);')];x=x.replace('source.AcousticSymbolCount','acousticCount').replace('return new(false, ReadOnlyMemory<byte>.Empty, null);','return new(false, ReadOnlyMemory<byte>.Empty);');eq('literal1C01B0 byte producer/failure/no-forward',x,y)
o,n=source('LegacyOrdinaryGraphAcousticPermissionSession.cs');x=body(o,'public LegacyOrdinaryGraphPermissionPass Evaluate(');y=body(n,'private LegacyOrdinaryGraphPermissionPass EvaluateCore(');start='if (Interlocked.CompareExchange';end='var permission =';eq('entire pooled selector/invalidation/recompute prefix',x[x.index(start):x.index(end)],y[y.index(start):y.index(end)]);start='source = permission.WorkingMap!;';eq('unchanged post-permission return/failure invalidation',x[x.index(start):],y[y.index(start):])
assert 'caseTarget.BuildAndRefresh(this,lease!,source,reserved,rows,cancellationToken)'in n
out={'status':'PASS','baseCommit':base,'oldFiles':7,'exactNumericSections':len(checked),'sections':checked,'vendorExecuted':False};a.output.write_text(json.dumps(out,indent=2)+'\n');print(json.dumps({k:v for k,v in out.items()if k!='sections'}))
