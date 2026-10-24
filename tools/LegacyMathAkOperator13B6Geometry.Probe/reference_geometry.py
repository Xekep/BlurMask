from pathlib import Path
import importlib.util,json,math,struct,sys
p=Path(sys.argv[1])/'tools/LegacyMathAkProcessedPrefix.Probe/reference_bootstrap.py';s=importlib.util.spec_from_file_location('bootstrap',p);boot=importlib.util.module_from_spec(s);s.loader.exec_module(boot)
F=boot.r.F;add=boot.r.add;sub=boot.r.sub;mul=boot.r.mul;div=boot.r.div
PI=F(math.pi);HALF=F(math.pi/2)
def sq(x):return mul(x,x)
def d2(a,b):return add(sq(sub(a[1],b[1])),sq(sub(a[0],b[0])))
def priority(a,b,p):
 u=(sub(p[0],a[0]),sub(p[1],a[1]));v=(sub(p[0],b[0]),sub(p[1],b[1]))
 cross=sub(mul(u[0],v[1]),mul(u[1],v[0]));dot=add(mul(v[1],u[1]),mul(u[0],v[0]))
 angle=F(math.atan2(cross,dot));fold=sub(F(math.fmod(add(angle,HALF),PI)),HALF)
 if fold < -HALF:fold=add(fold,PI)
 den=d2(a,b);dist=d2(a,p)
 if den!=0:
  e=(sub(b[0],a[0]),sub(b[1],a[1]));t=div(add(mul(e[0],u[0]),mul(e[1],u[1])),den)
  if t>=0:
   if t<=1:dist=d2((add(mul(e[0],t),a[0]),add(mul(e[1],t),a[1])),p)
   else:dist=d2(b,p)
 return mul(F(math.sqrt(dist)),add(div(abs(fold),HALF),.25))
def simplify(points,spacing):
 nodes=list(range(len(points)));weight={}
 sentinel=struct.unpack('<f',struct.pack('<I',0x7c23d70a))[0]
 for i in nodes:weight[i]=sentinel if i in (0,len(points)-1) else priority(points[i-1],points[i+1],points[i])
 inverse=div(1,spacing)
 while len(nodes)>2:
  k=min(range(len(nodes)),key=lambda j:weight[nodes[j]])
  if not mul(weight[nodes[k]],inverse)<=5:break
  assert 0<k<len(nodes)-1
  a,b=nodes[k-1],nodes[k+1]
  if k>1:weight[a]=priority(points[nodes[k-2]],points[b],points[a])
  if k+2<len(nodes):weight[b]=priority(points[a],points[nodes[k+2]],points[b])
  nodes.pop(k)
 return nodes

def fit(points,start,end):
 if end-start==1:return points[start],points[end]
 weighted=[];prev=F(0)
 for i in range(start,end+1):
  if i<end:
   h=mul(F(math.sqrt(d2(points[i],points[i+1]))),.5)
   mid=(mul(add(points[i+1][0],points[i][0]),.5),mul(add(points[i+1][1],points[i][1]),.5))
   weighted.append((mid,h))
  else:h=prev
  if i==start:prev=h
  weighted.append((points[i],mul(add(h,prev),.5)));prev=h
 mass=sx=sy=xx=xy=yy=F(0)
 for (x,y),w in weighted:
  wx=mul(x,w);wy=mul(y,w);mass=add(mass,w);sx=add(sx,wx);sy=add(sy,wy)
  xx=add(xx,mul(wx,x));xy=add(xy,mul(wx,y));yy=add(yy,mul(wy,y))
 cx=div(sx,mass);cy=div(sy,mass)
 covariance=sub(mul(xy,mass),mul(sy,sx));covariance=add(covariance,covariance)
 anisotropy=add(sub(sq(sy),sq(sx)),mul(sub(xx,yy),mass))
 theta=mul(F(math.atan2(covariance,anisotropy)),.5);ux=F(math.cos(theta));uy=F(math.sin(theta))
 projections=[add(mul(sub(y,cy),uy),mul(sub(x,cx),ux))for x,y in points[start:end+1]]
 def endpoint(t):return add(mul(ux,t),cx),add(mul(uy,t),cy)
 a,b=endpoint(min(projections)),endpoint(max(projections))
 if d2(a,points[end])<d2(a,points[start]):a,b=b,a
 return a,b

