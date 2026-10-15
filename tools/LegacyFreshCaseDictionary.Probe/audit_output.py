"""Reuse untouched independent case expansion oracle; audit this freshfalse policy separately."""
import ast,base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=5:raise ValueError('AK LK case-preparation-controls capture')
ak,lk,reference,capture=map(Path,sys.argv[1:]);root=Path(__file__).resolve().parents[2]
#Execute exactly the older oracle's setup and normal-record loop, stopping
#BEFORE its separate512 preparedmetadata cases (ours has256 freshC4false).
old=root/'tools/LegacyCaseQuery.Probe/audit_actual.py';tree=ast.parse(old.read_text(encoding='utf8'));body=[]
for node in tree.body:
 if isinstance(node,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='categories' for t in node.targets):break
 body.append(node)
original_argv=sys.argv;sys.argv=[str(old),str(reference),str(capture)];ns={'__file__':str(old)}
try:exec(compile(ast.Module(body=body,type_ignores=[]),'unchanged-independent-case-normal-oracle','exec'),ns)
finally:sys.argv=original_argv
got=json.loads(capture.read_text(encoding='utf8'));classes=ns['classes'];categories=[9,11,1,4,4,4,4,4,5,5,5,10,1,12,4,6,6,6,5,5,5,2,2,2,3,3,3,5,0,0,0,0]
assert len(got['metadata'])==256
for row in got['metadata']:
 assert not row['ignored'];kind=classes[row['label']][1];expected=7 if row['start'] and kind==8 else 8 if kind==9 else categories[row['state']]
 value=bytes.fromhex(row['bytes']);assert struct.unpack_from('<i',value,8)[0]==expected and value[:4]==bytes(4) and value[12:]==bytes([7,0,0,0])
reader=ast.parse((root/'tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text(encoding='utf8'));rawns={}
exec(compile(ast.Module(body=[n for n in reader.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'raw-al-reference','exec'),rawns)
raw=ak.read_bytes()[99:];a=rawns['read'](raw);l=rawns['read'](lk.read_bytes()[607074:]);labels={s:i for i,s in a['text'].items()}
control={'\0':65533,' ':65531,'\n':65529,'\u00a0':65532,'\ufffd':65535,'\U000f000c':65530}
primary=[65535 if l['reverse'][i]==65535 else control.get(l['text'][i],labels.get(l['text'][i],65535)) for i in range(l['count'])]
permission=bytearray(a['count']);header=struct.unpack_from('<8H',raw);reserved=list(header[4:7])+[labels.get(t,65535) for t in ['\U000f0000','\u200c','\u200d']]
for label in reserved:
 if label!=65535:permission[label]=3
assert got['permissionScale']==1
for i,cost in enumerate(got['permissionCosts'][:a['count']]):permission[i]|=(3 if cost<0x4000000 else 0)|(4 if cost<40000 else 0)
at=a['positions'][0]-4;forms=[struct.unpack_from('<3H',raw,at-a['forms']*6+i*6) for i in range(a['forms'])]
for lower,upper,title in forms:
 for src,dst in [(upper,lower),(title,lower),(title,upper),(lower,upper)]:
  if src!=65535 and dst!=65535:permission[dst]|=permission[src]
assert base64.b64decode(got['permissions'])==permission
assert got['workingMap']==[65535 if value<a['count'] and not permission[value]&1 else value for value in primary]
assert got['firstGroups']==got['mainGroups']==10 and got['firstSelectedCost']==5399
assert sum(base64.b64decode(got['mask']))==145
assert len(got['restarts'])==32
for tag,result in got['restarts']:assert result==1|(1<<32)|(12<<40)|(tag&(0x7c<<56))
print(f'PASS independent {ns["total"]} actualnormal case records/256freshC4false metadata/32freshrestarts/actualNNcache permission->same-session workingmap; geometry/winner excluded')
