"""Independent logical tree, clone identities, raw command/BE2/cache replay.

Input is actual post-inference CHAR40/WORD48/ref24/TEXT32/line56, not expected
recognition labels. The committed native tree/range primitives are reused;
this oracle independently owns the joined ordering and transfer/apply checks.
"""
import base64,hashlib,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
primitive=root/'tools/LegacyMainDetachedTextDom.Probe/audit_output.py'
ns={'__file__':str(primitive)}
exec(compile(primitive.read_text(encoding='utf-8').split("for s in a['actual']:")[0],str(primitive),'exec'),ns)
b64=base64.b64decode
data=json.loads(Path(sys.argv[1]).read_bytes())
steps=['prepared','source-lines','text-registered','line-consistency','coverage','source-indices',
    'control-cleanup','input-transforms','counter-handoff','source-ranges','dom-built','exported','transported']
def ids(tree,start=1,names=None):
    names={} if names is None else names
    ordinal=start
    def visit(n):
        nonlocal ordinal
        result=dict(n,id=ordinal,nameId=names.setdefault(n['name'],len(names)+1) if n['name'] else 0)
        ordinal+=1
        result['attributes']=[visit(x) for x in n['attributes']]
        result['children']=[visit(x) for x in n['children']]
        return result
    return visit(tree),names,ordinal
def root4(children):return dict(kind=4,name='',tag=0,value='',attributes=[],children=children)
transport=struct.pack('<qq',-1,-1)+bytes(24)
edit=struct.pack('<I',2)+bytes(36)
def commands(s):
    selections=[(c['tag'],'CHAR',c['ranges16']) for c in s['characters'] if c['tag']!=-1]
    if s['word']['tag']!=-1:selections.append((s['word']['tag'],'WORD',s['word']['ranges16']))
    selections.append((s['text']['tag'],'TEXT',s['text']['ranges16']))
    out=[]
    for i,(tag,name,ranges) in enumerate(selections,1):
        assert tag==i+1 # Actual two retained INPUT entries; initial Next==2.
        out.append(dict(operation=4,id=i,next=i+1,tag=tag,name=name,category=100,ranges16=ranges))
    out.append(dict(operation=7,id=len(out)+1,next=0,
        body56=base64.b64encode(struct.pack('<qq',len(selections)+2,2)+bytes(40)).decode()))
    return out
def check_snapshot(s,snapshot,stage,applied=False,transferred=True):
    assert snapshot['steps']==steps+(['library-target-applied'] if applied else [])
    assert snapshot['transferred']==transferred and snapshot['applied']==applied and not snapshot['faulted']
    expected=ns['expected'](s)
    tree,names,_=ids(expected)
    assert snapshot['writer']==tree
    if applied:
        # A distinct target arena already owns constructor kind4/id1. V50
        # and apply must not publish the writer's live node identities.
        target,_,_=ids(root4([expected]),names=dict(names))
    else:target,_,_=ids(root4([]))
    assert snapshot['target']==target
    assert list(map(b64,snapshot['edit40']))==[edit]
    assert b64(snapshot['transport40'])==transport
    assert snapshot['commands']==commands(s)
    return len(list(flatten(tree)))
def flatten(n):
    yield n
    for c in n['attributes']+n['children']:yield from flatten(c)
total=0
for s in data['actual']:
    total+=check_snapshot(s,s['before'],'transported')
    check_snapshot(s,s['after'],'library-target-applied',True)
for f in data['faults']:
    mode=f['mode'];p=f['prefix']
    if mode<2:
        assert p['faulted'] and not p['transferred'] and not p['applied'] and p['edit40']==[]
        assert p['commands']==commands(f)
        assert p['target']==ids(root4([]))[0]
        if mode==0:
            assert p['writer'] is None and p['steps']==steps[:10]+['dom-refused']
        else:
            assert p['writer']==ids(ns['expected'](f))[0] and p['steps']==steps[:11]+['export-refused']
    elif mode==2:
        assert p['steps']==steps[:-1] and not p['faulted'] and not p['transferred']
        assert p['transport40'] is None and p['commands']==commands(f)
        check_snapshot(f,f['recovered'],'transported')
    elif mode==3:
        assert p['faulted'] and p['transferred'] and not p['applied'] and p['steps']==steps
        assert p['target']==ids(root4([]))[0] and p['commands']==commands(f)
    else:
        assert not p['faulted'] and p['transferred'] and not p['applied'] and p['steps']==steps
        assert p['target']==ids(root4([]))[0]
