"""Independent raw full-source AL/category reader; no model/runtime imports."""
from pathlib import Path
import hashlib,struct

def read_profile(model):
    d=Path(model).read_bytes();sha=hashlib.sha256(d).hexdigest();assert sha=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
    at=8843+99;assert struct.unpack_from('<IH',d,at-6)==(0x2b,0x104)
    header=list(struct.unpack_from('<8H',d,at));assert header==[181,178,65535,180,184,182,183,217];at+=16
    def u16():
        nonlocal at
        x=struct.unpack_from('<H',d,at)[0];at+=2;return x
    n=u16();tables=[]
    for inc in [32,64,1]:
        table=[u16() for _ in range(n)];tables.append(table)
        n=next(x for x in reversed(table) if x!=65535)+inc
    spans=[u16() for _ in range(n+1)];entries=[]
    for _ in range(spans[-1]+1):entries.append(struct.unpack_from('<II',d,at));at+=8
    blob=d[at:at+entries[-1][0]]
    def lookup(text):
        data=text.encode('utf8');h=0x1505
        for byte in data:h=((h*33)^byte)&0xffffffff
        index=tables[0][(h>>16)&15]
        if index==65535:return 65535
        index=tables[1][index+((h>>6)&31)]
        if index==65535:return 65535
        index=tables[2][index+(h&63)]
        if index==65535:return 65535
        lo,hi=spans[index:index+2]
        # Native ordered bucket byte-compare; this oracle separately enumerates
        # the retained exact bucket and refuses duplicate encoded keys.
        matches=[label&65535 for (begin,label),(end,_) in zip(entries[lo:hi],entries[lo+1:hi+1],strict=True) if blob[begin:end]==data]
        assert len(matches)<=1;return matches[0] if matches else 65535
    labels={};digest=hashlib.sha256()
    for (begin,label),(end,_) in zip(entries,entries[1:]):
        text=blob[begin:end].decode('utf8');assert text not in labels;labels[text]=label&65535
        assert lookup(text)==label&65535
        digest.update(struct.pack('<II',len(blob[begin:end]),label&65535));digest.update(blob[begin:end])
    underscore=lookup('_');assert underscore==labels['_'] and underscore!=65535
    at=17017;assert struct.unpack_from('<HH',d,at)==(0x201,217);at+=4
    def ids():
        nonlocal at
        n=d[at];at=(at+4)&~3;x=list(struct.unpack_from('<'+'i'*n,d,at));at+=4*n;return x
    first=ids();firstmap=list(struct.unpack_from('<220b',d,at));at+=220
    geometry=ids();start=list(struct.unpack_from('<220b',d,at));at+=220;end=list(struct.unpack_from('<220b',d,at));at+=220
    prototypes=[]
    for _ in geometry:prototypes.append(list(struct.unpack_from('<fffB',d,at)));at+=13
    assert len(geometry)==24 and all(-1<=x<24 for x in start+end)
    exponent=struct.unpack_from('<f',d,373957)[0];assert exponent==.75
    out=dict(Status='PASS_RAW_FULL_AL_AND_SOURCE_CATEGORY_REFERENCE',SDKExecuted=False,NNEvaluated=False,TargetsRead=False,ResourceSHA256=sha,
             Source=[8843,373957],Selected249=[8827,373961],AlphabetKind=0x2b,AlphabetVersion=0x104,
             Header=header,StoredKeys=len(labels),LookupRoundtripSHA256=digest.hexdigest(),UnderscoreLabel=underscore,
             SourceScoreExponent=exponent,GeometryIds=geometry,StartMap=start,EndMap=end,Prototypes=prototypes,
             Scope='Actual stored-readable2B source data and pure UTF8 hash lookup; no source720/guide runtime or native ABI claim')
    out["Labels"]=labels;out["FirstIds"]=first;out["FirstMap"]=firstmap
    out["SourceFingerprint"]=hashlib.sha256(d[8843:373957]).hexdigest()
    return out
