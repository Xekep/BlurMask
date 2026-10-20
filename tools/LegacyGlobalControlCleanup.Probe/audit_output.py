"""Independent genuine-global023260 no-control/prefix oracle; no SDK/runtime import."""
import json,runpy,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)not in(8,9):raise SystemExit('MODELS CHAR PARENT COVERAGE SELECTED INDEX CLEANUP [NATIVE]')
models,chars,parent,coverage,selected,index,cleanup=sys.argv[1:8];saved=sys.argv[:]
try:
 sys.argv=['audit_output.py',models,chars,parent,coverage,selected,index]
 runpy.run_path(str(root/'tools/LegacyGlobalDocumentSourceIndices.Probe/audit_output.py'),run_name='__main__')
finally:sys.argv=saved
states={s['name']:s for s in json.loads(Path(selected).read_bytes())['samples']}
indices={s['name']:s for s in json.loads(Path(index).read_bytes())['samples']}
d=json.loads(Path(cleanup).read_bytes());assert len(d['samples'])==len(states)==13
def prefixes(word,order):
 result=[dict(stage='entered',rows=0,cleared=False,word=word,characters=0),dict(stage='word-row',rows=1,cleared=False,word=word,characters=0),dict(stage='word-cleared',rows=1,cleared=True,word=word&~0x100000,characters=0)]
 for i,_ in enumerate(order,1):result.append(dict(stage='character',rows=1,cleared=True,word=word&~0x100000,characters=i))
 return result+[dict(stage='complete',rows=1,cleared=True,word=word&~0x100000,characters=len(order))]
count=records=0
for sample in d['samples']:
 state=states[sample['name']];ix=indices[sample['name']];assert sample['completed']==state['completed']==ix['completed']
 if not state['completed']:assert sample==dict(name=sample['name'],completed=False,refusal='238B0');continue
 assert sample['word48']==state['word48After']
 raw=bytes.fromhex(sample['word48']);assert len(raw)==48 and int.from_bytes(raw[8:12],'little')&0x100000==0
 assert sample['wordBefore']==state['wordFlags'] and sample['wordAfter']==state['wordFlags']&~0x100000
 assert sample['cleared'] and sample['rows']==1
 assert sample['order']==ix['charInsertion']
 expected=[dict(node=c['node'],flags=c['flags'],records=c['records'])for c in state['characters']]
 assert sample['characters']==expected
 for c in expected:
  assert c['flags']&0x100000==0
  for row in c['records']:
   assert len(bytes.fromhex(row['scalar40']))==40
   if row['utf8']is not None:
    text=bytes.fromhex(row['utf8']).decode('utf8');assert text not in ('\U000f0000','\U000f0001')
  records+=len(c['records'])
 assert sample['prefixes']==prefixes(state['wordFlags'],sample['order']);count+=len(expected)
assert (d['completed'],d['refused'],count,d['characters'],records,d['records'],d['guards'],d['flagControls'])==(11,2,33,33,72,72,178,96)
assert d['claimRace']is True
m=next(s for s in d['samples']if s['name']=='M');all_prefixes=prefixes(m['wordBefore'],m['order'])
for at,f in enumerate(d['failures']):
 assert f['stage']==['Entered','WordRow','WordCleared','Character','Complete'][at]
 expected=all_prefixes[:at+1]
 #The complete observer fires before successful completion is committed.
 if at==4:expected=all_prefixes[:-1]
 last=dict(expected[-1],stage='fault');assert f['prefixes']==expected+[last]
 assert f['rows']==last['rows']and f['cleared']==last['cleared']and f['wordAfter']==last['word']
 assert f['order']==m['order'][:last['characters']]
assert len(d['invalidations'])==2
for disposed,f in zip((False,True),d['invalidations']):
 assert f['disposed']==disposed and f['faulted']and not f['complete']
 assert f['prefixes']==all_prefixes[:-1]+[dict(all_prefixes[-2],stage='fault')]
if len(sys.argv)==9:assert Path(cleanup).read_bytes()==Path(sys.argv[8]).read_bytes(),'Managed/Native capture mismatch'
print('PASS independent11 actual global cleanup owners/33 CHAR/72 full records/WORD48 flags and ordered clear/5 cancellation prefixes/2 late invalidations/178 guards; no active control services or quality claim.')
