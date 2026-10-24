"""Original raw-bit controls and independent representative-list13A830 oracle."""
import argparse,base64,json,math,random,struct
from pathlib import Path

def b64(b):return base64.b64encode(b).decode('ascii')
def unpack_count(packed):
    value=(packed<<2)&65535
    return (value if value<32768 else value-65536)>>4
def make_row(rng,identity,score):
    row=bytearray(rng.randbytes(40));kind,symbol,publication=identity
    struct.pack_into('<H',row,4,(rng.randrange(8192)<<3)|kind)
    struct.pack_into('<H',row,6,(publication<<8)|rng.randrange(256))
    struct.pack_into('<H',row,8,symbol);struct.pack_into('<I',row,12,score)
    return row

def generate():
    rng=random.Random(0x13a830);controls=[]
    scores=[0,0x80000000,0x3f000000,0x3f800000,0xbf800000,0x7f800000,0xff800000,
            0x7fc00001,0xffc00100,0x7f800001,1,0x80000001,0x7f7fffff]
    def append(rows,marker,capacity,available,flags=0,packed_override=None):
        packed=(len(rows)<<2)|flags if packed_override is None else packed_override
        key=rng.randrange(1<<24)|(rng.randrange(1<<16)<<24)|(rng.randrange(2)<<56)
        controls.append(dict(key=key,packed=packed,owner=b64(b''.join(rows)),marker=marker,
            initialCapacity=capacity,allocation=b64(rng.randbytes(available*4))))
    # All raw comparison classes in both directions, duplicate and distinct.
    for a in scores:
        for b in scores:
            for duplicate in [False,True]:
                ids=[(0,1,3),(0,1,3) if duplicate else (0,2,3)]
                rows=[make_row(rng,ids[0],a),make_row(rng,ids[1],b)]
                for marker in [0,1,None]:append(rows,marker,0,4)
    # Different descriptor IDs and low publication bytes do NOT prevent dedup;
    # descriptor high bits, kind and publication high byte DO distinguish rows.
    for index in range(1024):
        count=rng.randrange(0,97);ids=[(rng.randrange(5),rng.randrange(8),rng.randrange(4)) for _ in range(12)]
        rows=[make_row(rng,rng.choice(ids),rng.choice(scores) if index%3 else rng.getrandbits(32)) for _ in range(count)]
        available=rng.choice([0,1,2,3,6,13,64,128]);capacity=rng.randrange(available+1)
        marker=rng.choice([None,0,max(0,count-1),rng.randrange(count+5),0xffffffff,0xffff8000])
        append(rows,marker,capacity,available,rng.choice([0,1,2,3])|rng.choice([0,0x4000,0x8000,0xc000]))
    # Capacity/refusal boundaries, unchanged unused capacity and max owner size.
    for count in [0,1,2,3,4,5,6,7,9,10,15,16,63,64,256,2040]:
        rows=[make_row(rng,(i%5,i,0),(0x3f000000+i)&0xffffffff) for i in range(count)]
        for available in sorted({0,max(0,count-1),count,count+100}):
            append(rows,count//2,min(available,3),available,0xc003)
    # Signed negative native count takes the empty-loop success branch while
    # its unrelated prepared row-capacity bytes remain untouched.
    for packed in [0x2000,0x3fff,0x6003,0xffff]:
        append([make_row(rng,(0,4,0),0x3f800000)],4,3,8,packed_override=packed)
    append([make_row(rng,(i%8,i,0),0x3f800000) for i in range(2040)],2039,0,4096)
    append([make_row(rng,(kind,1,3),0x3f800000) for kind in range(8)],7,0,16)
    return controls

def oracle(control):
    owner=base64.b64decode(control['owner']);allocation=bytearray(base64.b64decode(control['allocation']))
    n=unpack_count(control['packed']);capacity=control['initialCapacity'];original=control['marker'];marked=original if original is not None else 0
    # Use symbolic identities + representative objects rather than C# storage
    # shifts. Bytes are committed to the emulated allocation only after the
    # independent list update, keeping the untouched tail intact.
    identity_to_rep={};order=[];success=True
    def identity(i):
        row=owner[i*40:(i+1)*40];kind,pub,label=struct.unpack_from('<HHH',row,4)
        return kind&7,label,pub>>8
    def score(i):return struct.unpack_from('<f',owner,i*40+12)[0]
    for current in range(max(0,n)):
        ident=identity(current);previous=identity_to_rep.get(ident)
        if previous is None:
            target=next((i for i,other in enumerate(order) if score(current)>score(other)),len(order))
            if len(order)==capacity:
                requested=max(len(order)+1,int(capacity*1.5))
                if requested>len(allocation)//4:success=False;break
                capacity=requested
            order.insert(target,current);identity_to_rep[ident]=current
        elif score(current)>score(previous):
            old_position=order.index(previous)
            target=next((i for i,other in enumerate(order[:old_position]) if score(current)>score(other)),old_position)
            order.pop(old_position);order.insert(target,current);identity_to_rep[ident]=current
            if previous==marked:marked=current
        else:
            if current==original:marked=previous
        for position,value in enumerate(order):struct.pack_into('<I',allocation,position*4,value)
    marker=original
    if success and original is not None:marker=order.index(marked) if marked in order else 0xffffffff
    return dict(succeeded=success,count=len(order),capacity=capacity,nativeOwnerCount=n,marker=marker,
        storage=b64(allocation[:capacity*4]),rows=b64(allocation[:len(order)*4]),owner=b64(owner))

def main():
    parser=argparse.ArgumentParser();parser.add_argument('controls',type=Path);parser.add_argument('--audit',type=Path);args=parser.parse_args()
    if args.audit is None:
        controls=generate();args.controls.parent.mkdir(parents=True,exist_ok=True)
        args.controls.write_bytes((json.dumps(dict(controls=controls),separators=(',',':'))+'\n').encode())
        print(f'Generated {len(controls)} original raw-bit owner controls; no transcription or native execution.')
    else:
        controls=json.loads(args.controls.read_bytes())['controls'];actual=json.loads(args.audit.read_bytes())
        assert len(actual['results'])==len(controls)
        for i,(control,result) in enumerate(zip(controls,actual['results'])):
            expected=oracle(control);assert result==expected,(i,result,expected)
        assert actual['guards']==7
        failed=sum(not x['succeeded'] for x in actual['results']);rows=sum(x['count'] for x in actual['results'])
        print(f'PASS independent raw-float/identity-list oracle: {len(controls)} owners/{rows} selected rows/{failed} allocation refusals/7 guards, full capacity and source bytes.')
if __name__=='__main__':main()
