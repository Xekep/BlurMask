"""Bounded DATA replay for future f6 actual. Never executes Ink/vendor/NN.
Known missing export channels are always reported, never asserted PASS.
"""
import argparse,ast,base64,copy,decimal,hashlib,json,math,struct
from pathlib import Path
import numpy as np
HERE=Path(__file__).resolve().parent
IMP=0x4000000;MAX=(1<<64)-1
PRIOR='bc5a6fd66742ed9e2395b0c7872a58168e67d8b1615c80146563dd46da3abdfc'
INPUT='d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99'
AK='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'

def sha(b):return hashlib.sha256(b).hexdigest()
def dec(v):return base64.b64decode(v,validate=True)
def enc(v):return base64.b64encode(v).decode()
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<i',struct.pack('<f',v))[0]
def fl(b,p):return struct.unpack_from('<f',b,p)[0]
def ii(b,p):return struct.unpack_from('<i',b,p)[0]
def hh(b,p):return struct.unpack_from('<H',b,p)[0]
def qq(b,p):return struct.unpack_from('<Q',b,p)[0]
def wi(b,p,v):struct.pack_into('<I',b,p,v&0xffffffff)
def wh(b,p,v):struct.pack_into('<H',b,p,v&65535)
def wq(b,p,v):struct.pack_into('<Q',b,p,v)
def wf(b,p,v):struct.pack_into('<f',b,p,v)
def exact_int(v):assert type(v)is int;return v

def defs(name,g):
 raw=(HERE/'reference'/name).read_bytes();pins=json.loads((HERE/'dependency-pins.json').read_text());assert sha(raw)==pins[name]
 tree=ast.parse(raw.decode('utf-8-sig'));exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)],type_ignores=[]),name,'exec'),g);return g

def setup(prior_path,input_path,ak_path):
 raw=prior_path.read_bytes();assert sha(raw)==PRIOR;prior=json.loads(raw)
 raw=input_path.read_bytes();assert sha(raw)==INPUT;source=json.loads(raw);assert len(source)==141 and source[4]==dict(Name='actual-4',Raw=[[[0,0],[7,40],[14,0]]],Resolution=96)
 ak=ak_path.read_bytes();assert sha(ak)==AK
 c=prior['Compact'];b=dec(c['Storage'])[:c['Records']*40];selected=c['Starts'][-2]+hh(dec(c['SelectedRow']),34)
 al=defs('old_raw_compact_oracle.py',{'struct':struct})['read_alphabet'](ak);labels={v:k for k,v in al['mapping'].items()}
 g=defs('flush_native.py',dict(struct=struct,np=np,p=dict(recordCount=c['Records'],selected=selected,alternative=65535,totalCost=c['Winner']['TotalCost'],rowCost14=c['Winner']['RowCost14'],cumulative=[0]*c['Nodes']),compact=b,starts=c['Starts'],graph=[[[a['Group'],a['Span'],a['Start'],a['End']]for a in n]for n in c['Graph']],marker=labels.get('\x03',65535),joiner=labels.get('\u200d',65535),soft=labels.get('\xad',65535),unit=hh(ak,105)))
 allrows,visited,flushes,calls=g['flush_expected'](bytes(72));assert not calls and len(flushes)==2
 assert not flushes[0]['protocol']and flushes[1]['protocol'] and len(allrows)==144
 # Independently reconstruct source-alphabet aliases from raw MAIN groups.
 ends=struct.unpack_from('<8H',ak,545544);ids=struct.unpack_from('<25H',ak,545560);weights=struct.unpack_from('<7f',ak,545610);header=struct.unpack_from('<8H',ak,99);aliases={}
 for i,w in enumerate(weights):
  if w<=0:continue
  group=ids[ends[i]:ends[i+1]];ordinary=[x for x in group if x<182]
  if ordinary:
   for x in group:
    if x>=182:aliases.setdefault(x,ordinary[0])
 for text in ['-','\uff0d','_','\uff3f','\xad','\u30fc','\u4e00','\U000f0002','\U000f0003']:
  if text in labels and not(text=='\u30fc'and labels[text]<182):aliases.setdefault(labels[text],header[1])
 aliases.setdefault(218,labels.get("'",65535));aliases.setdefault(219,labels.get('"',65535))
 return dict(prior=prior,c=c,b=b,selected=selected,al=al,labels=labels,aliases=aliases,closed=labels['\U000f0001'],allrows=allrows,visited=visited,flushes=flushes,g=g,ak=ak)

