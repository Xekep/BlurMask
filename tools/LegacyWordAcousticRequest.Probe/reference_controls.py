"""Independent1fd240 byte writer +152790/1c0fd0 signed32 oracle."""
import json,random,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
RNG=random.Random(0x1c0fd0)
IMP=0x4000000
def wrap(x):return(x+0x80000000)%0x100000000-0x80000000
def put(b,at,fmt,value):struct.pack_into('<'+fmt,b,at,value)
data=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
def categories(offset):
    count=struct.unpack_from('<H',data,offset+2)[0]+3;p=offset+4
    def ids():
        nonlocal p
        n=data[p];p=(p+4)&~3;a=struct.unpack_from('<'+'i'*n,data,p);p+=4*n;return a
    first=ids();fm=struct.unpack_from('<'+'b'*count,data,p);p+=count
    geometry=ids();start=struct.unpack_from('<'+'b'*count,data,p);p+=count
    end=struct.unpack_from('<'+'b'*count,data,p);out=[]
    for a,b,c in zip(fm,start,end):
        for x,y in[(a,b),(a,c),(first.index(0),b),(first.index(0),c)]:
            if x>=0 and y>=0 and(first[x],y)not in out:out.append((first[x],y))
    return out
cases=[];different=0;wrapped=0
for i in range(1024):
    offset=[11571,188222][i%2];category=i%35;geometry=categories(offset)[category][1]
    symbol=i%195;bypass=[179,180,181,182,183]
    thresholds=[10,20,30];source_kind=[0,1,2,3,6,7,8,15][i%8];transition_kind=(i//8)%8
    cls=[RNG.randrange(-100,100)for _ in range(195)]
    cls[symbol]=[9,10,11,19,20,21,29,30,31,IMP,IMP+1,-2147483648,2147483647][i%13]
    geom=[(j*37-400)*(1 if i%3 else -1)for j in range(35)]
    before=[0,100,-100,int(2**31-1),-2147483648][i%5]
    node=i%97;arc=i%65536;span=i%31;tag=RNG.getrandbits(64);length=i%256;flags=transition_kind|(i%32<<3)
    header=(source_kind<<9)|RNG.getrandbits(32)&~0x1e00;permission=(i*17)%256
    residue=bytearray(RNG.randbytes(80));packet=bytearray(residue)
    for at,v in[(0,node),(4,0xffffffff),(8,0xffffffff),(20,0xffffffff),(24,0xffffffff),(32,span),(48,0),(52,IMP),(64,source_kind),(68,0)]:put(packet,at,'I',v)
    for at,v in[(12,arc),(14,65535),(16,65535),(36,symbol),(38,65535)]:put(packet,at,'H',v)
    packet[18]=geometry;packet[19]=255;packet[28]=flags&7;packet[56]=length;packet[57]=0;packet[58]=permission
    put(packet,40,'Q',0x1122334455667788);packet[72]=(tag>>56)&1;packet[73]=0
    initial=packet.hex();suppression_enabled=i%3==0;suppressed=i%7==0
    lookup=not(suppression_enabled and suppressed)
    if not lookup:after=IMP;ok=False
    elif cls[symbol]==IMP or(source_kind in[1,2,3]and cls[symbol]>thresholds[source_kind-1]):
        after=IMP;ok=False;put(packet,48,'i',IMP)
    else:
        gcost=0 if symbol in bypass else geom[geometry]
        unwrapped=gcost+before+cls[symbol];after=wrap(unwrapped);wrapped+=after!=unwrapped
        ok=after<IMP;put(packet,48,'i',wrap(after-before)if ok else IMP)
        if symbol not in bypass and geometry!=category:
            wrong=wrap(geom[category]+before+cls[symbol]);different+=wrong!=after
    cases.append(dict(offset=offset,category=category,symbol=symbol,node=node,arc=arc,span=span,tag=tag,length=length,flags=flags,
                      header=header,permission=permission,before=before,residue=residue.hex(),initial=initial,expected=packet.hex(),
                      classes=cls,geometry=geom,bypass=bypass,thresholds=thresholds,suppressionEnabled=suppression_enabled,suppressed=suppressed,
                      after=after,accepted=ok,lookup=lookup))
assert different>100 and wrapped>100
Path(sys.argv[1]).write_text(json.dumps(dict(cases=cases,differentIndexCases=different,wrappedCases=wrapped),separators=(',',':')),encoding='utf-8')
print(f'generated 1024 full80 requests/scoring cases; {different} geometry/composite mismatches; {wrapped} signed32 wraps')
