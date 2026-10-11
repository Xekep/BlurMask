"""Original packed-record/input-boundary conditional oracle, not search policy."""
import base64,json,random,struct,sys
rng=random.Random(0x204290)
first_ids=[0,0,0,0,0,0,0,0,0,2,2,4,0,3,1,2,4,0,4,0,0,4,1,3,0,0,0,4,3,0,3,0,0,6,5]
cases=[]
def i32(value):return ((value+2**31)%2**32)-2**31
for number in range(1024):
    n=rng.randint(1,8);groups=sum(max(0,n-size+1) for size in range(1,4))
    boundaries=[rng.choice([-1,-2,0,1,2,-2**31,2**31-1]) for _ in range(rng.randint(1,5))]
    descriptors=bytearray(rng.randbytes(len(boundaries)*0x58))
    for index,value in enumerate(boundaries):struct.pack_into('<i',descriptors,index*0x58+0x10,value)
    inputs=[rng.randrange(len(boundaries)) for _ in range(groups)]
    category=rng.randrange(35);mode=bool(rng.randrange(4)==0);path_type=rng.randrange(256)
    span=rng.randrange(-128,128);end=rng.randrange(-1,n+10)
    if number%5==0:span=rng.randrange(n);end=span
    if number%9==0:category=63;mode=True
    available=rng.randrange(n+1);lookup=bool(rng.randrange(4)!=0)
    if number%3==0 and number%9!=0:
        span=rng.randrange(16);end=span+rng.randrange(n+2);available=n;mode=False;path_type=rng.choice([8,12,16,20,24,28])
    record=bytearray(rng.randbytes(0x58));struct.pack_into('<H',record,0x1a,category|rng.randrange(1024)<<6);struct.pack_into('<b',record,0x20,span)
    calls=[];mapped=-1;start=previous=-1;delta=added=0
    if not mode and path_type&0x1c>7:
        mapped=first_ids[category];start=end-span;previous=start-1
        if previous>=0 and start<available and lookup:
            calls.append(previous);before=boundaries[inputs[previous]]
            if before!=-1:calls.append(start);delta=i32(boundaries[inputs[start]]-before)
        if mapped in (2,4) and (previous==-1 or delta!=0):added=8192
    cases.append(dict(n=n,descriptors=base64.b64encode(descriptors).decode(),boundaries=boundaries,inputs=inputs,
        record=base64.b64encode(record).decode(),end=end,available=available,mode=mode,pathType=path_type,lookup=lookup,
        expected=[mapped,start,previous,delta,added],calls=calls))
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump(cases,stream,separators=(',',':'))
print('Wrote',len(cases),'independent packed path/input-boundary controls')
