"""Independent stable-move path oracle for one deferred native seed."""
import json,random,sys
rng=random.Random(0x1a1ef0)
cases=[]
for _ in range(384):
    count=rng.randint(2,40); selected=rng.randrange(count-1); deferred=selected+1
    total=count+1; order=list(range(total)); paths=[]
    for shift in range(min(2,deferred)+1):
        path=order[:]; path.pop(deferred); path.insert(deferred-shift,deferred); paths.append(path)
    physical=lambda group:count if group==deferred else group if group<deferred else group-1
    arcs=[]
    for node in range(total):
        groups=([node] if node!=deferred else [])
        if deferred-2<=node<=deferred:groups.append(deferred)
        if deferred-1<=node<=deferred and node>0:groups.append(node-1)
        # The node before deferred has its own normal group first; the
        # deferred node starts with the deferred group.
        arcs.append([dict(group=group,pred=[],succ=[]) for group in groups])
    for node,variants in enumerate(arcs):
        for variant in variants:
            compatible=[path for path in paths if path[node]==variant['group']]
            variant['pred']=sorted({-1 if node==0 else next(i for i,a in enumerate(arcs[node-1]) if a['group']==path[node-1]) for path in compatible})
            variant['succ']=sorted({next(i for i,a in enumerate(arcs[node+1]) if a['group']==path[node+1]) for path in compatible if node+1<total})
            before={physical(group) for path in compatible for group in path[:node]}
            after={physical(group) for path in compatible for group in path[node:]}
            variant['flags']=(0 if before&after else 4) | (1 if variant['group']==deferred and node-deferred!=-1 else 0)
    # Context IDs are canonical classes of adjacency sets in native encounter
    # order. Build identity dictionaries, without replaying native linked lists.
    for node,variants in enumerate(arcs):
        left={}; right={}; next_id=1
        for ordinal,variant in enumerate(variants):
            key=tuple(variant['pred'])
            if key not in left:
                left[key]=node*128 if ordinal==0 else node*128+2*next_id
                if ordinal!=0:next_id+=1
            variant['start']=left[key]
        for variant in variants:
            key=tuple(variant['succ'])
            if key not in right:right[key]=node*128+2*next_id+1;next_id+=1
            variant['end']=right[key]
    cases.append(dict(count=count,selected=selected,paths=paths,arcs=arcs))
with open(sys.argv[1],'w',encoding='utf-8') as stream:json.dump(cases,stream,separators=(',',':'))
print('Wrote',len(cases),'independent stable-move graph references')
