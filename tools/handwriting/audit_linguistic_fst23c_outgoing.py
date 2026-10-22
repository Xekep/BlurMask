"""STATIC actual native outgoing-query primitive; no vendor execution."""
import argparse,hashlib,json,struct
from pathlib import Path
from reference_fst23c_outgoing import actual_main135_costs,outgoing
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();sha=lambda x:hashlib.sha256(x).hexdigest()
d=json.loads(Path(__file__).with_name('linguistic-fst23c-outgoing-pins.json').read_bytes());raw=(a.sdk/d['binary']).read_bytes();assert sha(raw)==d['sha256'];pe=struct.unpack_from('<I',raw,60)[0];op=pe+24;assert raw[pe:pe+4]==b'PE\0\0'and struct.unpack_from('<H',raw,op)[0]==0x20b;base=struct.unpack_from('<Q',raw,op+24)[0];at=op+struct.unpack_from('<H',raw,pe+20)[0];sections=[]
for i in range(struct.unpack_from('<H',raw,pe+6)[0]):
 _,v,n,f=struct.unpack_from('<IIII',raw,at+i*40+8);sections.append((v,n,f))
def read(at,n):
 rva=at-base
 for v,size,f in sections:
  if v<=rva and 0<=n<=size-(rva-v):return raw[f+rva-v:f+rva-v+n]
 raise AssertionError(('PE range',hex(at),n))
for row in d['regions']:
 start=int(row['start'],16);end=int(row['endExclusive'],16);assert end-start==row['bytes']and sha(read(start,end-start))==row['sha256']
for row in d['completeLeaves']+d['tables']:
 b=bytes.fromhex(row['bytes']);assert read(int(row.get('at',row.get('start')),16),len(b))==b
q=lambda at:struct.unpack('<Q',read(at,8))[0]
assert q(0x1802baea0)==0x18010bd30 and q(0x1802bada8)==0x18010a910
assert q(0x1802bae40)==0x18010b3d0 and q(0x1802bae70)==0x18010bac0
assert q(0x1802e9280)==0x18023e380 and q(0x1802e92b0)==0x18023fff0 and q(0x1802e92b8)==0x180240000
data=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();assert sha(data)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
assert struct.unpack_from('<HHIi',data,299576)==(0x109,16,98,299)
assert struct.unpack_from('<I',data,299100)[0]==0x3f800000
descriptors=[]
for i in range(99):
 offset,label,b6,b7,pad=struct.unpack_from('<IBBBB',data,299588+8*i);descriptors.append(dict(field0=offset,runtimeLabel16=65535 if label==255 else label,runtimeByte6=b6,runtimeByte7=b7))
words=list(struct.unpack_from('<299I',data,300384));costs=actual_main135_costs();rows=[outgoing(descriptors,words,state,costs)for state in range(98)]
assert all(0<=r['field0']<=299 for r in descriptors)and all(descriptors[i]['field0']<=descriptors[i+1]['field0']for i in range(98))
assert all(0<=x<98 for x in words)and sum(len(r['rows'])for r in rows)==299
assert len(costs)==256 and costs[0]==-155648 and costs[128]==0
report=dict(status='PASS STATIC actual13A.V80 outgoing primitive',completeFunctions=len(d['regions']),completeRegions=len(d['regions']),completeLeaves=len(d['completeLeaves']),tablePins=len(d['tables']),actualNumberStates=98,actualOutgoingRows=299,costsSignedMinus128To127=costs,costRawSha256=sha(struct.pack('<256i',*costs)),states=rows,outgoingPrimitiveStaticallyProved=True,full28CQueryImplemented=False,acceptanceOrExpectedStringsUsed=False,nativeExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS STATIC5completefunctions/9completeleaves/'+str(len(d['tables']))+'tables; actual98states/299raw outgoing rows/256MAIN135 costs; full28C mapping/history excluded')
