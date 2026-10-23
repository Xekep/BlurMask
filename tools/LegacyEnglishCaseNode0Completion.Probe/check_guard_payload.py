from pathlib import Path
import copy,hashlib,json,sys
HERE=Path(__file__).resolve().parent
EXPECTED_SHA256="9f8b6dc591ec1f8c0aad3159b77b2680da6a9f788dea076b9aa3aee9d06d99e0"

def expected():
    raw=(HERE/'guard-expectations.json').read_bytes()
    assert hashlib.sha256(raw).hexdigest()==EXPECTED_SHA256,'Pinned guard contract changed'
    return json.loads(raw)

def exact(actual,wanted,label):
    assert type(actual) is type(wanted) and actual==wanted,label

def check_guard_payload(g):
    e=expected()
    assert type(g) is dict,'Guards must be object'
    assert set(g)=={'Names','Count','Preentry','PreentryRetryPhase','PreentryRetryFaulted','Cancellations','CancellationCount','Faults','ExplicitCoverageExclusions'},'Exact guard fields required'
    assert type(g['Names']) is list and all(type(n) is str for n in g['Names']),'Names typed list'
    exact(g['Names'],e['Names'],'Exact ordered guard names')
    assert len(g['Names'])==len(set(g['Names']))==44,'No duplicate/empty guard list'
    exact(g['Count'],44,'Count44')
    assert type(g['Preentry']) is dict and set(g['Preentry'])==set(e['Preentry']),'Preentry exact keys'
    for k,v in e['Preentry'].items():exact(g['Preentry'][k],v,'Preentry '+k)
    exact(g['PreentryRetryPhase'],2,'Preentry retry phase')
    exact(g['PreentryRetryFaulted'],False,'Preentry retry fault')
    exact(g['CancellationCount'],17,'CancellationCount17')
    assert type(g['Cancellations']) is list and len(g['Cancellations'])==17,'Exact17 cancellations'
    for row,stage in zip(g['Cancellations'],e['CancellationStages']):
        assert type(row) is dict and set(row)=={'Stage'}|set(e['CancellationPayloadForEach']),'Exact cancellation witness keys'
        exact(row['Stage'],stage,'Ordered cancellation stage')
        for k,v in e['CancellationPayloadForEach'].items():exact(row[k],v,'Cancellation '+stage+' '+k)
    assert type(g['Faults']) is list and len(g['Faults'])==2,'Exact2 faults'
    for row,stage in zip(g['Faults'],e['FaultStages']):
        assert type(row) is dict and set(row)=={'Stage','Reached','Exception','Message','Faulted','Phase','PendingCleared'},'Exact fault witness keys'
        exact(row['Stage'],stage,'Ordered fault stage')
        for k in ['Reached','Exception','Faulted','Phase','PendingCleared']:exact(row[k],e['FaultPayloadForEach'][k],'Fault '+stage+' '+k)
        exact(row['Message'],e['FaultPayloadForEach']['MessagePrefix']+stage,'Exact fault message')
    exact(g['ExplicitCoverageExclusions'],e['ExplicitCoverageExclusions'],'Coverage exclusions cannot disappear')

def fixture():
    e=expected()
    return dict(Names=e['Names'],Count=44,Preentry=e['Preentry'],PreentryRetryPhase=2,PreentryRetryFaulted=False,
        Cancellations=[dict(Stage=s,**e['CancellationPayloadForEach']) for s in e['CancellationStages']],CancellationCount=17,
        Faults=[dict(Stage=s,**{k:v for k,v in e['FaultPayloadForEach'].items() if k!='MessagePrefix'},Message=e['FaultPayloadForEach']['MessagePrefix']+s) for s in e['FaultStages']],ExplicitCoverageExclusions=e['ExplicitCoverageExclusions'])

def self_test():
    good=fixture();check_guard_payload(good);cases=[]
    def reject(name,mutate):
        g=copy.deepcopy(good);mutate(g)
        try:check_guard_payload(g)
        except (AssertionError,KeyError,TypeError):cases.append(name);return
        raise AssertionError('Corruption accepted: '+name)
    reject('empty-names',lambda g:g.update(Names=[]))
    reject('duplicate-name',lambda g:g['Names'].__setitem__(1,g['Names'][0]))
    reject('ordered-name-swap',lambda g:g['Names'].__setitem__(slice(0,2),g['Names'][:2][::-1]))
    reject('wrong-count',lambda g:g.update(Count=43))
    reject('missing-cancellation',lambda g:g['Cancellations'].pop())
    reject('ordered-stage-swap',lambda g:g['Cancellations'].__setitem__(slice(0,2),g['Cancellations'][:2][::-1]))
    reject('unreached-cancellation',lambda g:g['Cancellations'][0].update(Reached=False))
    reject('generic-exception-masquerade',lambda g:g['Cancellations'][0].update(Exception='InvalidDataException'))
    reject('unpoisoned-phase',lambda g:g['Cancellations'][0].update(Phase=1))
    reject('wrong-cancellation-token',lambda g:g['Cancellations'][0].update(TokenMatches=False))
    reject('wrong-preentry-token',lambda g:g['Preentry'].update(TokenMatches=False))
    reject('wrong-observer-message',lambda g:g['Faults'][0].update(Message='Other failure'))
    reject('preentry-reset-mutated',lambda g:g['Preentry'].update(ResetAttempts=1))
    reject('numeric-bool-masquerade',lambda g:g['Cancellations'][0].update(Reached=1))
    reject('extra-complete-node-claim',lambda g:g.update(CompleteNode=True))
    reject('removed-coverage-exclusion',lambda g:g['ExplicitCoverageExclusions'].pop())
    return dict(status='PURE_STATIC_SYNTHETIC_GUARD_CONTRACT_CONTROLS_ONLY',positive=1,negative=len(cases),rejected=cases,ownerExecuted=False,nnExecuted=False)

if __name__=='__main__':
    if sys.argv[1:]==['--self-test']:print(json.dumps(self_test()))
    elif len(sys.argv)==3 and sys.argv[1]=='--guards':
        check_guard_payload(json.loads(Path(sys.argv[2]).read_bytes())['Guards'])
        print(json.dumps(dict(status='GUARD_PAYLOAD_CONTRACT_ONLY_NOT_OWNER_ORACLE',guardCount=44,cancellationCount=17)))
    else:raise SystemExit('check_guard_payload.py --self-test | --guards <capture-json>')
