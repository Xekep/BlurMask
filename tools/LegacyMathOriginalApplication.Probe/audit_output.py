"""Independent raw NN/C21/AL -> fourth outgoing edit -> original source/history
and complete original FIELD PAGE. No vendor code or expected recognition labels.
"""
from pathlib import Path
import copy,sys
source=Path(__file__).resolve().parents[1]/'LegacyMathOutgoingTransfer.Probe/audit_output.py'
code=source.read_text(encoding='utf-8-sig')
exec(compile(code.split("assert len(data['Actual'])==6")[0],str(source),'exec'),globals())

def application(evidence,before_failure=-1,after_failure=-1,inverse=False):
    _,transfer=outgoing(evidence);old=evidence['Field'];payload=transfer['Writer'];historical=[];active=[];scratch=[];registry=[];journal=[];trace=[];events=[]
    original,=[n for n in old['OriginalNodes']if n['Kind']==3 and n['Name']=='mathField']
    original_id=original['Id']
    by_parent={}
    for n in payload:by_parent.setdefault(n['Parent'],[]).append(n)
    count=len(old['OriginalNodes'])*3+len(payload)*2
    value_bytes=sum(len((n[k]or'').encode())for n in payload for k in('Name','Text','NamespaceUri','NamespacePrefix'))
    ranges=sum(len(c['Ranges'])for c in transfer['Commands']if c['Operation']==4)
    reservation=transfer['Reservation']+16384+count*1024+value_bytes*8+ranges*1024
    state=dict(Prepared=False,Failed=False,Released=False,Phase='claimed',Reservation=reservation,SourceEntered=False,
        SourceExecutionSucceeded=False,JournalComplete=True,DomEntered=False,Complete=False,RolledBack=False,
        InitialNext=old['Next'],ActiveNext=old['Next'],LibraryEpoch=0,CleanupCount=0,ScratchField=0,OriginalField=0,
        HistoricalPage=historical,Scratch=scratch,ActivePage=active,ActiveRegistry=registry,Journal=journal,Trace=trace,CanonicalOriginalNames=True)
    def emit(flow,name,index=-1):state['Phase']=name;events.append((flow,name,index,copy.deepcopy(state)))
    initial=dict(Id=old['Tag'],Name='INPUT',Category=102,Ranges=copy.deepcopy(old['OriginalRanges']),CanonicalC22Id=1)
    registry.append(initial);emit('Prepare','Claimed')
    for n in old['OriginalNodes']:
        historical.append(dict(Id=n['Id'],Kind=n['Kind'],Name=n['Name'],Parent=n['Parent'],OriginalId=n['Id'],PayloadOrigin=0,
            ValueTag=n['ValueTag'],Text=n['StringValue'],Number=n['NumericValue'],NamespaceUri=n['NamespaceUri'],NamespacePrefix=n['NamespacePrefix']))
    active.extend(copy.deepcopy(historical));state['OriginalField']=original_id;emit('Prepare','HistoricalPage')
    def clone_original(n,parent):
        identity=len(scratch)+1;scratch.append(dict(n,Id=identity,Parent=parent));emit('Prepare','ScratchNode',identity)
        for attr in(True,False):
            for child in historical:
                if child['Parent']==n['Id']and(child['Kind']==0)==attr:clone_original(child,identity)
        return identity
    scratch_root=clone_original(next(n for n in historical if n['Id']==original_id),0);state['ScratchField']=scratch_root
    def clone_payload(arena,node,parent,flow,stage):
        identity=len(arena)+1
        arena.append(dict(Id=identity,Kind=node['Kind'],Name=node['Name'],Parent=parent,OriginalId=0,PayloadOrigin=node['Id'],ValueTag=node['ValueTag'],Text=node['Text'],Number=node['Number'],NamespaceUri=node['NamespaceUri'],NamespacePrefix=node['NamespacePrefix']))
        emit(flow,stage,identity)
        for attr in(True,False):
            for child in by_parent.get(node['Id'],[]):
                if(child['Kind']==0)==attr:clone_payload(arena,child,identity,flow,stage)
        return identity
    root=next(n for n in payload if n['Id']==transfer['PayloadRoot'])
    inserted=clone_payload(scratch,root,0,'Prepare','ScratchPayloadNode');scratch[inserted-1]['Parent']=scratch_root
    state['Prepared']=True;trace.append('whole-kind3:scratch:compatible');emit('Prepare','Preflight');prepared=copy.deepcopy(state)
    state['SourceEntered']=True
    command_index=0
    for c in transfer['Commands']:
        if c['Operation']==7:
            assert state['ActiveNext']>=c['NewCounter18'];trace.append('counter7:skip');emit('Source','Counter');continue
        emit('Source','TemporarySelection',command_index)
        has_history=command_index!=before_failure
        if not has_history:journal.clear();state['JournalComplete']=False;trace.append('observer90:refused:clear-frame')
        emit('Source','HistoryPrepared',command_index)
        canonical=next((r['CanonicalC22Id']for r in registry if r['Ranges']==c['Ranges']),1+max(r['CanonicalC22Id']for r in registry))
        registry.append(dict(Id=c['Id'],Name=c['Name'],Category=c['Category'],Ranges=copy.deepcopy(c['Ranges']),CanonicalC22Id=canonical));state['ActiveNext']=max(state['ActiveNext'],c['Id']+1)
        trace.extend(('original:register4:'+str(c['Id']),'CB7:event0:discard:'+str(c['Id'])));emit('Source','Registered',command_index)
        if command_index==after_failure:journal.clear();state['JournalComplete']=False;trace.append('observer98:refused:clear-frame')
        elif has_history:journal.append(dict(Id=c['Id'],Name=c['Name'],Category=c['Category'],Ranges=copy.deepcopy(c['Ranges']),DistinctHistoryClone=True,Reversed=False))
        emit('Source','HistoryCommitted',command_index);command_index+=1
    state['SourceExecutionSucceeded']=True;trace.append('original:source:execution-succeeded');emit('Source','SourceComplete');source_state=copy.deepcopy(state)
    if before_failure>=0 or after_failure>=0:return events,prepared,source_state,copy.deepcopy(state)
    if inverse:
        for i in reversed(range(len(journal))):
            j=journal[i];registry[:]=[r for r in registry if r['Id']!=j['Id']];j['Reversed']=True
            # Canonical identity numbering is reconstructed from remaining entries.
            canonical={};
            for r in registry:
                key=repr(r['Ranges'])
                if key not in canonical:canonical[key]=len(canonical)+1
                r['CanonicalC22Id']=canonical[key]
            trace.extend(('original:delete4:'+str(j['Id']),'CB7:no-original-tag-match:'+str(j['Id'])));emit('Inverse','SourceDeleted',i)
        state['RolledBack']=True;trace.append('original:inverse:Next-monotonic');emit('Inverse','RolledBack');return events,prepared,source_state,copy.deepcopy(state)
    state['DomEntered']=True;emit('Dom','DomEntered')
    inserted=clone_payload(active,root,0,'Dom','LivePayloadNode');active[inserted-1]['Parent']=original_id
    trace.append('original:DOM:insert2:after-INPUT');emit('Dom','DomInserted')
    lookup={r['Id']:r for r in registry}
    def validate(id):
        for n in active:
            if n['Parent']!=id:continue
            if n['Kind']==0 and n['Name']=='tag':
                assert n['ValueTag']==4 and n['Number']in lookup
                for r in lookup[n['Number']]['Ranges']:
                    assert r['First']['Value']>=old['OriginalRanges'][0]['First']['Value']and r['Last']['Value']<=old['OriginalRanges'][0]['Last']['Value']
            if n['Kind']==3:validate(n['Id'])
    validate(original_id);emit('Dom','PostValidated');state['LibraryEpoch']=1;state['Complete']=True;trace.append('library:DOM:epoch1');emit('Dom','Complete')
    return events,prepared,source_state,copy.deepcopy(state)

