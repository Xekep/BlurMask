"""Independent table-domain oracle for2324E0, from raw stored maps."""
import ast,base64,json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=3 or Path(sys.argv[2]).suffix.lower()!='.json':raise ValueError('range-controls.json output.json')
tree=ast.parse((Path(__file__).parents[1]/'LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text(encoding='utf8'));ns={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'byte-reader','exec'),ns)
out=[]; inputs=json.loads(Path(sys.argv[1]).read_text(encoding='utf8'))
#Independent mode maps expose the mode1 repair and mode2 substitution branches.
base=inputs[0];payload=base64.b64decode(base['payload']);m=ns['read'](payload)
flag_at=m['positions'][0]-4;forms_at=flag_at-m['forms']*6;rng=random.Random(0x2324e0)
for n in range(128):
 triplets=[]
 for i in range(m['forms']):
  triplets.append([rng.choice([65535,rng.randrange(m['count'])]) for _ in range(3)])
 maps=[[rng.randrange(m['forms']) for _ in range(m['count'])] for _ in range(3)]
 raw=payload[:forms_at]+b''.join(struct.pack('<3H',*v) for v in triplets)+struct.pack('<I',7)+b''.join(bytes(v) for v in maps)+bytes([1])
 inputs.append(dict(name='preparedIndependentModes'+str(n),payload=base64.b64encode(raw).decode(),version=260,expected=[0,65535,65535,65535]))
for a in inputs:
 if a['expected'] is None:continue
 raw=base64.b64decode(a['payload']);meta=ns['read'](raw);maps={};triplets=[]
 if meta['positions']:
  at=meta['positions'][0]-4;flags=struct.unpack_from('<I',raw,at)[0];start=at-meta['forms']*6
  triplets=[list(struct.unpack_from('<3H',raw,start+i*6)) for i in range(meta['forms'])];values=iter(meta['maps'])
  for m in range(3):
   if flags&(1<<m):maps[m]=next(values)
 def forms(label,mode):
  m=maps.get(mode,maps.get(0))
  if m is None or m[label]<0:return None
  v=triplets[m[label]].copy()
  if label not in v:
   for i in (0,1):
    if v[i]==65535:v[i]=label;break
  return v
 #Use explicit equality decision matrix, not C# control flow.
 def category(label,v):
  low,up,title=v
  return 0 if low==65535 else (0 if up==65535 or title==65535 else 1+int(up!=title)) if label==low else 3+int(title!=65535 and label!=title)
 labels={v:k for k,v in meta['text'].items()};boundary=a['expected'][3] if a['expected'][3]!=65535 else a['expected'][1];rows=[]
 for signed in range(-29,meta['count']):
  label=signed&65535;p=q=0;f=g=None;zero=forms(label,0) if signed>=0 and label<=boundary else None
  if zero is not None:
   f=zero;p=category(label,zero);one=forms(label,1);two=forms(label,2);assert one is not None and two is not None
   g=one.copy()
   if label==one[0] and one[1]==65535 and one[2]!=65535:g=[one[0],one[2],65535];q=1
   elif one[0]==65535:q=0
   elif label==one[0]:q=category(label,one)
   else:q=3+int(label!=one[2] and zero[2]!=65535)
   if p==0:
    if two[0]==65535:q=0
    else:
     p=category(label,two)
     if p!=0:f=two
  elif label==65532 or label==labels.get('-'):p=q=6
  elif -9<=signed<=-5:p=q=5
  elif -29<=signed<=-22:p=q=7
  if label in [labels.get(t) for t in ',:;']:p=8
  if label in [labels.get(t) for t in '.!?']:p=9
  rows.append([label,p,q,f,g])
 out.append(dict(alphabet=a,rows=rows))
Path(sys.argv[2]).write_text(json.dumps(out,separators=(',',':'))+'\n',encoding='utf8');print(f'Generated {len(out)} alphabet tables/{sum(len(a["rows"]) for a in out)} extended symbols.')
