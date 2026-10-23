"""STATIC native number-node0 reference. Never executes a vendor or C# module.

2144F0 root path, 20E6A0 two count-2 pools, 1FC940 ordering, 1F9830
then 1F93A0 duplicate merge, 203DE0 heap rebuild, phase0 quota/materialize,
and the COMPLETE skipped-language common tail. Inputs are historical captures;
prepared controls below confer no live owner or continuation authority.
"""
import hashlib, json, struct
IMP = 0x4000000
H, L, E = 250, 125, 156
def u(b,o,n=4): return int.from_bytes(b[o:o+n], 'little')
def s(b,o): return int.from_bytes(b[o:o+4], 'little', signed=True)
def put(b,o,v,n=4): b[o:o+n]=(v&((1<<(8*n))-1)).to_bytes(n,'little')
def key(b):
    # 1FC940, NOT English 1FCA30: no history70/68 comparison.
    return (u(b,0,8),u(b,0x24,2),u(b,0x18),s(b,0x30),s(b,0x34),u(b,0x1c,2),b[0x21]>>7)
def quicksort(rows,lo,hi,row_masks=None):
    if lo>=hi:return
    l,r=lo,hi;pivot=bytes(rows[(lo+hi)//2])
    while True:
        while key(rows[l])<key(pivot):l+=1
        while key(rows[r])>key(pivot):r-=1
        if l>r:break
        rows[l],rows[r]=rows[r],rows[l]
        if row_masks is not None:row_masks[l],row_masks[r]=row_masks[r],row_masks[l]
        l+=1;r-=1
        if l>r:break
    if lo<r:quicksort(rows,lo,r,row_masks)
    if l<hi:quicksort(rows,l,hi,row_masks)
def select(pool,a,b,first_a):
    def entry(row,i):return pool[(row*2+i)*40:(row*2+i+1)*40]
    selected=[(a if first_a else b,0)];ids={u(entry(selected[0][0],0),8,8)}
    ai,bi=(1,0) if first_a else (0,1)
    while len(selected)<2:
        av=ai<2 and s(entry(a,ai),0)<IMP;bv=bi<2 and s(entry(b,bi),0)<IMP
        if not(av or bv):break
        if av and (not bv or s(entry(a,ai),0)<s(entry(b,bi),0)):row,i=a,ai;ai+=1
        else:row,i=b,bi;bi+=1
        ident=u(entry(row,i),8,8)
        if ident not in ids:ids.add(ident);selected.append((row,i))
    return selected
def merge(pool,leader,duplicate,primary,pool_mask=None,leader_mask=None,duplicate_mask=None):
    a,b=u(leader,88),u(duplicate,88)
    def entry(row,i):return pool[(row*2+i)*40:(row*2+i+1)*40]
    if not primary and s(entry(b,0),0)<s(entry(a,0),0):
        leader[:],duplicate[:]=duplicate[:],leader[:];a,b=b,a
        if leader_mask is not None:leader_mask[:],duplicate_mask[:]=duplicate_mask[:],leader_mask[:]
    aa,bb=entry(a,0),entry(b,0)
    heap=primary and (aa[31]&bb[31]&32)!=0
    first_a=(s(leader,16) if heap else s(aa,0))<(s(duplicate,16) if heap else s(bb,0)) if primary else True
    chosen=select(pool,a,b,first_a)
    for i in range(len(chosen)-1,-1,-1):
        row,index=chosen[i]
        if row==a and index==i:continue
        # Each read is from the live pool at its descending copy iteration.
        src=entry(row,index);at=(a*2+i)*40
        pool[at:at+8]=src[:8];pool[at+30]=src[30]
        pool[at+24:at+28]=src[24:28];pool[at+8:at+24]=src[8:24]
        mask=63 if primary else 31;pool[at+31]=(pool[at+31]&~mask)|(src[31]&mask)
        if not primary:pool[at+32:at+36]=src[32:36]
        if pool_mask is not None:
            # Transfer masks from the SAME live source position at this exact
            # descending iteration; earlier writes may alias a later source.
            start=(row*2+index)*40;ms=pool_mask[start:start+40]
            pool_mask[at:at+8]=ms[:8];pool_mask[at+30]=ms[30]
            pool_mask[at+24:at+28]=ms[24:28];pool_mask[at+8:at+24]=ms[8:24]
            pool_mask[at+31]=(pool_mask[at+31]&~mask)|(ms[31]&mask)
            if not primary:pool_mask[at+32:at+36]=ms[32:36]
    if not primary:
        best=entry(a,0);leader[16:24]=best[:8];leader[38]=best[30]
        leader[30:32]=best[24:26];leader[32]=best[26]
        leader[33]=(leader[33]&192)|(best[31]&31);leader[62:64]=best[32:34]
        if leader_mask is not None:
            ms=pool_mask[a*80:a*80+40]
            leader_mask[16:24]=ms[:8];leader_mask[38]=ms[30]
            leader_mask[30:32]=ms[24:26];leader_mask[32]=ms[26]
            # Native assignment clears bit5, preserves high2, copies low5.
            leader_mask[33]=(leader_mask[33]&192)|(ms[31]&31)|32
            leader_mask[62:64]=ms[32:34]
def allocate(rows,count,cap,record,row_masks=None,record_mask=None):
    cost=s(record,16)
    if not cap or cost>=IMP:return count
    if count==cap:
        if s(rows[0],16)<=cost:return count
        at=0
        while at*2<count-1:
            child=at*2+1
            if child<count-1 and s(rows[child],16)<=s(rows[child+1],16):child+=1
            if s(rows[child],16)<=cost:break
            rows[at]=bytearray(rows[child])
            if row_masks is not None:row_masks[at]=bytearray(row_masks[child])
            at=child
    else:
        at=count;count+=1
        while at:
            parent=(at-1)//2
            if cost<=s(rows[parent],16):break
            rows[at]=bytearray(rows[parent])
            if row_masks is not None:row_masks[at]=bytearray(row_masks[parent])
            at=parent
    rows[at]=bytearray(record)
    if row_masks is not None:row_masks[at]=bytearray(record_mask)
    return count
def boundary(graph):
    # 152400 param3=0: graphV30(1)=min(2, root cap8), not all graph arcs.
    cap=8;seen=[]
    for node in range(1,1+min(2,cap)):
        if node>=len(graph):break
        for arc in graph[node]:
            if node-arc['span']-1==0:
                seen.append([node,arc['ordinal'],arc['span'],arc['flags']])
                if arc['flags']&4:return True,seen
    return False,seen
def pools_from_case(case):
    rows=[bytearray.fromhex(r)+bytearray(8) for r in case['records']]
    assert len(rows)==8 and all(len(r)==128 for r in rows)
    # Capture grafts normalize IDs to zero-based slots; actual heap U64 context
    # identities are slot+1 (zero is the genuine null sentinel).
    roots={g[2]+1:g[1]+1 for g in case['input']['grafts']}
    primary=bytearray(H*80);secondary=bytearray(H*80)
    for i,row in enumerate(rows):
        put(row,88,i);suffix=u(row,8,8);assert suffix in roots
        for pool,p in ((primary,True),(secondary,False)):
            at=i*80;pool[at:at+8]=row[16:24]
            put(pool,at+8,roots[suffix],8);put(pool,at+16,suffix,8)
            pool[at+24:at+26]=row[30:32];pool[at+26]=row[32];pool[at+30]=row[38]
            pool[at+31]=(row[33]&31)|(32 if p else 0)
            put(pool,at+40,IMP);put(pool,at+44,IMP);pool[at+70]=0
    rows.extend(bytearray(128) for _ in range(H-len(rows)))
    return rows,primary,secondary
def root_pool_mask(active,primary):
    mask=bytearray(H*80)
    for i in range(active):
        at=i*80;mask[at:at+27]=bytes([255])*27;mask[at+30]=255;mask[at+31]=63 if primary else 31
        mask[at+40:at+48]=bytes([255])*8;mask[at+70]=255
    return mask
def phase0(rows,active,primary,secondary,row_masks=None,primary_mask=None,secondary_mask=None):
    rows=[bytearray(r) for r in rows];primary=bytearray(primary);secondary=bytearray(secondary)
    row_masks=[bytearray(m) for m in row_masks] if row_masks is not None else [bytearray([255]*120+[0]*8) if i<active else bytearray(128) for i in range(H)]
    primary_mask=bytearray(primary_mask) if primary_mask is not None else root_pool_mask(active,True)
    secondary_mask=bytearray(secondary_mask) if secondary_mask is not None else root_pool_mask(active,False)
    tmp=rows[:active];tm=row_masks[:active];quicksort(tmp,0,active-1,tm);rows[:active]=tmp;row_masks[:active]=tm;unique=1 if active else 0
    for i in range(1,active):
        if key(rows[unique-1])==key(rows[i]):
            merge(secondary,rows[unique-1],rows[i],False,secondary_mask,row_masks[unique-1],row_masks[i])
            merge(primary,rows[unique-1],rows[i],True,primary_mask,row_masks[unique-1],row_masks[i])
        else:
            if unique!=i:rows[unique]=bytearray(rows[i]);row_masks[unique]=bytearray(row_masks[i])
            unique+=1
    count=0
    for i in range(unique):count=allocate(rows,count,H,bytes(rows[i]),row_masks,bytes(row_masks[i]))
    old=count
    if count>L:
        count=L
        for i in range(L,old):count=allocate(rows,count,L,bytes(rows[i]),row_masks,bytes(row_masks[i]))
    # Full owned logical arrays plus definedness; unspecified native bytes are
    # zeros here only as library allocation values, never native-byte evidence.
    stored=bytearray(E*88);sp=bytearray(L*80);ss=bytearray(E*80)
    rm=bytearray(E*88);pm=bytearray(L*80);sm=bytearray(E*80)
    for i in range(L):
        put(stored,i*88+16,IMP);put(stored,i*88+20,IMP)
        rm[i*88+16:i*88+24]=bytes([255])*8
        # 2144F0 initializes ONLY the first alternative's two costs, in both
        # primary and secondary node storage. The second remains residue.
        for backing,mask in ((sp,pm),(ss,sm)):
            put(backing,i*80,IMP);put(backing,i*80+4,IMP)
            mask[i*80:i*80+8]=bytes([255])*8
    for i in range(count):
        row=rows[i];dest=i*88;stored[dest:dest+88]=row[:88];rm[dest:dest+88]=row_masks[i][:88]
        if u(stored,dest+60,2)==65535:put(stored,dest+60,i,2);rm[dest+60:dest+62]=b'\xff\xff'
        ordinal=u(row,88);assert ordinal<H
        sp[i*80:(i+1)*80]=primary[ordinal*80:(ordinal+1)*80]
        ss[i*80:(i+1)*80]=secondary[ordinal*80:(ordinal+1)*80]
        # Writer copied whole pool records, but only pool fields originally
        # written by20E6A0 or duplicate merge carry native definedness.
        pm[i*80:(i+1)*80]=primary_mask[ordinal*80:(ordinal+1)*80]
        sm[i*80:(i+1)*80]=secondary_mask[ordinal*80:(ordinal+1)*80]
    # Common tail after phase0: 125 rows' 3E always FFFF, phase1 descriptor
    # count0/cap124 WITHOUT a language call, both ends125 (NOT count).
    for i in range(L):put(stored,i*88+62,65535,2);rm[i*88+62:i*88+64]=b'\xff\xff'
    cleanup=0
    for i in range(L*2):
        at=i*40
        cost_known=pm[at:at+4]==bytes([255])*4
        flag_known=bool(pm[at+31]&32)
        parent_known=pm[at+24:at+26]==b'\xff\xff'
        definitely_skipped=(cost_known and s(sp,at)>=IMP) or (flag_known and bool(sp[at+31]&32)) or (parent_known and u(sp,at+24,2)<L)
        predicate_known=cost_known and flag_known and parent_known
        if s(sp,at)<IMP and not(sp[at+31]&32) and u(sp,at+24,2)>=L:
            put(sp,at,IMP);cleanup+=1
            if predicate_known:pm[at:at+4]=bytes([255])*4
        # Unknown allocator residue may change the branch. The chosen portable
        # zero-owned value is deterministic; do not promote its possible write
        # to native certainty without fully known operands or a short circuit.
        if not(predicate_known or definitely_skipped):pm[at:at+4]=bytes(4)
    return {'active':active,'unique':unique,'retained':count,'sourceEnd':125,'retainedEnd':125,
      'phase1Count':0,'phase1Capacity':124,'allocatedHeapCapacity':250,'cleanup':cleanup,
      'cumulative2520':0,'backward2528':0,'periodicPublication':False,'nextNode':1,
      'sourceResetNode':0,'stored':stored.hex(),'primary':sp.hex(),'secondary':ss.hex(),
      'storedDefined':rm.hex(),'primaryDefined':pm.hex(),'secondaryDefined':sm.hex()}
def actual(capture,observation):
    out=[]
    for case,graph in zip(capture['cases'],observation['graphs']):
        gate,seen=boundary(graph)
        if gate or len(graph)==1:
            out.append({'index':case['index'],'admitted':False,'reason':'native-language-branch-required','boundaryReads':seen});continue
        rows,p,q=pools_from_case(case)
        result=phase0(rows,case['count'],p,q)
        assert result['retained']==8 and result['unique']==8 and not result['cleanup']
        assert all(s(bytes.fromhex(result['stored']),i*88+52)==0 for i in range(8))
        out.append({'index':case['index'],'admitted':True,'boundaryReads':seen,'result':result})
    return out
def prepared_inputs(n,duplicate):
    rows=[bytearray(128) for _ in range(H)];p=bytearray(H*80);q=bytearray(H*80)
    for i in range(n):
        group=i//3 if duplicate else i;row=rows[i]
        put(row,0,group+1,8);put(row,36,group%192,2);put(row,16,1000-i);put(row,20,-i)
        put(row,60,65535,2);put(row,62,65535,2);put(row,88,i)
        for pool,primary in ((p,True),(q,False)):
            for j in range(2):
                at=i*80+j*40;put(pool,at,1000-i+j*5);put(pool,at+4,-i-j)
                put(pool,at+8,i*2+j,8);put(pool,at+16,i*2+j+1000,8)
                pool[at+31]=(32 if primary else 0);put(pool,at+32,65535,2)
    return rows,p,q
def prepared():
    controls=[]
    for n in (0,1,125,126,250):
        for duplicate in (False,True):
            rows,p,q=prepared_inputs(n,duplicate)
            # Explicit caller-prepared input bytes/masks, not native allocator
            # residue or genuine consumer authority.
            r=phase0(rows,n,p,q,[bytes([255])*128 for _ in rows],bytes([255])*len(p),bytes([255])*len(q))
            assert r['retained']<=125 and r['phase1Count']==0 and r['sourceEnd']==125
            controls.append({'name':'prepared-%d-%s'%(n,'duplicate' if duplicate else 'unique'),
                'sha256':hashlib.sha256(json.dumps(r,sort_keys=True).encode()).hexdigest(),
                'unique':r['unique'],'retained':r['retained']})
    return controls
def mask_controls():
    """PREPARED literal copy witnesses; no source/consumer authority."""
    out=[]
    for primary in (False,True):
        for suffix_mode in ('native-unknown','prepared-partial','prepared-known'):
            p=bytearray(H*80);pm=root_pool_mask(2,primary)
            a=bytearray(128);b=bytearray(128);am=bytearray([255]*120+[0]*8);bm=bytearray(am)
            put(a,88,0);put(b,88,1);put(a,16,10);put(b,16,20)
            for i in range(2):
                at=i*80;put(p,at,10+i*10);put(p,at+4,2+i)
                put(p,at+8,100+i,8);put(p,at+16,200+i,8)
                p[at+31]=32 if primary else 0;put(p,at+40,IMP);put(p,at+44,IMP)
                p[at+32:at+36]=b'\x11\x22\x33\x44'
            # The destination was unavailable before the copy. Its opaque bits
            # carry explicit PREPARED knowledge to test preservation/replacement.
            pm[40+31]=128 if primary else 160
            pm[40+32:40+36]=bytes([255])*4
            source_suffix={'native-unknown':bytes(4),'prepared-partial':b'\xf0\x0f\xaa\x55','prepared-known':bytes([255])*4}[suffix_mode]
            pm[80+32:80+36]=source_suffix
            source_flags=37 if primary else 17;pm[80+31]=source_flags
            before=bytes(pm[40:80]);merge(p,a,b,primary,pm,am,bm)
            after=bytes(pm[40:80])
            assert after[8:27]==bytes([255])*19 and before[8:27]==bytes(19)
            assert after[31]==((128 if primary else 160)|source_flags)
            if primary:
                assert after[32:36]==bytes([255])*4 #primary never writes20..23
            else:
                assert after[32:36]==source_suffix #replace, not OR with destination
                assert am[62:64]==bytes(2) #best FIRST32..33 native unknown
            out.append(dict(name=('primary' if primary else 'secondary')+'-FIRST-to-SECOND-'+suffix_mode,
                before=before.hex(),after=after.hex(),leaderDefined=am.hex(),
                primaryNoSuffixWrite=primary,sourceSuffixDefined=source_suffix.hex()))
    # A secondary winner swap must move whole heap masks before any narrow
    # leader-field writes. Tail78..7F distinguishes the two prepared masks.
    p=bytearray(H*80);pm=root_pool_mask(2,False);a=bytearray(128);b=bytearray(128)
    am=bytearray([255]*120+[0]*8);bm=bytearray([255]*128)
    put(a,88,0);put(b,88,1);put(p,0,20);put(p,80,10)
    put(p,8,1,8);put(p,88,2,8);put(p,40,IMP);put(p,120,IMP)
    merge(p,a,b,False,pm,am,bm)
    assert u(a,88)==1 and am[120:128]==bytes([255])*8 and bm[120:128]==bytes(8)
    out.append(dict(name='secondary-full-slot-swap-before-narrow-writes',leaderOrdinal=u(a,88),
        leaderTail=am[120:128].hex(),duplicateTail=bm[120:128].hex()))
    # Heap movements and eviction copy masks with values, not with destination
    # positions. A rejected incoming row must not replace a retained mask.
    rows=[bytearray(128) for _ in range(H)];masks=[bytearray(128) for _ in range(H)];n=0
    for cost,mark in [(10,1),(20,2),(5,3)]:
        r=bytearray(128);put(r,16,cost);rm=bytes([mark])*128
        n=allocate(rows,n,2,r,masks,rm)
    assert sorted(s(x,16) for x in rows[:n])==[5,10]
    assert sorted(x[0] for x in masks[:n])==[1,3]
    prior=[bytes(x) for x in masks];r=bytearray(128);put(r,16,50)
    assert allocate(rows,n,2,r,masks,bytes([4])*128)==n and prior==[bytes(x) for x in masks]
    out.append(dict(name='heap-move-evict-and-reject-provenance',costs=[s(x,16) for x in rows[:n]],
        masks=[x[0] for x in masks[:n]]))
    rows=[];masks=[]
    for value in (30,10,20):
        r=bytearray(128);put(r,0,value,8);rows.append(r);masks.append(bytearray([value])*128)
    quicksort(rows,0,2,masks)
    assert [u(r,0,8) for r in rows]==[10,20,30] and [r[0] for r in masks]==[10,20,30]
    out.append(dict(name='quicksort-full-row-mask-permutation',values=[u(r,0,8) for r in rows],masks=[r[0] for r in masks]))
    rows,p,q=prepared_inputs(3,False);rows[1][0:8]=rows[0][0:8];rows[1][36:38]=rows[0][36:38]
    masks=[bytearray([255])*128 for _ in rows]
    for i,mask in enumerate((240,15,170)):masks[i][67]=mask
    r=phase0(rows,3,p,q,masks,bytes([255])*len(p),bytes([255])*len(q))
    b=bytes.fromhex(r['stored']);bm=bytes.fromhex(r['storedDefined']);assert r['retained']==2
    correlated=[]
    for i in range(2):
        tag=u(b,i*88,8);expected=15 if tag==1 else 170
        assert tag in (1,3) and bm[i*88+67]==expected;correlated.append([tag,bm[i*88+67]])
    out.append(dict(name='secondary-winner-unique-compaction-materialization-masks',rows=correlated))
    return out
if __name__=='__main__':
    import argparse
    p=argparse.ArgumentParser();p.add_argument('consumer');p.add_argument('observation');p.add_argument('output');a=p.parse_args()
    c=json.load(open(a.consumer));o=json.load(open(a.observation))
    result={'scope':'STATIC historical native reference; no live query/parent/recognition authority',
      'cases':actual(c,o),'prepared':prepared(),'maskControls':mask_controls(),
      'logicalEndSourceReset':'internal library state adaptation; no discarded Engine/live query authority'}
    from pathlib import Path
    Path(a.output).write_text(json.dumps(result,separators=(',',':'))+'\n',encoding='utf-8')
    print(json.dumps({'status':'STATIC_REFERENCE_PASS','actualAdmitted':sum(x['admitted'] for x in result['cases']),
       'preclaimRefusals':sum(not x['admitted'] for x in result['cases']),'prepared':len(result['prepared']),
       'outputSHA256':hashlib.sha256(Path(a.output).read_bytes()).hexdigest()}))
