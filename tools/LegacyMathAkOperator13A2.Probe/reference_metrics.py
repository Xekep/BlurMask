"""Numerical DATA only, no source/graph authority, C# or NN execution."""
import struct,math,json,sys
from pathlib import Path
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<I',struct.pack('<f',v))[0]
def metrics(row):
 row=[list(map(f,p))for p in row];minimum=row[0].copy();maximum=row[0].copy()
 for p in row[1:]:
  for j in [0,1]:
   if p[j]>=maximum[j]:maximum[j]=p[j]
   if not minimum[j]<=p[j]:minimum[j]=p[j]
 lengths=[math.sqrt(f(f(f(q[0]-p[0])**2)+f(f(q[1]-p[1])**2)))for p,q in zip(row,row[1:])]
 arc=sum(lengths)
 center=[f(sum(float(f(p[j]+q[j]))*w for p,q,w in zip(row,row[1:],lengths))*.5/arc)for j in [0,1]]if arc>0 else row[0].copy()
 return minimum+maximum+center

def controls():
 data=[('single',[[3,7]]),('two-identical',[[3,7],[3,7]]),('horizontal',[[0,0],[4,0]]),('unequal-edges',[[0,0],[1,0],[4,0]]),('right-angle',[[0,0],[4,0],[4,2]]),('signed-zero-ties',[[-0.,0.],[0.,-0.]]),('zigzag',[[0,0],[3,2],[-1,5],[7,-3]]),('large-finite',[[100000,200000],[100002,200003],[100010,200001]]),('subnormal-zero-square',[[0.,0.],[2**-149,0.]])]
 result=[dict(name=n,points=p,metric=metrics(p),metricBits=list(map(bits,metrics(p))))for n,p in data]
 by={x['name']:x for x in result};assert by['unequal-edges']['metric'][4]==2 and by['unequal-edges']['metric'][4]!=sum(p[0]for p in data[3][1])/3
 assert by['right-angle']['metric'][4:]==[f(16/6),f(2/6)]
 assert by['signed-zero-ties']['metricBits']==[0x80000000,0,0,0x80000000,0x80000000,0]
 assert by['subnormal-zero-square']['metric'][4:]==[0.,0.]
 return dict(cases=result,sourceAuthority=False,csharpExecution=False,networkForwards=0)
if __name__=='__main__':Path(sys.argv[1]).write_text(json.dumps(controls(),indent=2)+'\n',encoding='utf8')
