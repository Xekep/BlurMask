"""Independent actual-command/original-registry/generated-journal replay.

Commands are additionally reconstructed from captured post-inference native
CHAR40/WORD48/ref24/TEXT32 metadata by the committed tree oracle. No labels
or journal rows are fed back into inference or the runtime source executor.
"""
import base64,hashlib,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
old=root/'tools/LegacyOriginalSelectedDocument.Probe/audit_output.py'
ns={'__file__':str(old)}
exec(compile(old.read_text(encoding='utf-8').split('total=0')[0],str(old),'exec'),ns)
a=json.loads(Path(sys.argv[1]).read_bytes());b64=base64.b64decode
def check(s):
    cmds=s['commands'];initial_next=len(s['initialIds'])
    source=s['source'];selections=[(c['tag'],'CHAR',c['ranges16'])for c in source['characters']if c['tag']!=-1]
    if source['word']['tag']!=-1:selections.append((source['word']['tag'],'WORD',source['word']['ranges16']))
    selections.append((source['text']['tag'],'TEXT',source['text']['ranges16']))
    expected_cmds=[]
    for i,(tag,name,ranges)in enumerate(selections,1):
        assert tag==initial_next+i-1
        expected_cmds.append(dict(operation=4,id=i,next=i+1,tag=tag,name=name,category=100,ranges16=ranges))
    expected_cmds.append(dict(operation=7,id=len(selections)+1,next=0,
        body56=base64.b64encode(struct.pack('<qq',initial_next+len(selections),initial_next)+bytes(40)).decode()))
    assert cmds==expected_cmds
    raw_tree=ns['ns']['expected'](s['source'])
    expected,names,_=ns['ids'](raw_tree)
    assert s['writer']==expected
    if s['applied']:
        tree=ns['ids'](ns['root4']([raw_tree]),names=dict(names))[0]
        assert s['target']==tree
    else:assert s['target']==ns['ids'](ns['root4']([]))[0]
    domain=struct.pack('<QQ',3,((len(s['counts'])-1)<<32)|((s['counts'][-1]-1)<<16)|0xff05)
    initial=[dict(id=i,name='INPUT',category=100,command=0,ranges16=base64.b64encode(domain).decode())for i in s['initialIds']]
    assert s['initialIds']==list(range(initial_next)) and b64(s['originalCounter'])==b64(s['selectedCounter'])==struct.pack('<qqB',initial_next,initial_next-1,0)
    tags=[struct.unpack('<q',b64(n['value']))[0]for n in s['originalNodes']if n['name']=='tag']
    assert tags==s['initialIds']
    rows=list(initial);journal=[];next_id=initial_next;maximum=initial_next-1;complete=True;completed=0
    trace=['library-source-frame:begin']
    for i,c in enumerate(cmds):
        if i==s['cancellation']:break
        if i==s['temporaryFailure']:trace.append('temporary-C22:refused');break
        if c['operation']==4:
            if i==s['observerFailure']:journal=[];complete=False;trace.append('observer90:refused:clear-frame')
            assert c['tag']==next_id and c['tag'] not in tags
            entry=dict(id=c['tag'],name=c['name'],category=c['category'],command=c['id'],ranges16=c['ranges16'])
            rows.append(entry);next_id=max(next_id,c['tag']+1);maximum=max(maximum,c['tag'])
            trace+=['register4:'+str(c['tag']),'CB7:event0:discard:'+str(c['tag'])]
            if i!=s['observerFailure']:journal.append(dict(entry,operation=4))
        else:
            new,old=struct.unpack_from('<qq',b64(c['body56']));assert old==initial_next and next_id>=new
            trace.append('counter7:skip')
        completed=i+1
    trace.append('library-source-frame:end');success=completed==len(cmds)
    expected=dict(entered=True,execution=success,faulted=not success,journalComplete=complete,rolledBack=False,
        recording=True,safe=success and complete,completed=completed,reversed=0,
        counter17=base64.b64encode(struct.pack('<qqB',next_id,maximum,0)).decode(),registry=rows,journal=journal,trace=trace)
    assert s['forward']==expected,(s['case'],'forward')
    final=dict(expected);undo=(s['mode']==-1 and s['chosen']==1)or s['mode']in(0,2,3)
    if undo:
        rows=list(rows);trace=list(trace)+['rollback:recording0'];reversed_count=0;dirty=0
        for row in reversed(journal):
            if s['mode']==3 and reversed_count==1:break
            rows=[e for e in rows if e['id']!=row['id']];dirty|=row['id']==maximum;reversed_count+=1
            trace+=['C4D:B0:skip-recording0','CB7:event2:'+str(row['id']),'delete4:'+str(row['id']),
                'CB7:no-original-tag-match:'+str(row['id']),'CB7:clear-events','C4D:B8:skip-recording0']
        done=s['mode']!=3
        if done:trace+=['rollback:recording1']
        final.update(registry=rows,trace=trace,reversed=reversed_count,recording=done,rolledBack=done,
            safe=False,faulted=not done,counter17=base64.b64encode(struct.pack('<qqB',next_id,maximum,dirty)).decode())
    assert s['final']==final,(s['case'],'final')
    if not complete:assert success and not s['final']['safe'] # ignored observer failure != VE0 refusal
for s in a['actual']:check(s)
for next_id,maximum,old,new,worked,result in a['counterControls']:
    assert worked==(old!=-1 and new!=-1)
    assert result==(old if worked and maximum<old else next_id)
assert len(a['actual'])==14 and len(a['counterControls'])==4096 and a['guards']>=150
def stable(x):
    if isinstance(x,list):return[stable(v)for v in x]
    if not isinstance(x,dict):return x
    out={k:stable(v)for k,v in x.items()}
    if 'uuid36'in out:out['uuid36']='LIBRARY-UUID'
    if out.get('name')=='gen':out['value']='LIBRARY-UUID'
    return out
digest=hashlib.sha256(json.dumps(stable(a),sort_keys=True,separators=(',',':')).encode()).hexdigest()
Path(str(sys.argv[1])+'.stable.sha256').write_text(digest+'\n',encoding='ascii')
print(f'PASS independent14 actual source applications/full original registries/generated frame order/rollback/fault prefixes +4096 signed counter controls; stableSHA={digest}')
