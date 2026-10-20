"""Independent raw geometry/AL/C21 -> complete candidate/temp DOM and ordered
CB8 registry/patch/prefix oracle. Uses tracked numerical helpers only.
"""
from pathlib import Path
import copy
import sys

# Reuse independently checked stdlib/NumPy setup and native raw AL reader;
# stop BEFORE its fixture-specific assertion loop. No vendor code is run.
source=Path(__file__).resolve().parents[1]/'LegacyMathOriginalFieldSource.Probe/audit_output.py'
setup=source.read_text(encoding='utf-8-sig').split('feature_error = 0.; expected_first = None')[0]
exec(compile(setup,str(source),'exec'),globals())
utf8=load('native_range_utf8',root/'tools/LegacyMainSourceRangeUtf8.Probe/reference_controls.py')

def rich_range(pair):
 def p(v):return dict(Value=v,SourceIndex=v>>32,PointIndex=v>>16&65535,Fraction=v>>8&255,Mode=v&255)
 return dict(First=p(pair[0]),Last=p(pair[1]))

def format_ranges(rows):
 return base64.b64decode(utf8.replay(rows)['utf8']).decode()

def model(evidence,fail_selection=-1):
 candidate=[];temporary=[];registry=[];pending=[];patch=[];old=[];new=[];events=[]
 count=len(evidence['Rows']);range_count=sum(map(len,evidence['Ranges']));nbytes=sum(r['ByteCount']for r in evidence['Rows'])
 reservation=evidence['CopyReservation']+8192+count*1536+nbytes*32+range_count*512+1024
 state=dict(Complete=False,Failed=False,Released=False,Reservation=reservation,Phase='claimed',TemporaryNext=2,
  RegistryNext=0,HeaderLive=False,CleanupCount=0,SharedNames=True,DistinctInks=True,
  Candidate=candidate,Temporary=temporary,Registry=registry,Pending=pending,Patch=patch,OldTags=old,NewTags=new)
 def observe(name,index=-1):
  state['Phase']=name;events.append((name,index,copy.deepcopy(state)))
 def node(arena,kind,name,parent=0,origin=0,value_tag=0,text=None,number=None,uri=None,prefix=None):
  row=dict(Id=len(arena)+1,Kind=kind,Name=name,Parent=parent,Origin=origin,ValueTag=value_tag,Text=text,Number=number,NamespaceUri=uri,NamespacePrefix=prefix)
  arena.append(row);return row['Id']
 def string_tag(text):return 3 if not text else 2 if len(text.encode())<16 else 1
 def attr(parent,name,tag,text=None,number=None):return node(candidate,0,name,parent,value_tag=tag,text=text,number=number)
 def string_attr(parent,name,text):return attr(parent,name,string_tag(text),text=text)
 def temporary_id(name,ranges):
  identity=state['TemporaryNext'];state['TemporaryNext']+=1
  has_selection=len(pending)!=fail_selection
  pending.append(dict(TemporaryId=identity,Name=name,ResolvedId=-1,HasSelection=has_selection,Ranges=[rich_range(r)for r in ranges]if has_selection else []))
  observe('PendingLinked',len(pending)-1)
  if not has_selection:raise ValueError('selection-refused')
  return identity
 observe('Claimed');observe('OuterCopied')
 domain=values(evidence['Field']['SelectedRanges']);assert len(domain)==1
 state['RegistryNext']=1;registry.append(dict(Id=0,Name='INPUT',Category=102,Ranges=[rich_range(r)for r in domain]));observe('ThirdRegistryCloned')
 selected=evidence['Field']['SelectedNodes']
 def clone_field(identity,parent):
  n=next(n for n in selected if n['Id']==identity)
  actual=node(temporary,n['Kind'],n['Name'],parent,identity,n['ValueTag'],n['StringValue'],n['NumericValue'],n['NamespaceUri'],n['NamespacePrefix'])
  for attr_node in selected:
   if attr_node['Parent']==identity and attr_node['Kind']==0:clone_field(attr_node['Id'],actual)
  for child in selected:
   if child['Parent']==identity and child['Kind']!=0:clone_field(child['Id'],actual)
  return actual
 actual_fields=[n for n in selected if n['Kind']==3 and n['Name']=='mathField'
  and any(a['Kind']==0 and a['Name']=='id'and a['Parent']==n['Id']and a['StringValue']=='formula'for a in selected)]
 assert len(actual_fields)==1
 temporary_document=node(temporary,4,'');field_root=clone_field(actual_fields[0]['Id'],temporary_document)
 old_root=node(temporary,3,'mathResult',field_root);observe('FieldCloned')
 root_id=node(candidate,4,'');result=node(candidate,3,'mathResult',root_id);observe('CandidateRoot')
 sequence=node(candidate,3,'sequence',result);observe('Sequence')
 try:
  for i,(row,ranges)in enumerate(zip(evidence['Rows'],evidence['Ranges'],strict=True)):
   symbol=node(candidate,3,'symbol');pairs=[[r['First'],r['Last']]for r in ranges]
   if pairs:
    string_attr(symbol,'range',format_ranges(pairs));tag=temporary_id('MATH_SYMBOL',pairs);attr(symbol,'tag',4,number=-tag)
   text=base64.b64decode(row['Bytes']).split(b'\0',1)[0].decode()
   node(candidate,1,'',symbol,value_tag=string_tag(text),text=text)
   candidate[symbol-1]['Parent']=sequence;observe('Symbol',i)
  full=format_ranges(domain)
  segment_list=node(candidate,3,'symbolSegmentList',result);observe('SegmentList')
  segment=node(candidate,3,'symbolSegment',segment_list);string_attr(segment,'range',full);observe('Segment')
  choice=node(candidate,3,'symbolCandidate',segment);attr(choice,'nrs',10,number=0);observe('Candidate')
  char=node(candidate,3,'char',choice);node(candidate,1,'',char,value_tag=2,text='?');observe('Character')
  terminal=node(candidate,3,'mathTerminalNode',result);string_attr(terminal,'name','');string_attr(terminal,'ref',full);attr(terminal,'selected',7,number=0)
  tag=temporary_id('MATH_NODE',domain);attr(terminal,'tag',4,number=-tag);observe('Terminal')
 except ValueError:
  state['Failed']=True;return events,state
 children=[n['Id']for n in candidate if n['Parent']==result];assert len(children)==3
 def clone(identity,parent=0):
  n=candidate[identity-1]
  cloned=node(temporary,n['Kind'],n['Name'],parent,identity,n['ValueTag'],n['Text'],n['Number'],n['NamespaceUri'],n['NamespacePrefix'])
  #Native clone visits attributes before linked children.
  for attr_node in candidate:
   if attr_node['Parent']==identity and attr_node['Kind']==0:clone(attr_node['Id'],cloned)
  for child in candidate:
   if child['Parent']==identity and child['Kind']!=0:clone(child['Id'],cloned)
  return cloned
 state['HeaderLive']=True
 for ordinal,identity in enumerate(children):
  payload=clone(identity);observe('PayloadCloned',ordinal)
  patch.append(dict(Operation=2,Ordinal=ordinal,CandidateRoot=identity,PayloadRoot=payload,Parent=old_root,Applied=False));observe('ActionQueued',ordinal)
 for action in patch:
  temporary[action['PayloadRoot']-1]['Parent']=old_root;action['Applied']=True;observe('PatchApplied',action['Ordinal'])
 state['HeaderLive']=False;observe('HeaderFreed')
 def walk(identity):
  for child in temporary:
   if child['Parent']==identity and child['Kind']!=0:
    yield child
    yield from walk(child['Id'])
 for n in walk(old_root):
  for a in temporary:
   if a['Parent']!=n['Id']or a['Kind']!=0 or a['Name']!='tag':continue
   assert a['ValueTag']==4 and a['Number']<0
   p=next(p for p in pending if p['TemporaryId']==-a['Number'])
   if p['ResolvedId']<0:
    assigned=state['RegistryNext'];state['RegistryNext']+=1
    registry.append(dict(Id=assigned,Name=p['Name'],Category=102,Ranges=copy.deepcopy(p['Ranges'])));p['ResolvedId']=assigned
    observe('Registered',p['TemporaryId']-2)
   a['Number']=p['ResolvedId'];observe('TagWritten',p['TemporaryId']-2)
 for n in walk(old_root):
  for a in temporary:
   if a['Parent']==n['Id']and a['Kind']==0 and a['Name']=='tag':new.append(a['Number'])
 observe('NewTags');observe('StaleCleaned');state['CleanupCount']=1;observe('Complete');state['Complete']=True
 return events,state

