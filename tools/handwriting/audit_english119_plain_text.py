"""Actual119 rich WORD composition +mechanical shared body. Static PE only."""
from pathlib import Path
import hashlib,json,runpy,sys
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv;p=Path(__file__).with_name('audit_english119_plain_text_plan.py');sys.argv=[str(p),str(target.with_suffix('.plan.json'))]
g=runpy.run_path(str(p));sys.argv=saved;root=g['root']
source=root/'src/InkRecognition';body=(source/'LegacyNativeFirstPlainTextKernel.cs').read_text(encoding='utf-8-sig')
body=body[body.index('        int First('):body.index('\n    }\n    private static int I(')]
replacements=[('var result=factory.CreatePlainResult(token);','var result=storage;'),('new byte[120]','wordStorage.Span'),('new byte[40]','glyphStorage.Span'),
 ('literal.Apply(','operations.ApplySymbol('),('literal.Create(','operations.CreatePlan('),('allocation.ShouldAllocate(','operations.ShouldAllocate('),
 ('interior.Read(','operations.ReadInterior('),('Cache(','operations.Cache('),('binding.SourceKind','sourceKind'),('return new(owner,result,choice,','return new(result,choice,')]
original=body
for old,new in reversed(replacements):original=original.replace(new,old)
originalHash=hashlib.sha256(original.encode()).hexdigest();assert originalHash=='a42ba5acc99f99ffbd767df23046322f1c8bbccc7758a0030940ba9760645e61'
factory=(source/'LegacyOwnedEnglish119PlainResultFactory.cs').read_text(encoding='utf-8-sig')
for text in('ReferenceEquals(Policy.SourceRanges,Candidates.SourceRanges)','ReferenceEquals(Policy.SourceRanges.OriginalSource,Preparation.OriginalSource)',
 'terminal.Invocation.Configuration99||terminal.Invocation.ConfigurationA8||Policy.NativeModeE0',
 'terminal.Alternatives.Count!=1','AssignedDescriptor=raw with{CurrentLineId=0}','CopyChildEnvelopeBytes(Candidates.GraphChild,token)',
 'span.NativeSecondByte!=1||bytes[350]!=0','source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="B9")',
 'internal bool CaptureGeometry=>false','internal bool OwnerC4=>false','symbol!=Policy.Blank&&symbol!=Policy.Closed',
 'actual.ReadClassCost(','confidence.ReadSymbolScore(symbol,token)','float first=values.Length==0?0:values[0].Value'):
 assert text in factory,text
assert 'LegacyFirstAcousticModel'not in factory
public=(source/'LegacyOwnedEnglish119PlainText.cs').read_text(encoding='utf-8-sig')
for text in('private readonly byte[] word,glyph','allocatedEmptyWord120.Length!=120','LegacyOwnedEnglish119PlainResultFactory.Prepare(terminal,token)',
 'factory.Alternative.Choice,factory.Alternative.Cells','internal LegacyFirstPlainTextStorage Storage','public byte[] CopyGlyphRecord',
 'UTF8'):
 assert text in public,text
assert '.Evaluate('not in factory and 'ReadSource('not in factory and 'LegacyFreshFirstResultOwnershipFactory'not in factory
paths=['LegacyFirstPlainTextMaterializer.cs','LegacyFreshFirstGlyphSymbolPlan.cs','LegacyFirstOwnedInteriorDelimiter.cs',
 'LegacyNativeFirstPlainTextKernel.cs','LegacyNativeFirstLiteralGlyphKernel.cs','LegacyNativeFirstInteriorDelimiterKernel.cs',
 'LegacyOwnedEnglish119PlainResultFactory.cs','LegacyOwnedEnglish119InteriorDelimiter.cs','LegacyOwnedEnglish119PlainText.cs']
report=dict(staticOnly=True,vendorExecution=False,planComposition=str(target.with_suffix('.plan.json')),
 original4a533ecSharedBodySha256=originalHash,currentSharedBodySha256=hashlib.sha256(body.encode()).hexdigest(),reversibleReplacements=replacements,
 sourceHashes={name:hashlib.sha256((source/name).read_bytes()).hexdigest()for name in paths},
 owner='One immutable actual119 terminal/context/primary/108category/source247/cut owner; no RU factory coercion. Existing ranked confidence and controller caches reused, no feature/NN calls.',
 stages='Actual selected literal allocation and hiddenE2/E4 guard; zero-span endpoints; genuine assigned1A0 geometry/interiorFFFB; graph context boundaries; word cost deltas/raw scalar channels; exact pool/WORD reset/link/publish order.',
 explicitBoundaries=['WORD120 and glyph40 allocator storage','raw pre24A descriptor-1 vs post24A assigned0','unknown TOP18 binding','no source720/UTF8/main winner/accepted text','no geometry finalization B9true','one surviving A4=1 terminal choice'])
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS actual119 factory/composed native proof +exact reversible committed4a533ec TEXT numeric loop; three old source wrappers only, no vendor execution')
