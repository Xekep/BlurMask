from pathlib import Path
import json,base64,struct,sys,hashlib

def scalar(n):
 t=n['ValueTag'];s=bytearray(16);heap=b''
 if t in(1,2):
  raw=n['Text'].encode()
  if t==1:struct.pack_into('<I',s,8,len(raw));heap=raw
  else:s[:len(raw)]=raw;s[15]=0 if len(raw)==15 else len(raw)
 elif t==4:struct.pack_into('<q',s,0,n['Number'])
 elif t==7:struct.pack_into('<i',s,0,n['Number'])
 else:assert t in(0,3,10)
 return(t,bytes(s),heap)
def equal(a,b):
 if a['ValueTag']!=b['ValueTag']:return False
 t=a['ValueTag'];return True if t in(3,10)else scalar(a)==scalar(b)
def cost(a,b):
 if a['Kind']!=b['Kind']:return 10
 k=a['Kind']
 if k==3:return 0 if a['Name']==b['Name']else 2
 if k==0 and a['Name']!=b['Name']:return 2
 if k in(0,1):return 0 if equal(a,b)else 1
 return 10
def align(old,new):
 p=s=0
 while p<min(len(old),len(new))and cost(old[p],new[p])==0:p+=1
 while len(old)-p-s>1 and len(new)-p-s>1 and cost(old[-1-s],new[-1-s])==0:s+=1
 m,n=len(old)-p-s,len(new)-p-s;d=[[0]*(n+1)for _ in range(m+1)]
 for j in range(n+1):d[0][j]=j
 for i in range(1,m+1):
  d[i][0]=i
  for j in range(1,n+1):
   best=min(d[i-1][j]+1,d[i][j-1]+1);diagonal=d[i-1][j-1]
   if diagonal<best:best=min(best,diagonal+10*cost(old[p+i-1],new[p+j-1]))
   d[i][j]=best
 r=[];a,b=m,n
 while a or b:
  up=d[a-1][b]if a else float('inf');left=d[a][b-1]if b else float('inf');diag=d[a-1][b-1]if a and b else float('inf')
  if a<1 or b<1 or d[a][b]-diag>1:diag=float('inf')
  minimum=min(left,up,diag)
  if minimum==diag:r.append([p+a-1,p+b-1]);a-=1;b-=1
  elif minimum==left:r.append([-1,p+b-1]);b-=1
  else:r.append([p+a-1,-1]);a-=1
 return [[i,i]for i in range(p)]+r[::-1]+[[len(old)-i,len(new)-i]for i in range(s,0,-1)]
def expected(case):
 c=case['Candidate'];old,new=c['Temporary'],c['Candidate'];oldby={n['Id']:n for n in old};out=[];nextid=len(old)+1
 children=lambda xs,n,k:[a for a in xs if a['Parent']==n['Id']and(a['Kind']==0)==k]
 def clone(n):
  nonlocal nextid
  value=nextid;nextid+=1
  for attr in children(new,n,True):clone(attr)
  for child in children(new,n,False):clone(child)
  return value
 def action(op,target,payload=0,ordinal=-1,source=0,ov=None,nv=None):
  a=dict(Operation=op,NodeAt18=target['Id'],NodeAt20=payload,Ordinal=ordinal,SourceCandidate=source,EnvelopeKind=target['Kind'],Name=target['Name'],OldTag=0,NewTag=0,OldStorage='',NewStorage='',OldHeap='',NewHeap='',Filtered=False)
  if ov is not None:
   for prefix,n in [('Old',ov),('New',nv)]:
    tag,storage,heap=scalar(n);a[prefix+'Tag']=tag;a[prefix+'Storage']=base64.b64encode(storage).decode();a[prefix+'Heap']=base64.b64encode(heap).decode()
  out.append(a)
 def walk(a,b):
  assert a['Kind']==b['Kind']
  if a['Kind']==1:
   if not equal(a,b):action(7,a,source=b['Id'],ov=a,nv=b)
   return
  assert a['Kind']==3 and a['Name']==b['Name']
  aa,ba=children(old,a,True),children(new,b,True)
  for attr in aa:
   other=next((x for x in ba if x['Name']==attr['Name']),None)
   if other is None:action(6,a,payload=attr['Id'])
   elif not equal(attr,other):action(7,attr,source=other['Id'],ov=attr,nv=other)
  for attr in ba:
   if not any(x['Name']==attr['Name']for x in aa):action(5,a,payload=clone(attr),source=attr['Id'])
  ac,bc=children(old,a,False),children(new,b,False);cursor=-1
  for x,y in align(ac,bc):
   if x<0:cursor=max(0,cursor+1);action(2,a,payload=clone(bc[y]),ordinal=cursor,source=bc[y]['Id'])
   elif y<0:action(4,a,payload=ac[x]['Id'],ordinal=max(0,cursor+1))
   else:walk(ac[x],bc[y]);cursor=max(0,cursor+1)
 walk(next(n for n in old if n['Kind']==3 and n['Name']=='mathResult'),next(n for n in new if n['Kind']==3 and n['Name']=='mathResult'))
 reg={r['Id']:r for r in c['Registry']};pending={-r['TemporaryId']:r for r in c['Pending']}
 for a in out:
  if a['Operation']==7 and a['EnvelopeKind']==0 and a['Name']=='tag':
   before=oldby[a['NodeAt18']];after=next(n for n in new if n['Id']==a['SourceCandidate'])
   assert before['ValueTag']==after['ValueTag']==4
   ro,rn=reg.get(before['Number']),pending.get(after['Number'])
   a['Filtered']=bool(ro and rn and ro['Name']==rn['Name'] and (ro['Ranges']==rn['Ranges']if rn['HasSelection']else not ro['Ranges']))
 return out
if __name__=='__main__':
 B=Path(__file__).resolve().parent
 fixtures=json.loads((B/'alignment-fixtures.json').read_bytes())
 for f in fixtures:
  node=lambda ch:dict(Kind=3,Name=ch)
  assert align(list(map(node,f['Old'])),list(map(node,f['New'])))==f['Pairs']
 if len(sys.argv)==1:print('PASS961 frozen independent default alignments; no capture input');raise SystemExit
 cap=json.loads(Path(sys.argv[1]).read_bytes());summary=[]
 for case in cap['Actual']:
  want=expected(case);got=case['Diff'];assert got['Actions']==want,(case['Case'],got['Actions'],want)
  kept=sum(not x['Filtered']for x in want);assert got['Produced']==len(want) and got['Kept']==kept and got['Filtered']==len(want)-kept
  assert got['Complete']and got['Ready']and got['HeaderLive']and not got['Failed']and not got['Released']
  summary.append(dict(case=case['Case'],produced=len(want),kept=kept,filtered=len(want)-kept))
 report=dict(PASS=True,alignments=len(fixtures),cases=summary,evidenceSha256=hashlib.sha256(Path(sys.argv[1]).read_bytes()).hexdigest(),scope='independent default scalar/schema/alignment/action fields/payload clone IDs + actual registry/pending CB8 filter; no apply')
 Path(sys.argv[2]).write_text(json.dumps(report,indent=2)+'\n');print(report)
