"""Independent linear normalization and decimal formatter; no DLL execution."""
import base64,importlib.util,json,random,sys
from pathlib import Path
path=Path(__file__).resolve().parents[1]/'LegacyRawInkRangeOperations.Probe/reference_controls.py'
spec=importlib.util.spec_from_file_location('range_oracle',path);ops=importlib.util.module_from_spec(spec);spec.loader.exec_module(ops)
def canonical(rows):
 for (_,last),(first,_) in zip(rows,rows[1:]):
  if last&255==4:break
  if first<=last or ops.cross(last,first):return False
 return True
def endpoint(x):
 f=x>>8&255
 if x>>32>0x7fffffff or 200<=f<255:raise ValueError('unsupported native digit-table domain')
 return str(x>>32)+':'+str(x>>16&65535)+('$'if f==255 else '.'+str(f*5).zfill(3).rstrip('0')if f else '')
def replay(rows,fail=-1,final=False):
 iscanonical=canonical(rows);normal=rows[:];attempts=0;capacity=0;out=b''
 def allocate(required):
  nonlocal attempts,capacity
  attempts+=1
  if attempts-1==fail:return False
  capacity=max(required,capacity*3//2);return True
 def result(ok):return dict(succeeded=ok,canonical=iscanonical,rows=normal,utf8=base64.b64encode(out).decode(),attempts=attempts,capacity=capacity)
 if not iscanonical:
  if not allocate(len(rows)):return result(False)
  normal=rows[:1]
  for row in rows[1:]:
   if row[0]&255!=4 and normal[0][0]&255!=4:normal=ops.union(normal,[row])
  capacity=0
 if not allocate(len(normal)*18+1):return result(False)
 for i,(a,b) in enumerate(normal):
  if i:
   if capacity<len(out)+2 and not allocate(len(out)+2):return result(False)
   out+=b'U'
  part=(('['if a&255==3 else ']')+endpoint(a)+','+endpoint(b)+(']'if b&255==5 else '[')).encode()
  if capacity<len(out)+len(part)+1 and not allocate(len(out)+len(part)+1):return result(False)
  out+=part
 return result(not final)
def make(output):
 rng=random.Random(0xc21180);cases=[]
 def p(s,n,f,m):return s<<32|n<<16|f<<8|m
 for i in range(2048):
  rows=[]
  for j in range(rng.randrange(0,9)):
   s=rng.choice([0,1,2,32767,2147483647]);n=rng.randrange(60000)
   a=p(s,n,rng.choice([0,1,2,20,25,100,199,255]),rng.choice([1,3,7]));b=p(s,n+rng.randrange(0,50),rng.choice([0,1,199,255]),rng.choice([1,5,7]))
   if a>b:a,b=b,a
   rows.append([a,b])
  if i%16==0:rows=[[4,4]]+rows
  elif i%16==1:rows=rows[:1]+[[4,4]]+rows[1:]
  if i%16==2:rows=[[p(0,0,0,3),p(0,4,255,5)],[p(1,0,0,3),p(1,8,255,5)]]
  if i<200:rows=[[p(0,0,i,3),p(0,1,i,5)]]
  fail=i%6-1 if i%3 else -1;final=i%17==0;flags=rng.choice([0,0x400000,0x80400000])
  cases.append(dict(rows=rows,fail=fail,final=final,flags=flags,expected=replay(rows,fail,final)))
 #Force an allocation failure after the U has already been written.
 a=[[p(2147483647,65534,199,3),p(2147483647,65535,255,5)],[p(2147483647,65535,255,7),p(2147483647,65535,255,7)]]
 for fail in range(5):cases.append(dict(rows=a,fail=fail,final=False,flags=0x400000,expected=replay(a,fail)))
 Path(output).write_bytes((json.dumps(dict(cases=cases),separators=(',',':'))+'\n').encode())
if __name__=='__main__':make(sys.argv[1])
