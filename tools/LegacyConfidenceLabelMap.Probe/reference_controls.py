"""Independent native3A0 insertion order and one-step output-index reference."""
import json,pathlib,random,sys
texts=['proto','x',"'",'"','a','b','o','0','-','_','\u00ad','\uff0d','\uff3f','\u30fc','\u4e00','\U000f0002','\U000f0003','\U000f0001','tail','z','c','d','e','f']
r=random.Random(0x3a0);cases=[];n=len(texts)
for iteration in range(512):
 width=r.randrange(2,n+1);dash=0 if iteration%3 else 65535;closed=1 if iteration%2 else 65535;groups=[];mapping={}
 for g in range(iteration%7):
  labels=[r.randrange(n) for _ in range(r.randrange(0,10))];weight=[-1,0,1,2][r.randrange(4)];groups.append(dict(labels=labels,weight=weight))
  inside=[v for v in labels if v<width];outside=[v for v in labels if v>=width]
  if weight>0 and inside and outside:
   for v in outside:
    if v not in mapping:mapping[v]=inside[0]
 if dash!=65535:
  for t in ['-','\uff0d','_','\uff3f','\u00ad','\u30fc','\u4e00','\U000f0002','\U000f0003']:
   label=texts.index(t)
   if label not in mapping and not(t=='\u30fc' and label<width):mapping[label]=dash
 for label,target in [(n+1,texts.index("'")),(n+2,texts.index('"'))]:
  if label not in mapping:mapping[label]=target
 outputs=[]
 for label in range(n+3):
  output=mapping.get(label,closed if label==texts.index('\U000f0001') else label)
  outputs.append([output<width,output])
 cases.append(dict(width=width,dash=dash,closed=closed,groups=groups,map=sorted([k,v] for k,v in mapping.items()),outputs=outputs))
target=pathlib.Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(dict(texts=texts,cases=cases),separators=(',',':'))+'\n');print('512 independent first-wins/order/bounds remap controls')
