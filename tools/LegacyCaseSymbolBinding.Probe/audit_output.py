"""Raw AL/triplet oracle for owning case enumeration and permission→dictionary refresh."""
import ast,base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=4:raise ValueError('AK LK capture.json')
tree=ast.parse((Path(__file__).parents[1]/'LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text(encoding='utf8'));ns={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'raw-al-reader','exec'),ns)
ak=Path(sys.argv[1]).read_bytes()[99:];a=ns['read'](ak);l=ns['read'](Path(sys.argv[2]).read_bytes()[607074:]);got=json.loads(Path(sys.argv[3]).read_text(encoding='utf8'))
labels={v:k for k,v in a['text'].items()};control={'\0':65533,' ':65531,'\n':65529,'\u00a0':65532,'\ufffd':65535,'\U000f000c':65530}
primary=[65535 if l['reverse'][i]==65535 else control.get(l['text'][i],labels.get(l['text'][i],65535)) for i in range(l['count'])];assert got['map']==primary
at=a['positions'][0]-4;flags=struct.unpack_from('<I',ak,at)[0];forms=[list(struct.unpack_from('<3H',ak,at-a['forms']*6+i*6)) for i in range(a['forms'])];maps={};it=iter(a['maps'])
for mode in range(3):
 if flags&(1<<mode):maps[mode]=next(it)
def related(label,mode):
 m=maps.get(mode,maps.get(0))
 if m is None or m[label]<0:return None
 v=forms[m[label]].copy()
 if label not in v:
  for i in (0,1):
   if v[i]==65535:v[i]=label;break
 return v
callbacks=[]
for label in primary:
 first=related(label,0) if label<a['count'] else None
 if first is None:callbacks.append(label);continue
 for mode in range(3):
  v=related(label,mode)
  if v is not None:callbacks.extend(x for x in v if x!=65535)
assert got['callbacks']==callbacks;seed=bytearray(a['count'])
for v in callbacks:
 if 65507<=v<=65514:seed[:]=bytes([1])*len(seed)
 elif v<len(seed):seed[v]=1
assert base64.b64decode(got['seed'])==seed
def expand(p):
 v=bytearray(p)
 for low,upper,title in forms:
  for src,dst in [(upper,low),(title,low),(title,upper),(low,upper)]:
   if src!=65535 and dst!=65535:v[dst]|=v[src]
 return v
def working(p,flags):
 required=5 if flags&0x1e00==0x800 else 1
 return [65535 if label<a['count'] and p[label]&required!=required else label for label in primary]
for row in got['refresh']:
 p=expand(base64.b64decode(row['input']));assert row['working']==working(p,row['flags']) and row['preserved']==primary
header=struct.unpack_from('<8H',ak);reserved=list(header[4:7])+[labels.get(t,65535) for t in ['\U000f0000','\u200c','\u200d']]
for row in got['produced']:
 if not all(g['ok'] for g in row['groups']):assert not row['ok'] and row['working']==[] and not base64.b64decode(row['permissions']);continue
 permission=bytearray(a['count']);special=False
 for label in reserved:
  if label!=65535:permission[label]=3
 for g in row['groups']:
  if g['recomputed'] and g['kind']==1:special=g['present'] and g['flags']&7!=0
  for i,cost in enumerate(g['costs']):permission[i]|=(1 if special else 3) if cost<0x4000000 else 0;permission[i]|=4 if cost<int(g['scale']*40000) else 0
 permission=expand(permission);assert row['ok'] and base64.b64decode(row['permissions'])==permission and row['working']==working(permission,0xc0a)
print(f'PASS independent {len(callbacks)} callbacks, {len(got["refresh"])} ordered refreshes/{len(got["produced"])} acoustic permission paths, actual AL/LK/header ownership.')
