"""Independent genuine global pending operation4/7 command oracle."""
from pathlib import Path
import json,runpy,struct,sys
root=Path(__file__).resolve().parents[2]
if len(sys.argv)not in(10,11):raise SystemExit('MODELS CHAR PARENT COVERAGE SELECTED INDEX CLEANUP TRANSFORMS COUNTER [NATIVE]')
models,char,parent,coverage,selected,index,cleanup,transforms,counter=sys.argv[1:10];saved=sys.argv[:]
try:
 sys.argv=['audit_output.py',models,char,parent,coverage,selected,index,cleanup,transforms]
 runpy.run_path(str(root/'tools/LegacyGlobalInputTransforms.Probe/audit_output.py'),run_name='__main__')
finally:sys.argv=saved
parents={s['name']:s for s in json.loads(Path(parent).read_bytes())['actual']}
prior={s['name']:s for s in json.loads(Path(transforms).read_bytes())['samples']}
d=json.loads(Path(counter).read_bytes());assert len(d['samples'])==14
def commands(p,append):
 out=[];length=len(p['commands'])+int(append)
 for n,c in enumerate(p['commands'],1):
  out.append(dict(op=4,id=n,previous=n-1,next=n+1 if n<length else 0,selection=dict(id=c['id'],name=c['name'],category=c['category'],ranges=c['ranges']),counter=None))
 if append:out.append(dict(op=7,id=length,previous=length-1,next=0,selection=None,counter=[p['nextBefore'],p['nextAfter']]))
 return out
total=0
for s in d['samples'][:-1]:
 p=parents[s['name']];t=prior[s['name']];assert s['completed']==t['completed']
 if not t['completed']:assert s==dict(name=s['name'],completed=False,refusal=t['refusal']);continue
 assert s['initialIds']==[0,1]and s['initial']==p['nextBefore']==2
 assert s['final']==p['nextAfter']and s['selectionCount']==len(p['commands'])
 assert s['commands']==commands(p,True)
 assert bytes.fromhex(s['body56'])==struct.pack('<qq',s['final'],s['initial'])+bytes(40)
 total+=len(s['commands'])
# Prior entered-allocation controls unchanged; one extra genuine owner runs6 guards:69+6=75.
assert(d['completed'],d['refused'],d['commands'],total,d['guards'])==(12,1,70,70,75)
failure=d['samples'][-1];p=parents['M']
assert failure==dict(name='prepared-allocation',failed=True,next=p['nextAfter'],commands=commands(p,False))
if len(sys.argv)==11:assert Path(counter).read_bytes()==Path(sys.argv[10]).read_bytes(),'Managed/Native mismatch'
print('PASS independent12 genuine global pending owners/58 original selection commands/12 trailing operation7/75 guards; no selected/original registry replay or DOM claim.')
