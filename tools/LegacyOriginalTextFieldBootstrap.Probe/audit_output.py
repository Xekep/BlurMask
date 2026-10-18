"""Independent native-call-plan oracle. No vendor binaries or inference labels."""
import argparse,base64,copy,json,struct
from pathlib import Path

def encoded(raw):return base64.b64encode(raw).decode()

def native_plan(ids,counts,stop=-1,cancel=False):
    state=dict(succeeded=False,faulted=False,attempts=0,next=0,maximum=-(1<<63),root=0,
               nodes=[],inputs=[],history=[],sourceEvents=[],observer=[],trace=[])
    depth=pending=retained=pending_field=retained_field=0
    class Stop(Exception):pass
    def service(name):
        ordinal=state['attempts'];state['attempts']+=1;state['trace'].append('try:'+name)
        if ordinal==stop:
            if not cancel:state['trace'].append('refused:'+name)
            raise Stop()
    def allocate(kind,name):
        service(f'node:{kind}:{name}')
        row=dict(id=len(state['nodes'])+1,kind=kind,name=name,tag=3 if kind==0 else 0,
                 value='',uri=None,parent=0,children=[],attributes=[])
        state['nodes'].append(row);return row
    def attached(node):
        while node:
            if node['id']==state['root']:return True
            node=state['nodes'][node['parent']-1] if node['parent'] else None
        return False
    def event(point):state['observer'].append([point,depth,pending,retained,pending_field,retained_field])
    def journal(source,kind,parent,node,previous=0,registry=-1,category=0):
        raw=struct.pack('<QQ',((255<<8)if counts[0]==1 else 0)|3,
                        ((len(counts)-1)<<32)|((counts[-1]-1)<<16)|(255<<8)|5)if source=='C4D'else b''
        state['history'].append(dict(source=source,kind=kind,parent=parent,node=node,previous=previous,next=0,registry=registry,category=category,copiedRanges=encoded(raw)))
    def queue(field_id):
        nonlocal pending,pending_field
        pending=1;pending_field=field_id;event('input-change')
    def child(parent,node):
        observed=attached(parent)
        if observed:service('BE2-record:1')
        previous=parent['children'][-1] if parent['children'] else 0
        node['parent']=parent['id'];parent['children'].append(node['id'])
        state['trace'].append(f"append:{parent['id']}:{node['id']}")
        if observed:journal('BE2',1,parent['id'],node['id'],previous)
        if node['name']=='input':queue(parent['id'])
    def attribute(parent,name,tag,raw):
        a=allocate(0,name)
        if tag==2 and len(raw)>=16:service('attribute-value')
        a['tag']=(3 if not raw else 2 if len(raw)<16 else 1)if tag==2 else tag;a['value']=encoded(raw)
        observed=attached(parent)
        if observed:service('BE2-record:5')
        a['parent']=parent['id'];parent['attributes'].append(a['id'])
        state['trace'].append(f"attribute:{parent['id']}:{a['id']}")
        if observed:journal('BE2',5,parent['id'],a['id'])
        if parent['name']=='input':queue(parent['parent'])
    def begin(name):
        nonlocal depth
        service('history-begin:'+name);depth=1;event('begin:'+name)
    def end(name):
        nonlocal depth,pending,retained,pending_field,retained_field
        service('history-end:'+name)
        if pending:
            state['trace'].append('1165C0:missing-fresh-BBD');event('before-end');retained=1;retained_field=pending_field;pending=0;pending_field=0
        depth=0;event('merged-end:'+name)
        if retained:state['trace'].append('dispatch:C9D:field-input-changed');retained=0;retained_field=0
        event('end:'+name)
    try:
        root=allocate(4,'');state['root']=root['id'];page=allocate(3,'page');namespace=allocate(2,'')
        namespace['uri']='http://www.myscript.com/ns/myscript/page';child(page,namespace)
        attribute(page,'version',7,struct.pack('<i',5))
        content=allocate(3,'content');layout=allocate(3,'layout');child(page,content);child(page,layout);child(root,page)
        state['trace'].append('attach:same-CB7:original-Ink+DOM');event('attached')
        first=(255<<8 if counts[0]==1 else 0)|3
        last=((len(counts)-1)<<32)|((counts[-1]-1)<<16)|(255<<8)|5
        ranges=encoded(struct.pack('<QQ',first,last))
        for field_id in ids:
            begin('field');field=allocate(3,'textField');attribute(field,'id',2,field_id.encode());child(content,field);end('field')
            begin('addItems');inp=allocate(3,'input');child(field,inp);service('C51-register')
            assigned=state['next'];state['maximum']=max(state['maximum'],assigned);state['next']=assigned+1
            state['inputs'].append(dict(field=field_id,fieldNode=field['id'],inputNode=inp['id'],id=assigned,category=100,name='INPUT',ranges=ranges))
            journal('C4D',4,0,0,registry=assigned,category=100)
            state['trace']+=['register:INPUT:'+str(assigned)+':100','CB7:new-tag-state0:'+str(assigned)+':discard-before-rewrite']
            state['sourceEvents'].append([assigned,0,100]);attribute(inp,'tag',4,struct.pack('<q',assigned));end('addItems')
        state['succeeded']=True
    except Stop:state['faulted']=True
    return state

def check_inverse(state):
    """Replay inverse completed mutations on a private value graph. Native
    global history ownership, allocator pruning and source counters stay separate."""
    nodes=copy.deepcopy(state['nodes']);registry={r['id'] for r in state['inputs']}
    for record in reversed(state['history']):
        if record['source']=='C4D':registry.remove(record['registry']);continue
        child=nodes[record['node']-1];parent=nodes[record['parent']-1]
        lane=parent['children'] if record['kind']==1 else parent['attributes']
        assert child['parent']==parent['id'] and child['id'] in lane
        if record['kind']==1:
            pos=lane.index(child['id']);assert record['previous']==(lane[pos-1]if pos else 0)
            assert record['next']==(lane[pos+1]if pos+1<len(lane)else 0)
        lane.remove(child['id']);child['parent']=0
    assert not registry
    if state['root']:assert nodes[state['root']-1]['children']==[]

def audit(path):
    capture=json.loads(Path(path).read_text());actual=failures=cancellations=histories=0
    for row in capture['cases']:
        wanted=native_plan(row['ids'],row['pointCounts'],row['fault'],row['role']=='preparedCancel')
        for key,value in wanted.items():assert row[key]==value,(row['role'],row['fault'],key,row[key],value)
        assert row['c51']==encoded(struct.pack('<qqB',wanted['next'],wanted['maximum'],0))
        check_inverse(row);histories+=len(row['history'])
        if row['role']=='actual':actual+=1
        elif row['role']=='preparedFailure':failures+=1
        else:cancellations+=1
    assert(actual,failures,cancellations)==(64,248,248)
    assert capture['guards']==1278 and capture['streams']==560
    print(f'PASS independent native ordering/full DOM/registry/journal/inverse-prefix: {actual} raw roots, {failures}+{cancellations} fault prefixes, {histories} completed mutations, 1278 guards')
if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('capture');audit(p.parse_args().capture)
