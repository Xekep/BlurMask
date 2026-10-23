"""Independent prepared generic-kernel output audit, never actual owner evidence."""
import argparse,base64,copy,json,struct,sys
from pathlib import Path
sys.dont_write_bytecode=True;sys.path.insert(0,str(Path(__file__).resolve().parent))
from raw_profile import read_profile
from reference_rows import produce
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--controls',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--report',type=Path,required=True);p.add_argument('--tamper',action='store_true');a=p.parse_args()
controls=json.loads(a.controls.read_bytes());out=json.loads(a.capture.read_bytes());profile=read_profile(a.models/'en-US/en_US-ak-cur.res')
def audit(out):
 assert set(out)=={'PreparedOnly','NNEvaluated','Cases'} and out['PreparedOnly'] is True and out['NNEvaluated'] is False
 assert controls['PreparedOnly'] is True and controls['ModelSHA256']==profile['ResourceSHA256']
 assert len(out['Cases'])==len(controls['Cases']) and len(set(c['Name'] for c in out['Cases']))==len(out['Cases'])
 produced=refused=powvectors=0
 for spec,case in zip(controls['Cases'],out['Cases'],strict=True):
  assert spec['Name']==case['Name']
  if 'ExceptionType' in spec:
   assert set(case)=={'Name','Status','ExceptionType'} and case['Status']=='Refused' and case['ExceptionType']==spec['ExceptionType'];refused+=1;continue
  assert set(case)=={'Name','Status','Storage','LabelCount','Projected'} and case['Status']=='Produced'
  expected,ranges,count,_=produce(profile,spec['Bits'],spec['Mapping'],spec['Ranges'],spec['Records'],spec['Selected'])
  assert base64.b64decode(spec['Expected'],validate=True)==expected and spec['Projected']==ranges and spec['LabelCount']==count
  assert base64.b64decode(case['Storage'],validate=True)==expected and case['Projected']==ranges and case['LabelCount']==count
  produced+=1;powvectors+=spec['Name'].startswith('pow75-k255-')
 assert powvectors==256 and refused==4
 return dict(Produced=produced,Refused=refused,DoublePow75Vectors=powvectors)
metrics=audit(out);negative=[]
if a.tamper:
 def reject(label,change):
  mutated=copy.deepcopy(out);change(mutated)
  try:audit(mutated)
  except(AssertionError,ValueError,KeyError,IndexError,TypeError):negative.append(label);return
  raise AssertionError('accepted '+label)
 reject('extra-case',lambda d:d['Cases'].append(copy.deepcopy(d['Cases'][0])))
 reject('missing-case',lambda d:d['Cases'].pop())
 reject('720-pointer-byte',lambda d:d['Cases'][0].__setitem__('Storage',base64.b64encode(bytes([1])+base64.b64decode(d['Cases'][0]['Storage'])[1:]).decode()))
 reject('labelcount',lambda d:d['Cases'][0].__setitem__('LabelCount',999))
 reject('wrong-exception',lambda d:d['Cases'][-1].__setitem__('ExceptionType','InvalidDataException'))
 reject('wrong-point-range',lambda d:d['Cases'][0]['Projected'][0].__setitem__(2,999))
report=dict(Status='PASS_INDEPENDENT_PREPARED_SOURCE720',Metrics=metrics,TamperRefusals=negative,PreparedOnly=True,NNEvaluated=False,TargetsRead=False,SDKExecuted=False)
a.report.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
