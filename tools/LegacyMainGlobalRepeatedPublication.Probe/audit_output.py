"""Independent repeated-gate/count/retained byte audit plus raw packed LK oracle."""
import base64,hashlib,json,subprocess,sys,tempfile
from pathlib import Path
if len(sys.argv)!=5:raise SystemExit('AK LK case-preparation-controls capture')
ak,lk,classes,capture=map(Path,sys.argv[1:]);data=json.loads(capture.read_bytes())
assert hashlib.sha256(ak.read_bytes()).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert hashlib.sha256(lk.read_bytes()).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
assert len(data['cases'])==4
converted=[];nodes=groups=cycles=0
for c,physical in zip(data['cases'],[1,61,91,121]):
 n=physical*4;expected_cycles=(n-1)//120
 assert c['physical']==physical and c['nodeCount']==n and c['cycles']==expected_cycles and c['nextNode']==120*expected_cycles
 assert c['groups']==(10 if physical==1 else 8*n-28)
 assert 0<c['requestedBytes']<=128*1024*1024 and c['primary']==125
 cursor=0;previous=0
 for start,end,before,after,checks,calls in c['gates']:
  assert (start,end,before)==(cursor,cursor+119,previous) and after>=before and checks>=0 and calls>=0
  cursor=end+1;previous=after
 assert cursor==c['nextNode'] and len(c['starts'])==n+1 and c['starts'][-1]==c['finalCount']
 assert all(a<=b for a,b in zip(c['starts'],c['starts'][1:]))
 assert len(base64.b64decode(c['compact']))==4096*40 and c['finalCount']<=4096
 if physical>1:assert not c['hasWinner'] and c['finalCount']==0 and all(g[3]==0 for g in c['gates'])
 else:assert c['hasWinner'] and c['finalCount']>0
 assert len(c['nodes'])==n
 for index,row in enumerate(c['nodes']):
  assert row['node']==index and 125<=row['retainedEnd']<=row['sourceEnd']
  assert len(base64.b64decode(row['records']))%88==0 and row['sourceEnd']<=len(base64.b64decode(row['records']))//88
  assert len(base64.b64decode(row['alternatives']))==125*80
 converted.append(dict(primary=125,nodes=c['nodes']));nodes+=n;groups+=c['groups'];cycles+=c['cycles']
assert (data['rawNodes'],data['rawCycles'],data['rawNn'],data['rawHasWinner'])==(244,2,1924,False)
assert data['guards']>=40 and data['comparedBytes']>50_000_000 and data['comparedContextPaths']>500_000
with tempfile.TemporaryDirectory(prefix='repeated-live-lk-',dir=capture.resolve().parent) as directory:
 out=Path(directory)/'converted.json';out.write_text(json.dumps(converted),encoding='utf-8')
 audit=Path(__file__).resolve().parents[1]/'LegacyCaseMainPass.Probe/audit_output.py'
 subprocess.run([sys.executable,str(audit),str(ak),str(lk),str(classes),str(out)],check=True)
print(f'PASS independent repeated LIVE {nodes}nodes/{groups}cachedNN/{cycles}gates; fixed pool/cumulative budgets and rawLK; no accuracy claim')
