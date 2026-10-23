"""Independent actual WORD48/CHAR40 provenance and023260 no-control replay."""
import argparse,base64,json,struct,subprocess,sys
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__);p.add_argument('capture',type=Path)
p.add_argument('--selected',type=Path);p.add_argument('--indices',type=Path);args=p.parse_args()
assert (args.selected is None)==(args.indices is None),'Both current parent captures are required together'
j=json.loads(args.capture.read_bytes());words=chars=records=0
if args.selected is not None:
 root=Path(__file__).resolve().parents[2]
 for name,path in [('LegacyMainDocumentSelectedSources',args.selected),('LegacyMainDocumentSourceIndices',args.indices)]:
  subprocess.run([sys.executable,'-I','-B',str(root/'tools'/f'{name}.Probe'/'audit_output.py'),str(path)],check=True)
 selected=json.loads(args.selected.read_bytes());indices=json.loads(args.indices.read_bytes())
 assert len(j['samples'])==len(selected['samples'])==len(indices['samples'])==4
 for sample,parent,index in zip(j['samples'],selected['samples'],indices['samples']):
  expected={c['node']:c['flags']for c in parent['selectedSources']['characters']}
  assert len(expected)==len(parent['selectedSources']['characters'])
  assert sample['visits']==index['charInsertion']
  assert len(sample['characters'])==len(expected)==len(index['charInsertion'])
  assert {c['id']:c['flags']for c in sample['characters']}==expected
  assert {s['id']:s['flags']for s in index['inputs']if not s['word']}==expected
  assert sample['wordBefore']==parent['selectedSources']['wordFlags']
  assert sample['pendingBefore']==parent['pendingAfter']
  expected_words={w['index']:w['before48']for w in parent['words']}
  assert len(sample['words'])==len(expected_words)
  assert [w['before48']for w in sample['words']]==[expected_words[i]for i in range(len(expected_words))]
for s in j['samples']:
 assert s['readyBefore']and s['succeeded']and s['wordCleared']
 assert s['pendingBefore']==s['pendingAfter']
 assert s['rowsInspected']==len(s['words'])
 assert s['wordAfter']==s['wordBefore']&0xffefffff
 sources={}
 for c in s['characters']:
  assert c['id']not in sources and c['records']
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
assert len(j['samples'])==4 and words==8
# Original standalone capture remains a historical12-CHAR control. Current
# source trees may grow; their COMPLETE membership must be independently proven.
if args.selected is None:assert chars==12,'Current captures require --selected and --indices parents'
print(f'PASS independent4 actual owners/{words} fullWORD48/{chars} source flags/{records} fullCHAR40+ownedUTF8 records/selected flags provenance/insertion order; noactivecontrol invocation or transform claim.')