def join(segments):
 out=[segments[0][0]];prior=F(math.sqrt(d2(*segments[0])))
 branches=[]
 for i in range(1,len(segments)):
  ps,pe=segments[i-1];a,b=segments[i];current=F(math.sqrt(d2(a,b)))
  weighted=tuple(div(add(mul(pe[k],prior),mul(current,a[k])),add(current,prior))for k in (0,1))
  rev=tuple(sub(a[k],b[k])for k in (0,1));pd=tuple(sub(pe[k],ps[k])for k in (0,1))
  det=sub(mul(pd[1],rev[0]),mul(rev[1],pd[0]));used=False
  if det!=0:
   delta=tuple(sub(b[k],ps[k])for k in(0,1));t=div(sub(mul(delta[1],pd[0]),mul(delta[0],pd[1])),det);u=div(sub(mul(delta[1],rev[0]),mul(delta[0],rev[1])),det)
   if t>0 and u>0:
    cross=tuple(add(mul(rev[k],t),b[k])for k in(0,1));gap=d2(a,pe)
    residual=add(d2(cross,pe),d2(cross,a))
    #Native sums the four squares left-to-right, not two pairwise d2 sums.
    residual=add(add(add(sq(sub(cross[1],pe[1])),sq(sub(cross[0],pe[0]))),sq(sub(cross[1],a[1]))),sq(sub(cross[0],a[0])))
    if gap>0 and div(residual,gap)<10:
     weighted=tuple(div(add(mul(cross[k],gap),mul(weighted[k],residual)),add(residual,gap))for k in(0,1));used=True
  out.append(weighted);branches.append(used);prior=current
 out.append(segments[-1][1]);return out,branches

def execute(original):
 prefix=boot.run(original)['result'];rows=prefix['points'];spacing=div(prefix['scale'],100);result=[]
 for row in rows:
  xs=[p[0]for p in row];ys=[p[1]for p in row];threshold=mul(mul(prefix['scale'],.5),1)
  if abs(sub(max(xs),min(xs)))<threshold and abs(sub(max(ys),min(ys)))<threshold:
   result.append(dict(skippedSmallBounds=True,indices=None,points=None));continue
  selected=simplify(row,spacing)
  if len(selected)==1:poly=[row[0]];branches=[]
  else:poly,branches=join([fit(row,a,b)for a,b in zip(selected,selected[1:])])
  result.append(dict(skippedSmallBounds=False,indices=selected,points=poly,joinIntersectionBlend=branches))
 return dict(spacing=spacing,rows=result)

def controls():
 cases=[('line4',[[(0,0),(1,0),(2,0),(3,0)]]),('diagonal5',[[(i,i)for i in range(5)]]),('vertical5',[[(0,i)for i in range(5)]]),('corner',[[(0,0),(2,0),(2,2)]]),('gentle',[[(0,0),(1,.01),(2,0),(3,.01),(4,0)]]),('closed-square',[[(0,0),(1,0),(1,1),(0,1),(0,0)]]),('single-origin',[[(0,0)]])]
 results=[dict(name=n,input=x,result=execute(x))for n,x in cases]
 assert results[0]['result']['rows'][0]['indices']==[0,3]
 assert results[0]['result']['rows'][0]['points']==[(0.,0.),(3.,0.)]
 assert results[3]['result']['rows'][0]['indices']==[0,1,2]
 assert results[-1]['result']['rows'][0]['skippedSmallBounds'] is True
 #Prepared NUMERICAL DATA branch controls, not production owner ingress.
 joincases=[('gap-intersect',[((0.,0.),(1.,0.)),((1.1,.1),(1.1,1.))]),('parallel',[((0.,0.),(1.,0.)),((1.1,0.),(2.,0.))])]
 joins=[dict(name=n,segments=x,result=join(x))for n,x in joincases]
 return dict(cases=results,preparedJoinData=joins,portableCRTScope=True,csharpExecution=False,networkForwards=0)
if __name__=='__main__':Path(sys.argv[2]).write_text(json.dumps(controls(),indent=2)+'\n',encoding='utf8');print('PASS7 source-derived pure geometry cases+2 numerical joins;0NN')
