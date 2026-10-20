"""Independent original PAGE construction, real result metadata tree and FIELD edits.

No vendor DLL, ignored research tree, expected label or runtime output tree is
used as an input to recognition or as a substitute for payload reconstruction.
"""
import base64,copy,hashlib,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
old=root/'tools/LegacyOriginalSelectedDocument.Probe/audit_output.py'
ns={'__file__':str(old)}
exec(compile(old.read_text(encoding='utf8').split('total=0')[0],str(old),'exec'),ns)
b64=base64.b64decode
enc=lambda x:base64.b64encode(x).decode()
data=json.loads(Path(sys.argv[1]).read_bytes())

def node(kind,name='',tag=0,value=b'',children=None,attributes=None,origin=0,prefix=None,uri=None):
    return dict(kind=kind,name=name,tag=tag,value=enc(value),children=children or [],attributes=attributes or [],original=origin,prefix=prefix,uri=uri)

def initial(count):
    fields=[]
    for i in range(count):
        start=7+4*i
        inp=node(3,'input',children=[],attributes=[node(0,'tag',4,struct.pack('<q',i),origin=start+3)],origin=start+2)
        fields.append(node(3,'textField',children=[inp],attributes=[node(0,'id',2,('field'+str(i)).encode(),origin=start+1)],origin=start))
    page=node(3,'page',origin=2,attributes=[node(0,'version',7,struct.pack('<i',5),origin=4)],children=[
        node(2,origin=3,prefix='',uri='http://www.myscript.com/ns/myscript/page'),
        node(3,'content',children=fields,origin=5),node(3,'layout',origin=6)])
    return node(4,origin=1,children=[page])

def flatten(n):
    yield n
    for a in n['attributes']+n['children']:yield from flatten(a)

def identify(tree,start=1):
    tree=copy.deepcopy(tree)
    for i,n in enumerate(flatten(tree),start):n['id']=i
    return tree,start+len(list(flatten(tree)))

def original_payload(n):
    out={k:v for k,v in n.items()if k not in ('id','nameId')}
    out.update(original=0,prefix=None,uri=None)
    out['attributes']=[original_payload(a)for a in n['attributes']]
    out['children']=[original_payload(a)for a in n['children']]
    return out

