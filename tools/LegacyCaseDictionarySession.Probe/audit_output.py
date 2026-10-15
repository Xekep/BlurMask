"""Independent old case oracle plus raw-AL permissions and aggregate restart routing."""
import ast,base64,json,struct,subprocess,sys
from pathlib import Path
if len(sys.argv)!=5:raise ValueError('AK LK case-preparation-controls capture')
ak,lk,reference,capture=map(Path,sys.argv[1:]);root=Path(__file__).resolve().parents[2]
subprocess.run([sys.executable,str(root/'tools/LegacyCaseQuery.Probe/audit_actual.py'),str(reference),str(capture)],check=True)
got=json.loads(capture.read_text(encoding='utf-8'))
assert len(got['restart'])==512
for incoming,actual in got['restart']:
 assert incoming>>32&255==1
 if incoming>>40&4:
  expected=1|(12<<40)|(incoming&(0x7c<<56))|(1<<32)
 else:expected=8<<40
 assert actual==expected,(incoming,actual,expected)
tree=ast.parse((root/'tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text(encoding='utf-8'));ns={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'independent-raw-al','exec'),ns)
raw=ak.read_bytes()[99:];a=ns['read'](raw);at=a['positions'][0]-4
triplets=[struct.unpack_from('<3H',raw,at-a['forms']*6+i*6) for i in range(a['forms'])]
baseline=bytes.fromhex(got['source']);assert len(got['refresh'])==3
for sample in got['refresh']:
 permission=bytearray(base64.b64decode(sample['input']))
 for low,upper,title in triplets:
  for src,dst in [(upper,low),(title,low),(title,upper),(low,upper)]:
   if src!=65535 and dst!=65535:permission[dst]|=permission[src]
 expected=[]
 for at in range(0,len(baseline),32):
  row=baseline[at:at+32];label=struct.unpack_from('<H',row,20)[0]
  if label>=a['count'] or permission[label]&1:expected.append(row)
 assert bytes.fromhex(sample['filteredRaw'])==b''.join(expected)
print('PASS independent selected1 restart/case bit routing and3 permission refreshes tosame raw dictionary; no inferred source configuration')
