"""STATIC ONLY ordinary2DB proposal; PE reads, no vendor module execution."""
import argparse,hashlib,json,math,runpy,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path)
p.add_argument('--native-repo',type=Path,required=True);p.add_argument('--capture',type=Path,required=True)
a=p.parse_args();root=a.native_repo.resolve();here=Path(__file__).resolve().parent
dependencies={}
for index,name in enumerate(('audit_global_cropped_source.py','audit_textfield_input_context.py')):
 out=a.output.with_name('ggo-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]+'-'+str(index)+'.json')
 args=[str(root/'tools/handwriting'/name),str(out)]
 if index==0:args+=['--native-repo',str(root),'--capture',str(a.capture)]
 saved=sys.argv
 try:sys.argv=args;runpy.run_path(args[0],run_name='__main__')
 finally:sys.argv=saved
 dependencies[name]=hashlib.sha256(out.read_bytes()).hexdigest()
sys.path.insert(0,str(root/'research/myscript/scripts'));from inspect_text_features import Binary
b=Binary('MyScriptInk.dll');spec=json.loads((here/'global-geometric-ordinary-pins.json').read_bytes())
assert hashlib.sha256(b.data).hexdigest()==spec['dllSha256']
md=Cs(CS_ARCH_X86,CS_MODE_64);decoded={};calls={};ranges=[]
for row in spec['regions']:
 raw=b.data[b.off(row['start']):b.off(row['end'])]
 assert raw.hex()==row['bytes'] and hashlib.sha256(raw).hexdigest()==row['sha256']
 ins=list(md.disasm(raw,row['start']));assert sum(i.size for i in ins)==len(raw)
 calls[row['function']]=[[i.address,i.op_str]for i in ins if i.mnemonic=='call']
 decoded.update({i.address:(i.mnemonic,i.op_str)for i in ins})
 ranges.append([row['function'],row['start'],row['end'],len(raw)])
assert calls==spec['calls']
for at,mn,op in spec['pins']:assert decoded[at]==(mn,op)
for at,target in spec['bindings']:assert b.qwords(at,1)[0]==target
rows=(here/'global-geometric-ordinary-bodies.tsv').read_text().splitlines()[1:]
assert [[fn,int(s,16),int(e,16)+1,int(n)]for fn,s,e,n in (r.split('\t')for r in rows)]==ranges
# Semantic pins distinguish actual signed 64-bit time/tag from XY/raw ordinal.
semantic={
 0x18008de0b:('mov','rax, qword ptr [rsp + 0x50]'),
 0x18008de10:('mov','qword ptr [rbx + 0x30], rax'),
 0x180090d48:('mov','qword ptr [rsp + 0x20], rcx'),
 0x180090d56:('call','0x18008ddb0'),
 0x180097b27:('divss','xmm0, dword ptr [r15 + 0x10]'),
 0x180097b3f:('mov','dword ptr [rsi + 0x28], eax'),
 0x180097b5b:('cmp','dword ptr [rbx + 8], 4'),
 0x180097b6e:('mov','dword ptr [rsi + 0x30], edi'),
 0x180098509:('movsxd','rcx, dword ptr [rdx + 0x2c]'),
 0x18009850d:('movsxd','rax, dword ptr [r10 + r8 + 0x10]'),
 0x180098516:('add','rax, qword ptr [rsp + 0x28]'),
 0x18009851c:('movsxd','rax, dword ptr [rdx + 0x28]'),
 0x180098523:('imul','rax, rcx'),
 0x180098527:('add','rax, qword ptr [rsp + 0x28]')}
for at,v in semantic.items():assert decoded[at]==v
f=lambda at:struct.unpack('<f',b.data[b.off(at):b.off(at)+4])[0]
assert f(0x18009_7b27+0x62869)==1_000_000 # prior MOVSS ends at97B27
assert f(0x18009_7b3b+0x62675)==.5
capture=a.capture.read_bytes();assert hashlib.sha256(capture).hexdigest()=='1f2d2f6e2bb57e6d249fc11325a5e9736f358a5cd9dce1fe2664dd8ecdebea58'
actual=json.loads(capture)['actual'];structural=[]
for s in actual:
 assert s['complete'] and not s['faulted'] and s['lineCount']==s['alternatives']==s['choiceCount']==1
 assert s['wordFlags']==(0x10000 if s['name']=='II' else 0) and s['wordIndex']==0 and len(bytes.fromhex(s['sourceLines56']))==56
 angle=struct.unpack_from('<d',bytes.fromhex(s['sourceLines56']),24)[0]
 assert math.isfinite(angle)
 end=0;chars=[]
 for l in s['links']:
  assert l['ordinary'] and l['line']==0 and l['metadataFlags']==(0x10000 if s['name']=='II' else 0) and not l['reused']
  row=l['records'][l['marker']];flags=int.from_bytes(bytes.fromhex(row['scalar40'])[8:12],'little')
  assert flags&0x400==0
  copied=len(bytes.fromhex(row['utf8'])) if flags&0xc00==0 else 0;end+=copied
  chars.append(dict(node=l['node'],marker=l['marker'],line=l['line'],selectionId=l['selectionId'],wordIndex=0,lastUTF8Byte=end-1,selectedFlags=flags))
 structural.append(dict(name=s['name'],wordFlags=s['wordFlags'],characters=chars,selectedUTF8Bytes=end,meanAngle=angle,
  wordCharacterStart=0,wordCharacterEnd=len(chars),wordLineFirst=0,wordLineEnd=1))
assert len(actual)==13 and sum(len(s['characters'])for s in structural)==37
assert sum(s['selectedUTF8Bytes']for s in structural)==71
assert sum(s['meanAngle']!=0 for s in structural)==4
# Independent symbolic counterexample, NOT native geometry numerical oracle:
# same XY and source IDs admit distinct constructor metadata and distinct V18.
assert 10_000*3+500 != 5_000*3+500 != 5_000*3+0
point=(root/'src/InkRecognition/StrokeTextRecognizer.cs').read_text(encoding='utf8')
assert 'record struct InkPoint(float X, float Y)' in point or 'record struct InkPoint(float X,float Y)' in point
report=dict(status='PASS STATIC proposal; full geometry blocked on typed timing lineage',vendorExecution=False,
 dependencies=dependencies,fullFunctions=len(ranges),linearBytes=sum(r[3]for r in ranges),instructionPins=len(spec['pins']),bindings=len(spec['bindings']),semanticPins=len(semantic),
 directCallGraph=calls,actualStructuralProjection=structural,
 counts=dict(parents=13,characters=37,selectedUTF8Bytes=71,nonzeroMeanAngles=4),
 nativeOrder=['actual WORD/TEXT parent before selected-source overlay','050660 structural WORD64/CHAR80/line48','mean retained F8 angle then cos/sin','04B4C0 including04AA60 moments','04E480 including04D2C0 source.V50 and04E0E0 metadata writes','WORD64 time min/max aggregation'],
 constructorBoundary=dict(C49='fifth factory argument copied to+30, not defaulted here',
  C1C='rate supplied at descriptor+10 read as float; 1e6/rate+.5 rounded by cvttss2si',
  noTimeChannel='signed format+28 * unsigned pointIndex + signed64 baseTag',
  timeChannel='signed channel32 * signed multiplier32 + signed64 baseTag',
  freshINPUT='existing C20 registration/selected copy does not establish raw constructor defaults',
  pending='full upstream fresh raw ingestion constructor receipt assigning actual rate/baseTag; no absence or zero inferred'),
 proposal=dict(newOnly=True,oldRuntimeSeams=0,firstBoundedOutput='incomplete structural/rotation/moments/vertical prefix with written masks',
  ready2DB=False,geometryRuntimeImplemented=False,geometryNumericalParity=False,
  excludes=['synthetic timing','synthetic2DB or238B0','selected overlay as preselection parent','unproved transformed/DOM/prior owners','missing geometry treated as success']),
 qualityClaim=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f"PASS STATIC{len(ranges)} full functions/{len(spec['pins'])} instructions/7 bindings; actual13 structural projections only; timing admission still pending")
