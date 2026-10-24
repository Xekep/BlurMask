"""Pure count/fill chronology + native byte-shape oracle. No C#/model loading."""
import argparse,ast,base64,copy,hashlib,json,math,struct
from pathlib import Path
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def key(x):return [x['First'],x['Second'],x['Third']]
def audit(d):
 assert d['NN']==0 and len(d['Cases'])==12 and len(d['Duplicates'])==108 and len(d['Ranks'])==56
 count=merged=0
 for case in d['Cases']:
  kept=[];keys=[];ends=[]
  for s,raw in enumerate(case['Raw']):
   ids=[i for i,p in enumerate(raw)if i==0 or any(abs(a)+abs(b)<abs(f(a-b))*8388608 for a,b in zip(p,raw[i-1],strict=True))]
   kept.append(ids)
   for i,p in enumerate(ids):keys.append([0,s,p]);ends.append([0,s,ids[i+1]-1 if i+1<len(ids)else len(raw)-1])
  assert list(map(key,case['Keys']))==keys
  assert len(case['Map']) in (0,sum(case['Counts'])) and case['Factor']>0
  metadata=[];global_at=0
  for stroke,rows in enumerate(case['Metadata']):
   assert len(rows)==case['Counts'][stroke];out=[]
   for row in rows:
    ticks=case['Map'][global_at]if case['Map']else global_at;whole,rem=divmod(ticks,case['Factor'])if case['Map']else(ticks,0);at=whole+bool(rem)
    remainder=(case['Factor']-rem)&255 if rem else 0
    assert row==dict(Start=dict(zip(('First','Second','Third'),keys[at])),End=dict(zip(('First','Second','Third'),ends[at])),KeyRecordIndex=at,RemainderByte=remainder)
    out.append((keys[at],ends[at],at,remainder));global_at+=1
   metadata.append(out)
  def visit(node,ordinal,last):
   a=case['Nodes'][node][ordinal]
   if a['End']>last:return None
   if a['End']==last:return [a['Group']]
   for child in a['Next']:
    path=visit(node+1,child,last)
    if path is not None:return[a['Group']]+path
  for q in case['Queries']:
   path=None
   for ordinal,a in enumerate(case['Nodes'][q['First']>>7]):
    if a['Start']==q['First']:
     path=visit(q['First']>>7,ordinal,q['Last'])
     if path is not None:break
   assert path is not None
   rows=[]
   for group in path:
    r=case['Seeds'][group];s=r['StrokeIndex'];a=r['StartPoint'];b=r['EndPointExclusive'];first=metadata[s][a];last=metadata[s][b-1]
    rows.append(dict(first=first,last=last,tail=b==case['Counts'][s]))
   def order(r):return (*r['first'][0],-r['first'][3],*r['last'][1],-r['last'][3])
   for stop in range(len(rows)-1,0,-1):
    for j in range(stop):
     if order(rows[j])>order(rows[j+1]):rows[j],rows[j+1]=rows[j+1],rows[j]
   for i in range(len(rows)-1,0,-1):
    a,b=rows[i-1],rows[i];old=a['last'];cur=b['first'];equal=cur[0]==old[0]and cur[3]==old[3];adj=a['tail']and old[2]+1<len(keys)and cur[0]==keys[old[2]+1]
    if equal or adj:a.update(last=b['last'],tail=b['tail']);rows.pop(i);merged+=1
   expected=[dict(First=dict(zip(('First','Second','Third'),r['first'][0])),Last=dict(zip(('First','Second','Third'),r['last'][1]if r['tail']else r['last'][0])),FirstResidual=r['first'][3],LastResidual=r['last'][3])for r in rows]
   assert q['Count']==expected and q['Fill']==expected
   data=b''.join(struct.pack('<6i',*key(r['First']),*key(r['Last']))for r in expected)+bytes(v for r in expected for v in (r['FirstResidual'],r['LastResidual']))
   assert base64.b64decode(q['Storage'])==data and len(data)==26*len(expected)
   bounds=q['Bounds'];depth=(q['Last']>>7)-(q['First']>>7)+1;starts=len(case['Nodes'][q['First']>>7]);capacity=4
   while capacity<depth*2+2:capacity*=2
   one=4096+starts*(256+24*capacity)+depth*1024
   assert bounds==dict(Starts=starts,Depth=depth,SourcePoints=len(keys),Payload=2*one+26*depth,Copies=4*one+52*depth)
   count+=1
 for r in d['Duplicates']:assert r['Update']==(r['Index']>=r['Initial']and(r['Class']or r['NewCost']<r['OldCost']))
 assert d['Queries']==count and merged>0
 return dict(SourceFixtures=12,CountFill=count,ReverseMerges=merged,DuplicatePredicates=108,RankCases=56)
