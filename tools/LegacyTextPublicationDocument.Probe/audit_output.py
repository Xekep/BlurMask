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
        assert tag==i # Actual fresh selected INPUT registry.Next==1.
        out.append(dict(operation=4,id=i,next=i+1,tag=tag,name=name,category=100,ranges16=ranges))
    out.append(dict(operation=7,id=len(out)+1,next=0,
        body56=base64.b64encode(struct.pack('<qq',len(selections)+1,1)+bytes(40)).decode()))
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
assert len(data['actual'])==4 and len(data['faults'])==5 and data['guards']>=60
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
print(f'PASS independent4 full trees/{total} native allocation/name identities +5 prefix states/all pending commands/40B edits+cache; stableSHA={digest}; no SDK acceptance.')
