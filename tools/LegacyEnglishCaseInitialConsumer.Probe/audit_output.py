from pathlib import Path
import argparse,json,struct,hashlib
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--parent',type=Path,required=True);p.add_argument('--geometry',type=Path);p.add_argument('--categories',type=Path);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
sha=lambda b:hashlib.sha256(b).hexdigest();u16=lambda b,o:struct.unpack_from('<H',b,o)[0];u64=lambda b,o:struct.unpack_from('<Q',b,o)[0];i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
raw=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();assert sha(raw)=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
# Pinned raw247 B8/B9, independently located by raw source parser proof.
assert raw[7953:7955]==bytes(2)
s=(a.repo/'tools/LegacyEnglishCaseMain.Probe/reference_case_main.py').read_text(encoding='utf-8-sig');fn=s[s.index('def alphabet('):s.index('lk=(a.models')].replace("if bitFlag:get('B',(count+7)//8)","symbolFlags=get('B',(count+7)//8) if bitFlag else ()").replace('return dict(count=count,','return dict(symbolFlags=symbolFlags,count=count,')
exec(compile(fn,'independent-raw-alphabet','exec'),globals());ac=alphabet(raw,99,7907);rev={v:k for k,v in ac['text'].items()};excluded={rev.get(x,65535)for x in ['\U000f0001','\u0651']};bypass={rev.get(x,65535)for x in ['\U000f0001','\U000f0002','\U000f0003','\U000f0004','\U000f0008']}
# Independently decode actual201 from raw model, including absolute padding
# and entry-major encounter-order composite interning. No library hex/capture.
pos=182477
assert u16(raw,pos)==0x201 and u16(raw,pos+2)==217;pos+=4
readIds=[]
for ordinal in range(2):
 count=raw[pos];pos+=1;pos+=(-pos)&3
 values=list(struct.unpack_from('<'+'i'*count,raw,pos));pos+=count*4
 if ordinal==0:
  firstIds=values;firstMap=list(struct.unpack_from('<220b',raw,pos));pos+=220
 else:geometryIds=values
startMap=list(struct.unpack_from('<220b',raw,pos));pos+=220
endMap=list(struct.unpack_from('<220b',raw,pos));pos+=220
prototypes=[]
for i in geometryIds:
 values=struct.unpack_from('<fffB',raw,pos);pos+=13
 assert all(__import__('math').isfinite(x)for x in values[:3]);prototypes.append(values)
assert pos==183584
pairs=[];maps=[[],[],[],[]];zero=firstIds.index(0)
for f,left,right in zip(firstMap,startMap,endMap):
 for index,(a0,b0)in enumerate([(f,left),(f,right),(zero,left),(zero,right)]):
  assert -1<=a0<len(firstIds) and -1<=b0<len(geometryIds)
  if a0==-1 or b0==-1:maps[index].append(-1);continue
  pair=[firstIds[a0],b0]
  if pair not in pairs:pairs.append(pair)
  maps[index].append(pairs.index(pair))
cats=dict(startComposite=maps[0],endComposite=maps[1],zeroStartComposite=maps[2],zeroEndComposite=maps[3],compositePairs=pairs)
if a.categories is not None:
 reference=json.loads(a.categories.read_bytes())
 for key,value in cats.items():assert reference[key]==value
