from pathlib import Path
import importlib.util,sys,math,struct
R=Path(sys.argv[1]);p=R/'tools/LegacyMathAkOperator13B6Geometry.Probe/reference_geometry.py';s=importlib.util.spec_from_file_location('geom',p);g=importlib.util.module_from_spec(s);s.loader.exec_module(g)
F=g.F;add=g.add;sub=g.sub;mul=g.mul;div=g.div;PI=g.PI;HALF=g.HALF;TWO=mul(PI,2)
def wrap(v,half,period):
 x=sub(F(math.fmod(add(v,half),period)),half)
 return add(x,period) if x < -half else x
def flag(points,scale):
 if len(points)<4:return False
 lengths=[];angles=[];mean=F(0);total=F(0);variance=F(0)
 for a,b in zip(points,points[1:]):
  dx=sub(b[0],a[0]);dy=sub(b[1],a[1]);ln=F(math.sqrt(add(mul(dx,dx),mul(dy,dy))));angle=F(math.atan2(dy,dx));lengths.append(ln);angles.append(angle)
  fold=wrap(angle,HALF,PI)
  if HALF<=abs(wrap(sub(mean,fold),PI,TWO)):mean=add(mean,PI)
  mean=wrap(mean,PI,TWO);fold=wrap(fold,PI,TWO);updated=add(ln,total)
  if not updated>0:raise ValueError('positive length subset')
  mean=wrap(div(add(mul(mean,total),mul(fold,ln)),updated),HALF,PI);total=updated
 c=F(math.cos(-mean));s=F(math.sin(-mean));projected=[add(mul(s,p[0]),mul(c,p[1]))for p in points]
 if sub(max(projected),min(projected))<mul(scale,.5):return False
 runs=[];median=[];prior=None;run=F(0)
 for angle,ln in zip(angles,lengths):
  diff=sub(angle,mean);fold=wrap(diff,HALF,PI);variance=add(variance,mul(mul(ln,fold),fold));absolute=abs(wrap(diff,PI,TWO));cls=0 if F(.6)<=absolute<=sub(PI,F(.6)) else -1 if F(.6)<=absolute else 1
  if prior is None:prior=cls;run=ln
  elif prior==cls:run=add(run,ln)
  else:
   runs.append((prior,run))
   if prior!=0:median.append(run)
   prior=cls;run=ln
 runs.append((prior,run));median.append(run);median.sort();med=median[len(median)//2]
 bins=[(sum(1 for c,l in runs if c==k),F(0),F(0))for k in (-1,0,1)]
 for k in range(3):
  count,totalRun,maximum=bins[k]
  for cls,ln in runs:
   if cls==k-1:totalRun=add(totalRun,ln);maximum=max(maximum,ln)
  bins[k]=(count,totalRun,maximum)
 neg,neutral,pos=bins;small=min(neg[0],pos[0])
 if small<=1 or neg[0]-small>=6 or pos[0]-small>=6 or neg[0]+pos[0]<5:return False
 nm=div(neg[1],neg[0]);pm=div(pos[1],pos[0]);limit=mul(med,3)
 if limit<neg[2] or limit<pos[2]:return False
 small=min(nm,pm)
 if not small>0:raise ValueError('positive directional means')
 if div(nm,small)>4 or div(pm,small)>4 or neutral[1]>mul(med,1.5):return False
 score=mul(div(F(math.sqrt(div(variance,total))),HALF),struct.unpack('<f',bytes.fromhex('721c473f'))[0]);return score<F(.5)
def turn(a,b,p):return sub(mul(sub(b[0],a[0]),sub(p[1],a[1])),mul(sub(b[1],a[1]),sub(p[0],a[0])))
def hull(points):
 #Independent monotone-chain set semantics; successful convex membership is
 #orientation invariant. Collinear/repeated points disappear identically.
 ordered=sorted(set(map(tuple,points)));lo=[];hi=[]
 for row,target in [(ordered,lo),(list(reversed(ordered)),hi)]:
  for p in row:
   while len(target)>1 and not turn(target[-2],target[-1],p)>0:target.pop()
   target.append(p)
 return lo[:-1]+hi[:-1] if len(ordered)>1 else ordered
def inside(p,h):
 signs=[(v>0)-(v<0)for v in [turn(a,b,p)for a,b in zip(h,h[1:]+h[:1])]]
 return not (1 in signs and -1 in signs)
def fraction(row,h):
 if len(h)<3:return F(0)
 previous=inside(row[0],h);total=F(0);within=F(0)
 for a,b in zip(row,row[1:]):
  now=inside(b,h);ln=F(math.sqrt(g.d2(a,b)));total=add(total,ln)
  if previous and now:within=add(within,ln)
  elif previous!=now:within=add(within,mul(ln,.5))
  previous=now
 return F(1 if previous else 0)if total==0 else div(within,total)
def bbox(row):return(min(p[0]for p in row),min(p[1]for p in row),max(p[0]for p in row),max(p[1]for p in row))
def coverage(a,b):
 x=sub(add(sub(a[2],a[0]),sub(b[2],b[0])),sub(max(a[2],b[2]),min(a[0],b[0])))
 y=sub(add(sub(b[3],b[1]),sub(a[3],a[1])),sub(max(a[3],b[3]),min(a[1],b[1])))
 return div(mul(y,x),mul(sub(b[3],b[1]),sub(b[2],b[0])))if x>0 and y>0 else F(0)
def execute(original):
 prefix=g.boot.run(original)['result'];rows=prefix['points'];boxes=list(map(bbox,rows));marked=set();records=[];descriptors=0;eligible=[];analyses=[]
 for cur,row in enumerate(rows):
  if not any(prior not in marked and coverage(boxes[cur],boxes[prior])>F(.5)for prior in range(cur)):continue
  eligible.append(cur);spacing=div(prefix['scale'],100);threshold=mul(prefix['scale'],.5)
  if abs(sub(boxes[cur][2],boxes[cur][0]))<threshold and abs(sub(boxes[cur][3],boxes[cur][1]))<threshold:continue
  selected=g.simplify(row,spacing);poly=[row[0]]if len(selected)==1 else g.join([g.fit(row,a,b)for a,b in zip(selected,selected[1:])])[0]
  yes=flag(poly,prefix['scale']);analyses.append((cur,yes))
  if not yes:continue
  h=hull(row);prior=[s for s in range(cur)if s not in marked and coverage(boxes[cur],boxes[s])>F(.5)and fraction(rows[s],h)>F(.5)]
  if not prior:continue
  descriptors+=1;marked.add(cur)
  def record(s):
   ids=prefix['originalPointIndices'][s];return struct.pack('<Iff',s,0.,F(ids[-1]-ids[0])).hex()
  records.append([record(cur),''.join(record(s)for s in prior)]);marked.update(prior)
 words=[sum(1<<(s&31)for s in marked if s>>5==w)for w in range((len(rows)>>5)+1)]
 return dict(processedWords=words,records=records,descriptorCount=descriptors,rangeResultCount=len(records),eligible=eligible,analysis=analyses)
