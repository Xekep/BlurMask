"""Independent full-domain scalar packing + pinned linear C21 intersection oracle."""
from pathlib import Path
import ast,hashlib,json,math,struct,sys,copy
D=Path(__file__).resolve().parent
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
source=(D/'reference_range_operations.py').read_bytes();assert hashlib.sha256(source).hexdigest()=='dc5e998a42e4dcedbfda714d97dd58d06136af608c5f4aa5bc03bf70b89fb750'
scope={'MASK':2**64-1};tree=ast.parse(source.decode('utf-8-sig'));exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)],type_ignores=[]),'pinned_linear_range_oracle','exec'),scope)
def pack(obj,x,mode,count):
 x=f(x);i=math.floor(x);remainder=f(x-i)
 if remainder==0 and i==count-1:q=255
 else:
  q=int(f(f(remainder*200)+.5))
  if q==200:i+=1;q=255 if i==count-1 else 0
 return obj<<32|i<<16|q<<8|mode

def normal(v):return v&~0xff00 if v>>8&255==255 else v

def project(counts,refs):
 out=[]
 for r in refs:
  assert all(type(v)is int for v in r)and len(r)==8 and r[0]==r[3]==0
  for obj in range(r[1],r[4]+1):
   start=f(r[2]-f(r[6]/200))if obj==r[1]else 0
   end=f(r[5]-f(r[7]/200))if obj==r[4]else counts[obj]-1
   a=pack(obj,start,3,counts[obj]);b=pack(obj,end,5,counts[obj]);fraction=b>>8&255
   same=(a>>32==b>>32 and a>>16&65535==b>>16&65535 and((a>>8&255)==fraction or(a>>8&255)==255 or fraction==255))
   #05AFA0 ordinary wrapper: original-end/FF/point0/degenerate closes; otherwise opens.
   originalend=obj==len(counts)-1 and end==counts[-1]-1
   closed=originalend or fraction==255 or((b>>16&65535)==0 and fraction==0)or same
   out.append([a,(b&~255)|(5 if closed else 1)])
 return out

def strict(a,b):
 assert type(a)is type(b)
 if type(a)is dict:
  assert set(a)==set(b)
  for k in b:strict(a[k],b[k])
 elif type(a)is list:
  assert len(a)==len(b)
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b

def expected(cases):
 rows=[]
 for c in cases['Cases']:
  counts=c['PointCounts'];word=project(counts,c['WordReferences']);glyph=project(counts,c['GlyphReferences']);root=[[pack(0,0,3,counts[0]),pack(len(counts)-1,counts[-1]-1,5,counts[-1])]]
  rows.append(dict(Id=c['Id'],OriginalRoot=root,WordClip=word,Projected=glyph,Clipped=scope['intersect'](glyph,word),ExactCancel=True,BadGroup=True,BadPoint=True))
 return dict(Scope='PREPARED_NEUTRAL_GEOMETRY_NOT_ACTUAL_EN_OWNER',Cases=rows)

def main():
 cases=json.loads(Path(sys.argv[1]).read_bytes());actual=json.loads(Path(sys.argv[2]).read_bytes());want=expected(cases);strict(actual,want);neg=[]
 for field in ['OriginalRoot','WordClip','Projected','Clipped','ExactCancel','BadGroup','BadPoint','Id']:
  x=copy.deepcopy(actual)
  if field in('ExactCancel','BadGroup','BadPoint'):x['Cases'][0][field]=False
  elif field=='Id':x['Cases'][0][field]='wrong'
  else:x['Cases'][0][field][0][0]^=1
  try:strict(x,want)
  except AssertionError:neg.append(field)
  else:raise AssertionError('Tamper accepted '+field)
 for name,change in [('bool position',lambda x:x['Cases'][0]['OriginalRoot'][0].__setitem__(0,True)),('omitted case',lambda x:x['Cases'].pop()),('extra channel',lambda x:x.update(Fake=True))]:
  x=copy.deepcopy(actual);change(x)
  try:strict(x,want)
  except AssertionError:neg.append(name)
  else:raise AssertionError(name)
 print(json.dumps(dict(Status='PURE_NEUTRAL_GEOMETRY_PASS',Cases=len(want['Cases']),ExactPreparedGuards=3*len(want['Cases']),TamperRefusals=len(neg),Negatives=neg,LiveEnglishOwnerExecuted=False,NNEvaluated=False)))
if __name__=='__main__':main()
