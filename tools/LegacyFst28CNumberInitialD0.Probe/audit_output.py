"""Independent actual initial-D0 arithmetic; raw owning graph/geometry are diagnostic inputs, not an OCR golden."""
import argparse,ast,copy,hashlib,json,math,struct,types
from pathlib import Path
import numpy as np
p=argparse.ArgumentParser();p.add_argument('models',type=Path);p.add_argument('capture',type=Path);a=p.parse_args()
R=Path(__file__).resolve().parents[2]
def definitions(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
 ns={};exec(compile(tree,str(path),'exec'),ns);return ns
def f(x):return float(np.float32(x))
# Reuse the ORIGINAL independent raw source/AL/v110/M parser, including its
# complete prefix partition and empty-group assertion. No returned C# masks.
path=R/'tools/LegacyFst28CNumberSymbolMasks.Probe/audit_output.py';text=path.read_text(encoding='utf-8-sig')
text=text[text.index('ak=(a.models'):text.index('logical=')]
raw={'a':a,'R':R,'struct':struct,'hashlib':hashlib,'ast':ast}
exec(compile(text,str(path),'exec'),raw)
ak=raw['ak'];expected=raw['expected'];header,label=definitions(R/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py')['labels'](ak)
common=definitions(R/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py')
engine=json.loads((a.models/'ru-RU/legacy-engine-profile.json').read_text())
# These four default-table byte hashes are independently pinned to native2B1
# table548-byte extents by the merged whole-producer proof, not caller JSON IDs.
for mode,key,digest in [('temporal','minima','48e3034ea277c9fa1dbbc6ffba6f841452ecd970803c7bc69bb83e6914d44a48'),('temporal','maxima','fb4c595107c923d7280041710949079f5d867111057437cd59fc132f16c1833b'),('spatial','minima','a8a6e49ffa0dfbe044d20c1cda2ed396750e5bd1c06901980b8b321f72622c7e'),('spatial','maxima','0f6882f5871f4ef4d7e954b97af3c2e6d2dd7fdce5ad905b2b01a2f20eab0232')]:
 assert hashlib.sha256(struct.pack('<274h',*engine[mode][key])).hexdigest()==digest
hidden=[math.floor(1024*math.tanh(3*i/47))for i in range(48)]
assert engine['activations']['hidden']==hidden
actual_inputs=common['independent_actual2b1_inputs'](R,engine)
nn=definitions(R/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');network=nn['network'];exp_prob=nn['exp_prob']
geometry=definitions(R/'tools/LegacyFirstRowContext.Probe/reference_controls.py')['reference']
dynamic=definitions(R/'tools/LegacyDynamicWeights.Probe/reference_controls.py')['reference']
costs=definitions(R/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['reference']
weights_profile=[struct.unpack_from('<H',ak,195771)[0]]+list(struct.unpack_from('<8f',ak,195773))
pos=566330
def take(fmt):
 global pos
 v=struct.unpack_from('<'+fmt,ak,pos)[0];pos+=struct.calcsize('<'+fmt);return v
pos+=1;fallback=take('f');pos+=1+8
for _ in range(take('H')):
 length=take('H');pos+=length*2
pos+=4;other=[take('f')for _ in range(5)]
assert take('B')==0;pos+=8;assert take('B')==0 and take('h')==0
caps=[take('i'),take('H'),take('H')];pos+=13;assert take('f')==0
cache_profile=[fallback]+other+caps
reserved=list(header[4:7])+[label.get(s,65535)for s in ['\U000f0000','\u200c','\u200d']]
offset,lv,rv,reverse,maxwindow,blank,cutoff,retain,selector,pointmean,extended=struct.unpack_from('<3fBBHf4B',ak,195746)
assert(reverse,maxwindow,blank,retain,selector,pointmean,extended)==(0,3,28,1,2,0,1)
combine=definitions(R/'tools/LegacyMainGlobalContext.Probe/audit_output.py')
combine.update(offset=offset,lv=lv,rv=rv);combine['combine'].__globals__.update(offset=offset,lv=lv,rv=rv)
def fixed(values):return [((int(f(x*1024))+32768)&65535)-32768 for x in values]
def reservations(c,t,windows):
 n,g,ar,s,d,p0,p=(c[k]for k in ['n','g','a','s','d','p0','p']);r=c['r']
 b=1411577+512*(p0+p)+128*d+256*s+16*(n+g+ar)+1024*windows+327680*t+4096
 w=4887306+n+g+ar+32*(p0+p)+64*s+64*d+4*d*s+r*(t+64)+4096
 w+=windows*(5828+32768+256*min(p,4096)+16*s+16*d+2048)
 w+=t*(368720+64*255**2+64*4096**2+32768+8*d+16*windows+64*255+64*s+4096)
 return b,w
def audit(j):
 assert j['schema']==1 and j['policy']=='actual-direct-number-fresh-node0-span0-reusefalse' and j['query']is False
 required=[v+'-'+str(i)for i in range(4)for v in ['same-cohort','initial-only-once','one-shot-replay']]
 required+=['four-real-foreign-graphs','null-masks','empty-ak','actual-foreign-english-ak','auth-failure-did-not-claim','pure-maximum-cardinalities','pure-domain-excess','one-byte-short','one-work-short','exact-actual-envelope','cancel-before-entry','fault-after-group','fault-poisoned','one-concurrent-claim-winner','authenticated-private-snapshot','defensive-group-and-mask-copies','all-detached-output-buffers-defensive','group-minus-one','inspection-cancel','parallel-detached-inspection']
 required+=['resource-mutation-'+str(i)for i in [0,99,5233,189755,195871,195895,283697,367899,467101,566416,572762]]
 required+=['byte-domain-'+str(i)for i in [-1,32983514,9223372036854775807]]+['work-domain-'+str(i)for i in [-1,69123317897,9223372036854775807]]
 stages=['before-snapshot','after-snapshot','after-hash','after-claim','after-model','after-context','before-group-0','after-group-0','before-permissions','before-refresh','before-publish']
 required += [p+s for s in stages for p in ['cancel-','checkpoint-reached-']]+['poisoned-claim-'+s for s in stages[3:]]
 assert len(j['cases'])==4 and len(j['guards'])==len(required)==79 and set(j['guards'])==set(required)
 groups=windows_total=channels=0
 for i,case in enumerate(j['cases']):
  assert case['index']==i and case['target']==100
  assert case['mask']==expected['primary'] and case['secondaryMask']==expected['secondary']
  points=[[[f(v)for v in p]for p in s]for s in case['points']];counts=case['counts'];counts=dict(counts,r=len(case['arcs']))
  assert counts['s']==len(points) and counts['p']==sum(map(len,points)) and counts['g']==len(case['allGroups'])
  assert [g['id']for g in case['allGroups']]==list(range(counts['g']))
  assert len(case['pointKeys'])==len(case['endingKeys'])==len(points)
  assert all(len(p)==len(k)==len(e)for p,k,e in zip(points,case['pointKeys'],case['endingKeys']))
  owners={d['ordinal']:d for d in case['descriptors']};accepted=[];window_count=0
  for d in case['descriptors']:
   for start in range(d['first']+1,d['last']+1):
    for end in range(start,min(start+maxwindow,d['last']+1)):
     reference=max(p[0]for stroke in points[d['first']:start]for p in stroke)
     features=geometry(points[start:end+1],d['height'],reference,d['baseline'],False,False)
     probability=exp_prob(network(ak,189755,fixed(features),hidden));mass=f(0.)
     for k,v in enumerate(probability):
      if k!=blank-1:mass=f(mass+f(v/32768))
     sx=f(0.);cnt=0
     for stroke in points[start:end+1]:
      for point in stroke:sx=f(sx+point[0]);cnt+=1
     if cutoff<mass:accepted.append((d['ordinal'],f(sx/cnt),[f(v/32768)for v in probability]))
     window_count+=1
  selected=[]
  for arc in case['arcs']:
   if arc['span']==0 and arc['group']not in selected:selected.append(arc['group'])
  assert selected==case['selected']==[row['group']for row in case['rows']]
  assert (case['bytes'],case['work'])==reservations(counts,len(selected),window_count)
  permission=[0]*192;eligibility=[0]*195
  for slot in reserved:
   if slot!=65535:permission[slot]=3
  for row in case['rows']:
   actual=case['allGroups'][row['group']];assert actual['kind']==0
   assert actual['descriptor']==row['descriptor'] and actual['secondary']==row['secondary']
   assert row['storedRanges']==actual['ranges']
   key=lambda r:case['pointKeys'][r[0]][r[1]]
   ranges=sorted(actual['ranges'],key=key)
   assert ranges==row['ranges'] and [key(r)for r in ranges]==row['startKeys']
   rawstrokes=[points[s][lo:hi]for s,lo,hi in ranges]
   d=owners[row['descriptor']];base=f(d['baseline']);height=f(d['height'])
   if row['secondary']!=65535:
    other=owners[row['secondary']];base=max(base,f(other['baseline']));height=f(base-min(f(d['baseline']-d['height']),f(other['baseline']-other['height'])))
   assert f(row['baseline'])==base and f(row['height'])==height
   minimum=min(p[0]for stroke in rawstrokes for p in stroke);assert minimum==f(row['minX'])
   normalized=[[[min(32767,int(f(f(f(p[0]-minimum)*100)/height))),min(32767,int(f(f(f(p[1]-base)*100)/height)))]for p in stroke]for stroke in rawstrokes]
   assert normalized==row['normalized'] and row['baselineShort']==[0,100]
   boundaries=[1024 if ranges[0][1]==0 else -1024,1024 if ranges[-1][2]==len(points[ranges[-1][0]])else -1024]
   assert boundaries==row['boundaries']
   sx=f(0.);cnt=0
   for s,lo,hi in actual['ranges']:
    for p in points[s][lo:hi]:sx=f(sx+p[0]);cnt+=1
   observations=[(x,p)for owner,x,p in accepted if owners[owner]['line']==d['line']]
   context=combine['combine'](observations,f(sx/cnt),f(d['height']));assert context==row['context']
   inputs=actual_inputs(normalized,context,boundaries)
   assert inputs==row['recomputedInputs'],('independent full vectors vs diagnostic numeric replay',i)
   probabilities=[exp_prob(network(ak,n,v,hidden))for n,v in zip([195895,283697,367899,467101],inputs)]
   assert probabilities==row['probabilities'],('NN',i)
   dyn=dynamic(rawstrokes,weights_profile);np.testing.assert_allclose(row['dynamic'],dyn[:3],rtol=1e-6,atol=1e-6)
   assert dyn[3]==row['dynamicPoints']
   expected_weights=[f(dyn[2])]*4+[0.]*3
   assert struct.pack('<7f',*row['weights'])==struct.pack('<7f',*expected_weights),('exact float32 derived weights',i)
   # Captured weights are checked above as diagnostic values only. They must
   # never authorize the independently reconstructed final integer channel.
   integer=costs([dict(probabilities=p,weight=expected_weights[k])for k,p in enumerate(probabilities)]+[None]*3,1,expected['primary'][:180])
   assert integer==row['integerCosts']
   whole=len(ranges)==1 and ranges[0][1]==0 and ranges[0][2]==len(points[ranges[0][0]])
   zero=whole and case['pointKeys'][ranges[0][0]][0][2]==0 and case['endingKeys'][ranges[0][0]][ranges[0][2]-1][2]==0
   assert(whole,zero)==(row['whole'],row['zero'])
   if whole:assert row['probability']==case['sourceProbabilities'][ranges[0][0]]==0
   else:assert row['probability']is None
   final,candidates=common['cache'](integer,normalized,expected['primary'],header,label,cache_profile,whole,zero)
   assert final==row['cacheCosts'] and candidates==sorted(row['candidates'])
   for k,v in enumerate(final):
    if v<0x4000000:eligibility[k]=1
    if k<192:permission[k]|=(3 if v<0x4000000 else 0)|(4 if v<40000 else 0)
   groups+=1;channels+=sum(map(len,inputs))
  assert permission==case['permissions'] and eligibility==case['eligibility']
  current=[65535 if code<192 and not(permission[code]&1)else code for code in expected['preserved']]
  assert current==case['primary'] and case['secondary']==[code if code>=192 else 65535 for code in expected['preserved']]
  windows_total+=window_count
 return dict(owners=4,groups=groups,networks=4*groups+windows_total,contextWindows=windows_total,independentlyDerivedFeatureChannels=channels,permissionChannels=4*192,eligibilityChannels=4*195)
j=json.loads(a.capture.read_text());result=audit(j);negative=0
mutations=[lambda x:x['cases'][0]['rows'][0]['probabilities'][0].__setitem__(0,-1),lambda x:x['cases'][0]['rows'][0]['integerCosts'].__setitem__(0,17),lambda x:x['cases'][0]['rows'][0]['cacheCosts'].__setitem__(0,17),lambda x:x['cases'][0]['permissions'].__setitem__(0,17),lambda x:x['cases'][0]['eligibility'].__setitem__(0,17),lambda x:x['cases'][0]['primary'].__setitem__(0,17),lambda x:x['cases'][0]['rows'].clear(),lambda x:x['cases'][0]['arcs'][0].__setitem__('group',1),lambda x:x['cases'][0]['rows'][0]['context'].__setitem__(0,17),lambda x:x['cases'][0].__setitem__('work',1),lambda x:x.__setitem__('query',True)]
mutations.append(lambda x:x['cases'][0]['rows'][0]['recomputedInputs'][0].__setitem__(0,17))
mutations.append(lambda x:x['cases'][0]['rows'][0]['weights'].__setitem__(0,float(np.nextafter(np.float32(x['cases'][0]['rows'][0]['weights'][0]),np.float32(math.inf)))))
for mutate in mutations:
 bad=copy.deepcopy(j);mutate(bad)
 try:audit(bad)
 except AssertionError:negative+=1
 else:raise AssertionError('Accepted corrupted D0 capture')
print(json.dumps(dict(status='PASS initial actual D0 only; no OCR/query claim',**result,guards=len(j['guards']),negativeCaptures=negative,captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest())))