max_error=0
for c in data['Actual']:
 evidence=c['Evidence'];ink=[[[p['X'],p['Y']]for p in s['Points']]for s in evidence['Ink']];e=ns['expected'](ink)
 assert c['Policy']==('ColdDefault'if c['Width']==1 else'PreparedSearch')
 assert (evidence['Order'],evidence['Positions'],evidence['Separators'])==(e['order'],e['positions'],e['ends'])
 got=np.asarray(evidence['FeatureBits'],dtype=np.int64).astype(np.uint32).view(np.float32);want=features26(e['sampled'],297).ravel()
 error=float(np.max(np.abs(got-want)));max_error=max(max_error,error);assert np.allclose(got,want,rtol=1e-5,atol=1e-6)
 f=evidence['Field'];assert f['OriginalNodes']==field_nodes and f['SelectedNodes']==selected_nodes
 assert (f['Tag'],f['Next'],f['Category'])==(0,1,102)and f['SourcePrepared']and f['InferenceClaimed']
 assert values(f['C23Ranges'])==e['originals']==values(f['SelectedRanges'])
 extra=sum(map(len,ink))*8+2*12*64+2*len('formula')+2*len(namespace.encode())+4*16+2*160
 assert f['PayloadReservation']==extra
 assert evidence['PositionReservation']==extra+sum(e['counts'])*8+len(e['positions'])*8+len(ink)*128+32
 assert evidence['Ranges']==range_rows(evidence,e)
 pool={}
 for unit,row in zip(evidence['Units'],evidence['Rows'],strict=True):
  text=intervals[unit&65535]
  if text not in pool:pool[text]=len(pool)+1
  assert row==dict(Word=unit,Identity=pool[text],ByteCount=len(text),Bytes=base64.b64encode(text).decode())
 assert len(evidence['Primary'])==len(evidence['Secondary'])
 strings=sum(5*len(v)+1 for v in pool);peak=max((5*len(v)for v in pool),default=0)
 assert evidence['AttachmentReservation']>evidence['PositionReservation']
 assert evidence['CopyReservation']==evidence['AttachmentReservation']+96+4*len(evidence['Primary'])+68*len(evidence['Units'])+8*len(evidence['Runs'])+16*sum(map(len,evidence['Ranges']))+strings+peak
 events,result=model(evidence);assert c['Publication']==result