def confidence(x,s):
 score=s['c']['Scores'];observed=x['Vector10'];assert len(observed)==len(score)<=64
 results={};decimal.getcontext().prec=80;expected_by_key={}
 for old in score:
  key=(exact_int(old['Group']),exact_int(old['Span']));assert key not in expected_by_key;expected_by_key[key]=old
 seen=set()
 for o in observed:
  assert set(o)=={'Group','Span','SourceProbabilityBits','Candidates','Probabilities','ConfidenceBits'}
  for k in('Group','Span','SourceProbabilityBits'):exact_int(o[k])
  key=(o['Group'],o['Span']);assert key not in seen and key in expected_by_key and o['Group'] not in results
  seen.add(key);old=expected_by_key[key]
  expected_candidates=[dict(Symbol=a,Cost=b)for a,b in zip(old['Symbols'],old['Costs'],strict=True)];assert o['Candidates']==expected_candidates
  for candidate in o['Candidates']:
   assert set(candidate)=={'Symbol','Cost'}and type(candidate['Symbol'])is int and type(candidate['Cost'])is int
  family=o['Probabilities'];assert len(family)==4 and all(len(v)==182 for v in family)
  assert all(type(p)is int and 0<=p<=32768 for v in family for p in v)
  output=[]
  for k,candidate in enumerate(expected_candidates):
   sym=candidate['Symbol']
   if k>=20:output.append(0);continue
   # Source90 is not exported independently. No current ordinary fixture
   # includes closed; explicitly refuse rather than echo its captured value.
   assert sym!=s['closed'],'closed source90 channel lacks independent input'
   target=s['aliases'].get(sym,sym);assert target<182
   vals=[]
   for v in family:
    product=(v[target]/32768)*-.575
    exact=decimal.Decimal(1)/(decimal.Decimal.from_float(product).exp()+1)
    vals.append(f(float(exact)))
   total=f(vals[1]+vals[0]);total=f(total+vals[2]);total=f(total+vals[3]);out=f(total/4)
   output.append(bits(min(out,1)))
  assert o['ConfidenceBits']==output and all(type(v)is int for v in o['ConfidenceBits'])
  results[o['Group']]={c['Symbol']:struct.unpack('<f',struct.pack('<i',v))[0]for c,v in reversed(list(zip(expected_candidates,output,strict=True)))}
 assert seen==set(expected_by_key)
 return results

def empty_word():
 w=bytearray(120);wq(w,8,MAX)
 for p,v in [(16,0x3f800000),(20,0x3f800000),(24,0),(28,0),(40,0),(48,65534),(60,-1),(84,-1),(96,0),(112,0)]:wi(w,p,v)
 return w

def packet_word(w,q):
 w=w.copy()
 for a,b,n in [(0,16,4),(4,24,4),(8,28,4),(12,48,2),(14,50,2),(16,53,1),(17,54,1),(20,60,4),(28,68,4),(32,72,4),(36,76,4),(40,80,4),(44,55,1)]:w[b:b+n]=q[a:a+n]
 return w

