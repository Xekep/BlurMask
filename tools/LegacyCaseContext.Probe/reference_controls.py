"""Independent forward-dependency context records and ordered special-row selection oracle."""
import json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=4 or Path(sys.argv[3]).suffix.lower()!='.json':raise ValueError('case-controls.json normalization.json output.json')
case=json.loads(Path(sys.argv[1]).read_text(encoding='utf8'));vmap=dict(case['variants'])
ranges=json.loads(Path(sys.argv[2]).read_text(encoding='utf8'))['combiningRanges']
def ccc(cp):
 for lo,hi,v in ranges:
  if lo<=cp<=hi:return v
 return -1
rng=random.Random(0x5f8e0);pool=[0x49,0x69,0x307,0x301,0x323,0x315,0x34f,0x10ffff,0x3a3,0x130,0x430,0x416,0x1c5,0x390]
sequences=[[cp] for cp in vmap]
sequences+=[[rng.choice(pool) for _ in range(1+i%28)] for i in range(256)]
sequences+=[[0x49,0x323,0x301,0x307],[0x69,0x315,0x307],[0x10ffff,0x323,0x307],[0x323,0x323,0x301]]
out=[];marked=0
for cps in sequences:
 classes=[ccc(cp) for cp in cps];records=[];selected=[]
 for j,cl in enumerate(classes):
  above=any(classes[i]==230 and all(v!=0 for v in classes[j:i]) for i in range(j+1,len(cps)))
  dot=any(cps[i]==0x307 and all(v not in (0,230) for v in classes[j:i]) for i in range(j+1,len(cps)))
  flags=[int(above),int(dot),0,0];marked+=sum(flags)
  records.append((struct.pack('<i4B',cl,*flags)).hex())
  choices=vmap.get(cps[j],[]);indexes=[]
  for request in range(4):
   for requested in range(5):
    language=requested if requested and any(v[3]==requested for v in choices) else 0
    match=-1
    for n,v in enumerate(choices):
     condition={0:True,1:not(request&1),2:bool(request&2),4:bool(flags[3]),8:bool(flags[0]),16:bool(flags[2]),32:not flags[1]}.get(v[4],False)
     if v[3]==language and condition:match=n;break
    indexes.append(match)
  selected.append(indexes)
 out.append(dict(cps=cps,records=records,selected=selected))
Path(sys.argv[3]).write_text(json.dumps(out,separators=(',',':'))+'\n',encoding='utf8');print(f'Generated{len(out)} independent context cases/{marked} nonzero backward flags.')
