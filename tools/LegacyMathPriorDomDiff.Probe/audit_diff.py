from pathlib import Path
import json,sys,base64,struct
c=json.loads(Path(sys.argv[1]).read_bytes());fixtures=json.loads((Path(__file__).parent/'alignment-fixtures.json').read_bytes());assert c['AlignmentCases']==len(fixtures)==961
assert [x['Operation']for x in c['PreparedActions']]==[6,7,5,4,4,2]
assert [x['Ordinal']for x in c['PreparedActions']if x['Operation']in[2,4]]==[0,0,0]
for case in c['Actual']:
 a=case['Candidate'];d=case['Diff'];old=a['Temporary'];new=a['Candidate'];reg={x['Id']:x for x in a['Registry']};pending={-x['TemporaryId']:x for x in a['Pending']};expected=[]
 def walk(x,y):
  assert x['Kind']==y['Kind'] and x['Name']==y['Name']
  for attr in [n for n in old if n['Parent']==x['Id'] and n['Kind']==0]:
   found=next(n for n in new if n['Parent']==y['Id']and n['Kind']==0 and n['Name']==attr['Name'])
   assert attr['ValueTag']==found['ValueTag']
   if (attr['Text'],attr['Number'])!=(found['Text'],found['Number']):
    assert attr['Name']=='tag' and attr['ValueTag']==4
    # Independently resolve actual old registry and actual negative candidate pending ID.
    ro,rn=reg[attr['Number']],pending[found['Number']]
    assert ro['Name']==rn['Name']and ro['Ranges']==rn['Ranges']and rn['HasSelection']and rn['ResolvedId']==-1
    expected.append(dict(Operation=7,NodeAt18=attr['Id'],NodeAt20=0,Ordinal=-1,SourceCandidate=found['Id'],EnvelopeKind=0,Name='tag',OldTag=4,NewTag=4,OldStorage=base64.b64encode(struct.pack('<q',attr['Number'])+b'\0'*8).decode(),NewStorage=base64.b64encode(struct.pack('<q',found['Number'])+b'\0'*8).decode(),OldHeap='',NewHeap='',Filtered=True))
  oc=[n for n in old if n['Parent']==x['Id']and n['Kind']!=0];nc=[n for n in new if n['Parent']==y['Id']and n['Kind']!=0]
  assert len(oc)==len(nc)
  for o,n in zip(oc,nc,strict=True):
   if o['Kind']==1:assert (o['Text'],o['ValueTag'])==(n['Text'],n['ValueTag'])
   else:walk(o,n)
 walk(next(n for n in old if n['Kind']==3 and n['Name']=='mathResult'),next(n for n in new if n['Kind']==3 and n['Name']=='mathResult'))
 assert len(expected)==3 and d['Actions']==expected
 assert d['Complete']and d['Ready']and not d['Failed']and not d['Released']and d['HeaderLive']and d['Produced']==d['Filtered']==3 and d['Kept']==0
 assert 0<d['Visits']<=16777216 and 0<d['MatrixCells']<=1048576
assert len(c['Actual'])==6
print('PASS6 independent actual-source action fields/int64 bytes/CB8 range-name resolution; 961 frozen prepared alignments separately tested; no apply/receiver proof')
