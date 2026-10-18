"""Owned119 terminal composition/whole native range evidence. Static only."""
from pathlib import Path
import hashlib,json,runpy,sys
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv
p=Path(__file__).with_name('audit_english119_terminal_plan.py');sys.argv=[str(p),str(target.with_suffix('.plan.json'))]
g=runpy.run_path(str(p));sys.argv=saved;b=g['b'];root=g['root']
a,z=0x1801f42d0,0x1801f6c32;h='c4422cf488eba5defdbeed9d8d981d442e7e02f6cb53002ce2b9956bbf408b6d'
assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
#Win64 exception directory supplies the actual function end, not an arbitrary
#next guessed decompiler address. pefile reads metadata and never loads a DLL.
import pefile
pe=pefile.PE(data=b.data)
assert any(e.struct.BeginAddress==a-0x180000000 and e.struct.EndAddress==z-0x180000000 for e in pe.DIRECTORY_ENTRY_EXCEPTION)
source=(root/'src/InkRecognition/LegacyOwnedEnglish119Terminal.cs').read_text(encoding='utf-8-sig')
for text in ('context.Passes.CopyB8Storage()','LegacyFirstTerminalSelection.Select(grid,invocation,token)',
 'LegacyFirstSelectedGlyphTrace.Read(grid,choice,token)','ReferenceEquals(candidates.SourceRanges,policy.SourceRanges)',
 'policy.Shadda,0xa000,false,false,source.SourceKind,0'):
 assert text in source,text
assert '.ScoreGroup('not in source and '.Evaluate('not in source and 'ReadSource('not in source
kernels={name:hashlib.sha256((root/'src/InkRecognition'/name).read_bytes()).hexdigest()for name in('LegacyFirstTerminalSelection.cs','LegacyFirstSelectedGlyphTrace.cs')}
report=dict(staticOnly=True,vendorExecution=False,nativeFunction=dict(start=hex(a),end=hex(z),sha256=h,endOwner='Win64 .pdata RUNTIME_FUNCTION'),
 policyComposition=str(target.with_suffix('.plan.json')),sharedExistingKernels=kernels,
 actualRuntime='One typed completed context input; same immutable source/model/primary/cache; actual B8 fullstorage ->existing terminal selector ->existing causal trace. No additional NN evaluation or RU resource reader.',
 exclusions=['rich273/274 allocation','WORD276/275','source720','dictionary winner','accepted English text','reused/externally configured source'])
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS whole .pdata-bounded1F42D0 +selected invocation/owner policies +same-context kernel composition; no vendor execution')
