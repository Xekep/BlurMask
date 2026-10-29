"""Independent normalized temporary writer replay; frozen before actual937/83 run.
Tree rows: Id, Attributes(list IDs), Children(list IDs), root first.
No expected text, operation count, or kept count. Adapter may only normalize schema.
"""
import base64,copy,struct

def decode(action):
 tag=action['NewTag'];s=base64.b64decode(action['NewStorage']);h=base64.b64decode(action['NewHeap']);assert len(s)==16
 if tag==1:
  assert struct.unpack_from('<I',s,8)[0]==len(h) and len(h)>=16
  return tag,h.decode('utf-8',errors='strict'),None
 if tag==2:
  size=s[15] or 15;assert 1<=size<=15 and not h
  return tag,s[:size].decode('utf-8',errors='strict'),None
 if tag==3:return tag,'',None
 if tag==4:return tag,None,struct.unpack_from('<q',s)[0]
 if tag==7:return tag,None,struct.unpack_from('<i',s)[0]
 if tag==10:
  assert s==b'\0'*16 and not h
  return tag,None,0
 raise AssertionError(('unproved new scalar',tag))

def replay(before,before_tree,actions,candidate):
 nodes={n['Id']:copy.deepcopy(n)for n in before};assert len(nodes)==len(before)
 assert sorted(nodes)==list(range(1,len(nodes)+1))
 candidate={n['Id']:n for n in candidate}
 links={i:dict(Attributes=[],Children=[])for i in nodes}
 for n in before:
  if n['Parent']:
   assert n['Parent'] in nodes
   links[n['Parent']]['Attributes'if n['Kind']==0 else 'Children'].append(n['Id'])
 assert before_tree
 root=before_tree[0]['Id']
 seen=set()
 for row in before_tree:
  i=row['Id'];assert i not in seen;seen.add(i)
  # Snapshot allocation order need not be the live child order; the actual
  # before-tree is the initial ordered operand for already attached nodes.
  for kind in ('Attributes','Children'):
   assert sorted(row[kind])==sorted(links[i][kind]);links[i][kind]=list(row[kind])
 def reachable():
  out=[];seen=set()
  def visit(i):
   assert i not in seen;seen.add(i)
   out.append(dict(Id=i,Attributes=links[i]['Attributes'][:],Children=links[i]['Children'][:]))
   for k in ('Attributes','Children'):
    for ch in links[i][k]:assert nodes[ch]['Parent']==i;visit(ch)
  visit(root);return out
 assert reachable()==before_tree
 def clone_proof(i,origin):
  n=nodes[i];c=candidate[origin]
  assert n['PayloadOrigin']==origin and n['OriginalId']==0
  assert all(n[k]==c[k]for k in ('Kind','Name','ValueTag','Text','Number'))
  assert n['NamespaceUri'] is None and n['NamespacePrefix'] is None
  for key,attr in (('Attributes',True),('Children',False)):
   expected=[v['Id']for v in candidate.values()if v['Parent']==origin and (v['Kind']==0)==attr]
   actual=links[i][key];assert len(expected)==len(actual)
   for child,source in zip(actual,expected,strict=True):clone_proof(child,source)
 def detach(i):
  old=nodes[i]['Parent']
  if old:
   links[old]['Attributes'if nodes[i]['Kind']==0 else 'Children'].remove(i);nodes[i]['Parent']=0
 applied=0
 for a in actions:
  if a['Filtered']:continue
  op=a['Operation'];target=a['NodeAt18'];assert target in nodes
  if op==2:
   payload=a['NodeAt20'];assert payload in nodes and nodes[target]['Kind'] in (3,4) and 1<=nodes[payload]['Kind']<=3
   assert nodes[payload]['Parent']==0
   clone_proof(payload,a['SourceCandidate'])
   children=links[target]['Children'];ordinal=a['Ordinal'];before=children[ordinal]if 0<=ordinal<len(children)else None
   assert before!=payload
   detach(payload)
   children.insert(len(children)if before is None else children.index(before),payload);nodes[payload]['Parent']=target
  elif op==4:
   # E4B90 selects NOW-current child by ordinal; +20 is not the selector.
   if nodes[target]['Kind'] in (3,4):
    children=links[target]['Children'];ordinal=a['Ordinal']
    if 0<=ordinal<len(children):detach(children[ordinal])
  elif op==7:
   assert nodes[target]['Kind'] in (0,1)
   tag,text,number=decode(a);nodes[target].update(ValueTag=tag,Text=text,Number=number)
  else:raise AssertionError(('outside produced 2/4/7 scope',op))
  applied+=1
 return [nodes[i]for i in sorted(nodes)],reachable(),applied
