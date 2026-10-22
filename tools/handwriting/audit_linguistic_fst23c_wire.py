"""STATIC exact raw23C/109 owning-reader layout; no state/query interpretation."""
import argparse,hashlib,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();sha=lambda b:hashlib.sha256(b).hexdigest()
d=json.loads(Path(__file__).with_name('linguistic-fst23c-wire-pins.json').read_bytes());pebytes=(a.sdk/d['binary']).read_bytes();assert sha(pebytes)==d['sha256'];pe=struct.unpack_from('<I',pebytes,60)[0];op=pe+24;assert pebytes[pe:pe+4]==b'PE\0\0' and struct.unpack_from('<H',pebytes,op)[0]==0x20b;image=struct.unpack_from('<Q',pebytes,op+24)[0];sectionat=op+struct.unpack_from('<H',pebytes,pe+20)[0];sections=[]
for i in range(struct.unpack_from('<H',pebytes,pe+6)[0]):
 _,v,n,f=struct.unpack_from('<IIII',pebytes,sectionat+i*40+8);sections.append((v,n,f))
def read(at,n):
 rva=at-image
 for v,size,f in sections:
  if v<=rva and 0<=n<=size-(rva-v):return pebytes[f+rva-v:f+rva-v+n]
 raise AssertionError(('PE span',hex(at),n))
for row in d['regions']:
 start=int(row['start'],16);end=int(row['endExclusive'],16);assert end-start==row['bytes']and sha(read(start,end-start))==row['sha256']
for row in d['leaves']+d['tables']:
 raw=bytes.fromhex(row['bytes']);assert read(int(row.get('at',row.get('start')),16),len(raw))==raw
assert struct.unpack('<Q',read(0x1802e6b08,8))[0]==0x1801e1fb0 and struct.unpack('<Q',read(0x1802e6b48,8))[0]==0x1801e1560
raw=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();assert sha(raw)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
assert struct.unpack_from('<IIIHH',raw,299000)==(0x101fe90,2572,0x13a,0,0x202)
version,labels,states,transitions=struct.unpack_from('<HHIi',raw,299576);assert(version,labels,states,transitions)==(0x109,16,98,299)
cursor=299588;assert cursor%4==0;descriptors=[]
for i in range(states+1):
 at=cursor;word0,labelbyte,raw5,raw6,padding=struct.unpack_from('<IBBBB',raw,at);cursor+=8
 descriptors.append(dict(index=i,offset=at,rawHex=raw[at:cursor].hex(),field0=word0,runtimeLabel16=65535 if labelbyte==255 else labelbyte,runtimeByte6=raw5,runtimeByte7=raw6,opaquePadding=padding))
assert cursor==300380;rawField20,=struct.unpack_from('<I',raw,cursor);cursor+=4;words=list(struct.unpack_from('<'+'I'*transitions,raw,cursor));assert cursor+4*transitions==301580
assert rawField20==12 and descriptors[-1]['field0']==transitions
report=dict(status='PASS STATIC raw23C109 wire',completeFunctions=3,completeRegions=3,completeLeafRegions=1,tablePins=4,resourceOffset=299000,resourceEnd=301580,serializedVersionOffset=299576,payloadOffset=299578,alignmentOrigin='absolute underlying stream',descriptorOffset=299588,rawField20Offset=300380,transitionOffset=300384,endExclusive=301580,labelCount=labels,stateCount=states,declaredTransitionCount=transitions,rawField20=rawField20,descriptorRawSha256=sha(raw[299588:300380]),transitionRawSha256=sha(raw[300384:301580]),payloadSha256=sha(raw[299578:301580]),descriptors=descriptors,transitionWords=words,querySemanticsProved=False,nativeExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS STATIC3completefunctions/1completeleaf/4tables; actualnumber23C109 16labels/98states/299transitionwords; allrawbytes preserved; field20 semantics excluded')