for width in(1,3,16):
 pair=[c for c in data['Actual']if c['Width']==width];assert len(pair)==2
 assert pair[0]['Evidence']['RawHashes']==pair[1]['Evidence']['RawHashes']
assert len(data['Actual'])==6
baseline=data['Actual'][0]['Evidence'];events,result=model(baseline)
assert data['Events']==[dict(Name=name,Index=index)for name,index,_ in events]
assert len(data['Failures'])==len(events)*2
for (name,index,state),pair in zip(events,[data['Failures'][i:i+2]for i in range(0,len(data['Failures']),2)],strict=True):
 for canceled,failure in zip((False,True),pair,strict=True):
  assert(failure['Stage'],failure['Index'],failure['Canceled'])==(name,index,canceled)
  assert failure['Before']==state
  assert failure['After']==dict(state,Failed=True)
_,refused=model(baseline,0);assert data['RefusedSelection']==refused
assert data['Gc']==dict(DetachedAllowsSourceCollection=True,SourceBackedRetainsSource=True,Publication=result)
assert data['Concurrent']==dict(Completed=1,Refused=1,BorrowedCopyLive=True)
assert data['Guards']==180
if len(sys.argv)>=4:assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[3]).read_bytes()
print(f'PASS6 actual cold1/prepared3/16 NN rows->rawAL/C79/C21->full typed candidate/tempDOM/3ordered clonedop2/thirdregistry;{len(data["Failures"])} independent prefixes/180guards/GC; feature error{max_error}')
