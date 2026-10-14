"""Independent boundary override and word scalar byte controls."""
import json,pathlib,random,struct,sys
r=random.Random(1416);out=[]
def f32(v):return struct.unpack('<f',struct.pack('<f',v))[0]
for i in range(512):
 first,last=r.randrange(-100000,100000),r.randrange(-100000,100000)
 a=-1 if i%4<2 else r.randrange(-100000,100000);b=-1 if i%4%2==0 else r.randrange(-100000,100000)
 scalars=[r.uniform(-20,20),1.,r.uniform(-5,5),r.uniform(-5,5)]
 raw=b''.join(struct.pack('<f',v) for v in scalars)
 total=r.randrange(-67108864,67108864);acoustic=r.randrange(-67108864,67108864);nodes=r.randrange(1,4097);kind=309 if i%2 else 311
 native=[f32(f32(-5 if kind==309 else -1)*f32(total)),1.,f32(f32(total)/f32(nodes*8192)),f32(f32(acoustic)/8192)]
 native[0]=f32(native[0]/8192)
 out.append(dict(first=first,last=last,a=a,b=b,expectedFirst=first if a==-1 else a,expectedLast=last if b==-1 else b,scalars=list(struct.unpack('<4I',raw)),
  total=total,acoustic=acoustic,nodes=nodes,kind=kind,nativeWordScalars=list(struct.unpack('<4I',b''.join(struct.pack('<f',v) for v in native)))))
p=pathlib.Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(out,separators=(',',':'))+'\n')
print('512 independent native span-override/scalar ownership controls')
