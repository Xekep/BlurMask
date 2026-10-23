"""Future actual-owner output oracle. Does not execute .NET or vendor code.

Input is the owning probe's raw pre-transfer heap and before-context snapshot.
All expected phase0 outputs are independently reconstructed from native rules.
The recursive HEIGHT AVL is the same independent method already frozen for the
previous initial consumer; it does not replay captured balance transitions.
"""
import sys,json,hashlib
from pathlib import Path
HERE=Path(__file__).resolve().parent;sys.path.insert(0,str(HERE))
from native_phase0_oracle import phase0,u,s,put,IMP
from immutable_baseline_adapter import adapt_known_134
def complete_contexts(before,slots):
    nodes={x[0]:x.copy() for x in before}
    roots={k for k,x in nodes.items() if x[1]==0};children={v for k in roots for v in nodes[k][3:5] if v}
    root,=roots-children
    def height(k):return 0 if not k else 1+max(height(nodes[k][3]),height(nodes[k][4]))
    def refresh(k):nodes[k][6]=height(nodes[k][4])-height(nodes[k][3]);return k
    def left(k):
        r=nodes[k][4];nodes[k][4]=nodes[r][3];nodes[r][3]=k;refresh(k);return refresh(r)
    def right(k):
        l=nodes[k][3];nodes[k][3]=nodes[l][4];nodes[l][4]=k;refresh(k);return refresh(l)
    def add(k,label):
        if not k:
            identity=len(nodes)+1;index=identity-1;nodes[identity]=[identity,0,0,0,0,label,0,index//102,index%102];return identity
        if label<nodes[k][5]:nodes[k][3]=add(nodes[k][3],label)
        elif label>nodes[k][5]:nodes[k][4]=add(nodes[k][4],label)
        else:return k
        refresh(k)
        if nodes[k][6]>1:
            if label<nodes[nodes[k][4]][5]:nodes[k][4]=right(nodes[k][4])
            return left(k)
        if nodes[k][6]<-1:
            if label>nodes[nodes[k][3]][5]:nodes[k][3]=left(nodes[k][3])
            return right(k)
        return k
    pool=bytearray(250*80)
    for i,slot in enumerate(slots):
        identity=u(slot,8,8);leaf=nodes[identity];parent=nodes[leaf[1]]
        assert leaf[5]<217 and parent[5]==0xfffa and parent[1]==0
        symbol=leaf[5];root=add(root,symbol);mapped=next(k for k,x in nodes.items() if x[1]==0 and x[5]==symbol)
        at=i*80;pool[at:at+8]=slot[16:24];put(pool,at+8,mapped,8);put(pool,at+16,identity,8)
        pool[at+24:at+26]=slot[30:32];pool[at+26]=slot[32];pool[at+30]=slot[38];pool[at+31]=32|(slot[33]&31)
        put(pool,at+40,IMP);put(pool,at+44,IMP)
    return [nodes[k] for k in sorted(nodes)],pool
def normalize_nodes(raw):
    ids={n[0]:i+1 for i,n in enumerate(raw)}
    return [[(0 if x==0 else ids[x]) if j<5 else x for j,x in enumerate(n)] for n in raw],ids
BASELINES={'134':('global-en-case-consumer-successor-134-managed-20261023.json','d1ffe91e88cf7e48f7d68cda029790c2d73f9e8c9f9d8c5640890c3ff1c712d0'),
 '100':('global-en-case-consumer-successor-100-managed-20261023.json','f68d9507a6d857235a7bd73394ee931862681274d530f3bc74237a6cad4ce818')}
def read_baseline(dataset,baseline_path,source_input_path=None):
    assert dataset in BASELINES,'Explicit134/100 dataset required'
    name,digest=BASELINES[dataset];raw=Path(baseline_path).read_bytes()
    if hashlib.sha256(raw).hexdigest()!=digest:
        assert dataset=='134' and source_input_path is not None,'Immutable baseline or exact known source provenance required'
        raw=adapt_known_134(raw,Path(source_input_path).read_bytes())
    assert hashlib.sha256(raw).hexdigest()==digest
    rows=json.loads(raw)['Rows'];assert len(rows)==int(dataset) and len({r['Name'] for r in rows})==int(dataset)
    return rows
def bind_prefix(capture,dataset,baseline_path,source_input_path=None):
    baseline=read_baseline(dataset,baseline_path,source_input_path);rows=capture['Rows'];expected_names=[r['Name'] for r in baseline]
    assert len(rows)==int(dataset) and len({r['Name'] for r in rows})==int(dataset),'Exact unique owner denominator required'
    assert [r['Name'] for r in rows]==expected_names,'Exact ordered immutable owner names required'
    bound=[]
    for r,prior in zip(rows,baseline):
        before,before_ids=normalize_nodes(r['BeforeContexts'])
        assert before==prior['Nodes'],(r['Name'],'immutable before-context mismatch')
        assert before_ids[r['BeforeRoot']]==prior['Root'],(r['Name'],'immutable before-root mismatch')
        assert len(r['Input'])==len(prior['Heap']),(r['Name'],'immutable before-heap denominator')
        slots=[]
        for i,(h,expected) in enumerate(zip(r['Input'],prior['Heap'])):
            slot=bytearray.fromhex(h);put(slot,8,before_ids[u(slot,8,8)],8)
            assert slot==bytes.fromhex(expected),(r['Name'],i,'immutable full128 before-heap mismatch')
            put(slot,88,i);slots.append(slot)
        bound.append((r,before,slots))
    return bound
def check(capture,dataset,baseline_path,source_input_path=None):
    report={'status':'PASS_ACTUAL_PRIMARY_OWNER_INDEPENDENT_NATIVE_ORACLE','owners':0,'rows':0,'newNodeMaximum':0,'vendorExecuted':False}
    stages=['query:reset-normal','query:reset-auxiliary','query:reset-backend','query:reset','query:before-D0','span:entered','source:bytes','source:expanded','source:refreshed','source:forwarded','span:installed','query:D0','query:map-installed','query:before-read','query:page','query:publish']
    expected_guards=['budget-low','budget-high','one-owner-run','one-receipt-export','mutation-after-transfer','preentry-cancel']
    for stage in stages:expected_guards.extend(['cancel:'+stage,'canceled-retry:'+stage])
    expected_guards.extend(['publish-fault','outside-completion-seam','outside-transfer-seam','same-byte-foreign-lease','foreign-consumer','foreign-heap','stale-closed-lease','foreign-prepared-attachment'])
    assert len(expected_guards)==46 and len(set(expected_guards))==46
    guards=capture['Guards'];assert guards['Names']==expected_guards and guards['Count']==46 and len(set(guards['Names']))==46,'Exact46 reviewed guard names required'
    assert guards['Cancellations']==stages and guards['CancellationCount']==16,'Exact ordered16 cancellation stages required'
    for r,before,slots in bind_prefix(capture,dataset,baseline_path,source_input_path):
        after,after_ids=normalize_nodes(r['AfterContexts'])
        contexts,pool=complete_contexts(before,slots);assert contexts==after,(r['Name'],'AVL all9 fields/page order')
        roots={x[0] for x in contexts if x[1]==0};children={v for x in contexts if x[1]==0 for v in x[3:5] if v}
        expected_root,=roots-children;assert after_ids[r['AfterRoot']]==expected_root
        assert len(contexts)<=1506 and len(contexts)-len(before)<=217
        oracle=phase0(b''.join(slots)+bytes((250-len(slots))*128),len(slots),pool)
        count=oracle['Count'];rows=bytearray(125*88);alts=bytearray(125*80);rm=bytearray(len(rows));am=bytearray(len(alts));ordinals=[-1]*250
        for i in range(125):
            put(rows,i*88+16,IMP);put(rows,i*88+20,IMP);rm[i*88+16:i*88+24]=b'\xff'*8
            put(alts,i*80,IMP);put(alts,i*80+4,IMP);am[i*80:i*80+8]=b'\xff'*8
        for i,x in enumerate(oracle['Rows']):
            rows[i*88:(i+1)*88]=bytes.fromhex(x['Record']);alts[i*80:(i+1)*80]=bytes.fromhex(x['Primary']);ordinals[i]=x['Ordinal'];rm[i*88:(i+1)*88]=b'\xff'*88
            for j in range(2):
                at=i*80+j*40;am[at:at+8]=b'\xff'*8;am[at+30]=255
                if s(alts,at)<IMP:am[at+8:at+27]=b'\xff'*19;am[at+31]=0x3f
        actual_rows=bytearray.fromhex(r['Rows']);actual_alts=bytearray.fromhex(r['Alternatives'])
        for i in range(count):
            put(actual_rows,i*88+8,after_ids[u(actual_rows,i*88+8,8)],8)
            for j in range(2):
                at=i*80+j*40
                if s(actual_alts,at)<IMP:
                    for off in (8,16):put(actual_alts,at+off,after_ids[u(actual_alts,at+off,8)],8)
        assert actual_rows==rows and actual_alts==alts,(r['Name'],'all88/80 bytes including residue')
        assert bytes.fromhex(r['RowMasks'])==rm and bytes.fromhex(r['AlternativeMasks'])==am
        assert r['Ordinals']==ordinals and r['Count']==count and r['InitialCount']==len(slots)
        assert r['ContextNodes']==len(contexts) and r['ReservedContextResolutions']==3*len(slots)
        assert r['LogicalPayloadBytes']==690192-136*(250-len(slots))-168*(125-count)
        assert r['ReservedCopyWorkBytes']==32000+384*len(slots)**2+2880*len(slots)+168*count
        assert r['ReservedCopyWorkBytes']<=64*1024*1024
        report['owners']+=1;report['rows']+=count;report['newNodeMaximum']=max(report['newNodeMaximum'],len(contexts)-len(before))
    assert report['owners']==int(dataset);report['dataset']=dataset;report['baselineSha256']=BASELINES[dataset][1]
    report['guardCount']=46;report['cancellationStageCount']=16;report['guardPayloadVerified']=True
    report['deterministicMidSortContextCancellationProved']=False
    return report
if __name__=='__main__':
    assert len(sys.argv) in (4,5),'check_owner.py capture.json 134|100 immutable-prefix-baseline.json [exact-known-source-input.json]'
    path=Path(sys.argv[1]);result=check(json.loads(path.read_text()),sys.argv[2],sys.argv[3],sys.argv[4] if len(sys.argv)==5 else None);result['captureSha256']=hashlib.sha256(path.read_bytes()).hexdigest();print(json.dumps(result))
