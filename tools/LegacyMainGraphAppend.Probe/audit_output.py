"""Independent value/edge oracle for ordinary19D470 append-stage snapshots.
No vendor execution, classifier, cross-source merge or boundary pruning.
"""
import base64,copy,json,pathlib,sys
j=json.loads(pathlib.Path(sys.argv[1]).read_text());appends=arcs=groups=contexts=wrapped=sticky=0
def shift(index,offset):return index if index==65535 else index+offset
def expected(before,local):
    global groups,contexts,arcs,wrapped
    assert before['cap']==local['cap'];cap=before['cap'];node_base=len(before['nodes']);group_base=len(before['groups']);context_base=len(before['contexts'])
    mapping=[];live=[]
    for n,g in enumerate(local['groups']):
        if g['fields'][3]==0:mapping.append(-1)
        else:mapping.append(group_base+len(live));live.append((n,g))
    result=copy.deepcopy(before);result['contexts']+=copy.deepcopy(local['contexts']);contexts+=len(local['contexts'])
    for n,g in live:
        out=copy.deepcopy(g);out['fields'][2]=shift(out['fields'][2],context_base)
        eligible=any((i<cap or i==len(local['nodes'])-1) and any(a['header'][0]==n for a in row) for i,row in enumerate(local['nodes']))
        out['history']=copy.deepcopy(g['history']) if eligible else []
        if eligible:out['fields'][4]=1
        result['groups'].append(out);groups+=1
    for row in local['nodes']:
        out=[]
        for arc in row:
            a=copy.deepcopy(arc);a['header'][0]=mapping[a['header'][0]]
            for slot in (4,5):
                value=a['header'][slot]+((node_base&0xffffff)<<7);wrapped+=value>0xffffffff;a['header'][slot]=value&0xffffffff
            a['header'][6]=shift(a['header'][6],context_base);out.append(a);arcs+=1
        result['nodes'].append(out)
    if node_base:
        #Root stitching is an ordered product: every old terminal ordinal,
        #then every new sole-root arc in native node/ordinal scan order.
        terminal_count=len(before['nodes'][-1])
        for selector in range(min(cap,len(local['nodes']))):
            roots=[n for n,a in enumerate(local['nodes'][selector]) if a['previous']==[-1]]
            for ordinal in roots:result['nodes'][node_base+selector][ordinal]['previous']=list(range(terminal_count))
            for ordinal in range(terminal_count):result['nodes'][node_base-1][ordinal]['next'][selector]+=roots
    else:result['root']=copy.deepcopy(local['root'])
    return result,mapping,[node_base,group_base,context_base]
def check(test):
    global appends
    out,mapping,bases=expected(test['before'],test['local']);assert test['output']==out
    assert test['map']==mapping
    if 'base' in test:assert test['base']==bases
    appends+=1
for t in j['prepared']:check(t)
for t in j['counters']:
    tracked=200 if t['allocated']>=200 else t['allocated']-t['removed'];assert tracked==t['counter'];sticky+=tracked==200
    assert t['local']['groups'][0]['fields'][3]==tracked
    assert len(t['local']['nodes'][0])==t['allocated']-t['removed']
    empty=dict(cap=t['local']['cap'],groups=[],contexts=[],nodes=[],root=copy.deepcopy(t['local']['root']))
    check(dict(before=empty,local=t['local'],output=t['output'],map=t['map']))
for t in j['pending']:
    raw=base64.b64decode(t['input'])
    if any(raw[:4]):
        assert t['ordinal']==1 and len(t['contexts'])==2
        out=bytearray(16);out[:6]=raw[:6];out[8:12]=raw[8:12]
        assert base64.b64decode(t['contexts'][1]['prefix'])==out
        assert t['contexts'][1]['controls'][0]==-7
        #The second vector item is the input generator's code, independently
        #encoded in untouched byte12 before native prefix copying drops it.
        assert t['contexts'][1]['controls'][1]==(raw[12]-84)&255
    else:assert t['ordinal']==65535 and len(t['contexts'])==1
for t in j['actual']:
    assert len(t['steps'])==2
    for n,s in enumerate(t['steps']):
        assert s['descriptor']==n*2 and s['line']==n
        assert s['local']['groups'][0]['fields']==[n*2,65535,65535 if n==0 else 0,1,1,0,0,0]
        assert s['local']['groups'][0]['ranges'][0][0]==n
        if n:
            c=s['local']['contexts'][0]
            assert base64.b64decode(c['prefix'])==bytes([1])+bytes(15) and c['controls']==[-7]
        check(s)
assert j['preparedCount']==288 and j['counterCases']==18 and j['actualCount']==3 and j['guards']>=12
assert wrapped and sticky
print(f'PASS independent{appends} append snapshots/{groups} mapped groups/{arcs} copied arcs/{contexts} auxiliary records;{wrapped} UINT endpoint wraps/{sticky} sticky counter states;32 pending prefixes/3 actual cut-source owners')