def check(s):
    mode=s['mode'];count=1 if s['case']==18 else 3 if s['case']==19 else 2
    raw_payload=ns['ns']['expected'](s['source'])
    assert s['writer']==ns['ids'](raw_payload)[0],(s['case'],'actual native metadata payload')
    payload=original_payload(raw_payload);payload_nodes=len(list(flatten(payload)))
    page,next_id=identify(initial(count));n_initial=next_id-1
    assert s['historicalPage']==page,(s['case'],'whole original PAGE constructor/order/names')
    assert s['sourceName']=='./page/content/textField[@id="field'+str(s['chosen'])+'"]'
    edit=bytearray(40);struct.pack_into('<I',edit,0,2);struct.pack_into('<I',edit,24,1)
    assert b64(s['newEdit40'])==edit
    assert list(map(b64,s['oldEdit40']))==[struct.pack('<I',2)+bytes(36)]
    # The entire original kind3 FIELD, including id/INPUT/tag, is cloned.
    field=page['children'][0]['children'][1]['children'][s['chosen']]
    scratch,next_scratch=identify(field,next_id);n_field=len(list(flatten(field)))
    assert n_field==4
    inserted,after_insert=identify(payload,next_scratch)
    scratch['children'].append(inserted)
    pre=s['preflight'];final=s['final']
    if mode in (0,1,2):
        completed=0 if mode==0 else 3 if mode==1 else 2
        assert pre==final
        assert pre['entered'] and not pre['worked'] and pre['compatible'] and pre['faulted'] and not pre['ready']
        assert not pre['applyEntered'] and not pre['succeeded'] and pre['epoch']==0
        assert pre['allocated']==n_initial+completed and pre['attempts']==completed+1
        assert pre['trace']==['try:scratch-node']*(completed+1) and 'scratch' not in pre
    else:
        assert pre['entered'] and pre['worked'] and pre['compatible'] and pre['ready'] and not pre['faulted']
        assert pre['allocated']==n_initial+n_field+payload_nodes and pre['attempts']==n_field+payload_nodes+2
        assert pre['page']==page and pre['scratch']==scratch
        assert pre['trace']==['try:scratch-node']*n_field+['try:map-all-parents']+['try:payload-node']*payload_nodes+[
            'try:append','append:'+str(scratch['id'])+':'+str(inserted['id'])+':1']
        if mode in (7,8,9,10,11):assert final==pre
        else:
            failed=mode in(3,4,5,6)
            assert final['entered'] and final['worked'] and final['compatible'] and final['applyEntered']
            assert final['faulted']==failed and final['succeeded']==(not failed) and final['ready']==(not failed)
            assert final['epoch']==(0 if failed else 1)
            assert final['scratch']==scratch
            live=copy.deepcopy(page);livefield=live['children'][0]['children'][1]['children'][s['chosen']]
            livepayload,_=identify(payload,after_insert)
            if mode not in (3,4,6):livefield['children'].append(livepayload)
            assert final['page']==live,(s['case'],'actual original field append/preserved siblings')
            if mode==3:assert final['trace']==['try:map-all-parents'] and final['allocated']==pre['allocated'] and final['attempts']==1
            elif mode in(4,6):
                allocated=2 if mode==4 else 1
                assert final['trace']==['try:map-all-parents']+['try:payload-node']*(allocated+1)
                assert final['allocated']==pre['allocated']+allocated and final['attempts']==allocated+2
            else:
                assert final['allocated']==pre['allocated']+payload_nodes and final['attempts']==payload_nodes+3
                assert final['trace']==['try:map-all-parents']+['try:payload-node']*payload_nodes+[
                    'try:append','append:'+str(livefield['id'])+':'+str(livepayload['id'])+':1','try:postcheck']
            assert final['map']==[[c['tag'],c['tag']]for c in s['commands']if c['operation']==4]
    selections=[(c['tag'],'CHAR',c['ranges16'])for c in s['source']['characters']if c['tag']!=-1]
    if s['source']['word']['tag']!=-1:selections.append((s['source']['word']['tag'],'WORD',s['source']['word']['ranges16']))
    selections.append((s['source']['text']['tag'],'TEXT',s['source']['text']['ranges16']))
    assert s['commands']==[dict(operation=4,id=i+1,tag=t,name=n,ranges16=r)for i,(t,n,r)in enumerate(selections)]+[dict(operation=7,id=len(selections)+1)]
    assert [t for t,_,_ in selections]==list(range(count,count+len(selections)))
    # Original INPUTs retain full raw-domain C79 ranges; actual source commands
    # add every produced CHAR/WORD/TEXT without renumbering those INPUTs.
    counts=[6] if s['case']>=6 or s['case']<2 else [33,25,33,33,25,33]
    domain=enc(struct.pack('<QQ',3,((len(counts)-1)<<32)|((counts[-1]-1)<<16)|0xff05))
    expected_registry=[dict(id=i,name='INPUT',ranges16=domain)for i in range(count)]
    if mode not in(0,1,2,10,11):expected_registry +=[dict(id=t,name=n,ranges16=r)for t,n,r in selections]
    assert s['registry']==expected_registry,(s['case'],'complete original registry')
    if final['succeeded'] or mode==5:
        actual_field=final['page']['children'][0]['children'][1]['children'][s['chosen']]
        registry={e['id']:e for e in expected_registry}
        normalize=lambda p:p&~0xff00 if p>>8&255==255 else p
        lower,upper=struct.unpack('<QQ',b64(domain))
        for n in flatten(actual_field):
            if n['kind']!=3:continue
            attrs=[a for a in n['attributes']if a['name']=='tag']
            assert len(attrs)<=1
            for tag in attrs:
                assert tag['kind']==0 and tag['tag']==4 and len(b64(tag['value']))==8
                value=struct.unpack('<q',b64(tag['value']))[0];entry=registry[value]
                raw=b64(entry['ranges16']);assert len(raw)%16==0
                for first,last in struct.iter_unpack('<QQ',raw):
                    assert first<=last and normalize(lower)<=normalize(first)<=normalize(upper) and last<=upper
    assert s['oldApplied']==final['succeeded']
    if not final['applyEntered']:assert final['page']==page

for s in data['actual']:check(s)
assert len(data['actual'])==20 and data['guards']>=300
def stable(x):
    if isinstance(x,list):return[stable(v)for v in x]
    if not isinstance(x,dict):return x
    out={k:stable(v)for k,v in x.items()}
    if 'uuid36'in out:out['uuid36']='LIBRARY-UUID'
    if out.get('name')=='gen':out['value']='LIBRARY-UUID'
    return out
digest=hashlib.sha256(json.dumps(stable(data),sort_keys=True,separators=(',',':')).encode()).hexdigest()
Path(str(sys.argv[1])+'.stable.sha256').write_text(digest+'\n',encoding='ascii')
print(f'PASS independent20 actual whole-PAGE/FIELD edits/source identity maps/scratch and live prefixes; stableSHA={digest}')
