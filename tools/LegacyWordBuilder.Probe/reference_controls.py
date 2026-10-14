"""Independent tuples/references/header oracle; not a native DLL replay."""
import base64,json,random,struct,sys
from pathlib import Path

def main(path):
    rng=random.Random(0x13f520);cases=[]
    for case in range(512):
        count=rng.randrange(1,24); glyphs=[];references=[];spans=[];flags=[]
        storage=bytearray(rng.randbytes(120))
        for o in [32,88,104]:struct.pack_into('<Q',storage,o,0)
        for o in [96,112]:struct.pack_into('<i',storage,o,0)
        raw=storage[:]
        for i in range(count):
            #Distinct graph keys, sometimes primitive negative-arc keys.
            node=i+case*32;arc=-rng.randrange(1,8) if rng.randrange(5)==0 else rng.randrange(16)
            owner=(node&0xffffff)|((arc&0xffff)<<24)|((case%2)<<56)
            publication=rng.randrange(65536);span=0 if i in [0,count-1] and case%3==0 else rng.randrange(1,5)
            kind=1 if span!=1 else 0;symbol=(span<<2)|1 if kind==1 else rng.randrange(192)
            glyphs.append(dict(key=owner,descriptor=i+case*100,publication=publication,kind=kind,symbol=symbol));references.append(owner);spans.append(span);flags.append(publication)
        for o,v in [(8,0xffffffffffffffff)]:struct.pack_into('<Q',raw,o,v)
        for o,v in [(16,0x3f800000),(20,0x3f800000),(24,0),(28,0),(40,0),(48,0xfffe),(60,-1),(84,-1),(96,0),(112,0)]:struct.pack_into('<I',raw,o,v&0xffffffff)
        struct.pack_into('<H',raw,53,0);raw[55]=0
        struct.pack_into('<Q',raw,8,references[0])
        scalars=[rng.getrandbits(32) for _ in range(4)]
        for o,bits in zip([16,20,24,28],scalars):struct.pack_into('<I',raw,o,bits)
        firsthead=glyphs[0]; valid=[g for g in glyphs if ((g['key']>>24)&0xffff)<0x8000];lastvalid=valid[-1] if valid else firsthead
        def decode(g):
            n=g['key']&0xffffff;n=n if n<0x800000 else n-0x1000000
            a=(g['key']>>24)&0xffff;a=a if a<0x8000 else a-0x10000
            return n,a
        ln,la=decode(lastvalid);rn,ra=decode(firsthead)
        first=ln*100+la if la>=0 else ln+la;last=rn*100+ra+7 if ra>=0 else rn
        if la<0 and lastvalid['key']==firsthead['key']:first,last=last,first
        ownerc4=bool(case%2);aggregate=0;carry=ownerc4
        for flag in flags:
            aggregate|=flag
            if flag&0x800 and not flag&0x400:carry=False
        aggregate=(aggregate|0x400) if carry else (aggregate&0xfbff)
        published=raw[:];struct.pack_into('<Q',published,32,0);struct.pack_into('<I',published,40,count)
        cap=0
        for i in range(1,count+1):
            if cap<i:cap=max(i,int(cap*1.5))
        struct.pack_into('<I',published,44,cap);struct.pack_into('<H',published,50,aggregate)
        cases.append(dict(storage=base64.b64encode(storage).decode(),glyphs=glyphs,scalars=scalars,raw=base64.b64encode(raw).decode(),published=base64.b64encode(published).decode(),reading=list(reversed(references)),spanCount=sum(spans),first=first,last=last,ownerc4=ownerc4))
    out=Path(path);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(dict(cases=cases),separators=(',',':')))
    print('Generated512 independent WORD276/publisher controls including zero-span endpoints')
if __name__=='__main__':main(sys.argv[1])