def expected_scan(s,first,last):
 c=s['c'];b=s['b'];graph=c['Graph'];rows=[];selected=[]
 for at in range(c['Starts'][-2],c['Starts'][-1]):
  r=b[at*40:at*40+40]
  if not r[32]&8:continue
  key=qq(r,8);node=key&0xffffff;arc=key>>24&65535
  if node!=len(graph)-1 or graph[node][arc]['End']!=last:continue
  assert r[32]&7==0 and ii(r,36)==-1 and hh(r,28)<0xff00
  parent=ii(r,16);p=b[parent*40:parent*40+40];assert ii(p,16)==-1 and hh(p,28)==0xfffa
  assert graph[node][arc]['Start']==first
  row=bytearray(72);struct.pack_into('<iH',row,0,node,arc);wi(row,8,at)
  pk=qq(p,8);pn=pk&0xffffff;pa=pk>>24&65535;struct.pack_into('<iH',row,12,pn,pa);wi(row,20,parent)
  struct.pack_into('<iH',row,24,-1,65535);wi(row,32,-1);struct.pack_into('<iH',row,36,-1,65535)
  struct.pack_into('<QHH',row,48,node|(arc<<24),hh(r,28),hh(r,28));row[61]=(r[32]>>6)&1;row[62]=r[33];row[69]=(r[7]>>1)&1
  selected.append(at);rows.append(bytes(row))
 assert selected
 return selected,rows

def normalize(words):
 out=[bytearray(w)for w in words];order=[]
 for i,w in enumerate(out):
  lo=0;hi=i;mid=i//2
  while lo<hi:
   old=fl(out[order[mid]],16);now=fl(w,16)
   if now<=old:
    if old<=now:break
    lo=mid+1
   else:hi=mid
   mid=(hi+lo)//2
  order.insert(mid,i)
 for i in reversed(order):
  difference=f(fl(out[i],16)-fl(out[order[0]],16));wf(out[i],16,0 if difference< -75 else 3.73e32 if difference>75 else f(math.exp(difference)))
 if len(out)==1:wf(out[0],16,1);wf(out[0],20,1);return out
 prev=fl(out[order[0]],16);kept=0
 if prev>0:
  kept=1;wf(out[order[0]],16,f(prev/f(prev+fl(out[order[1]],16))))
  for j in range(1,len(out)):
   now=fl(out[order[j]],16)
   if now==0:break
   wf(out[order[j]],16,f(f(now*fl(out[order[j-1]],16))/prev));prev=now;kept+=1
 for j in range(kept,len(out)):wf(out[order[j]],16,0)
 minimum=min(fl(w,28)for w in out)
 for i in order:wf(out[i],20,f(math.exp(-f(fl(out[i],28)-minimum))))
 return out

# This oracle predicts only the original immutable graph2/single ordinary
# glyph plus empty leading boundary domain. A new branch must refuse.
def group_separations(values,s):
 groups={a['Group']for layer in s['c']['Graph']for a in layer}
 assert type(values)is list and 1<=len(values)<=128 and groups==set(range(len(values)))
 assert all(type(v)is int and -(1<<31)<=v<(1<<31)for v in values)
 return {g:values[g]for g in groups}

def request_cost(base,separation):
 #Native1C13A0 finite selectedcandidate path. Sourcegeometry applied only
 #when candidatecost<IMP. No sourcecost inferred from NNconfidence.
 assert type(base)is int and type(separation)is int
 return (base+separation+(1<<31))%(1<<32)-(1<<31)if base<IMP else base

