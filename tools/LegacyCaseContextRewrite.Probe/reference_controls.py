"""Independent context suffix remapping from prepared raw table controls."""
import ast,base64,json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=3 or Path(sys.argv[2]).suffix.lower()!='.json':raise ValueError('preparation-controls.json output.json')
rng=random.Random(0x232f10);out=[]
for item in json.loads(Path(sys.argv[1]).read_text(encoding='utf8')):
 a=item['alphabet'];tables={r[0]:r for r in item['rows']};ranges=a['expected'];paths=[]
 def cased(label):return ranges[0]<=label<=ranges[1] or ranges[2]<=label<=ranges[3]
 choices=[r[0] for r in item['rows'] if r[0]<65507]+[60002,65535]
 for n in range(24):
  count=[0,1,2,31,32,33][n%6];symbols=[rng.choice(choices) for _ in range(count)];mode=n%3;alternate=n%2==1
  expected=[];refusal=False
  if count>32:expected=None
  else:
   for label in symbols:
    if label in tables and label<65507 and cased(label):
     forms=tables[label][4 if alternate else 3]
     if forms is None:refusal=True;break
     expected.append(forms[mode])
    else:expected.append(label)
  paths.append(dict(symbols=symbols,mode=mode,alternate=alternate,expected=expected,refusal=refusal,root=n%4==0))
 out.append(dict(alphabet=a,paths=paths))
#A separate mode1-only owner has cased ranges but no initialized mode0 row.
tree=ast.parse((Path(__file__).parents[1]/'LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text(encoding='utf8'));ns={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'byte-reader','exec'),ns)
a=out[0]['alphabet'];raw=base64.b64decode(a['payload']);m=ns['read'](raw);at=m['positions'][0]-4
raw=raw[:at]+struct.pack('<I',2)+bytes(m['count'])+bytes([1])
out.append(dict(alphabet=dict(payload=base64.b64encode(raw).decode(),version=260),paths=[dict(symbols=[0],mode=0,alternate=False,expected=[],refusal=True,root=False)]))
Path(sys.argv[2]).write_text(json.dumps(out,separators=(',',':'))+'\n',encoding='utf8');print(f'Generated {len(out)} alphabet owners/{sum(len(v["paths"]) for v in out)} context paths.')
