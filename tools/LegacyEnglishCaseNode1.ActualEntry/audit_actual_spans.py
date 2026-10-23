"""Scoped actual entry structural/consumed-mask audit, NOT whole native numeric oracle."""
import hashlib,json,struct,sys
from pathlib import Path
x=Path(sys.argv[1]).read_bytes();d=json.loads(x)
assert d['Completed'] is True and d['Admission']=='NONE' and d['InputSha']=='d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99' and d['OriginalIndex']==4
assert (d['GraphNodes'],d['RawPoints'],d['RawStrokes'],d['Resolution'])==(2,3,1,96)
assert d['GlobalReadinessGateUnchanged'] is True
assert 0<d['Count']<=125 and d['ScoringCalls']==d['AcceptedAllocations'] and d['SelectedD0Rows']==2
assert 'span0+span1' in d['PrefixScope']
p=bytes.fromhex(d['ParentRows88']);pm=bytes.fromhex(d['ParentRowMasks']);a=bytes.fromhex(d['ParentAlternatives80']);am=bytes.fromhex(d['ParentAlternativeMasks'])
assert len(p)==len(pm)==125*88 and len(a)==len(am)==125*80
read=list(range(34))+list(range(36,39))+list(range(40,62));finite=[];unknown=[]
for n in range(125):
 row=p[n*88:n*88+88];mask=pm[n*88:n*88+88]
 assert mask[16:20]==bytes([255]*4)
 if struct.unpack_from('<i',row,16)[0]>=0x4000000:continue
 assert all(mask[k]==255 for k in read)
 for at in [n*80,n*80+40]:
  assert am[at:at+4]==bytes([255]*4)
  if struct.unpack_from('<i',a,at)[0]<0x4000000:assert am[at+4:at+24]==bytes([255]*20)
 finite.append(n);unknown.append(tuple(i for i in range(88) if mask[i]!=255))
raw=bytes.fromhex(d['QueryRows32']);assert len(raw)==d['QueryRows']*32 and len(d['QueryFrames'])==d['QueryPages']
position=0;previousParent=-1;skip=0;parents=[]
parentFrames=[f for f in d['QueryFrames'] if f['ParentOrdinal']>=0]
rootFrames=[f for f in d['QueryFrames'] if f['ParentOrdinal']==-1]
assert d['QueryFrames']==parentFrames+rootFrames and rootFrames
for f in parentFrames:
 parent=f['ParentOrdinal'];count=f['Returned'];assert parent in finite and 0<=count<=128
 if parent!=previousParent:
  assert parent>previousParent
  if previousParent!=-1:assert previousCount==0
  skip=0;parents.append(parent)
 assert f['Skip']==skip and f['SourceTag']==struct.unpack_from('<Q',p,parent*88)[0]
 for at in range(position,position+count*32,32):
  row=raw[at:at+32];assert row[4]==1 and row[22:24]==b'\0\0'
  assert struct.unpack_from('<H',row,20)[0]<217
 position+=count*32;skip+=count;previousParent=parent;previousCount=count
assert parents==finite and previousCount==0
rootRows=0;rootSkip=0
for f in rootFrames:
 assert f['SourceTag']==8<<40 and f['Skip']==rootSkip and 0<=f['Returned']<=128
 rootRows+=f['Returned'];rootSkip+=f['Returned']
 for at in range(position,position+f['Returned']*32,32):
  row=raw[at:at+32];assert row[4]==1 and row[22:24]==b'\0\0' and struct.unpack_from('<H',row,20)[0]<217
 position+=f['Returned']*32
assert rootFrames[-1]['Returned']==0 and position==len(raw)
assert len(d['SelectedOrdinals'])==len(d['SelectedCosts'])==2 and all(len(c)==220 for c in d['SelectedCosts'])
assert len(bytes.fromhex(d['Heap128']))==250*128 and len(bytes.fromhex(d['Rows88']))==125*88 and len(bytes.fromhex(d['Alternatives80']))==125*80
rows=bytes.fromhex(d['Rows88']);rootRetained=sum(rows[i*88+32]==1 for i in range(d['Count']));parentRetained=d['Count']-rootRetained
assert all(rows[i*88+32] in (0,1) for i in range(d['Count']))
print(json.dumps({'scope':'Actual-entry structural/source-frame/definedness only; no full numeric/word/quality equivalence','sha256':hashlib.sha256(x).hexdigest(),'finiteParents':len(finite),'finiteUnknownPatterns':[list(x) for x in sorted(set(unknown))],'unknownMaskBytesAllParents':sum(x!=255 for x in pm),'queryRows':d['QueryRows'],'queryFrames':d['QueryPages'],'rootQueryRows':rootRows,'rootQueryFrames':len(rootFrames),'retainedRootSpan1Rows':rootRetained,'retainedParentSpan0Rows':parentRetained,'primaryRows':d['Count'],'scoringCalls':d['ScoringCalls'],'accepted':d['AcceptedAllocations'],'selectedD0Rows':d['SelectedD0Rows'],'noNewNNReplay':True},sort_keys=True))
