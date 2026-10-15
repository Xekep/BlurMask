"""Independent0FE2A0 final-unigram-level/float32 oracle; data only."""
import hashlib,json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise ValueError('LK output.json')
lk=Path(sys.argv[1]).read_bytes()
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
d=lk[2513123:4565174];p=90
def take(fmt):
    global p
    values=struct.unpack_from('<'+fmt,d,p);p+=struct.calcsize('<'+fmt)
    return values[0] if len(values)==1 else list(values)
order,flags,words,classes,uc,bc=take('6I')
assert (order,flags,uc)==(2,3,classes)
packed,count=take('2I');bits=take('B');assert packed==0x1000000 and count==words
p+=((words*bits+31)//32)*4+words
wordlevels=take('i'*take('I'))
p+=classes+classes*classes
nu,nb=take('2I');uni=take('i'*nu);bi=take('i'*nb)
assert take('2I')==[0,0]
tail=p
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def convert(level,slope,offset,scale):
    a=f(f(scale)*f(slope));b=f(f(scale)*f(offset))
    return int(f(f(f(f(level)*f(1/8192))*a)-b)*8192)
#Multiplication by8192 is exact here; the prior subtract rounds tobinary32.
profiles=[]
for i in range(4):
    profiles.append([struct.unpack_from('<f',d,tail+64+i*8)[0],
                     struct.unpack_from('<f',d,tail+i*8)[0]])
rng=random.Random(0xfe2a0);controls=[]
for i in range(2048):
    slope=f(rng.uniform(-3,3));offset=f(rng.uniform(-2,2));scale=f(rng.uniform(.01,6))
    controls.append([slope,offset,scale,convert(uni[-1],slope,offset,scale)])
actual=[]
for i,(slope,offset) in enumerate(profiles):
    for scale in [.125,.5,1,1.25,2,4]:actual.append([i,scale,convert(uni[-1],slope,offset,scale)])
prefix=lk[606964:4565174];first=struct.unpack_from('<I',prefix,92)[0]
scalar_at=100 if first&0xff000000==0 else 108
resource_scale=struct.unpack_from('<f',prefix,scalar_at)[0]
slope,offset=profiles[2];penalty=convert(uni[-1],slope,offset,resource_scale)
scaled=f(f(penalty)*f(.8));rounded=int(f(scaled+(.5 if scaled>0 else -.5)))
out=dict(finalUni=uni[-1],initialUni=uni[0],finalBi=bi[-1],finalWord=wordlevels[-1],
         counts=[nu,nb],controls=controls,actual=actual,resourceScale=resource_scale,
         penalty=penalty,terminalDelta=rounded)
path=Path(sys.argv[2]);path.parent.mkdir(parents=True,exist_ok=True)
path.write_bytes((json.dumps(out,separators=(',',':'))+'\n').encode('utf-8'))
print(f'Generated{len(controls)} scalar/{len(actual)} actual profile controls; unigram{nu} final{uni[-1]}, actualscale{resource_scale}, penalty{penalty}')
