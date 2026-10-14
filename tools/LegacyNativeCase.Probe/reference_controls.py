"""Independent set-domain null-output count oracle, no vendor code execution."""
import json,sys
from collections import Counter
from pathlib import Path
if len(sys.argv)!=3 or Path(sys.argv[2]).suffix.lower()!='.json':raise ValueError('data.json output.json')
r=json.loads(Path(sys.argv[1]).read_text(encoding='utf8'))
lower=set();upper=set();lower_block=set();upper_block=set()
for lo,hi in r['alternatingRanges']:
 lower_block.update(range(lo,hi+1));upper_block.update(range(lo+1,hi+2))
 lower.update(range(lo,hi+1,2));upper.update(range(lo+1,hi+2,2))
for lo,hi,_ in r['lowerRanges']:lower.update(set(range(lo,hi+1))-lower_block)
for cp,_ in r['lowerSingles']:
 if cp not in lower_block:lower.add(cp)
for lo,hi,_ in r['upperRanges']:upper.update(set(range(lo,hi+1))-upper_block)
for cp,_ in r['upperSingles']:
 if cp not in upper_block:upper.add(cp)
counts=Counter(row[0] for row in r['specialRows'])
counts.update(lower|upper);counts.pop(0xb5,None)
out=[[cp,v] for cp,v in sorted(counts.items())]
#Dictionary/set implementation is separate from the library binary searches.
lo={cp:cp+delta for a,b,delta in r['lowerRanges'] for cp in range(a,b+1)}
lo.update({cp:cp+d for cp,d in r['lowerSingles']})
hi={cp:cp+delta for a,b,delta in r['upperRanges'] for cp in range(a,b+1)}
hi.update({cp:cp+d for cp,d in r['upperSingles']})
for a,b in r['alternatingRanges']:
 for cp in range(a,b+1):
  lo.pop(cp,None)
  if (cp-a)%2==0:lo[cp]=cp+1
 for cp in range(a+1,b+2):
  hi.pop(cp,None)
  if (cp-a)%2==1:hi[cp]=cp-1
ti={cp:cp+d for cp,d in r['titleSingles']}
seq=[row[0] for row in r['sequenceScalars']]
variants={}
for row in r['specialRows']:
 cp,lp,ln,tp,tn,up,un,lang,context=row
 variants.setdefault(cp,[]).append([seq[lp:lp+ln],seq[up:up+un],seq[tp:tp+tn],lang,context])
for cp in lo.keys()|hi.keys():
 variants.setdefault(cp,[]).append([[lo.get(cp,cp)],[hi.get(cp,cp)],[ti.get(cp,hi.get(cp,cp))],0,0])
variants.pop(0xb5,None)
assert {cp:len(v) for cp,v in variants.items()}==dict(out)
Path(sys.argv[2]).write_text(json.dumps(dict(counts=out,variants=[[cp,v] for cp,v in sorted(variants.items())]))+'\n',encoding='utf8')
print(f'Generated{len(out)} nonzero scalar counts; all other valid Unicode scalars must return0.')
