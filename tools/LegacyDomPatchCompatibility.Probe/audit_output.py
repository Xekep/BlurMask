"""Independent BE2 clone/path-map/simulation oracle over actual DOM/edit data.

The all-path map is built against the copied initial tree, retaining objects
even if an earlier simulated row later detaches them. This is not sequential
path lookup against the edited tree. No expected transcription enters here.
"""
import base64, hashlib, json, sys
from pathlib import Path

data = json.loads(Path(sys.argv[1]).read_bytes())

def simulate(target, rows, fault):
    stage, fail_initial, fail_payload, cancel = fault
    initial = mapped = simulated = 0
    next_id = 1
    tree = None
    def clone(n):
        nonlocal next_id
        out = {k:v for k,v in n.items() if k not in ('attributes','children')}
        out['id'] = next_id; next_id += 1
        out['attributes'] = [clone(a) for a in n['attributes']]
        out['children'] = [clone(c) for c in n['children']]
        return out
    def done(worked, compatible, written=True, unchecked=False, canceled=False):
        return dict(worked=worked, compatible=compatible, written=written,
            initial=initial, mapped=mapped, simulated=simulated,
            uncheckedClone=unchecked, canceled=canceled, tree=tree)
    if not rows:return done(True,1)
    if stage == 1:return done(False,1)
    tree = dict(id=next_id,kind=4,name='',nameId=0,tag=0,value='',attributes=[],children=[])
    next_id += 1
    for n in target['children']:
        if initial == fail_initial:return done(False,1)
        tree['children'].append(clone(n)); initial += 1
    if stage == 2:return done(False,1)
    parents=[]
    # Native E2AE0..E2B6D completes this entire pass before E2B91.
    for row in rows:
        p=tree
        for step in row['path']:
            if step >= len(p['children']):return done(True,0)
            p=p['children'][step]
        parents.append(p); mapped += 1
    for i,(row,parent) in enumerate(zip(rows,parents)):
        if i == cancel:return done(False,0,False,canceled=True)
        index=row['ordinal']; children=parent['children']
        exists=0<=index<len(children)
        if row['operation']==4:
            if not exists:return done(True,0)
            children.pop(index)
        elif row['operation']==2:
            if i == fail_payload:return done(False,0,False,True)
            payload=clone(row['payload'])
            if exists:children.insert(index,payload)
            elif index==len(children):children.append(payload)
            else:return done(True,0)
        else:raise AssertionError('outside actual library2/4 scope')
        simulated += 1
    return done(True,1)

actual=prepared=0
for case in data['actual']:
    mode=case['mode']; state=case['state']
    assert case['targetBefore']['children']==[]
    assert len(case['rows'])==1 and case['rows'][0]['operation']==2
    assert case['rows'][0]['ordinal']==0 and case['rows'][0]['path']==[]
    if mode>=8:
        assert not state['wasRun'] and not state['written'] and not state['faulted']
        assert state['tree'] is None and case['targetBefore']==case['targetAfter']
    else:
        fault={4:[1,-1,-1,-1],5:[2,-1,-1,-1],6:[0,-1,0,-1],7:[0,-1,-1,0]}.get(mode,[0,-1,-1,-1])
        expected=simulate(case['targetBefore'],case['rows'],fault)
        assert state['wasRun']
        assert {k:state[k] for k in expected if k!='canceled'}=={k:v for k,v in expected.items() if k!='canceled'},mode
        ready=expected['worked'] and expected['written'] and expected['compatible']==1
        assert state['ready']==ready and state['faulted']!=ready
        if mode<4:
            assert case['targetAfter']==expected['tree'];actual+=1
        else:assert case['targetBefore']==case['targetAfter']
    if 'actualLibraryReplacement' in case:
        r=case['actualLibraryReplacement']
        assert [x['operation'] for x in r['rows']]==[4,2]
        assert len(r['target']['children'])==1
        expected=simulate(r['target'],r['rows'],[0,-1,-1,-1])
        assert r['result']==expected and r['after']==expected['tree']
    for control in case.get('prepared',[]):
        expected=simulate(control['target'],control['rows'],control['fault'])
        assert control['result']==expected,control['index']
        prepared+=1
assert actual==4 and prepared==2048 and data['guards']>=100

uuid_values=set()
def collect_uuid(x):
    if isinstance(x,list):
        for v in x:collect_uuid(v)
    elif isinstance(x,dict):
        if x.get('name')=='gen':uuid_values.add(x['value'])
        for v in x.values():collect_uuid(v)
collect_uuid(data)
def stable(x):
    if isinstance(x,list):return [stable(v) for v in x]
    if isinstance(x,str) and x.startswith('value:') and x.rsplit(':',1)[-1] in uuid_values:
        return x.rsplit(':',1)[0]+':LIBRARY-UUID'
    if not isinstance(x,dict):return x
    result={k:stable(v) for k,v in x.items()}
    if result.get('name')=='gen':result['value']='LIBRARY-UUID'
    return result
digest=hashlib.sha256(json.dumps(stable(data),sort_keys=True,separators=(',',':')).encode()).hexdigest()
Path(str(sys.argv[1])+'.stable.sha256').write_text(digest+'\n',encoding='ascii')
print(f'PASS independent {actual} actual scratch/apply trees +2048 full ordered scratch controls/all-map-before-edit/failure prefixes; stableSHA={digest}.')
