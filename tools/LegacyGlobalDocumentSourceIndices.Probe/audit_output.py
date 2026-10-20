"""Independent actual-global source-key/index/page/member/failed-prefix oracle.
Imports only independent Python numeric definitions, never runtime or SDK code.
"""
import ast,json,runpy,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)not in(7,8):raise SystemExit('MODELS CHAR13 PARENT13 COVERAGE13 SELECTED13 INDEX13 [NATIVE13]')
models,char_path,parent_path,coverage_path,selected_path,index_path=sys.argv[1:7];saved=sys.argv[:]
try:
 sys.argv=['audit_output.py',models,char_path,parent_path,coverage_path,selected_path]
 runpy.run_path(str(root/'tools/LegacyGlobalDocumentSelectedSources.Probe/audit_output.py'),run_name='__main__')
finally:sys.argv=saved
independent=root/'tools/LegacyMainDocumentSourceIndices.Probe/audit_output.py'
tree=ast.parse(independent.read_text(encoding='utf-8-sig'));namespace={}
definitions=[n for n in tree.body if isinstance(n,(ast.ClassDef,ast.FunctionDef))]
exec(compile(ast.Module(body=definitions,type_ignores=[]),str(independent),'exec'),namespace);replay=namespace['replay']
d=json.loads(Path(index_path).read_bytes());states={s['name']:s for s in json.loads(Path(selected_path).read_bytes())['samples']}
parents={s['name']:s for s in json.loads(Path(parent_path).read_bytes())['actual']}
assert len(d['samples'])==13 and len({s['name']for s in d['samples']})==13
def summary(index):return[index['added'],len(index['intervals']),len(index['vectors']),len(index['members']),int(index['dirty'])]
def both(sequence):return[replay([(i+1,s['ranges'])for i,s in enumerate(sequence)if s['word']==word])[0]for word in(False,True)]
def prefixes(sequence,fail=None):
 empty=both([]);result=[dict(stage='entered',char=summary(empty[0]),word=summary(empty[1]))];done=[]
 if fail==0:return result+[dict(stage='fault',char=summary(empty[0]),word=summary(empty[1]))]
 for source in sequence:
  indexes=both(done);result.append(dict(stage='word-enter'if source['word']else'char-enter',char=summary(indexes[0]),word=summary(indexes[1])))
  done.append(source);indexes=both(done);result.append(dict(stage='word-complete'if source['word']else'char-complete',char=summary(indexes[0]),word=summary(indexes[1])))
  if fail==len(done):return result+[dict(stage='fault',char=summary(indexes[0]),word=summary(indexes[1]))]
 indexes=both(done);return result+[dict(stage='ready',char=summary(indexes[0]),word=summary(indexes[1]))]
total=0
for sample in d['samples']:
 state=states[sample['name']];parent=parents[sample['name']];assert sample['completed']==state['completed']
 if not state['completed']:assert sample['refusal']=='238B0';continue
 byid={c['node']:c for c in state['characters']};shape={n[0]:n for n in state['tree']}
 insertion=[];current=next(n[0]for n in state['tree']if n[4]==0)
 while current:
  assert current not in insertion;insertion.append(current);current=shape[current][5]
 assert sample['charInsertion']==insertion and sample['charInorder']==[c['node']for c in state['characters']]
 expected=[]
 for node in insertion:
  source=byid[node]
  if source['flags']&0x200000:continue
  data=bytes.fromhex(source['key16']);ranges=[[int.from_bytes(data[i:i+8],'little'),int.from_bytes(data[i+8:i+16],'little')]for i in range(0,len(data),16)]
  expected.append(dict(word=False,node=node,flags=source['flags'],header=0x20,ranges=ranges))
 if state['wordFlags']&0x200000==0:expected.append(dict(word=True,node=parent['node'],flags=state['wordFlags'],header=0x30 if parent['ordinary']else 0x20,ranges=parent['ordinary']or parent['wordKey']))
 assert sample['sources']==expected and sample['sourceBound']==len(expected)
 assert sample['membershipBound']==sum(len(s['ranges'])for s in expected)
 chars,words=both(expected);assert sample['characters']==chars and sample['words']==words
 assert sample['prefixes']==prefixes(expected)and sample['ready']and not sample['faulted'];total+=len(expected)
assert(total,d['sources'],d['completed'],d['refused'],d['guards'],d['budgetGuards'])==(44,44,11,2,189,4)
success=next(s for s in d['samples']if s['name']=='M')['sources'];assert len(d['failures'])==3
for failure in d['failures']:
 count=failure['after'];assert count in(0,1,2)and failure['faulted']and not failure['ready']and failure['visited']==count
 chars,words=both(success[:count]);assert failure['characters']==chars and failure['words']==words
 assert failure['prefixes']==prefixes(success,fail=count)
assert len(d['prepared'])==40;rotations=[0]*4;nodes=members=duplicates=0
for case in d['prepared']:
 expected,a,b=replay([(i+1,key)for i,key in enumerate(case['inputs'])]);assert case['index']==expected
 rotations=[x+y+z for x,y,z in zip(rotations,a,b)];nodes+=len(expected['intervals'])+len(expected['vectors']);members+=len(expected['members']);duplicates+=len(expected['members'])-len(expected['intervals'])
 assert max(n['fields'][1]for n in expected['intervals'])>=2 and max(n['fields'][1]for n in expected['vectors'])>=2 and expected['members'][-1][1]>=2
assert all(rotations)and duplicates>0
assert d['partialVectorFailure']==dict(vectorNodes=65536,sources=0,dirty=True,intervalKey=[[(1<<64)-2,(1<<64)-1]],member=[1,99,0])
#Independent AVL minimum-node recurrence bounds every preflighted tree path.
minimum=[0,1]
for height in range(2,33):minimum.append(1+minimum[-1]+minimum[-2])
assert minimum[22]<=65536<minimum[23]==75024
if len(sys.argv)==8:assert Path(index_path).read_bytes()==Path(sys.argv[7]).read_bytes()
print(f'PASS independent11 global indices/44 typed sources/3 entered-failure prefixes/40 prepared AVL cases/{nodes} nodes/{members} memberships/rotations{rotations};no023260/DOM/quality.')
