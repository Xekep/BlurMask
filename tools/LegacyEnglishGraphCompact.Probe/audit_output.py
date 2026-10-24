"""Independent prepared marked207CB0 composition; no actual query/owner claim."""
from pathlib import Path
import ast,base64,hashlib,json,math,struct,sys
D=Path(__file__).resolve().parent
REF={'native_sequence.py':'4587f2d20a5bfbac7209708649fc0b69977f644bda4640fcef8ee14d1ff0ba2c','native_primary_flags_oracle.py':'0ba028a3c582a4005c66f581e1bcd39c1f6f0da052c117d448b178f1d332a0a1','old_raw_compact_oracle.py':'ed1326e3f0bcdf3ece754a1ab2d90c24cbe595f23b7ae530db6b2e808900a997'}
def defs(name,names,g=None):
 p=D/'reference'/name;raw=p.read_bytes();assert hashlib.sha256(raw).hexdigest()==REF[name]
 t=ast.parse(raw.decode('utf-8-sig'));out=dict(g or {});items=[]
 for n in t.body:
  if isinstance(n,ast.FunctionDef)and n.name in names:items.append(n)
  if isinstance(n,ast.Assign)and any(isinstance(x,ast.Name)and x.id in names for x in n.targets):items.append(n)
 exec(compile(ast.Module(body=items,type_ignores=[]),str(p),'exec'),out);return out
base=defs('old_raw_compact_oracle.py',{'u16','u32','i32','u64','s8','w16','w32','f','wrap','read_alphabet'},{'struct':struct})
seq=defs('native_sequence.py',{'TABLE','u16','i32','w16','w32','sentinel','normalize'},{'struct':struct})
choose=defs('native_primary_flags_oracle.py',{'choose'})['choose']
u16,i32,u64,s8,w16,w32,f,wrap=[base[n]for n in ['u16','i32','u64','s8','w16','w32','f','wrap']]
def metadata(ak):
 assert hashlib.sha256(ak).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
 al=base['read_alphabet'](ak);p=182477;assert struct.unpack_from('<HH',ak,p)==(0x201,217);p+=4
 def ids():
  nonlocal p
  n=ak[p];p=(p+4)&~3;x=list(struct.unpack_from('<'+'i'*n,ak,p));p+=n*4;return x
 first=ids();fm=struct.unpack_from('<220b',ak,p);p+=220;geometry=ids();start=struct.unpack_from('<220b',ak,p);p+=220;end=struct.unpack_from('<220b',ak,p);zero=first.index(0);pairs=[]
 for a,b,c in zip(fm,start,end):
  for x,y in ((a,b),(a,c),(zero,b),(zero,c)):
   if x!=-1 and y!=-1 and(first[x],y)not in pairs:pairs.append((first[x],y))
 assert len(pairs)==42
 maps=[pairs.index((first[a],b))if a!=-1 and b!=-1 else 255 for a,b in zip(fm,start)]
 return al,maps

