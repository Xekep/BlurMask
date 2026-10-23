"""STATIC full initial-number qualifier/category/scorer/heap proposal. Vendor bytes only."""
from pathlib import Path
import argparse,hashlib,json,struct,capstone,subprocess,sys
p=argparse.ArgumentParser()
for name in ['native-repo','models','context-capture','d0-capture','output']:p.add_argument('--'+name,type=Path,required=True)
a=p.parse_args();repo=Path(__file__).resolve().parents[2];pins=json.loads(Path(__file__).with_name('linguistic-fst28c-number-initial-consumer-pins.json').read_bytes());sha=lambda b:hashlib.sha256(b).hexdigest()

def pe(path,expected):
 b=path.read_bytes();assert sha(b)==expected;off=struct.unpack_from('<I',b,60)[0];at=off+24+struct.unpack_from('<H',b,off+20)[0]
 sections=[struct.unpack_from('<IIII',b,at+40*i+8)for i in range(struct.unpack_from('<H',b,off+6)[0])]
 def read(address,n):
  r=address-0x180000000
  for size,start,length,raw in sections:
   if start<=r and n<=start+length-r:return b[raw+r-start:raw+r-start+n]
  raise AssertionError(('outsidePE',hex(address),n))
 return read
text=pe(a.native_repo/'research/myscript/sdk/build/x64/MyScriptText.dll',pins['nativeSHA256']);engine=pe(a.native_repo/'research/myscript/sdk/build/x64/MyScriptEngine.dll',pins['engine']['sha256'])
for row in pins['regions']:assert sha(text(int(row['start'],16),row['bytes']))==row['sha256']
for row in pins['upstreamD0Regions']:assert sha(text(int(row['start'],16),row['bytes']))==row['sha256']
for row in pins['upstreamD0Spans']:
 assert text(int(row['start'],16),row['bytes']).hex()==row['hex'] and sha(bytes.fromhex(row['hex']))==row['sha256']
for row in pins['pointers']:assert text(int(row['at'],16),8).hex()==row['hex']
for row in pins['boundedLeaves']:assert text(int(row['start'],16),row['bytes']).hex()==row['hex']
for row in pins['inheritedCompleteLeaves']:assert text(int(row['start'],16),len(bytes.fromhex(row['bytes']))).hex()==row['bytes']
for key in ['registeredAcousticDescriptor','registeredSamplerDescriptor']:
 r=pins[key];assert text(int(r['at'],16),r['bytes']).hex()==r['hex']
assert struct.unpack('<I',text(0x1802e3c88,4))[0]==0x21d
assert struct.unpack('<I',text(0x1802e15e8,4))[0]==0x297
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
for row in pins['completeCFG']:
 lo=int(row['start'],16);end=int(row['endExclusive'],16);blob=text(lo,end-lo);assert blob.hex()==row['bytes'] and sha(blob)==row['sha256']
 pending=[lo];seen={};edges=[]
 while pending:
  at=pending.pop()
  if at in seen:continue
  assert lo<=at<end;x=next(cs.disasm(text(at,min(15,end-at)),at));assert x.mnemonic not in ['call','int3']
  seen[at]=dict(at=hex(at),instruction=x.mnemonic,operand=x.op_str,bytes=x.bytes.hex())
  if x.mnemonic=='ret':continue
  nexts=[]
  if x.mnemonic.startswith('j'):
   assert x.op_str.startswith('0x');nexts.append(int(x.op_str,16))
   if x.mnemonic!='jmp':nexts.append(at+x.size)
  else:nexts.append(at+x.size)
  for q in nexts:assert lo<=q<end;pending.append(q);edges.append([hex(at),hex(q)])
 assert [seen[x]for x in sorted(seen)]==row['reachableInstructions']and sorted(edges)==row['edges']
for r in pins['engine']['regions']:assert sha(engine(int(r['start'],16),r['bytes']))==r['sha256']
r=pins['engine']['slot'];assert engine(int(r['at'],16),8).hex()==r['bytes'] and struct.unpack('<Q',bytes.fromhex(r['bytes']))[0]==0x180052b50
r=pins['engine']['flagPredicate'];assert engine(int(r['start'],16),r['bytes']).hex()==r['hex'];ins=list(cs.disasm(bytes.fromhex(r['hex']),int(r['start'],16)));assert sum(x.size for x in ins)==47 and sum(x.mnemonic=='ret'for x in ins)==2
for row in pins['sourceLF']:assert sha((repo/row['path']).read_bytes().replace(b'\r\n',b'\n'))==row['sha256LF'],row['path']
ak=(a.models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
for r in pins['rawModelSpans']:assert sha(ak[r['at']:r['at']+r['length']])==r['sha256']
assert struct.unpack_from('<I',ak,188165)[0]==0x297 and struct.unpack_from('<H',ak,188171)[0]==0x106
assert struct.unpack_from('<HH',ak,188222)==(0x201,192)
assert ak[5279:5281]==bytes(2) and struct.unpack_from('<H',ak,572667)[0]==125
assert struct.unpack_from('<3i',ak,566390)==(20480,24576,32768)
# Literal original v110-tail walk after the already-proved prefix, no NN load.
o=566330+14;n=struct.unpack_from('<H',ak,o)[0];o+=2;assert n==0
o+=33;assert ak[o]==0;o+=1;assert struct.unpack_from('<h',ak,o)[0]==0;o+=2;o+=8;assert o==566390
# Historical actual D0/page/AVL numerical oracle stays unchanged in this STATIC checkout.
subprocess.run([sys.executable,'-I','-B',str(repo/'tools/LegacyFst28CNumberInitialConsumerContext.Probe/audit_output.py'),str(a.models),str(a.context_capture),str(a.d0_capture)],check=True)
projection=a.output.with_name(a.output.stem+'-prepared.json')
subprocess.run([sys.executable,'-I','-B',str(Path(__file__).with_name('reference_fst28c_number_initial_consumer.py')),str(a.models),str(a.context_capture),str(a.d0_capture),str(projection)],check=True)
j=json.loads(projection.read_bytes());assert(j['bytes'],j['work'],j['maximumAttempts'])==(754206,16264293,6144)
report=dict(scope='STATIC complete bounded initial-number consumer proposal; no NEW runtime execution',nativeRegions=len(pins['regions']),engineRegions=len(pins['engine']['regions']),completeAccessorCFG=len(pins['completeCFG']),slots=len(pins['pointers'])+1,boundedLeaves=len(pins['boundedLeaves']),sourcePins=len(pins['sourceLF']),rawModelSpans=len(pins['rawModelSpans']),preparedScoringAttempts=sum(len(x['preparedAttempts'])for x in j['predictions']),missingEndAlternates=sum(x['missingAlternate']for x in j['predictions']),preparedHeapCapacity=250,logicalBytes=j['bytes'],work=j['work'],runtimeApproved=False,vendorExecuted=False,preparedProjection=str(projection),preparedProjectionSHA256=sha(projection.read_bytes()),contextCaptureSHA256=sha(a.context_capture.read_bytes()),d0CaptureSHA256=sha(a.d0_capture.read_bytes()))
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS STATIC',report['nativeRegions'],'Text/'+str(report['engineRegions'])+'Engine complete regions/3closedAccessorCFG/32preparedscoring attempts; no runtime/root-list/wordauthority')
