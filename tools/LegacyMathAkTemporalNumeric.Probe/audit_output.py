"""Portable synthetic numerical controls; no SDK files or actual model forward."""
from pathlib import Path
import math,json,hashlib,sys,copy
repo,capture,output=map(Path,sys.argv[1:]);H=lambda v:hashlib.sha256(v).hexdigest()
table=tuple(int(1024*math.tanh(3*i/47))for i in range(48))
exp=tuple(int(25*math.exp(-4+22*i/351))for i in range(352))
log=(-67108864,)+tuple(int(8192*math.log(i/2048))for i in range(1,2049))
def convert(raw):
 threshold=(351<<16)-4194304;offset=max(threshold,max(raw))-threshold
 assert offset<=2147483647 and min(raw)-offset>=-2147483648
 vals=[0 if v-offset+4194304<0 else exp[min(351,(v-offset+4194304)>>16)]for v in raw]
 while sum(vals)>2147483647:vals=[v>>1 for v in vals]
 total=sum(vals);maximum=max(vals)
 if maximum==0:probs=[0]*len(vals)
 else:
  shift=max(0,total.bit_length()-1-6);divisor=total>>shift;left=30-(maximum.bit_length()-1)
  probs=[min(32767,((v<<left)&4294967295)//divisor >> ((left+shift-15)&31))for v in vals]
 costs=[]
 for v in probs:
  v=max(v,1);q,rem=divmod(v,16);costs.append(-(log[q+(rem>8)]if q else log[rem]-0x58b9))
 return probs,costs
expectedNames=['signed','pair-overflow','sum-wrap','offset','signed-scale','scale-wrap','offset','scale-one']
def wrap(v):return ((v+2**31)%2**32)-2**31
def check(r):
 assert r['modelExecutions']==0 and r['vendorExecution']is False and r['intMinRefused']is True
 assert [x['name']for x in r['rows']]==expectedNames
 assert len(r['hidden'])==17 and len(r['scoreRows'])==3
 for q in r['scoreRows']:
  assert len(q['raw'])==257
  probability,costs=convert(q['raw']);assert q['probability']==probability and q['costs']==costs
 for row in r['rows']:
  x,w,at=row['input'],row['weights'],row['at'];assert len(x)>0 and at+len(x)<=len(w)
  s=0
  #Independent pairwise PMADDWD reduction followed by modular scaling.
  for i in range(0,len(x),2):s=wrap(s+wrap(sum(x[j]*w[at+j]for j in range(i,min(i+2,len(x))))))
  assert row['result']==wrap(s*row['scale'])
 for q in r['hidden']:
  v=q['value'];assert v!=-2**31
  at=abs(v)>>16;m=table[at]if at<48 else 1018
  assert q['result']==(-m if v<=0 else m)
r=json.loads(capture.read_text());check(r);neg=[]
for name,mut in [('dot',lambda z:z['rows'][0].__setitem__('result',0)),('scale',lambda z:z['rows'][4].__setitem__('scale',1)),('hidden',lambda z:z['hidden'][3].__setitem__('result',0)),('int-min',lambda z:z.__setitem__('intMinRefused',False)),('neural-claim',lambda z:z.__setitem__('modelExecutions',1)),('probability',lambda z:z['scoreRows'][0]['probability'].__setitem__(0,0)),('cost',lambda z:z['scoreRows'][0]['costs'].__setitem__(0,0))]:
 q=copy.deepcopy(r);mut(q)
 try:check(q)
 except AssertionError:neg.append(name)
 else:raise AssertionError(name)
report=dict(captureSHA256=H(capture.read_bytes()),syntheticDots=8,hiddenControls=17,nativeTableEntries=0,mathematicalHiddenEntries=48,expEntries=352,logEntries=2049,syntheticScoreRows=3,syntheticCosts=771,modularDotScopes=2,corruptions=neg,modelExecutions=0,vendorExecution=False,scope='Portable mathematical reference only; separate static PE audit establishes native table identity; no real weights evaluated')
output.write_text(json.dumps(report,indent=2)+'\n');print('PASS 8 synthetic dots /17 hidden /48 mathematical entries /7 corruptions /3x257 costs /0NN')
