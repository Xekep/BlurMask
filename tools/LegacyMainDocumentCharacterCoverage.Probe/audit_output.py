"""Independent01C740 preferred-source selection and native C21 subtraction."""
import base64,importlib.util,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('ops',root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py');ops=importlib.util.module_from_spec(spec);spec.loader.exec_module(ops)
j=json.loads(Path(sys.argv[1]).read_bytes());b64=base64.b64decode;calls=total=0
for s in j['samples']:
 assert [word['index']for word in s['words']]==list(reversed(range(s['wordCount'])))
 ordinary=[]
 for word in reversed(s['words']):
  for c in word['chars']:
   if c['positive']:ordinary=ops.union(ordinary,c['key'])
 assert ordinary==s['wordOrdinary']
 for word in s['words']:
  initial=s['wordOrdinary']if s['wordOrdinary']else s['wordOriginal']
  assert word['ordinary']==bool(s['wordOrdinary'])and word['initial']==initial
  remaining=[r[:]for r in initial]
  assert [c['index']for c in word['chars']]==list(range(len(word['chars'])))
  for c in word['chars']:
   assert c['priorCount']==0 and c['prior']is False
   raw=b64(c['sourceKey16']);keys=[list(struct.unpack_from('<QQ',raw,i))for i in range(0,len(raw),16)]
   assert keys==c['key']
   remaining=ops.subtract(remaining,keys,s['counts']);total+=1
  assert remaining==word['remaining']==[]and b64(word['before48'])==b64(word['after48']);calls+=1
 assert s['subtractions']==sum(len(w['chars'])for w in s['words'])
 assert b64(s['lineBefore56'])==b64(s['lineAfter56'])and len(b64(s['lineAfter56']))==56
 assert s['pendingBefore']==s['pendingAfter']and s['lastPending']=='TEXT'
assert len(j['samples'])==4 and calls==8
print(f'PASS independent4 actual owners/{calls} reverse WORD calls/{total} forward CHAR subtractions/raw16B key identity/preferred-source selection/8 empty remainders; no labels or acceptance.')
