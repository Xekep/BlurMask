"""Independent prepared2257C0 single-alternative/no-rescue branch oracle."""
import json, random, struct, sys
from pathlib import Path
rng=random.Random(0x2257c0); impossible=0x4000000
def s32(v):return (v+2**31)%2**32-2**31
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def penalty(v):
    product=f(f(v)*f(.8));return int(f(product+(.5 if product>0 else -.5)))
tests=[]
for case in range(1024):
    sources=[[rng.randrange(0,16)*0x200 | rng.randrange(8)*4,rng.randrange(-2**31,2**31)] for _ in range(4)]
    primary=rng.randrange(0,20); sentinel=bool(case&1); filter4=bool(case&2); alternate=bool(case&4); require=bool(case&8)
    result_flag=rng.randrange(256); cumulative=rng.randrange(-2**31,2**31)
    rows=[];contexts=[];winner=None
    for ordinal in range(case%33):
        row=bytearray(rng.randbytes(88));row[4]=rng.randrange(4);row[5]=rng.randrange(8);row[39]=rng.randrange(4)
        struct.pack_into('<Q',row,8,0)
        ctx=rng.choice([None,0xfff9,0xfffb,0xfffa,65]);contexts.append(ctx)
        cost=rng.choice([rng.randrange(-100000,100000),impossible,impossible-1,-2**31,2**31-1])
        language=rng.randrange(-2**31,2**31);field30=rng.choice([-1,0,1])
        struct.pack_into('<i',row,16,cost);struct.pack_into('<i',row,20,language);struct.pack_into('<i',row,48,field30)
        rows.append(row.hex())
        if cost>=impossible or require and row[39]&1==0:continue
        header,raw=sources[row[4]];flag=0
        gated=ordinal<primary or row[5]&4!=0
        if sentinel and ctx in (0xfff9,0xfffb):gated=False
        if gated:
            #Native unsigned category interval; formulate as a finite set here.
            disallowed=(header&0x1e00) in (0x200,0x400,0x600,0x800,0xa00,0xe00,0x1000)
            if not filter4 or row[5]&2 or field30!=-1 or header&0x1e00==0xc00 and alternate or disallowed:
                continue
            cost=s32(cost+(penalty(raw) if header&0x1c else 0));flag=1
        if cost<impossible and (winner is None or cost<winner[1]):
            winner=[ordinal,cost,s32(cost+cumulative),language,struct.unpack_from('<H',row,34)[0],flag,0 if row[39]&1 else result_flag]
    tests.append(dict(sources=sources,primary=primary,sentinel=sentinel,filter4=filter4,alternate=alternate,require=require,
                      resultFlag=result_flag,cumulative=cumulative,rows=rows,contexts=contexts,winner=winner))
if len(sys.argv)!=2:raise ValueError('output.json')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(tests,separators=(',',':'))+'\n',encoding='utf-8')
print(f'Generated {len(tests)} independent terminal scans with native source/category/float32/wrap behavior')
