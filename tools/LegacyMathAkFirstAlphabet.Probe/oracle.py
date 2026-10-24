"""Strict DATA oracle. Raw dictionary/native literal bytes + old same-input header DATA; never an issuer."""
from pathlib import Path
import json,hashlib,struct,copy,sys
D=Path(__file__).resolve().parent
RESOURCE='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
FIXTURE='ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb'
OLD='89fdc6a8822cad6bfc45ca9f331c8ec7e5eb556d825a89b0b1c78261348d9d06'
def loads(b):
 def pairs(items):
  d={}
  for k,v in items:
   assert k not in d,('duplicate JSON key',k)
   d[k]=v
  return d
 def bad(s):raise AssertionError(('nonfinite JSON',s))
 return json.loads(b,object_pairs_hook=pairs,parse_constant=bad)
def exact(a,b,path='/'):
 assert type(a)is type(b),(path,'type',type(a).__name__,type(b).__name__)
 if isinstance(a,dict):
  assert a.keys()==b.keys(),(path,'keys')
  for k in a:exact(a[k],b[k],path+k+'/')
 elif isinstance(a,list):
  assert len(a)==len(b),(path,'length')
  for i,(x,y)in enumerate(zip(a,b,strict=True)):exact(x,y,path+str(i)+'/')
 else:assert a==b,(path,a,b)
def expected(raw):
 assert hashlib.sha256(raw).hexdigest()==RESOURCE
 assert struct.unpack_from('<IIIHH',raw,18)==(0x101fe90,7145,0x2b,0,0x104)
 at=34;head=struct.unpack_from('<8H',raw,at);assert head==(256,65535,65535,65535,65535,65535,65535,259);at+=16
 n=struct.unpack_from('<H',raw,at)[0];at+=2
 for inc in [32,64,1]:
  table=struct.unpack_from('<'+'H'*n,raw,at);at+=2*n;n=next(v+inc for v in table[::-1]if v!=65535)
 spans=struct.unpack_from('<'+'H'*(n+1),raw,at);at+=2*(n+1)
 entries=[struct.unpack_from('<II',raw,at+8*i)for i in range(spans[-1]+1)];at+=8*len(entries)
 strings=raw[at:at+entries[-1][0]]
 # Independent direct dictionary enumeration, NOT the portable hash implementation.
 mapping={strings[o:entries[i+1][0]]:tag&65535 for i,(o,tag)in enumerate(entries[:-1])}
 assert len(mapping)==258 and head[-1]==259
 nativeLiterals=['27','22','2d','5f','e383bc','e4b880','f3b08082','f3b08083','f3b08080','f3b08081','6f','4f','30','efbfbd']
 labels=[mapping.get(bytes.fromhex(x),65535)for x in nativeLiterals]
 oldRaw=(D/'historical-header14.json').read_bytes();assert hashlib.sha256(oldRaw).hexdigest()==OLD
 old=loads(oldRaw);fRaw=(D/'fixtures.json').read_bytes();assert hashlib.sha256(fRaw).hexdigest()==FIXTURE
 fixture=loads(fRaw);assert len(old['cases'])==len(fixture['cases'])==14
 assert old['resourceSHA']==RESOURCE and old['fixtureSHA']==FIXTURE
 guards=[]
 def guard(name,type_,param=None,token=None):guards.append(dict(Name=name,Type=type_,Param=param,TokenMatches=token))
 guard('profile-pre-cancel','System.OperationCanceledException',token=True)
 guard('profile-null-span','System.ArgumentException','resource');guard('profile-truncated','System.ArgumentException','resource')
 guard('profile-hash-refusal','System.IO.InvalidDataException');guard('caller-A5-overwrite-retained','assertion')
 guard('special-index-negative','System.ArgumentOutOfRangeException','ordinal');guard('special-index-past','System.ArgumentOutOfRangeException','ordinal')
 cases=[]
 for i,(a,b)in enumerate(zip(fixture['cases'],old['cases'],strict=True)):
  assert a['name']==b['name']
  if i==0:
   guard('source-pre-cancel','System.OperationCanceledException',token=True);guard('source-null','System.ArgumentNullException','original')
  name=a['name'];header=b['data']
  assert header['WorkWords']==(header['SegmentCount']>>5)+1 and header['HeapCapacity']==10
  data=dict(Version=1,AlphabetCount=head[-1],SpecialLabels=labels.copy(),Header=header,ProfileRetainedPayload=5875,
   AdditionalPayload=182,AdditionalCopies=226,AdditionalSourceWork=105755,KnownRequestNumericBytes=17,OpaqueRequestBytes=7,
   HasPreparedFirstReceiver=False,HasCandidateAuthority=False)
  cases.append(dict(name=name,data=data))
  for prefix in ['version','count-labels','request-opacity','ledger','no-FIRST','header-W']:guard(prefix+'-'+name,'assertion')
 assert len(guards)==93 and sum(x['data']['Header']['Positive1390Count']for x in cases)>0
 alphabet=dict(Count=head[-1],Labels=labels.copy(),RetainedPayload=5875,ChargedCopies=11782,ChargedSourceWork=2839083)
 return dict(schema='first276-al-source-actual-v1',resourceSHA=RESOURCE,fixtureSHA=FIXTURE,AlphabetBefore=alphabet,AlphabetAfter=copy.deepcopy(alphabet),
  CallerOverwritten=True,cases=cases,guards=guards,successfulProfiles=1,hashRejectedProfiles=1,owningSourceCalls=14,preentrySourceRequests=2,
  firstCalls=0,modelEvaluations=0,processedGraphAuthority=False,matcherRequestAuthority=False,maximumCaptureBytes=131072)
