"""Independent point metadata consumer of dense-spline/Bezier oracle.
Imports definitions only, leaving the prior generator's test corpus untouched.
"""
import ast,json,math,random,sys
from pathlib import Path
import numpy as np

path=Path(__file__).resolve().parents[1]/'LegacyOrientationSmoothing.Probe'/'reference_controls.py'
parsed=ast.parse(path.read_text(encoding='utf-8'))
definitions=[n for n in parsed.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef)) or (isinstance(n,ast.Assign) and isinstance(n.value,ast.Lambda))]
environment={};exec(compile(ast.Module(body=definitions,type_ignores=[]),str(path),'exec'),environment)
f,approx,fit,prepare=[environment[name] for name in ('f','approx','fit','prepare')]

def keys(strokes,profile,scale,metrics):
    coords,derived,weight=prepare(strokes,profile,scale,metrics);allkeys=[];offset=0
    for points in strokes:
        unique=[points[0]];indices=[0]
        for i,point in enumerate(points[1:],1):
            if not approx(point[0],unique[-1][0]) or not approx(point[1],unique[-1][1]):unique.append(point);indices.append(i)
            elif i==len(points)-1:indices[-1]=i
        if len(points)<3 or len(unique)<2:allkeys.extend((offset+i)*200 for i in range(len(points)))
        elif len(unique)<4:allkeys.extend((offset+i)*200 for i in indices)
        else:
            h=[math.hypot(f(b[0]-a[0]),f(b[1]-a[1])) for a,b in zip(unique,unique[1:])]
            amplitude=f(len(unique))
            for factor in [weight]*4+[scale]*4:amplitude=f(amplitude*factor)
            penalty=amplitude/sum(h)
            cx=fit([v[0] for v in unique],h,penalty);cy=fit([v[1] for v in unique],h,penalty)
            threshold=f(f(scale*profile[7])*scale)
            def distance(point,a,b):
                delta=b-a;squared=float(delta@delta);t=0 if squared==0 else max(0,min(1,float((point-a)@delta)/squared))
                d=point-(a+t*delta);return float(d@d)
            def fractional(b,nextpoint,begin,end,depth=0):
                terminal(b,nextpoint,begin,end,depth,True)
            def terminal(b,nextpoint,begin,end,depth=0,left=False):
                if depth>=11 or (distance(b[1],b[0],b[3])<threshold and distance(b[2],b[0],b[3])<threshold):
                    allkeys.append(int(math.floor(f(f(f(nextpoint-1)+end)*200)+.5)) if left else nextpoint*200);return
                first=(b[:-1]+b[1:])*.5;second=(first[:-1]+first[1:])*.5;middle=(second[0]+second[1])*.5;t=f(f(begin+end)*.5)
                l=np.array([b[0],first[0],second[0],middle]);r=np.array([middle,second[1],first[2],b[3]])
                fractional(l,nextpoint,begin,t,depth+1)
                (fractional if left else terminal)(r,nextpoint,t,end,depth+1)
            for i,length in enumerate(h):
                def bezier(c):
                    a,b,c2,d=c;return[a,a+b*length/3,a+(2*b+c2*length)*length/3,a+((d*length+c2)*length+b)*length]
                b=np.column_stack([bezier(cx[i]),bezier(cy[i])])
                if i==0:allkeys.append((offset+indices[0])*200)
                terminal(b,offset+indices[i+1],0.,1.)
        offset+=len(points)
    return coords,allkeys,derived,weight

random.seed(173171);result=[]
for trial in range(256):
    strokes=[]
    for s in range(1+trial%4):
        if trial%13==0:points=[[f(s),f(s)]]*(1+trial%8)
        else:
            points=[[f(random.randrange(-80,81)+s*150),f(random.randrange(-80,81))] for i in range(1+(trial+s)%13)]
            if trial%3==0:points=[p for v in points for p in (v,v)]
            if trial%5==0:points+=points[-1:]
        strokes.append(points)
    profile=list(map(f,[.05,.075,.075,.15,.35,.10,.15,[.0001,.001,.01][trial%3]]));scale=f(8+trial%31)
    metrics=[-1.,-1.] if trial%2 else [0.,0.]
    coords,mapkeys,derived,weight=keys(strokes,profile,scale,metrics)
    result.append(dict(strokes=strokes,profile=profile,scale=scale,metrics=metrics,prepared=coords,keys=mapkeys,derived=derived,weight=weight))
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(result,separators=(',',':')),encoding='utf-8')
print(f'{len(result)} exact fractional/end key controls -> {out}')
