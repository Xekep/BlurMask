"""Original numeric2257 matrix/exclusion machine; no vendor execution."""
import random,struct,json,base64,sys
from pathlib import Path
r=random.Random(22570)
f=lambda n:struct.unpack('<f',struct.pack('<f',n))[0]
wrap=lambda n:(n+2**31)%2**32-2**31
imp=0x4000000
def select(rows,alts,sources,o,excluded,cumulative):
    best=imp;selected=-1;kbest=0;identity=0;inc=imp;field=2**32-1;fallback=0
    for i,row in enumerate(rows):
        if row['cost']>=imp or o['require'] and not row['flag27']&1:continue
        header,penalty=sources[row['source']]
        value=f(f(penalty)*f(.8));penalty=int(f(value-f(.5) if value<=0 else value+f(.5))) if header&28 else 0
        inspect=i<o['primary'] or bool(row['flags']&4)
        if inspect and o['sentinels'] and row['context'] in (1,2,3):inspect=False
        for k,a in enumerate(alts[i]):
            cost,increment,ident,compact=a
            if cost>=imp:break
            fb=0
            if inspect:
                category=header&0x1e00
                if not o['filter'] or row['flags']&2 or row['field30']!=-1 or category==0xc00 and o['alternate'] or (category-0x200)&0xffffffff<0x801 or category in (0xe00,0x1000):cost=imp
                else:cost=wrap(cost+penalty);fb=~(row['flags']>>1)&1
            if cost<best and ident not in excluded:
                best=cost;selected=i;kbest=k;identity=ident;inc=increment;field=compact;fallback=fb
    if selected<0:return [-1,0,imp,imp,imp,2**32-1,0,o['flag'],0]
    flag=0 if rows[selected]['flag27']&1 else o['flag']
    return [selected,kbest,best,wrap(best+cumulative),inc,field,fallback,flag,identity]
out=[]
values=[-2**31,-100,0,50,imp-1,imp,imp+1,2**31-1]
for index in range(512):
    size=r.randrange(1,6);count=r.randrange(2,5)
    sources=[(category|r.choice([0,4,8,28]),r.choice([-2**31,-33333,0,1,2**31-1])) for category in [0,0x200,0xa00,0xc00,0xe00,0x1000,0x1400]]
    rows=[dict(cost=r.choice(values),source=r.randrange(len(sources)),flags=r.randrange(8),flag27=r.randrange(4),field30=r.choice([-1,0,500]),context=r.randrange(5)) for _ in range(size)]
    alts=[[[r.choice(values),r.randrange(-2**31,2**31),r.randrange(12),r.randrange(65536)] for k in range(count)] for _ in range(size)]
    o=dict(primary=r.randrange(size+1),sentinels=bool(r.randrange(2)),require=bool(r.randrange(2)),filter=bool(r.randrange(2)),alternate=bool(r.randrange(2)),flag=r.randrange(256))
    #Manual early cases distinguish root versus child controls and prove
    #first-impossible break/exclusion versus cheapest cost/tie behavior.
    if index<5:
        rows=[dict(cost=1,source=3,flags=0,flag27=0,field30=-1,context=index)]
        alts=[[[10,20,1,13],[5,30,2,14]]];size=1;count=2
        o=dict(primary=1,sentinels=True,require=False,filter=False,alternate=False,flag=7)
    excluded=r.sample(range(12),r.randrange(min(size,4)+1))
    cumulative=r.randrange(-2**31,2**31)
    expected=select(rows,alts,sources,o,excluded,cumulative)
    out.append(dict(rows=rows,alts=alts,sources=sources,options=o,excluded=excluded,cumulative=cumulative,expected=expected))
Path(sys.argv[1]).write_text(json.dumps(out)+'\n',encoding='utf-8')
print('Generated512 independent matrix/exclusion/filter/root-context/overflow controls.')
