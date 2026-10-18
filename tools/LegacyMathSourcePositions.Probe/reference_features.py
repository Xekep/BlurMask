"""Independent feature oracle, mechanically extracted from static research.

Only stdlib/NumPy; no vendor/model/runtime imports. Original function bodies
are unchanged. Source file digests below document extraction provenance; the
original ignored research files are not required to run this tracked oracle.
"""
import math
import numpy as np

F = np.float32
EPS = F(1.1920929e-7)

SOURCE_FILE_SHA256 = {'static_text_preprocessor.py': '0b2871f9d8c1dda88eb7568ce4f579f9b22943f34901b995c5288093cee2b5a8', 'math_preprocessor_prototype.py': '33fe1bcb5159b3b1ec436b993fff21b25c740bc4e9705620d72ceec9692bb2a1', 'math_feature_prototype.py': '43020bd14532181c15675dd3d70957e0826299d8b54f3543e55e0b357a43181f'}


def _distance(a, b):
    x, y = F(b[0]-a[0]), F(b[1]-a[1])
    return F(np.sqrt(F(F(x*x)+F(y*y))))


def _close(a,b):
    return abs(F(a-b))<=max(EPS,F(max(abs(a),abs(b))*F(2.3841858e-7)))


def group_extrema_scale(strokes,quantil=.5):
    """FUN18001c380: quantile of significant chords between vertical extrema."""
    arrays=[np.asarray(s,dtype=np.float32) for s in strokes]
    points=np.concatenate(arrays)
    bboxheight=F(points[:,1].max()-points[:,1].min())
    if bboxheight==0: return F(1),[],float(bboxheight)
    threshold=F(F(bboxheight*F(.01))*F(bboxheight*F(.01)))
    distances=[]
    for stroke in arrays:
        anchor=stroke[0].copy();previous_dy=F(0);last_vertical_x=F(np.finfo(np.float32).max)
        for i,point in enumerate(stroke):
            distance2=F(0);vertical=False
            if i<len(stroke)-1:
                dy=F(stroke[i+1,1]-point[1])
                if F(dy*previous_dy)<0:
                    delta=point-anchor
                    distance2=F(F(delta[1]*delta[1])+F(delta[0]*delta[0]))
                    vertical=_close(anchor[0],point[0])
                    anchor=point.copy()
                if dy!=0: previous_dy=dy
            else:
                delta=point-anchor
                distance2=F(F(delta[1]*delta[1])+F(delta[0]*delta[0]))
            if threshold<distance2:
                if vertical and _close(last_vertical_x,anchor[0]): continue
                distances.append(F(np.sqrt(distance2)))
                if vertical:last_vertical_x=anchor[0]
    ordered=sorted(distances)
    # Native selects floor(n*quantil), not the average of the middle pair.
    scale=ordered[int(F(F(len(ordered))*F(quantil)))] if ordered else bboxheight
    return F(scale),[float(d) for d in ordered],float(bboxheight)


def resample_v1(strokes,spacing=.4):
    """FUN180017060 version1, noise0, has_interstrokesfalse point branch.

    Coordinates remain centered; original fractional indices are returned as
    diagnostic values; native 1/200 raw-position quantization is used to
    suppress duplicate interpolations, as in 18000db00 and 180017060.
    """
    result=[];positions=[];details=[]
    for stroke in strokes:
        s=np.asarray(stroke,dtype=np.float32)
        if len(s)>=0x8000:raise ValueError('Native 15-bit raw pointer limit exceeded')
        ds=[_distance(a,b) for a,b in zip(s,s[1:])]
        length=F(0)
        for distance in ds:length=F(length+distance)
        minimum=min(ds) if ds else F(np.finfo(np.float32).max)
        step=max(F(.001),F(max(F(1),minimum)*F(spacing)))
        intervals=0
        if length>0:
            intervals=max(0,int(F(F(length/step)-F(.5)))+1)
            if intervals==0: raise ValueError('Unexpected zero intervals')
            step=F(length/F(intervals))
        sampled=[s[0].tolist()];indices=[0.0];residual=F(0);last_key=(0,255 if len(s)==1 else 0)
        for j,current in enumerate(s[1:],1):
            previous=s[j-1];distance=_distance(previous,current)
            combined=F(residual+distance)
            if step<combined and EPS<distance:
                fraction=F(F(step-residual)/distance)
                while fraction<1:
                    combined=F(combined-step)
                    point=[F(F(fraction*current[k])+F(F(F(1)-fraction)*previous[k])) for k in (0,1)]
                    position=F(F(j-1)+fraction)
                    integral=math.floor(float(position))
                    quantized=int(F(F(F(position-F(integral))*F(200))+F(.5)))
                    if quantized==200:integral+=1;quantized=255 if integral==len(s)-1 else 0
                    key=(integral,quantized)
                    distinct=key[0]!=last_key[0] or (key[1]!=last_key[1] and key[1]!=255 and last_key[1]!=255)
                    if distinct:
                        sampled.append([float(v) for v in point]);indices.append(float(position));last_key=key
                    fraction=F(fraction+F(step/distance))
            residual=combined
        if step<F(residual+residual):
            sampled.append(s[-1].tolist());indices.append(float(len(s)-1))
        # v1 retains one for single-point input; non-singleton needs two.
        if len(s)>1 and len(sampled)==1:
            sampled.append(s[-1].tolist());indices.append(float(len(s)-1))
        result.append(sampled);positions.append(indices)
        details.append(dict(raw_points=len(s),frames=len(sampled),length=float(length),spacing=float(step),intervals=intervals))
    return result,positions,details


