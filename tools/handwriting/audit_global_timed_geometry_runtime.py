"""Read-only retained-native closure and independent NEW timed geometry checks.
Never loads SDK modules. Recognition quality is deliberately not inferred.
"""
import argparse,hashlib,json,runpy,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,required=True)
p.add_argument('--recorded-manifest',type=Path,required=True);p.add_argument('--parent-capture',type=Path,required=True)
p.add_argument('--capture',type=Path,required=True);p.add_argument('--native-capture',type=Path)
a=p.parse_args();root=a.native_repo.resolve();package=Path(__file__).resolve().parents[2]
reports={}
def run(name,script,args):
 destination=a.output.with_name('tgr-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]+'-'+str(len(reports))+'.json')
 saved=sys.argv
 try:sys.argv=[str(script),str(destination),*map(str,args)];runpy.run_path(str(script),run_name='__main__')
 finally:sys.argv=saved
 reports[name]=dict(path=str(destination),sha256=hashlib.sha256(destination.read_bytes()).hexdigest())
run('numericNativeClosure',root/'tools/handwriting/audit_global_geometric_numeric_closure.py',
 ['--native-repo',root,'--capture',a.parent_capture])
run('physicalConstructorNativeClosure',root/'tools/handwriting/audit_global_timed_geometry_plan.py',
 ['--native-repo',root,'--caller-plan-root',root,'--ordinary-plan-root',root,
  '--capture',a.parent_capture,'--recorded-manifest',a.recorded_manifest])
sys.path.insert(0,str(root/'research/myscript/scripts'));from inspect_text_features import Binary
b=Binary('MyScriptText.dll');spec=json.loads((package/'tools/handwriting/global-timed-geometry-runtime-pins.json').read_bytes())
assert hashlib.sha256(b.data).hexdigest()==spec['dllSha256'];md=Cs(CS_ARCH_X86,CS_MODE_64);decoded={}
for region in spec['regions']:
 raw=b.data[b.off(region['start']):b.off(region['end'])]
 assert raw.hex()==region['bytes']and hashlib.sha256(raw).hexdigest()==region['sha256']
 instructions=list(md.disasm(raw,region['start']));assert sum(i.size for i in instructions)==len(raw)
 decoded.update({i.address:(i.mnemonic,i.op_str)for i in instructions})
for at,mn,op in spec['pins']:assert decoded[at]==(mn,op)
# The fallback is native, including the reversed-byte reset and boundary-only
# repair. Scratch64 Previous/Next is deliberately not reset to a fake chain.
for at,value in {0x18004f2d2:('jne','0x18004f318'),0x18004f2e7:('jne','0x18004f318'),
 0x18004f300:('je','0x18004f318'),0x18004f30b:('je','0x18004f318'),
 0x18004fb66:('je','0x18004fb91'),0x18004fb85:('mov','r9d, dword ptr [rax + r12 + 4]'),
 0x18004fbb0:('je','0x18004fc15'),0x18004fc0e:('jmp','0x18004fc15'),
 0x18004fc26:('mov','byte ptr [rax + rcx*8 + 0x28], 0'),
 0x18004fc3b:('mov','dword ptr [rax + rcx*8 + 4], edx'),
 0x18004fc58:('mov','dword ptr [rdx + rcx*8 + 4], eax')}.items():assert decoded[at]==value
sources={name:(package/'src/InkRecognition'/name).read_text(encoding='utf-8-sig')for name in[
 'LegacyPhysicalRawInkIngress.cs','LegacyPhysicalRawInkTiming.cs',
 'LegacyFreshMainGlobalOrdinaryGeometry.cs','LegacyGlobalOrdinaryGeometryKernels.cs']}
assert all('DllImport'not in source and'NativeLibrary.Load'not in source for source in sources.values())
ingress=sources['LegacyPhysicalRawInkIngress.cs'];timing=sources['LegacyPhysicalRawInkTiming.cs']
owner=sources['LegacyFreshMainGlobalOrdinaryGeometry.cs'];kernels=sources['LegacyGlobalOrdinaryGeometryKernels.cs']
assert 'LegacyOwnedRawInkDocument.Create(snapshots,token)'in ingress
assert 'LegacyFreshRawInkCurrentView.Prepare(document,token)'in ingress
assert 'AttachTiming'not in ingress
assert 'for(int point=0;point<n;point++)'in ingress and 'foreach(var q in actualStrokes[i].Points)'not in ingress
assert 'MaximumAbsoluteMicroseconds=(1L<<52)-2'in timing
assert 'ReferenceEquals(parent.Scope.Document,timing.Document)'in owner
assert 'if(view.Kind==LegacyGlobalRawCurveViewKind.OriginalC49)return s.Base'in timing
assert 'if(view.Kind==LegacyGlobalRawCurveViewKind.IntegerCropC4A)return first'in timing
assert 'double value=scaled+(double)first;value=value+.5'in timing
assert '(row.Flags&0xc00)!=0'in owner and '(s.Flags.Value&0x10600000)!=0'in owner
assert 'sourceLines.Length!=56'in owner and 'parent.LineCount!=1'in owner
assert 'parent.WordSource.Flags&0x10200000'in owner and 'rows>65536'in owner
assert 'visits>1_048_576'in owner and 'payload>32*1024*1024'in owner
assert 'trace.Add("failed:"+stage)'in owner
assert 'if(entered)throw new InvalidOperationException'in owner
assert 'class LegacyOrdinaryGeometryMetadataMap'in owner and 'new(ReferenceEqualityComparer.Instance)'in owner
assert 'cell.Initial!=flags'in owner and 'ReferenceEquals(cell.Source,s)'in owner
assert 'var shared=metadata.Get(s)'in owner and 'Metadata=shared'in owner
assert 'space' in kernels and 'width>meanWidth*4f'in kernels
assert 'floor=(double)height*.05'in kernels
# Portable independent oracle executes only Python arithmetic on original
# inputs and typed captures, never the C# kernel nor a vendor DLL.
oracle=package/'tools/LegacyGlobalTimedGeometry.Probe/audit_output.py';saved=sys.argv
numeric_report=a.output.with_name('tgr-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]+'-numeric.json')
try:
 sys.argv=[str(oracle),str(a.recorded_manifest),str(a.parent_capture),str(a.capture),'--report',str(numeric_report)]
 if a.native_capture:sys.argv+=['--native',str(a.native_capture)]
 runpy.run_path(str(oracle),run_name='__main__')
finally:sys.argv=saved
report=dict(status='PASS NEW timed ordinary geometry closure and independent numerical oracle',vendorExecution=False,
 nativeDependencies=reports,nativeFallbackRanges=len(spec['regions']),nativeFallbackPins=len(spec['pins']),
 productionSources={name:hashlib.sha256((package/'src/InkRecognition'/name).read_bytes()).hexdigest()for name in sources},
 numericalReportSha256=hashlib.sha256(numeric_report.read_bytes()).hexdigest(),
 limitations=['no238B0/document adoption','no application default change','no recognition quality claim',
 'System.Math moderate-finite kernels; not a bit-exact port of vendor exceptional CRT paths',
 'native malformed-chain fallback proven statically; actual ordinary initial scratch is fresh'])
a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS NEW timed geometry: native constructor/numeric closure,122 focused fallback pins, independent full scalar oracle')
