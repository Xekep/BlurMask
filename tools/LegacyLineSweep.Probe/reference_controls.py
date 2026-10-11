"""Independent prepared unknown-row group controls and actual proposal NN fixture composition."""
import importlib.util
import json
import math
from pathlib import Path
import sys
import numpy as np
f = np.float32

def module():
    spec = importlib.util.spec_from_file_location('pair_reference', Path(__file__).parents[1] / 'LegacyLinePairFeatures.Probe' / 'reference_controls.py')
    value = importlib.util.module_from_spec(spec); spec.loader.exec_module(value); return value

features = module()

def sort_desc(rows):
    if len(rows) < 2: return rows
    rows = rows.copy(); l, r = 0, len(rows)-1; pivot = rows[r//2][2]
    while l <= r:
        while rows[l][2] > pivot: l += 1
        while rows[r][2] < pivot: r -= 1
        if l <= r: rows[l], rows[r] = rows[r], rows[l]; l += 1; r -= 1
    return sort_desc(rows[:r+1])+rows[r+1:l]+sort_desc(rows[l:])

def sort_ids(rows,channel):
    if len(rows)<2:return rows[:]
    rows=rows.copy();a,b=0,len(rows)-1;p=rows[b//2][channel]
    while a<=b:
        while rows[a][channel]<p:a+=1
        while p<rows[b][channel]:b-=1
        if a<=b:rows[a],rows[b]=rows[b],rows[a];a+=1;b-=1
    return sort_ids(rows[:b+1],channel)+rows[b+1:a]+sort_ids(rows[a:],channel)

def stage(stats,pairs,scale,previous,mask,final):
    membership = [-1]*len(stats); groups = [g[:] for g in previous]; costs = {}; deferred=[]
    for gi,g in enumerate(groups):
        for i in g:membership[i]=gi
    pairs=sort_ids(pairs,0);cost_pairs=[];start=0
    while start<len(pairs):
        first=pairs[start]
        if not final and (mask[first[0]] or mask[first[1]]):start+=1;continue
        end=start+1
        while end<len(pairs) and pairs[end][0]==first[0]:end+=1
        pairs[start:end]=sort_ids(pairs[start:end],1);cost_pairs+=pairs[start:end];start=end
    for a, b, confidence, label in cost_pairs:
        confidence = f(confidence); other = f(f(1.0000001192092896)-confidence)
        logs = (f(math.log(float(confidence))), f(math.log(float(other))))
        costs[(a,b)] = logs[::-1] if label else logs
    pitch = float(scale)*1.4500000476837158
    def center(ids):
        weights = [max(s[6], f(1)) for s in [stats[i] for i in ids]]
        total = f(0)
        for w in weights: total = f(total+w)
        return sum(float(w)/float(total)*float(stats[i][5]) for i,w in zip(ids,weights))
    def near(a,b): return abs(center(a)-center(b)) < pitch
    def join(a,b):
        yes, no = f(0), f(0)
        for x in a:
            for y in b:
                c = costs.get(tuple(sorted([x,y])))
                if c: yes, no = f(yes+c[0]), f(no+c[1])
        if yes != 0 and no != 0 and (f(yes+no) >= -5 or abs(f(no-yes)) < 5.5): return near(a,b)
        return no < yes
    def box(ids):
        return [min(stats[i][j] for i in ids) if j < 2 else max(stats[i][j] for i in ids) for j in range(4)]
    def compatible(a,b):
        a.sort(); b.sort()
        lo = lambda g: g[math.floor(float(f(f(f(len(g))/f(20))+f(.5))))]
        hi = lambda g: g[math.floor(float(f(f(f(len(g))/f(20))*f(19))))]
        if lo(a) < hi(b) and lo(b) < hi(a): return True
        aa, bb = box(a), box(b)
        aw, ah, bw, bh = f(aa[2]-aa[0]), f(aa[3]-aa[1]), f(bb[2]-bb[0]), f(bb[3]-bb[1])
        with np.errstate(divide='ignore', invalid='ignore'):
            if f(aw/ah) < 5 or not(f(bw/bh) >= 5 and abs(f(ah-bh)) <= f(scale*f(.25))): return True
            ox = f(f(bw+aw)-f(max(aa[2],bb[2])-min(aa[0],bb[0])))
            if not(ox > 0 and max(f(ox/bw),f(ox/aw)) >= .75): return True
            oy = f(f(bh+ah)-f(max(aa[3],bb[3])-min(aa[1],bb[1])))
            return bool(max(f(oy/bh),f(oy/ah)) >= .75)
    for a,b,p,label in sort_desc(pairs):
        if not final and (mask[a] or mask[b]):deferred.append([a,b,p,label]);continue
        if label: continue
        ga,gb = membership[a],membership[b]
        if ga == gb and ga >= 0: continue
        if ga < 0 and gb < 0:
            if f(p) <= f(.99) and not near([a],[b]): continue
            groups.append([a,b]); membership[a] = membership[b] = len(groups)-1
        elif ga < 0:
            if join(groups[gb],[a]): groups[gb].append(a); membership[a] = gb
        elif gb < 0:
            if join(groups[ga],[b]): groups[ga].append(b); membership[b] = ga
        else:
            limit = f(scale*f(10))
            if f(stats[a][3]-stats[a][1]) > limit or f(stats[b][3]-stats[b][1]) > limit or not join(groups[ga],groups[gb]) or (f(p) < f(.98) and not near(groups[ga],groups[gb])) or not compatible(groups[ga],groups[gb]): continue
            groups[ga] += groups[gb]
            for i in groups[gb]: membership[i] = ga
            del groups[gb]
            membership = [g-1 if g >= gb else g for g in membership]
    return groups,membership,deferred

def import_ref(name,folder):
    spec=importlib.util.spec_from_file_location(name,Path(__file__).parents[1]/folder/'reference_controls.py');out=importlib.util.module_from_spec(spec);spec.loader.exec_module(out);return out

proposal_ref=import_ref('sweep_proposals','LegacyNeuralLineProposals.Probe')
source_ref=import_ref('sweep_source','LegacyOrdinaryLineScale.Probe')
post_ref=import_ref('sweep_post','LegacyLinePostselection.Probe')

def aggregate(rows):
    result={}
    for a,b,p,label in rows:
        k=tuple(sorted([a,b]))
        if k not in result or p>result[k][0]:result[k]=(p,label)
    return [[*k,*v] for k,v in result.items()]

def predict(stats,ids,scale,width,network,tiny):
    rows=np.array([features.pair(stats[a],stats[b],a,b,scale,width,tiny) for a,b in zip(ids,ids[1:])])
    net,affine,means,scales=network
    hidden=np.tanh(np.array([proposal_ref.network.reference.affine(affine,x) for x in (rows-means)*scales])).astype(np.float32)
    scores,_=proposal_ref.network.reference.infer(net,hidden)
    return [[a,b,float(max(row)),bool(row[0]<row[1])] for a,b,row in zip(ids,ids[1:],scores)]

def internal_samples(points,half,quarter):
    if len(points) < 2:
        return []
    anchor_x, anchor_y = points[0]
    older_y = anchor_y
    out = []

    def append(x,y,endpoint=False):
        dx, dy = abs(f(x-anchor_x)), abs(f(y-anchor_y))
        horizontal = f(dx * f(.5)) if half else dx
        out.append(max(f(dy*f(.75)) if half and quarter and not endpoint else dy,horizontal))

    for i in range(1, len(points)):
        current_y = points[i][1]
        previous_x, previous_y = points[i-1]
        if current_y == previous_y:
            continue
        first = previous_y <= older_y or previous_y <= current_y
        if first:
            second = older_y <= previous_y
            older_y = previous_y
            if second or current_y <= previous_y:
                continue
        append(previous_x, previous_y)
        older_y = previous_y
        anchor_x, anchor_y = previous_x, previous_y
    if points[-1][1] != anchor_y:
        append(*points[-1],endpoint=True)
    return out


def whole(raw,half,quarter,q,models):
    clean=[source_ref.clean(s) for s in raw];stats=[features.statistics(s) for s in clean]
    source_samples=[v for s in clean for v in source_ref.sample.extract(s,False)]
    initial=f(source_ref.geometry.scales(source_samples,0,96,list(map(f,[.96,.85,3,10])))[0])
    internal=[v for s,h in zip(clean,half) for v in internal_samples(source_ref.sample.prepare(s),h,quarter)]
    scale=initial
    if len(stats)>1 and len(internal)>9:
        pos=f(f(len(internal)-1)*q);idx=min(len(internal)-1,int(f(pos+f(.5))))
        scale=max(f(sorted(internal)[idx]),f(f(96)/f(25.4)))
    tiny=[] if len(stats)==1 else [i for i,r in enumerate(stats) if f(r[3]-r[1])<=f(scale*f(.1)) or r[6]<=f(scale*f(.25))]
    width,proposals=proposal_ref.prepare(stats,[True]*len(stats),f(scale*f(5)),f(.75))
    groups=[];member=[-1]*len(stats);pending=[];allpairs=[];strips=0
    top=min(s[1] for s in stats);bottom=max(s[3] for s in stats);height=f(scale*f(40));margin=f(height*f(.2))
    if len(stats)==1:groups=[[0]];member=[0]
    else:
        while top<bottom:
            strips+=1;end=f(top+height);final=bottom-margin<=end
            if final:end=bottom
            future=[False]*len(stats);current=[]
            for prop in proposals:
                if len(prop['ids'])<=1:continue
                ids=[]
                for i in prop['ids']:
                    r=stats[i]
                    if r[3]<top or end<r[1]:
                        if end<r[1]:future[i]=True
                        continue
                    ids.append(i)
                    if end-margin<r[3]:future[i]=True
                if len(ids)>1:current+=predict(stats,ids,scale,f(width),models[0 if prop['ordering']!=3 else 1],tiny)
            allpairs=aggregate(allpairs+current);groups,member,pending=stage(stats,aggregate(pending+current),scale,groups,future,final)
            if final:break
            top=f(end-margin)
        for i,g in enumerate(member):
            if g<0:member[i]=len(groups);groups.append([i])
    groups,member=post_ref.post(stats,groups,scale)
    return dict(neuralScale=float(scale),strips=strips,tiny=tiny,groups=[sorted(g) for g in groups],membership=member,pairs=allpairs)

def main():
    output=Path(sys.argv[1]);output.parent.mkdir(parents=True,exist_ok=True);rng=np.random.default_rng(7853160);controls=[]
    for trial in range(512):
        n=2+trial%11;stats=[features.statistics(rng.uniform(-200,200,(5,2)).astype(np.float32).tolist()) for i in range(n)]
        pairs=[[a,b,float(f([.8,.95,.999][trial%3])),bool(rng.random()<.25)] for a in range(n) for b in range(a+1,n) if rng.random()>.25]
        pairs=[pairs[i] for i in rng.permutation(len(pairs))]
        previous=[[i,i+1] for i in range(0,n-1,3)] if trial%3 else []
        mask=[bool(rng.random()<.35) for _ in range(n)];final=bool(trial%5==0);scale=f([10,50,100][trial%3])
        groups,membership,deferred=stage(stats,pairs,scale,previous,mask,final)
        controls.append(dict(statistics=[s.tolist() for s in stats],pairs=pairs,previous=previous,mask=mask,final=final,scale=float(scale),groups=groups,membership=membership,deferred=deferred))
    paths=[output.parent/'sweep-own30.res',output.parent/'sweep-own50.res']
    for width,path in zip([30,50],paths):proposal_ref.network.synthetic(width,path)
    models=[proposal_ref.network.load(path) for path in paths];full=[]
    for i in range(16):
        raw=[]
        for stroke in range(1+i%5):
            center=(stroke%3)*[30,5000][i%2]
            points=[[float(f(stroke*25+p*3)),float(f(center+(20 if p%2 else -20)))] for p in range(2+i%17)]
            if i==0:points=[[0,20],[20,20]]
            raw.append(points)
        half=[bool(i%4==0)]*len(raw);q=f([0,.8,1][i%3]);quarter=bool(i%3==0);expected=whole(raw,half,quarter,q,models)
        full.append(dict(strokes=raw,half=half,quarter=quarter,quantile=float(q),expected=expected))
    output.write_text(json.dumps(dict(stages=controls,models=[str(p.resolve()) for p in paths],full=full),separators=(',',':')),encoding='utf8');print(f'Wrote {len(controls)} independent continuation controls and {len(full)} full raw/synthetic-NN sweep controls.')
if __name__=='__main__':main()