assert len(data['actual'])==6 and len(data['faults'])==5 and data['guards']>=150

def registry(sample,commands,fail=-1,cancel=-1,not_entered=False):
    state=sample['registry'];ids={0,1};next_id=2;maximum=1;rows=[];completed=0
    for index,c in enumerate(commands):
        if not_entered or index==fail or index==cancel:break
        if c['operation']==4:
            tag=c['tag'];assert tag not in ids and tag>=0 and c['category']==100
            ids.add(tag);next_id=max(next_id,tag+1);maximum=max(maximum,tag)
            rows.append(dict(command=c['id'],id=tag,name=c['name'],category=c['category'],ranges16=c['ranges16']))
        else:
            new,old=struct.unpack_from('<qq',b64(c['body56']));assert old==2
            if next_id<new:next_id=max(new,maximum+1)
        completed=index+1
    assert state['initialIds']==[0,1] and state['selections']==rows
    assert b64(state['counter16'])==struct.pack('<qq',next_id,maximum) and state['completed']==completed
    assert state['success']==(not not_entered and fail<0 and cancel<0)
    assert state['faulted']==(fail>=0 or cancel>=0)
for s in data['actual']:
    assert s['sourceTag']==s['chosen'] and s['initialNext']==2 and s['initialMax']==1 and s['allInitialIds']==[0,1]
    counts=s['counts'];domain=struct.pack('<QQ',3,((len(counts)-1)<<32)|((counts[-1]-1)<<16)|0xff05)
    assert b64(s['inputRanges'])==domain and b64(s['originalDomain'])==domain
    assert b64(s['originalPresence'])==bytes([1])*len(counts) and b64(s['originalOutput'])==b'\1\1'
    assert b64(s['seed20'])==struct.pack('<q',s['chosen'])+bytes(12)
    assert s['transforms']==s['daf']==0 and s['matches']==list(range(len(counts)))
    assert b64(s['matrices48'])==struct.pack('<6d',1,0,0,0,1,0)*len(counts)
    registry(s,s['after']['commands'])
for f in data['replayFaults']:
    mode=f['mode'];registry(f,f['commands'],f['fail'],f['cancel'],mode>=3)
    if mode>=3:
        assert b64(f['compatibilityOutput'])==(b'\0\1'if mode<5 else b'\1\0')
        assert f['worked']==(mode==5) and not f['ready']
assert len(data['replayFaults'])==6
f=data['transformFault']
assert b64(f['exported64'])==bytes(i^0xa7 for i in range(64))
assert f['count']==1 and f['daf']==0 and f['tag']==1 and not f['success'] and not f['following']

# UUID is deliberately environmental. Normalize gen and its source UUID36
# only for Managed/Native capture parity; each unnormalized tree was checked.
def stable(x):
    if isinstance(x,list):return [stable(v) for v in x]
    if not isinstance(x,dict):return x
    result={k:stable(v) for k,v in x.items()}
    if 'uuid36' in result:result['uuid36']='LIBRARY-UUID'
    if result.get('name')=='gen':result['value']='LIBRARY-UUID'
    return result
digest=hashlib.sha256(json.dumps(stable(data),sort_keys=True,separators=(',',':')).encode()).hexdigest()
Path(str(sys.argv[1])+'.stable.sha256').write_text(digest+'\n',encoding='ascii')
print(f'PASS independent6 original-selected full trees/{total} native allocation/name identities +5 prefix states/all pending commands/40B edits+cache; stableSHA={digest}; no SDK acceptance.')