def run_rank(d,rank_path,score_path,out):
 # Reuse authenticated independent byte-addressed native oracles. Retain their
 # entire per-case algorithms, replacing only the old standalone driver input
 # and corpus-size assertions; the per-record assertions are unchanged.
 rank=rank_path.read_bytes();score=score_path.read_bytes()
 assert hashlib.sha256(rank).hexdigest()=='d9a752db151f6d6dd520f02b60d920123f24ff53f98be63a9f5c236ffd01e869'
 assert hashlib.sha256(score).hexdigest()=='53d9c8e4c51dfbcbcae27ac27c9e9d17f0781caedb1911694fa426ae37b35fd5'
 cases=[];scores=[]
 for c in d['Ranks']:
  r=c['Ranked'];cases.append(dict(words=c['Input'],selected=c['Selected'],quota=2,special=False,output=r['Words'],payloadSources=r['OutputSourceOrdinals'],rows=r['RankingByOriginalIndex'],map=r['OldToNew'],released=r['Released'],releasedSources=r['ReleasedSourceOrdinals'],quotaAfter=r['QuotaAfter']))
  scores.append(dict(input=r['Words'],output=c['Scored']['Words'],order=c['Scored']['ScoreOrder']))
 def execute(raw,values):
  tree=ast.parse(raw);body=[]
  for node in tree.body:
   if isinstance(node,ast.If)and isinstance(node.test,ast.Compare)and ast.unparse(node.test).startswith('len(sys.argv)'):continue
   if isinstance(node,ast.Assign)and any(isinstance(t,ast.Name)and t.id=='cases'for t in node.targets):continue
   if isinstance(node,ast.Assert)and 'len(cases)'in ast.unparse(node.test):continue
   if isinstance(node,ast.Expr)and isinstance(node.value,ast.Call)and isinstance(node.value.func,ast.Name)and node.value.func.id=='print':continue
   body.append(node)
  env={'cases':values};exec(compile(ast.Module(body=body,type_ignores=[]),'<authenticated-native-oracle>','exec'),env)
 execute(rank,cases);execute(score,scores)
def main():
 p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('rank_oracle',type=Path);p.add_argument('score_oracle',type=Path);p.add_argument('report',type=Path);a=p.parse_args();d=json.loads(a.capture.read_bytes());report=audit(d);run_rank(d,a.rank_oracle,a.score_oracle,a.report.parent);neg=[]
 edits=[('extra-case',lambda x:x['Cases'].append(copy.deepcopy(x['Cases'][0]))),('count-only-change',lambda x:x['Cases'][0]['Queries'][0]['Count'].clear()),('fill-only-change',lambda x:x['Cases'][0]['Queries'][0]['Fill'].clear()),('packed26-change',lambda x:x['Cases'][0]['Queries'][0].__setitem__('Storage','AA==')),('metadata-key',lambda x:x['Cases'][0]['Metadata'][0][0]['Start'].__setitem__('Third',999)),('duplicate-predicate',lambda x:x['Duplicates'][0].__setitem__('Update',not x['Duplicates'][0]['Update']))]
 for name,edit in edits:
  bad=copy.deepcopy(d);edit(bad)
  try:audit(bad)
  except(AssertionError,IndexError,KeyError):neg.append(name)
  else:raise AssertionError(('tamper accepted',name))
 for name,field,offset in [('ranked-payload','Ranked',0),('normalized-payload','Scored',0),('normalized-mass-outside4ULP','Scored',19)]:
  bad=copy.deepcopy(d);blob=bytearray(base64.b64decode(bad['Ranks'][0][field]['Words'][0]));blob[offset]^=1;bad['Ranks'][0][field]['Words'][0]=base64.b64encode(blob).decode()
  try:run_rank(bad,a.rank_oracle,a.score_oracle,a.report.parent)
  except(AssertionError,IndexError,KeyError):neg.append(name)
  else:raise AssertionError(('numeric tamper accepted',name))
 report.update(Status='PASS_PREPARED_ONLY',NN=0,LiveOwnerExecuted=False,TamperRefused=neg,CaptureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest());a.report.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
if __name__=='__main__':main()
