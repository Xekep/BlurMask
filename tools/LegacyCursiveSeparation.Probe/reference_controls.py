"""Own float32 prefix/suffix group separation controls; no vendor lookup values."""
import struct,random,math,json,sys,base64
from pathlib import Path
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
def native_round_float(value):
 bits=struct.unpack('<I',struct.pack('<f',value))[0];magnitude=bits&0x7fffffff
 if magnitude>=0x4b000000:return value
 if magnitude<0x3f800000:bits=(bits&0x80000000)|(0x3f800000 if magnitude>=0x3f000000 else 0)
 else:
  shift=150-(magnitude>>23);bits=((bits>>shift)+((bits>>(shift-1))&1))<<shift
 return struct.unpack('<f',struct.pack('<I',bits))[0]
r=random.Random(0x193860);cases=[]
table=json.loads(Path(sys.argv[2]).read_text()) if len(sys.argv)>2 else [f(-math.log1p(math.exp(-(-4+8*i/255)))) for i in range(256)]
for case in range(768):
 n=r.choice([1,2,3,4,7,8,9,10,16,64]);strokes=[]
 for i in range(n):
  center=r.uniform(-100,100);strokes.append([[f(center+r.uniform(-12,12)),f(r.uniform(-5,5))] for _ in range(r.randrange(1,5))])
 height=f(r.uniform(.1,100));offset=f(r.uniform(-1,1));slope=f(r.uniform(0,20));cap=r.choice([1000,8192,65536,0x4000000])
 lows=[min(p[0] for p in s) for s in strokes];highs=[max(p[0] for p in s) for s in strokes];costs=[]
 for boundary in range(1,n):
  left=[p[0] for s in strokes[:boundary] for p in s];right=[p[0] for s in strokes[boundary:] for p in s]
  gap=max(f(min(right)-max(left)),f(min(left)-max(right)))
  x=f(f(f(gap/height)-offset)*f(-slope))
  value=0 if x>4 else table[int(f(f(f(x+4)*255)+.5))>>3] if x>=-4 else x
  scaled=f(f(-value)*8192);rounded=int(native_round_float(scaled)) if n<9 else int(f(scaled+(.5 if scaled>0 else -.5)));costs.append(rounded)
 total=0
 for c in costs:total=total+c if n>=9 or f(f(total)+f(c))<0x4000000 else 0x4000000
 cases.append(dict(strokes=strokes,height=height,offset=offset,slope=slope,cap=cap,expected=min(total,cap)))
profiles=[]
for case in range(64):
 offset=case%4;node=bytearray(65);struct.pack_into('<IIII',node,0,0x0101fe90,0,0x297,0x1060000);node[64]=1
 node.extend(struct.pack('<HHB',0x201,1,1))
 while (offset+len(node))%4:node.append(255)
 node.extend(struct.pack('<i',0));node.extend(b'\0'*4);node.append(1)
 while (offset+len(node))%4:node.append(255)
 node.extend(struct.pack('<i',0));node.extend(b'\0'*8);node.extend(struct.pack('<fffB',1,2,3,0))
 fields=len(node);node.extend(b'\0'*214)
 gap=f(r.uniform(-2,2));slope=f(r.uniform(0,15));cap=r.randrange(0x4000001)
 struct.pack_into('<f',node,fields+25,gap);struct.pack_into('<f',node,fields+185,slope);struct.pack_into('<i',node,fields+198,cap)
 struct.pack_into('<I',node,4,len(node)-8)
 profiles.append(dict(bytes=base64.b64encode(bytes(offset)+node).decode(),offset=offset,fields=fields+offset,gap=gap,slope=slope,cap=cap))
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(dict(table=table,cases=cases,profiles=profiles),separators=(',',':')),encoding='utf8')
print('Generated',len(cases),'own separation controls.')
