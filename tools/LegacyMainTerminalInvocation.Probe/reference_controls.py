"""Independent first fresh2271F0 invocation/no-active-rescue control data."""
import hashlib,json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise ValueError('AK output.json')
ak=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
primary=struct.unpack_from('<H',ak,len(ak)-186+64+13*2)[0]
assert primary==125
rng=random.Random(0x2271f0);tests=[]
def s32(x):return (x+2**31)%2**32-2**31
for index in range(512):
    size=rng.randrange(primary+36);end=size if index%3==0 else rng.randrange(size+1)
    cumulative=rng.randrange(-2**31,2**31);rows=[];winner=None
    for ordinal in range(size):
        row=bytearray(rng.randbytes(88));row[4]=rng.randrange(2);row[5]=rng.randrange(8)
        struct.pack_into('<Q',row,8,0xdeadbeef) #24B8false makes this foreignfield unused.
        cost=rng.choice([rng.randrange(-200000,200000),0x4000000,0x3ffffff,-2**31,2**31-1])
        row14=rng.randrange(-2**31,2**31);struct.pack_into('<ii',row,16,cost,row14)
        rows.append(row.hex())
        if ordinal>=end or cost>=0x4000000:continue
        #Fresh9Cfalse means Filter4false undereitherV240 outcome.
        if ordinal<primary or row[5]&4:continue
        if winner is None or cost<winner[1]:
            winner=[ordinal,cost,s32(cost+cumulative),row14,struct.unpack_from('<H',row,34)[0],0,0]
    tests.append(dict(rows=rows,end=end,cumulative=cumulative,winner=winner))
p=Path(sys.argv[2]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_bytes((json.dumps(dict(primary=primary,tests=tests),separators=(',',':'))+'\n').encode('utf-8'))
print(f'Generated{len(tests)} independent fresh SDK first-invocation scans; actual primary{primary}')
