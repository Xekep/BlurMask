"""Independent original source-count/reference-controller branch controls."""
import argparse,json,random
p=argparse.ArgumentParser();p.add_argument('output');args=p.parse_args();r=random.Random(227)
cases=[]
for index in range(1024):
    counts=[r.randrange(40) for _ in range(r.randrange(25))]
    cached=r.choice([-10,0,0,0,r.randrange(1,500)])
    old=r.randrange(-(1<<50),1<<50);cumulative=r.randrange(-60000000,60000001)
    cost=r.choice([0x4000000,0x4000000+cumulative,r.randrange(-60000000,60000001)])
    first=r.randrange(2);flag99=r.randrange(2);flagA8=r.randrange(2);kind=r.choice([0x135,0x134,0x136,-1,0,2048])
    count=cached if cached>0 else sum(counts)
    present=cost not in [0x4000000,0x4000000+cumulative]
    reference=cost if present and first and not flag99 and not flagA8 else old
    cases.append(dict(counts=counts,cached=cached,old=old,cost=cost,cumulative=cumulative,first=first,flag99=flag99,flagA8=flagA8,kind=kind,count=count,reference=reference,multiplier=5 if kind==0x135 else 1))
with open(args.output,'w',encoding='utf8')as file:json.dump(cases,file)
print(f'Wrote {len(cases)} original controls.')
