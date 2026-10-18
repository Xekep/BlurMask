"""Native context ownership + mechanical extraction proof. Static PE only."""
from pathlib import Path
import hashlib,json,runpy,sys
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv;root=Path(__file__).resolve().parents[2]
sys.argv=[str(Path(__file__).with_name('audit_english119_context_plan.py')),str(target.with_suffix('.plan.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved;b=g['b'];f=g['f']
sys.argv=[str(Path(__file__).with_name('audit_first_blank_context.py')),'--output',str(target.with_suffix('.blank.json'))]
runpy.run_path(sys.argv[0]);sys.argv=saved
#Keep the literal full hash rather than accepting a decompiler text match.
native='c501b4378e44442183a2a1c6578e5026116ce9a8a9f2ce96d2987323f2e88007'
assert hashlib.sha256(b.data[b.off(0x1801ef070):b.off(0x1801f2440)]).hexdigest()==native
kernel=(root/'src/InkRecognition/LegacyFirstBlankContextKernel.cs').read_text(encoding='utf-8')
start=kernel.index('        var rows=Enumerable.Range(0,primary.NodeCount)');end=kernel.rfind('\n    }');body=kernel[start:end]
restore={'r.Symbol>blank+2':'r.Symbol>binding.Alphabet.LabelCount+2','nativeModeE0':'binding.NativeModeE0',
 'excludeConsecutiveBlank':'producer.NativeConsecutiveBlankExclusion','r.Symbol==closed':'r.Symbol==literal.ClosedSymbolE4',
 'glyphCost':'(preparedCost??producer.ReadGroupCost)','correct118':'geometry.Correct118','correct140':'geometry.Correct140'}
for a,z in restore.items():body=body.replace(a,z)
expected='9e6f8543a7f696104598ad3426bee3d62d7e7dbb2a2acefa6a01840ba4cc5cc8'
assert hashlib.sha256(body.encode()).hexdigest()==expected
old=(root/'src/InkRecognition/LegacyFirstBlankContextPasses.cs').read_text(encoding='utf-8')
assert 'return LegacyFirstBlankContextKernel.Run(primary,graph,connections,width,cap0,cap8,blank,shadda,newline,space,nbsp,'in old
assert 'literal.ClosedSymbolE4,binding.NativeModeE0,producer.NativeConsecutiveBlankExclusion,preparedCost??producer.ReadGroupCost,'in old
assert 'geometry.Correct118,geometry.Correct140,observer,token)'in old
owned=(root/'src/InkRecognition/LegacyOwnedEnglish119ContextPasses.cs').read_text(encoding='utf-8')
for text in ('LegacyEnglish119PrimaryResult primary','LegacyFirstAcousticShortlistTable.Build','e.NormalizedLikelihood',
 'Policy.Categories.CompositeCategories.Span[composite].GeometryIndex','ReadClassCost(symbol,ushort.MaxValue,prototype,true,false,token)',
 'LegacyFirstBlankContextKernel.Run(Primary.Primary','p.NativeModeE0,p.ExcludeConsecutiveBlank,ReadGroupCost,geometry.Correct118,geometry.Correct140,observer,token)',
 'internal LegacyFirstOrdinaryContextPasses Passes','CopySymbolContexts()=>Passes.Contexts.CopyNodes()'):
 assert text in owned,text
assert 'LegacyFreshFirstResultOwnershipFactory'not in owned and'LegacyFreshFirstLiteralContextPolicy'not in owned
target.write_text(json.dumps(dict(staticOnly=True,vendorExecution=False,native1EF070Sha256=native,
 sharedBodyOriginal='LegacyFirstBlankContextPasses numeric row walk from committed57e8855',sharedBodySha256=expected,
 mechanicalRenames=restore,policyComposition=str(target.with_suffix('.plan.json')),ancestorComposition=str(target.with_suffix('.blank.json')),
 scope='Actual selected frontend249/119 A8+B0+B8 with same completed primary, private geometry/cache/symbolic owner. No terminal273/WORD/text/English accuracy claim. Prepared costs remain friend-only.'),indent=2)+'\n',encoding='utf-8')
print('PASS entire native1EF070 SHA +exact mechanical blank-context numeric body +same old delegates +typed EN119/prototype/FFFF/field0 ownership; no vendor execution.')
