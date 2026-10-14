"""Independent raw-triplet/mode-map callback ordering oracle."""
import ast,base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=3 or Path(sys.argv[2]).suffix.lower()!='.json':raise ValueError('range-controls.json output.json')
s=Path(__file__).parents[1]/'LegacyCaseAlphabetRanges.Probe/reference_controls.py'
tree=ast.parse(s.read_text(encoding='utf8'));ns={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'owned-byte-reader','exec'),ns)
result=[];callbacks=0
for item in json.loads(Path(sys.argv[1]).read_text(encoding='utf8')):
 if item['expected'] is None:continue #Native wrapper preparation refuses this metadata.
 raw=base64.b64decode(item['payload']);meta=ns['read'](raw);maps={};forms=[]
 if meta['positions']:
  at=meta['positions'][0]-4;flags=struct.unpack_from('<I',raw,at)[0]
  start=at-meta['forms']*6;forms=[struct.unpack_from('<3H',raw,start+i*6) for i in range(meta['forms'])]
  values=iter(meta['maps'])
  for mode in range(3):
   if flags&(1<<mode):maps[mode]=next(values)
 def related(cp,mode):
  index=maps.get(mode,maps.get(0))
  if index is None or index[cp]==-1:return None
  row=list(forms[index[cp]])
  if cp not in row:
   if row[0]==65535:row[0]=cp
   elif row[1]==65535:row[1]=cp
  return row
 rows=[]
 for cp in list(range(meta['count']))+[meta['count'],65535,65531]:
  first=related(cp,0) if cp<meta['count'] else None
  if first is None:out=[cp]
  else:
   out=[v for v in first if v!=65535]
   for mode in (1,2):
    row=related(cp,mode)
    if row is not None:out.extend(v for v in row if v!=65535)
  callbacks+=len(out);rows.append([cp,out])
 result.append(dict(alphabet=item,rows=rows))
Path(sys.argv[2]).write_text(json.dumps(result,separators=(',',':'))+'\n',encoding='utf8');print(f'Generated{len(result)} prepared/actual owned alphabets/{callbacks} ordered native callbacks; duplicates retained.')
