import json,base64,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('new-augmentation-capture.json prior-corrected-raw-capture.json')
a=json.loads(Path(sys.argv[1]).read_bytes());old=json.loads(Path(sys.argv[2]).read_bytes())
assert a['Scenario']=='new-success' and a['NNSourceInvocations']==1
assert a['RawCompact']==base64.b64encode(base64.b64decode(old['compact'])[:old['count']*40]).decode()
assert a['ActualBit8']==old['bit8']>0
t=a['Topology'];assert t['Count']==old['count'] and t['Candidates']>0 and t['MaximumParentDepth']>0
names=['genuine-positive-preclaim-candidates','pre-cancel-exact-token','invalid-residue-beforeclaim','pre-entry-no-live-mutation','actual-selected-eligible-candidates','new-complete','new-repeat','new-thread-bound','old-after-new-eligibility-refusal','post-new-protocol-claim-collision','race-loser-does-not-poison-winner','augmentation-no-additional-NN']
e=a['Evidence'];assert [r['Name'] for r in e]==names
for r in e:
 if 'Pass' in r:assert r['Pass'] is True
assert e[1]['Type']=='System.OperationCanceledException' and e[1]['TokenMatches'] is True
assert e[2]['Type']=='System.ArgumentException'
for i in [6,7,9]:assert e[i]['Type']=='System.InvalidOperationException'
assert e[8]['Type']=='System.NotSupportedException' and e[8]['ExactMessageChecked'] is True and e[8]['Message']=='Active219580 bit8 augmentation is not restored.'
assert e[9]['ExactMessageChecked'] is True and e[9]['Message']=='Actual completed global TOP source stage already claimed or unavailable.'
r=a['Result'];assert 0<r['Candidates']<=t['Candidates'];assert r['Appended']+r['Duplicates']==r['Candidates'] and 0<=r['UpdatedNewDuplicates']<=r['Duplicates']
assert len(base64.b64decode(a['Before120']))==120 and len(base64.b64decode(r['Source26']))%26==0
assert 1<=len(r['Words'])<=1+r['Appended'];assert all(len(base64.b64decode(w))==120 for w in r['Words'])
print(json.dumps(dict(scope='Actual owning eligible lifecycle/accounting + exact prior raw-input binding; NOT independent full219580 numeric/WORD oracle',pass_=True,candidates=r['Candidates'],appended=r['Appended'],duplicates=r['Duplicates'])))

assert a['BeforeSource26Available'] is False
for stage in [a['BeforeState'],a['AfterState']]:
 assert stage['SpanFirst']<=stage['SpanLast']
 words=stage['Words'];assert 1<=len(words)<=4096
 assert all(0<=o<len(words) for o in stage['SelectedOrdinals'])
 pool={}
 assert len(stage['Pool'])<=4096
 for n in stage['Pool']:
  assert n['Key'] not in pool
  rows=base64.b64decode(n['Records']);assert len(rows)%40==0 and len(rows)//40==(n['PackedCount']&0x1fff)>>2
  pool[n['Key']]=rows
 assert sum(len(v)//40 for v in pool.values())<=65536
 flat=0
 for w in words:
  assert len(base64.b64decode(w['Storage120']))==120
  flat+=len(w['FlatReferences']);assert flat<=65536
  for ref in w['FlatReferences']:
   key=ref&0xffffffffff|ref&(1<<56);ordinal=(ref>>40)&65535
   assert key in pool and ordinal<32768 and ordinal<len(pool[key])//40
assert [w['Storage120'] for w in a['AfterState']['Words']]==a['Result']['Words']
assert a['BeforeState']['SelectedOrdinals']==[0]
print('PASS bounded current275/274 export shape+flatref resolution; numeric replay remains separate')
