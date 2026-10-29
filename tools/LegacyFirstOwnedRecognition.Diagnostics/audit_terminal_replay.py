from pathlib import Path
import json,struct,sys
INF=0x4000000
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
count=0
for d in json.loads(Path(sys.argv[1]).read_text()):
 def rows(k):return [bytes.fromhex(x)for x in d[k]]
 def record(k,n,r,c):return rows(k)[n][(r*d['cap8']+c)*40:(r*d['cap8']+c+1)*40]
 def sequence(nodes,identity):
  out=[];seen=set()
  while identity:
   assert identity not in seen;seen.add(identity);v=nodes[identity];out.append(v['symbol']);identity=v['parent']
  return tuple(reversed(out))
 actual={x['identity']:x for x in d['actualContexts']};observed={x['identity']:x for x in d['contexts']}
 #IDs come from separate genuine allocations. Validate their complete ancestry,
 #and compare every other byte, including inactive/reserved record storage.
 for a,b in [('actualB0','b0'),('actualB8','b8')]:
  assert len(d[a])==len(d[b])
  for x,y in zip(rows(a),rows(b)):
   assert len(x)==len(y) and len(x)%40==0
   for off in range(0,len(x),40):
    u,v=x[off:off+40],y[off:off+40]
    assert u[:24]==v[:24] and u[32:]==v[32:]
    ui=struct.unpack_from('<Q',u,24)[0];vi=struct.unpack_from('<Q',v,24)[0]
    assert sequence(actual,ui)==sequence(observed,vi)
 n=len(d['actualB8'])-1;best=None
 for r in range(d['beam']):
  for c in range(d['cap8']):
   v=record('actualB8',n,r,c);cost=struct.unpack_from('<i',v)[0]
   if cost>=INF:break
   assert v[32]==0 and struct.unpack_from('<H',v,12)[0]!=d['trailing']
   if best is None or cost<best[0]:best=(cost,r,c)
 assert best is not None and best[0]==d['total']
 _,r,c=best;v=record('actualB8',n,r,c);assert struct.unpack_from('<i',v,4)[0]==d['acoustic']
 reverse=[]
 while n>=0:
  v=record('actualB8',n,r,c);symbol,packed,arc=struct.unpack_from('<HHH',v,12);span=packed>>1&7
  reverse.append([n,arc,span,symbol,r,c,packed&1]);n-=span+1;r,c=v[9],v[10]
 assert list(reversed(reverse))==d['selected']
 w=bytes.fromhex(d['wordStorage']);assert len(w)==120
 #WORD unadjusted normalized cost and acoustic are independent of the TOP
 #relative anchor; no unknown source-kind multiplier is inferred here.
 assert w[20:24]==struct.pack('<f',1)
 assert w[24:28]==struct.pack('<f',f(f(d['total'])/f(len(d['actualB8'])*8192)))
 assert w[28:32]==struct.pack('<f',f(f(d['acoustic'])*f(0.00012207031)))
 count+=len(reverse)
print('PASS independent terminal minimum/backtrack/WORD20,24,28 plus complete record storage with validated symbolic identity ancestry',count)
