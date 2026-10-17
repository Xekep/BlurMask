"""Independent275 weight/rank publication; exponential channels allow4ULP.

All other120-byte payload fields and the native binary-insertion index order
are checked exactly. This does not execute the vendor CRT or certify bitwise
equivalence of different system libm implementations.
"""
import base64,json,math,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
dec=base64.b64decode
def f(v):
    try:return struct.unpack('<f',struct.pack('<f',v))[0]
    except OverflowError:return math.copysign(math.inf,v)
def read(b,p):return struct.unpack_from('<f',b,p)[0]
def close(actual,expected,ulp=4):
    a=struct.unpack('<I',struct.pack('<f',actual))[0];e=struct.unpack('<I',struct.pack('<f',expected))[0]
    assert abs(a-e)<=ulp,(actual,expected,a,e)
cases=json.loads(Path(sys.argv[1]).read_text());checked=ties=zeros=singleton=large=0
for case in cases:
    words=list(map(dec,case['input']));output=list(map(dec,case['output']));n=len(words);order=[]
    for i,word in enumerate(words):
        left,right=0,i
        while left<right:
            mid=(left+right)//2;old=read(words[order[mid]],16);current=read(word,16)
            if old==current:ties+=1;break
            if current>old:right=mid
            else:left=mid+1
        else:mid=left
        order.insert(mid,i)
    assert order==case['order']and len(output)==n
    if not n:continue
    masses=[f(math.exp(f(read(words[i],16)-read(words[order[0]],16))))if f(read(words[i],16)-read(words[order[0]],16))>=-75 else 0.0 for i in order]
    #Normalize in the samefloat32 recurrence domain; no sum-of-all-softmax
    #substitution. The raw previous mass and published prior value differ.
    normalized=[]
    if n==1:normalized=[1.0];ranks=[1.0];singleton+=1
    else:
        previous=masses[0];normalized=[f(previous/f(previous+masses[1]))]
        for mass in masses[1:]:
            if mass==0:normalized.append(0.0)
            else:normalized.append(f(f(mass*normalized[-1])/previous))
            previous=mass
        minimum=min(read(word,28)for word in words)
        ranks=[f(math.exp(-f(read(words[i],28)-minimum)))for i in order]
    for pos,index in enumerate(order):
        #Byte-exact preservation includes full opaque/pointer residue.
        assert output[index][:16]==words[index][:16]and output[index][24:]==words[index][24:]
        close(read(output[index],16),normalized[pos]);close(read(output[index],20),ranks[pos])
        if normalized[pos]==0:zeros+=1
        checked+=1
    if n>60:large+=1
assert len(cases)==4096 and ties>1000 and zeros>1000 and large>1000 and singleton>1
print(f'PASS independent{len(cases)} span finalizers/{checked} full120B payloads/{ties} tie insertions/{zeros} zero masses/{large} native heap-size domains; channels4ULP, other bytes exact')
