"""Independent raw XY/header/coverage/matrix oracle; never loads vendor code."""
import argparse,base64,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('capture',type=Path);a=p.parse_args();data=json.loads(a.capture.read_bytes())
def raw(v):return base64.b64decode(v)
def f(v):return struct.unpack('<f',struct.pack('<I',v))[0]
def f32(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def quantize(v):return int(f32(f32(v*200)+.5))
def descriptor(d):return [*d[:4],f(d[4]),f(d[5]),*d[6:]]
def covers(v,q):
 v,q=descriptor(v),descriptor(q)
 if v[0]!=q[0] or v[1]!=q[1] or v[8] or q[8]:return False
 vf,vl,vx,vy,vc,vd=v[2:8];qf,ql,qx,qy,qc,qd=q[2:8]
 if ql==-1:ql=q[1]-1
 def endpoint(n):return 2147483647 if n==-1 else n
 if qf==vf and quantize(qx)==quantize(vx) and qc==vc and endpoint(ql)==endpoint(vl) and quantize(qy)==quantize(vy) and qd==vd:return True
 # Compare the two open/closed intervals lexicographically, preserving
 # native special treatment of the raw first/last point.
 left=(vf<qf) or (vf==qf and vx<=qx and (vx!=qx or not qc or vc or qf!=0))
 right=(endpoint(ql)<=endpoint(vl)) and (vl!=ql or (qy<=vy and (qy!=vy or vd or not qd or ql!=v[1]-1)))
 return left and right
layout=struct.pack('<9I',2,0,0,8,0x10420,4,0,8,0x10420)
identity=struct.pack('<6d',1,0,0,0,1,0)
objects=fragments=matched=unmatched=discriminators=0
for sample in data['actual']:
 for obj in sample['objects']:
  assert obj['type']==0xc49 and raw(obj['layout'])==layout
  assert raw(obj['xy'])==b''.join(struct.pack('<2I',*point) for point in obj['points'])
  assert raw(obj['viewTail'])==struct.pack('<II',obj['source'],0)
  d=descriptor(obj['view']);assert d==[obj['source'],len(obj['points']),0,len(obj['points'])-1,0.,0.,True,True,False]
  objects+=1
 assert sample['transforms']==0
 for q,view in zip(sample['fragments'],sample['matches']):assert covers(sample['objects'][view]['view'],q)
 assert len(sample['fragments'])==len(sample['matches'])
 assert raw(sample['matrices'])==identity*len(sample['fragments'])
 fragments+=len(sample['fragments'])
for row in data['preparedCoverage']:
 expected=covers(row['view'],row['request']);assert expected==row['matched'],row
 if row['view'][4]==struct.unpack('<I',struct.pack('<f',.005))[0] and row['request'][4]==0x3b23d70a:
  assert expected and int(f(row['view'][4])*200+.5)!=int(f(row['request'][4])*200+.5)
  discriminators+=1
 matched+=expected;unmatched+=not expected
assert objects==data['actualObjects'] and fragments==data['actualFragments']
assert len(data['actual'])==4 and len(data['preparedCoverage'])==4096 and matched>0 and unmatched>0 and data['guards']==16 and discriminators==256
print(f'PASS4 actual raw roots/{objects} exactXY+36Bheaders+8Bviewtails/{fragments} exact48Bidentity matrices/4096 independent coverage({matched} accepted,{unmatched} refused)/256F32 discriminators/16 guards; no pointer-byte fabrication.')
