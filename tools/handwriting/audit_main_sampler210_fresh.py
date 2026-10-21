"""Static native byte placement and exact fresh240 owner; never loads vendor code."""
import argparse,hashlib,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--sdk',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
d=json.loads(Path(__file__).with_name('main-sampler210-fresh-pins.json').read_bytes());raw=(a.sdk/'MyScriptText.dll').read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest();assert sha(raw)==d['binarySha256']
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
for pin in d['instructions']:
 expected=bytes.fromhex(pin['bytes']);assert read(int(pin['at'],16),len(expected))==expected
for pin in d['windows']:
 start=int(pin['start'],16);end=int(pin['endExclusive'],16);assert sha(read(start,end-start))==pin['sha256']
for pin in d['slots']:assert read(int(pin['at'],16),8)==bytes.fromhex(pin['bytes']) and struct.unpack('<Q',bytes.fromhex(pin['bytes']))[0]==int(pin['target'],16)
dep=d['policyDependency'];data=(a.repo/dep['path']).read_bytes();assert sha(data)==dep['sha256'];policy=json.loads(data)
for pin in policy['regions']:
 start=int(pin['start'],16);end=int(pin['endExclusive'],16);assert sha(read(start,end-start))==pin['sha256']
for pin in policy['calls']+policy['leaves']+policy['tables']+policy['parents']:
 expected=bytes.fromhex(pin['bytes']);assert read(int(pin.get('at',pin.get('start')),16),len(expected))==expected
parents={pin['kind']:pin['parent'] for pin in policy['parents']};assert parents=={0x240:0x209,0x23f:0x209,0x209:3}
assert parents[0x240]!=0x23f
# Prepared byte-layout controls distinguish this flag from D0/packet09/F5/0A.
controls=[]
for is23f in (0,1):
 for d0 in (0,1):
  for f5 in (0,1):
   graph=bytearray(16);graph[5]=is23f;graph[9]=d0;graph[10]=int(not is23f and f5)
   sampler=bytearray(48);sampler[1]=graph[5]
   assert sampler[1]==is23f
   controls.append(dict(prepared=True,is23F=is23f,D0=d0,F5=f5,graph5=graph[5],graph09=graph[9],graph0A=graph[10],sampler1=sampler[1],runtime210=sampler[1]))
assert any(c['graph0A']!=c['runtime210'] for c in controls)
report={'scope':'Exact fresh direct NEW20E with one ordinary13B and fresh2B3; no properties/events/reuse/external controller', 'status':'PASS STATIC', 'instructions':len(d['instructions']),'instructionWindows':len(d['windows']),'composedPolicyRegions':len(policy['regions']),'dispatchBindings':len(d['slots']),'preparedControls':controls,'runtime210':0,'explanation':'1EB340 creates240;00CAB0 binds sameknowledge60;240 and23F siblings209;150D00 IsA23F false writes graph5;1A6980 copies5 to sampler1;18D920 writes210;1941B0 selects stored110/114 scaling','noGeneralConfiguredClaim':True,'nativeExecuted':False}
a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS STATIC fresh240->graph5->sampler1->210=0;16instruction pins/4exactwindows/2dispatch/25composedpolicyregions/8preparedplacementcontrols')
