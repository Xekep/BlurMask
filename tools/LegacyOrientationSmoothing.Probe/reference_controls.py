"""Independent child265 oracle: dense NumPy solve of R+6lambda*Q*Q.T,
then cubic/Bezier subdivision. No native binary, band-LU implementation or C# calls.
"""
import json, math, random, sys
from pathlib import Path
import numpy as np

f=lambda x:float(np.float32(x))
approx=lambda a,b:abs(f(f(a)-f(b)))<=max(f(max(abs(a),abs(b))*2**-22),2**-23)

def fit(values,h,penalty):
    n=len(values);h=np.asarray(h,dtype=np.float64);values=np.asarray(values,dtype=np.float32)
    q=np.zeros((n-2,n));r=np.zeros((n-2,n-2));rhs=np.zeros(n-2)
    for i in range(n-2):
        inv0,inv1=1/h[i],1/h[i+1]
        q[i,i:i+3]=[inv0,-(inv0+inv1),inv1]
        r[i,i]=2*(h[i]+h[i+1])
        if i+1<n-2:r[i,i+1]=r[i+1,i]=h[i+1]
        rhs[i]=3*(float(f(values[i+2]-values[i+1]))*inv1-float(f(values[i+1]-values[i]))*inv0)
    curvature=np.zeros(n);curvature[1:-1]=np.linalg.solve(r+6*penalty*(q@q.T),rhs)
    fitted=values.astype(float)-2*penalty*(q.T@curvature[1:-1])
    result=[]
    for i,length in enumerate(h):
        result.append([float(fitted[i]),float((fitted[i+1]-fitted[i])/length-length*(2*curvature[i]+curvature[i+1])/3),float(curvature[i]),float((curvature[i+1]-curvature[i])/(3*length))])
    result.append([float(fitted[-1]),0.,0.,0.]);return result

def metrics(strokes):
    edges=ortho=corners=0
    for points in strokes:
        edges+=len(points)-1
        for i in range(1,len(points)):
            vertical=approx(points[i][0],points[i-1][0]);horizontal=approx(points[i][1],points[i-1][1])
            ortho+=int(vertical or horizontal)
            if i+1<len(points):corners+=int((vertical and approx(points[i][1],points[i+1][1])) or (horizontal and approx(points[i][0],points[i+1][0])))
    return [f(ortho/edges) if edges else 0.,f(corners/(edges-len(strokes))) if edges>len(strokes) else 0.]

def prepare(strokes,profile,s,supplied):
    m=metrics(strokes) if min(supplied)<0 else supplied
    low,high,extra,ol,oh,cl,ch,tolerance=profile;weight=low
    if low<high and 0<ol<oh and m[0]>=ol:
        weight=high if m[0]>oh else f(f(f(f(high-low)*f(m[0]-ol))/f(oh-ol))+low)
    if extra>0 and 0<cl<ch:
        if m[1]>ch:weight=f(weight+extra)
        elif m[1]>cl:weight=f(weight+f(f(f(m[1]-cl)*extra)/f(ch-cl)))
    output=[]
    for points in strokes:
        unique=[points[0]]
        for point in points[1:]:
            if not approx(point[0],unique[-1][0]) or not approx(point[1],unique[-1][1]):unique.append(point)
        if len(points)<3 or len(unique)<2:output.append(points);continue
        if len(unique)<4:output.append(unique);continue
        h=[math.hypot(f(b[0]-a[0]),f(b[1]-a[1])) for a,b in zip(unique,unique[1:])]
        amplitude=f(len(unique))
        for factor in [weight]*4+[s]*4:amplitude=f(amplitude*factor)
        penalty=amplitude/sum(h)
        cx=fit([v[0] for v in unique],h,penalty);cy=fit([v[1] for v in unique],h,penalty)
        flattened=[];threshold=float(f(f(s*tolerance)*s))
        def distance(point,a,b):
            #Independent vector projection onto the closed segment.
            delta=b-a;denominator=float(delta@delta)
            t=0 if denominator==0 else max(0,min(1,float((point-a)@delta)/denominator))
            residual=point-(a+t*delta);return float(residual@residual)
        def flatten(b,depth=0):
            if depth>=11 or (distance(b[1],b[0],b[3])<threshold and distance(b[2],b[0],b[3])<threshold):
                flattened.append(list(map(f,b[3])));return
            level1=(b[:-1]+b[1:])*.5;level2=(level1[:-1]+level1[1:])*.5;center=(level2[0]+level2[1])*.5
            flatten(np.array([b[0],level1[0],level2[0],center]),depth+1)
            flatten(np.array([center,level2[1],level1[2],b[3]]),depth+1)
        for index,length in enumerate(h):
            def bezier(c):
                a,b,c2,d=c
                return [a,a+b*length/3,a+(2*b+c2*length)*length/3,a+((d*length+c2)*length+b)*length]
            b=np.column_stack([bezier(cx[index]),bezier(cy[index])])
            if not index:flattened.append(list(map(f,b[0])))
            flatten(b)
        output.append(flattened)
    return output,m,weight

random.seed(2651717);fits=[];preparations=[]
for trial in range(256):
    n=4+trial%21;v=[f(random.randrange(-100,101)*.25) for _ in range(n)];h=[random.uniform(1,50) for _ in range(n-1)]
    penalty=0. if trial%7==0 else 10**random.uniform(-5,4)
    fits.append(dict(values=v,lengths=h,penalty=penalty,coefficients=fit(v,h,penalty)))
for trial in range(192):
    strokes=[]
    for s in range(1+trial%4):
        n=1+((trial+s)%12)
        if trial%4==0:
            points=[[f(i//2*10+s*20),f((i%4)//2*20+s*10)] for i in range(n)]
        else:points=[[f(random.randrange(-30,31)+s*40),f(random.randrange(-40,41))] for _ in range(n)]
        if trial%9==0:points+=points[-1:]
        strokes.append(points)
    profile=list(map(f,[.05,.075,.075,.15,.35,.1,.15,[.0001,.001,.01][trial%3]]))
    supplied=[-1.,-1.] if trial%3==0 else [0.,0.] if trial%3==1 else [f(.22),f(.12)]
    s=f(10+trial%35);prepared,derived,weight=prepare(strokes,profile,s,supplied)
    preparations.append(dict(strokes=strokes,profile=profile,scale=s,metrics=supplied,prepared=prepared,derived=derived,weight=weight))
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(dict(fits=fits,preparations=preparations),separators=(',',':')),encoding='utf-8')
print(f'{len(fits)} dense spline +{len(preparations)} full child265 controls -> {out}')
