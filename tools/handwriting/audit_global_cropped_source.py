"""Static consumption of ordinary global C49/C4A/C4B point-view semantics.
Separate portable numerical oracle checks all produced coordinate bits.
"""
from pathlib import Path
import argparse,json,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,default=Path(__file__).resolve().parents[2]);p.add_argument('--capture',type=Path,required=True);p.add_argument('--models',type=Path);p.add_argument('--child',type=Path);a=p.parse_args()
if (a.models is None)!=(a.child is None):p.error('--models and --child must be supplied together')
paired=[] if a.models is None else ['--models',str(a.models),'--child',str(a.child)]
root=Path(__file__).resolve().parents[2];deps=[]
for i,name in enumerate(('audit_global_geometric_numeric_closure.py','audit_global_raw_crop_leaf_dispatch.py')):
 out=a.output.with_name('gcs-native-'+str(i)+'.json');args=[sys.executable,str(a.native_repo/'tools/handwriting'/name),str(out),'--native-repo',str(a.native_repo)]
 if i==0:args+=['--capture',str(a.capture),*paired]
 subprocess.run(args,check=True);deps.append(json.loads(out.read_bytes()))
s=(root/'src/InkRecognition/LegacyGlobalOrdinaryCroppedSource.cs').read_text(encoding='utf8')
rules=('!actual.IsComplete||actual.IsFaulted','ReferenceEquals(pending[^1],actual.Text.Selection)',
 'ReferenceEquals(pending[^2],actual.WordSource.Selection)','!s.Source.IsOrdinarySource',
 'ReferenceEquals(s.Node.FirstGlobalSource,s.Source)','a.SourceIndex>b.SourceIndex',
 'b.SourceIndex>=(uint)document.ObjectCount','a.Fraction is>=200 and<255',
 '!(a.PointIndex==0&&a.Fraction==0&&a.Mode==3)','b.Fraction!=255',
 'a.Fraction*.005f','b.Fraction*.005f','last=hasLast?b.PointIndex:n-1',
 'last-first+1+(f1!=0?1:0)','plans.Count>=65536||visits>1_048_576',
 'Interpolate(source[p],source[p+1],plan.Fraction0)',
 'Interpolate(source[plan.Last],source[plan.Last+1],plan.Fraction1)',
 'float dx=b.X-a.X,dy=b.Y-a.Y;float x=dx*f,y=dy*f;return new(x+a.X,y+a.Y)',
 'actual.Scope.SharedState.Selections.Failed','Publication.Scope.SharedState.Selections.NextSelectionId!=next',
 'Publication.CopyPendingSelections().SequenceEqual(pending)',
 'Publication.WordSource.CopyCharacterLinks().SequenceEqual(links)')
for rule in rules:assert rule in s,rule
assert s.index('Add(true,actual.WordSource.Node.Identity')<s.index('new InkPoint[plan.Count]')
assert all(x not in s for x in('LegacyOwnedMainWordSourceChoicesResult','LegacyLibraryTextPublicationScope','FusedMultiplyAdd'))
a.output.write_text(json.dumps(dict(vendorExecution=False,dependencies=deps,runtimeContracts=len(rules)+4,
 scope='Bounded actual global original C49/integer C4A/fractional C4B point views only; no2DB boundaries or238B0 source mutation'),indent=2)+'\n',encoding='utf8')
print(f'PASS{len(rules)+4} original global point-view contracts +actual raw/cropped dispatch dependencies; no vendor execution.')
