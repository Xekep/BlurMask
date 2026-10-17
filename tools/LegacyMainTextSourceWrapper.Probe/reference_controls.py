"""Prepared source-key numeric cases; no manufactured decoder/WORD ownership."""
import importlib.util,json,random,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('ranges',root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py')
ops=importlib.util.module_from_spec(spec);spec.loader.exec_module(ops)
def same(a,b):return a>>16==b>>16 and(a>>8&255==b>>8&255 or a>>8&255==255 or b>>8&255==255)
def produce(path):
 rng=random.Random(0x24f30);cases=[]
 for index in range(2048):
  counts=[rng.randrange(2,20)for _ in range(rng.randrange(1,5))]
  original=[[3,((len(counts)-1)<<32)|((counts[-1]-1)<<16)|0xff05]]
  def coord():
   k=rng.randrange(len(counts));p=rng.randrange(counts[k]);f=255 if p==counts[k]-1 else rng.choice([0,10,100,199])
   return(k<<32)|(p<<16)|(f<<8)
  def vector():
   pairs=[]
   for _ in range(rng.randrange(1,4)):
    a,b=sorted([coord(),coord()]);pairs.append([a|3,b|5])
   return ops.union([],pairs)
  ranges=[vector()for _ in range(rng.randrange(2,8))];n=len(ranges)
  skip=[rng.randrange(8)==0 for _ in range(n)];positive=[rng.randrange(8)!=0 for _ in range(n)]
  joiner=[False]*n;joiner[0]=rng.randrange(12)==0;skip[0]=False;positive[0]=True
  #Deterministic coverage for an outside degenerate, endpoint normalization,
  #full overlap ownership and native skip/negative later-source exclusions.
  if index%8==0:ranges[0]=[[((len(counts)+1)<<32)|3,((len(counts)+1)<<32)|5]]
  elif index%8==1:ranges[1]=[r[:]for r in ranges[0]];skip[1]=False;positive[1]=True;joiner[0]=False
  elif index%8==2:joiner[0]=True;ranges[0]=[[3,((counts[0]-1)<<16)|0xff05]]
  before=[[r[:]for r in v]for v in ranges];out=[]
  for i in range(n):
   if skip[i]:continue
   p=positive[i];key=ranges[i]
   if p:
    if len(key)==1 and same(*key[0])and not any(ops.n(a)<=ops.n(key[0][0])<=ops.n(b)and key[0][1]<=b for a,b in original):
     p=False;key=[[(key[0][0]&~255)|4,(key[0][1]&~255)|4]]
    elif joiner[i]:
     end=key[0][1]
     if end>>8&255==255:end&=~0xff00
     end=(end&~255)|4;key=[[end,end]];p=False
    else:
     for later in range(i+1,n):
      if skip[later]or not positive[later]:continue
      intersection=ops.intersect(ops.union([],ranges[later]),key)
      if not intersection:continue
      if key!=intersection:key=ops.subtract(key,intersection,counts)
      else:ranges[later]=ops.subtract(ranges[later],intersection,counts)
      break
   ranges[i]=key;out.append(dict(index=i,positive=p,key=key))
  cases.append(dict(counts=counts,original=original,ranges=before,skip=skip,positive=positive,joiner=joiner,expected=out,after=ranges))
 Path(path).write_bytes((json.dumps(dict(cases=cases),separators=(',',':'))+'\n').encode())
 print('PASS2048 independent prepared source-lifecycle controls; no actual glyph/WORD claim.')
if __name__=='__main__':produce(sys.argv[1])