def source_inputs(q,first,last):
 assert set(q)=={'Nodes','Seeds','PreparedCounts','PreparedMap','Factor','PreparedIdentities','Keys','OriginalIndices','OriginalXY','OriginalIdentities','OriginalEndKeys'}
 def integer(v,lo,hi):assert type(v)is int and lo<=v<=hi;return v
 def array(v,lo,hi):assert type(v)is list and lo<=len(v)<=hi;return v
 def key(v):
  assert type(v)is dict and set(v)=={'First','Second','Third'}
  return[integer(v[k],-(1<<31),(1<<31)-1)for k in('First','Second','Third')]
 def identity(v):
  assert type(v)is dict and set(v)=={'Key','MetadataPointIndex','KeyRecordIndex','RemainderByte'}
  key(v['Key']);integer(v['MetadataPointIndex'],0,127);integer(v['KeyRecordIndex'],0,127);integer(v['RemainderByte'],0,255)
 counts=array(q['PreparedCounts'],1,16);factor=integer(q['Factor'],1,0xffffffff);pointmap=array(q['PreparedMap'],0,128)
 keys=[key(k)for k in array(q['Keys'],1,128)]
 for count in counts:integer(count,1,128)
 assert 0<sum(counts)<=128 and len(q['PreparedIdentities'])==len(counts)
 for tick in pointmap:integer(tick,0,0xffffffff)
 for name in('OriginalIndices','OriginalXY','OriginalIdentities','OriginalEndKeys'):array(q[name],1,16)
 assert len(q['OriginalIndices'])==len(q['OriginalXY'])==len(q['OriginalIdentities'])==len(q['OriginalEndKeys'])
 #Original rawsource key chronology from immutable3point input, no duplicates.
 expectedkeys=[[0,0,i]for i in range(3)];assert keys==expectedkeys
 original=[]
 for indices,xy,ids,ends in zip(q['OriginalIndices'],q['OriginalXY'],q['OriginalIdentities'],q['OriginalEndKeys'],strict=True):
  array(indices,1,128);array(ids,1,128);array(ends,1,128);array(xy,2,256)
  assert len(indices)==len(ids)==len(ends) and len(xy)==2*len(ids) and all(type(v)in(int,float)and math.isfinite(v)for v in xy)
  for idx in indices:integer(idx,0,127)
  for id in ids:identity(id)
  for end in ends:key(end)
  assert indices==[i['Key']['Third']for i in ids]
  for id,end in zip(ids,ends,strict=True):
   identity(id)
   at=exact_int(id['KeyRecordIndex']);assert key(id['Key'])==keys[at] and id['RemainderByte']==0
   assert key(end)==expectedkeys[at] #all3 original keys distinct; no dropped rawtail
   original.append((id,end))
 assert len(original)==3 and sorted(i[0]['KeyRecordIndex']for i in original)==list(range(3))
 #Flat metadata source uses its exported native MetadataPointIndex, not
 #physical stroke position or group ordinal as a replacement authority.
 metadata={i['MetadataPointIndex']:(i,end)for i,end in original};assert len(metadata)==3
 assert len(pointmap)in(0,sum(counts));prepared=[];at=0
 for count,ids in zip(counts,q['PreparedIdentities'],strict=True):
  array(ids,1,128);assert len(ids)==count;stroke=[]
  for value in ids:
   identity(value)
   ticks=pointmap[at]if pointmap else at;whole,rem=divmod(ticks,factor)if pointmap else(ticks,0);mi=whole+bool(rem);ri=(factor-rem)&255 if rem else 0
   original_id,end=metadata[mi]
   expected=dict(Key=original_id['Key'],MetadataPointIndex=mi,KeyRecordIndex=original_id['KeyRecordIndex'],RemainderByte=ri)
   assert value==expected
   stroke.append((key(value['Key']),key(end),value['KeyRecordIndex'],ri));at+=1
  prepared.append(stroke)
 nodes=array(q['Nodes'],1,16);seeds=array(q['Seeds'],1,128)
 for layer in nodes:array(layer,1,128)
 assert sum(map(len,nodes))<=128
 for seed in seeds:
  assert type(seed)is dict and set(seed)=={'Stroke','First','End'}
  integer(seed['Stroke'],0,len(counts)-1);integer(seed['First'],0,counts[seed['Stroke']]-1);integer(seed['End'],seed['First']+1,counts[seed['Stroke']])
 for n,layer in enumerate(nodes):
  for arc in layer:
   assert type(arc)is dict and set(arc)=={'Group','Start','End','Next'}
   integer(arc['Group'],0,len(seeds)-1);integer(arc['Start'],0,0xffffffff);integer(arc['End'],arc['Start'],0xffffffff);array(arc['Next'],0,128)
   for idx in arc['Next']:integer(idx,0,len(nodes[n+1])-1)if n+1<len(nodes)else integer(idx,0,-1)
   assert not arc['Next']or n+1<len(nodes)
   assert all(type(i)is int and 0<=i<len(nodes[n+1])for i in arc['Next'])
 def search(n,o):
  a=nodes[n][o]
  if a['End']>last:return None
  if a['End']==last:return[a['Group']]
  for child in a['Next']:
   path=search(n+1,child)
   if path is not None:return[a['Group']]+path
 path=None
 for ordinal,arc in enumerate(nodes[first>>7]):
  if arc['Start']==first:
   path=search(first>>7,ordinal)
   if path is not None:break
 assert path is not None
 rows=[]
 for group in path:
  seed=seeds[group];stroke=prepared[seed['Stroke']];a=stroke[seed['First']];b=stroke[seed['End']-1]
  rows.append(dict(first=a,last=b,tail=seed['End']==len(stroke)))
 #Native stable key/remainder/end sorting +reverse next-F8-record merge.
 rows.sort(key=lambda r:(*r['first'][0],-r['first'][3],*r['last'][1],-r['last'][3]))
 merged=0
 for i in range(len(rows)-1,0,-1):
  a,b=rows[i-1],rows[i];old=a['last'];cur=b['first']
  equal=cur[0]==old[0]and cur[3]==old[3];nxt=old[2]+1;adj=a['tail']and nxt<len(keys)and cur[0]==keys[nxt]
  if equal or adj:a.update(last=b['last'],tail=b['tail']);rows.pop(i);merged+=1
 def kp(v):return dict(zip(('First','Second','Third'),v))
 refs=[dict(First=kp(r['first'][0]),Last=kp(r['last'][1]if r['tail']else r['last'][0]),FirstResidual=r['first'][3],LastResidual=r['last'][3])for r in rows]
 return dict(References=refs,SeedGroups=path,PreparedPoints=sum(counts),OriginalPoints=len(original),ReverseMerges=merged,ChronologyIdentities=at)

