"""Strict stored/live-capture DATA audit; geometry scalar oracle only, never C#/NN execution."""
from pathlib import Path
import argparse,copy,importlib.util,json,hashlib
D=Path(__file__).resolve().parent
s=importlib.util.spec_from_file_location('input14_independent',D/'independent_oracle.py');m=importlib.util.module_from_spec(s);s.loader.exec_module(m)
def audit(repo,capture,selftest=False):
 e=m.expected(repo);c=m.load(capture);assert capture.stat().st_size<=262144
 result=m.check(c,e);controls=[]
 if selftest:
  def reject(label,change):
   bad=copy.deepcopy(c);change(bad)
   try:m.check(bad,e)
   except (AssertionError,KeyError,TypeError):controls.append(label)
   else:raise AssertionError('accepted negative control: '+label)
  for i in range(276):reject('short-'+str(i),lambda b,i=i:b['cases'][0]['data']['InitializedInput'].__setitem__(i,b['cases'][0]['data']['InitializedInput'][i]^1))
  for label,change in [
   ('short-bool',lambda b:b['cases'][0]['data']['InitializedInput'].__setitem__(0,True)),
   ('count-bool',lambda b:b.__setitem__('successfulProfiles',True)),
   ('meter-bool',lambda b:b['cases'][0]['data'].__setitem__('ChargedSourceWork',True)),
   ('count275',lambda b:b['cases'][0]['data'].__setitem__('InitializedInputCount',275)),
   ('fake-evaluator',lambda b:b.__setitem__('evaluatorSuccesses',1)),
   ('fake-AL-parity',lambda b:b.__setitem__('fullOldALHeaderProjectionVerified',True)),
   ('fake-multirange',lambda b:b.__setitem__('liveMultiRangeCoverage',True)),
   ('padding',lambda b:b['cases'][0]['data'].__setitem__('InputPadding',[0]*12)),
   ('wrong-order',lambda b:b['cases'].reverse()),
   ('wrong-refusal-message',lambda b:b['cases'][10].__setitem__('message','other refusal')),
   ('fake-refusal-data',lambda b:b['cases'][10].__setitem__('data',{})),
   ('eligible-refusal',lambda b:b['cases'].__setitem__(0,dict(name=b['cases'][0]['name'],outcome='earlier-refusal-no-input',exceptionType='System.NotSupportedException',message='Existing directional source-point domain exceeded.',data=None)))]:reject(label,change)
  def all_refusals(b):
   for r in b['cases']:r.update(outcome='earlier-refusal-no-input',exceptionType='System.NotSupportedException',message='Existing directional source-point domain exceeded.',data=None)
   b.update(completeInputBoundaries=0,earlierRefusals=14,exportedShortPayloadBytes=0)
  reject('all-refusals',all_refusals)
 return dict(scope='Strict DATA audit with independent scalar geometry expectations; not live issuer/native engine parity',captureSHA256=hashlib.sha256(capture.read_bytes()).hexdigest(),**result,negativeControlsRejected=len(controls),negativeControls=controls,oracleManagedExecutions=0,oracleNNCalls=0)
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('repo',type=Path);p.add_argument('output',type=Path);p.add_argument('--capture',type=Path,default=D/'historical-input14.json');p.add_argument('--selftest',action='store_true');a=p.parse_args()
 assert not a.output.exists();r=audit(a.repo.resolve(),a.capture.resolve(),a.selftest);a.output.write_text(json.dumps(r,indent=2)+'\n',encoding='utf-8');print('PASS strict12+2 input14 DATA; full14FeatureEquality=false, evaluatorSuccesses=0, oracle NN0')
