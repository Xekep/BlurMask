"""Independent native-profile repeat export oracle. DATA only, never authority.
Old original content must come from a future actual-owner observation. Historical
83 clone predictions are CONTENT_ONLY and do not authenticate original IDs.
"""
from pathlib import Path
import base64,copy,hashlib,importlib.util
REF=Path(__file__).resolve().parent/'reference/default_diff.py'
assert hashlib.sha256(REF.read_bytes()).hexdigest()=='8f8c4942e58644b1076135d6cef1288d422a67433cbec28debe58b2a10118256'
spec=importlib.util.spec_from_file_location('default_reference',REF);ref=importlib.util.module_from_spec(spec);spec.loader.exec_module(ref)

def links(nodes,ordered=None):
 result={n['Id']:dict(Attributes=[],Children=[])for n in nodes}
 for n in nodes:
  if n['Parent']:result[n['Parent']]['Attributes'if n['Kind']==0 else 'Children'].append(n['Id'])
 if ordered:
  for row in ordered:
   for k in ('Attributes','Children'):
    assert sorted(result[row['Id']][k])==sorted(row[k]);result[row['Id']][k]=row[k][:]
 return result

def content(nodes,root,order=None):
 byid={n['Id']:n for n in nodes};ls=links(nodes,order)
 def walk(i):
  n=byid[i]
  return dict(Kind=n['Kind'],Name=n['Name'],ValueTag=n['ValueTag'],Text=n['Text'],Number=n['Number'],NamespaceUri=n['NamespaceUri'],NamespacePrefix=n['NamespacePrefix'],Attributes=[walk(j)for j in ls[i]['Attributes']],Children=[walk(j)for j in ls[i]['Children']])
 return walk(root)

def dom_export(old,new,oldroot,newroot,oldtree=None,newtree=None):
 oldby={n['Id']:n for n in old};newby={n['Id']:n for n in new};ol=links(old,oldtree);nl=links(new,newtree);raw=[];scratch=[]
 fourth=[dict(Id=1,Kind=4,Name='',Parent=0,OriginalId=0,PayloadOrigin=0,ValueTag=0,Text=None,Number=None,NamespaceUri=None,NamespacePrefix=None)]
 fl={1:dict(Attributes=[],Children=[])}
 def clone(source,sl,identity,parent=0):
  n=source[identity];i=len(fourth)+1
  v=copy.deepcopy(n);v.update(Id=i,Parent=parent,PayloadOrigin=0);fourth.append(v);fl[i]=dict(Attributes=[],Children=[])
  if parent:fl[parent]['Attributes'if v['Kind']==0 else 'Children'].append(i)
  for key in ('Attributes','Children'):
   for ch in sl[identity][key]:clone(source,sl,ch,i)
  return i
 def action(op,target,operand=None,source=0,ordinal=-1,ov=None,nv=None):
  payload=0
  if operand:
   sourceby,sourcels,ident=operand;payload=clone(sourceby,sourcels,ident);scratch.append(payload)
  n=oldby[target];a=dict(Operation=op,NodeAt18=target,NodeAt20=payload,Ordinal=ordinal,SourceCandidate=source,EnvelopeKind=n['Kind'],Name=n['Name'],OldTag=0,NewTag=0,OldStorage='',NewStorage='',OldHeap='',NewHeap='',Filtered=False)
  if ov is not None:
   for prefix,v in [('Old',ov),('New',nv)]:
    tag,storage,heap=ref.scalar(v);a[prefix+'Tag']=tag;a[prefix+'Storage']=base64.b64encode(storage).decode();a[prefix+'Heap']=base64.b64encode(heap).decode()
  raw.append(a)
 def walk(i,j):
  a,b=oldby[i],newby[j];assert a['Kind']==b['Kind']
  if a['Kind']==1:
   if not ref.equal(a,b):action(7,i,source=j,ov=a,nv=b)
   return
  assert a['Kind']==3 and a['Name']==b['Name']
  aa=[oldby[k]for k in ol[i]['Attributes']];ba=[newby[k]for k in nl[j]['Attributes']]
  for v in aa:
   other=next((x for x in ba if x['Name']==v['Name']),None)
   if other is None:action(6,i,(oldby,ol,v['Id']))
   elif not ref.equal(v,other):action(7,v['Id'],source=other['Id'],ov=v,nv=other)
  for v in ba:
   if not any(x['Name']==v['Name']for x in aa):action(5,i,(newby,nl,v['Id']),source=v['Id'])
  ac=[oldby[k]for k in ol[i]['Children']];bc=[newby[k]for k in nl[j]['Children']];cursor=-1
  for x,y in ref.align(ac,bc):
   if x<0:cursor=max(0,cursor+1);action(2,i,(newby,nl,bc[y]['Id']),source=bc[y]['Id'],ordinal=cursor)
   elif y<0:action(4,i,(oldby,ol,ac[x]['Id']),ordinal=max(0,cursor+1))
   else:walk(ac[x]['Id'],bc[y]['Id']);cursor=max(0,cursor+1)
 walk(oldroot,newroot)
 def path(i):
  reversed=[]
  while i!=oldroot:
   p=oldby[i]['Parent'];assert p in oldby and i in ol[p]['Children'];reversed.append(ol[p]['Children'].index(i));i=p
  return reversed[::-1]
 exported=[]
 for a in raw:
  op=a['Operation'];target=a['NodeAt18'];parent=oldby[target]['Parent']if op==7 else target;payload=0
  if op in (2,5,6):payload=clone({n['Id']:n for n in fourth},fl,a['NodeAt20'])
  elif op==7:
   payload=clone(oldby,ol,target);newvalue=newby[a['SourceCandidate']];fourth[payload-1].update(ValueTag=newvalue['ValueTag'],Text=newvalue['Text'],Number=newvalue['Number'])
  else:assert op==4
  exported.append(dict(Operation=op,Path=path(parent),Ordinal=a['Ordinal']if op in (2,4)else 0,PayloadRoot=payload,OriginalNode=target,NewNode=a['SourceCandidate']))
 return dict(Dom=exported,FourthWriter=fourth,ReleasedScratchRoots=scratch,RawActions=raw)

def source_export(old,new,oldnext,newnext):
 before={r['Id']:r for r in old};after={r['Id']:r for r in new};assert len(before)==len(old)and len(after)==len(new)
 def key(r):return r['Name'],r['Category'],[(x['First']['Value'],x['Last']['Value'])for x in r['Ranges']]
 for i in before.keys()&after.keys():assert key(before[i])==key(after[i]),'Distinct UPDATE5 union required beforeclaim'
 assert 0<oldnext<=newnext<=65536
 out=[]
 def append(op,r):out.append(dict(Operation=op,Id=r['Id'],Name=r['Name'],Category=r['Category'],Ranges=copy.deepcopy(r['Ranges']),OldCounter20=-1,NewCounter18=-1,DetachedC22=True))
 for i in sorted(after.keys()-before.keys()):append(4,after[i])
 for i in sorted(before.keys()-after.keys()):append(6,before[i])
 if oldnext<newnext:out.append(dict(Operation=7,Id=-1,Name=None,Category=0,Ranges=[],OldCounter20=oldnext,NewCounter18=newnext,DetachedC22=False))
 return out
