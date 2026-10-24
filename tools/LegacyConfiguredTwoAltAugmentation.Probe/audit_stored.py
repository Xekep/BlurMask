"""Strict stored original RAW DATA plus92 negatives; no C#/model/inference."""
from pathlib import Path
import importlib.util,json,copy,sys,hashlib
T=Path(__file__).resolve().parent
sp=importlib.util.spec_from_file_location('raw_auditor',T/'audit_raw.py');a=importlib.util.module_from_spec(sp);sp.loader.exec_module(a)
mutations=[('bool-schema',lambda d:d.__setitem__('schema',True)),('extra-field',lambda d:d.__setitem__('forged',1)),('unmarked',lambda d:d.__setitem__('marked',False)),('incomplete',lambda d:d.__setitem__('complete',False)),('faulted',lambda d:d.__setitem__('faulted',True)),('diagnostic-failure',lambda d:d.__setitem__('diagnosticFailure',True)),('debit-bool',lambda d:d['sourceDebit'].__setitem__('copies',True)),('debit-float',lambda d:d['sourceDebit'].__setitem__('work',float(d['sourceDebit']['work']))),('phase',lambda d:d['sourceDebit'].__setitem__('phase',1)),('group',lambda d:d['sourceDebit'].__setitem__('activeGroup',0)),('missing-site',lambda d:d['sourceDebit']['sites'].pop()),('site-order',lambda d:d['sourceDebit']['sites'].reverse()),('copy-sum',lambda d:d['sourceDebit'].__setitem__('copies',d['sourceDebit']['copies']+1)),('work-sum',lambda d:d['sourceDebit'].__setitem__('work',d['sourceDebit']['work']+1)),('neural-ratio',lambda d:(d['sourceDebit'].__setitem__('copies',d['sourceDebit']['copies']+14824),d['sourceDebit']['sites'][10].__setitem__('copies',d['sourceDebit']['sites'][10]['copies']+14824))),('allocation',lambda d:d.__setitem__('allocatedLogical',d['allocatedLogical']+1)),('visits',lambda d:d.__setitem__('memoryVisits',d['memoryVisits']+1)),('copy-ledger',lambda d:d.__setitem__('copyBytes',d['copyBytes']+1)),('clear-ledger',lambda d:d.__setitem__('pendingClearBytes',d['pendingClearBytes']+1)),('missing-event',lambda d:d['events'].pop()),('extra-event',lambda d:d['events'].append(copy.deepcopy(d['events'][-1]))),('event-bool',lambda d:d['events'][0].__setitem__('argument',False)),('invalid-base64',lambda d:d['events'][0].__setitem__('raw','AA==\n'))]
assert len(sys.argv)==2,'fresh-report.json'
out=Path(sys.argv[1]);assert not out.exists()
provenance=a.r.strict_json(T/'provenance.json');assert provenance['schema']==1
rows=[]
for i in range(4):
    path=T/'fixtures'/('raw-'+str(i)+'.json')
    digest=hashlib.sha256(path.read_bytes()).hexdigest()
    assert digest==provenance['fixtureSHA256'][path.name]
    base=a.r.strict_json(path);report=a.audit(base);refusals=[]
    for name,mut in mutations:
        data=copy.deepcopy(base);mut(data)
        try:a.audit(data)
        except (AssertionError,ValueError,IndexError) as e:refusals.append(dict(name=name,type=type(e).__name__));continue
        raise AssertionError(('accepted mutation',i,name))
    assert len(refusals)==23
    rows.append(dict(sample=i,fixtureSHA256=digest,audit=report,refusals=refusals))
assert sum(row['audit']['comparisons'] for row in rows)==16
assert sum(row['audit']['orderedGlyphReads'] for row in rows)==60
out.write_text(json.dumps(dict(scope='Strict stored original4 RAW +92 mutations only; not full callback/cost/source/NN proof',samples=rows,totalRefusals=92,targetExecutions=0,wholePipelineNNCountInstrumented=False),indent=2)+'\n',encoding='utf-8')
print('PASS stored4/16comparisons/60orderedreads/92negatives;0target0NN')
