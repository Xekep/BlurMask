"""Portable generic TOP scalar controls; no actual EN/lifecycle admission."""
import argparse,copy,hashlib,json,re
from pathlib import Path
PROGRAM_LF='c253ecf34db493473d26beea126c04345265bb83c54ce72baefe0f646232273f'
REVIEWED_PRODUCTION_LF={'LegacyNativePlainWordPublisher.TopScalars.cs':'42e496869af55e6818d7dba3218fd0b2755cb84927613dea905b1152d2770a45',
    'LegacyEnglishCaseGraphTopOwner.cs':'f09f8e8548ca9711cac8ac4607ff0c7c6df1d08665e1829178e87c12335069ce'}
def program(data):assert hashlib.sha256(data.replace(b'\r\n',b'\n')).hexdigest()==PROGRAM_LF
def reviewed(name,data):assert hashlib.sha256(data.replace(b'\r\n',b'\n')).hexdigest()==REVIEWED_PRODUCTION_LF[name]
def unique(pairs):
    result={}
    for key,value in pairs:
        assert key not in result;result[key]=value
    return result
def check(value):
    expected={'PreparedOnly','NoModelOrNN','Runtime','Positives','Refusals','AlternativeCount','ScalarLookups','MeasuredAllocatedBytes','AllocationObservationOnly','Scope'}
    assert type(value) is dict and set(value)==expected
    assert value['PreparedOnly'] is True and value['NoModelOrNN'] is True
    assert type(value['Runtime']) is str and re.fullmatch(r'10\.0\.[0-9]+',value['Runtime'])
    assert value['Scope']=='generic publisher scalar accessor; actual EN TOP owner unexecuted'
    for key,want in [('Positives',4),('Refusals',10),('AlternativeCount',4095),('ScalarLookups',10000)]:
        assert type(value[key]) is int and value[key]==want
    assert value['AllocationObservationOnly'] is True
    assert type(value['MeasuredAllocatedBytes']) is int and value['MeasuredAllocatedBytes']>=0
def main():
    p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('report',type=Path);p.add_argument('--tamper',action='store_true');a=p.parse_args()
    source=Path(__file__).with_name('Program.cs').read_bytes();program(source)
    root=Path(__file__).resolve().parents[2]
    production={name:(root/'src/InkRecognition'/name).read_bytes() for name in REVIEWED_PRODUCTION_LF}
    for name,data in production.items():reviewed(name,data)
    raw=a.capture.read_bytes();assert len(raw)<=16384
    value=json.loads(raw,object_pairs_hook=unique);check(value);negatives=0
    if a.tamper:
        bad=[]
        for key in ('Positives','Refusals','AlternativeCount','ScalarLookups'):
            v=copy.deepcopy(value);v[key]+=1;bad.append(v)
        for key,replacement in [('PreparedOnly',1),('NoModelOrNN',False),('Refusals',10.0),('AlternativeCount',True),('Runtime','11.0.0'),('Scope','actual owner accepted'),('MeasuredAllocatedBytes',-1),('AllocationObservationOnly',False)]:
            v=copy.deepcopy(value);v[key]=replacement;bad.append(v)
        v=copy.deepcopy(value);v['Accepted']=True;bad.append(v)
        v=copy.deepcopy(value);del v['Refusals'];bad.append(v)
        for v in bad:
            try:check(v)
            except (AssertionError,KeyError,TypeError):negatives+=1
            else:raise AssertionError('Invalid prepared report accepted')
        for mutated in (source+b'\n',source.replace(b'4095',b'4094',1)):
            try:program(mutated)
            except AssertionError:negatives+=1
            else:raise AssertionError('Altered prepared source accepted')
        for name,data in production.items():
            try:reviewed(name,data+b'\n// unreviewed alteration\n')
            except AssertionError:negatives+=1
            else:raise AssertionError('Unreviewed allocation source accepted')
        try:json.loads('{"PreparedOnly":true,"PreparedOnly":false}',object_pairs_hook=unique)
        except AssertionError:negatives+=1
        else:raise AssertionError('Duplicate report field accepted')
    with a.report.open('x',encoding='utf-8') as out:json.dump(dict(PreparedOnly=True,ActualEnglishOwnerExecuted=False,GenericChecks=14,TamperRefusals=negatives,ObservedRuntime=value['Runtime'],ScalarAllocatedBytes=value['MeasuredAllocatedBytes'],AllocationObservationOnly=True,ZeroAllocationGuarantee=False,ProgramLF=PROGRAM_LF,ReviewedProductionLF=REVIEWED_PRODUCTION_LF),out,indent=2)
    print(f'PASS generic TOP scalar14 checks/{negatives} tamper refusals; allocation informational; no actual owner/NN')
if __name__=='__main__':main()
