"""Literal selected262 sort/prune/materialize SOURCE controls; no model forward."""
import struct,json,pathlib,sys
INF=0x4000000

def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def sort_ids(costs,ids):
 ids=list(ids)
 def go(lo,hi):
  while True:
   pivot=costs[ids[(lo+hi)//2]];i=lo;j=hi
   while True:
    while costs[ids[i]]<pivot:i+=1
    while pivot<costs[ids[j]]:j-=1
    if j<i:break
    ids[i],ids[j]=ids[j],ids[i];i+=1;j-=1
    if j<i:break
   if lo<j:go(lo,j)
   lo=i
   if hi<=i:return
 if ids:go(0,len(ids)-1)
 return ids

def finalize(costs,skip_prune=False):
 assert len(costs)==262 and all(type(x)is int and -2**31<=x<2**31 for x in costs)
 c=list(costs);rank=sort_ids(c,[i for i in range(262)if i!=259 and c[i]<INF]);threshold=None
 if rank and not skip_prune:
  # Literal14470, actual C0=46000/C4=0/C6=10. Strict best<rank10 is significant.
  threshold=46000
  if len(rank)>10:
   if c[rank[0]]<c[rank[10]] and c[rank[10]]<=threshold:threshold=c[rank[10]]
  if threshold<c[rank[0]]:threshold=c[rank[0]]
  for i in range(262):
   if (i!=259 and threshold<=c[i])or(i==259 and 0<=c[i]):c[i]=INF
 # Single-stroke native14660 scale pow(1,D8)=1; F32 conversion still happens.
 for i in range(262):
  if c[i]<INF:c[i]=min(INF,int(f(f(c[i])*f(1))))
 # Rank is not recomputed after pruning/scaling. 15EB0 first20 includes reserved IDs.
 output=[];inspected=[]
 for label in rank[:20]:
  inspected.append(label)
  if INF<=c[label]:break
  if label<259:output.append([label,c[label]])
 return dict(rank=rank,threshold=threshold,costs=c,inspected=inspected,output=output)

def controls():
 rows=[]
 def case(name,changes,skip=False):
  c=[INF]*262
  for k,v in changes:c[k]=v
  r=finalize(c,skip);rows.append(dict(name=name,**r));return r
 assert case('empty',[])['output']==[]
 assert case('single-pruned-at-own-best',[(7,50000)])['output']==[]
 r=case('equal-best-rank10-keeps-46000',[(i,100)for i in range(11)]);assert r['threshold']==46000 and len(r['output'])==11
 r=case('distinct-rank10-is-exclusive',[(i,100+i)for i in range(11)]);assert r['threshold']==110 and len(r['output'])==10
 r=case('ten-does-not-read-rank10',[(i,100+i)for i in range(10)]);assert r['threshold']==46000 and len(r['output'])==10
 r=case('quote-scratch-consumes-top20',[(260,0)]+[(i,1)for i in range(25)],True);assert len(r['inspected'])==20 and len(r['output'])==19
 r=case('special259-never-ranked',[(259,-7),(7,100)]);assert 259 not in r['rank']and r['costs'][259]==-7
 r=case('special259-nonnegative-cleared',[(259,1),(7,100)]);assert r['costs'][259]==INF
 r=case('stored257-258-are-real-labels',[(257,1),(258,2)],True);assert r['output']==[[257,1],[258,2]]
 r=case('three-scratch-width',[ (i,100)for i in range(262)],True);assert len(r['rank'])==261 and 259 not in r['rank']
 # Native unstable equality order must be preserved independently of sorted cost values.
 assert sort_ids([4]*8,range(8))==[5,4,7,6,1,0,3,2]
 # Algebra-vs-literal negative: simplifying threshold to min(rank10,46000) drops all equal ties.
 assert rows[2]['threshold']!=100
 return dict(scope='PURE synthetic scalar finalization only; no classifier issuer or inference',cases=rows,caseCount=len(rows),sortTieWitness=sort_ids([4]*8,range(8)),modelForwards=0)
if __name__=='__main__':
 report=controls();pathlib.Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS 10 literal262 finalization cases; 0NN')
