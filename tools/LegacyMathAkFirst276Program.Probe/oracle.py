"""Pure raw-DATA oracle. Does not load .NET or invoke profile/model/source code."""
from pathlib import Path
import hashlib,json,struct,sys,copy,importlib.util
D=Path(__file__).resolve().parent
SHA='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
def strict(a,b,path='$'):
 if type(a)is not type(b):raise AssertionError((path,'type',type(a).__name__,type(b).__name__))
 if isinstance(b,dict):
  if a.keys()!=b.keys():raise AssertionError((path,'keys'))
  for k in b:strict(a[k],b[k],path+'/'+k)
 elif isinstance(b,list):
  if len(a)!=len(b):raise AssertionError((path,'length'))
  for i,(x,y)in enumerate(zip(a,b)):strict(x,y,path+'/'+str(i))
 elif a!=b:raise AssertionError((path,'value',a,b))
def expected(raw):
 assert len(raw)==3685881 and hashlib.sha256(raw).hexdigest()==SHA
 # Full independent sequential resource decoder validates slot boundaries/model frames.
 p=D/'raw_decode_first.py'
 spec=importlib.util.spec_from_file_location('raw_first',p);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);m.decode(raw)
 u16=lambda p:struct.unpack_from('<H',raw,p)[0]
 u32=lambda p:struct.unpack_from('<I',raw,p)[0]
 s16=lambda p:struct.unpack_from('<h',raw,p)[0]
 desc=dict(SlotOrdinal=2,WhichModel=1,CoordinatePrefix=raw[8697],BoundaryPair=raw[8698],Mode=raw[8699],CalibrationWidth=u16(7591),ModelInputWidth=u32(8713),ModelStart=8702,ModelEnd=8702+8+u32(8706),Parent13A5Start=7465,Parent13A5End=7465+8+u32(7469),Source58=raw[177695])
 fields=[('ModeX','B',1005996),('ModeY','B',1005997),('NormalizationTarget','H',1005998),('LegacyMask','H',1006005),('LegacyFlags','I',1006007),('TemporalPointCount','H',1006011),('SpatialBins','H',1006013),('GridSide','H',1006015),('Quantizer','H',1006017),('DirectionalMask','H',1006019),('GlobalMask','I',1006021),('BaselineMask','I',1006025),('TemporalMask','I',1006029),('SpatialMask','I',1006033),('UpperMask','I',1006037),('TargetCount','I',1006041),('GridScale','I',1006045),('TemporalWindows','h',1006051),('SpatialColumns','h',1006055),('SpatialRows','h',1006057),('UpperColumns','h',1006061),('UpperRows','h',1006063),('UnitPolicy','B',1006065),('Backend','B',1006066)]
 tail=dict(Program={n:struct.unpack_from('<'+t,raw,p)[0]for n,t,p in fields},Minimum=[],Maximum=[],CalibrationPayloadBytes=4772,HasCandidateAuthority=False,SourceFingerprint=SHA)
 for p,n in zip([1006082,1288051,1458131,1628211],[412,279,279,223]):
  assert raw[p]==1 and u16(p+1)==n
  tail['Minimum'].append([s16(p+3+2*i)for i in range(n)])
  tail['Maximum'].append([s16(p+3+2*n+2*i)for i in range(n)])
 before=dict(Description=desc,Program=list(raw[7481:7590]),Minimum=[s16(7593+2*i)for i in range(276)],Maximum=[s16(8145+2*i)for i in range(276)],Tail=tail)
 guards=[]
 def g(name,t,param=None,token=None):guards.append(dict(Name=name,Type='System.'+t,Param=param,TokenMatches=token))
 g('pre-cancel','OperationCanceledException',token=True);g('null-span','ArgumentException','resource');g('truncated','ArgumentException','resource');g('hash-failure','IO.InvalidDataException')
 for i in [-1,109]:g('program-index-'+str(i),'ArgumentOutOfRangeException','index')
 for i in [-1,276]:
  g('minimum-index-'+str(i),'ArgumentOutOfRangeException','index');g('maximum-index-'+str(i),'ArgumentOutOfRangeException','index')
 return dict(Schema='first276-profile-actual-v1',ResourceSha256=SHA,ResourceBytes=len(raw),Before=before,After=copy.deepcopy(before),CallerOverwritten=True,Guards=guards,SuccessfulProfiles=1,HashRejectedProfiles=1,GeometryCalls=0,SourceCalls=0,FirstCalls=0,NnCalls=0,MaximumCaptureBytes=262144)
def audit(actual,raw):strict(actual,expected(raw))
def controls(raw):
 e=expected(raw);count=0
 def refuse(x):
  nonlocal count
  # Same strict validator; derive the immutable raw reference once for controls.
  try:strict(x,e)
  except AssertionError:count+=1;return
  raise AssertionError('tamper accepted')
 # Every exported FIRST lane in BOTH observations, exact integers (bool is rejected).
 for side in ['Before','After']:
  for field in ['Program','Minimum','Maximum']:
   for i in range(len(e[side][field])):
    x=copy.deepcopy(e);x[side][field][i]+=1;refuse(x)
  for key in e[side]['Description']:
   x=copy.deepcopy(e);x[side]['Description'][key]+=1;refuse(x)
 for key in e:
  x=copy.deepcopy(e);del x[key];refuse(x)
 x=copy.deepcopy(e);x['Extra']=0;refuse(x)
 for path in [('Before','Program',0),('Before','Minimum',0),('After','Maximum',275)]:
  x=copy.deepcopy(e);x[path[0]][path[1]][path[2]]=True;refuse(x)
 for side in ['Before','After']:
  for field in ['Minimum','Maximum']:
   for family,row in enumerate(e[side]['Tail'][field]):
    for i in range(len(row)):
     x=copy.deepcopy(e);x[side]['Tail'][field][family][i]+=1;refuse(x)
  for field in e[side]['Tail']['Program']:
   x=copy.deepcopy(e);x[side]['Tail']['Program'][field]+=1;refuse(x)
 for k in ['CallerOverwritten','GeometryCalls','SourceCalls','FirstCalls','NnCalls']:
  x=copy.deepcopy(e);x[k]=0 if isinstance(e[k],bool)else False;refuse(x)
 return dict(StrictTamperRefusals=count,ActualProfileExecuted=False,ExpectedDerivedFromRaw=True)
if __name__=='__main__':
 raw=Path(sys.argv[2]).read_bytes()
 if sys.argv[1]=='--controls':print(json.dumps(controls(raw)))
 else:
  b=Path(sys.argv[1]).read_bytes();assert len(b)<=262144
  def pairs(ps):
   d={}
   for k,v in ps:
    if k in d:raise AssertionError('duplicate key '+k)
    d[k]=v
   return d
  audit(json.loads(b.decode('utf-8'),object_pairs_hook=pairs),raw);print('PASS exact raw-derived profile DATA')
