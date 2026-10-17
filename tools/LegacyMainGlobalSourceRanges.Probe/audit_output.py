"""Independent recursive compact21B source traversal and reverse coalescing."""
import json,sys
from pathlib import Path
cases=json.loads(Path(sys.argv[1]).read_text());queries=merges=0
for case in cases:
 nodes=case['nodes'];metadata=case['metadata'];keys=case['keys']
 def visit(node,ordinal,target):
  group,start,end,span,children=nodes[node][ordinal]
  assert span==0 and start>>7==node
  if target<end:return None
  if target==end:return [group]
  for child in children:
   path=visit(node+1,child,target)
   if path is not None:return [group]+path
  return None
 for query in case['queries']:
  path=None
  for a,arc in enumerate(nodes[query['first']>>7]):
   if arc[3]==0 and arc[1]==query['first']:
    path=visit(query['first']>>7,a,query['last'])
    if path is not None:break
  assert path is not None
  rows=[]
  for group in path:
   for stroke,start,end in case['groups'][group]:
    rows.append(dict(first=metadata[stroke][start],last=metadata[stroke][end-1],tail=end==len(metadata[stroke])))
  #Independent bubble ordering, rather than calling the managed sorter.
  def order(r):return tuple(r['first'][0])+(-r['first'][3],)+tuple(r['last'][1])+(-r['last'][3],)
  for end in range(len(rows)-1,0,-1):
   for i in range(end):
    if order(rows[i])>order(rows[i+1]):rows[i],rows[i+1]=rows[i+1],rows[i]
  for i in range(len(rows)-1,0,-1):
   a,b=rows[i-1],rows[i];tail=a['last'];first=b['first']
   equal=first[0]==tail[0] and first[3]==tail[3]
   adjacent=a['tail'] and tail[2]+1<len(keys) and first[0]==keys[tail[2]+1]
   if equal or adjacent:a['last']=b['last'];a['tail']=b['tail'];rows.pop(i);merges+=1
  expected=[[r['first'][0],r['last'][1 if r['tail'] else 0],r['first'][3],r['last'][3]]for r in rows]
  assert expected==query['ranges'],(expected,query['ranges'])
  queries+=1
assert len(cases)==7 and queries==211 and merges>0
print(f'PASS independent{len(cases)} actual shared graphs/{queries} source callbacks/{merges} reverse merges, full residual bytes and global original key owner')
