"""Own cache vectors and independent sorted-set oracle. No vendor parameters."""
import base64,json,math,random,struct,sys
from pathlib import Path
R=random.Random(0x1bb020);IMP=0x4000000;COUNT=192
texts=['own'+str(i) for i in range(COUNT)]
for i,s in {4:'o',5:'O',6:'0',180:'-',181:'_',182:'\u30fc',183:'\u4e00',184:'\U000f0001',185:'\U000f0002',186:'\U000f0003',187:'\ufffd',188:"'",189:'"',190:'.',191:'\u00ad'}.items():texts[i]=s
header=[179,140,65535,178,182,180,181,COUNT]
def alphabet():
 b=bytearray(struct.pack('<8HHH',*header,1,0))
 for n in [32,64]:b.extend(struct.pack('<'+'H'*n,*([65535]*(n-1)+[0])))
 b.extend(struct.pack('<HH',0,COUNT));offset=0
 for i,s in enumerate(texts):b.extend(struct.pack('<II',offset,i));offset+=len(s.encode())
 b.extend(struct.pack('<II',offset,65535));b.extend(''.join(texts).encode());b.extend(struct.pack('<'+'H'*COUNT,*range(COUNT)));b.extend(bytes(3));return b
def rounded(x):return int(x+(.5 if x>0 else -.5))
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
cases=[]
# Unique costs let a simple sorted-set oracle independently verify heap selection.
# Symbol prototypes and copied quote buffers stay disabled in these randomized cases.
for n in range(768):
 costs=R.sample(range(1,200000),180);costs[140]=costs[178]=costs[179]=IMP
 mask=[int(i<140 or 140<i<178) for i in range(COUNT)]
 for i in range(180):
  if R.randrange(8)==0:mask[i]=0
 prune=bool(n%2);threshold=R.randrange(100001);minimum=R.randrange(1,8);cap=R.randrange(minimum,25)
 current=[IMP]*(COUNT+3)
 for i,v in enumerate(costs):current[i]=v if mask[i] else IMP
 current[COUNT]=IMP+8192
 threshold1=min(threshold,85173);selection=[]
 for gate in ([threshold1,170346,IMP] if prune else [IMP]):
  selection=sorted((v,i) for i,v in enumerate(current) if i!=COUNT and v<gate)[:cap if prune else 150]
  if len(selection)>=minimum or not prune:break
 if prune and len(selection)>=minimum:
  current=[IMP]*(COUNT+3);current[COUNT]=IMP+8192
  for v,i in selection:current[i]=v
 candidates=[[i,v] for v,i in selection if i<COUNT]
 cases.append(dict(raw=costs,mask=mask,prune=prune,threshold=threshold,minimum=minimum,cap=cap,costs=current,candidates=candidates))
profiles=[]
for n in range(32):
 b=bytearray(struct.pack('<IIII',0x0101fe90,0,0x21d,0x1100000));b.extend(bytes([255,255])+struct.pack('<H',100)+bytes([1]));b.extend(struct.pack('<HHI',0xfea0,0x106,3)+bytes(3));b.extend(bytes(24)+bytes([1])+struct.pack('<H',30));b.extend(struct.pack('<9f',.15,.30,.40,.50,.95,25,.20,1,.1));b.extend(bytes([1,0])+bytes(44)+bytes(6)+bytes(16)+bytes(2))
 for slot in range(7):b.extend(bytes(4)+struct.pack('<f',0)+bytes(3))
 tail=len(b);b.extend(bytes([0])+struct.pack('<f',1)+bytes([1])+struct.pack('<ff',1.2,1.8));lists=n%4;b.extend(struct.pack('<H',lists))
 for j in range(lists):b.extend(struct.pack('<H',j)+bytes(j*2))
 b.extend(struct.pack('<6f',.99,.25,.50,.9,.8,.75)+bytes([0])+struct.pack('<ff',1,1.5)+bytes([0])+struct.pack('<h',0))
 b.extend(struct.pack('<iHHiiiBf',85173,1,10,20480,24576,32768,1,0));b.extend(b'FOLLOWING_FIELDS');struct.pack_into('<I',b,4,len(b)-8)
 profiles.append(dict(bytes=base64.b64encode(b).decode(),tail=tail))
directions=[]
for n in range(512):
 points=[[R.randrange(-30000,30001),R.randrange(-30000,30001)] for _ in range(2+n%6)]
 if n<4:points=[[[0,0],[100,0]],[[0,0],[-100,0]],[[0,0],[0,100]],[[0,0],[0,0]]][n]
 pos=neg=0.0
 for (ax,ay),(bx,by) in zip(points,points[1:]):
  dx=bx-ax;dy=by-ay;length=f(math.hypot(dx,dy))
  if dx>0:pos=f(pos+length)
  elif dx<0:neg=f(neg+length)
  else:pos=f(pos+f(length*.5));neg=f(neg+f(length*.5))
 total=f(pos+neg)
 if abs(total)<=max(f(abs(total)*f(2.3841858e-7)),f(1.1920929e-7)):pos=neg=.5
 else:pos=f(pos/total);neg=f(neg/total)
 base=R.randrange(100000)
 values=[IMP if w<=f(.1) else min(IMP,base+rounded(-math.log(f(w/f(.9)))*8192)) for w in [neg,pos]]
 directions.append(dict(points=points,base=base,expected=values))
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(dict(alphabet=base64.b64encode(alphabet()).decode(),cases=cases,profiles=profiles,directions=directions),separators=(',',':')),encoding='utf8')
print('Generated768 independent shortlist/cache,512 hypot direction and32 serialized profile controls.')
