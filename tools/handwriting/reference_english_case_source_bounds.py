"""STATIC source-wide EN auxiliary and conservative case-query output bound.
Raw model only; no target input or execution/publication authority.
"""
from pathlib import Path
import argparse,collections,functools,hashlib,json,runpy,sys
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
sys.argv=['raw-map',str(a.models),str(a.output.with_name(a.output.stem+'-map.json'))]
r=runpy.run_path(str(a.repo/'tools/LegacyEnglishCaseMain.Probe/reference_case_main.py'),run_name='__main__')
sys.argv=['raw-packed',str(a.models),str(a.output.with_name(a.output.stem+'-packed.json'))]
d=runpy.run_path(str(a.repo/'tools/LegacyEnglishLkPacked102.Probe/reference_actual.py'),run_name='__main__')
primary,aux=r['primary'],r['auxiliary'];labels,counts,starts=d['labels'],d['counts'],d['starts'];mask=d['labelMask'];term=d['terminalBit'];rev={v:k for k,v in r['ac']['text'].items()};zwj=rev['\u200d']
assert [(i,v)for i,v in enumerate(aux)if v!=65535]==[(0,zwj),(90,0xfffb)]
assert all(v<217 or v in (0xffff,0xfffb)for v in primary)
edges=collections.Counter();terminals=collections.Counter();control_nodes=[]
for node,packed in enumerate(labels):
 lexical=packed&mask
 if aux[lexical]!=65535:
  edges[lexical]+=1;control_nodes.append(node)
  if packed&term:terminals[lexical]+=1
assert edges=={0:2,90:14} and not terminals
@functools.lru_cache(None)
def bound(node,depth):
 result=0;followed=0
 for child in range(starts[node],starts[node]+counts[node]):
  value=primary[labels[child]&mask]
  if value in (65535,zwj):continue
  if value==0xfffb:
   if depth<7:
    nested,branches=bound(child,depth+1);result+=5*nested;followed+=5*(1+branches)
  else:
   assert value<217;result+=5
 return result,followed
maximum=0;witnesses=[];maximum_followed=0;histogram=collections.Counter()
for node in range(len(labels)):
 value,followed=bound(node,0);maximum_followed=max(maximum_followed,followed);histogram[value]+=1
 if value>maximum:maximum=value;witnesses=[node]
 elif value==maximum and len(witnesses)<8:witnesses.append(node)
 #Retain only the sparse recursive control-node memo, not666k result tuples.
 if node not in control_nodes:bound.cache_clear()
root_output,root_follows=bound(0,0);assert root_follows==0
result={'status':'PASS STATIC raw complete-source bound','targetsRead':False,'vendorExecuted':False,'runtimeCompared':False,
 'dictionaryNodes':len(labels),'dictionaryTerminals':d['terminals'],'mappedAuxiliaryOccurrences':dict(edges),'mappedAuxiliaryTerminalOccurrences':dict(terminals),
 'auxiliaryMapSha256':hashlib.sha256(b''.join(v.to_bytes(2,'little')for v in aux)).hexdigest(),
 'normalCaseOutputUpperBound':maximum,'normalCaseOutputWitnessNodes':witnesses,'normalControlFollowUpperBound':maximum_followed,'normalOutputBoundHistogram':dict(sorted(histogram.items())),
 'initialRootRawChildren':counts[0],'initialRootOutputUpperBound':root_output,'initialRootControlFollowUpperBound':root_follows,
 'boundPolicy':'Conservative <=5 case outputs per accepted inner edge; onlyFFFB recurses, ZWJ drops, eight native levels. Not a runtime count or source-owner receipt.',
 'auxiliaryConclusion':'Fixed60 map plus native terminal-only093F50 gives zero backend32 rows at every node. Must also validate actual forwarding/map/lifetime before use.',
 'poolConclusion':'The first-root400-row/1289-node reserve does not authorize source-wide requests.'}
a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps({k:v for k,v in result.items()if k!='normalOutputBoundHistogram'}))
