"""STATIC runtime proposal, native scalar leaves and actual measured-time availability."""
import argparse,hashlib,json,runpy,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path)
p.add_argument('--native-repo',type=Path,required=True);p.add_argument('--caller-plan-root',type=Path,required=True)
p.add_argument('--ordinary-plan-root',type=Path,required=True);p.add_argument('--capture',type=Path,required=True)
p.add_argument('--recorded-manifest',type=Path,required=True);p.add_argument('--avalonia-xml',type=Path)
a=p.parse_args();root=a.native_repo.resolve();here=Path(__file__).resolve().parent
out=a.output.with_name('gtg-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]+'.json')
dep=a.caller_plan_root/'tools/handwriting/audit_global_raw_timing_caller.py';saved=sys.argv
try:
 sys.argv=[str(dep),str(out),'--native-repo',str(root),'--ordinary-plan-root',str(a.ordinary_plan_root),'--capture',str(a.capture)]
 runpy.run_path(str(dep),run_name='__main__')
finally:sys.argv=saved
sys.path.insert(0,str(root/'research/myscript/scripts'));from inspect_text_features import Binary
b=Binary('MyScriptInk.dll');spec=json.loads((here/'global-timed-geometry-plan-pins.json').read_bytes())
assert hashlib.sha256(b.data).hexdigest()==spec['dllSha256'];md=Cs(CS_ARCH_X86,CS_MODE_64);decoded={};calls={};rows=[]
for r in spec['regions']:
 raw=b.data[b.off(r['start']):b.off(r['end'])]
 assert raw.hex()==r['bytes'] and hashlib.sha256(raw).hexdigest()==r['sha256']
 ins=list(md.disasm(raw,r['start']));assert sum(i.size for i in ins)==len(raw)
 decoded.update({i.address:(i.mnemonic,i.op_str)for i in ins});calls[r['function']]=[[i.address,i.op_str]for i in ins if i.mnemonic=='call']
 rows.append([r['function'],r['start'],r['end'],len(raw)])
assert calls==spec['calls']
for at,mn,op in spec['pins']:assert decoded[at]==(mn,op)
for at,value in spec['bindings']:assert b.qwords(at,1)[0]==value
for r in spec['constants']:
 raw=b.data[b.off(r['start']):b.off(r['start'])+len(r['bytes'])//2]
 assert raw.hex()==r['bytes'] and hashlib.sha256(raw).hexdigest()==r['sha256']
tsv=(here/'global-timed-geometry-plan-bodies.tsv').read_text().splitlines()[1:]
assert [[f,int(s,16),int(e,16)+1,int(n)]for f,s,e,n in (r.split('\t')for r in tsv)]==rows
# Bounded literal-ms unit semantics. This does not execute broadC58 grammar.
target=b.data[b.off(0x1800e7060):b.off(0x1800e7060)+96]
prefix=struct.unpack('<21d',b.data[b.off(0x1800f3e00):b.off(0x1800f3e00)+168])
source=bytearray(target);struct.pack_into('<d',source,56,prefix[3])
assert prefix[3]==.001 and struct.unpack_from('<d',target,56)[0]==1e-6
assert struct.unpack_from('<7i',source,8)==struct.unpack_from('<7i',target,8)==(0,0,1,0,0,0,0)
scale=1.
for i,power in enumerate(struct.unpack_from('<7i',source,8)):
 s,t=struct.unpack_from('<d',source,40+8*i)[0],struct.unpack_from('<d',target,40+8*i)[0]
 scale*=t/s if power<0 else s/t
coefficient=(scale*struct.unpack_from('<d',source)[0])/struct.unpack_from('<d',target)[0]
assert int(coefficient)==1000
# 04D2C0 reads a view start ONCE and updates BOTH min/max from THAT value.
t=Binary('MyScriptText.dll');ins={i.address:(i.mnemonic,i.op_str)for i in md.disasm(t.data[t.off(0x18004d48d):t.off(0x18004d4b1)],0x18004d48d)}
for at,value in {0x18004d496:('mov','r8, qword ptr [rax + 0x50]'),0x18004d49a:('call','r8'),
 0x18004d49d:('cmp','rax, qword ptr [r13 + 0x30]'),0x18004d4a3:('mov','qword ptr [r13 + 0x30], rax'),
 0x18004d4a7:('cmp','rax, qword ptr [r13 + 0x38]'),0x18004d4ad:('mov','qword ptr [r13 + 0x38], rax')}.items():assert ins[at]==value
manifest=json.loads(a.recorded_manifest.read_bytes());assert manifest['commit']=='c6629026b1fdef51fd08e3d2fddffd82f97e60a5' and manifest['penUpMetadata']=='absent'
# Select only filenames/hashes; do not read expected labels into this path.
inputs=[(s['file'],s['trackSha256'],s['sourceTimingSha256'])for s in manifest['samples']if s['kind']=='word'];del manifest
assert len(inputs)==9;timed=[]
for name,sha,origin in inputs:
 raw=(a.recorded_manifest.parent/name).read_bytes();assert hashlib.sha256(raw).hexdigest()==sha
 d=json.loads(raw);points=d['points'];intervals=d['intervalsMs'];assert len(points)==len(intervals) and d['penUpMetadata']=='absent'
 times=[];elapsed=0
 for interval in intervals:
  assert isinstance(interval,int)and not isinstance(interval,bool)and interval>=0
  elapsed+=interval;assert elapsed<=2**31-1;times.append(elapsed)
 assert all(times[i]<=times[i+1]for i in range(len(times)-1))
 baseTag=times[0]*1000;channel=[t-times[0]for t in times]
 assert channel[0]==0 and all(baseTag+x*1000==t*1000 for x,t in zip(channel,times))
 timed.append(dict(file=name,trackSha256=sha,sourceTimingSha256=origin,points=len(points),firstRecordedMilliseconds=times[0],lastRecordedMilliseconds=times[-1],
  physicalBaseTagMicroseconds=baseTag,
  measuredChannelSha256=hashlib.sha256(struct.pack('<'+'i'*len(channel),*channel)).hexdigest(),rawObjects=1,penUpsInvented=0))
assert sum(s['points']for s in timed)==1466
contracts={
 'src/InkRecognition/LegacyOwnedRawInkDocument.cs':['internal static LegacyOwnedRawInkDocument Create'],
 'src/InkRecognition/LegacyFreshMainRecognitionContext.cs':['internal LegacyFreshMainGlobalPlainRecognition PrepareGlobalDocument','document,resolution,table,caseData,publication,token'],
 'src/InkRecognition/LegacyLibraryOriginalTextFieldBootstrap.cs':['public LegacyOwnedRawInkDocument Document => CurrentView.Document','Prepare(LegacyFreshRawInkCurrentView actualRaw'],
 'src/InkRecognition/LegacyLibraryOriginalSelectedFieldContext.cs':['public LegacyOwnedRawInkDocument Document => Original.Document'],
 'src/InkRecognition/LegacyLibraryGlobalTextPublicationScope.cs':['PrepareOriginalSelected(','models.PrepareGlobalDocument(context.Document'],
 'src/BlurMask/InkDocument.cs':['public List<Point> Points','Length < 0.65'],
 'src/BlurMask/AiInkLayout.cs':['strokes.Where(members.Contains)','new InkStroke(s.Points.Select']}
sourceHashes={}
for name,rules in contracts.items():
 raw=(root/name).read_bytes();text=raw.decode('utf8');assert all(r in text for r in rules),name
 sourceHashes[name]=hashlib.sha256(raw).hexdigest()
xml=None
if a.avalonia_xml:
 raw=a.avalonia_xml.read_bytes();assert b'P:Avalonia.Input.PointerEventArgs.Timestamp' in raw and b'GetIntermediatePoints' in raw
 xml=dict(sha256=hashlib.sha256(raw).hexdigest(),unitsOrBackendBehaviorProved=False)
report=dict(status='PASS STATIC reviewable NEW-only timed ingress/full ordinary geometry proposal',vendorExecution=False,
 dependencySha256=hashlib.sha256(out.read_bytes()).hexdigest(),unitFunctions=3,unitInstructions=len(spec['pins']),unitBindings=4,
 fixedMillisecondCoefficient=coefficient,storedSigned32Multiplier=int(coefficient),broadUnitGrammarProved=False,
 timingReadNativePins=6,recordedTimedInputAvailability=timed,totalMeasuredPoints=1466,
 sourceContracts=sourceHashes,avaloniaLocalAPI=xml,
 proposedProductionFiles=4,oldLibraryRuntimeSeams=0,appIntegrationEditsProposedSeparately=True,
 runtimeImplemented=False,existingUntimed13ReadyPermitted=False,newTimed9GeometryRun=False,recognitionQualityClaim=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f"PASS STATIC3 unit leaves/{len(spec['pins'])} instructions/4bindings +genuine9/1466 measured timestamps; proposal only")
