"""Independent native CB8 late replay, no engine or owner ingress.
Derived from033560 prepending,0336C0 first-head/cache-before-write,
033870 ordered collection and0339D0 ordered stale removal.
"""
import base64,copy,struct
MAX=(1<<63)-1;MIN=-(1<<63)

def select_pending(rows,value):
 if value==MAX:return None,False
 if value>=0:return None,True
 key=(-value)&((1<<64)-1)
 if key>=1<<63:key-=1<<64
 for row in reversed(rows):
  if row['TemporaryId']==key:return row,None
 return None,False

def resolve_one(rows,value,register,write):
 match,done=select_pending(rows,value)
 if done is not None:return done
 if match['ResolvedId']<0:match['ResolvedId']=register(match)
 return match['ResolvedId']>=0 and bool(write(match['ResolvedId']))

def live_tags(nodes,tree=None):
 byid={n['Id']:n for n in nodes}
 if tree is None:
  links={i:dict(Attributes=[],Children=[])for i in byid}
  for n in nodes:
   if n['Parent']:links[n['Parent']]['Attributes'if n['Kind']==0 else 'Children'].append(n['Id'])
  root=next(n['Id']for n in nodes if n['Kind']==3 and n['Name']=='mathResult' and n['Parent']==1)
 else:
  links={r['Id']:r for r in tree};root=tree[0]['Id']
 out=[]
 def descend(i):
  for ch in links[i]['Children']:
   if byid[ch]['Kind']!=3:continue
   for attr in links[ch]['Attributes']:
    n=byid[attr]
    if n['Name']=='tag':assert n['Kind']==0 and n['ValueTag']==4 and n['Number'] is not None;out.append(attr)
   descend(ch)
 descend(root);return out

def replay(case):
 c=case['Candidate'];nodes=copy.deepcopy(case['Application']['ObservedTemporary']);tree=copy.deepcopy(case['AfterTree']);byid={n['Id']:n for n in nodes}
 pending=copy.deepcopy(c['Pending']);registry=copy.deepcopy(c['Registry']);nextid=c['RegistryNext']
 oldmap={n['Id']:n for n in c['Temporary']};old=[oldmap[i]['Number']for i in live_tags(c['Temporary'])]
 assert old==c['OldTags'];assert all(t>=0 and any(r['Id']==t for r in registry)for t in old)
 aliases={r['Id']:('original',r['CanonicalSelection'])for r in registry}
 newtags=[];removed=[];registrations=written=0;registering=False;events=[]
 def snapshot(complete=False):
  compact={};rows=[]
  for r in sorted(registry,key=lambda r:r['Id']):
   token=aliases[r['Id']]
   if token not in compact:compact[token]=len(compact)+1
   rows.append(dict(r,CanonicalSelection=compact[token]))
  return dict(Complete=complete,Ready=False,Failed=False,Released=False,Registrations=registrations,TagsWritten=written,RegistrationInProgress=registering,NewTags=newtags[:],Removed=removed[:],Observed=dict(Next=nextid,Registry=rows,Pending=copy.deepcopy(pending),Temporary=copy.deepcopy(nodes)))
 def event(stage,complete=False):events.append(dict(Stage=stage,Snapshot=snapshot(complete)))
 event('Claimed')
 for i in live_tags(nodes,tree):
  value=byid[i]['Number'];match,done=select_pending(pending,value)
  assert done is not False,('unresolved live tag',value)
  if done is True:continue
  if match['ResolvedId']<0:
   assert match['HasSelection'] and match['Ranges'],'Actual nonempty C22 scope'
   registering=True;event('BeforeRegister')
   def ranges(row):return [(r['First']['Value'],r['Last']['Value'])for r in row['Ranges']]
   same=next((r for r in registry if ranges(r)==ranges(match)),None)
   token=aliases[same['Id']]if same is not None else ('pending',match['TemporaryId'])
   resolved=nextid;nextid+=1
   registry.append(dict(Id=resolved,Name=match['Name'],Category=102,CanonicalSelection=0,Ranges=copy.deepcopy(match['Ranges'])))
   aliases[resolved]=token;match['ResolvedId']=resolved
   registrations+=1;registering=False;event('Registered')
  byid[i].update(ValueTag=4,Text=None,Number=match['ResolvedId']);written+=1;event('TagWritten')
 event('Resolved')
 for i in live_tags(nodes,tree):
  value=byid[i]['Number'];assert value!=-1;newtags.append(value);event('TagCollected')
 event('Collected')
 for value in old:
  if value in newtags:continue
  registry[:]=[r for r in registry if r['Id']!=value];removed.append(value);event('Removed')
 event('Completed',True)
 final=snapshot(True);final['Ready']=True
 for row in tree:
  n=byid[row['Id']]
  if n['Name']=='tag' and n['Kind']==0:
   row.update(ValueTag=4,Storage=base64.b64encode(struct.pack('<q',n['Number'])+b'\0'*8).decode(),Heap='')
 return final,tree,events
