#Pure original-source numerical reference. No C#/NN/vendor execution.
from pathlib import Path
import ast,importlib.util,json,math,struct,sys
R=Path(sys.argv[1]);D=Path(__file__).resolve().parent
p=R/'tools/LegacyOrientationSmoothing.Probe/reference_controls.py';tree=ast.parse(p.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))or(isinstance(n,ast.Assign)and isinstance(n.value,ast.Lambda))];env={};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(p),'exec'),env)
f,approx,fit,metrics=[env[n]for n in ['f','approx','fit','metrics']]
p=R/'tools/LegacyMathAkOperator13B6.Probe/reference_connected.py';s=importlib.util.spec_from_file_location('connected',p);conn=importlib.util.module_from_spec(s);s.loader.exec_module(conn)

def original(c):
 if 'input'in c:return [[[f(v)for v in p]for p in row]for row in c['input']]
 r=c['recipe'];n=r['count'];step=f(r['step'])
 return [[[f(i*step),f(0 if r['kind']=='horizontal'else [.0,.001,.0,-.001][i%4])]for i in range(n)]]
def source(raw,policy):
 pref=conn.g.boot.run(raw)['result'];rows=pref['points'];scale=pref['scale'];m=metrics(rows);lo,hi,extra,ol,oh,cl,ch,tol=policy;weight=lo
 if lo<hi and 0<ol<oh and m[0]>=ol:weight=hi if m[0]>oh else f(f(f(f(hi-lo)*f(m[0]-ol))/f(oh-ol))+lo)
 if extra>0 and 0<cl<ch:
  if m[1]>ch:weight=f(weight+extra)
  elif m[1]>cl:weight=f(weight+f(f(f(m[1]-cl)*extra)/f(ch-cl)))
 out=[];maps=[];modes=[]
 for row,oldmap in zip(rows,pref['originalPointIndices']):
  #Exact native RAW-count gate, before last-accepted dedup.
  if not 3<=len(row)<=8192:out.append(row);maps.append(oldmap);modes.append('original');continue
  unique=[row[0]];accepted=[0]
  for i,p in enumerate(row[1:],1):
   if not approx(p[0],unique[-1][0])or not approx(p[1],unique[-1][1]):unique.append(p);accepted.append(i)
   elif i==len(row)-1:accepted[-1]=i
  if len(unique)<2:out.append(row);maps.append(oldmap);modes.append('original');continue
  if len(unique)<4:out.append(unique);maps.append([oldmap[i]for i in accepted]);modes.append('dedup');continue
  #Large horizontal controls have analytically zero curvature; this avoids a
  #dense8192x8192 allocation. Numeric comparison remains portable tolerance.
  if len(unique)>64:
   assert all(p[1]==unique[0][1]for p in unique)and all(a[0]<b[0]for a,b in zip(unique,unique[1:]))
   out.append(unique);maps.append([oldmap[i]for i in accepted]);modes.append('spline');continue
  h=[math.sqrt(float(f(b[0]-a[0]))**2+float(f(b[1]-a[1]))**2)for a,b in zip(unique,unique[1:])];amplitude=f(len(unique))
  for v in [weight]*4+[scale]*4:amplitude=f(amplitude*v)
  penalty=amplitude/sum(h);cx=fit([p[0]for p in unique],h,penalty);cy=fit([p[1]for p in unique],h,penalty);threshold=f(f(scale*tol)*scale);points=[];keys=[]
  def distance(p,a,b):
   sx=b[0]-a[0];sy=b[1]-a[1];dx=p[0]-a[0];dy=p[1]-a[1];den=sx*sx+sy*sy;t=0 if den==0 else min(1,max(0,(dx*sx+dy*sy)/den));return(p[0]-(a[0]+t*sx))**2+(p[1]-(a[1]+t*sy))**2
  def split(curve):
   first=[[(a+b)*.5 for a,b in zip(curve[i],curve[i+1])]for i in range(3)];second=[[(a+b)*.5 for a,b in zip(first[i],first[i+1])]for i in range(2)];middle=[(a+b)*.5 for a,b in zip(*second)];return[curve[0],first[0],second[0],middle],[middle,second[1],first[2],curve[3]]
  def walk(curve,a,b,depth,fractional,nextIndex):
   if depth>=11 or(distance(curve[1],curve[0],curve[3])<threshold and distance(curve[2],curve[0],curve[3])<threshold):
    points.append(list(map(f,curve[3])));key=int(math.floor(f(f(nextIndex-1)+b)+.5))if fractional else nextIndex;keys.append(oldmap[key]);return
   left,right=split(curve);middle=f(f(a+b)*.5);walk(left,a,middle,depth+1,True,nextIndex);walk(right,middle,b,depth+1,fractional,nextIndex)
  for i,length in enumerate(h):
   def bez(c):
    a,b,c2,d=c;return[a,a+b*length/3,a+(2*b+c2*length)*length/3,a+((d*length+c2)*length+b)*length]
   xx=bez(cx[i]);yy=bez(cy[i]);curve=list(map(list,zip(xx,yy)))
   if i==0:points.append(list(map(f,curve[0])));keys.append(oldmap[accepted[0]])
   walk(curve,0,1,0,False,accepted[i+1])
  out.append(points);maps.append(keys);modes.append('spline')
 return dict(points=out,originalIndices=maps,modes=modes,scale=scale,metrics=m,weight=weight,prefix=pref,before=conn.execute(raw))
def execute(c,policy):return source(original(c),policy)
