"""Independent selected fresh219580/218A30 decision replay, ROOT DATA only.
Actual218560 producer outputs, boundary/source services and owner counts are
explicit authenticated operands; full allocator/source-geometry are excluded.
"""
import argparse,base64,copy,hashlib,importlib.util,json,math,struct
from pathlib import Path
D=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('rank_reference',D/'audit_rank_equality.py');b=importlib.util.module_from_spec(spec);spec.loader.exec_module(b)
m=b.score_module();require=m.require;I=m.integer;V=m.vector;blob=m.blob
END=2**64-1;MASK=(1<<40)-1|(1<<56)
u16=lambda x,p:struct.unpack_from('<H',x,p)[0]
u32=lambda x,p:struct.unpack_from('<I',x,p)[0]
i32at=lambda x,p:struct.unpack_from('<i',x,p)[0]
u64=lambda x,p:struct.unpack_from('<Q',x,p)[0]
f32=m.f32
wrap=lambda x:(x+2**31)%2**32-2**31
sign=lambda x,n:(x+2**(n-1))%2**n-2**(n-1)
pack=lambda n,a:(n&0xffffff)|((a&65535)<<24)
def ref(x):return sign(x&0xffffff,24),(x>>24)&65535
def bytes_rows(a,width,name,cap):
 require(type(a)is list and len(a)<=cap,name);return[blob(x,width,name)for x in a]
def physical(x):
 x=I(x,0,END,'physical ref');require(x>>57==0 and ((x>>40)&65535)<2040,'physical domain');return x

def verify_equal(call,left,right,ignored,ordinal,last_scratch):
 require(type(call)is dict and I(call['Ordinal'],0,255,'eq ordinal')==ordinal and call['Applied']is True,'applied equality')
 require(type(call['ObservedEqual'])is bool and call['Mapper']=='Identity2360'and call['ControlLookup']=='Null','equality scope')
 require(I(call['IgnoredPrimaryLabel'],0,65535,'ignored')==ignored,'ignored AL')
 require(blob(call['LeftWord120'],120,'left120')==left and blob(call['RightWord120'],120,'right120')==right,'candidate comparison input/order')
 ls=bytearray(blob(call['LeftBefore4'],4,'left4'));rs=bytearray(blob(call['RightBefore4'],4,'right4'))
 if last_scratch is not None:require((bytes(ls),bytes(rs))==last_scratch,'augmentation scratch continuity')
 events=call['ReadEvents'];require(type(events)is list and len(events)<=8192,'equality reads');at=0;records={}
 def read(reference):
  nonlocal at
  require(at<len(events),'UNOBSERVED equality read');e=events[at];at+=1
  actual=physical(e['Reference']);require(actual==reference,'physical equality read order')
  row=blob(e['RawRecord40'],40,'equality raw40')
  if actual in records:require(records[actual]==row,'same comparison row immutable')
  records[actual]=row;return row
 result=b.equality(left,right,ls,rs,ignored,read)
 require(at==len(events),'extra equality read');require(result is call['ObservedEqual'],'independent candidate13F970 result')
 require(bytes(ls)==blob(call['LeftAfter4'],4,'left after')and bytes(rs)==blob(call['RightAfter4'],4,'right after'),'candidate scalar after')
 return result,(bytes(ls),bytes(rs)),records

