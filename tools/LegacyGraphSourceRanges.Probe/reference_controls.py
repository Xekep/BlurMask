"""Independent native-path/metadata oracle. No managed library or vendor execution."""
import argparse
import json
import pathlib
import random

p=argparse.ArgumentParser();p.add_argument('path');p.add_argument('--audit',action='store_true');a=p.parse_args()
if not a.audit:
    rng=random.Random(0x1a9ae0);cases=[]
    for i in range(512):
        raw=[]
        for s in range(1+i%4):
            points=[]
            for n in range(2+rng.randrange(8)):
                point=[s*100+n*5+rng.randrange(3),rng.randrange(-90,91)]
                if points and rng.randrange(3)==0:point=points[-1].copy()
                points.append(point)
            raw.append(points)
        cases.append(dict(raw=raw,reorder=False,shift=0))
    for i in range(64):
        positions=[0,200,400,70+i%20,95+i%13]
        raw=[[[x,10],[x+10,35],[x+20,75],[x+10,90]]for x in positions]
        cases.append(dict(raw=raw,reorder=True,shift=1))
    pathlib.Path(a.path).write_text(json.dumps(dict(cases=cases)),encoding='utf-8')
    print('Generated512 independent raw inputs and64 interacting deferred graph inputs')
else:
    query_count=0;merged=0;tail_differences=0;branch_cases=0
    for case in json.loads(pathlib.Path(a.path).read_text(encoding='utf-8')):
        nodes=case['nodes'];seeds=case['seeds'];metadata=case['metadata'];keys=case['keys']
        branch_cases+=any(len(node)>1 for node in nodes)
        def visit(node,ordinal,target):
            group,start,end,children=nodes[node][ordinal]
            if target<end:return None
            if target==end:return [group]
            for child in children:
                path=visit(node+1,child,target)
                if path is not None:return [group]+path
            return None
        for query in case['queries']:
            path=None
            for ordinal,arc in enumerate(nodes[query['first']>>7]):
                if arc[1]==query['first']:
                    path=visit(query['first']>>7,ordinal,query['last'])
                    if path is not None:break
            assert path is not None
            rows=[]
            for group in path:
                stroke,start,end=seeds[group];first=metadata[stroke][start];last=metadata[stroke][end-1]
                tail=end==len(metadata[stroke])
                rows.append(dict(first=first,last=last,tail=tail))
                tail_differences+=tail and last[0]!=last[1]
            #Use the independent native bubble comparator rather than OrderBy.
            for end in range(len(rows)-1,0,-1):
                for i in range(end):
                    def order(r):return tuple(r['first'][0])+(-r['first'][3],)+tuple(r['last'][1])+(-r['last'][3],)
                    if order(rows[i])>order(rows[i+1]):rows[i],rows[i+1]=rows[i+1],rows[i]
            for i in range(len(rows)-1,0,-1):
                previous,current=rows[i-1],rows[i];last=previous['last'];first=current['first']
                equal=first[0]==last[0] and first[3]==last[3]
                adjacent=previous['tail'] and last[2]+1<len(keys) and first[0]==keys[last[2]+1]
                if equal or adjacent:
                    previous['last']=current['last'];previous['tail']=current['tail'];rows.pop(i);merged+=1
            result=[[r['first'][0],r['last'][1 if r['tail'] else 0]]for r in rows]
            assert result==query['ranges'],(result,query['ranges'])
            query_count+=1
    assert query_count>1000 and merged>0 and tail_differences>0 and branch_cases>=64
    print(f'PASS independent recursive21B forward-path oracle/{query_count} callbacks/{merged} exact reverse merges/{tail_differences} duplicate-tail key differences/{branch_cases} interacting deferred graphs')