def expected(sample,actual,al,startmap):
 N=al['label_count'];labels={v:k for k,v in al['mapping'].items()};knowledge=al['fields'][3];special_de=labels.get('\u200c',65535);special_e2=labels.get('\u0651',65535);closed=labels['\U000f0001'];reserved={N,*al['fields'][4:7]}
 ids=actual['ContextMap'];assert len(ids)==5 and all(type(x)is int and x>0 for x in ids)and len(set(ids))==5
 context={x['Identity']:(x['ParentLink'],x['Symbol'])for x in actual['Contexts']};assert len(context)==len(actual['Contexts'])==5
 logical={}
 for n,path in enumerate(sample['ContextPaths'],1):
  parent=0
  for symbol in path:
   candidates=[key for key,(pa,sy)in context.items()if pa==parent and sy==symbol];assert len(candidates)==1;parent=candidates[0]
  assert ids[n-1]==parent;logical[n]=parent
 for n,c in enumerate(actual['Contexts']):
  assert c['PageIndex']==0 and c['PageSlot']==n and type(c['Balance'])is int and -1<=c['Balance']<=1
  assert c['Identity']==ids[n]
  for field in ['ParentLink','ChildTreeRoot','Left','Right']:assert type(c[field])is int and(c[field]==0 or c[field]in context)
 rows=[bytearray.fromhex(x['Rows'])for x in sample['Nodes']];pools=[bytearray.fromhex(x['Primary'])for x in sample['Nodes']];limits=[x['SourceEnd']for x in sample['Nodes']];graph=[[[0,0,0,1]],[[1,0,1,2]]]
 for node in range(2):
  for o in range(limits[node]):
   at=o*88
   if i32(rows[node],at+16)>=0x4000000:continue
   struct.pack_into('<Q',rows[node],at+8,logical[u64(rows[node],at+8)])
   if o>=125:continue
   for k in range(2):
    p=o*80+k*40
    if i32(pools[node],p)>=0x4000000:break
    for offset in [8,16]:struct.pack_into('<Q',pools[node],p+offset,logical[u64(pools[node],p+offset)])
 def path(identity):
  out=[];seen=set()
  while identity:
   assert identity not in seen and len(seen)<50;seen.add(identity);out.append(identity);identity=context[identity][0]
  return out[::-1]
 weights=[];weightbits=[]
 for candidates,scale in zip(sample['Candidates'],sample['Scales'],strict=True):
  scale=f(scale);values=[];total=0.
  if len(candidates)==1:values=[1.]
  else:
   for _,cost in candidates[:20]:value=f(math.exp(float(f(-f(cost)/f(scale*8192.)))));values.append(value);total=f(total+value)
   values=[f(v/total)for v in values]+[0.]*(len(candidates)-len(values))
  weights.append(dict(zip((x[0]for x in candidates),values)));weightbits.append([struct.unpack('<i',struct.pack('<f',x))[0]for x in values])
 def score(node,arc,symbol):return weights[graph[node][arc][0]].get(symbol,0.)
 def score_byte(node,row):
  symbol=u16(row,36)
  if symbol==65535:return 255
  if u16(row,24)!=node and row[5]&128:symbol=N
  return max(1,min(255,int(f(score(node,u16(row,28),symbol)*255.))))
 def frame(node,row,current,previous,difference):
  raw=bytearray(944)
  if u16(row,24)!=node and row[5]&128:delta=[knowledge]
  else:
   a,b=path(current),path(previous);i=0
   while i<min(len(a),len(b))and a[i]==b[i]:i+=1
   delta=[context[x][1]for x in a[i:]]
  w32(raw,0,len(delta));control=0
  for i,symbol in enumerate(delta):
   at=4+i*12;w16(raw,at,symbol);kind=3 if((symbol+21)&65535)<19 or symbol in(special_de,65535)else 0;raw[at+2]=kind;control+=kind==3;w32(raw,at+4,-1)
  w32(raw,0x3ac,control);w32(raw,0x3a4,difference);w32(raw,0x25c,1);struct.pack_into('<Q',raw,0x260,node|(u16(row,28)<<24))
  out=seq['normalize'](raw,False);assert 1<=i32(out,0)<=50
  return[(u16(out,4+i*12),out[6+i*12],u64(out,0x260+i32(out,8+i*12)*16))for i in range(i32(out,0))]
 storage=bytearray([167])*(4096*40);count=0;starts=[];probes=[]
 def write(tag,coord,parent,cost,inc,symbol,alt,flags,value,aux):
  nonlocal count
  at=count*40;assert at+40<=len(storage);struct.pack_into('<QQiiiHHBB',storage,at,tag,coord,parent,wrap(cost),wrap(inc),symbol,alt,flags,value);w32(storage,at+36,aux);count+=1;return count-1
 def append(items,row,node,tag,parent,cost,inc,alt,b3,b4,value):
  for j,(symbol,kind,coord)in enumerate(items):
   final=j==len(items)-1;terminal6=False
   if final:terminal6=symbol in(N+1,N+2)or not(u16(row,24)!=node and row[5]&128)and symbol not in(special_e2,closed,knowledge)and symbol<65515 and row[26]&63!=startmap[symbol]
   parent=write(tag,coord,parent,cost,inc,symbol,alt if final else 65535,(kind&7)|(64 if terminal6 else 0)|(8 if final and b3 else 0)|(16 if final and b4 else 0),value,-1)
  return parent
 def pool(n,o,k):return memoryview(pools[n])[(o*2+k)*40:(o*2+k+1)*40]
 def beam(n,o):return memoryview(rows[n])[o*88:(o+1)*88]
 def previous(n,relative):return -1 if relative==65535 else starts[n]+relative
 def probe(n,o):
  r=beam(n,o);value=bool(r[5]&2)or sample['Replies'][o%2];probes.append([n,o,int(value)]);return value
 product=f(f(sample['Penalty'])*f(.8));penalty=int(f(product+(.5 if product>0 else-.5)))
 for node in range(2):
  starts.append(count);used=set();candidates=[]
  for o in range(125):
   for k in range(2):
    p=pool(node,o,k)
    if not p[27]&128:continue
    r=beam(node,o);arc=u16(r,28);assert r[4]==1 and i32(r,48)==-1 and r[7]==0
    candidates.append(dict(o=o,k=k,cost=i32(p,0),inc=i32(p,4),Tag=u64(r,0),Context=u64(p,16),key=i32(r,52),end=graph[node][arc][3],Auxiliary=i32(r,48),Fallback=bool(u64(r,0)&(1<<41)),Bit3=False,Bit4=False))
  def compare(a,b):
   for field in('key','end','cost'):
    delta=wrap(a[field]-b[field])
    if delta:return delta
   return(context[a['Context']][1]if a['Context']else 65535)-(context[b['Context']][1]if b['Context']else 65535)
  def sort(first,last):
   while True:
    left,right=first,last;pivot=candidates[(first+last)//2]
    while True:
     while compare(candidates[left],pivot)<0:left+=1
     while compare(candidates[right],pivot)>0:right-=1
     if left>right:break
     candidates[left],candidates[right]=candidates[right],candidates[left];left+=1;right-=1
     if left>right:break
    if first<right:sort(first,right)
    if last<=left:return
    first=left
  if candidates:sort(0,len(candidates)-1)
  group_start=selected=0
  for index,item in enumerate(candidates):
   if index==0 or(item['key'],item['end'])!=(candidates[index-1]['key'],candidates[index-1]['end']):group_start=index;selected=0
   decision=choose(candidates,index,group_start,selected,2,sample['Headers'],lambda:probe(node,item['o']));selected=decision['SelectedDistinctAfter'];item['Bit3']=decision['TerminalBit3'];item['Bit4']=decision['TerminalBit4']
   r=beam(node,item['o']);p=pool(node,item['o'],item['k']);pn=node-s8(p,26)-1;po=u16(p,24);pa=p[27]&127;reset=bool(p[31]&32);assert pn<node and not p[31]&128
   prev_ctx=0 if pn<0 or reset else u64(pool(pn,po,pa),16);parent=-1 if pn<0 or reset else previous(pn,u16(pool(pn,po,pa),28));glyph=p[31]&31
   terminal=append(frame(node,r,item['Context'],prev_ctx,glyph),r,node,item['Tag'],parent,item['cost']+(penalty if item['Bit4']else 0),item['inc'],65535,item['Bit3'],item['Bit4'],score_byte(node,r));w16(p,28,terminal-starts[node]);used.add(u16(r,28))
  for o in range(limits[node]):
   r=beam(node,o)
   if not r[6]&2:continue
   arc=u16(r,28);used.add(arc);pn=node-s8(r,32)-1;po=u16(r,30);alt=65535 if o<125 else 65534;parent=-1 if pn<0 else previous(pn,u16(beam(pn,po),34));glyph=r[33]&63
   if pn>=0 and po>=125:glyph=(glyph-(beam(pn,po)[33]&63))&255
   terminal=append(frame(node,r,u64(r,8),0,glyph),r,node,u64(r,0),parent,i32(r,16),i32(r,20),alt,False,False,score_byte(node,r));w16(r,34,terminal-starts[node])
  for arc in sorted(used):
   group=graph[node][arc][0];emitted={}
   for symbol,_ in sample['Candidates'][group]:
    if symbol==65535:break
    if symbol in reserved:continue
    delta=int(f(weights[group][symbol]*255.))
    if symbol in emitted:
     at=emitted[symbol]*40+33;storage[at]=max(1,min(255,storage[at]+delta))
    else:
     emitted[symbol]=write(18446744073709551615,node|(arc<<24),-1,0,0,symbol,65535,32,max(1,min(255,delta)),-1)
     if len(emitted)==2:break
 starts.append(count)
 final=beam(1,125);selected_index=starts[1]+u16(final,34)
 winner=dict(HasWinner=True,Ordinal=125,RelativeCost=i32(final,16),TotalCost=i32(final,16),RowCost14=i32(final,20),RowField22=u16(final,34),FallbackFlag=0,ResultFlag8=0)
 return dict(Count=count,Starts=starts,PrefixOwners=[-1,-1,1],Storage=storage.hex(),Nodes=[dict(Rows=x.hex(),Primary=y.hex())for x,y in zip(rows,pools,strict=True)],WeightsBits=weightbits,ProbeCalls=probes,Winner=winner,Selected=selected_index)

def backtrack(expected,al):
 p=D/'reference/native_backtrack.py';raw=p.read_bytes();assert hashlib.sha256(raw).hexdigest()=='fe9a3cdd6571b0d1780a486c7520d20f25b669c2b98a6dafb6655ccf23d33308'
 # Exact additive AL-count adaptation of the old192-label oracle. Preserve
 # its immutable bytes; native symbolCount+1 replaces only the literal193.
 text=raw.decode('utf-8-sig');assert text.count('normalized==193')==1;text=text.replace('normalized==193','normalized==N+1')
 labels={v:k for k,v in al['mapping'].items()};compact=bytes.fromhex(expected['Storage'])[:expected['Count']*40]
 g={'struct':struct,'p':{'recordCount':expected['Count'],'selected':expected['Selected'],'alternative':65535},'starts':expected['Starts'],'compact':compact,
 'graph':[[[0,0,0,1]],[[1,0,1,2]]],'labels':labels,'N':al['label_count'],'marker':labels.get('\x03',65535),'joiner':labels.get('\u200d',65535),'soft':labels.get('\xad',65535),'unit':al['fields'][3]}
 t=ast.parse(text);exec(compile(ast.Module(body=[x for x in t.body if isinstance(x,ast.FunctionDef)],type_ignores=[]),str(p),'exec'),g)
 rows,visited,residuals=g['expected'](bytes([0x5d])*72)
 return dict(Rows=rows.hex(),Visited=visited,Residuals=residuals)

def validate(fixtures,output,al,startmap):
 assert set(output)=={'Scope','NNCalls','Cases','Budgets'}and output['Scope']=='PREPARED_MARKED207CB0_2257_21FFD0_NUMERIC_NOT_LIVE_OWNER'and type(output['NNCalls'])is int and output['NNCalls']==0
 assert len(output['Cases'])==len(fixtures['Cases'])==64
 total=bit3=bit4=0
 for sample,actual in zip(fixtures['Cases'],output['Cases'],strict=True):
  assert type(actual['Id'])is int and actual['Id']==sample['Id']
  e=expected(sample,actual,al,startmap)
  assert set(actual)==set(e)|{'Id','ContextMap','Contexts','Backtrack'}
  wanted_backtrack=backtrack(e,al);got_backtrack=actual['Backtrack'];assert set(got_backtrack)==set(wanted_backtrack)
  assert bytes.fromhex(got_backtrack['Rows'])==bytes.fromhex(wanted_backtrack['Rows'])and got_backtrack['Visited']==wanted_backtrack['Visited']and got_backtrack['Residuals']==wanted_backtrack['Residuals']
  assert all(type(v)is int for v in got_backtrack['Visited']+got_backtrack['Residuals'])
  for field,value in e['Winner'].items():assert type(actual['Winner'][field])is type(value)
  assert all(type(v)is int for row in actual['ProbeCalls']+actual['WeightsBits']for v in row)
  for key,value in e.items():
   if key=='Storage':assert bytes.fromhex(actual[key])==bytes.fromhex(value),(sample['Id'],key)
   elif key=='Nodes':
    assert len(actual[key])==len(value)==2
    for a,b in zip(actual[key],value,strict=True):
     assert set(a)==set(b)
     for k in a:assert bytes.fromhex(a[k])==bytes.fromhex(b[k]),(sample['Id'],key,k)
   else:assert actual[key]==value,(sample['Id'],key,actual[key],value)
  assert type(actual['Count'])is int and all(type(x)is int for x in actual['Starts'])
  raw=bytes.fromhex(actual['Storage']);total+=actual['Count'];bit3+=sum(bool(raw[i*40+32]&8)for i in range(actual['Count']));bit4+=sum(bool(raw[i*40+32]&16)for i in range(actual['Count']))
 assert len(output['Budgets'])==len(fixtures['Budgets'])==18
 for b,o in zip(fixtures['Budgets'],output['Budgets'],strict=True):
  payload=b['Records']*240+b['Nodes']*(12*(156*88+125*80)+4096)+b['Arcs']*8192+65536;copy=payload+b['Nodes']*2*(156*88+125*80)
  assert(o['RecordCapacity'],o['AllocationBytes'],o['CopyBytes'])==(b['Records'],payload,copy)
  for name,target,cap,current,overhead in [('Payload',payload,9*1024*1024,1584,0),('Copies',copy,64*1024*1024,64,16)]:
   result=o[name];over=target>cap-current-overhead
   assert result['OverCap']is over and result['ExactAccepted']is(not over)and result['OneShortRefused']is(not over)and result['Before']==result['After']and result['Exception']=='System.IO.InvalidDataException'
 return dict(cases=64,compactRecords=total,fullStorageBytes=64*4096*40,fullNodeBytes=64*2*(156*88+125*80),bit3=bit3,bit4=bit4,budgetControls=18,NNCalls=0,scope='PREPARED numeric full207CB0/2257 only; no actual stage/query authority')

if __name__=='__main__':
 if len(sys.argv)not in(4,5):raise SystemExit('AK fixtures capture [--tamper]')
 ak=Path(sys.argv[1]).read_bytes();b=Path(sys.argv[2]).read_bytes();assert hashlib.sha256(b).hexdigest()=='74e2e74d49e902e1a2e84180826d8563e8ef4c459cd94301382680a6ca1b2ba7';fixtures=json.loads(b);output=json.loads(Path(sys.argv[3]).read_bytes());al,startmap=metadata(ak);report=validate(fixtures,output,al,startmap);negatives=[]
 if len(sys.argv)==5:
  assert sys.argv[4]=='--tamper'
  for field in ['Storage','Count','Starts','PrefixOwners','Selected','ProbeCalls','WeightsBits','Winner','Nodes','Backtrack']:
   case=output['Cases'][0];old=case[field]
   if field=='Storage':case[field]=('00'if old[:2]!='00'else'ff')+old[2:]
   elif field in('Count','Selected'):case[field]=old+1
   elif field=='Winner':case[field]=dict(old,Ordinal=0)
   elif field=='Nodes':case[field]=old[:1]
   elif field=='Backtrack':case[field]=dict(old,Visited=[])
   elif field in('ProbeCalls','WeightsBits'):case[field]=[]
   else:case[field]=[False]+old[1:]
   try:
    try:validate(fixtures,output,al,startmap)
    except(AssertionError,ValueError,TypeError,KeyError,IndexError):negatives.append(field)
    else:raise AssertionError('tamper survived:'+field)
   finally:case[field]=old
  old=output['Cases'];output['Cases']=old+[old[-1]]
  try:
   try:validate(fixtures,output,al,startmap)
   except AssertionError:negatives.append('extra-case')
   else:raise AssertionError('extra-case survived')
  finally:output['Cases']=old
 report['tamperRefusals']=negatives;print(json.dumps(report,indent=2))
