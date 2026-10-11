"""Independent scalar stage interpreters composed in recovered191950 RU106 order.
Imports only definitions from frozen control oracles; never invokes their random generators.
"""
import ast,json,math,random,struct,sys
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
def oracle(name):
 text=(ROOT/'tools'/name/'reference_controls.py').read_text();tree=ast.parse(text)
 tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.ClassDef,ast.FunctionDef)) and not isinstance(n,ast.FunctionDef) or isinstance(n,ast.FunctionDef) and n.name!='make']
 space={};exec(compile(tree,name,'exec'),space);return text,space
def adapted(name,func,initial,start,finish,changes=()):
 text,space=oracle(name);body=text[text.index(start):text.index(finish,text.index(start))]
 for a,b in changes:body=body.replace(a,b)
 exec('def '+func+':\n'+initial+body+'\n',space);return space[func.split('(')[0]]
ht,hs=oracle('LegacyCursiveHorizontal.Probe');Stage=hs['Stage'];f=hs['f'];add=hs['add'];sub=hs['sub'];mul=hs['mul'];bits=hs['bits']
loops=adapted('LegacyCursiveLoops.Probe','loops(state,height)','    rows=state.rows;counts=state.counts;stroke=state.stroke;start=state.start;spacing=mul(height,f(.1));i=start\n','    def insert(at,p,tag):','    return dict(')
baseline_stage=adapted('LegacyCursiveBaseline.Probe','baseline_stage(state,height,baseline)','    rows=state.rows;counts=state.counts;stroke=state.stroke;start=state.start;radius=mul(height,f(.35));i=start;pending=False;pending_at=0;anchor=start\n','    while i<start+counts[stroke]:','    return dict(')
override=adapted('LegacyCursiveOverride.Probe','override(state,strokes,height,baseline)',' rows=state.rows;counts=state.counts;stroke=state.stroke;target=strokes[stroke];starts=[0]\n for n in counts:starts.append(starts[-1]+n)\n first=0;last=len(strokes)-1;crosses=[];farthest=0.;best=[0,0]\n',' if baseline is not None and height<',' return dict(')
pre=adapted('LegacyCursiveFilters.Probe','pre(flat,tags,start,end)',' anchor=None\n',' for i in range(start,end-1):',' before=tags.copy();')
post=adapted('LegacyCursiveFilters.Probe','post(flat,tags,start,end,height,baseline)',' anchor=start\n',' for i in range(start+1,end):',' return dict(')
limit=adapted('LegacyCursiveLimit.Probe','limit(strokes,counts,tags,lengths,height,cap)','    start=0\n','    for stroke in strokes:','    a=f(rng.uniform(', [('for stroke in strokes:','for physical,stroke in enumerate(strokes):'),('end=start+len(stroke)','end=start+counts[physical]')])
def filter_stage(state,callback,*args):
 tags=[r[5] for r in state.rows];callback(state.rows,tags,state.start,state.end,*args)
 for r,t in zip(state.rows,tags):r[5]=t
def prune(rows,height):
 tags=[r[5] for r in rows];radius=mul(height,.5)
 for i in range(len(rows)):
  if tags[i]!=1:continue
  if i>0:
   if tags[i-1]!=1:tags[i-1]=0
   distance=0.
   for j in range(i-2,-1,-1):
    distance=add(distance,rows[j][4])
    if distance>=radius or tags[j]==1:break
    tags[j]=0
  distance=0.
  for j in range(i+1,len(rows)):
   distance=add(distance,rows[j-1][4])
   if distance>=radius or tags[j]==1:break
   tags[j]=0
 if tags[-1]!=1:tags[-1]=0
 distance=0.
 for j in range(len(rows)-2,-1,-1):
  distance=add(distance,rows[j][4])
  if distance>=radius or tags[j]==1:break
  tags[j]=0
 return tags
def make(rng):
 strokes=[]
 for _ in range(rng.randint(1,4)):
  points=[[f(rng.randint(-4,4)),f(rng.randint(-4,4))]]
  for i in range(rng.randint(3,18)):points.append([add(points[-1][0],rng.choice([1,2,3,-2])),add(points[-1][1],rng.choice([0,.25,-.25,1,-1,3,-3]))])
  strokes.append(points)
 tags=[1 if i==0 else 0 for s in strokes for i in range(len(s))];state=Stage(strokes,tags,0);height=f(rng.choice([1,2,4,8]));baseline=rng.choice([None,-2.,0.,2.,8.]);confidence=[rng.choice([0.,.25,.5,1.]) for _ in strokes];whole=bool(rng.randrange(5)==0);cap=rng.choice([None,2,4,6]);previous=f(-3.4028234663852886e38)
 for stroke,raw in enumerate(strokes):
  state.stroke=stroke;state.start=sum(state.counts[:stroke]);maxx=max(p[0] for p in raw)
  if not whole:
   normal=baseline is None
   if not normal and confidence[stroke]<.5:
    normal=sub(previous,mul(height,f(.3)))<maxx
    if not normal and add(height,height)<sub(maxx,min(p[0] for p in raw)) and height<sub(max(p[1] for p in raw),min(p[1] for p in raw)):
     direction=0;reversals=0
     for i in range(state.start,state.end-1):
      r=state.rows[i]
      if r[3]<0:
       if direction==-1:reversals+=1
       direction=1
      elif r[3]>0:
       if direction==1 and i>0 and state.rows[i-1][2]>0:reversals+=1
       direction=-1
     normal=reversals>3
   if normal:
    direction=0
    for i in range(state.start,state.end-1):
     r=state.rows[i]
     if r[3]<0:
      if direction==-1:r[5]=7
      direction=1
     elif r[3]>0:
      if direction==1 and i>0 and state.rows[i-1][2]>0:r[5]=8
      direction=-1
    loops(state,height);state.run(height,False,False);filter_stage(state,pre);baseline_stage(state,height,baseline);filter_stage(state,post,height,baseline)
   override(state,strokes,height,baseline)
  previous=max(previous,maxx)
 tags=prune(state.rows,height);limit(strokes,state.counts,tags,[r[4] for r in state.rows],height,cap)
 for r,t in zip(state.rows,tags):r[5]=t
 markers=[];offset=0
 for stroke,n in enumerate(state.counts):
  for i in range(n):
   t=tags[offset+i]
   if t and not 11<=t<=14:markers.append([t,stroke,i])
  offset+=n
 return dict(strokes=strokes,height=height,baseline=baseline,confidence=confidence,whole=whole,cap=cap,counts=state.counts,markers=markers,expected=[[bits(v) for v in r[:5]]+r[5:] for r in state.rows])
rng=random.Random(0x297191950)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([make(rng) for _ in range(512)],stream,separators=(',',':'))
print('Wrote512 independent whole-buffer Russian106 pipeline controls')
