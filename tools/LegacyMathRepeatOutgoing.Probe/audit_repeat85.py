"""Pre-runtime frozen repeat85 source/DOM serializer comparison. DATA only."""
from pathlib import Path
import base64,json,struct,sys,hashlib
from repeat_export_oracle import dom_export,source_export,content
P=Path(__file__).resolve().parent
PRED=P/'ROOT83-LATEV6-CONTENT-PREDICTIONS.json'

def scalar(n):
 t=n['ValueTag'] if n['Kind'] in (0,1)else 0;s=bytearray(16);h=b''
 if t in (1,2):
  b=n['Text'].encode('utf-8',errors='strict')
  if t==1:assert len(b)>=16;h=b;struct.pack_into('<I',s,8,len(b))
  else:assert 1<=len(b)<=15;s[:len(b)]=b;s[15]=len(b)if len(b)<15 else 0
 elif t==4:struct.pack_into('<q',s,0,n['Number'])
 elif t==7:struct.pack_into('<i',s,0,n['Number'])
 else:assert t in (0,3,10)
 return dict(ValueTag=t,Storage=base64.b64encode(s).decode(),Heap=base64.b64encode(h).decode())

def treecheck(nodes,tree):
 byid={n['Id']:n for n in nodes};assert tree
 for row in tree:
  n=byid[row['Id']];assert row==dict(Id=n['Id'],Kind=n['Kind'],Name=n['Name'],**scalar(n),Attributes=row['Attributes'],Children=row['Children'])
 return tree[0]['Id']

def audit(path,field='Outgoing'):
 cap=json.loads(path.read_bytes());pred={r['case']:r for r in json.loads(PRED.read_bytes())};results=[]
 assert len(cap['Actual'])==len(pred)==6
 for case in cap['Actual']:
  g=case[field];old=g['OriginalBefore'];new=g['LateBefore'];ot=g['OriginalTree'];nt=g['LateTree'];oldroot=treecheck(old,ot);newroot=treecheck(new,nt);previous=pred[case['Case']]
  assert content(old,oldroot,ot)==previous['oldFieldContent'],'Actual owner content differs from previously clone-proved83 input'
  assert content(new,newroot,nt)==previous['newFieldContent'],'Late actual field differs from frozen native replay'
  expected=dom_export(old,new,oldroot,newroot,ot,nt)
  for key in ('RawActions','Dom','FourthWriter','ReleasedScratchRoots'):assert g[key]==expected[key],(case['Case'],key)
  semantic=[dict(Operation=d['Operation'],Path=d['Path'],Ordinal=d['Ordinal'],Payload=content(expected['FourthWriter'],d['PayloadRoot'])if d['PayloadRoot']else None)for d in expected['Dom']]
  assert semantic==previous['expectedSemanticDom']
  expectedsource=source_export(case['CurrentInput']['Registry'],case['Late']['Observed']['Registry'],case['CurrentInput']['Next'],case['Late']['Observed']['Next'])
  assert g['Source']==expectedsource==previous['expectedSource']
  assert old==g['OriginalAfter'] and new==g['LateAfter']
  assert g['OldNext']==case['CurrentInput']['Next'] and g['NewNext']==case['Late']['Observed']['Next'] and g['Epoch']==case['CurrentInput']['AppliedEpoch']
  assert g['ComparisonProfile']=='admitted-library-default-schema0/native-local88-unresolved'
  assert g['Complete']and g['Ready']and not(g['Failed']or g['Released'])and g['DistinctFourthNames']and g['SameCurrentProjection']
  assert g['Geometry']==case['Identity'];assert 0<g['Reservation']<=64*1024*1024 and g['Visits']>0
  results.append(dict(case=case['Case'],domOperations=[a['Operation']for a in g['Dom']],sourceOperations=[(a['Operation'],a['Id'])for a in g['Source']],paths=[a['Path']for a in g['Dom']],oldActualField=oldroot,newActualField=newroot))
 return dict(PASS=True,evidenceSha256=hashlib.sha256(path.read_bytes()).hexdigest(),cases=results,scope='Independent default-profile rawdiff/fullFourth/scratch/paths/ordinal/typedscalar/source4/6/7 replay from actualoriginal+late observations;83clone only content anchor. Not originalreceiver/native-local88 proof.')
if __name__=='__main__':
 r=audit(Path(sys.argv[1]),sys.argv[3]if len(sys.argv)>3 else'Outgoing');Path(sys.argv[2]).write_text(json.dumps(r,indent=2)+'\n',encoding='utf-8');print(json.dumps(r))
