"""Independent range-partition and actual graph/cache/context oracle."""
import base64,json,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
def normalize(source):
    values=[tuple(x) for x in source];stack=[(0,len(values)-1)]
    while stack:
        lo,hi=stack.pop();pivot=values[(lo+hi)//2][:2];i,j=lo,hi
        while i<=j:
            while values[i][:2]<pivot:i+=1
            while values[j][:2]>pivot:j-=1
            if i<=j:values[i],values[j]=values[j],values[i];i+=1;j-=1
        if i<hi:stack.append((i,hi))
        if lo<j:stack.append((lo,j))
    position=1
    while position<len(values):
        a,b=values[position-1:position+1]
        if a[0]==b[0] and b[1]==a[2]-1:
            values[position-1]=(a[0],a[1],b[2]);del values[position]
        else:position+=1
    return [list(x) for x in values]
capture=json.loads(Path(sys.argv[1]).read_bytes())
for case in capture['preparedRangeControls']:assert normalize(case['input'])==case['output']
graph=capture['graph'];groups=capture['groups'];extras=[[] for _ in graph]
def decode(ref):return ref&0xffffff,(ref>>24)&65535
def pack(node,arc):return node|(arc<<24)
higher=0;beforeMergeCapacity=0
for case in capture['merges']:
    refs=case['input'];allranges=[];capacity=0
    for ref in refs:
        node,arc=decode(ref);owned=groups[graph[node][arc][0]];required=len(allranges)+len(owned)
        if required>capacity:capacity=max(required,int(capacity*1.5))
        allranges.extend(owned)
    ranges=normalize(allranges);last,_=decode(refs[-1]);expected=None
    for i,(old,cap,original) in enumerate(extras[last]):
        if old==ranges:expected=pack(last,len(graph[last])+i);break
    if expected is None:
        matching=next((i for i,g in enumerate(groups) if g==ranges),None)
        if matching is not None:
            starts=[];ends=[]
            for ref in refs:
                n,a=decode(ref);starts.append(graph[n][a][1]);ends.append(graph[n][a][2])
            for node,arcs in enumerate(graph):
                for ordinal,arc in enumerate(arcs):
                    if arc==[matching,min(starts),max(ends)]:expected=pack(node,ordinal);break
                if expected is not None:break
        if expected is None:
            expected=pack(last,len(graph[last])+len(extras[last]));extras[last].append((ranges,capacity,refs[-1]))
    assert case['reference']==expected and case['ranges']==ranges
    n,a=decode(expected);isHigher=a>=len(graph[n]);assert case['higher']==isHigher
    if isHigher:
        stored,cap,original=extras[n][a-len(graph[n])]
        assert case['capacity']==cap and case['original']==original;higher+=1
        beforeMergeCapacity+=cap!=len(ranges)
    else:assert case['capacity'] is None and case['original']==expected
assert len(capture['preparedRangeControls'])==512
assert len(capture['merges'])==capture['mergeCount']==capture['caseMerges']==384
assert higher==capture['higherResults']==307 and beforeMergeCapacity>0
assert capture['actualResiduals']==[] and capture['actualRemaining']==0 and capture['actualRows']==2
assert base64.b64decode(capture['beforeRows'])==base64.b64decode(capture['afterRows'])
assert capture['guards']==13
print(f'PASS512 rangeoracles/384 actualgraph source merges ({higher} higher,{beforeMergeCapacity} pre-coalescing-capacities), CASE parity and real2row/0residual217DB0 path; no fabricated residual provenance')
