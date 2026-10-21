"""Independent actual maps, ordinary32B pages and height-based AVL/page oracle.
Input events are explicitly PREPARED operations, never handwriting NN requests.
"""
from pathlib import Path
import argparse,copy,hashlib,json,runpy,struct,sys
p=argparse.ArgumentParser()
for name in ('repo','models','capture','output'):p.add_argument('--'+name,type=Path,required=True)
a=p.parse_args();a.output.parent.mkdir(parents=True,exist_ok=True);prefix='eqs-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]
sys.argv=['reference',str(a.models),str(a.output.with_name(prefix+'-dictionary.json'))]
d=runpy.run_path(str(a.repo/'tools/LegacyEnglishLkPacked102.Probe/reference_actual.py'),run_name='__main__')
special={'\0':0xfffd,' ':0xfffb,'\n':0xfff9,'\u00a0':0xfffc,'\ufffd':0xffff,'\U000f000c':0xfffa,'\U000f0009':0xffff,'\U000f000a':0xffff,'\U000f000b':0xffff,'\U000f0000':0xfff6,'\U000f0006':0xfff8,'\U000f0007':0xfff7,'\U000f0005':0xffea}
for n in range(0x10,0x19):special[chr(0xf0000+n)]=0xfff3-(n-0x10)
for n in range(0x20,0x27):special[chr(0xf0000+n)]=0xffe9-(n-0x20)
primary=[special.get(row['text'],row['exactAcoustic']if row['exactAcoustic']is not None else 65535)for row in d['mapping']]
auxiliary=[symbol if symbol>=217 or row['text']in('\u200c','\u200d')else 65535 for row,symbol in zip(d['mapping'],primary)]
assert len(primary)==104
class Refusal(Exception):pass
class Canceled(Exception):pass
class Session:
 def __init__(self,bounds):
  self.bounds=bounds;self.nodes={};self.next=1;self.root=0;self.used=[];self.capacity=0;self.cursor=0;self.initial=[]
  self.fault=False;self.attempts=self.operations=self.outputs=0;self.checkpoints=1;self.epoch=1;self.working=primary[:];self.queries={};self.slots=[self.slot()for _ in range(8)];self.nextslot=0
  for symbol in primary:self.initial.append(0 if symbol in(65535,65533)else self.resolve(0,symbol)[0])
  assert len(self.nodes)==86;self.saved={None:self.pool()}
 @staticmethod
 def slot():return dict(node=-1,start=0,end=0,block=0,cursor=0,accepted=0,key=None)
 def pool(self):return copy.deepcopy((self.nodes,self.root,self.used,self.cursor))
 def height(self,node):return 0 if not node else 1+max(self.height(self.nodes[node]['l']),self.height(self.nodes[node]['r']))
 def balance(self,node):
  if node:self.nodes[node]['balance']=self.height(self.nodes[node]['r'])-self.height(self.nodes[node]['l'])
 def rotate_left(self,node):
  right=self.nodes[node]['r'];self.nodes[node]['r']=self.nodes[right]['l'];self.nodes[right]['l']=node;self.balance(node);self.balance(right);return right
 def rotate_right(self,node):
  left=self.nodes[node]['l'];self.nodes[node]['l']=self.nodes[left]['r'];self.nodes[left]['r']=node;self.balance(node);self.balance(left);return left
 def alloc(self,owner,key):
  page=-1
  if self.cursor<len(self.used)and self.used[self.cursor]<102:page=self.cursor
  else:
   for n in range(len(self.used)-1,-1,-1):
    if self.used[n]<102:page=n;break
  if page<0:
   page=len(self.used);need=page+1
   if need>self.capacity:self.capacity=max(need,int(self.capacity*1.5))
   self.used.append(0)
  self.cursor=page;slot=self.used[page];self.used[page]+=1;node=self.next;self.next+=1
  self.nodes[node]=dict(id=node,parent=0,child=0,l=0,r=0,key=key,balance=0,page=page,slot=slot,owner=owner)
  return node
 def insert(self,node,owner,key):
  if not node:
   leaf=self.alloc(owner,key);return leaf,leaf,True
  record=self.nodes[node]
  if key==record['key']:return node,node,False
  side='l'if key<record['key']else'r';record[side],leaf,created=self.insert(record[side],owner,key);self.balance(node)
  if record['balance']>1:
   right=record['r']
   if self.nodes[right]['balance']<0:record['r']=self.rotate_right(right)
   node=self.rotate_left(node)
  elif record['balance']< -1:
   left=record['l']
   if self.nodes[left]['balance']>0:record['l']=self.rotate_left(left)
   node=self.rotate_right(node)
  return node,leaf,created
 def resolve(self,parent,key):
  if parent:
   if parent not in self.nodes:raise Refusal()
   root,leaf,created=self.insert(self.nodes[parent]['child'],parent,key);self.nodes[parent]['child']=root
  else:root,leaf,created=self.insert(self.root,0,key);self.root=root
  return leaf,created
 def snapshot(self):
  nodes=sorted(self.nodes.values(),key=lambda n:(n['page'],n['slot']))
  return dict(Faulted=self.fault,Attempts=self.attempts,Operations=self.operations,Outputs=self.outputs,Checkpoints=self.checkpoints,Epoch=self.epoch,Root=self.root,PageCapacity=self.capacity,CurrentPage=self.cursor,Pages=self.used[:],Primary=primary,Auxiliary=auxiliary,Working=self.working[:],Initial=self.initial[:],Nodes=[[n['id'],n['parent'],n['child'],n['l'],n['r'],n['key'],n['balance'],n['page'],n['slot'],n['owner']]for n in nodes])
 def check_stage(self,stage,event):
  if event.get('FailureStage')==stage:raise Canceled()
 def begin(self,attempts,outputs,event):
  if self.fault or attempts>self.bounds[0]-self.attempts or outputs>self.bounds[2]-self.outputs or self.operations>=self.bounds[1]:raise Refusal()
  self.operations+=1;self.check_stage('prepared:enter',event)
 def range(self,node):return d['starts'][node]+(node==0),d['starts'][node]+d['counts'][node]
 def refill(self,slot):
  if slot['cursor']<slot['block']:return
  slot['start']+=slot['block'];slot['cursor']=0;slot['block']=min(256,max(0,slot['end']-slot['start']))
 def read(self,args,event):
  q=self.queries[args['Name']];skip=args['Skip'];parent=q['Parent'];cap=q['Capacity'];hasroot=q['RootPresent'];aux=q['Auxiliary']
  if parent and parent not in self.nodes:raise Refusal()
  self.begin(cap if parent and hasroot else 0,cap,event)
  if q['slot']is None:
   q['slot']=self.nextslot;self.nextslot=(self.nextslot+1)&7;self.slots[q['slot']]['node']=-1
  slot=self.slots[q['slot']];node=q['Node']
  if slot['node']!=node:slot['start'],slot['end']=self.range(node);slot['node']=node;slot['accepted']=-1
  key=(q['Flags'],aux)
  if slot['accepted']!=skip or slot['key']!=key:
   slot['start'],slot['end']=self.range(node);slot['block']=slot['cursor']=slot['accepted']=0;slot['key']=key
  selected=auxiliary if aux else primary if q['Flags']&0x40 else self.working
  def accept(raw):return selected[raw&127]!=65535 and(not aux or bool(raw&128))
  while slot['start']+slot['cursor']<slot['end']and slot['accepted']<skip:
   self.refill(slot)
   while slot['cursor']<slot['block']and slot['accepted']<skip:
    raw=d['labels'][slot['start']+slot['cursor']];slot['cursor']+=1
    if accept(raw):slot['accepted']+=1
  result=bytearray()
  while slot['start']+slot['cursor']<slot['end']and slot['accepted']<skip+cap:
   self.refill(slot);cursor=slot['cursor'];accepted=slot['accepted'];written=accepted-skip
   while cursor<slot['block']and written<cap:
    child=slot['start']+cursor;raw=d['labels'][child];cursor+=1
    if not accept(raw):continue
    lexical=raw&127;symbol=selected[lexical]
    if not parent:context=self.initial[lexical]
    elif not hasroot:context=0
    else:
     self.attempts+=1;context,created=self.resolve(parent,symbol);self.check_stage('prepared:resolved',event)
     if created:self.nodes[context]['parent']=parent;self.check_stage('prepared:linked',event)
    row=bytearray(32);struct.pack_into('<I',row,0,child);row[5]=(node==0)|(2 if raw&128 else 0);struct.pack_into('<Q',row,8,context);struct.pack_into('<H',row,20,symbol);result.extend(row);accepted+=1;written+=1
   slot['cursor']=cursor;slot['accepted']=accepted
  returned=slot['accepted']-skip
  if returned<0:raise Refusal()
  self.outputs+=returned;self.check_stage('prepared:page',event)
  return dict(Returned=returned,Slot=q['slot'],NextChild=slot['start']+slot['cursor'],Accepted=slot['accepted'],Rows=result.hex().upper(),Extra=('00'*(returned*12)))
 def operation(self,event):
  kind=event['Kind'];args=event.get('Args');result=None
  if kind=='cursor':
   if args['Parent']and args['Parent']not in self.nodes:raise Refusal()
   self.begin(0,0,event);self.queries[args['Name']]=dict(args,slot=None)
  elif kind=='read':result=self.read(args,event)
  elif kind=='parallel-read':
   for n in range(args['Count']):self.read(dict(Name=args['Name'],Skip=0),{})
  elif kind in('root','child'):
   parent=0 if kind=='root'else args['Parent'];symbol=args if kind=='root'else args['Symbol']
   if parent and parent not in self.nodes:raise Refusal()
   self.begin(1,0,event);self.attempts+=1;result,created=self.resolve(parent,symbol);self.check_stage('prepared:resolved',event)
   if parent and created:self.nodes[result]['parent']=parent;self.check_stage('prepared:linked',event)
  elif kind=='capture':
   if self.checkpoints>=self.bounds[3]:raise Refusal()
   self.begin(0,0,event);pool=self.pool();self.checkpoints+=1;self.check_stage('prepared:captured',event);self.saved[args]=pool
  elif kind=='restore':
   self.begin(0,0,event);self.nodes,self.root,self.used,self.cursor=copy.deepcopy(self.saved[args]);self.epoch+=1;self.check_stage('prepared:restored',event)
  elif kind=='refresh':
   self.begin(0,0,event);permission=bytes.fromhex(args);assert len(permission)==217;self.working=[symbol if symbol>=217 or permission[symbol]&1 else 65535 for symbol in primary];self.epoch+=1;self.check_stage('prepared:refreshed',event)
  elif kind=='reset':
   self.begin(0,0,event)
   for slot in self.slots:slot['node']=-1;slot['accepted']=slot['cursor']=slot['block']=0
   self.nextslot=0;self.epoch+=1;self.check_stage('prepared:reset',event)
  elif kind=='guard-state':pass
  else:raise AssertionError(kind)
  self.check_stage('prepared:complete',event);return result
