import copy,hashlib,json,pathlib,sys
S,F,O=map(pathlib.Path,sys.argv[1:]);assert not O.exists()
success=json.loads(S.read_bytes());failure=json.loads(F.read_bytes())
def check(a):
    assert a['BeforeText']==success['BeforeText']
    assert a['AfterPrefix']==success['AfterText']
    trace=a['Trace'];assert trace[-1]=='fault' and 'text-registered' not in trace
    for i in range(2):assert trace.count('word-complete:'+str(i))==1
    assert trace.index('word-complete:1')<trace.index('word-registered')<trace.index('text-registration-prefix')<trace.index('fault')
    evidence={e['Name']:e for e in a['Evidence']}
    for name in ('whole-publication-poisoned','completed-augmentation-preserved','complete-word-prefix-not-complete-TEXT'):
        assert evidence[name]['Pass']is True
    assert evidence['entered-final-text-failure']['Type']=='System.NotSupportedException'
    assert evidence['faulted-no-retry']['Type']=='System.InvalidOperationException'
check(failure);rejected=[]
for name,mutate in [
    ('word',lambda q:q['AfterPrefix']['Words'][0].__setitem__('Storage120','')),
    ('completion',lambda q:q['Trace'].append('text-registered')),
    ('poison',lambda q:next(e for e in q['Evidence']if e['Name']=='whole-publication-poisoned').__setitem__('Pass',False)),
    ('augmentation',lambda q:next(e for e in q['Evidence']if e['Name']=='completed-augmentation-preserved').__setitem__('Pass',False)),
    ('retry',lambda q:next(e for e in q['Evidence']if e['Name']=='faulted-no-retry').__setitem__('Type','System.Exception'))]:
    bad=copy.deepcopy(failure);mutate(bad)
    try:check(bad)
    except AssertionError:rejected.append(name)
    else:raise AssertionError(name)
report=dict(status='PASS_DATA_ONLY',captureSHA256=hashlib.sha256(F.read_bytes()).hexdigest(),prefixMatchesSeparateSuccessfulRun=True,completedAugmentationPreserved=True,wholeTextPublicationPoisoned=True,corruptions=rejected,modelExecutions=0,scope='Separate fresh deliberate failure; completed WORD prefix is not completed TEXT')
O.write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report))
