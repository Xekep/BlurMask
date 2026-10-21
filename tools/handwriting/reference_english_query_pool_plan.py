"""Raw actual English map/root-query facts and labelled pool projections.
No handwriting labels, inferred controller qualifiers, NN or vendor execution.
"""
from pathlib import Path
import argparse,hashlib,json,runpy,sys,struct
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
a.output.parent.mkdir(parents=True,exist_ok=True);suffix=hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]
sys.argv=['reference',str(a.models),str(a.output.with_name('eqp-'+suffix+'-dictionary.json'))]
d=runpy.run_path(str(a.repo/'tools/LegacyEnglishLkPacked102.Probe/reference_actual.py'),run_name='__main__')
mapping={x['lexical']:x['exactAcoustic']for x in d['mapping']};initial=[];seen=set()
for lexical in range(104):
 symbol=mapping[lexical]
 if symbol is not None and symbol not in(0xffff,0xfffd)and symbol not in seen:initial.append(symbol);seen.add(symbol)
assert len(initial)==86 and len(mapping)==104
root=[];rowsDigest=hashlib.sha256();first=d['starts'][0]+1;end=d['starts'][0]+d['counts'][0]
for node in range(first,end):
 lexical=d['labels'][node]&127;symbol=mapping[lexical]
 if symbol is None:continue
 flags=1|(2 if d['labels'][node]&128 else 0);tag=node|(1<<32)|(flags<<40)
 assert (tag>>42)&1==0
 row=dict(node=node,lexical=lexical,acoustic=symbol,flags=flags,sourceId=1,suppressFromBit42=(tag>>42)&1)
 root.append(row);rowsDigest.update(struct.pack('<IHHBB',node,lexical,symbol,flags,1))
#This projects ALL actual ordinary root-symbol grafts, WITHOUT claiming that
#native frontend110/NN accepts every raw edge or reaches these allocations.
rootSymbols={row['acoustic']for row in root};total=len(initial)+1+len(rootSymbols)
used=[];capacity=0
for node in range(total):
 if not used or used[-1]==102:
  need=len(used)+1
  if capacity<need:capacity=max(need,int(capacity*1.5))
  used.append(0)
 used[-1]+=1
assert len(root)>0 and total>104 and len(used)==2
result=dict(status='PASS raw EN dictionary/source-tag and prepared context-growth projection',initialMappedContexts=86,lexicalPositions=104,rawRootChildren=d['counts'][0],rootSkippedChild=first-1,ordinaryMappedRootRows=len(root),rootRecordDigest=rowsDigest.hexdigest(),rootRows=root,ordinarySourceTagBit42AlwaysZero=True,allRootGraftsPreparedProjection=dict(initial=86,sentinel=1,distinctRawSymbols=len(rootSymbols),requiredNodes=total,pageUsed=used,pageDescriptorCapacity=capacity,existingMapOnly104BoundInsufficient=True,actualNativeQualificationOrReachabilityClaim=False),nativePoolAlphabetBound=None,targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(result,indent=2)+'\n');print('PASS EN pool raw:86 initial contexts,',len(root),'ordinary mapped root rows, prepared all-root graft needs',total,'nodes; NOT actual NN reachability')