capture=json.loads(a.capture.read_bytes());assert not capture['ControllerRequestsAvailable']and not capture['TargetsRead']and not capture['VendorExecuted']
assert capture['MContextCountAfter']==86 and capture['CanceledPreparations']==5 and capture['Guards']==33
assert capture['AdditionalLogicalBytes']==(104+2048)*(1024+128*4)+8192*64+512*1024+65536
sessions={};compared=0;nodeReferences=0;rows=0;faults=0;preentry=0
for event in capture['Events']:
 name=event['Session'];kind=event['Kind']
 if kind=='fresh':sessions[name]=Session(event['Args']);expectedFailure=False;result=None
 else:
  s=sessions[name];before=s.operations
  try:result=s.operation(event);expectedFailure=False
  except Canceled:expectedFailure=True;result=None;s.fault=True;faults+=1
  except Refusal:
   expectedFailure=True;result=None
   if s.operations!=before:s.fault=True;faults+=1
   else:preentry+=1
  assert bool(event.get('Failure'))==expectedFailure,('failure',name,kind,event.get('Failure'))
  assert event.get('Result')==result,('result',name,kind,event.get('Args'))
 expected=sessions[name].snapshot();observed=event['State']
 assert expected==observed,('complete independent state',name,kind,next((k for k in expected if expected[k]!=observed[k]),None))
 nodeReferences+=len(expected['Nodes']);compared+=1
 if kind=='read'and result:rows+=result['Returned']
assert compared==188 and len(sessions['main'].nodes)==86 and sessions['main'].capacity==2 and preentry>=4 and faults>=10
result=dict(status='PASS independent fresh-query32/map/AVL/page/link/prefix oracle',stateEvents=compared,nodeReferences=nodeReferences,returnedPreparedRows=rows,mutatingFaultPrefixes=faults,preentryRefusals=preentry,actualInitialContexts=86,preparedGrowthBeyond104=True,M104Untouched=True,controllerRequestsAvailable=False,targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(result,indent=2)+'\n');print('PASS fresh preparation188 fullstates,',nodeReferences,'node references,',rows,'prepared32B rows,',faults,'ordered fault prefixes; no controller queries')
