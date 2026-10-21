"""Independent actual global01FD40/020000 metadata/tree/link-order oracle.
Composes raw CHAR/WORD/TEXT/coverage oracles first. No expected text labels.
"""
import json,runpy,struct,sys,importlib.util
from collections import Counter
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)not in(6,7):raise SystemExit('MODELS CHAR13 WORD-TEXT13 COVERAGE13 SELECTED13 [NATIVE13]')
models,char_path,parent_path,coverage_path,selected_path=sys.argv[1:6];saved=sys.argv[:]
try:
 sys.argv=['audit_output.py',models,char_path,parent_path,coverage_path]
 runpy.run_path(str(root/'tools/LegacyGlobalDocumentCoverage.Probe/audit_output.py'),run_name='__main__')
finally:sys.argv=saved
parents={s['name']:s for s in json.loads(Path(parent_path).read_bytes())['actual']}
children={s['name']:s for s in json.loads(Path(char_path).read_bytes())['samples']}
coverage={s['name']:s for s in json.loads(Path(coverage_path).read_bytes())['samples']}
d=json.loads(Path(selected_path).read_bytes());nodes=writes=0
spec=importlib.util.spec_from_file_location('actual_raw_kind_reference',root/'tools/LegacyGlobalDocumentSelectedSources.Probe/raw_kind_reference.py');kindref=importlib.util.module_from_spec(spec);spec.loader.exec_module(kindref)
actual_lookups=[]
def lookup(key):
 b=bytes.fromhex(key);return struct.unpack_from('<I',b,4)[0]==struct.unpack_from('<I',b,12)[0]and struct.unpack_from('<H',b,2)[0]==0 and b[1]==0 and struct.unpack_from('<H',b,10)[0]==3 and b[9]==255
assert len(d['samples'])==13 and len({s['name']for s in d['samples']})==13
for s in d['samples']:
 name=s['name'];p=parents[name];c=children[name];v=coverage[name]
 assert v['completed']
 assert s['completed']
 assert s['tree']==c['tree'];tree={n[0]:n for n in s['tree']};order=[]
 def visit(identity,depth=0):
  assert depth<64
  if identity:visit(tree[identity][1],depth+1);order.append(identity);visit(tree[identity][2],depth+1)
 visit(s['root']);assert len(order)==len(tree)==len(set(order))
 assert [x['node']for x in s['characters']]==order
 bynode={g['node']:g for g in p['links']};assert set(bynode)==set(order)
 ordinal={identity:i for i,identity in enumerate(order)};state={}
 for g in s['characters']:
  old=bynode[g['node']];assert g['before']==old['metadataFlags']
  expected_meta,raw_lookup=kindref.metadata(old['metadataFlags'],old['selectionId'],old['processed'],c['rawCounts'])
  if raw_lookup is not None:actual_lookups.append((name,g['node'],raw_lookup))
  assert g['metadata']==expected_meta
  assert g['selection']==old['selectionId']and g['before44']==-1
  assert bytes.fromhex(g['key16'])==b''.join(struct.pack('<QQ',*r)for r in old['processed'])
  if g['before']&0x10600000==0:
   if g['before']&0x20000000:assert g['selection']==-1
   elif len(old['processed'])==1 and lookup(g['key16']):assert raw_lookup is not None and raw_lookup['reported']==0xc1e
  assert len(g['records'])==len(old['records'])
  for row,before in zip(g['records'],old['records']):
   assert bytes.fromhex(row['scalar40'])==bytes.fromhex(before['scalar40'])and bytes.fromhex(row['utf8'])==bytes.fromhex(before['utf8'])
  state[g['node']]=[g['metadata'],-1]
 expectedlinks=[[g['node'],g['marker']]for g in p['links']];assert s['links']==expectedlinks
 expectedwrites=[[ordinal[node],node,marker]for node,marker in expectedlinks];assert s['writes']==expectedwrites
 for node,marker in expectedlinks:state[node][0]|=0x08000000;state[node][1]=marker
 for g in s['characters']:assert [g['flags'],g['index44']]==state[g['node']]
 word=p['wordFlags']&0xfeffffff
 if all(kindref.metadata(bynode[node]['metadataFlags'],bynode[node]['selectionId'],bynode[node]['processed'],c['rawCounts'])[0]&0x11004000 for node,_ in expectedlinks):word|=0x01000000
 assert(s['wordBefore'],s['wordMetadata'],s['wordFlags'],s['wordIndex'])==(p['wordFlags'],word,word|0x08000000,0)
 assert s['pendingBefore']==s['pendingAfter']==[q['id']for q in p['commands']]
 assert bytes.fromhex(s['lineBefore'])==bytes.fromhex(s['lineAfter'])==bytes.fromhex(p['sourceLines56'])
 assert bytes.fromhex(s['word48Before'])==bytes.fromhex(s['word48After'])==bytes.fromhex(p['wordChoice48'])
 nodes+=len(tree);writes+=len(expectedwrites)
