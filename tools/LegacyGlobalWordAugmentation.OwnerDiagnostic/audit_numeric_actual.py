"""Independent actual219580 ordinary/noaux replay. DATA only, no engine or NN.
Scope: complete observed ordinary chains; pool symbols/publication fields are
actual producer inputs, not an independent NN/glyph-production reference.
Native219580 costs/new-only update;13F970 ordinary equality;13D360/140B90.
"""
import base64,copy,json,math,struct,sys
from pathlib import Path
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
rf=lambda b,o:struct.unpack_from('<f',b,o)[0]
def w16(b,o,v):struct.pack_into('<H',b,o,v)
def w32(b,o,v):struct.pack_into('<i',b,o,v)
def wf(b,o,v):struct.pack_into('<f',b,o,f(v))
def wu64(b,o,v):struct.pack_into('<Q',b,o,v)
def key(ref):return ref&0xffffffffff|ref&(1<<56)
def decode_node(ref):
 n=ref&0xffffff
 return n-(1<<24) if n&0x800000 else n

def replay(a,trace,raw):
 assert a['Scenario']=='new-success' and a['NNSourceInvocations']==1
 data=base64.b64decode(a['RawCompact']);n=len(data)//40;assert len(data)==n*40
 assert data==base64.b64decode(raw['compact'])[:raw['count']*40]
 starts=raw['starts'];graph=trace['publications'][0]['graph'];before=a['BeforeState'];after=a['AfterState']
 first,last=before['SpanFirst'],before['SpanLast'];minimum=first>>7;lastnode=last>>7
 assert first>=0 and last>=first and after['SpanFirst']==first and after['SpanLast']==last
 assert before['SelectedOrdinals']==[0] and len(before['Words'])==1
 def row(i):assert 0<=i<n;return data[i*40:i*40+40]
 def pool(state):
  out={}
  for node in state['Pool']:
   value=base64.b64decode(node['Records']);assert len(value)%40==0
   for ordinal in range(len(value)//40):out[node['Key']|(ordinal<<40)]=value[ordinal*40:ordinal*40+40]
  return out
 p0,p1=pool(before),pool(after)
 # Quote/normalization leaves original pool records intact on this actual input.
 for ref,value in p0.items():assert p1[ref]==value,('original producer pool changed',ref)
 def symbols(word,p):
  assert u16(word,50)&0x300==0
  ref=u64(word,8);result=[];seen=set()
  while ref!=(1<<64)-1:
   assert ref not in seen;seen.add(ref);r=p[ref]
   assert u16(r,4)&7==0 and not r[6]&2,'ordinary unskipped equality scope'
   result.append(u16(r,8));ref=u64(r,32)
  return result[::-1]
 words=[bytearray(base64.b64decode(x['Storage120'])) for x in before['Words']];word_symbols=[symbols(w,p0) for w in words]
 original=len(words);selected=[];details=[];new_live_refs=set();appended=duplicates=updated=0
 # The actual input has ordinary0 rows and an FFFA delimiter root. Reject rather
 # than pretending this covers nonordinary/unit/residual/closed-label branches.
 for index in range(starts[lastnode],starts[lastnode+1]):
  r=row(index)
  if not r[32]&8:continue
  coord=u64(r,8);node=decode_node(coord);arc=coord>>24&65535
  if node!=lastnode or graph[node][arc][3]!=last:continue
  cursor=index;chain=[];seen=set();last_parent=-1;flags=65534
  while True:
   assert cursor not in seen;seen.add(cursor);v=row(cursor);coord=u64(v,8);node=decode_node(coord);arc=coord>>24&65535
   if node==-1 or node<minimum:raise AssertionError('outside observed actual successful scope')
   last_parent=i32(v,16);label=u16(v,28)
   if label==0xfffa:
    assert last_parent==-1 and v[32]&7==3;status=2;break
   assert label<0xff00 and v[32]&7==0 and i32(v,36)==-1
   chain.append((cursor,node,arc,label));flags=v[4] if v[4]!=255 else flags
   assert last_parent!=-1;cursor=last_parent
  assert chain
  earliest=chain[-1];source_begin=graph[earliest[1]][earliest[2]][2]
  if source_begin!=first:continue
  selected.append(index)
  primary=i32(r,20);secondary=i32(r,24)
  if last_parent!=-1:
   primary=(primary-i32(row(last_parent),20)+(1<<31))%(1<<32)-(1<<31)
   secondary=(secondary-i32(row(last_parent),24)+(1<<31))%(1<<32)-(1<<31)
  cost=f(f(primary)/f(((lastnode+1-minimum)<<13)));score=f(-5*cost);cost1c=f(f(secondary)*f(1/8192))
  labels=[v[3] for v in chain[::-1]]
  match=next((i for i,s in enumerate(word_symbols) if s==labels),None)
  if match is not None:
   duplicates+=1
   # Native selected original payload is deliberately not improved by this call.
   if match>=original:
    raise AssertionError('new-only duplicate-update not exercised by this capture')
   details.append(dict(index=index,kind='protected-original-duplicate',cost18=cost,cost1c=cost1c,score10=score,labels=labels,lastParent=last_parent))
   continue
  appended+=1
  refs=[]
  for compact_index,node,arc,label in chain[::-1]:
   owner=node|(arc<<24);descriptor=compact_index-starts[node]
   matches=[(ref,g) for ref,g in p1.items() if key(ref)==owner and i32(g,0)==descriptor and u16(g,8)==label and u16(g,4)&7==0]
   assert len(matches)==1,('exact real producer owner/descriptor/symbol',owner,descriptor,label)
   refs.append(matches[0][0]);new_live_refs.add(matches[0][0])
  for i,ref in enumerate(refs):assert u64(p1[ref],32)==(refs[i-1] if i else (1<<64)-1)
  w=bytearray(120);wu64(w,8,refs[-1]);wf(w,16,score);wf(w,20,1);wf(w,24,cost);wf(w,28,cost1c)
  w32(w,40,len(refs));capacity=0
  for count in range(1,len(refs)+1):
   if capacity<count:capacity=max(count,int(capacity*1.5))
  w32(w,44,capacity);w16(w,48,flags);publication=0x80 if r[32]&16 else 0
  for ref in refs:publication|=u16(p1[ref],6)
  # Fresh actual TOP ownerC4 is false; observed glyph publication has no carry.
  publication&=0xfbff;w16(w,50,publication);w32(w,60,-1);w32(w,84,-1)
  words.append(w);word_symbols.append(labels)
  details.append(dict(index=index,kind='new',cost18=cost,cost1c=cost1c,score10=score,labels=labels,lastParent=last_parent,flat=refs))
 assert set(p1)-set(p0)==new_live_refs,'no unaccounted surviving temporary rows'
 assert selected and len(words)==2,'actual two-word ranking scope only'
 assert words[0][60:64]==words[1][60:64] and word_symbols[0]!=word_symbols[1]
 assert all(u16(w,50)&0x300==0 for w in words)
 # Native13D360: distinct equality groups, original selected0 protected; default
 # quota2 holds both; tail length1 has no sort permutation. No payload replacement.
 quota=2;assert len(words)<=quota;selected_ordinals=[0]
 order=sorted(range(len(words)),key=lambda i:-rf(words[i],16));assert rf(words[0],16)!=rf(words[1],16)
 masses=[f(math.exp(f(rf(words[i],16)-rf(words[order[0]],16)))) for i in order]
 expected=[bytearray(w) for w in words]
 norm=[f(masses[0]/f(masses[0]+masses[1]))]
 norm.append(f(f(masses[1]*norm[0])/masses[0]))
 low=min(rf(w,28) for w in words)
 for pos,index in enumerate(order):wf(expected[index],16,norm[pos]);wf(expected[index],20,math.exp(-f(rf(words[index],28)-low)))
 observed=[base64.b64decode(w['Storage120']) for w in after['Words']]
 assert len(observed)==len(expected)==len(a['Result']['Words'])
 ulps=[]
 for i,(got,want) in enumerate(zip(observed,expected)):
  assert got[:16]==want[:16] and got[24:]==want[24:],('complete120 nonexponential fields',i)
  for offset in [16,20]:
   actual=struct.unpack_from('<I',got,offset)[0];reference=struct.unpack_from('<I',want,offset)[0]
   ulp=abs(actual-reference);assert ulp<=4,('exponential float32 channel',i,offset,ulp);ulps.append(ulp)
  assert observed[i]==base64.b64decode(a['Result']['Words'][i])
 assert a['Result']['Candidates']==len(selected) and a['Result']['Appended']==appended and a['Result']['Duplicates']==duplicates and a['Result']['UpdatedNewDuplicates']==updated
 assert a['Result']['QuotaAfter']==quota and after['SelectedOrdinals']==selected_ordinals
 assert [w['FlatReferences'] for w in after['Words']]==[before['Words'][0]['FlatReferences'],details[-1]['flat']]
 return dict(status='PASS_DATA_ONLY',selected=selected,details=details,words120=len(expected),ordinaryEqualityComparisons=duplicates+1,quota=quota,maxExponentialULP=max(ulps),producerBoundary='actual pool descriptor/symbol/link/publication data; NN confidence/temporary duplicate records not independently reconstructed')
if __name__=='__main__':
 if len(sys.argv)!=4:raise SystemExit('positive-capture.json corrected-raw-trace.json corrected-raw-capture.json')
 a,t,c=[json.loads(Path(x).read_bytes()) for x in sys.argv[1:]]
 report=replay(a,t,c);negative=[]
 def corrupt(field,change):
  b=copy.deepcopy(a);change(b)
  try:replay(b,t,c)
  except (AssertionError,KeyError,IndexError):negative.append(field)
  else:raise AssertionError('tamper accepted: '+field)
 for at in [0,8,24,28,40,48,60,84,119]:
  def mutate(x,at=at):
   z=bytearray(base64.b64decode(x['AfterState']['Words'][1]['Storage120']));z[at]^=1;x['AfterState']['Words'][1]['Storage120']=base64.b64encode(z).decode()
  corrupt('newword'+str(at),mutate)
 for name in ['Candidates','Appended','Duplicates','UpdatedNewDuplicates','QuotaAfter']:corrupt(name,lambda x,name=name:x['Result'].__setitem__(name,x['Result'][name]+1))
 corrupt('selected',lambda x:x['AfterState'].__setitem__('SelectedOrdinals',[1]))
 corrupt('flat',lambda x:x['AfterState']['Words'][1]['FlatReferences'].reverse())
 report['tamperControls']=negative;print(json.dumps(report,indent=2))
