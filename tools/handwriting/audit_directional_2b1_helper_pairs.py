"""STATIC raw PE helper-body/landmark-ABI proof. Never loads vendor binaries.
Body pins accompany manual supported-subset review, not formal equivalence.
"""
import argparse,hashlib,importlib.util,itertools,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();proof=json.loads(Path(__file__).with_name('directional-2b1-helper-pairs-pins.json').read_bytes());b=(a.sdk/proof['binary']).read_bytes();sha=lambda x:hashlib.sha256(x).hexdigest();assert sha(b)==proof['sha256'];pe=struct.unpack_from('<I',b,60)[0];assert b[pe:pe+4]==b'PE\0\0';o=pe+24;assert struct.unpack_from('<H',b,o)[0]==0x20b;base=struct.unpack_from('<Q',b,o+24)[0];table=o+struct.unpack_from('<H',b,pe+20)[0];sections=[]
for i in range(struct.unpack_from('<H',b,pe+6)[0]):
 q=table+i*40;vs,rv,n,raw=struct.unpack_from('<IIII',b,q+8);sections.append((rv,n,raw))
def read(va,n):
 for rv,size,offset in sections:
  if rv<=va-base and va-base+n<=rv+size:return b[offset+va-base-rv:offset+va-base-rv+n]
 raise AssertionError((hex(va),n))
for pieces in proof['regions'].values():
 for r in pieces:
  lo=int(r['start'],16);hi=int(r['endExclusive'],16);assert hi-lo==r['bytes']and sha(read(lo,hi-lo))==r['sha256']
for r in proof['landmarkInstructions']:assert read(int(r['at'],16),len(bytes.fromhex(r['bytes'])))==bytes.fromhex(r['bytes'])
assert {r['name']for r in proof['pairedHelpers']}=={'landmarks','intersections','global','temporalMembership','spatialMembership','temporalStats','spatialStats'}
assert proof['selectedPreparedPointCap']==255 and proof['nativeClassValues']==[0,1,2,3]
# Raw BC280 loads point1Y before incrementing pointer to point2. Generic B6D20
# has only its point1X load at the corresponding location.
assert read(0x1800bc42e,4)==bytes.fromhex('498d5804')
assert read(0x1800bc4da,8)==bytes.fromhex('0fbf43024883c304')
assert read(0x1800bc4f7,2)==bytes.fromhex('3bc5')
assert read(0x1800b6f70,11)==bytes.fromhex('440fbf0f41ffc74883c704')
assert read(0x1800b6f9a,3)==bytes.fromhex('443bc5')
root=Path(__file__).resolve().parents[2];path=root/'tools/LegacyDirectional2B1.Probe/reference_landmarks.py';spec=importlib.util.spec_from_file_location('bc280_reference',path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
# Independent direct three-point controls exercise every Y order/equality.
# Initial scan and endpoint behavior are tested, not recognition or fonts.
checks=0;different=0
for ys in itertools.product(range(5),repeat=3):
 points=[[0,ys[0]],[1,ys[1]],[2,ys[2]]];actual=m.reference([points])['flags'];flags=[65,0,130];direction=next((1 if z>x else -1 for x,z in zip(ys,ys[1:])if x!=z),0)
 if direction:flags[0]|=16 if direction>0 else 8
 now=(ys[2]>ys[1])-(ys[2]<ys[1])
 if now:
  if direction==-now:flags[1]|=16 if now>0 else 8
  direction=now
 if direction:flags[-1]|=8 if direction>0 else 16
 assert actual==flags;checks+=1
 oldnow=(ys[2]>ys[0])-(ys[2]<ys[0]);different+=now!=oldnow
assert m.reference([[[0,0],[1,2],[2,1]]])['flags']==[81,8,146]
r=dict(status='PASS',nativeFunctions=len(proof['regions']),bodyRegions=sum(map(len,proof['regions'].values())),pairedHelperReviews=7,landmarkABIInstructions=len(proof['landmarkInstructions']),independentThreePointOrders=checks,discriminatingAxisOrders=different,nativeExecuted=False,scope='Raw body/instruction pins and literal BC280 reference controls; paired helper semantic equivalence is manual and limited to explicitly admitted <=255 samples.')
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r))
