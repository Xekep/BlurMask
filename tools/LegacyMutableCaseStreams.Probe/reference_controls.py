"""Independent case row/context selection and segment-sort canonical oracle."""
import json,random,sys
from pathlib import Path
if len(sys.argv)!=4:raise ValueError('scalar-case-controls normalization-data output')
variants=dict(json.loads(Path(sys.argv[1]).read_text(encoding='utf8'))['variants']);data=json.loads(Path(sys.argv[2]).read_text(encoding='utf8'))
def ccc(cp):return next((v for lo,hi,v in data['combiningRanges'] if lo<=cp<=hi),-1)
parts={row[0]:row[2:2+row[1]] for row in data['canonicalDecompositions']};parts.pop(0x37e,None);parts.pop(0x387,None)
compositions={(a,b):v for a,b,v in data['compositions']}
def decompose(cp):
 if 0xac00<=cp<=0xd7a3:
  t=cp-0xac00;return [0x1100+t//588,0x1161+(t%588)//28]+([0x11a7+t%28] if t%28 else [])
 return sum((decompose(v) for v in parts[cp]),[]) if cp in parts else [cp]
def nfd(cps):
 expanded=sum((decompose(cp) for cp in cps),[]);output=[];segment=[]
 for cp in expanded:
  if ccc(cp)==0:output+=sorted(segment,key=ccc);segment=[]
  segment.append(cp)
 return output+sorted(segment,key=ccc)
def pair(a,b):
 if 0x1100<=a<0x1113 and 0x1161<=b<0x1176:return 0xac00+((a-0x1100)*21+b-0x1161)*28
 if 0xac00<=a<=0xd7a3 and (a-0xac00)%28==0 and 0x11a8<=b<0x11c3:return a+b-0x11a7
 return compositions.get((a,b))
def nfc(cps):
 out=[];starter=None;last=-1
 for cp in nfd(cps):
  cl=ccc(cp);joined=pair(out[starter],cp) if starter is not None and cl>last else None
  if joined is not None:out[starter]=joined
  else:
   out.append(cp)
   if cl==0:starter=len(out)-1;last=-1
   else:last=cl
 return out
rng=random.Random(0x4f170);sequences=[[cp] for cp in variants]
pool=[0x49,0x69,0x307,0x301,0x323,0x315,0x3a3,0x130,0x430,0x416,0x451,0x439,0x1c5,0x390,0x5b0]
sequences+=[[rng.choice(pool) for _ in range(1+i%16)] for i in range(128)]
sequences+=[[0x61,0x5b0,0x301],[0x1c6],[0x37e],[0x387],[0x435,0x308],[0x438,0x306]]
out=[]
for cps in sequences:
 classes=[ccc(cp) for cp in cps]
 for flags,language in [(0,0),(1,0),(2,0),(3,0),(0,1),(0,2),(0,3),(0,4)]:
  lo=[];up=[]
  for at,cp in enumerate(cps):
   choices=variants.get(cp,[]);lang=language if language and any(v[3]==language for v in choices) else 0
   above=any(classes[j]==230 and all(v!=0 for v in classes[at:j]) for j in range(at+1,len(cps)))
   dot=any(cps[j]==0x307 and all(v not in(0,230) for v in classes[at:j]) for j in range(at+1,len(cps)))
   selected=next((v for v in choices if v[3]==lang and {0:True,1:not(flags&1),2:bool(flags&2),4:False,8:above,16:False,32:not dot}.get(v[4],False)),None)
   lo+=selected[0] if selected else [cp];up+=selected[1] if selected else [cp]
  out.append(dict(input=cps,flags=flags,language=language,lower=nfc(lo),upper=nfc(up),stripped=nfc([cp for cp in nfd(up) if not 200<=ccc(cp)<=234])))
p=Path(sys.argv[3]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(out,separators=(',',':'))+'\n',encoding='utf8');print(f'Generated {len(out)} independent scalar case streams, ordered suffix-context conditions and canonical segment sorts.')