def audit(x,s):
 assert x['Completed']is True and x['Admission']=='NONE'and x['GlobalReadinessGateUnchanged']is True and x['NoExpectedLabels']is True
 assert (x['InputSha'],x['OriginalIndex'],x['RawPoints'],x['RawStrokes'],x['Resolution'],x['GraphNodes'],x['ParentChildren'])==(INPUT,4,3,1,96,2,1)
 assert x['Scope']=='ACTUAL_OWNING_ENGLISH_SOURCE188_AUGMENT_RANK275_NO_TOP273_ADMISSION'
 assert x['Metrics']==s['prior']['Metrics'],'original upstream metrics changed'
 conf=confidence(x,s);data=x['SourceCompletion'];assert set(data)=={'Raw','Source'};raw=data['Raw'];src=data['Source'];first=0;last=131
 for name in('First','Last','Appended','Duplicates','Updated','QuotaAfter','ReservedPayload','ReservedCopies'):exact_int(src[name])
 for ordinal in src['SelectedOrdinals']:exact_int(ordinal)
 assert src['First']==first and src['Last']==last
 # Whole physical stroke has three distinct original key records. Fullspan
 # endpoint follows original raw chronology, no target-derived penups.
 ref=dict(First=dict(First=0,Second=0,Third=0),Last=dict(First=0,Second=0,Third=2),FirstResidual=0,LastResidual=0)
 source_report=source_inputs(src['SourceInputs'],first,last)
 separations=group_separations(src['GroupSeparations'],s)
 assert src['References']==source_report['References']==[ref]
 source26=struct.pack('<6iBB',0,0,0,0,0,2,0,0);assert dec(src['Storage26'])==source26
 indices,augmentation=expected_scan(s,first,last);assert src['CandidateIndices']==indices
 assert raw['Visited']==s['visited'] and dec(raw['Rows72'])==s['allrows']
 assert len(raw['Flushes'])==2 and raw['MetadataEvents']==0
 pool={};unknown=[];flush_words=[];prefix_words=[]
 def append(row):
  nd=ii(row,0);arc=hh(row,4);group=s['c']['Graph'][nd][arc]['Group'];key=qq(row,48);descriptor=ii(row,8)-s['c']['Starts'][nd]
  owner=pool.setdefault(key,[])
  for ordinal,record in enumerate(owner):
   if ii(record,0)==descriptor:return key|(ordinal<<40)
  record=bytearray(40);wi(record,0,descriptor);wh(record,6,0);wh(record,8,hh(row,58));wf(record,12,1 if row[60]else row[62]/255);wf(record,16,1 if row[60]else conf[group][hh(row,58)]);wq(record,32,MAX)
  if not row[60]:
   observer=next(o for o in x['Vector10']if o['Group']==group)
   acoustic=next(c['Cost']for c in observer['Candidates']if c['Symbol']==hh(row,58))
   wi(record,20,request_cost(acoustic,separations[group]))
  if row[60]:
   assert row[60]==3 and hh(row,58)==0xfffa and ii(row,24)==-1
   wh(record,4,1);wh(record,8,1)
  else:
   wh(record,4,8 if row[61]else 0);assert hh(row,58) not in (s['closed'],s['labels'].get('\U000f0004'),s['labels'].get('\U000f0008'),s['labels'].get('\U000f0002'),s['labels'].get('\U000f0003'),184,182,183)
  owner.append(record);return key|((len(owner)-1)<<40)
 initial=empty_word();working=initial.copy()
 for i,(a,wanted)in enumerate(zip(raw['Flushes'],s['flushes'],strict=True)):
  assert (a['Current'],a['Filtered'],a['Protocol'],a['Rows'],a['Succeeded'])==(wanted['current'],wanted['filtered'],wanted['protocol'],1,True)
  assert a['SeedOrigins']==wanted['origins']
  packet=bytes(v if origin==-1 else initial[origin]for v,origin in zip(wanted['template'],wanted['origins'],strict=True));assert dec(a['Packet64'])==packet
  working=packet_word(working,packet);reference=append(wanted['rows']);wq(working,8,reference)
  assert a['SymbolSpan']==(0 if wanted['protocol']else 1) and a['PublishedIndex']==(None if wanted['protocol']else 0)
  assert dec(a['Word276'])==working
  flush_words.append(bytes(working))
  if not wanted['protocol']:
   published=working.copy();wi(published,40,1);wi(published,44,1);prefix_words.append((published,[reference]))
  #Native13F110 reset preserves uninitialized/persisted fields.
  reset=empty_word()
  for at,n in [(8,8),(16,16),(40,4),(48,4),(53,3),(60,4),(84,4),(96,4),(112,4)]:working[at:at+n]=reset[at:at+n]
 assert len(prefix_words)==1 and len(src['BeforeWords'])==1
 assert src['BeforeWords']==[dict(Word275=enc(prefix_words[0][0]),FlatReferences=prefix_words[0][1],First=first,Last=last,Index=0)]
 #Equality is actual ordinary same-pool scalar, not a synthetic equality key.
 #The ORIGINAL275 word cannot be updated on equal augmentation candidates.
 words=[prefix_words[0][0].copy()];refs=[prefix_words[0][1]];appended=duplicate=updated=0
 for idx,row in zip(indices,augmentation,strict=True):
  symbol=hh(row,58);reference=append(row);candidate=empty_word();wq(candidate,8,reference);wh(candidate,48,s['b'][idx*40+4]if s['b'][idx*40+4]!=255 else 65534)
  wh(candidate,50,hh(candidate,50)|(128 if s['b'][idx*40+32]&16 else 0))
  cost=f(f(ii(s['b'],idx*40+20))/f((2)*8192));wf(candidate,16,f(-5*cost));wf(candidate,24,cost);wf(candidate,28,f(f(ii(s['b'],idx*40+24))*f(1/8192)))
  old_symbols=[hh(pool[r[0]&((1<<40)-1)][r[0]>>40],8)for r in refs]
  if symbol in old_symbols:
   duplicate+=1;match=old_symbols.index(symbol)
   assert match==0,'new duplicate-update/rank domain needs independent extension'
   key=reference&((1<<40)-1);assert reference>>40==len(pool[key])-1;pool[key].pop()
  else:
   wi(candidate,40,1);wi(candidate,44,1);words.append(candidate);refs.append([reference]);appended+=1
 assert len(words)<=2,'native unstable ranking beyond two distinct rows needs separate replay'
 assert src['Appended']==appended and src['Duplicates']==duplicate and src['Updated']==updated
 #Protected sole selected0 stays first. Atmosttwo words, quota2, no trim or
 #tie-sort: this is a native-derived scoped ranking theorem, not OrderBy.
 assert src['QuotaAfter']==2 and src['SelectedOrdinals']==[0] and src['SourceMarked']is True and src['CheckboxGuard']is True
 normalized=normalize(words)
 assert len(src['Words'])==len(normalized)
 full=0
 for ordinal,(actual,expected,references)in enumerate(zip(src['Words'],normalized,refs,strict=True)):
  b=dec(actual['Word275']);assert len(b)==120
  assert actual['FlatReferences']==references and (actual['First'],actual['Last'],actual['Index'])==(first,last,ordinal)
  assert b[:16]==expected[:16] and b[24:]==expected[24:]
  for at in (16,20):assert abs(ii(b,at)-ii(expected,at))<=4,'mass outside pinned mathf-exp4ULP scope'
  full+=120
 assert raw['Published275']==[src['Words'][0]]
 assert raw['Owners']==src['Owners'];assert len(src['Owners'])==len(pool)
 checked=0;ignored=0
 for actual,(key,records)in zip(src['Owners'],pool.items(),strict=True):
  assert actual['Key']==key and actual['PackedCount']==(len(records)*4|(0x8000 if key&(1<<39)else 0))
  blob=dec(actual['Records']);assert len(blob)==40*len(records)
  for j,wanted in enumerate(records):
   got=blob[j*40:j*40+40];ordinary=(hh(wanted,4)&7)==0
   assert got[:20]==wanted[:20] and got[24:]==wanted[24:]
   assert got[20:24]==wanted[20:24];checked+=4
   checked+=36
 assert type(src['ReservedPayload'])is int and src['ReservedPayload']>0 and type(src['ReservedCopies'])is int and src['ReservedCopies']>=src['ReservedPayload']
 return dict(Status='PASS_SCOPED_DATA_CHANNELS_WITH_EXPLICIT_UNVERIFIED_EXPORTS',CandidateIndices=indices,Appended=appended,Duplicates=duplicate,Updated=updated,PerFlushPackets=2,PacketBytes=128,Word276Bytes=240,Before275Bytes=120,RankedNormalized275Bytes=full,CountFill26Bytes=len(source26),SourceReplay=source_report,PoolVerifiedBytes=checked,PoolUnverifiedCostBytes=ignored,FinalSeparationGroups=len(separations),Vector10Groups=len(conf),ConfidenceObservedOrder=[[o['Group'],o['Span']]for o in x['Vector10']],PriorScoreExportOrder=[[o['Group'],o['Span']]for o in s['c']['Scores']],NativeExecuted=False,NNEvaluated=False,
  NotIndependentlyVerified=['NN output and Separation geometry derivations themselves: observed4x182 outputs and actual SAME finalgraph I32 Separation vector are independently replayed scalar inputs; no NN/geometry pipeline replay','SourceProbabilityBits/source90; closed-label branch explicitly refused','Source188 count/fill actual invocation count not observed; BOTH final outputs cannot distinguish a cached identical result. SeedGraph/map snapshots are inputs: projection and exact26N are independently replayed, not seed/mapgeneration itself.','Actual dry/emission72B scratch buffers not exported; expected walk rows derive frompriorcompact and consumed channels compare through fullpool40/counters only','OriginalXY coordinates are finite diagnostic values, not independently rerun host geometry; raw key/source26 chronology is checked separately','ReservedPayload/Copies only basicshape, not whole resource/lifecycle proof','Private owner/epoch callback lineage is source-reviewed, not inferable from detached DATA','13D000 positive-checkbox branch absent here; only ordinary selected scalar non-checkbox verified','TOP273/TEXT/UTF8/quality/admission not checked'])

