"""Raw-resource category/AL and full40-byte, ordered-event21F320 oracle.
Prepared WORDs/callback maps are not recognized text or an enabled controller.
"""
from pathlib import Path
import base64,hashlib,json,struct,sys
if len(sys.argv)!=3:raise SystemExit('models-directory capture.json')

def parse_alphabet(data,start):
 """Independent bounded AL103/104 forward trie; no research/runtime imports."""
 if not 0<=start<len(data)<=64*1024*1024:raise ValueError('AL resource bounds')
 at=start
 def read(fmt,n=1):
  nonlocal at
  if not 0<=n<=65536:raise ValueError('AL table count')
  size=struct.calcsize('<'+fmt)*n
  if size>len(data)-at:raise ValueError('Truncated AL table')
  values=struct.unpack_from('<'+fmt*n,data,at);at+=size;return values
 count=read('H',8)[-1]
 if not 0<count<65536:raise ValueError('AL label count')
 n=read('H')[0]
 for increment in (32,64,1):
  if not 0<n<65536:raise ValueError('AL trie dimension')
  table=read('H',n)
  last=next((v for v in reversed(table) if v!=65535),None)
  if last is None:raise ValueError('AL trie has no continuation')
  n=last+increment
 if not 0<n<65536:raise ValueError('AL span dimension')
 spans=read('H',n+1)
 if any(a>b for a,b in zip(spans,spans[1:])):raise ValueError('AL unordered spans')
 entries=[read('I',2) for _ in range(spans[-1]+1)]
 length=entries[-1][0]
 if entries[0][0]!=0 or length>16*1024*1024 or length>len(data)-at:raise ValueError('AL string bounds')
 mapping={};inverse={}
 for (begin,label),(end,_) in zip(entries,entries[1:]):
  label&=65535
  if not 0<=begin<end<=length or label>=count:raise ValueError('AL label/string entry')
  text=data[at+begin:at+end].decode('utf8',errors='strict')
  if label in mapping and mapping[label]!=text:raise ValueError('AL conflicting label')
  if text in inverse and inverse[text]!=label:raise ValueError('AL ambiguous string')
  mapping[label]=text;inverse[text]=label
 return {'label_count':count,'mapping':mapping}

c=json.loads(Path(sys.argv[2]).read_bytes());models=Path(sys.argv[1]);D=lambda s:base64.b64decode(s,validate=True)
U=lambda b,o:struct.unpack_from('<H',b,o)[0]
I=lambda b,o:struct.unpack_from('<i',b,o)[0]
samplers={'en-US/en_US-ak-cur.lite.res':182412,'en-US/en_US-ak-cur.res':374182,'en-US/en_US-ak-superimposed.lite.res':207622,'en-US/en_US-ak-superimposed.res':207622,'ru-RU/ru_RU-ak-cur.lite.res':188157}
manifest={r['path']:r['sha256'] for r in json.loads((models/'manifest.json').read_bytes())['resources']}
def categories(data,at,count):
 assert U(data,at)==0x201 and U(data,at+2)==count
 p=at+4
 def ids():
  nonlocal p
  n=data[p];p+=1;p=(p+3)&~3
  values=list(struct.unpack_from('<'+'i'*n,data,p));p+=4*n;return values
 first=ids();fm=struct.unpack_from('<'+'b'*(count+3),data,p);p+=count+3
 geo=ids();gm=struct.unpack_from('<'+'b'*(count+3),data,p)
 return [None if a<0 or b<0 else first[a] for a,b in zip(fm,gm)][:count]
class Stop(Exception):
 def __init__(self,error):self.error=error
