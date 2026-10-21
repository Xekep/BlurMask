"""STATIC raw actual EN sparse query suffix/backoff reference; no DLL loads.
Compares packed binary-search traversal to independent tuple-table semantics.
These are numerical controls, not handwriting requests or expected word labels.
"""
from pathlib import Path
import runpy,sys,struct,bisect,hashlib,json,collections,argparse
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
root=a.repo;out=a.output.parent;out.mkdir(parents=True,exist_ok=True)
sys.argv=['reference_actual',str(a.models),str(out/'raw-model-reference.json')]
g=runpy.run_path(str(root/'tools/LegacyEnglishLkSparse101.Probe/reference_actual.py'),run_name='__main__')
arrays=g['arrays'];raws=g['raws'];classes=g['classCompiled'];backoff=g['backoffCompiled'];word=g['wordCompiled']
offsets=[arrays['B8.0'],arrays['B8.1']];labels=[arrays['B0.1'],arrays['B0.2']]
cost={};backs={};prefixes=[]
for h in range(1024):
 cost[(h,)]=classes[0][raws['C0.0'][h]];backs[(h,)]=backoff[0][raws['C8.0'][h]]
 for j in range(offsets[0][h],offsets[0][h+1]):
  p=(h,labels[0][j]);prefixes.append((p,j));cost[p]=classes[1][raws['C0.1'][j]];backs[p]=backoff[1][raws['C8.1'][j]]
  for k in range(offsets[1][j],offsets[1][j+1]):cost[p+(labels[1][k],)]=classes[2][raws['C0.2'][k]]
assert len(cost)==1024+113643+155484 and len(prefixes)==113643
def packed_search(history,current):
 seq=tuple(history[-2:])+(current,);penalty=0
 for start in range(len(seq)):
  suffix=seq[start:];index=suffix[0];matched=0
  for level,nextclass in enumerate(suffix[1:]):
   lo,hi=offsets[level][index:index+2];pos=bisect.bisect_left(labels[level],nextclass,lo,hi)
   if pos==hi or labels[level][pos]!=nextclass:break
   index=pos;matched+=1
  needed=len(suffix)-1
  if matched==needed:return penalty+classes[needed][raws['C0.'+str(needed)][index]],needed+1
  if matched==needed-1:penalty+=backoff[matched][raws['C8.'+str(matched)][index]]
 raise AssertionError('Unigram table must be total')
def tuple_suffix(history,current):
 seq=tuple(history[-2:])+(current,);penalty=0
 for i in range(len(seq)):
  suffix=seq[i:]
  if suffix in cost:return penalty+cost[suffix],len(suffix)
  prefix=suffix[:-1]
  if prefix in backs:penalty+=backs[prefix]
 raise AssertionError('Total unary map')
digest=hashlib.sha256();counts=collections.Counter();examples={};checks=0
def check(kind,history,current,wordindex=None):
 global checks
 left=packed_search(history,current);right=tuple_suffix(history,current);assert left==right,(kind,history,current,left,right)
 value,matched=left;newhistory=(tuple(history)+(current,))[-2:]
 if wordindex is not None:value+=word[raws['98.wordCodes'][wordindex]]
 row=dict(kind=kind,history=list(history),currentClass=current,wordIndex=wordindex,classAndWordCost=value,matchedOrder=matched,retainedHistory=list(newhistory),retainedCount=len(newhistory))
 counts[kind]+=1;counts['matchedOrder'+str(matched)]+=1;checks+=1
 digest.update(struct.pack('<8i',len(history),*(tuple(history)+(0,0))[:2],current,-1 if wordindex is None else wordindex,value,matched,len(newhistory)))
 examples.setdefault(kind+'/'+str(matched),row)
 # Native record+4=matched order versus record+8=retained count are separate.
 if matched<len(history)+1:examples.setdefault('backoffRecordCountDistinct',row)
def first_missing(start,end,level):
 expected=0
 for value in labels[level][start:end]:
  if value!=expected:return expected
  expected+=1
 assert expected<1024;return expected
# FIRST byte0=1 inserts the real reserved word's class0, not empty history.
assert arrays['88'][260074]==0
for wordindex,current in enumerate(arrays['88'][:260074]):check('firstWordClass0', (0,),current,wordindex)
for p,j in prefixes:
 check('actualBigram',p[:-1],p[-1]);lo,hi=offsets[1][j:j+2]
 for k in range(lo,hi):check('actualTrigram',p,labels[1][k])
 check('missingTrigram',p,first_missing(lo,hi,1))
for h in range(1024):check('missingBigram',(h,),first_missing(offsets[0][h],offsets[0][h+1],0))
assert checks==643868 and 'backoffRecordCountDistinct'in examples
result=dict(status='PASS independent actual packed-search/tuple suffix semantics',checks=checks,counts=dict(counts),numericDigest=digest.hexdigest(),examples=list(examples.values()),initialClass=0,reservedInitialWord=260074,record4='matched order',record8='retained class/tag history count',context00=0x4000000,requestOrigin='Prepared raw numerical controls only; no actual NN selection or source request fabricated',targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(result,indent=2)+'\n');print('PASS',checks,'actual sparse numerical controls',digest.hexdigest())
