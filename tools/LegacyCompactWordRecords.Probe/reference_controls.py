"""Independent byte-addressed 206D30 publication controls; no vendor execution."""
import json, random, struct, sys
from pathlib import Path
rng=random.Random(0x206d30)
tests=[]
for case in range(2048):
    count=1+case%8; start=case%4
    storage=bytearray(rng.randbytes((start+count+2)*40))
    beam=bytearray(rng.randbytes(88))
    node=rng.choice([0,1,65535,65536,0xffffffff])
    struct.pack_into('<H',beam,24, node&65535 if case&1 else rng.randrange(65536))
    beam[5]=rng.choice([0,127,128,255]);beam[26]=rng.randrange(256)
    specials=[60001,60002,60003,60004,60005]
    last=rng.choice(specials+[0xffea,0xffeb,0xffff,65,0,300])
    symbols=[[rng.randrange(65536),rng.randrange(256),rng.getrandbits(64)] for _ in range(count)]
    symbols[-1][0]=last
    category=(beam[26]&63) if case&2 else rng.randrange(256)
    options=[rng.getrandbits(64),rng.randrange(-2**31,2**31),rng.randrange(-2**31,2**31),
             rng.randrange(-2**31,2**31),rng.randrange(65536),rng.randrange(-2**31,2**31),
             bool(case&4),bool(case&8),bool(case&16),rng.randrange(256),*specials,node]
    before=storage.hex();calls=0
    if last in specials[:2]: eligible=True
    elif struct.unpack_from('<H',beam,24)[0]!=node and beam[5]>=128: eligible=False
    elif last in specials[2:] or last>=0xffeb: eligible=False
    else:
        calls=1;eligible=(beam[26]&63)!=category
    parent=options[1]
    for i,(symbol,kind,coordinates) in enumerate(symbols):
        offset=(start+i)*40
        struct.pack_into('<QQiiiHH',storage,offset,options[0],coordinates,parent,
                         options[2],options[3],symbol,options[4] if i==count-1 else 65535)
        bits=[False]*8
        for bit in range(3):bits[bit]=bool(kind&(1<<bit))
        if i==count-1:bits[3],bits[4],bits[6],bits[7]=options[7],options[8],eligible,options[6]
        storage[offset+32]=sum(1<<bit for bit in range(8) if bits[bit])
        storage[offset+33]=options[9]
        struct.pack_into('<i',storage,offset+36,options[5] if i==count-1 else -1)
        parent=start+i
    tests.append(dict(before=before,beam=beam.hex(),start=start,symbols=symbols,options=options,
                      category=category,calls=calls,eligible=eligible,after=storage.hex()))
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_bytes((json.dumps(tests,separators=(',',':'))+'\n').encode('ascii'))
print('Generated2048 independent whole-storage publication cases')