geom={x['Name']:dict(x['Groups'])for x in json.loads(a.geometry.read_bytes())}if a.geometry is not None else None
parentPayload=json.loads(a.parent.read_bytes());assert parentPayload['TargetsRead']is False
parentRows=[r for r in parentPayload['Rows']if 'Pages'in r];parent={r['Name']:r for r in parentRows};assert len(parent)==len(parentRows)
cap=json.loads(a.capture.read_bytes());assert cap['TargetsRead']is False
assert cap['Scope']=='Actual first case290 primary allocation prefix only; no history/parent88/winner'
assert cap['InputSha256']==parentPayload['InputSha256']
assert len(cap['Rows'])>0 and len({r['Name']for r in cap['Rows']})==len(cap['Rows'])
assert set(parent)=={r['Name']for r in cap['Rows']} and cap['MultilineRefused']==parentPayload['MultilineRefused']
wrap=lambda x:((x+2**31)%2**32)-2**31
counts=dict(owners=0,rows=0,qualified=0,scoring=0,accepted=0,requestBytes=0,heapBytes=0,contextNodes=0)
for row in cap['Rows']:
 name=row['Name'];old=parent[name];nodes={n[0]:n for n in row['Nodes']};onodes={n[0]:n for n in old['Nodes']}
 def path(nodes,key):
  out=[];seen=set()
  while key:
   assert key in nodes and key not in seen;seen.add(key);n=nodes[key];out.append(n[5]);key=n[1]
  return tuple(reversed(out))
 # Complete AVL structural, height, symbol ordering, and separate parent ancestry.
 seen=set()
 def tree(key,lo=-1,hi=65536):
  if key==0:return 0
  assert key in nodes and key not in seen;seen.add(key);n=nodes[key];assert lo<n[5]<hi
  l=tree(n[3],lo,n[5]);r=tree(n[4],n[5],hi);assert n[6]==r-l and abs(n[6])<=1;return max(l,r)+1
 tree(row['Root'])
 for n in row['Nodes']:
  if n[2]:
   tree(n[2]);
   for other in nodes.values():
    if other[1]==n[0]:assert path(nodes,other[0])[:-1]==path(nodes,n[0])
 assert len(seen)==len(nodes);counts['contextNodes']+=len(nodes)
 def pages(owner,ns):
  out=[]
  for page in owner['Pages']:
   b=bytes.fromhex(page['Records']);assert len(b)==page['Count']*32
   for k in range(0,len(b),32):
    b32=b[k:k+32];out.append((b32[:8],path(ns,u64(b32,8)),b32[16:]))
  return out
 assert pages(row,nodes)==pages(old,onodes),name+' raw32 source/context mismatch'
 sourceRows=[]
 for page in row['Pages']:
  b=bytes.fromhex(page['Records']);sourceRows.extend(b[k:k+32]for k in range(0,len(b),32))
 # Actual corpus has one complete nonempty page (<128), then empty page:
 # all source lookup inserts precede consumer grafts. Reuse only the already
 # independently raw-audited query prefix as data; replay every child insertion
 # with a separate recursive HEIGHT AVL, not captured balance-state transitions.
 assert len(old['Pages'])==2 and 0<old['Pages'][0]['Count']<128 and old['Pages'][1]['Count']==0
 expected={n[0]:n.copy() for n in old['Nodes']}
 assert all(n[1]==0 and n[2]==0 for n in expected.values())
 fffa=next(n[0]for n in expected.values()if n[5]==0xfffa)
 def ht(k):return 0 if k==0 else 1+max(ht(expected[k][3]),ht(expected[k][4]))
 def balance(k):
  expected[k][6]=ht(expected[k][4])-ht(expected[k][3]);return k
 def left_rotate(k):
  right=expected[k][4];expected[k][4]=expected[right][3];expected[right][3]=k;balance(k);return balance(right)
 def right_rotate(k):
  left=expected[k][3];expected[k][3]=expected[left][4];expected[left][4]=k;balance(k);return balance(left)
 def add_child(k,symbol):
  if k==0:
   identity=len(expected)+1;slot=identity-1;expected[identity]=[identity,fffa,0,0,0,symbol,0,slot//102,slot%102];return identity
  if symbol<expected[k][5]:expected[k][3]=add_child(expected[k][3],symbol)
  elif symbol>expected[k][5]:expected[k][4]=add_child(expected[k][4],symbol)
  else:return k
  balance(k)
  if expected[k][6]>1:
   if symbol<expected[expected[k][4]][5]:expected[k][4]=right_rotate(expected[k][4])
   return left_rotate(k)
  if expected[k][6]<-1:
   if symbol>expected[expected[k][3]][5]:expected[k][3]=left_rotate(expected[k][3])
   return right_rotate(k)
  return k
 for b32 in sourceRows:
  symbol=u16(b32,20)
  if symbol not in excluded:expected[fffa][2]=add_child(expected[fffa][2],symbol)
 assert [expected[k]for k in sorted(expected)]==row['Nodes'],name+' all9 AVL fields/native graft allocation order'
 assert row['Root']==old['Root']
 selected=old['Selected'];trace=iter(row['Traces']);heap=[];calls=qualified=accepted=0
 def insert(record,cost):
  if len(heap)==250:
   if i32(heap[0],16)<=cost:return False
   target=0
   while target*2<len(heap)-1:
    child=target*2+1
    if child<len(heap)-1 and i32(heap[child],16)<=i32(heap[child+1],16):child+=1
    if i32(heap[child],16)<=cost:break
    heap[target]=heap[child];target=child
  else:
   target=len(heap);heap.append(None)
   while target:
    prev=(target-1)//2
    if cost<=i32(heap[prev],16):break
    heap[target]=heap[prev];target=prev
  heap[target]=record;return True
 for b32 in sourceRows:
  tag=u64(b32,0);symbol=u16(b32,20);base=i32(b32,16);assert b32[22:]==bytes(10)
  if symbol in excluded:continue
  if not any(s['Costs'][symbol]<0x4000000 for s in selected):continue
  qualified+=1
  contextPath=(0xfffa,symbol);ctx=next(n[0]for n in nodes.values()if path(nodes,n[0])==contextPath)
  for variant in range(2):
   category=cats['startComposite'if variant==0 else'endComposite'][symbol]
   if category<0:assert variant==1;break
   geometryIndex=cats['compositePairs'][category][1]
   for sr in selected:
    t=next(trace);calls+=1;assert t['Node']==0 and t['Arc']==sr['Ordinal'];assert t['ClassCosts']==sr['Costs'] and t['Group']==sr['Group'] and t['Span']==0
    # Actual root singleton source ranges, not a memorized zero cache. Native
    #193860 returns0 for exactly one ordinary range before numeric/table work.
    assert len(t['Ranges'])==1 and t['Ranges'][0][0]>=0 and 0<=t['Ranges'][0][1]<t['Ranges'][0][2]
    actualGeometry=0
    assert t['Geometry']==[actualGeometry]*24
    if geom is not None:assert geom[name][sr['Group']]==actualGeometry
    b=bytearray(80);mask=bytearray(80)
    def write(at,fmt,value):
     data=struct.pack('<'+fmt,value);b[at:at+len(data)]=data;mask[at:at+len(data)]=bytes([1])*len(data)
    for at,v in [(0,0),(4,-1),(8,-1),(20,-1),(24,-1),(32,0),(48,0),(52,0x4000000),(64,6),(68,0)]:write(at,'i',v)
    for at,v in [(12,sr['Ordinal']),(14,65535),(16,65535),(36,symbol),(38,65535)]:write(at,'H',v)
    for at,v in [(18,geometryIndex),(19,255),(28,0),(56,0),(57,0),(58,0),(72,tag>>56&1),(73,0)]:write(at,'B',v)
    write(40,'Q',0)
    assert bytes.fromhex(t['Mask'])==mask and bytes.fromhex(t['Bytes'])==b and t['BaseCost']==base,name+' masked80 mismatch';counts['requestBytes']+=80
    cc=sr['Costs'][symbol];total=0x4000000 if cc==0x4000000 else wrap(base+cc+(0 if symbol in bypass else actualGeometry))
    if cc==0x4000000 or total>=0x4000000:continue
    rec=bytearray(128);struct.pack_into('<Q',rec,0,tag);rec[5]=(tag>>40&0x7d)|((tag&0x20000000000)!=0)*2
    flags=ac['symbolFlags'];flagged=bool(flags and flags[symbol>>3]&(1<<(symbol&7)));rec[5]|=int(flagged)<<7;rec[6]=tag>>48&0xfc
    struct.pack_into('<QiiIHH',rec,8,ctx,total,wrap(total-base),category<<16|sr['Ordinal']<<22,sr['Ordinal'],65535)
    struct.pack_into('<HH',rec,34,65535,symbol);struct.pack_into('<Q',rec,40,2**64-1);struct.pack_into('<i',rec,48,-1)
    # Exact actual source B8 getter value from same private selected graph.
    struct.pack_into('<iiHH',rec,52,wrap(t['StartContext']),0,65535,65535)
    struct.pack_into('<Q',rec,88,2**64-1);struct.pack_into('<i',rec,104,-1)
    accepted+=insert(bytes(rec),total)
 try:next(trace);raise AssertionError('extra trace')
 except StopIteration:pass
 assert (len(sourceRows),qualified,calls,accepted)==(row['Rows'],row['Qualified'],row['Scoring'],row['HeapAccepted']),name+' counters'
 assert row['HeapAttempts']==sum(1 for t in row['Traces']if (lambda cc,b,g:cc!=0x4000000 and wrap(b+cc+g)<0x4000000)(t['ClassCosts'][u16(bytes.fromhex(t['Bytes']),36)],t['BaseCost'],0 if u16(bytes.fromhex(t['Bytes']),36)in bypass else t['Geometry'][bytes.fromhex(t['Bytes'])[18]]))
 actual=[bytes.fromhex(x)for x in row['Heap']]
 assert actual==heap,name+' full128 allocator/prefix/tail/order mismatch'
 assert bytes.fromhex(row['SlotMask'])==bytes([1])*120+bytes(8)
 for k,v in [('owners',1),('rows',len(sourceRows)),('qualified',qualified),('scoring',calls),('accepted',accepted),('heapBytes',128*len(heap))]:counts[k]+=v
if 'GuardNames'in cap:
 assert cap['Guards']==len(cap['GuardNames']) and cap['Cancellations']==len(cap['CancelNames'])
 if any(r['Name']=='actual-0' for r in cap['Rows']):
  assert cap['Guards']==58 and cap['Cancellations']==17
  assert {'preentry-historical-rejection-retry','preentry-malformed-rejection-retry','active-foreign-beforeissuer-retry','active-malformed-beforeissuer-retry','equal-byte-foreign-lease','active-zero-state','masked-unwritten-read','masked-illegal-write','mid-acoustic-cancel','query-blocks-score','query-blocks-D0','query-blocks-snapshot'}<=set(cap['GuardNames'])
result=dict(status='PASS independent full raw32/masked80/heap128/category/cost/order reference',captureSha256=sha(a.capture.read_bytes()),parentSha256=sha(a.parent.read_bytes()),geometrySha256=sha(a.geometry.read_bytes())if a.geometry is not None else None,categorySourceStart=182477,categorySourceEnd=pos,targetsRead=False,vendorExecuted=False,**counts)
a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))
