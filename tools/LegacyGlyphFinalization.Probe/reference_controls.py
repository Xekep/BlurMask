"""Independent byte-level2173E0/13AF30 oracle; no vendor code is executed."""
import json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('controls.json')
rng=random.Random(0x2173E0)
cases=[]
edges=[(256,0,0,0xffff),(257,0,0,0xffff),(255,0,0,42),(0,0,0,0xfff9),
       (-1,1,0,0xfff9),(-7935,1,0,0xfffa),(-7936,1,0,0xfffb),
       (-2147483648,1,0,0xfffc),(2147483647,0,0,42),(-1,0,0,42),
       (-4,1,255,0xfff9),(0,0,1,0xfff9)]
def w16(b,o,v):struct.pack_into('<H',b,o,v&65535)
def w32(b,o,v):struct.pack_into('<I',b,o,v&0xffffffff)
for i in range(512):
    cursor,flag,external,label=edges[i%len(edges)]
    if i>=len(edges):
        flag=rng.randrange(256) if flag else 0
        label=rng.choice([label,0xfff9,0xfffa,0xfffb,0xfffc,rng.randrange(65536)])
    success=i%17!=0
    labels=[rng.randrange(65536) for _ in range(256)]
    if cursor<0 or cursor>=256:labels[0]=label
    else:labels[cursor]=label
    before=bytearray(rng.randbytes(40));w32(before,0,123)
    other=bytearray(rng.randbytes(40));w32(other,0,124)
    row=bytearray(rng.randbytes(72));row[61]=rng.choice([0,1,255])
    a,b=rng.choice([0,0x80000000,0x7fc12345,0x7f800000,rng.getrandbits(32)]),rng.getrandbits(32)
    cost=rng.randrange(-2147483648,2147483648);pub=rng.randrange(65536)
    expected=bytearray(before);result=False;reject=False
    if success:
        if cursor<0 and flag==0:reject=True
        else:
            symbol=labels[0] if cursor<0 else labels[cursor] if cursor<256 else 65535
            count=(256-cursor)&0xffffffff;count=count if count<0x80000000 else count-0x100000000
            if external:kind=4
            elif count==0:kind=1;symbol=1
            elif 0xfff9<=symbol<=0xfffc:
                if count>=8192:kind=None
                else:kind=1;symbol=((((flag<<13)|(count&8191))<<2)|((symbol-1)&3))&65535
            else:kind=0
            if kind is not None:
                old=struct.unpack_from('<H',before,4)[0]
                w16(expected,4,((old&0xfff8)|kind)&0xfff7|(8 if row[61] else 0));w16(expected,8,symbol)
                w32(expected,12,a);w32(expected,16,b);w32(expected,20,cost);w16(expected,6,pub);result=True
    cases.append(dict(success=success,cursor=cursor,flag=flag,external=external,labels=labels if success else [],
        source=row.hex() if success else '',before=before.hex(),other=other.hex(),expected=expected.hex(),
        score0C=a,score10=b,cost=cost,pub=pub,result=result,reject=reject))
destination=Path(sys.argv[1]);destination.parent.mkdir(parents=True,exist_ok=True)
destination.write_bytes((json.dumps(cases,separators=(',',':'))+'\n').encode('utf-8'))
print('Generated512 independent full40 finalizations, cursor/wrap/flag/macro/scratch/rawscore controls')
