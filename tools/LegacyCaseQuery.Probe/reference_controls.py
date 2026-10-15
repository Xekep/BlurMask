"""Independent finite-state emission oracle. Numeric policies are pinned native facts;
functional expectations use a declarative variant plan, not managed output."""
import base64,json,random,struct,sys
from pathlib import Path
POLICIES = {0: {'rows': [[0, 0, 0, 1, 2, 3, 5, 6, 7, 4, 255, 11, 12, 13, 255], [0, 0, 1, 1, 1, 1, 255, 255, 255, 255, 255, 255, 255, 255, 255], [0, 0, 2, 11, 2, 3, 8, 9, 10, 4, 255, 255, 255, 255, 255], [0, 0, 3, 12, 3, 3, 9, 9, 9, 255, 255, 255, 255, 255, 255], [0, 0, 4, 13, 4, 255, 10, 255, 10, 4, 255, 255, 255, 255, 255], [0, 0, 5, 11, 5, 6, 8, 9, 10, 7, 255, 255, 255, 255, 255], [0, 0, 6, 12, 6, 6, 9, 9, 255, 255, 255, 255, 255, 255, 255], [0, 0, 7, 13, 7, 255, 10, 255, 10, 7, 255, 255, 255, 255, 255], [0, 0, 8, 255, 8, 9, 8, 9, 10, 10, 255, 255, 255, 255, 255], [0, 0, 9, 255, 9, 9, 9, 9, 255, 255, 255, 255, 255, 255, 255], [0, 0, 10, 255, 10, 255, 10, 255, 10, 10, 255, 255, 255, 255, 255], [0, 0, 11, 11, 11, 12, 255, 255, 255, 13, 255, 255, 255, 255, 255], [0, 0, 12, 12, 12, 12, 255, 255, 255, 255, 255, 255, 255, 255, 255], [0, 0, 13, 13, 13, 255, 255, 255, 255, 13, 255, 255, 255, 255, 255]], 'alternate': [True, False, False, False, False, False, False, False, False, False, False, False, False, False]}, 512: {'rows': [[0, 0, 0, 2, 3, 4, 8, 9, 10, 6, 1, 24, 25, 26, 27], [0, 0, 1, 11, 255, 255, 255, 255, 255, 255, 1, 255, 255, 255, 255], [0, 0, 2, 2, 12, 12, 255, 255, 255, 12, 11, 255, 255, 255, 255], [0, 0, 3, 21, 14, 4, 15, 16, 17, 6, 13, 255, 255, 255, 255], [0, 0, 4, 22, 5, 5, 16, 16, 255, 255, 13, 255, 255, 255, 255], [0, 0, 5, 22, 5, 5, 16, 16, 255, 255, 255, 255, 255, 255, 255], [0, 0, 6, 23, 7, 255, 17, 255, 17, 7, 13, 255, 255, 255, 255], [0, 0, 7, 23, 7, 255, 17, 255, 17, 7, 255, 255, 255, 255, 255], [0, 8, 8, 24, 8, 9, 18, 19, 20, 10, 255, 255, 255, 255, 255], [0, 9, 9, 25, 9, 9, 19, 19, 255, 255, 255, 255, 255, 255, 255], [0, 10, 10, 26, 10, 255, 20, 255, 20, 10, 255, 255, 255, 255, 255], [0, 11, 11, 11, 255, 255, 255, 255, 255, 255, 11, 255, 255, 255, 255], [0, 0, 12, 12, 12, 12, 255, 255, 255, 12, 255, 255, 255, 255, 255], [0, 13, 13, 13, 255, 255, 255, 255, 255, 255, 13, 255, 255, 255, 255], [0, 0, 14, 21, 14, 5, 15, 16, 17, 7, 255, 255, 255, 255, 255], [0, 15, 15, 255, 15, 16, 15, 16, 17, 17, 255, 255, 255, 255, 255], [0, 16, 16, 255, 16, 16, 16, 16, 255, 255, 255, 255, 255, 255, 255], [0, 17, 17, 255, 17, 255, 17, 255, 17, 17, 255, 255, 255, 255, 255], [0, 18, 18, 255, 18, 19, 18, 19, 20, 20, 255, 255, 255, 255, 255], [0, 19, 19, 255, 19, 19, 19, 19, 255, 255, 255, 255, 255, 255, 255], [0, 20, 20, 255, 20, 255, 20, 255, 20, 20, 255, 255, 255, 255, 255], [0, 0, 21, 21, 21, 22, 255, 255, 255, 23, 13, 255, 255, 255, 255], [0, 0, 22, 22, 22, 22, 255, 255, 255, 255, 13, 255, 255, 255, 255], [0, 0, 23, 23, 23, 255, 255, 255, 255, 23, 13, 255, 255, 255, 255], [0, 24, 24, 24, 24, 25, 255, 255, 255, 26, 255, 255, 255, 255, 255], [0, 25, 25, 25, 25, 25, 255, 255, 255, 255, 255, 255, 255, 255, 255], [0, 26, 26, 26, 26, 255, 255, 255, 255, 26, 255, 255, 255, 255, 255], [0, 255, 255, 255, 255, 255, 8, 255, 255, 255, 255, 255, 255, 255, 255]], 'alternate': [True, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False, False]}}
if len(sys.argv)!=3 or Path(sys.argv[2]).suffix.lower()!='.json':raise ValueError('case-preparation-controls.json output.json')
source=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'))
chosen=[source[0]]
for candidate in source[2:]:
 if any(r[1] in (2,4) or r[2] in (2,4) for r in candidate['rows']):
  chosen.append(candidate)
  if len(chosen)==4:break