def main():
 p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('prior',type=Path);p.add_argument('input',type=Path);p.add_argument('ak',type=Path);p.add_argument('report',type=Path);p.add_argument('--tamper',action='store_true');a=p.parse_args()
 s=setup(a.prior,a.input,a.ak);x=json.loads(a.capture.read_bytes());report=audit(x,s);negative=[]
 if a.tamper:
  def flip(obj,field,offset):b=bytearray(dec(obj[field]));b[offset]^=16;obj[field]=enc(b)
  edits=[('corrupt-final-separation',lambda v:v['SourceCompletion']['Source']['GroupSeparations'].__setitem__(2,v['SourceCompletion']['Source']['GroupSeparations'][2]+1)),('input-index',lambda v:v.__setitem__('OriginalIndex',5)),('missing-observer',lambda v:v['Vector10'].pop()),('confidence-bit',lambda v:v['Vector10'][0]['ConfidenceBits'].__setitem__(0,v['Vector10'][0]['ConfidenceBits'][0]+1)),('source26',lambda v:flip(v['SourceCompletion']['Source'],'Storage26',12)),('source-ref',lambda v:v['SourceCompletion']['Source']['References'][0]['Last'].__setitem__('Third',1)),('omit-candidate',lambda v:v['SourceCompletion']['Source']['CandidateIndices'].pop()),('packet64',lambda v:flip(v['SourceCompletion']['Raw']['Flushes'][0],'Packet64',0)),('word276',lambda v:flip(v['SourceCompletion']['Raw']['Flushes'][0],'Word276',24)),('omit-leading-flush',lambda v:v['SourceCompletion']['Raw']['Flushes'].pop()),('before275',lambda v:flip(v['SourceCompletion']['Source']['BeforeWords'][0],'Word275',24)),('protected-ordinal',lambda v:v['SourceCompletion']['Source']['SelectedOrdinals'].__setitem__(0,1)),('duplicate-count',lambda v:v['SourceCompletion']['Source'].__setitem__('Duplicates',999)),('mass-beyond-tolerance',lambda v:flip(v['SourceCompletion']['Source']['Words'][0],'Word275',19)),('pool-symbol',lambda v:flip(v['SourceCompletion']['Source']['Owners'][0],'Records',8)),('unmarked-source',lambda v:v['SourceCompletion']['Source'].__setitem__('SourceMarked',False))]
  for name,change in edits:
   bad=copy.deepcopy(x);change(bad)
   try:audit(bad,s)
   except(AssertionError,ValueError,IndexError,KeyError,struct.error):negative.append(name)
   else:raise AssertionError('mutation accepted:'+name)
 report.update(CaptureSHA256=sha(a.capture.read_bytes()),TamperRefused=negative);assert not a.report.exists();a.report.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
if __name__=='__main__':main()
