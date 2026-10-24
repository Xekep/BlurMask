"""Independent DATA audit of owning augmentation exports; no NN or authority."""
import base64,copy,json,math,pathlib,struct,sys
U64=lambda b,i:struct.unpack_from('<Q',b,i)[0]
I32=lambda b,i:struct.unpack_from('<i',b,i)[0]
U16=lambda b,i:struct.unpack_from('<H',b,i)[0]
F32=lambda b,i:struct.unpack_from('<f',b,i)[0]
decode=lambda s:base64.b64decode(s,validate=True)
def check(a,prior):
 assert a['Scenario']=='new-success' and a['NNSourceInvocations']==1
 compact=decode(a['RawCompact']);n=len(compact)//40
 assert len(compact)%40==0 and compact==decode(prior['compact'])[:prior['count']*40]
 assert a['ActualBit8']==sum(bool(compact[40*i+32]&8)for i in range(n))>0
 depths=[0]*n
 for start in range(n):
  path=[];i=start
  while i!=-1 and depths[i]==0:
   depths[i]=-1;path.append(i);i=I32(compact,40*i+16);assert -1<=i<n
  assert i==-1 or depths[i]>0
  d=0 if i==-1 else depths[i]
  for i in reversed(path):d+=1;depths[i]=d
 t=a['Topology'];assert t['Count']==n and t['MaximumParentDepth']==max(depths)
 end=(a['BeforeState']['SpanLast']>>7)&0xffffff
 assert end<len(prior['starts'])-1
 lo,hi=prior['starts'][end:end+2]
 assert (t['CandidateStart'],t['CandidateEnd'])==(lo,hi)
 assert t['Candidates']==sum(bool(compact[40*i+32]&8)for i in range(lo,hi))>0
 recordCounts=[];wordCounts=[];allChains=[]
 for name in ['BeforeState','AfterState']:
  state=a[name];assert state and len(state['Words'])in range(1,4097)
  assert (state['SpanFirst'],state['SpanLast'])==(a['BeforeState']['SpanFirst'],a['BeforeState']['SpanLast'])
  assert len(set(state['SelectedOrdinals']))==len(state['SelectedOrdinals']) and all(0<=i<len(state['Words'])for i in state['SelectedOrdinals'])
  records={};total=0
  for node in state['Pool']:
   key=node['Key'];b=decode(node['Records']);packed=node['PackedCount']
   assert key==key&0x010000ffffffffff and len(b)%40==0
   count=(packed&0x7fff)>>2
   assert count==len(b)//40 and key not in records and count<=2040
   records[key]=b;total+=count
  assert len(records)<=4096 and total<=65536
  def row(ref):
   key=ref&0x010000ffffffffff;index=(ref>>40)&65535
   assert key in records and index<32768 and index<len(records[key])//40
   return records[key][index*40:index*40+40]
  chains=[]
  for item in state['Words']:
   word=decode(item['Storage120']);assert len(word)==120
   for offset in [16,20,24,28]:assert math.isfinite(F32(word,offset))
   ref=U64(word,8);chain=[];seen=set()
   while ref!=2**64-1:
    assert ref not in seen and len(chain)<4096
    seen.add(ref);chain.append(ref);glyph=row(ref)
    assert U16(glyph,4)&7==0
    for offset in [12,16]:assert math.isfinite(F32(glyph,offset))
    ref=U64(glyph,32)
   #13FE20 stores the flat vector in reading order, opposite the reverse-linked head.
   assert chain[::-1]==item['FlatReferences'] and chain
   chains.append(chain)
  recordCounts.append(total);wordCounts.append(len(state['Words']));allChains.append(chains)
 r=a['Result'];assert r['Candidates']<=t['Candidates'] and r['Candidates']==r['Appended']+r['Duplicates']
 assert wordCounts[0]==1 and 1<=wordCounts[1]<=1+r['Appended']
 assert r['Words']==[w['Storage120']for w in a['AfterState']['Words']]
 assert len(decode(r['Source26']))%26==0 and a['BeforeSource26Available']is False
 return dict(topologyDepth=max(depths),eligibleInLastNode=t['Candidates'],wordCounts=wordCounts,poolRecordCounts=recordCounts,linkedChains=sum(wordCounts),scope='Independent topology, complete pool references and linked WORD consistency only; not full219580 numerical/quality proof',NNCalls=0)
if __name__=='__main__':
 a=json.loads(pathlib.Path(sys.argv[1]).read_bytes());p=json.loads(pathlib.Path(sys.argv[2]).read_bytes());report=check(a,p)
 mutations=[('word-head',lambda x:x['AfterState']['Words'][0].__setitem__('Storage120',base64.b64encode(bytes(120)).decode())),('flat-reference',lambda x:x['AfterState']['Words'][0]['FlatReferences'].append(0)),('topology',lambda x:x['Topology'].__setitem__('MaximumParentDepth',999)),('eligible',lambda x:x.__setitem__('ActualBit8',0)),('pool-count',lambda x:x['AfterState']['Pool'][0].__setitem__('PackedCount',0)),('selected',lambda x:x['AfterState']['SelectedOrdinals'].append(9999)),('result-words',lambda x:x['Result']['Words'].clear())]
 for name,mutate in mutations:
  b=copy.deepcopy(a);mutate(b)
  try:check(b,p)
  except (AssertionError,KeyError):pass
  else:raise AssertionError('Accepted corruption: '+name)
 report['tamperRefusals']=[name for name,_ in mutations]
 pathlib.Path(sys.argv[3]).write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps(report))
