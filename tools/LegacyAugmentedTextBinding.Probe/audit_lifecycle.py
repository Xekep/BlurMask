import json,sys,base64,pathlib
x=json.loads(pathlib.Path(sys.argv[1]).read_text());e=x['Evidence'];names=['actual-positive-source','old-typed-positive-eligibility-refusal','early-incomplete-aug','pre-cancel-before-bind','early-guards-no-bind-or-poison','actual-aug-complete-positive','wrong-resource-before-bind','wrong-resource-extent-before-bind','foreign-scope-same-original-document','foreign-scope-no-run-refusal','foreign-scope-no-bind','argument-guards-before-adoption','same-live-wrapper-cache','all-current275-metadata-retained','selected-top-count-from-snapshot','text-pre-cancel-exact-token','text-bad-allocator-before-entry','text-pre-entry-guards-no-poison','actual-selected-text-complete','no-additional-acoustic-NN','text-repeat','repeat-loser-preserves-completed-source','disposed-wrapper','disposed-text','disposed-pending'];assert [v['Name'] for v in e]==names
for v in e:
 assert v.get('Pass') is True or v.get('Type')
 if v['Name'] in ['pre-cancel-before-bind','text-pre-cancel-exact-token']:assert v['Type']=='System.OperationCanceledException' and v['TokenMatches'] is True
for n in ['disposed-wrapper','disposed-text','disposed-pending']:assert next(v for v in e if v['Name']==n)['Type']=='System.ObjectDisposedException'
assert x['NNSourceInvocations']==1 and x['ActualBit8']>0 and x['Augmentation']['Candidates']>0
T=x['Text'];assert T['SelectedTopCount']>=1 and T['Actual275PayloadCount']==len(T['Actual275'])==len(x['BeforeText']['Words'])
assert T['Actual275']==[w['Storage120'] for w in x['BeforeText']['Words']]
assert all(len(base64.b64decode(w))==120 for w in T['Actual275'])
import runpy
audit_pool_growth=runpy.run_path(str(pathlib.Path(__file__).resolve().parent/'pool_growth.py'))['audit_pool_growth']
raw=json.loads(pathlib.Path(sys.argv[2]).read_text());prior=json.loads(pathlib.Path(sys.argv[3]).read_text())
growth=audit_pool_growth(x,raw,prior)
assert all(k in T for k in ['WordOriginalRanges','TextRanges','OriginalNullTextRanges'])
print(json.dumps(dict(Pass=True,ExactOrderedEvidence=25,TokenGuards=2,NNSourceInvocations=1,SelectedTopCount=T['SelectedTopCount'],Actual275PayloadCount=T['Actual275PayloadCount'],PoolGrowth=growth,Scope='structural lifecycle/preserved275/274prefix and exact compact-derived descriptor growth; appended numeric/UTF8 values require independent root oracle'),indent=2))
