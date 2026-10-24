"""Independent scalar first-two-operator reference; no inference or issued request."""
import json,struct,sys
from pathlib import Path
F=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
def add(a,b):return F(F(a)+F(b))
def sub(a,b):return F(F(a)-F(b))
def mul(a,b):return F(F(a)*F(b))
def div(a,b):return F(F(a)/F(b))
def box(points):
 xs=[p[0] for p in points];ys=[p[1] for p in points]
 return min(xs),min(ys),max(xs),max(ys)
def extent(points):
 a,b,c,d=box(points);return max(sub(c,a),sub(d,b))
def blend(values,q):
 total=F(0)
 for v in values:total=add(total,v)
 rank=mul(len(values)-1,q);index=int(sub(rank,.5) if rank<=0 else add(rank,.5))
 alpha=div(2,add(sub(2,1),len(values)))
 return add(mul(sub(1,alpha),sorted(values)[index]),mul(alpha,div(total,len(values))))
def run(strokes):
 rows=[[(F(x),F(y)) for x,y in row]for row in strokes]
 nearest=[];bounds=[box(r)for r in rows]
 if len(rows)<5:
  for a in bounds:
   ds=[]
   for b in bounds:
    dx=max(sub(a[0],b[2]),sub(b[0],a[2]),0);dy=max(sub(a[1],b[3]),sub(b[1],a[3]),0);d=add(dx,dy)
    if d>0:ds.append(d)
   if ds:nearest.append(min(ds))
 near=blend(nearest,.5) if nearest else F(0)
 ext=[v for r in rows if (v:=extent(r))>0]
 initial=blend(ext,.5) if ext else F(1)
 refined=[]
 for row in rows:
  if len(row)<2:continue
  landmarks=[row[0]];last_x=row[1][0];last_dy=sub(row[1][1],row[0][1])
  for i in range(2,len(row)):
   dy=sub(row[i][1],row[i-1][1])
   if (last_dy<0<dy)or(last_dy>0>dy):landmarks.append((mul(add(last_x,row[i-1][0]),.5),row[i-1][1]))
   if dy!=0:last_x=row[i][0];last_dy=dy
  landmarks.append(row[-1])
  for start in range(0,len(landmarks),2):
   v=extent(landmarks[start:start+3])
   if mul(initial,F(.1))<v<mul(initial,F(1.1)):refined.append(v)
 scale=mul(blend(refined,F(.7)),F(1.4)) if refined else initial
 scale=max(add(near,near),scale);delta=mul(scale,F(.05))
 out=[];indices=[];base=0;expanded=0
 for row in rows:
  maps=list(range(base,base+len(row)))
  if len(row)==1:
   x,y=row[0];xx=add(x,delta);yy=add(y,delta)
   if xx!=x and yy!=y:out.append([(x,y),(x,yy),(xx,yy),(xx,y),(x,y)]);indices.append([base]*5);expanded+=1
   else:out.append(row);indices.append(maps)
  else:out.append(row);indices.append(maps)
  base+=len(row)
 return dict(scale=scale,delta=delta,expanded=expanded,points=out,originalPointIndices=indices,originalStrokeIndices=list(range(len(rows))))
def controls():
 cases=[('single-origin',[[(0,0)]]),('one-axis-rounds-unchanged',[[(1e9,0)]]),('horizontal',[[(0,0),(10,0)]]),('vertical',[[(0,0),(0,10)]]),('positive-nearest',[[(0,0)],[(10,0)]]),('five-no-nearest',[[(i*10,0)]for i in range(5)]),('flat-y-turn',[[(0,0),(2,2),(4,2),(6,0),(8,3)]]),('mixed-single-provenance',[[(0,0)],[(2,0),(4,2)],[(10,3)]])]
 results=[dict(name=n,input=s,result=run(s))for n,s in cases]
 assert results[0]['result']['scale']==1 and results[0]['result']['expanded']==1
 assert results[0]['result']['originalPointIndices']==[[0]*5]
 assert results[1]['result']['expanded']==0 #Conjunction, not either axis.
 assert results[4]['result']['scale']==20 and results[4]['result']['delta']==1
 assert results[5]['result']['scale']==1 #Count>=5 skips nearest phase.
 mixed=results[-1]['result'];assert mixed['originalPointIndices']==[[0]*5,[1,2],[3]*5]
 assert all(len(x['result']['originalStrokeIndices'])==len(x['input'])for x in results)
 return dict(cases=results,controls=len(cases),executedCSharp=False,networkForwards=0,scope='Pure finite scalar reference, not runtime producer proof')
if __name__=='__main__':
 report=controls();Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS 8 pure prefix controls; no CSharp/NN')
