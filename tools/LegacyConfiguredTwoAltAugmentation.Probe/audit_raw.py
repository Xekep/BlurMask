"""RAW equality/rank route auditor. No target/model execution; not a whole219580 oracle."""
import base64, importlib.util, json, math, struct, sys
from pathlib import Path
spec=importlib.util.spec_from_file_location('raw_reference',Path(__file__).with_name('reference_raw.py'))
r=importlib.util.module_from_spec(spec);spec.loader.exec_module(r)

def audit(data):
    keys={'schema','complete','faulted','marked','diagnosticFailure','allocatedLogical','memoryVisits','copyBytes','pendingClearBytes','sourceDebit','events'}
    assert type(data) is dict and set(data)==keys
    r.integer(data['schema'],1,1)
    for k in ('complete','faulted','marked','diagnosticFailure'):assert type(data[k]) is bool
    assert data['complete'] and not data['faulted'] and data['marked'] and not data['diagnosticFailure']
    for k in ('allocatedLogical','memoryVisits','copyBytes','pendingClearBytes'):r.integer(data[k],0,128*1024*1024)
    debit=data['sourceDebit']
    assert type(debit) is dict and set(debit)=={'copies','work','phase','activeGroup','sites'}
    r.integer(debit['copies'],0,128*1024*1024);r.integer(debit['work'],0,64*1024*1024)
    r.integer(debit['phase'],2,2);r.integer(debit['activeGroup'],-1,-1)
    sites=debit['sites'];assert type(sites) is list and len(sites)==12
    for i,site in enumerate(sites):
        assert type(site) is dict and set(site)=={'site','copies','work'}
        r.integer(site['site'],i,i);r.integer(site['copies'],0,128*1024*1024);r.integer(site['work'],0,64*1024*1024)
    assert sum(site['copies'] for site in sites)==debit['copies']
    assert sum(site['work'] for site in sites)==debit['work']
    # Current source premises: fixed Owner288 + one scoring scope24;
    # Neural is charged once immediately BEFORE each actual four-family miss.
    assert sites[0]['copies']==288+24 and sites[0]['work']==128+64
    nc,nw=sites[10]['copies'],sites[10]['work']
    assert nc%14824==0 and nw%386624==0 and nc//14824==nw//386624
    misscharges=nc//14824
    events=data['events'];assert type(events) is list and 1<=len(events)<=32768
    decoded=[]
    for event in events:
        assert type(event) is dict and set(event)=={'kind','argument','raw'}
        r.integer(event['kind'],1,25);r.integer(event['argument'],-1,131072)
        assert type(event['raw']) is str
        raw=base64.b64decode(event['raw'],validate=True)
        assert base64.b64encode(raw).decode()==event['raw']
        decoded.append((event['kind'],event['argument'],raw))
    assert sum(len(e[2]) for e in decoded)<=32*1024*1024
    current=-1
    for kind,arg,raw in decoded:
        if kind==1:current=arg
        elif kind in (2,3,4,9,13,14,16,17,19,20,21,22,23,24,25):
            assert arg==current,('candidate chronology',kind,arg,current)
    pos=0;comparisons=0;reads_total=0;temporary=0
    def take(kind):
        nonlocal pos
        assert pos<len(decoded) and decoded[pos][0]==kind,('event',pos,kind)
        event=decoded[pos];pos+=1;return event
    def words(kind):
        out=[]
        while pos<len(decoded) and decoded[pos][0]==kind:
            b=take(kind)[2];assert len(b)==120;out.append(b)
        return out
    def snapshot(kind,final=False):
        nonlocal temporary
        _,n,b=take(kind);assert 1<=n<=4095 and len(b)==16
        span=r.ints(b);selected=span[2:];assert all(0<=i<(4095 if final else n) for i in selected)
        w=words(19);assert len(w)==n;temporary+=n
        return w,selected,span[:2]
    def compare(left,right):
        nonlocal comparisons,reads_total
        _,number,before=take(5);comparisons+=1;assert number==comparisons
        assert before[:120]==left and before[120:240]==right
        reads=[];stream=[]
        while pos<len(decoded) and decoded[pos][0] in (6,7):
            k,c,b=decoded[pos]
            assert c==number;take(k)
            if k==6:assert len(b)==48;reads.append(b)
            else:assert len(b)>=2 and len(b)%2==0
            stream.append((k,b))
        # Preserve the FULL interleave of pool reads and control lookups.
        expected=r.equality(before,stream);_,c,after=take(8)
        assert c==number and after==expected
        reads_total+=len(reads);return bool(after[0])
    compact=take(16)[2];assert len(compact)%40==0 and 1<=len(compact)//40<=131072
    starts=r.ints(take(17)[2]);assert starts and all(0<=i<=len(compact)//40 for i in starts)
    initial,selected,span=snapshot(18)
    scan=r.ints(take(2)[2]);assert len(scan)==2 and len(set(scan))==2
    assert all(0<=i<len(compact)//40 for i in scan)
    assert all(compact[i*40+32]&8 and r.I32(compact,i*40+36)==-1 for i in scan)
    for index,candidate in enumerate(scan):
        _,ordinal,b=take(1);assert ordinal==candidate and not b
        _,ordinal,rows=take(3);assert ordinal==candidate and len(rows)%72==0 and len(rows)<=4096*72
        _,ordinal,word=take(4);assert ordinal==candidate and len(word)==120;temporary+=1
        match=-1
        for i,old in enumerate(initial):
            if compare(old,word):match=i;break
        _,ordinal,outcome=take(9);assert ordinal==candidate and len(outcome)==16
        got=r.ints(outcome)
        # Strictly scoped observed route: BOTH emitted candidates duplicate an
        # original WORD; no append/update numerical theorem is claimed here.
        assert match>=0 and got==[match,0,index+1,0]
    before,selected2,span2=snapshot(10);assert before==initial and selected2==selected and span2==span
    ranked=r.ranking(before,selected2,compare)
    assert words(20)==ranked['words']
    rankingrows=[]
    while pos<len(decoded) and decoded[pos][0]==21:
        row=take(21)[2];assert len(row)==20;rankingrows.append(row)
    assert rankingrows==ranked['rows']
    for kind,name in ((22,'mapping'),(23,'released'),(24,'sources'),(25,'releasedsources')):
        assert r.ints(take(kind)[2])==ranked[name]
    _,quota,b=take(11);assert quota==ranked['quota'] and not b
    post,postselected,postspan=snapshot(12)
    assert post==ranked['words'] and postspan==span
    assert postselected==[ranked['mapping'][i] for i in selected]
    projected,order=r.normalize(post)
    actual=words(13);assert len(actual)==len(projected)
    for a,b in zip(actual,projected):
        assert a[:20]==b[:20] and a[24:]==b[24:]
        # Only the MathF.Exp rank channel is compared to independent libm
        # with explicit <=2 F32 ULP tolerance; every other byte is exact.
        x,y=struct.unpack_from('<I',a,20)[0],struct.unpack_from('<I',b,20)[0]
        assert abs(x-y)<=2 and math.isfinite(r.F32(a,20)) and r.F32(a,20)>=0
    assert r.ints(take(14)[2])==order
    final,finalselected,finalspan=snapshot(15,True)
    assert final==[actual[i] for i in postselected] and finalselected==postselected and finalspan==span
    assert pos==len(decoded),'extra trace events'
    # Derive all cumulative backing allocation/growth, header/payload writes,
    # full future clear and terminal traversal charges from actual event lengths.
    payloadcap=headercap=used=count=0;allocated=192;visits=5824;copies=0
    for _,_,raw in decoded:
        need=used+len(raw)
        if need>payloadcap:
            cap=min(32*1024*1024,max(need,max(4096,payloadcap*2)))
            visits+=2*cap+used;allocated+=cap;copies+=used;payloadcap=cap
        if count==headercap:
            cap=min(32768,max(64,headercap*2))
            visits+=32*cap+16*count;allocated+=16*cap;copies+=16*count;headercap=cap
        visits+=4*len(raw)+144;copies+=len(raw);used=need;count+=1
    visits+=240*temporary;allocated+=120*temporary;copies+=120*temporary
    assert data['allocatedLogical']==allocated and data['memoryVisits']==visits
    assert data['copyBytes']==copies and data['pendingClearBytes']==payloadcap+16*headercap
    return dict(scope='Two actual duplicate-original routes and observed finite non-special ranking only; walker/emission/cost derivation OPEN.',
                sourceDebitTotals=dict(copies=debit['copies'],work=debit['work']),augmentationFourFamilyMissReservations=misscharges,wholePipelineNNCountInstrumented=False,
                candidates=2,comparisons=comparisons,orderedGlyphReads=reads_total,events=count,
                nativeExecution=False,targetExecution=False,whole219580=False)

if __name__=='__main__':
    assert len(sys.argv)==3,'raw-terminal.json fresh-report.json'
    output=Path(sys.argv[2]);assert not output.exists()
    report=audit(r.strict_json(Path(sys.argv[1])))
    output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print('PASS RAW observed equality/rank route; whole219580 remains OPEN')
