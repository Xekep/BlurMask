"""Ordered raw-triplet byte permission oracle234840, not Unicode/form-getter repair."""
import ast,base64,json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=3 or Path(sys.argv[2]).suffix.lower()!='.json':raise ValueError('preparation-controls.json output.json')
tree=ast.parse((Path(__file__).parents[1]/'LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text(encoding='utf8'));ns={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'byte-reader','exec'),ns)
rng=random.Random(0x234840);out=[]
for item in json.loads(Path(sys.argv[1]).read_text(encoding='utf8')):
 a=item['alphabet'];raw=base64.b64decode(a['payload']);m=ns['read'](raw);triplets=[]
 if m['positions']:
  start=m['positions'][0]-4-m['forms']*6
  triplets=[struct.unpack_from('<3H',raw,start+i*6) for i in range(m['forms'])]
 masks=[bytes(m['count']),bytes([255])*m['count']]
 masks.extend(bytes(rng.randrange(256) for _ in range(m['count'])) for _ in range(16))
 for _ in range(8):
  values=bytearray(m['count']);values[rng.randrange(m['count'])]=1<<rng.randrange(8);masks.append(bytes(values))
 def expand(values,modern):
  v=bytearray(values)
  #Execute the ordered graph edge writes; repeated/chained rows observe earlier writes.
  for low,upper,title in triplets:
   edges=[(upper,low),(title,low),(title,upper)]+([(low,upper)] if modern else [])
   for source,target in edges:
    if source!=65535 and target!=65535:v[target]=v[target]|v[source]
  return bytes(v)
 def b64(v):return base64.b64encode(v).decode()
 out.append(dict(alphabet=a,rows=[[b64(v),b64(expand(v,False)),b64(expand(v,True))] for v in masks]))
Path(sys.argv[2]).write_text(json.dumps(out,separators=(',',':'))+'\n',encoding='utf8');print(f'Generated {len(out)} raw-triplet alphabets/{sum(len(v["rows"]) for v in out)} byte masks, TCM0/200.')
