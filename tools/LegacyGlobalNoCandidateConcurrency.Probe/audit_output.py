"""Check observed same-owner lifecycle data; never invokes recognition or vendor code."""
from pathlib import Path
import argparse,base64,copy,hashlib,json
p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('report',type=Path);p.add_argument('--baseline',type=Path);a=p.parse_args()
def check(r):
 assert r['Scenario']=='same-old-owner-two-starts' and type(r['NNSourceInvocations'])is int and r['NNSourceInvocations']==1
 assert r['VerifiedNoEligible']is True and type(r['ObservedCompactRecords'])is int and 1<=r['ObservedCompactRecords']<=4096
 assert r['IsTopV68Completed']is True and r['IsFaulted']is False and r['SessionStopped']is False and r['NoAdditionalNN']is True
 assert r['Results']==[dict(Success=True,Type=None,Message=None),dict(Success=False,Type='System.InvalidOperationException',Message='Entered live V68 cannot restart.')]
 assert len(base64.b64decode(r['After120'],validate=True))==120
 source=base64.b64decode(r['Source26'],validate=True);assert 26<=len(source)<=4096*26 and len(source)%26==0
r=json.loads(a.capture.read_bytes());check(r);controls=[]
def bad_source(z):z['Source26']=base64.b64encode(base64.b64decode(z['Source26'])+b'x').decode('ascii')
def bad_word(z):z['After120']=base64.b64encode(b'x'*119).decode('ascii')
for name,change in [('two-winners',lambda z:z['Results'][1].__setitem__('Success',True)),('loser-exception',lambda z:z['Results'][1].__setitem__('Type','System.ArgumentException')),('faulted-winner',lambda z:z.__setitem__('IsFaulted',True)),('stopped-session',lambda z:z.__setitem__('SessionStopped',True)),('zero-compact-rows',lambda z:z.__setitem__('ObservedCompactRecords',0)),('word-truncated',bad_word),('source-stride',bad_source)]:
 z=copy.deepcopy(r);change(z)
 try:check(z)
 except AssertionError:controls.append(name)
 else:raise AssertionError('Corruption accepted: '+name)
baseline_hash=None
if a.baseline:
 b=json.loads(a.baseline.read_bytes());expected=b['Result']
 assert (r['Source26'],r['After120'],r['QuotaAfter'])==(expected['Source26'],expected['After120'],expected['QuotaAfter'])
 baseline_hash=hashlib.sha256(a.baseline.read_bytes()).hexdigest()
if a.report.exists():raise FileExistsError(a.report)
a.report.write_text(json.dumps(dict(captureSHA256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),oneWinnerOnePreEntryLoser=True,
 baselineSHA256=baseline_hash,fullOutputEqualsBaseline=bool(a.baseline),corruptions=controls,modelExecutions=0,
 scope='Observed one-pair actual lifecycle only; no stress coverage, eligible augmentation, native equivalence or OCR quality claim'),indent=2)+'\n',encoding='utf8')
print('PASS actual race capture / 7 corruption controls / data-only audit')
