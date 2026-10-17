"""Independent complete source AVL walk,01FD40 guards and020000 last writes."""
import base64,json,struct,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
#The prior independent raw-key/subtraction replay also checks the preserved
#input WORD48, line56 and registration context in this extended capture.
subprocess.run([sys.executable,str(root/'tools/LegacyMainDocumentCharacterCoverage.Probe/audit_output.py'),sys.argv[1]],check=True)
j=json.loads(Path(sys.argv[1]).read_bytes());nodes=writes=0
for sample in j['samples']:
 s=sample['selectedSources'];shape={r[0]:r for r in s['tree']};order=[]
 def visit(node):
  if not node:return
  assert node not in order
  row=shape[node];visit(row[1]);order.append(node);visit(row[2])
 visit(s['root']);assert len(order)==len(shape)==s['actualTreeCount']
 assert order==[c['node']for c in s['characters']]and set(order)==set(shape)
 current={};indices={};keys=[]
 for c in s['characters']:
  key=base64.b64decode(c['key16']);keys.append([struct.unpack_from('<QQ',key,i)for i in range(0,len(key),16)])
  flags=c['before']
  if not flags&0x10600000:
   if flags&0x20000000:assert c['selectionId']==-1 #otherwise unsupported prior owner read
   elif len(key)==16:
    lookup=struct.unpack_from('<I',key,4)==struct.unpack_from('<I',key,12)and struct.unpack_from('<H',key,2)[0]==0 and key[1]==0 and struct.unpack_from('<H',key,10)[0]==3 and key[9]==255
    assert not lookup #the bounded producer refuses the actually used C25 getter
  assert c['metadata']==flags;current[c['node']]=flags;indices[c['node']]=c['before44'];nodes+=1
 assert keys==sorted(keys) #unsigned lexical C21 key order, not source creation order
 assert s['selectedWord']==s['nativeSelectedWord']
 expected_meta=(s['wordBefore']&0xfeffffff)|(0x01000000 if all(current[node]&0x11004000 for node,_ in s['selectedLinks'])else 0)
 assert s['wordMetadata']==expected_meta and s['wordFlags']==(expected_meta|0x08000000)
 expected_events=[]
 for node,marker in s['selectedLinks']:
  current[node]|=0x08000000;indices[node]=marker;expected_events.append([order.index(node),node,marker]);writes+=1
 assert s['writes']==expected_events
 for c in s['characters']:assert c['flags']==current[c['node']]and c['index44']==indices[c['node']]
for case in j['preparedAliasControls']:
 flags=case['beforeFlags'][:];indices=case['beforeIndices'][:]
 for node,marker in zip(case['ordinals'],case['markers']):flags[node]|=0x08000000;indices[node]=marker
 assert flags==case['flags']and indices==case['indices']
 assert indices[0]==case['markers'][-1] #last raw signed marker, never max/sort/dedup
assert len(j['samples'])==4 and len(j['preparedAliasControls'])==256
print(f'PASS independent4 actual complete trees/{nodes} CHAR nodes/{writes} selected link writes/WORD+CHARflags18/index44/256 explicit alias-order controls; previous snapshots preserved, no acceptance.')
