"""Strict B7 lifecycle payload check; no recognition, model load or labels.
Use on the two scoped guard fixtures, or the standard actual corpus capture.
Numerical corpus records are checked separately by audit_output.py.
"""
import argparse, collections, copy, hashlib, json
from pathlib import Path

STAGES=['entry','child:entry','child:cloned','range:counted','range:filled',
        'glyph:interned','glyph:score','glyph:complete','child:materialized','child:published']

def audit(data):
    assert data['TargetsRead'] is False and data['Refused']==0
    assert data['Accepted']==len(data['Actual'])
    assert data['LifecycleBasicRuns']==data['LifecycleMultiRuns']==1
    eligible=next(x for x in data['Actual'] if x['Variants'][0]['Children'] and x['Variants'][0]['Children'][0]['Words'])
    assert any(len(x['Variants'][0]['Children'])>=2 for x in data['Actual'])
    expected=['null-owner','prepare-cancel','preentry-cancel','double-run',
              'one-short-peak','one-short-work','one-short-visits','oversized-budget',
              'allocator39','incomplete-B6','B6-entered-cancel','faulted-B6',
              'PREPARED-header-182','PREPARED-header-183','PREPARED-header-184',
              'PREPARED-symbol-FFFF','foreign-B6-child']
    for stage in STAGES:expected+=['entered-cancel:'+stage,'retry-after-cancel:'+stage]
    expected+=['injected-fault','retry-after-fault','barrier-second-run']
    proof_labels=expected.copy()
    for child in eligible['Variants'][0]['Children']:
        expected+=['owner-snapshot-detached']*len(child['Owners'])
        expected+=['range-snapshot-detached']*len(child['Ranges'])
        expected+=['word-snapshot-detached']*len(child['Words'])
    expected+=['B6-byte-identity-and-NN-count-unchanged','multichild-prefix-cancel','retry-prefix-fault']
    proof_labels+=['multichild-prefix-cancel','retry-prefix-fault']
    assert data['LifecycleGuards']==expected,'ordered guard/snapshot coverage'
    assert [x[0] for x in data['LifecycleProofs']]==proof_labels,'exact caught exception coverage'
    reached=['B6-entered-cancel:entry']+['entered-cancel:'+s for s in STAGES]
    reached+=['injected-fault:child:materialized','multichild-prefix-cancel:child:entry:2']
    assert data['LifecycleReached']==reached,'actual entered-stage coverage'
    cancellations={'prepare-cancel','preentry-cancel','B6-entered-cancel','multichild-prefix-cancel'}|{'entered-cancel:'+s for s in STAGES}
    invalid={'double-run','retry-after-fault','barrier-second-run','retry-prefix-fault'}|{'retry-after-cancel:'+s for s in STAGES}
    unsupported={'PREPARED-header-182','PREPARED-header-183','PREPARED-header-184','PREPARED-symbol-FFFF'}
    for label,typ,message in data['LifecycleProofs']:
        want='System.OperationCanceledException' if label in cancellations else (
             'System.InvalidOperationException' if label in invalid else (
             'System.NotSupportedException' if label in unsupported else (
             'System.ArgumentNullException' if label=='null-owner' else (
             'System.IO.InvalidDataException' if label=='injected-fault' else 'System.ArgumentException'))))
        assert typ==want,(label,typ,want)
        assert message==('Injected' if label=='injected-fault' else ''),'only invariant injected message is canonical'
    assert all(x['NetworkBefore']==x['NetworkAfter'] for x in data['Actual'])
    return dict(Guards=len(expected),ExceptionProofs=len(proof_labels),ReachedStages=len(reached),
                CancelTypes=len(cancellations),SnapshotChecks=sum('snapshot' in s for s in expected))

def negatives(data):
    edits=[('wrong-cancel-type',lambda d:d['LifecycleProofs'][1].__setitem__(1,'System.ArgumentException')),
           ('wrong-injected-message',lambda d:next(x for x in d['LifecycleProofs'] if x[0]=='injected-fault').__setitem__(2,'Other')),
           ('missing-stage',lambda d:d['LifecycleReached'].pop()),
           ('missing-snapshot',lambda d:d['LifecycleGuards'].remove('owner-snapshot-detached')),
           ('missing-proof',lambda d:d['LifecycleProofs'].pop()),
           ('localized-message',lambda d:d['LifecycleProofs'][1].__setitem__(2,'localized')),
           ('NN-replay',lambda d:d['Actual'][0].__setitem__('NetworkAfter',-1))]
    refused=[]
    for label,edit in edits:
        bad=copy.deepcopy(data);edit(bad)
        try:audit(bad)
        except (AssertionError,KeyError,IndexError):refused.append(label)
        else:raise AssertionError(('lifecycle tamper accepted',label))
    return refused

def main():
    p=argparse.ArgumentParser();p.add_argument('--capture',type=Path,required=True);p.add_argument('--baseline',type=Path);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
    data=json.loads(a.capture.read_bytes());counts=audit(data)
    if a.baseline:
        old={x['Name']:x for x in json.loads(a.baseline.read_bytes())['Actual']}
        assert all(row==old[row['Name']] for row in data['Actual']),'test-only build changed actual numerical/source payload'
    result=dict(Status='PASS_EXACT_LIFECYCLE_PROOFS',**counts,ActualCases=len(data['Actual']),
                TamperRefused=negatives(data),CaptureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),
                Scope='Exact exception types/stages and snapshot checks, no native ABI or accuracy claim')
    a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))

if __name__=='__main__':main()
