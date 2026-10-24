"""Sequential FIRST resource readset. Pure bytes, no model execution."""
from pathlib import Path
import struct,json,hashlib,sys
H=lambda b:hashlib.sha256(b).hexdigest()
def decode(b):
 assert H(b)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
 p=7481;trace=[]
 def rd(name,t,n=1):
  nonlocal p
  a=p;v=struct.unpack_from('<'+t*n,b,p);p+=struct.calcsize('<'+t*n);v=v[0]if len(v)==1 else list(v);trace.append(dict(name=name,offset=a,end=p,value=v));return v
 source={k:rd('source'+k,t)for k,t in [('10','B'),('11','B'),('12','H'),('14','B'),('40','H'),('48','B')]}
 assert source=={'10':1,'11':1,'12':8192,'14':0,'40':0,'48':1}
 record={'00':rd('record00','B')};assert record['00']==0
 for k,t in [('14','H'),('18','I'),('1c','H'),('1e','H'),('20','H'),('22','H'),('28','H'),('2c','I'),('30','I'),('34','I'),('38','I'),('3c','I'),('40','I'),('44','I')]:record[k]=rd('record'+k,t)
 for k,arrays in [('48',1),('58',2),('70',2)]:
  n=rd('record'+k+'-count','H')
  for i in range(arrays):rd('record'+k+'-values'+str(i),'h',n)if n else None
 for k,t in [('88','B'),('89','B'),('90','H'),('94','I'),('98','I'),('9c','H'),('9e','H')]:record[k]=rd('record'+k,t)
 assert rd('recordoptional','B')==0
 legacy=rd('recorda8','B');assert legacy==0
 slots=[]
 for i in range(7):
  start=p;s={'ordinal':i,'sourceOffset':hex(0xc0+64*i),'start':p,'calibrated':rd(f'slot{i}.calibrated','B')}
  if s['calibrated']:
   n=rd(f'slot{i}.count','H');s['count']=n;s['arrays']=[]
   for k in ['minimum','maximum']:
    a=p;v=rd(f'slot{i}.{k}','h',n);s['arrays'].append(dict(name=k,offset=a,count=n,sha256=H(b[a:p]),values=v))
  s['includeCoordinatePrefix']=rd(f'slot{i}.18','B');s['includeBoundaryPair']=rd(f'slot{i}.19','B');s['mode']=rd(f'slot{i}.38','B')
  if s['mode']==0:s['modeExtra']=rd(f'slot{i}.3c','i')
  s['models']=[]
  for j in range(3):
   present=rd(f'slot{i}.model{j}.present','B')
   if present:
    a=p;magic,wire,size,kind,zero,version=struct.unpack_from('<HHIIHH',b,p)
    assert(magic,wire)==(0xfea0,0x107)
    p+=8+size;s['models'].append(dict(which=j,start=a,end=p,wireMagic=magic,wireVersion=wire,payloadHeaderWord=kind,sha256=H(b[a:p]),headerHex=b[a:a+48].hex()))
  s['end']=p;slots.append(s)
 assert [(s['ordinal'],s.get('count'),[(m['start'],m['end'])for m in s['models']])for s in slots if s['models']]==[(2,276,[(8702,177650)])]
 assert [s['includeBoundaryPair']for s in slots]==[0,0,1,0,0,0,0]
 assert record['14']==0 and record['18']==0 and record['28']==17 and record['30']==511 and record['88']==1 and record['89']==2
 # Return the unconsumed 13A5 trailer honestly; no fake whole-frame decode.
 assert p<177763
 return dict(scope='STATIC FIRST13A5 program and all seven complete slot encodings; source trailer not decoded',source=source,record=record,legacyCalibration=legacy,slots=slots,trace=trace,decodedEnd=p,remaining13A5Bytes=177763-p,dimensions=dict(bias=1,globalFeatures=17,localFeatures=256,baselineFeatures=0,boundaryFeatures=2,total=276),executedModels=0)
if __name__=='__main__':
 assert len(sys.argv)==3
 out=Path(sys.argv[2]);assert not out.exists()
 result=decode(Path(sys.argv[1]).read_bytes());out.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print('PASS FIRST program/seven slots/calibration276; no forward')
