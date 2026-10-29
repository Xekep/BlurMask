from pathlib import Path
import copy,json,hashlib
out=Path(__file__).resolve().parent
cases=[]
def node(kind,name,parent=0,tag=0,text=None,number=None):return dict(Kind=kind,Name=name,Parent=parent,Tag=tag,Text=text,Number=number)
def initial(size=0,attrs=0):return [node(4,''),node(3,'root',1)]+[node(0,'a'+str(i),2,4,number=i)for i in range(attrs)]+[node(3,'n'+str(i),2)for i in range(size)]
def action(op,target=2,payload=0,ordinal=-1,tag=0,text=None,number=None):return dict(Op=op,Target=target,Payload=payload,Ordinal=ordinal,Tag=tag,Text=text,Number=number)
def reference(nodes,actions):
 ns=[dict(Id=i+1,**n,Freed=False)for i,n in enumerate(copy.deepcopy(nodes))];links={n['Id']:[]for n in ns}
 for n in ns:
  if n['Parent']:links[n['Parent']].append(n['Id'])
 def detach(n):
  if n['Parent']:links[n['Parent']].remove(n['Id']);n['Parent']=0
 status='success';visited=0
 for a in actions:
  op=a['Op']
  if op in [1,3]:status='scope-refusal';break
  if op not in [2,4,5,6,7]:status='unknown-opcode';break
  t=ns[a['Target']-1];children=links[t['Id']]
  if op==2:
   p=ns[a['Payload']-1];seq=[i for i in children if ns[i-1]['Kind']!=0];ordinal=a['Ordinal'];anchor=seq[ordinal]if 0<=ordinal<len(seq)else None
   if anchor==p['Id']:status='scope-refusal';break
   detach(p);where=children.index(anchor)if anchor is not None else len(children);children.insert(where,p['Id']);p['Parent']=t['Id']
  elif op==4:
   seq=[i for i in children if ns[i-1]['Kind']!=0];ordinal=a['Ordinal']
   if t['Kind']>2 and 0<=ordinal<len(seq):detach(ns[seq[ordinal]-1])
  elif op==5:
   p=ns[a['Payload']-1]
   if p['Parent']!=t['Id']:
    detach(p)
    same=[i for i in children if ns[i-1]['Kind']==0 and ns[i-1]['Name']==p['Name']]
    assert len(same)<=1,'Only valid unique native attribute chains are in scope'
    for i in same:detach(ns[i-1]);ns[i-1]['Freed']=True
    places=[i for i,k in enumerate(children)if ns[k-1]['Kind']==0];where=places[-1]+1 if places else 0;children.insert(where,p['Id']);p['Parent']=t['Id']
  elif op==6:
   p=ns[a['Payload']-1]
   if p['Id']in children and p['Kind']==0:detach(p)
  elif op==7:t.update(Tag=a['Tag'],Text=a['Text'],Number=a['Number'])
  visited+=1
 return dict(Status=status,Visited=visited,Nodes=ns,Links=[dict(Id=n['Id'],Children=links[n['Id']])for n in ns])
def add(name,nodes,actions):cases.append(dict(Name=name,Nodes=nodes,Actions=actions,Expected=reference(nodes,actions)))
for size in range(8):
 for ordinal in range(-2,size+3):
  n=initial(size);n.append(node(3,'payload'));add(f'insert-{size}-{ordinal}',n,[action(2,payload=len(n),ordinal=ordinal)])
  if size:
   for payload in {3,size+2}:add(f'move-{size}-{ordinal}-{payload}',initial(size),[action(2,payload=payload,ordinal=ordinal)])
  n=initial(size);n.append(node(3,'misleading'));add(f'delete-{size}-{ordinal}',n,[action(4,payload=len(n),ordinal=ordinal)])
 if size:add(f'delete-shift-{size}',initial(size),[action(4,payload=size+2,ordinal=0)for _ in range(size+1)])
for size in range(6):
 for name in ['new',*['a'+str(i)for i in range(size)]]:
  n=initial(3,size);n.append(node(0,name,tag=4,number=99));add(f'attr-add-{size}-{name}',n,[action(5,payload=len(n))])
 if size:
  add(f'attr-same-parent-{size}',initial(3,size),[action(5,payload=3)])
  add(f'attr-remove-{size}',initial(3,size),[action(6,payload=3),action(6,payload=3)])
 n=initial(3,size);n.append(node(0,'a0',tag=4,number=99));add(f'attr-wrong-identity-{size}',n,[action(6,payload=len(n))])
values=[(4,None,-9223372036854775808),(4,None,9223372036854775807),(7,None,-2147483648),(7,None,2147483647),(10,None,0),(3,'',None)]+[(2,'x'*k,None)for k in [1,14,15]]+[(1,'x'*k,None)for k in [16,500,501,4096]]+[(1,'\u044f'*8,None)]
for oi,old in enumerate(values):
 for ni,new in enumerate(values):
  n=initial();n.append(node(0,'value',2,*old));add(f'scalar-{oi}-{ni}',n,[action(7,target=3,tag=new[0],text=new[1],number=new[2])])
n=initial(3,2);n.extend([node(3,'payload'),node(0,'new',tag=4,number=9)])
ops=[action(2,payload=8,ordinal=0),action(4,payload=7,ordinal=1),action(5,payload=9),action(7,target=3,tag=4,number=-8),action(6,payload=4)]
add('mixed-order',n,ops)
for op in [0,1,3,8,255]:add(f'prefix-stop-{op}',n,[*ops[:1],action(op),*ops[1:]])
target=out/'fixtures.json';assert not target.exists();target.write_text(json.dumps(cases,ensure_ascii=True,separators=(',',':')))
(out/'fixture-freeze.json').write_text(json.dumps(dict(count=len(cases),sha256=hashlib.sha256(target.read_bytes()).hexdigest(),scope='PREPARED_TYPED_ARENA_ONLY_NO_CURRENT_OWNER_OR_VENDOR_EXECUTION'),indent=2));print('frozen',len(cases),'prepared cases')
