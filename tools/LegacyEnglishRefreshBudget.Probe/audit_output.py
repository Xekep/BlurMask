"""Pure count/cache-budget oracle; no actual failure74 counter reconstruction claim."""
import base64,copy,json,struct,sys
from pathlib import Path
def charge(m,r):
 n=m+r
 return dict(Payload=8192+4096+182*256+128*16+m*512+n*64,Copies=16384+8192+182*512+128*32+m*1024+n*128,Visits=4096+m*8192+32*n*n)
def verify(c):
 assert c['ProgressSize']==72 and c['ProgressReferenceFree']is True
 assert c['Scope']=='PREPARED_REFRESH_COUNTS_CACHE_REUSE_AND_SYNTHETIC_LEDGER_PREFIX_NOT_ACTUAL_74_REPLAY'
 assert c['NN']is False and c['ActualOwnerExecuted']is False and c['Actual74UsedPrefixAvailable']is False
 assert type(c['Controls'])is int and c['Controls']==256 and len(c['Cases'])==256
 def exact(a,b):assert type(a)is type(b)and a==b,(a,b)
 for traits,x in enumerate(c['Cases']):
  matched=(traits&32)!=0 and(traits&7)==0;expected=3 if matched else 0
  for k,v in [('Traits',traits),('Predicted',expected),('Actual',expected),('Callbacks',expected),('WarmRefreshCharge',0)]:exact(x[k],v)
  assert x['ColdCharge']==charge(expected,1)
  def record(d,symbol=None,end=False):
   b=bytearray(40);struct.pack_into('<i',b,0,d);struct.pack_into('<I',b,6,0xffff0000);struct.pack_into('<f',b,12,1);struct.pack_into('<f',b,16,-1);b[32:40]=bytes([255]*8)
   if symbol is not None:
    struct.pack_into('<H',b,4,8 if end else 0);struct.pack_into('<H',b,8,symbol);struct.pack_into('<f',b,12,0);struct.pack_into('<f',b,16,.5);struct.pack_into('<i',b,20,123)
   return b
  rows=record(99)
  if matched:
   for i in range(3):rows+=record(i,10+i,((traits^(64 if i==1 else 0))&64)!=0)
  assert base64.b64decode(x['FullOwner'])==rows
 assert len(c['EntryGuards'])==32
 for bits,x in enumerate(c['EntryGuards']):
  exact(x['Bits'],bits);assert x['Allowed']is(bool(bits&1)and(not(bits&2)or bool(bits&4))and bool(bits&8)and bool(bits&16))
 assert len(c['Charges'])==24
 for x,(m,r)in zip(c['Charges'],[(m,r)for m in [0,1,2,10,128,4096]for r in [0,1,10,2040]],strict=True):
  exact(x['Matched'],m);exact(x['Rows'],r);assert x['Charge']==charge(m,r)
 s=c['SyntheticPrefix'];assert s==dict(Before=9*1024*1024-200000,OldRequested=1048576+8192,OldRefused=True,NewRequested=charge(2,1)['Payload'],After=9*1024*1024-200000+charge(2,1)['Payload'])
 assert c['Refusals']==['row39','negative-source','overmatched','foreign-cache']
c=json.loads(Path(sys.argv[1]).read_bytes());verify(c);bad=[]
for name,fn in [('warm-charge',lambda q:q['Cases'][0].__setitem__('WarmRefreshCharge',1)),('callback',lambda q:q['Cases'][32].__setitem__('Callbacks',0)),('cold-charge',lambda q:q['Cases'][0]['ColdCharge'].__setitem__('Payload',0)),('reuse-entry',lambda q:q['EntryGuards'][27].__setitem__('Allowed',True)),('missing-case',lambda q:q['Cases'].pop()),('bool-count',lambda q:q.__setitem__('Controls',True)),('actual-prefix-claim',lambda q:q.__setitem__('Actual74UsedPrefixAvailable',True)),('prefix-refund',lambda q:q['SyntheticPrefix'].__setitem__('After',0))]:
 q=copy.deepcopy(c);fn(q)
 try:verify(q)
 except AssertionError:bad.append(name)
 else:raise AssertionError('mutation accepted '+name)
report=dict(Passed=True,Scope='PREPARED_NUMERIC_ALLOCATION_NOT_OWNING_PIPELINE',PredicatesAndFullPoolCases=256,EntryGuards=32,ChargeCases=24,TamperRefused=bad,Actual74UsedPrefix='UNAVAILABLE_IN_FROZEN_FAILURE_CAPTURE',NN=False)
Path(sys.argv[2]).write_bytes((json.dumps(report,indent=2)+'\n').encode());print(json.dumps(report))
