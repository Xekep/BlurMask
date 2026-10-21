"""Pinned native13F/102 and133/100 wire grammar, no native execution."""
import argparse,hashlib,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();sha=lambda b:hashlib.sha256(b).hexdigest()
d=json.loads(Path(__file__).with_name('linguistic-root13f-wire-pins.json').read_bytes());raw=(a.sdk/d['binary']).read_bytes();assert sha(raw)==d['sha256']
pe=struct.unpack_from('<I',raw,60)[0];op=pe+24;assert raw[pe:pe+4]==b'PE\0\0' and struct.unpack_from('<H',raw,op)[0]==0x20b
base=struct.unpack_from('<Q',raw,op+24)[0];assert base==0x180000000
at=op+struct.unpack_from('<H',raw,pe+20)[0];sections=[]
for n in range(struct.unpack_from('<H',raw,pe+6)[0]):
 _,v,z,f=struct.unpack_from('<IIII',raw,at+n*40+8);sections.append((v,z,f))
def read(address,count):
 v=address-base
 for start,n,offset in sections:
  if start<=v and count<=start+n-v:return raw[offset+v-start:offset+v-start+count]
 raise AssertionError(('PE extent',hex(address),count))
for row in d['regions']:
 start=int(row['start'],16);end=int(row['endExclusive'],16);assert end-start==row['bytes'] and sha(read(start,end-start))==row['sha256']
for row in d['instructions']+d['tables']:
 expected=bytes.fromhex(row['bytes']);assert read(int(row['at'],16),len(expected))==expected
assert struct.unpack('<Q',read(0x1802e53b8,8))[0]==0x1801d1240
assert struct.unpack('<Q',read(0x1802e5508,8))[0]==0x1801d2680
assert struct.unpack('<Q',read(0x1802e5318,8))[0]==(0x133<<32|0x13f)
def inventory(data):
 def header(at):
  magic,n,kind,flags,version=struct.unpack_from('<IIIHH',data,at);assert magic==0x101fe90 and n>=8 and flags==0;end=at+8+n;assert end<=len(data);return end,kind,version
 end,kind,version=header(0);assert (end,kind,version)==(len(data),0x13f,0x102)
 sentinel,=struct.unpack_from('<i',data,76);countat=88 if sentinel==-1 else 84;count,=struct.unpack_from('<I',data,countat);cursor=countat+4;rows=[]
 for ordinal in range(count):
  start=cursor;end,kind,version=header(start);cursor=((end+3)&~3)if ordinal+1<count else end;assert cursor<=len(data)
  rows.append(dict(ordinal=ordinal,start=start,endExclusive=end,kind=hex(kind),version=hex(version),paddingHex=data[end:cursor].hex(),sha256=sha(data[start:end])))
 assert cursor==len(data)
 # Decoding is a static observation of1D22E0, not needed by the opaque reader.
 name=data[20:76]
 if sentinel==-1:name=bytes(x^[0x14,0x12,0x19,0x77][i&3]for i,x in enumerate(name))
 return dict(sha256=sha(data),prefixBytes=countat-16,count=count,decodedRootName=name.split(b'\0',1)[0].decode('utf8'),children=rows)
resources={}
for name,count,expected in [('ru-RU/ru_RU-lk-text.lite.res',22,'d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'),('en-US/en_US-lk-text.lite.res',21,'9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9')]:
 data=(a.models/name).read_bytes();assert sha(data)==expected;result=inventory(data);assert result['count']==count;resources[name]=result
ru=resources['ru-RU/ru_RU-lk-text.lite.res'];assert [r['start']for r in ru['children']if r['kind']=='0x13b']==[606964,4575280]
assert [r['start']for r in ru['children']if r['kind']=='0x13d']==[4575188]
report=dict(status='PASS STATIC native13F/102 wire grammar',completeFunctions=3,completeRegions=len(d['regions']),instructionPins=len(d['instructions']),tablePins=len(d['tables']),resources=resources,activationImplemented=False,sourceAdmissionProved=False,nativeExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS STATIC3completefunctions/3regions/35instructions/8tablepins; actualRU22 andEN21 envelopes; fixed56name/68or72prefix/interchild4alignment; noactivation/admission')