def math7(strokes, disable_inter_strokes=False):
    """FUN180007990: x,y,end,next-current,next2-current."""
    count=sum(len(s) for s in strokes)
    out=np.zeros((count,7),dtype=np.float32)
    index=0
    for stroke in strokes:
        for j,p in enumerate(stroke):
            out[index,:2]=p
            out[index,2]=float(j==len(stroke)-1)
            index+=1
    for i in range(max(0,count-2)):
        if disable_inter_strokes and out[i,2]==1: continue
        out[i,3:5]=out[i+1,:2]-out[i,:2]
        if not (disable_inter_strokes and out[i+1,2]==1):
            out[i,5:7]=out[i+2,:2]-out[i,:2]
    if count>1 and not disable_inter_strokes:
        out[-2,3:5]=out[-1,:2]-out[-2,:2]
    return out


def _closest_on_perimeter(point,bounds):
    # FUN180009e30 tests top,left,right,bottom segments in this order.
    xmin,ymin,xmax,ymax=bounds
    candidates=[(np.clip(point[0],xmin,xmax),ymin),
                (xmin,np.clip(point[1],ymin,ymax)),
                (xmax,np.clip(point[1],ymin,ymax)),
                (np.clip(point[0],xmin,xmax),ymax)]
    best=None
    for candidate in candidates:
        q=np.asarray(candidate,dtype=np.float32)
        delta=q-point
        d2=np.float32(delta[0]*delta[0]+delta[1]*delta[1])
        if best is None or d2<best[0]: best=(d2,q)
    return best


def knn6_proximity(strokes,k=6):
    """FUN180009e30 default branch; missing-neighbor cells default-allocator zero.

    Bounded max-heap insertion and stable small sort follow 180009ba0/9cd0.
    """
    arrays=[np.asarray(s,dtype=np.float32) for s in strokes]
    if any(len(s)==0 for s in arrays): raise ValueError('Empty strokes unsupported')
    bounds=[(*s.min(axis=0),*s.max(axis=0)) for s in arrays]
    output=np.zeros((sum(map(len,arrays)),3*k),dtype=np.float32)
    row=0
    for own,stroke in enumerate(arrays):
        for point in stroke:
            heap=[]
            for other,b in enumerate(bounds):
                if other==own:continue
                candidate=_closest_on_perimeter(point,b)
                d2=candidate[0]
                if len(heap)==k:
                    if heap[0][0]<=d2:continue
                    index=0
                    while index*2+1<len(heap):
                        child=index*2+1
                        if child<len(heap)-1 and heap[child][0]<=heap[child+1][0]:child+=1
                        if heap[child][0]<=d2:break
                        heap[index]=heap[child];index=child
                    heap[index]=candidate
                else:
                    index=len(heap);heap.append(candidate)
                    while index>0:
                        parent=(index-1)//2
                        if d2<=heap[parent][0]:break
                        heap[index]=heap[parent];index=parent
                    heap[index]=candidate
            for j,(d2,q) in enumerate(sorted(heap,key=lambda pair:float(pair[0]))):
                distance=np.float32(np.sqrt(d2))
                output[row,j]=np.float32(1)/np.float32(distance+np.float32(1))
                if distance>0:
                    delta=(q-point)/distance
                    output[row,k+j]=delta[0]
                    output[row,2*k+j]=delta[1]
            row+=1
    return output


def features26(strokes,ink_symbol_id):
    """Scheme order symbols,math7,knn6Prox; explicit ink ID avoids guessing."""
    m=math7(strokes)
    symbols=np.full((len(m),1),ink_symbol_id,dtype=np.float32)
    return np.concatenate([symbols,m,knn6_proximity(strokes)],axis=1)
