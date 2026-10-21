"""STATIC source inventory + independently enumerated PROPOSED lease model.
No runtime implementation, vendor execution, native pointer or decoder claim.
"""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path
p=argparse.ArgumentParser()
for n in ('repo','models','sdk','acoustic-capture','output'):p.add_argument('--'+n,type=Path,required=True)
a=p.parse_args();here=Path(__file__).resolve().parent
pin=json.loads((here/'english-owned-word-request-source-inventory.json').read_bytes())
sha=lambda b:hashlib.sha256(b).hexdigest()
for r in pin['sourcePins']:
    b=(a.repo/r['path']).read_bytes();assert len(b)==r['bytes'] and sha(b)==r['sha256'],r['path']
read=lambda name:(a.repo/'src/InkRecognition'/name).read_text(encoding='utf-8-sig')
b=read('LegacyEnglishFreshDictionaryQuerySession.cs')
snapshot=b[b.index('public LegacyEnglishQueryPreparationSnapshot CopyPreparationSnapshot'):]
assert 'lock(gate)' in snapshot and 'contexts.Capture(token)' in snapshot
assert 'queryBusy' not in snapshot # Existing seam is a PROPOSED guard, not fixed.
factory=read('LegacySymbolContextFactory.cs')
assert 'Instances require serialized caller access' in factory
assert 'nodes.Values.OrderBy' in factory and 'nodes.Clear()' in factory
binding=read('LegacyEnglishMainD0Binding.cs')
assert 'faulted||!active||d0Busy' in binding
assert 'operations++;return lease' in binding
assert 'operations!=0||attempts!=0||outputs!=0' in binding
assert 'querySource.SetWorkingMap(next);working=next;d0Version=lease.Version;d0Busy=false' in binding
raw=read('LegacyPreparedDictionaryQuerySource.cs')
assert 'if (query.ParentContext == 0) context = initialContexts[lexical]' in raw
assert 'record.Clear()' in raw and 'record[5] = flags' in raw
aggregate=read('LegacyPreparedAggregateDictionaryQuerySource.cs')
assert 'records[i * 32 + 4] = (byte)(ordinal + 1)' in aggregate
assert 'finally { query.Skip = globalSkip' in aggregate

# Explicit abstract proposal transitions, never represented as C# runtime tests.
# State=(fault,busy,current_version,operations), IDs and budgets kept separate.
def attempt(state,action,limit=3):
    fault,busy,version,ops=state
    if action in ('beginQuery','beginD0'):
        if fault or busy or ops>=limit:return state,False
        return (False,action,version,ops+1),True
    if action=='snapshot':return state,busy!='beginQuery'
    if action=='commitD0':
        if fault or busy!='beginD0':return state,False
        return (False,False,version+1,ops),True
    if action=='releaseQuery':
        if fault or busy!='beginQuery':return state,False
        return (False,False,version,ops),True
    if action=='fault':
        if not busy:return state,False
        return (True,False,version,ops),True
    raise AssertionError(action)
initial=(False,False,1,0);states={initial};edges=[];front=[initial]
actions=('beginQuery','beginD0','snapshot','commitD0','releaseQuery','fault')
while front:
    state=front.pop()
    for action in actions:
        result,accepted=attempt(state,action);edges.append((state,action,result,accepted))
        assert result[3]>=state[3] and result[2]>=state[2]
        if state[1] and action in ('beginQuery','beginD0'):assert not accepted
        if state[1]=='beginQuery' and action=='snapshot':assert not accepted
        if action=='releaseQuery' and accepted:assert result[2]==state[2]
        if action=='commitD0' and accepted:assert result[2]==state[2]+1
        if result not in states:states.add(result);front.append(result)
assert any(s[0] for s in states) and any(s[3]==3 for s in states)
# 80B initial literal writes independently specified by scalar store extents.
mask=[False]*80
for offset,width in [(0,4),(4,4),(8,4),(12,2),(14,2),(16,2),(18,1),(19,1),
 (20,4),(24,4),(28,1),(32,4),(36,2),(38,2),(40,8),(48,4),(52,4),
 (56,1),(57,1),(58,1),(64,4),(68,4),(72,1),(73,1)]:
    for i in range(offset,offset+width):assert not mask[i];mask[i]=True
unwritten=[i for i,x in enumerate(mask) if not x]
assert sum(mask)==66 and unwritten==list(range(29,32))+list(range(59,64))+list(range(74,80))
# Snapshot race witness: no queryBusy guard in current method; proposed guard
# rejects the snapshot before factory enumeration once query enters off-lock.
assert attempt((False,'beginQuery',1,1),'snapshot')[1] is False
up=a.output.with_name('ewr-'+sha(str(a.output.resolve()).encode())[:12]+'-upstream.json')
up.parent.mkdir(parents=True,exist_ok=True)
subprocess.run([sys.executable,'-I','-B',str(a.repo/'tools/handwriting/audit_english_word_lifecycle.py'),
 '--repo',str(a.repo),'--models',str(a.models),'--sdk',str(a.sdk),
 '--acoustic-capture',str(a.acoustic_capture),'--output',str(up)],check=True)
report=dict(status='PASS STATIC source inventory/native dependencies/PROPOSED lease model',
 sourceFiles=len(pin['sourcePins']),oldSeamsProposed=pin['proposedOldSeams'],
 proposalModelStates=len(states),proposalModelTransitions=len(edges),
 initialRequestWrittenBytes=sum(mask),initialRequestUnwrittenBytes=unwritten,
 initialPageContextResolveCalls=0,startupGraftMaximumAttemptsPerRow=2,
 nativeClosure=json.loads(up.read_bytes()),runtimeChanged=False,
 productionConcurrencyTested=False,requestOwnerImplemented=False,wordDecoderReady=False,
 targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS STATIC query/request proposal; no production runtime or decoder claim')
