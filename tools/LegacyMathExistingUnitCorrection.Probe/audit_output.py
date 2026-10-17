"""Independent finite040CD0 pairing and literal coordinate oracle."""
import json,struct,sys
from pathlib import Path
def f32(x):return struct.unpack('<f',struct.pack('<f',x))[0]
fmax=struct.unpack('<f',bytes.fromhex('ffff7f7f'))[0]
capture=json.loads(Path(sys.argv[1]).read_bytes());assert len(capture['Rows'])==2048 and capture['Guards']==14
holes=rewrites=unmatched=0
for c in capture['Rows']:
 source=c['Existing'];n=len(c['Map']);s=len(source);starts=[];i=0
 while i<s:
  if source[i]>>24:i+=1;continue
  assert i+1<s and source[i+1]>>24==0
  starts.append(i);i+=2
 p=len(starts)
 avg=[[f32(f32(f32(c['Matrix'][r*s+j])+f32(c['Matrix'][r*s+j+1]))*.5) for j in starts] for r in range(n)]
 assert [struct.pack('<f',v) for row in avg for v in row]==[struct.pack('<f',v) for v in c['Average']]
 a=[row[:] for row in avg];matching=[]
 while len(matching)<min(n,p):
  rs={r for r,k in matching};cs={k for r,k in matching}
  a=[[(-fmax if r in rs or k in cs else a[r][k]) for k in range(p)] for r in range(n)]
  for r,k in matching:a[r][k]=fmax
  grouped={}
  for k in range(p):
   value=max(row[k] for row in a)
   if value==-fmax:continue
   r=next(r for r,row in enumerate(a) if row[k]==value);grouped.setdefault(r,[]).append((value,k))
  new=[(r,max(options,key=lambda x:x[0])[1]) for r,options in grouped.items()];assert len(new)>len(matching);matching=new
 assert [r for r,k in matching]==c['MatchingRows'] and [k for r,k in matching]==c['MatchingColumns']
 out=c['Initial'][:];pairrows={k:r for r,k in matching}
 for r,k in matching:out[c['Map'][r]]=source[starts[k]];rewrites+=1
 assert out==c['Output']
 expected=[]
 for k in range(p):
  if k not in pairrows:value=0xffffffff;unmatched+=1
  else:
   r=pairrows[k];value=out[r]
   holes+=value!=out[c['Map'][r]]
  expected.extend([value,value])
 assert expected==c['Associations']
assert holes>100 and rewrites>1000 and unmatched>100
print(f'PASS independent2048 corrections/{rewrites} rewrites/{holes} literal compressed-row discriminators/{unmatched} unmatched pairs/14 guards; prepared only')
