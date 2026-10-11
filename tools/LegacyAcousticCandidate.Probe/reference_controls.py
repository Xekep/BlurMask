"""Own integer acoustic candidate and serialized v110 tail controls; no vendor weights."""
import base64,json,random,struct,sys
from pathlib import Path
r=random.Random(0x1c0fd0);cases=[];profiles=[]
for i in range(1024):
 thresholds=[r.randrange(0x4000001) for _ in range(3)];symbol=r.randrange(8);category=r.randrange(35);kind=r.randrange(8)
 cls=r.choice([0,0x4000000,*thresholds,r.randrange(0x4000001)]);geometry=r.randrange(0x4000001);previous=r.randrange(-100000,100001);limit=r.randrange(0x4000001)
 reason=1 if cls==0x4000000 else 2 if 1<=kind<=3 and cls>thresholds[kind-1] else 0
 applied=0 if reason or symbol in [1,2,3,4,5] else geometry
 total=0x4000000 if reason else cls+applied+previous
 accepted=not reason and total<limit
 if not reason and not accepted:reason=3
 cases.append(dict(thresholds=thresholds,symbol=symbol,category=category,kind=kind,cls=cls,geometry=geometry,previous=previous,limit=limit,applied=applied,total=total,accepted=bool(accepted),reason=reason))
for i in range(64):
 b=bytearray(struct.pack('<IIII',0x0101fe90,0,0x21d,0x1100000));b.extend(bytes([255,255])+struct.pack('<H',100)+bytes([1]));b.extend(struct.pack('<HHI',0xfea0,0x106,3)+bytes(3));b.extend(bytes(24)+bytes([1])+struct.pack('<H',30));b.extend(struct.pack('<9f',.15,.30,.40,.50,.95,25,.20,1,.1));b.extend(bytes([1,0])+bytes(44)+bytes(6)+bytes(16)+bytes(2));
 for slot in range(7):b.extend(bytes(4)+struct.pack('<f',0)+bytes(3))
 tail=len(b);b.extend(bytes(14));n=r.randrange(4);b.extend(struct.pack('<H',n))
 for j in range(n):m=r.randrange(6);b.extend(struct.pack('<H',m)+struct.pack('<'+'H'*m,*[r.randrange(256) for _ in range(m)]))
 b.extend(bytes(33)+bytes([0]));groups=r.randrange(4);b.extend(struct.pack('<h',groups))
 if groups:
  starts=[0]
  for j in range(groups):starts.append(starts[-1]+r.randrange(4))
  b.extend(struct.pack('<'+'H'*(groups+1),*starts));b.extend(bytes(starts[-1]*2)+bytes(4))
 b.extend(bytes(8));threshold_start=len(b);thresholds=[r.randrange(0x4000001) for _ in range(3)];b.extend(struct.pack('<iii',*thresholds));consumed=len(b);b.extend(b'FOLLOWING_FIELDS');struct.pack_into('<I',b,4,len(b)-8)
 profiles.append(dict(bytes=base64.b64encode(b).decode(),tail=tail,threshold_start=threshold_start,consumed=consumed,thresholds=thresholds))
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(dict(cases=cases,profiles=profiles),separators=(',',':')),encoding='utf8');print('Generated1024 candidate and64 serialized tail controls.')