assert len(data['Actual'])==6
for row in data['Actual']:
    _,transferred=outgoing(row['Evidence']);assert row['Transfer']==transferred
    events,prepared,source_state,final=application(row['Evidence'])
    assert row['Prepared']==prepared and row['Source']==source_state and row['Final']==final
events,prepared,source_state,final=application(data['Actual'][0]['Evidence'])
assert data['Events']==[dict(Flow=f,Name=n,Index=i)for f,n,i,_ in events]
assert len(data['Failures'])==len(events)*2==138
for(f,n,i,prefix),pair in zip(events,[data['Failures'][i:i+2]for i in range(0,len(data['Failures']),2)],strict=True):
    for canceled,failure in zip((False,True),pair,strict=True):
        assert(failure['Flow'],failure['Stage'],failure['Index'],failure['Canceled'])==(f,n,i,canceled)
        assert failure['Before']==prefix,(f,n,i,'before')
        after=dict(prefix,Failed=True)
        if f=='Source':after['JournalComplete']=False
        assert failure['After']==after,(f,n,i,'after')
for row in data['UnsafeSources']:
    _,_,_,state=application(data['Actual'][0]['Evidence'],after_failure=row['Index']if row['AfterObserver']else-1,before_failure=-1 if row['AfterObserver']else row['Index'])
    assert row['State']==state
inverse_events,_,_,inverse=application(data['Actual'][0]['Evidence'],inverse=True);assert data['Inverse']==inverse
inverse_events=[e for e in inverse_events if e[0]=='Inverse'];assert data['InverseEvents']==[dict(Flow=f,Name=n,Index=i)for f,n,i,_ in inverse_events]
for(f,n,i,prefix),pair in zip(inverse_events,[data['InverseFailures'][i:i+2]for i in range(0,len(data['InverseFailures']),2)],strict=True):
    for canceled,failure in zip((False,True),pair,strict=True):
        assert(failure['Flow'],failure['Stage'],failure['Index'],failure['Canceled'])==(f,n,i,canceled)
        assert failure['Before']==prefix and failure['After']==dict(prefix,Failed=True)
postcheck=next(s for f,n,i,s in events if n=='DomInserted');assert data['Postcheck']==dict(postcheck,Failed=True)
assert data['Gc']==final
assert(data['ConcurrentCompleted'],data['ConcurrentRefused'],data['Guards'])==(1,1,379)
if len(sys.argv)>=4:assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[3]).read_bytes()
print(f'PASS6 independent actual NN->source/C22/journal/original typed PAGE;138+8 prefixes/379guards/6unsafe observers/monotonic inverse/GC;feature error{max_error};no CBB acceptance.')
