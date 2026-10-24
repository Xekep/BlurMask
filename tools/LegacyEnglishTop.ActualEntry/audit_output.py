"""DATA-only first-TOP checker; does not validate whole-capture admission."""
import argparse,base64,hashlib,json,struct
from pathlib import Path

def read(path,sha):
    data=Path(path).read_bytes()
    assert len(data)<=32*1024*1024 and hashlib.sha256(data).hexdigest()==sha
    return json.loads(data)
def integer(value):
    assert type(value) is int
    return value
def f(value):return struct.unpack('<f',struct.pack('<f',value))[0]
def image(value):
    assert type(value) is str
    data=base64.b64decode(value,validate=True)
    assert len(data)==400
    return data
def same(actual,expected):
    assert type(actual) is type(expected)
    if type(expected) is dict:
        assert actual.keys()==expected.keys()
        for key in expected:same(actual[key],expected[key])
    elif type(expected) is list:
        assert len(actual)==len(expected)
        for a,b in zip(actual,expected,strict=True):same(a,b)
    else:assert actual==expected
def check(actual,source,compact):
    assert actual['Scope']=='ACTUAL_OWNING_ENGLISH_FIRST_TOP_PRECOMPUTED_SCORE_SOURCE_CALLBACK_NO_TEXT_ADMISSION'
    assert actual['Completed'] is True and actual['Admission']=='NONE'
    assert actual['NoExpectedLabels'] is True and actual['GlobalReadinessGateUnchanged'] is True
    same(actual['Metrics'],source['Metrics'])
    same(actual['SourceCompletion'],source['SourceCompletion'])
    same(actual['Vector10'],source['Vector10'])
    for key in ('InputSha','OriginalIndex','RawPoints','RawStrokes','Resolution','GraphNodes','ParentChildren'):
        same(actual[key],source[key])
    for key in ('OriginalIndex','RawPoints','RawStrokes','GraphNodes','ParentChildren'):integer(actual[key])
    top=actual['Top']; assert set(top)==set(('Constructor400','Scored400','Normalized400','TerminalOrdinal','TotalCost','RowCost','NodeCount','SelectedWordOrdinal','WordChoices','RawMass','Top80'))
    winner=compact['Compact']['Winner']; assert winner['HasWinner'] is True
    for key,expected in [('TerminalOrdinal',winner['Ordinal']),('TotalCost',winner['TotalCost']),('RowCost',winner['RowCost14']),('NodeCount',compact['GraphNodes'])]:
        assert integer(top[key])==integer(expected)
    words=source['SourceCompletion']['Source']
    assert words['SelectedOrdinals']==[integer(top['SelectedWordOrdinal'])]
    assert integer(top['WordChoices'])==len(words['Words'])
    constructor=bytearray(400); struct.pack_into('<I',constructor,20,0x7f7fffff)
    assert image(top['Constructor400'])==constructor
    scored=constructor.copy()
    #Fresh first reference equals genuine terminal total: signed SUB=0;
    #therefore Exp(+0)=1 exactly, independent of a libm approximation.
    struct.pack_into('<f',scored,8,1.0);struct.pack_into('<f',scored,16,0.0)
    struct.pack_into('<f',scored,20,f(f(integer(top['RowCost']))*f(0.00012207031)))
    assert image(top['Scored400'])==scored
    normalized=scored.copy();struct.pack_into('<f',normalized,12,1.0)
    assert image(top['Normalized400'])==normalized
    assert type(top['RawMass']) in (int,float) and top['RawMass']==1
    assert type(top['Top80']) in (int,float) and top['Top80']==-1
    return {'ImageBytes':1200,'WholeCaptureAcceptance':False,'HandwritingConfidence':False,
            'Scope':'pinned first-TOP scalar/portable allocation plus identical prior DATA only; live identities are source-reviewed, not inferable from JSON'}
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('actual');p.add_argument('actual_sha');p.add_argument('prior_source');p.add_argument('prior_compact');p.add_argument('report');a=p.parse_args()
    result=check(read(a.actual,a.actual_sha),read(a.prior_source,'04a55b4c2f1c435cbab421ad10c75f1476fbbfe6dc4d82858aac0b5c946d7846'),read(a.prior_compact,'bc5a6fd66742ed9e2395b0c7872a58168e67d8b1615c80146563dd46da3abdfc'))
    with Path(a.report).open('x',encoding='utf-8') as output:json.dump(result,output,indent=2)
