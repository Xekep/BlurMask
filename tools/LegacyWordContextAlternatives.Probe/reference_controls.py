"""Independent functional path oracle and byte-slice root alternative writer."""
import json, random, struct, sys

# Coordinates are literal descending symbols extracted from the native data,
# expressed as a dictionary rather than sharing the managed lookup algorithm.
actions = {
    0xfffc: [-1, -2, -1, -2, 3, 3],
    0xfffb: [-1, -1, -1, -2, 3, 3],
    0xfffa: [-2, -2, -1, -2, 3, 3],
    0xfff9: [-1, -1, -1, -1, -1, -1],
    0xfff8: [3, 3, 3, -2, 3, 3],
    0xfff7: [3, 3, 3, -2, 3, 3],
}

def compose(prefix, suffix, mapped):
    path = list(prefix)
    calls = []
    for key in suffix:
        if not path:
            if mapped and 0xfff7 <= key <= 0xfffc:
                continue
            if mapped:
                calls.append(key)
                key ^= 0x55
            path.append(key)
        elif key != 0xfffd:
            if path[-1] in actions and key in actions:
                action = actions[path[-1]][0xfffc-key]
                if action == -1:
                    continue
                path.pop()
                path.append(key if action == -2 else (-action-4)&0xffff)
            else:
                if mapped:
                    calls.append(key)
                    key ^= 0x55
                path.append(key)
    return path, calls

rng = random.Random(0x1f7e30)
values = list(range(0xfff6, 0x10000)) + [0,1,2,22,70,140,178,179,180,181,182,191,192,255,256,65500]
cases=[]
for previous in actions:
    for current in actions:
        for mapped in (False,True):
            prefix=[0x41,previous];suffix=[current]
            expected,calls=compose(prefix,suffix,mapped)
            cases.append(dict(prefix=prefix,suffix=suffix,mapped=mapped,expected=expected,calls=calls))
while len(cases)<1024:
    prefix=[rng.choice(values) for _ in range(rng.randrange(0,5))]
    suffix=[rng.choice(values) for _ in range(rng.randrange(0,9))]
    mapped=bool(rng.getrandbits(1));expected,calls=compose(prefix,suffix,mapped)
    cases.append(dict(prefix=prefix,suffix=suffix,mapped=mapped,expected=expected,calls=calls))

roots=[]
for index in range(256):
    n=rng.randrange(1,5);pc=rng.choice([1,2,3,16]);sc=rng.choice([1,2,3,16])
    if pc==sc==1:pc=2
    node=rng.randrange(0,128) if index%4 else rng.choice([-128,-1,0,127])
    storage=bytearray(rng.randbytes((n+1)*128))
    ordinals=list(range(n));rng.shuffle(ordinals)
    if index%8==0 and n>1:ordinals[-1]=ordinals[0]
    suffixes=[]
    expectedMapped=[];expectedRaw=[]
    for i in range(n):
        o=i*128
        struct.pack_into('<i',storage,o+0x58,ordinals[i])
        storage[o+0x20]=node&255
        suffix=[rng.choice(values) for _ in range(rng.randrange(0,6))]
        suffixes.append(suffix)
        expectedMapped.append(compose([],suffix,True)[0])
        expectedRaw.append(compose([],suffix,False)[0])
    p=bytearray(rng.randbytes((n+1)*pc*40));s=bytearray(rng.randbytes((n+1)*sc*40))
    expectedP=bytearray(p);expectedS=bytearray(s)
    for i in range(n):
        slot=storage[i*128:(i+1)*128];ordinal=ordinals[i]
        for count,dst,primary in ((pc,expectedP,True),(sc,expectedS,False)):
            if count==1:continue
            offset=ordinal*count*40
            dst[offset:offset+8]=slot[0x10:0x18]
            dst[offset+8:offset+24]=bytes(16) # symbolic identities patched by probe
            dst[offset+24:offset+26]=slot[0x1e:0x20]
            dst[offset+26]=slot[0x20]
            dst[offset+30]=slot[0x26]
            dst[offset+31]=(dst[offset+31]&(0xc0 if primary else 0xe0)) | (slot[0x21]&31) | (32 if primary else 0)
            for k in range(1,count):
                at=offset+k*40
                struct.pack_into('<ii',dst,at,0x4000000,0x4000000)
                dst[at+30]=0
    roots.append(dict(count=n,node=node&0xffffffff,pc=pc,sc=sc,slots=storage.hex(),primary=p.hex(),secondary=s.hex(),expectedP=expectedP.hex(),expectedS=expectedS.hex(),suffixes=suffixes,mapped=expectedMapped,raw=expectedRaw))
with open(sys.argv[1], 'w', encoding='utf-8') as output:
    json.dump(dict(paths=cases,roots=roots),output,separators=(',',':'))
print(f'generated {len(cases)} independent path controls / {len(roots)} full-byte pool controls')
