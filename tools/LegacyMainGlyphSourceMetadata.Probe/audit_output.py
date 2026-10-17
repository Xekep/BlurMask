"""Independent actual source-graph merge plus fixed-point/key/residual oracle."""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
c=json.loads(Path(sys.argv[1]).read_bytes());graph=c['graph'];groups=c['groupRanges'];keys=c['keys'];ends=c['endingKeys'];factor=c['factor'];ticks=c['map'];counts=c['counts']
def decode(ref):return ref&0xffffff,(ref>>24)&65535
def pack(node,arc):return node|(arc<<24)
def normalize(source):
    # Native range partition is deliberately not a stable sort on ties.
    values=[tuple(r) for r in source];pending=[(0,len(values)-1)]
    while pending:
        lo,hi=pending.pop();pivot=values[(lo+hi)//2][:2];i,j=lo,hi
        while i<=j:
            while values[i][:2]<pivot:i+=1
            while values[j][:2]>pivot:j-=1
            if i<=j:values[i],values[j]=values[j],values[i];i+=1;j-=1
        if i<hi:pending.append((i,hi))
        if lo<j:pending.append((lo,j))
    pos=1
    while pos<len(values):
        first,second=values[pos-1:pos+1]
        if first[0]==second[0] and second[1]==first[2]-1:values[pos-1]=(first[0],first[1],second[2]);del values[pos]
        else:pos+=1
    return [list(r) for r in values]
metadata=[];offset=0
for count in counts:
    row=[]
    for i in range(count):
        tick=ticks[offset+i] if ticks else offset+i;whole,remainder=divmod(tick,factor);index=whole+(remainder!=0)
        row.append(dict(first=keys[index],end=ends[index],index=index,residual=(factor-remainder)&255 if remainder else 0))
    metadata.append(row);offset+=count
assert not ticks or len(ticks)==offset
def source_bytes(ranges):
    rows=[]
    for stroke,start,end in ranges:rows.append(dict(first=metadata[stroke][start],last=metadata[stroke][end-1],tail=end==counts[stroke]))
    def order(r):return tuple(r['first']['first'])+(-r['first']['residual'],)+tuple(r['last']['end'])+(-r['last']['residual'],)
    for stop in range(len(rows)-1,0,-1):
        for i in range(stop):
            if order(rows[i])>order(rows[i+1]):rows[i],rows[i+1]=rows[i+1],rows[i]
    for i in range(len(rows)-1,0,-1):
        prev,cur=rows[i-1],rows[i];p=prev['last'];f=cur['first']
        if f['first']==p['first'] and f['residual']==p['residual'] or prev['tail'] and p['index']+1<len(keys) and f['first']==keys[p['index']+1]:
            prev['last']=cur['last'];prev['tail']=cur['tail'];del rows[i]
    refs=b'';res=b''
    for row in rows:
        f=row['first'];e=row['last'];refs+=struct.pack('<iiiiii',*f['first'],*(e['end'] if row['tail'] else e['first']));res+=bytes([f['residual'],e['residual']])
    return refs,res
extras=[[] for _ in graph];higher=nonzero=total_refs=0
for q in c['queries']:
    target=q['target'];n,a=decode(target);inputs=q['mergeInputs'];expected=target
    if inputs:
        raw=[];starts=[];stops=[]
        for ref in inputs:
            sn,sa=decode(ref);group,start,end=graph[sn][sa];raw+=groups[group];starts.append(start);stops.append(end)
        merged=normalize(raw);last,_=decode(inputs[-1]);expected=None
        for i,(old,original) in enumerate(extras[last]):
            if old==merged:expected=pack(last,len(graph[last])+i);break
        if expected is None:
            matching=next((i for i,ranges in enumerate(groups) if ranges==merged),None)
            if matching is not None:
                for node,arcs in enumerate(graph):
                    for ordinal,arc in enumerate(arcs):
                        if arc==[matching,min(starts),max(stops)]:expected=pack(node,ordinal);break
                    if expected is not None:break
            if expected is None:expected=pack(last,len(graph[last])+len(extras[last]));extras[last].append((merged,inputs[-1]))
        assert target==expected
    is_higher=a>=len(graph[n]);assert q['higher']==is_higher
    physical,original=extras[n][a-len(graph[n])] if is_higher else (groups[graph[n][a][0]],target)
    assert q['physicalRanges']==physical and q['base']==original
    bn,ba=decode(original);assert q['group']==graph[bn][ba][0]
    expected_refs,expected_res=source_bytes(physical)
    assert base64.b64decode(q['references24'])==expected_refs,(target,'refs')
    assert base64.b64decode(q['residuals2'])==expected_res,(target,'residuals')
    higher+=is_higher;nonzero+=sum(x!=0 for x in expected_res);total_refs+=len(expected_res)//2
assert higher>0 and c['guards']==8
print(f"PASS independent actual graph/range-cache/originalLast/raw-map oracle: {len(c['queries'])} queries/{higher} higher/{total_refs} source24 records/{nonzero} nonzero endpoint bytes/8 guards.")
