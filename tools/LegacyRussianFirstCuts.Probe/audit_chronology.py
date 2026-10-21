from pathlib import Path
import hashlib,json,struct,sys
capture=Path(sys.argv[1]);rawpath=Path(sys.argv[2]);p=json.loads(capture.read_bytes());raw=rawpath.read_bytes();assert hashlib.sha256(raw).hexdigest()==p['inputSha256']=='c8fe150f3ca960cf7216da88c1dd90e1045aed2ef4652a8fa66c9e7a85353dce'and p['expectedLabelsRead']is False
inputs=json.loads(raw)['samples'];points=arcs=duplicates=fractions=trailing=0
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
def different(a,b):return abs(a)+abs(b)<abs(f(a-b))*8388608.
for sample,original in zip(p['samples'],inputs):
 assert sample['id']==original['id'];strokes=[[[f(v)for v in xy]for xy in st]for st in original['strokes']];assert len(strokes)==1
 retained=[[i for i,xy in enumerate(st)if i==0 or any(different(v,last)for v,last in zip(xy,st[i-1]))]for st in strokes]
 assert retained==sample['retainedRawIndices'];assert sample['rawCounts']==list(map(len,strokes));indices=retained[0];duplicates+=len(strokes[0])-len(indices)
 expected=[]
 for row in sample['records']:
  ticks=sample['smoothingTicks200'][row[7]];whole,rem=divmod(ticks,200);metadata=whole+int(rem!=0);end=indices[metadata+1]-1 if metadata+1<len(indices)else len(strokes[0])-1
  expected.append([0,0,indices[metadata],metadata,metadata,200-rem if rem else 0,end]);fractions+=int(rem!=0)
 assert expected==sample['chronology'],('metadata',sample['id']);points+=len(expected)
 for arc in sample['arcs']:
  assert len(arc['ranges'])==1
  s,start,end=arc['ranges'][0];assert s==0 and 0<=start<end<=len(expected)
  first=expected[start];last=expected[end-1];tail=end==sample['counts'][0];reference=first[:3]+last[:2]+[last[6]if tail else last[2],first[5],last[5]]
  assert arc['references']==[reference],('range',sample['id'],arc['node'],arc['ordinal'],arc['ranges'],arc['references'],reference)
  trailing+=int(tail);arcs+=1
report=dict(status='PASS',scope='Independent actual raw167210retention +265fixedpoint200/297insertion +218key/endmetadata +ordinarygraph source-key projection on261 FIRST streams',samples=len(inputs),mappedPoints=points,fractionalPoints=fractions,rawAdjacentDropped=duplicates,graphArcs=arcs,physicalTailArcs=trailing,expectedLabelsRead=False,captureSha256=hashlib.sha256(capture.read_bytes()).hexdigest(),limitations=['Allrecordedinputsonetrack; multi-range ordering/merge notcovered','Actualsmoothingticksandgraphmembershipsareinputs, not independentlyderivedhere','Native sourcepath equivalence needsseparate staticcaller proof; keyprojectiononly'])
capture.with_name('chronology-audit.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
