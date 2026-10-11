"""Original exhaustive stable-interleaving oracle for multiple deferred seeds."""
import itertools,json,sys
cases=[]
for total in range(2,11):
    for deferred_count in range(1,total):
        ordinary_count=total-deferred_count
        for deferred in itertools.combinations(range(1,total),deferred_count):
            ordinary=[i for i in range(total) if i not in deferred]
            paths=[]
            for positions in itertools.combinations(range(total),deferred_count):
                if any(not d-2<=p<=d for d,p in zip(deferred,positions)):continue
                source_o=iter(ordinary);source_d=iter(deferred)
                paths.append([next(source_d) if i in positions else next(source_o) for i in range(total)])
            raw=[[[i*100,10],[i*100+20,90]] for i in range(ordinary_count)]
            for order,d in enumerate(deferred):
                preceding=d-order-1
                x=preceding*100+(9 if preceding<ordinary_count-1 else -10)
                raw.append([[x,10],[x,90]])
            case=dict(deferred=list(deferred),strokes=raw,paths=sorted(paths))
            if len(cases)%13==0:
                partitions=set()
                for path in paths:
                    def partition(offset,prefix):
                        if offset==total:partitions.add('|'.join(prefix));return
                        for size in range(1,min(3,total-offset)+1):
                            partition(offset+size,prefix+[','.join(map(str,sorted(path[offset:offset+size])))])
                    partition(0,[])
                case['partitions']=sorted(partitions)
            cases.append(case)
# One ordinary group crosses many consecutive deferred placements; native
# per-group reference counters must saturate at200 rather than overflow.
total=256
paths=[]
for ordinary_position in range(total):
    path=list(range(1,total));path.insert(ordinary_position,0);paths.append(path)
cases.append(dict(deferred=list(range(1,total)),strokes=[[[0,10],[20,90]]]+[[[-10,10],[-10,90]] for _ in range(total-1)],paths=sorted(paths)))
with open(sys.argv[1],'w',encoding='utf8') as f:json.dump(cases,f,separators=(',',':'))
print('Wrote',len(cases),'complete independent deferred interleaving references,',sum(len(c['paths']) for c in cases),'paths')