def oracle(row,labels,cats,count,profile):
 for vector,header in zip(row['vectors'],row['wordHeaders']):
  b=D(header);assert len(b)==120 and I(b,40)==I(b,44)==len(vector)
 state={n['key']:[bytearray(D(n['records'])[p:p+40]) for p in range(0,len(D(n['records'])),40)] for n in row['before']}
 mask=((1<<40)-1)|(1<<56)
 def record(ref):return state[ref&mask][(ref>>40)&65535]
 events=[];queries=[];wpos=ordinal=0
 apostrophe,quote,opend,closed,opens,closes,low=labels
 if 'preparedField24be' in row:low=row['preparedField24be']
 def emit(op,read,value,extra=0):
  events.append([op,*read[2:4],read[1],value,extra])
  if row['failEvent'] and len(events)==row['failEvent']:raise Stop('InvalidOperationException')
  if row['cancelEvent'] and len(events)==row['cancelEvent']:raise Stop('OperationCanceledException')
 def next_read():
  nonlocal wpos,ordinal
  while wpos<len(row['vectors']) and ordinal==len(row['vectors'][wpos]):wpos+=1;ordinal=0
  if wpos==len(row['vectors']):result=[65535,0,wpos,0]
  else:
   ref=row['vectors'][wpos][ordinal];n=ordinal;ordinal+=1;r=record(ref);kind=U(r,4)&7;label=U(r,8)
   if kind==3:
    if label&0x7ffc==0 or label&3==1:raise Stop('NotSupportedException')
    label={0:0,2:1,3:2}[label&3] #the explicitly prepared live TOP2A inserts LF/space/NBSP
   elif kind==1:label=((label&3)-7)&65535
   elif kind not in (0,2,4):label=65535
   result=[label,ref,wpos,n]
  emit('Read',result,result[0],int(wpos<len(row['vectors'])));return result
 def kind(read):
  arc=(read[1]>>24)&65535
  if arc&0x8000:return 255
  node=read[1]&0xffffff;node=node-(1<<24) if node&(1<<23) else node
  queries.append([node,arc])
  if row['failQuery'] and len(queries)==row['failQuery']:raise Stop('InvalidOperationException')
  key=(node&0xffffff)|(arc<<24)
  value=next(v for k,v in row['kindMap'] if k&((1<<40)-1)==key)
  emit('Kind',read,value);return value
 def write(read,value):
  r=record(read[1]);before=U(r,8);struct.pack_into('<H',r,8,value);emit('Write',read,value,before)
 def control(label):return 65529<=label<=65532
 def ordinary(label):return label in (apostrophe,quote)
 def opening(label):return opens if label==apostrophe else opend
 def closing(label):return closes if label==apostrophe else closed
 error=None
 try:
  if row['preCanceled']:raise Stop('OperationCanceledException')
  if row['f4'] and any(v!=65535 for v in (closes,opens,closed)):
   first=next_read();current=next_read();following=next_read();carry=low!=65535 and first[0]==low
   previous=first[0]
   if kind(first)==0 and ordinary(first[0]) and not control(current[0]):previous=opening(first[0]);write(first,previous)
   emit('Data',[0,0,-1,-1],profile)
   while following[0]!=65535:
    value=current[0]
    if kind(current)==0 and ordinary(value):
     emit('Count',current,count)
     category=-1
     if previous<count:
      category=cats[previous]
      if category is None:raise Stop('NotSupportedException')
      emit('Category',current,category,previous)
     #Independent neighbor truth table; ordered mutation, not a call to the C# rule.
     if category==1:value=opening(value);write(current,value)
     elif not control(previous):
      value=closing(value);write(current,value)
      if carry and value==closed:value=opend;write(current,value);carry=False
     elif not control(following[0]):value=opening(value);write(current,value)
    current[0]=value;fetched=next_read();carry=carry or (low!=65535 and value==low)
    previous=value;current=following;following=fetched
   if kind(current)==0 and ordinary(current[0]) and not control(previous):
    value=closing(current[0]);write(current,value)
    if carry and value==closed:write(current,opend)
 except Stop as ex:error=ex.error
 assert row['error']==error,(row['seed'],row.get('preparedField24be'),row['error'],error)
 assert row['queries']==queries,(row['seed'],'queries',row['queries'],queries)
 assert row['events']==events,(row['seed'],'events',next(((a,b) for a,b in zip(row['events'],events) if a!=b),None),len(row['events']),len(events))
 for n in row['after']:
  assert D(n['records'])==b''.join(state[n['key']]),(row['seed'],n['key'],'full40bytes')
  original=next(p for p in row['before'] if p['key']==n['key'])
  assert n['packed']==original['packed']
  for at,(a,b) in enumerate(zip(D(original['records']),D(n['records']))):assert a==b or at%40 in (8,9)
 return events,queries,error
total=changed=failed=double=half=rawalt=controls=0
for source in c['sources']:
 data=(models/source['path']).read_bytes();assert hashlib.sha256(data).hexdigest()==source['sha256'].lower()==manifest[source['path']]
 sampler=samplers[source['path']];assert source['profile']==sampler+65
 assert struct.unpack_from('<I',data,sampler+8)[0]==0x297 and U(data,sampler+14) in (0x106,0x108) and data[sampler+64]==1
 a=parse_alphabet(data,99);inv={text:label for label,text in a['mapping'].items()};assert source['count']==a['label_count']
 labels=[inv.get(s,65535) for s in ["'",'"','\u201c','\u201d','\u2018','\u2019','\u201e']]
 assert source['labels']==labels and labels[-1]==65535
 cats=categories(data,source['profile'],a['label_count']);assert source['categories']==cats
 assert len(source['cases'])==273 and len(source['preparedCarryControls'])==38
 for group in ('cases','preparedCarryControls'):
  for row in source[group]:
   assert ('preparedField24be'in row)==(group=='preparedCarryControls')
   events,queries,error=oracle(row,labels,cats,a['label_count'],source['profile'])
   changes=sum(D(x['records'])!=D(y['records']) for x,y in zip(row['before'],row['after']))
   changed+=changes>0;failed+=error is not None;total+=1
   writes=[e for e in events if e[0]=='Write']
   double+=sum(x[3]==y[3] and x[4]==labels[3] and y[4]==labels[2] for x,y in zip(writes,writes[1:]))
   if group=='preparedCarryControls' and error and writes and writes[-1][4]==labels[3]:half+=1
   if group=='cases' and row['seed']==8 and labels[3]!=65535:
    assert error is None and writes;rawalt+=1
   if group=='cases' and row['seed']==9 and labels[3]!=65535:
    assert any(e[0]=='Read' and e[4]==1 for e in events);controls+=1
assert total==c['cases']==1555 and changed==c['changed'] and failed==c['failed'] and c['guards']==1570
assert double>0 and half>0 and rawalt==4 and controls==4
print(f'PASS {total} prepared linked WORD drivers/full40byte pools/query+orderedwrite+read traces;{double} prepared double stores/{half} stopped half-write prefixes;4 English kind4 raw-ID+4 live-control namespace cases; no enabled main path')
