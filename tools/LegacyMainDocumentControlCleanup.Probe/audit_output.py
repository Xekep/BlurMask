"""Independent actual WORD48/CHAR40 provenance and023260 no-control replay."""
import base64,json,struct,sys
from pathlib import Path
j=json.loads(Path(sys.argv[1]).read_bytes());words=chars=records=0
for s in j['samples']:
 assert s['readyBefore']and s['succeeded']and s['wordCleared']
 assert s['pendingBefore']==s['pendingAfter']
 assert s['rowsInspected']==len(s['words'])
 assert s['wordAfter']==s['wordBefore']&0xffefffff
 sources={}
 for c in s['characters']:
  assert not c['flags']&0x100000
  source_records=[]
  for r in c['records']:
   before=base64.b64decode(r['before40']);after=base64.b64decode(r['after40']);text=base64.b64decode(r['utf8'])
   assert len(before)==40 and before==after
   flags=struct.unpack_from('<I',before,8)[0];length=struct.unpack_from('<I',before,32)[0]
   assert len(text)==length and not flags&0x100000;text.decode('utf8')
   #Exact native control predicate, not a Unicode category guess or AL alias.
   assert not(len(text)==4 and text[:3]==bytes([0xf3,0xb0,0x80])and text[3]in(0x80,0x81))
   source_records.append(flags);records+=1
  sources[c['id']]=source_records;chars+=1
 assert s['visits']==[c['id']for c in s['characters']] #actual insertion-list order
 for word in s['words']:
  before=base64.b64decode(word['before48']);after=base64.b64decode(word['after48'])
  assert len(before)==48 and before==after
  flags=struct.unpack_from('<I',before,8)[0];selected=0
  for node,index in word['selectedLinks']:selected|=sources[node][index]
  #Actual native WORD row8 comes from selected CHAR records of this WORD;
  #it is neither selected ALT0-only nor assumed zero from a constructor.
  assert flags==selected and not flags&0x100000;words+=1
assert len(j['samples'])==4 and words==8 and chars==12
print(f'PASS independent4 actual owners/{words} fullWORD48/{chars} source flags/{records} fullCHAR40+ownedUTF8 records/selected flags provenance/insertion order; noactivecontrol invocation or transform claim.')