rng=random.Random(0x233090);out=[]
for a in chosen:
 by={r[0]:r for r in a['rows']}; selected=[]
 for mode in (1,2):
  for category in range(10):
   matches=[r[0] for r in a['rows'] if r[mode]==category]
   selected.extend(matches[:2])
 selected=sorted(set(selected));tests=[]
 for version,table in POLICIES.items():
  for state,t in enumerate(table['rows']):
   alt=table['alternate'][state]
   for label in selected:
    meta=by[label];kind=meta[2 if alt else 1];forms=meta[4 if alt else 3]
    for flags in (0,1,2,3):
     #Commands(state-column,form-ordinal,costdelta). Native always preserves
     #record order and mutates original scalar, not prior generated output.
     plan=[]
     def put(col,form=-1,delta=0):
      if t[col]!=255:plan.append((t[col]&31,form,delta))
     if kind==1:
      put(3);put(6,1,1)
      if t[6]!=255 and flags==3:plan.append((t[11]&31,1,1))
     elif kind==2:
      put(3);put(7,1,1)
      if t[7]!=255 and flags==3:plan.append((t[12]&31,1,1))
      put(8,2,1)
      if t[8]!=255 and flags==3:plan.append((t[13]&31,2,1))
     elif kind==3:put(4);put(10,0,8192 if state==0 else 0)
     elif kind==4:
      put(5);put(9,2,1);put(10,0,(1 if t[9]!=255 else 0)+(8192 if state==0 else 0))
     elif kind==5:plan=[(t[0]&31,-1,0)]
     elif kind==6:plan=[(t[1]&31,-1,0)]
     elif kind==7:plan=[(state,-1,0)]
     else:plan=[(t[2]&31,-1,0)]
     raw=bytearray(rng.randbytes(32));struct.pack_into('<H',raw,20,label);raw[5]=flags;raw[7]=rng.randrange(256);cost=rng.randrange(-100000,100000);struct.pack_into('<i',raw,16,cost)
     wanted=[];unsupported=forms is None and any(form>=0 for _,form,_ in plan)
     if not unsupported:
      for nextstate,form,delta in plan:
       v=raw.copy();v[7]=(v[7]&3)|(nextstate<<2);struct.pack_into('<i',v,16,cost+delta)
       if form>=0:struct.pack_into('<H',v,20,forms[form]);struct.pack_into('<Q',v,8,0)
       wanted.append([v.hex(),delta])
     tests.append(dict(version=version,state=state,input=raw.hex(),expected=None if unsupported else wanted,count=len(plan)))
 out.append(dict(alphabet=a['alphabet'],tests=tests))
Path(sys.argv[2]).parent.mkdir(parents=True,exist_ok=True)
Path(sys.argv[2]).write_text(json.dumps(out,separators=(',',':'))+'\n',encoding='utf-8')
print('Generated',len(out),'alphabets/',sum(len(v['tests']) for v in out),'state/category/flags/cost controls')
