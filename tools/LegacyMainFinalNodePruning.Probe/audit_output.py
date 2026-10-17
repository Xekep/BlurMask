"""Independent raw-binary32 geometry + literal local/full mutable deletion."""
import copy,json,pathlib,struct,sys
j=json.loads(pathlib.Path(sys.argv[1]).read_bytes())
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def rounded(x):
 v=struct.unpack('<I',struct.pack('<f',x))[0];m=v&0x7fffffff
 if m>=0x4b000000:return x
 if m<0x3f800000:v=(v&0x80000000)|(0x3f800000 if m>=0x3f000000 else 0)
 else:
  shift=150-(m>>23);v=((v>>shift)+((v>>(shift-1))&1))<<shift
 return struct.unpack('<f',struct.pack('<I',v))[0]
def costs(c):
 out=[]
 for ranges in c['groups']:
  boxes=[]
  for stroke,start,end in ranges:
   points=c['strokes'][stroke][start:end];assert points;boxes.append((min(p[0] for p in points),max(p[0] for p in points)))
  total=0
  for n in range(1,len(boxes)):
   left,right=boxes[:n],boxes[n:]
   gap=max(f(min(p[0] for p in right)-max(p[1] for p in left)),f(min(p[0] for p in left)-max(p[1] for p in right)))
   x=f(f(f(gap/c['height'])-c['gap'])*f(-c['slope']))
   log=0 if x>4 else j['table'][int(f(f(f(x+4)*255)+.5))>>3] if x>=-4 else x
   rounded_cost=rounded(f(f(-log)*8192));total=total+int(rounded_cost) if f(f(total)+rounded_cost)<0x4000000 else 0x4000000
  out.append(total)
 return out
def prune(c,geometry,include_final):
 rows=copy.deepcopy(c['before']);labels=[list(range(len(v))) for v in rows];deleted=[]
 for node in range(len(rows) if include_final else len(rows)-1):
  for ordinal in range(len(rows[node])-1,-1,-1):
   arc=rows[node][ordinal]
   if arc[1]==0 or geometry[arc[0]]<=c['threshold']:continue
   deleted.append([node,labels[node][ordinal],arc[0],geometry[arc[0]]])
   for nn,row in enumerate(rows):
    for other in row:
     if nn-other[1]-1==node:other[5]=[p-(p>ordinal) for p in other[5] if p!=ordinal]
     other[6]=[[d,p-(p>ordinal)] if nn+d==node else [d,p] for d,p in other[6] if nn+d!=node or p!=ordinal]
   del rows[node][ordinal];del labels[node][ordinal]
 return rows,labels,deleted
actual_removed=all_removed=final_controls=0
for c in j['cases']+j['actual']:
 geometry=costs(c);local,_,ld=prune(c,geometry,False);complete,labels,cd=prune(c,geometry,True)
 assert local==c['local'] and complete==c['complete'] and labels==c['ordinals']
 assert len(ld)==c['localRemoved'] and len(cd)==c['completeRemoved']
 #The only additional decisions are at the actual native source-final node;
 #all earlier retained headers and original ordinals keep their identities.
 extra=[r for r in cd if r[0]==len(c['before'])-1]
 assert len(extra)==len(cd)-len(ld)
 assert all(labels[n]==[i for i in range(len(c['before'][n])) if not any(d[0]==n and d[1]==i for d in cd)] for n in range(len(labels)))
 if extra:final_controls+=1
 all_removed+=len(cd)
 if c['actual']:
  actual_removed+=len(extra)
  if extra:assert len(extra)==4 and c['height']==40 and c['threshold']==20480
assert len(j['cases'])==j['prepared']==72 and len(j['actual'])==j['actualCount']==4
assert j['actualFinalRemoved']==actual_removed==12 and j['sourceChecks']>0 and j['guards']==8
assert final_controls>3
print(f'PASS independent76 local/full graph pairs,{all_removed} raw-cost removals/{final_controls} final-rejection cases,12 genuine actual final removals; exact ordinals/links preserved')
