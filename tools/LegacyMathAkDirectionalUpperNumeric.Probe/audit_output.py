from pathlib import Path
import json,sys,copy,math,hashlib
capture,output=map(Path,sys.argv[1:]);H=lambda d:hashlib.sha256(d).hexdigest()
def wrap(x):return (x+2**31)%2**32-2**31
table=tuple(int(1024*math.tanh(3*i/47)) for i in range(48))
exp=tuple(int(25*math.exp(-4+22*i/351)) for i in range(352))
log=(-67108864,)+tuple(int(8192*math.log(i/2048)) for i in range(1,2049))
def convert(raw):
 threshold=(351<<16)-4194304;offset=max(threshold,max(raw))-threshold
 vals=[0 if v-offset+4194304<0 else exp[min(351,(v-offset+4194304)>>16)] for v in raw]
 while sum(vals)>2147483647:vals=[v>>1 for v in vals]
 total=sum(vals);maximum=max(vals)
 if maximum==0:probs=[0]*len(vals)
 else:
  shift=max(0,total.bit_length()-1-6);divisor=total>>shift;left=30-(maximum.bit_length()-1)
  probs=[min(32767,((v<<left)&4294967295)//divisor >> ((left+shift-15)&31)) for v in vals]
 costs=[]
 for v in probs:
  v=max(v,1);q,rem=divmod(v,16);costs.append(-(log[q+(rem>8)] if q else log[rem]-0x58b9))
 return probs,costs
GUARDS=['equal-data-foreign-features-before-forward','equal-data-foreign-profile-bind-refused','pre-cancel-exact-token-before-forward','same-shared-model-owner','exact5F-matrix-scope','prepared-hidden-dots-detached','prepared-hidden-values-detached','prepared-raw-detached','prepared-probability-detached','prepared-cost-detached','prepared-result-scope']
def check(r):
 assert r['scope']=='synthetic223 matrix kernel; actual ownership guards only'
 assert type(r['modelExecutions'])is int and r['modelExecutions']==0 and r['vendorExecution'] is False and r['guards']==GUARDS
 for key in ['input','hiddenDots','hidden','raw','probability','costs']:assert type(r[key])is list and all(type(v)is int for v in r[key])
 x=[i*127-12000 for i in range(223)];assert r['input']==x
 dots=[wrap(sum(x[c]*(((row+c)%11-5)*1000) for c in range(223))) for row in range(200)]
 hidden=[1024]
 for d in dots:
  assert d!=-2**31;index=abs(d)>>16;magnitude=table[index] if index<48 else 1018;hidden.append(-magnitude if d<=0 else magnitude)
 raw=[wrap(wrap(sum(hidden[c]*((row*3+c)%7-3) for c in range(201)))*(17+row%23)) for row in range(257)]
 p,c=convert(raw)
 assert r['hiddenDots']==dots and r['hidden']==hidden and r['raw']==raw and r['probability']==p and r['costs']==c
r=json.loads(capture.read_text());check(r);neg=[]
for field in ['input','hiddenDots','hidden','raw','probability','costs']:
 bad=copy.deepcopy(r);bad[field][0]^=1
 try:check(bad)
 except AssertionError:neg.append(field)
 else:raise AssertionError(field)
report=dict(captureSHA256=H(capture.read_bytes()),syntheticInputWidth=223,hiddenDots=200,hiddenValues=201,outputs=257,probabilities=257,costs=257,guards=11,tamperControls=neg,modelExecutions=0)
output.write_text(json.dumps(report,indent=2)+'\n');print('PASS 200 synthetic dots /201 hidden /257 outputs-probabilities-costs /11 guards /6 corruptions /0NN')
