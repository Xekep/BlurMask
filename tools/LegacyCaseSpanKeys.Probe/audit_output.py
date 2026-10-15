"""Native query keys/epoch schedule plus independent raw NN→case dictionary oracle."""
import hashlib,json,runpy,struct,sys
from pathlib import Path
if len(sys.argv)!=5:raise ValueError('AK LK case-preparation-controls capture')
p=Path(sys.argv[4]);got=json.loads(p.read_text(encoding='utf8'))
assert got['epochs']==[-1,0,1,2,3]
seen=set()
for row in got['spans']:
 key=(row['node'],row['span'])
 assert key not in seen and row['parameter1c']==key[0] and row['parameter20']==key[1]
 seen.add(key)
assert len(seen)==10
assert got['auxiliary']==[[s['node'],4294967295,0,0] for s in got['spans']]
#Scan the actual packed dictionary rather than assuming an empty root proves
#all auxiliary terminal pages are empty. This is model-specific evidence.
lk=Path(sys.argv[2]).read_bytes()
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
at=609980;count=struct.unpack_from('<I',lk,at+20)[0];width=lk[at+36]
assert count==895571 and width==9
labels=lk[at+37:at+37+((count*width+31)//32)*4]
aux=got['auxiliaryMap'];mapped={i for i,value in enumerate(aux) if value!=65535}
assert len(aux)==struct.unpack_from('<H',lk,607074+14)[0]==139
assert {i:aux[i] for i in mapped}=={0:184,68:65531}
edges=terminals=0;eligible={}
for node in range(1,count):
 p,shift=divmod(node*width,8)
 packed=int.from_bytes(labels[p:p+3],'little')>>shift&((1<<width)-1)
 label=packed&((1<<(width-1))-1)
 if label in mapped:
  edges+=1;eligible[label]=eligible.get(label,0)+1
  terminals+=bool(packed&(1<<(width-1)))
assert terminals==0 and edges==17 and eligible=={0:1,68:16}
runpy.run_path(str(Path(__file__).resolve().parents[1]/'LegacyCaseMainSpan.Probe/audit_output.py'),run_name='__main__')
print('PASS independent source keys: ten distinct(node,span), ONE node-boundary epoch sequence; same195cost->permission->case-map->397records oracle')
print('PASS ten owned/public auxiliary immutable60 queries use signed span-1 independently of normal D0 keys; actual resource admits zero terminals')
print(f'PASS actual895571-node packed dictionary: auxiliary60 mappedlabels={sorted(mapped)}, eligibleedges={eligible}, globallyzero terminaledges (not just root)')