class WalkInputs:
 def __init__(self,data,starts,nodes,labels,request,backtrack,boundaries,delimiters):
  self.data=data;self.starts=starts;self.nodes=nodes;self.labels=labels;self.request=request;self.backtrack=backtrack
  self.boundaries=boundaries;self.delimiters=delimiters;self.ba=self.da=0;self.walk=-1;self.emit=False;self.overlay=0
 def row(self,index):
  require(0<=index<len(self.data)//40,'compact index');return self.data[index*40:index*40+40]
 def boundary(self,first,node,arc):
  require(self.ba<len(self.boundaries),'UNOBSERVED boundary');e=self.boundaries[self.ba];self.ba+=1
  require(e['First']is first and e['Node']==node and e['Arc']==arc and e['WalkIndex']==self.walk and e['Emit']is self.emit,'boundary call/order')
  rn=I(e['ResolvedNode'],0,len(self.nodes)-1,'resolved node');ra=I(e['ResolvedArc'],0,len(self.nodes[rn])-1,'resolved arc')
  value=I(e['Value'],-2**31,2**31-1,'boundary value');want=wrap(self.nodes[rn][ra][1 if first else 2]);require(value==want,'boundary graph value')
  if 0<=node<len(self.nodes)and arc<len(self.nodes[node]):require((rn,ra)==(node,arc),'direct base boundary identity')
  else:self.overlay+=1
  return value
 def endpoint(self,node,arc):
  request=bytearray(self.request)
  for pos,fmt,value in [(0,'i',-1),(4,'H',65535),(8,'i',-1),(12,'H',65535),(16,'Q',END),(24,'i',node),(28,'Q',END),(28,'H',arc),(36,'H',65535),(40,'Q',1),(48,'Q',0),(56,'i',0)]:struct.pack_into('<'+fmt,request,pos,value)
  request[60]=0
  require(self.da<len(self.delimiters),'UNOBSERVED delimiter');e=self.delimiters[self.da];self.da+=1
  require(e['WalkIndex']==self.walk and e['Emit']is self.emit and blob(e['Request64'],64,'endpoint64')==request,'actual delimiter request/order')
  for k in ['Remaining','Emitted','FlagC','FlagD']:require(type(e[k])is int and e[k]==0,'UNSUPPORTED nonempty capacity0 delimiter')
 def run(self,index,minimum,emit,word_flags):
  self.walk=index;self.emit=emit;initial=index;current_index=index;previous_node=-1;previous_arc=65535;last_node=-1;last_parent=i32at(self.row(index),16)
  carry=0;first_key=True;last_regular=65535;last_source=END;initial_flags=self.row(index)[32]
  marker,joiner,closed,unit,soft=self.labels;rows=[];visited=[];seen=set();residual=[];scratch=bytearray(self.backtrack)
  inode,_=ref(u64(self.row(index),8))
  if emit and inode==len(self.nodes)-1:self.endpoint(len(self.nodes),65535)
  def finish(status):
   return {'Status':status,'Minimum':last_node,'Parent':last_parent,'Source':last_source,'Flags':65535 if emit and word_flags==65534 else word_flags,'Rows':b''.join(rows),'Visited':visited[:],'Residual':residual[:]}
  def span_key(last,first,alternate):return(last&0xffffff)|(((first-last)&65535)<<24)|((1<<56)if alternate else 0)
  while True:
   current=self.row(current_index);node,arc=ref(u64(current,8))
   if node==-1 or node<minimum:return finish(4 if node==-1 else 1)
   require(current_index not in seen and len(visited)<len(self.data)//40,'cyclic direct parent');seen.add(current_index);visited.append(current_index)
   last_node=node;last_parent=i32at(current,16);parent=(-1,65535)if last_parent==-1 else ref(u64(self.row(last_parent),8));symbol=u16(current,28);eligible=False
   if marker==65535 or symbol!=marker:
    skip_final=symbol==0xfffa and not rows and node==len(self.nodes)-1
    if skip_final:self.endpoint(len(self.nodes),65535)
    elif symbol in(0xfffb,0xfff9,0xfff5,0xfffa):self.endpoint(node,arc);return finish(2)
    elif symbol!=joiner and symbol!=0xfffd:
     if closed!=65535:
      carry&=255
      if symbol==closed:carry=1
     allowed=True
     if symbol==0xfff6:
      if carry&255:carry=0xff00
      else:allowed=not(initial_flags&16)
     if symbol==65535:allowed=False
     eligible=allowed
   if eligible:
    if symbol==unit:
     if emit:residual.append(pack(node,arc))
    elif not emit:last_source=pack(node,arc)
    else:
     kind=current[32]&7;require(kind<=3,'UNSUPPORTED emitter subtype4+')
     if kind<3:last_regular=symbol
     look=last_parent;look_ref=parent;look_seen=set()
     while look>=0:
      require(look not in look_seen,'cyclic lookahead');look_seen.add(look);candidate=self.row(look)
      if candidate[32]&7<3 and u16(candidate,28)!=65535 and u16(candidate,28)!=unit:break
      look=i32at(candidate,16);look_ref=(-1,65535)if look==-1 else ref(u64(self.row(look),8))
     normalized=soft if symbol==0xfff4 else 0xfff9 if symbol in(0xfff5,0xfff7,0xfff8)else symbol
     if kind<3:key=pack(node,arc);first_key=False
     elif(((normalized+7)&65535)<4 or normalized==soft)and look_ref[0]>=0 and previous_node>=0:
      a=self.boundary(True,previous_node,previous_arc);z=self.boundary(False,look_ref[0],look_ref[1])
      if a==-1:a=z;z=self.boundary(True,look_ref[0],look_ref[1])
      key=span_key(a,z,False)
     else:
      alternate=not first_key or look_ref[0]==-1;chosen=(node,arc)if alternate else look_ref
      key=span_key(self.boundary(False,*chosen),self.boundary(True,*chosen),alternate)
     for pos,fmt,value in [(0,'i',node),(4,'H',arc),(8,'i',current_index),(12,'i',parent[0]),(16,'H',parent[1]),(20,'i',last_parent),(24,'i',look_ref[0]),(28,'H',look_ref[1]),(32,'i',look),(36,'i',previous_node),(40,'H',previous_arc),(48,'Q',key),(56,'H',last_regular),(58,'H',normalized),(64,'i',0)]:struct.pack_into('<'+fmt,scratch,pos,value)
     scratch[60]=kind;scratch[61]=(current[32]>>6)&1;scratch[62]=current[33];scratch[68]=0;scratch[69]=(current[7]>>1)&1;rows.append(bytes(scratch))
   if emit and current[4]!=255:word_flags=current[4]
   previous_node=node;previous_arc=arc;current_index=last_parent
   if current_index==-1:return finish(4)
 def scan(self,first,last):
  if last<first:return[],[],[]
  last_node=sign(last>>7,24);minimum=-1 if first<0 else sign(first>>7,24);require(0<=last_node<len(self.nodes),'scan end')
  selected=[];attempts=[];sources=[]
  for index in range(self.starts[last_node],self.starts[last_node+1]):
   row=self.row(index)
   if not(row[32]&8):continue # actual fixed knowledge98=false
   node,arc=ref(u64(row,8))
   if node!=last_node or self.boundary(False,node,arc)!=last:continue
   dry=self.run(index,minimum,False,65535);require(dry['Status']!=-1,'dry failure')
   include=False
   if dry['Status']!=1:
    fn,fa=ref(dry['Source']);fa=sign(fa,16)
    value=self.boundary(True,fn,fa)if fa>=0 else wrap(fn+fa)
    include=value==first
    if include:selected.append(index)
   attempts.extend([index,minimum,dry['Status'],int(include)]);sources.append(dry['Source'])
  return selected,attempts,sources


def put(d,pos,fmt,value):struct.pack_into('<'+fmt,d,pos,value)
def reset_word(residue,head,word_flags,augmentation):
 require(len(residue)==120 and all(u64(residue,p)==0 for p in(32,88,104))and all(i32at(residue,p)==0 for p in(96,112)),'empty WORD allocator scope')
 out=bytearray(residue)
 for pos,fmt,value in [(8,'Q',head),(16,'I',0x3f800000),(20,'I',0x3f800000),(24,'I',0),(28,'I',0),(40,'I',0),(48,'I',0xfffe),(53,'H',0),(60,'i',-1),(84,'i',-1),(96,'I',0),(112,'I',0)]:put(out,pos,fmt,value)
 out[55]=0
 if augmentation:put(out,50,'H',u16(out,50)|128)
 put(out,48,'H',word_flags);return bytes(out)
def cost_values(data,index,parent,first,last):
 row=data[index*40:index*40+40];primary=i32at(row,20);secondary=i32at(row,24)
 if parent!=-1:
  old=data[parent*40:parent*40+40];primary=wrap(primary-i32at(old,20));secondary=wrap(secondary-i32at(old,24))
 denominator=f32(wrap((sign(last>>7,24)+1-(-1 if first<0 else sign(first>>7,24)))<<13))
 require(denominator!=0,'cost denominator')
 cost=f32(f32(primary)/denominator);return f32(-5*cost),cost,f32(f32(secondary)*f32(1/8192))
def post_cost(before,values):
 out=bytearray(before)
 for pos,value in zip((16,24,28),values):put(out,pos,'f',value)
 return bytes(out)
def row_map(events,name):
 require(type(events)is list and len(events)<=8192,name);out={}
 for e in events:
  reference=physical(e['Reference']);row=blob(e['RawRecord40'],40,name)
  if reference in out:require(out[reference]==row,'same phase physical row immutable')
  out[reference]=row
 return out
def merge_rows(target,other):
 for reference,row in other.items():
  if reference in target:require(target[reference]==row,'postchain physical row immutable')
  target[reference]=row

def cleanup(frame,first,last,created,live):
 c=frame['Cleanup'];require(type(c)is dict and c['Applied']is True,'actual completed cleanup')
 require(physical(c['First'])==first and physical(c['Last'])==last,'cleanup input chain operands')
 events=c['PreflightRows'];require(type(events)is list and 1<=len(events)<=4096,'cleanup prefix rows')
 chain=[];rows={};current=last
 for e in events:
  require(current not in rows and current!=END,'cleanup cycle/early end')
  reference=physical(e['Reference']);require(reference==current,'cleanup physical preflight order')
  row=blob(e['RawRecord40'],40,'cleanup row');require(u64(row,24)==0,'no-geometry cleanup scope')
  chain.append(current);rows[current]=row
  if current==first:current=END
  else:current=u64(row,32)
 require(chain[-1]==first and current==END and len(chain)==created,'cleanup inclusive new segment')
 require(V(c['ReversalReferences'],0,END,'reverse refs',4096)==chain,'independent reversal refs')
 require(V(c['NewLinks'],0,END,'new links',4096)==[END]+chain[:-1],'independent link writes')
 pops=chain[::-1];require(V(c['PopReferences'],0,END,'pop refs',4096)==pops,'native reverse visit')
 counts=V(c['CountsBefore'],1,2040,'owner counts',4096);removed=c['Removed'];require(type(removed)is list and len(counts)==len(removed)==len(pops)and all(type(x)is bool for x in removed),'cleanup scalar widths')
 owners={};expected=[]
 for i,reference in enumerate(pops):
  key=reference&MASK;ordinal=(reference>>40)&65535
  if key in owners:require(counts[i]==owners[key],'owner count continuity without allocations')
  require(ordinal<counts[i],'actual owner covers record')
  pop=ordinal==counts[i]-1;expected.append(pop);owners[key]=counts[i]-int(pop)
  if pop:require(reference not in live,'no live275 reference removed')
 require(removed==expected,'independent native tail-only decisions')
 return rows,sum(expected),len(expected)-sum(expected)

def candidate_case(case,expected_labels):
 data=case['scoreTrace']['CandidateTrace'];require(type(data)is dict and data['Scope']=='AUTHENTIC219580_INPUTS_AND_FATES_NOT_FULL_POOL_ALLOCATOR'and data['Truncated']is False,'complete C journal')
 residues=data['Residues'];require(type(residues)is dict,'actual residues')
 word_res=blob(residues['Word120'],120,'word residue');glyph_res=blob(residues['Glyph40'],40,'glyph residue');request=blob(residues['Request64'],64,'request residue');back=blob(residues['Backtrack72'],72,'backtrack residue')
 require(len(V(residues['Labels256'],0,65535,'label residue',256))==256,'complete256 label residue')
 basis=data['Basis'];require(type(basis)is dict and basis['ScanApplied']is True,'actual scan')
 first=I(basis['First'],-2**31,2**31-1,'first');last=I(basis['Last'],-2**31,2**31-1,'last');require((first,last)==(case['first'],case['last']),'span join')
 compact=blob(basis['Compact40'],len(base64.b64decode(basis['Compact40'])),'compact');require(len(compact)%40==0 and 1<=len(compact)//40<=131072,'active compact')
 require(compact==blob(case['compact'],len(compact),'original compact'),'same complete compact input')
 starts=V(basis['Starts'],0,len(compact)//40,'starts',4097);nodes=case['nodes'];require(type(nodes)is list and 1<=len(nodes)<=4096 and len(starts)==len(nodes)+1 and starts[0]==0 and starts[-1]==len(compact)//40 and starts==sorted(starts),'complete actual starts/nodes')
 for node in nodes:
  require(type(node)is list and len(node)<=65536,'node arcs')
  for arc in node:
   require(type(arc)is list and len(arc)==5,'actual graph arc shape')
   I(arc[0],-2**31,2**31-1,'graph group');I(arc[1],0,2**32-1,'graph start');I(arc[2],0,2**32-1,'graph end');I(arc[3],-2**31,2**31-1,'graph span');V(arc[4],0,65535,'selector',65536)
 labels=V(basis['SpecialLabels'],0,65535,'special labels',5);require(labels==expected_labels,'independent raw AL special bindings')
 require(I(basis['OriginalCount'],1,4095,'original count')==1,'UNSUPPORTED: this capture exports one initial WORD120; general original vector remains separate')
 walker=WalkInputs(compact,starts,nodes,labels,request,back,data['Boundaries'],data['Delimiters'])
 chosen,attempts,source_refs=walker.scan(first,last)
 require(V(basis['ScanSelected'],0,len(compact)//40-1,'selected compact',131072)==chosen,'independent219580 candidate inclusion')
 require(V(basis['ScanAttempts4'],-2**31,2**31-1,'scan attempts',4*131072)==attempts and V(basis['ScanFirstReferences'],0,END,'scan first refs',131072)==source_refs,'independent dry walk scan outcomes')
 frames=data['Candidates'];require(type(frames)is list and len(frames)==len(chosen)<=64,'all included candidate frames')
 words=[blob(case['before120'],120,'initial raw WORD120')];refs=[None];last_scratch=None
 stats={'Candidates':len(frames),'New275':0,'HeadReuses':0,'Duplicates':0,'ProtectedDuplicates':0,'Updates':0,'RemovedTemps':0,'RetainedTemps':0,'BoundaryOverlayInputs':0,'ClassHeaderInputs':0,'ReusedChainRows':0}
 for ordinal,(frame,index) in enumerate(zip(frames,chosen)):
  require(type(frame)is dict and I(frame['Ordinal'],0,63,'candidate ordinal')==ordinal and I(frame['CompactIndex'],0,len(compact)//40-1,'candidate index')==index and frame['Applied']is True,'candidate identity/applied')
  require(I(frame['OriginalCount'],1,4095,'frame original count')==1 and I(frame['InputCount'],1,4095,'input count')==len(words),'candidate list input extent')
  for field in ['InternAttempted','InternHeadReuse','InternApplied','InternOwnerC4','UpdateApplied','ClassHeaderObserved']:require(type(frame[field])is bool,'candidate bool '+field)
  I(frame['InternOrdinal'],-1,4095,'intern ordinal');I(frame['ClassFlags'],0,65535,'class flags');I(frame['ClassHeader'],0,2**32-1,'class header')
  I(frame['EmissionStatus'],0,4,'emission status');I(frame['LastDirectParent'],-1,len(compact)//40-1,'last parent');I(frame['UpdatedMinimum'],-1,len(nodes)-1,'updated minimum');I(frame['WordFlags'],0,65535,'word flags');I(frame['LastSourceKey'],0,END,'last source')
  require(i32at(compact,index*40+36)==-1,'UNSUPPORTED auxiliaryV70 candidate')
  emission=walker.run(index,-1 if first<0 else sign(first>>7,24),True,65534)
  require(frame['EmissionStatus']==emission['Status'] and frame['LastDirectParent']==emission['Parent'] and frame['UpdatedMinimum']==emission['Minimum'] and frame['WordFlags']==emission['Flags'] and frame['LastSourceKey']==emission['Source'],'independent218A30 return')
  rows=blob(frame['Rows72'],len(emission['Rows']),'rows72');require(rows==emission['Rows'] and V(frame['Visited'],0,len(compact)//40-1,'visited',131072)==emission['Visited'],'independent full72 row emission/opaque residue')
  residual=V(frame['ResidualReferences'],0,END,'residual refs',4096);require(residual==emission['Residual'],'independent unit residual list')
  require(not residual,'UNSUPPORTED: actual residual association service mapping not independently reconstructed')
  execution=blob(frame['ExecutionRows72'],len(rows),'execution rows');require(execution==rows and all(rows[i+60]==0 for i in range(0,len(rows),72)),'ordinary exact no-residual chain inputs')
  preflight=frame['OrdinaryPreflightRows'];require(type(preflight)is list and len(preflight)<=8192,'ordinary preflight')
  if refs[0]is None:
   refs[0]=[physical(e['Reference'])for e in preflight if e['WordOrdinal']==0]
   require(len(refs[0])==i32at(words[0],40) and bool(refs[0])and refs[0][-1]==u64(words[0],8),'initial actual275 ref vector')
  expected_pairs=[(i,j,reference)for i,rfs in enumerate(refs)for j,reference in enumerate(rfs)]
  require(len(preflight)==len(expected_pairs),'all original ordinary preflight reads')
  oldrows={}
  for e,(wi,ri,reference) in zip(preflight,expected_pairs):
   require(type(e['WordOrdinal'])is int and type(e['ReferenceOrdinal'])is int and(e['WordOrdinal'],e['ReferenceOrdinal'],physical(e['Reference']))==(wi,ri,reference),'ordinary preflight identity/order')
   raw=blob(e['RawRecord40'],40,'ordinary raw');require(u16(raw,4)&7==0,'original ordinary admission')
   if reference in oldrows:require(oldrows[reference]==raw,'prechain pool immutable')
   oldrows[reference]=raw
  for w,rfs in zip(words,refs):
   require(u64(w,8)==rfs[-1] and i32at(w,40)==len(rfs),'actual275 head/count sidecar')
   for j,reference in enumerate(rfs):require(u64(oldrows[reference],32)==(rfs[j-1]if j else END),'original chain links vs actual flat refs')
  require(frame.get('ChainObserved')is True and frame['ChainSucceeded']is True,'observed actual218560 return operands')
  created=I(frame['ChainCreatedCount'],0,4096,'created');reused=I(frame['ChainReusedCount'],0,4096,'reused');stats['ReusedChainRows']+=reused
  require(created+reused==len(rows)//72,'actual chain result row accounting')
  chain_first=I(frame['ChainFirstNewReference'],0,END,'first new');head=I(frame['ChainLastProcessedReference'],0,END,'chain head')
  require((chain_first==END)==(created==0) and(head==END)==(not rows),'chain output operand domain')
  if chain_first!=END:physical(chain_first)
  if head!=END:physical(head)
  before=blob(frame['PreCostWord120'],120,'pre cost');expected=reset_word(word_res,head,emission['Flags'],bool(compact[index*40+32]&16));require(before==expected,'native reset/augmentation/flags/full opaque preCost')
  costs=cost_values(compact,index,emission['Parent'],first,last);require(V(frame['ObservedCostBits'],0,2**32-1,'cost bits',3)==list(struct.unpack('<3I',struct.pack('<3f',*costs))),'independent signed32/F32 costs')
  candidate=post_cost(before,costs)
  calls=frame['Equalities'];require(type(calls)is list and len(calls)<=256,'candidate comparisons')
  postrows={};match=-1;calls_used=0
  for i,w in enumerate(words):
   require(calls_used<len(calls),'missing earliest-match comparison')
   result,last_scratch,raw=verify_equal(calls[calls_used],w,candidate,labels[1],calls_used,last_scratch);calls_used+=1;merge_rows(postrows,raw)
   if result:match=i;break
  require(calls_used==len(calls)and type(frame['MatchObserved'])is int and frame['MatchObserved']==match,'independent earliest same-pool match')
  for reference,raw in postrows.items():
   if reference in oldrows:require(raw==oldrows[reference],'218560 did not modify live old records')
  if match==-1:
   require(frame['InternAttempted']is True and frame['InternApplied']is True and frame['Cleanup']is None and not frame['UpdateApplied']and frame['ExistingDuplicate120']==''and not frame['ClassHeaderObserved'],'intern-only source branch')
   existing=next((i for i,w in enumerate(words)if u64(w,8)==head),-1)
   require(type(frame['InternHeadReuse'])is bool and frame['InternHeadReuse']==(existing>=0),'native exact-head reuse decision')
   if existing>=0:
    require(frame['InternOrdinal']==existing and frame['InternBefore120']==''and frame['InternAfter120']==''and frame['InternGlyphRows']==[],'head reuse no new275 payload')
    stats['HeadReuses']+=1
   else:
    require(frame['InternOrdinal']==len(words)and blob(frame['InternBefore120'],120,'intern before')==candidate,'new275 actual input/index')
    chain=V(frame['InternChain'],0,END,'intern chain',4096);require(len(chain)==len(rows)//72 and(not chain or chain[0]==head),'chain input size/head')
    glyph_events=frame['InternGlyphRows'];require(type(glyph_events)is list and len(glyph_events)==len(chain),'all native flag-fold reads')
    glyphs={}
    for e,reference in zip(glyph_events,chain):require(physical(e['Reference'])==reference,'intern read physical order');glyphs[reference]=blob(e['RawRecord40'],40,'intern raw40')
    merge_rows(postrows,glyphs)
    for j,reference in enumerate(chain):require(u64(glyphs[reference],32)==(chain[j+1]if j+1<len(chain)else END),'independent materialized chain topology')
    for reference,raw in glyphs.items():
     if reference in oldrows:require(raw==oldrows[reference],'reused old glyph immutable')
    out=bytearray(candidate);capacity=0
    for count in range(1,len(chain)+1):
     if capacity<count:capacity=max(count,int(capacity*1.5))
    put(out,32,'Q',0);put(out,40,'i',len(chain));put(out,44,'i',capacity)
    require(type(frame['InternOwnerC4'])is bool,'actual ownerC4 input');carry=frame['InternOwnerC4'];flags=u16(out,50)
    for reference in chain:
     publication=u16(glyphs[reference],6);flags|=publication
     if publication&0x800 and not publication&0x400:carry=False
    flags=(flags|0x400)if carry else(flags&0xfbff);put(out,50,'H',flags)
    require(blob(frame['InternAfter120'],120,'intern after')==out,'independent native intern full120/flag fold')
    flat=chain[::-1];require(V(frame['InternFlatReferences'],0,END,'intern flat',4096)==flat,'native reading-order refs')
    words.append(bytes(out));refs.append(flat);stats['New275']+=1
   require(frame['OutcomeObserved']=='InternReturned','observed intern branch crosscheck')
  else:
   stats['Duplicates']+=1;require(frame['InternAttempted']is False and frame['InternApplied']is False and blob(frame['ExistingDuplicate120'],120,'existing duplicate')==words[match],'duplicate existing payload')
   eligible=match>=1;class_word=False
   if eligible and u16(words[match],48)==emission['Flags'] and emission['Flags']<32768:
    require(frame['ClassHeaderObserved']is True and frame['ClassFlags']==emission['Flags'],'authentic class header input')
    header=I(frame['ClassHeader'],0,2**32-1,'class header');class_word=bool(header&0x1c);stats['ClassHeaderInputs']+=1
   else:require(frame['ClassHeaderObserved']is False,'class short-circuit no extra header read')
   update=eligible and(class_word or costs[1]<struct.unpack_from('<f',words[match],24)[0])
   require(type(frame['UpdateApplied'])is bool and frame['UpdateApplied']==update,'independent new-only duplicate update decision')
   if update:
    require(blob(frame['UpdateBefore120'],120,'update before')==words[match],'update actual prior bytes')
    changed=bytearray(words[match]);put(changed,16,'f',f32(f32(costs[0])-f32(.00001)));put(changed,24,'f',costs[1]);put(changed,28,'f',costs[2]);put(changed,48,'H',emission['Flags'])
    require(blob(frame['UpdateAfter120'],120,'update after')==changed,'independent exact duplicate written lanes');words[match]=bytes(changed);stats['Updates']+=1
   else:require(frame['UpdateBefore120']==''and frame['UpdateAfter120']==''and frame['OutcomeObserved']=='DuplicateNoUpdate','no update bytes/outcome')
   if update:require(frame['OutcomeObserved']=='DuplicateUpdated','updated branch marker')
   if not eligible:stats['ProtectedDuplicates']+=1
   if chain_first!=END:
    raw,popped,retained=cleanup(frame,chain_first,head,created,{x for row in refs for x in row});merge_rows(postrows,raw);stats['RemovedTemps']+=popped;stats['RetainedTemps']+=retained
   else:require(frame['Cleanup']is None,'no created segment => no cleanup call')
  # Authenticate physical chain descriptor/key operands from the exact emitted
  # rows. Allocation/forceDuplicate and NN/cache scalar production remain input.
  if head!=END and all(reference in postrows for reference in [head]):
   chain=[];reference=head
   while reference!=END:
    require(reference in postrows and reference not in chain and len(chain)<4096,'UNOBSERVED full candidate chain row');chain.append(reference);reference=u64(postrows[reference],32)
   require(len(chain)==len(rows)//72,'candidate chain length vs emitted rows')
   for j,reference in enumerate(chain):
    erow=rows[j*72:j*72+72];node=i32at(erow,0);descriptor=i32at(erow,8)-starts[node]
    raw=postrows[reference];require(reference&MASK==u64(erow,48)and i32at(raw,0)==descriptor,'physical emitted owner/descriptor identity')
   require(head==chain[0],'candidate head')
  else:require(head==END,'UNOBSERVED candidate chain')
 pre=case['scoreTrace']['Phases'][0]
 require(words==[blob(w,120,'actual preRank')for w in pre['Words120']]and refs==pre['FlatReferences'],'independent augmentation final275 bytes/refs join')
 require(walker.ba==len(data['Boundaries'])and walker.da==len(data['Delimiters']),'no unexplained source service calls')
 stats['BoundaryOverlayInputs']=walker.overlay
 if last_scratch is not None and case['scoreTrace']['Equalities']:
  call=case['scoreTrace']['Equalities'][0];require((blob(call['LeftBefore4'],4,'rank left start'),blob(call['RightBefore4'],4,'rank right start'))==last_scratch,'augmentation->rank scratch copy continuity')
 return stats

def audit(trace,plain,labels,ignored):
 rank=b.audit(trace,plain,ignored);cases=[candidate_case(c,labels)for c in trace['samples']]
 require(sum(c['Candidates']for c in cases)>0,'positive candidate coverage')
 return {'Status':'PASS_INDEPENDENT219580_SELECTED_DECISIONS_AND_RANK','Cases':cases,'RankAnd140B90':rank,'Actual218560ChainOutputsAreInputs':True,'BoundaryServiceInputsAuthenticated':True,'OverlayResolvedMappingIndependent':False,'ClassHeaderInputsAuthenticatedOnly':True,'InternOwnerC4AuthenticatedInput':True,'OwnerCountsAuthenticatedInputs':True,'FullAllocatorIndependent':False,'ResidualAssociationIndependent':False,'GlyphNNCalibrationIndependent':False,'Request48Lineage':False,'NativeCRTBitIdentity':False}
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('ak',type=Path);p.add_argument('trace',type=Path);p.add_argument('no_trace',type=Path);p.add_argument('fresh_report',type=Path);a=p.parse_args();require(not a.fresh_report.exists(),'fresh output')
 raw=a.ak.read_bytes();require(hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978','AK source')
 al=b.alphabet(raw);reverse={v:k for k,v in al.items()};label=lambda t:reverse.get(t,65535)
 labels=[label('\u0003'),label('\u200d'),label('\U000f0001'),u16(raw,105),label('\u00ad')]
 require(0<a.trace.stat().st_size<=96*2**20 and 0<a.no_trace.stat().st_size<=96*2**20,'bounded C capture files')
 tb=a.trace.read_bytes();pb=a.no_trace.read_bytes();report=audit(json.loads(tb),json.loads(pb),labels,label('\u200d'))
 report.update(AKSHA256=hashlib.sha256(raw).hexdigest(),TraceSHA256=hashlib.sha256(tb).hexdigest(),NoTraceSHA256=hashlib.sha256(pb).hexdigest())
 with a.fresh_report.open('x',encoding='utf8')as f:f.write(json.dumps(report,indent=2)+'\n')
 print(json.dumps(report))
if __name__=='__main__':main()