# Guards:13pre-cancel +13*5 historical/stale/disposed +2lazy refusals +5scalar failures =85.
assert(nodes,writes,d['completed'],d['refused'],d['totalNodes'],d['totalWrites'],d['guards'])==(37,37,13,0,37,37,13+13*5+2+5)
assert len(d['prepared'])==len(d['preparedWords'])==256 and len(d['lazy'])==14
for p in d['preparedWords']:
 expected=p['before']&0xfeffffff
 if all(v&0x11004000 for v in p['metadata']):expected|=0x01000000
 assert p['after']==expected
for p in d['prepared']:
 flags=p['before'][:];indices=p['initial'][:]
 for i,marker in zip(p['ordinals'],p['markers']):flags[i]|=0x08000000;indices[i]=marker
 assert flags==p['flags']and indices==p['indices']
for p in d['lazy']:
 expected=True
 if p['flags']&0x10600000==0:
  if p['flags']&0x20000000:expected=p['id']==-1
  elif p['count']==1:expected=not lookup(p['key16'])
 assert p['success']==expected
if len(sys.argv)==7:assert Path(selected_path).read_bytes()==Path(sys.argv[6]).read_bytes()
print('PASS independent13 global overlays/37 complete AVL nodes/37 link-order writes; actual typed kind resolution/WORD flags/full old CHAR rows/256 alias controls/14 lazy branches; no020920/DOM/quality claim.')

assert actual_lookups==[('MIR',2,dict(source=1,concrete=0xc49,reported=0xc1e))]
assert len(d['typedKinds'])==13
for receipt in d['typedKinds']:
 counts=children[receipt['name']]['rawCounts'];assert receipt['unchanged']is True and receipt['cancellationPrefixes']==len(counts)
 assert receipt['objects']==[dict(source=i,points=n,concrete=0xc49,reported=0xc1e)for i,n in enumerate(counts)]
expected_checks=Counter({'null':1,'preflight':6,'foreign-selected':1,'foreign-receipt':1,'snapshot-scalars':1,'snapshot-skip-flags':1,'snapshot-key':1,'foreign-source':1,'poisoned-validate':1,'poisoned-lookup':1,'poisoned-create':1,'cancel-prefix':16})
for stage in ['cancel-create','out-of-range','max-index','foreign-view','disposed-validate','disposed-lookup']:expected_checks[stage]=13
assert Counter(d['kindChecks'])==expected_checks and sum(expected_checks.values())==110
assert len(d['metadataKinds'])==512
for value in d['metadataKinds']:assert value['after']==(value['before']&0xfeffffff)|(0x01000000 if value['kind']==0xc25 else 0)
print('PASS13 actual C49 provenance receipts/16 objects/one MIR rawslot1 C49-to-C1E lookup/110 ownership-cancel guards/512 prepared bit24 controls.')