def leaves(a,path=()):
 if isinstance(a,dict):
  for k,v in a.items():yield from leaves(v,path+(k,))
 elif isinstance(a,list):
  for k,v in enumerate(a):yield from leaves(v,path+(k,))
 else:yield path,a
def selftest(raw,out):
 assert not out.exists();out.mkdir();e=expected(raw);exact(e,copy.deepcopy(e));controls=[]
 for path,value in leaves(e):
  q=copy.deepcopy(e);p=q
  for k in path[:-1]:p=p[k]
  p[path[-1]]=not value if type(value)is bool else value+1 if type(value)is int else 'tampered' if value is None else value+'!'
  try:exact(e,q)
  except AssertionError:controls.append('/'.join(map(str,path)))
  else:raise AssertionError(('accepted scalar mutation',path))
 extras=[('missing',lambda q:q.pop('schema')),('extra',lambda q:q.__setitem__('unknown',0)),('case-order',lambda q:q['cases'].reverse()),
  ('drop-case',lambda q:q['cases'].pop()),('bool-for-int',lambda q:q['cases'][0]['data'].__setitem__('AlphabetCount',True)),
  ('float-for-int',lambda q:q['AlphabetBefore'].__setitem__('Count',259.0)),('label-null',lambda q:q['cases'][0]['data']['SpecialLabels'].__setitem__(0,None)),
  ('foreign-header',lambda q:q['cases'][0]['data'].__setitem__('Header',q['cases'][-1]['data']['Header'])),('extra-label',lambda q:q['cases'][0]['data']['SpecialLabels'].append(0))]
 # Last header might have the same scalar content; use a changed identity-free field, no fake owner test.
 extras=[(n,f)for n,f in extras if n!='foreign-header']
 for name,mutate in extras:
  q=copy.deepcopy(e);mutate(q)
  try:exact(e,q)
  except AssertionError:controls.append(name)
  else:raise AssertionError(('accepted structure mutation',name))
 for text in ['{"x":0,"x":1}','{"x":NaN}','{"x":Infinity}']:
  try:loads(text)
  except AssertionError:controls.append('strict-json-'+text)
  else:raise AssertionError('non-strict JSON admitted')
 (out/'reference.json').write_bytes((json.dumps(e,indent=2)+'\n').encode('utf-8'))
 report=dict(PASS=True,scope='PURE strict expected-reference controls only, no C# profile/live-source execution',cases=14,labelsPerCase=14,guards=93,
  positive1390Cases=sum(x['data']['Header']['Positive1390Count']>0 for x in e['cases']),controls=len(controls),controlPaths=controls,profileCalls=0,sourceCalls=0,FIRST=0,NN=0)
 (out/'report.json').write_bytes((json.dumps(report,indent=2)+'\n').encode('utf-8'));print(json.dumps({k:v for k,v in report.items()if k!='controlPaths'}))
if __name__=='__main__':
 raw=Path(sys.argv[2]).read_bytes()
 if sys.argv[1]=='--selftest':selftest(raw,Path(sys.argv[3]))
 elif sys.argv[1]=='--check':
  capture=Path(sys.argv[3]);out=Path(sys.argv[4]);assert not out.exists();b=capture.read_bytes();assert len(b)<=131072;actual=loads(b.decode('utf-8'));e=expected(raw);exact(e,actual)
  out.write_bytes((json.dumps(dict(PASS=True,captureSha256=hashlib.sha256(b).hexdigest(),cases=14,guards=93,scope='AL scalar/raw dictionary + unchanged old header14 DATA only; no FIRST/NN/cache/opaque-byte proof'),indent=2)+'\n').encode('utf-8'));print('PASS exact14/93')
 else:raise AssertionError('mode')
