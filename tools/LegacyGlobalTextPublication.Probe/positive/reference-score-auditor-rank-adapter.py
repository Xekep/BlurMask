"""DATA-only 140B90 reconstruction and observed rank-map application.
No resources, weights, classifier, vendor library or target are imported.
Rank equality/decision semantics and augmentation/pool writes are EXCLUDED.
"""
import argparse,base64,copy,hashlib,json,math,struct
from decimal import Decimal,localcontext
from pathlib import Path

def require(ok,message):
    if not ok:raise AssertionError(message)
def integer(x,lo,hi,name):
    require(type(x) is int and lo<=x<=hi,name);return x
def vector(a,lo,hi,name,maximum=65536):
    require(type(a) is list and len(a)<=maximum,name)
    return [integer(v,lo,hi,name) for v in a]
def blob(s,n,name):
    require(type(s) is str,name)
    try:b=base64.b64decode(s,validate=True)
    except Exception as e:raise AssertionError(name) from e
    require(len(b)==n and base64.b64encode(b).decode()==s,name);return b
def f32(x):
    try:return struct.unpack('<f',struct.pack('<f',x))[0]
    except OverflowError:return math.copysign(math.inf,x)
def bits(x):return struct.unpack('<I',struct.pack('<f',x))[0]
def read(b,p):return struct.unpack_from('<f',b,p)[0]
def exp32(x):
    # Independent high-precision exponential, then binary32 rounding. The4ULP
    # policy is inherited from prepared SpanScores and is NOT a native CRT proof.
    if x==-math.inf or x< -120:return 0.0
    require(math.isfinite(x) and x<=75,'exp input domain')
    with localcontext() as c:
        c.prec=100
        v=Decimal.from_float(x).exp()
    return f32(float(v))
def native_order(words):
    order=[]
    # Native140B90 binary insertion breaks at the midpoint on equality.
    for index,w in enumerate(words):
        low=0;high=index;mid=index//2
        while low<high:
            current=read(w,16);old=read(words[order[mid]],16)
            if current<=old:
                if old<=current:break
                low=mid+1
            else:high=mid
            mid=(low+high)//2
        order.insert(mid,index)
    return order
def normalize(words):
    require(1<=len(words)<=4095,'normalize word count')
    for w in words:require(len(w)==120 and math.isfinite(read(w,16)) and math.isfinite(read(w,28)),'finite normalize input')
    order=native_order(words);out=[bytearray(w) for w in words]
    mass=[];top=read(words[order[0]],16)
    for i in order:
        difference=f32(read(words[i],16)-top)
        mass.append(0.0 if difference< -75 else f32(3.73e32) if difference>75 else exp32(difference))
    if len(words)==1:
        struct.pack_into('<ff',out[0],16,1.0,1.0);return [bytes(out[0])],order
    values=[0.0]*len(words);previous=mass[0]
    if previous>0:
        values[0]=f32(previous/f32(previous+mass[1]))
        for k in range(1,len(words)):
            if mass[k]==0:break
            values[k]=f32(f32(mass[k]*values[k-1])/previous)
            previous=mass[k]
    minimum=struct.unpack('<f',bytes.fromhex('ffff7f7f'))[0]
    for i in order:
        value=read(words[i],28)
        if value<=minimum:minimum=value
    for k,i in enumerate(order):
        difference=f32(read(words[i],28)-minimum)
        struct.pack_into('<f',out[i],16,values[k])
        struct.pack_into('<f',out[i],20,exp32(-difference))
    return list(map(bytes,out)),order

def phase(p,name):
    require(type(p) is dict and p.get('Phase')==name,'phase order/name')
    first=integer(p['First'],-2147483648,2147483647,'span first');last=integer(p['Last'],-2147483648,2147483647,'span last')
    quota=integer(p['CarriedQuota'],0,2147483647,'carried quota')
    raw=p['Words120'];require(type(raw) is list and 1<=len(raw)<=4095,'word width');words=[blob(x,120,'word120') for x in raw]
    refs=p['FlatReferences'];require(type(refs) is list and len(refs)==len(words),'reference rows')
    refs=[vector(x,0,2**64-1,'flat reference',65536) for x in refs];require(sum(map(len,refs))<=65536,'reference total')
    selected=vector(p['SelectedOrdinals'],0,len(words)-1,'selected ordinal',4096);require(bool(selected),'nonempty selected')
    channels=vector(p['FourChannelBits'],0,2**32-1,'U32 channels',4*4095)
    require(channels==[struct.unpack_from('<I',w,16+4*j)[0] for w in words for j in range(4)],'channels match word bytes')
    return words,refs,selected,(first,last,quota)

def audit(trace,plain):
    require(type(trace) is dict and type(plain) is dict,'capture roots')
    cases=trace['samples'];require(type(cases) is list and len(cases)==6,'six records')
    expected_keys=[(s,c) for s in range(3) for c in range(2)];keys=[];stripped=copy.deepcopy(trace)
    total=0;maxulp=0;lastcharge=[0,0,0]
    for ordinal,c in enumerate(cases):
        require(type(c) is dict,'case')
        keys.append((integer(c['sample'],0,2,'sample'),integer(c['chosen'],0,1,'chosen')))
        t=c.get('scoreTrace');require(type(t) is dict,'nonnull score trace')
        require(t['Scope']=='SCORE_ONLY_NOT_AUGMENTATION_POOL_REPLAY' and t['Truncated'] is False,'scope/truncation')
        require(t['ParentObserved'] is False and type(t['ParentFrontier']) is str and t['ParentFrontier'].startswith('NotObserved:'),'parent frontier')
        phases=t['Phases'];require(type(phases) is list and len(phases)==3,'three phases')
        pre,post,done=phases
        a,ar,aselect,key=phase(pre,'PreRank');b,br,bselect,key2=phase(post,'PostRankPreNormalize');out,orefs,oselect,key3=phase(done,'PostNormalizeApplied')
        require(key==key2==key3 and key[:2]==(c['first'],c['last']),'same span/quota')
        n=len(a);m=len(b)
        sources=vector(pre['OutputSourceOrdinals'],0,n-1,'payload source',4095)
        mapping=vector(pre['OldToNew'],-2147483648,2147483647,'old-to-new',4095)
        released=vector(pre['Released'],0,n-1,'released',4095)
        released_sources=vector(pre['ReleasedSourceOrdinals'],0,n-1,'released payload source',4095)
        require(len(sources)==m and len(set(sources))==m and len(mapping)==n,'map extents/unique sources')
        require(len(set(released))==len(released)==n-m,'released original count')
        require(len(set(released_sources))==len(released_sources)==n-m,'released source count')
        require(set(sources).isdisjoint(released_sources) and set(sources)|set(released_sources)==set(range(n)),'payload source partition')
        # Source935 uses specialMode=false. Actual payload-source mapping must
        # preserve ALL120 bytes, not permit an arbitrary sentinel/weight rewrite.
        require(b==[a[i] for i in sources] and br==[ar[i] for i in sources],'rank map application/refs')
        require(bselect==[mapping[i] for i in aselect],'selected ordinal rebind')
        rows=pre['RankingByOriginalIndex20'];require(type(rows) is list and len(rows)==n,'ranking rows')
        rows=[blob(r,20,'ranking20') for r in rows]
        for i,r in enumerate(rows):
            require(struct.unpack_from('<i',r)[0]==i and struct.unpack_from('<i',r,4)[0]==mapping[i],'ranking metadata map')
            require(all(r[j] in (0,1) for j in range(12,18)) and r[18:]==b'\0\0','ranking metadata booleans')
            require(r[14]==0,'source935 specialMode=false')
            require(r[13]==int(i in aselect),'original selected-byte metadata; First is independently checked by full20 rank replay')
        require(released==[i for i,r in enumerate(rows) if r[17]],'release metadata')
        q=integer(pre['QuotaAfter'],0,2147483647,'quota after')
        require(q==key[2]+sum(r[15] for r in rows) and q==c['quota'],'observed flags/quota application')
        require(pre['ScoreOrder']==[],'pre rank no normalize order')
        for p in (post,done):
            require(p['QuotaAfter'] is None and all(p[k]==[] for k in ['OldToNew','OutputSourceOrdinals','Released','ReleasedSourceOrdinals','RankingByOriginalIndex20']),'phase metadata separation')
        require(post['ScoreOrder']==[],'postrank no normalize order')
        expected,order=normalize(b)
        require(vector(done['ScoreOrder'],0,m-1,'score order',4095)==order,'native binary insertion order')
        require(len(out)==m and orefs==br and oselect==bselect,'normalize preserves refs/selection')
        for actual,want,before in zip(out,expected,b):
            require(actual[:16]==before[:16] and actual[24:]==before[24:],'normalize opaque bytes')
            for pos in (16,20):
                value=read(actual,pos);require(math.isfinite(value) and value>=0,'normalized domain')
                distance=abs(struct.unpack_from('<I',actual,pos)[0]-struct.unpack_from('<I',want,pos)[0])
                require(distance<=4,'140B90 exponential channels >4ULP');maxulp=max(maxulp,distance)
            total+=1
        receipts=t['Selected'];require(type(receipts) is list and len(receipts)==1,'one final selected receipt; parent not synthesized')
        receipt=receipts[0];require(receipt['Phase']=='FinalSelected275','final receipt phase')
        selected=integer(receipt['SelectedOrdinal'],0,m-1,'final selected')
        require(selected==oselect[0]==c['selected'],'selected final join')
        final=blob(receipt['Word120'],120,'final120')
        require(final==out[selected]==blob(c['after120'],120,'case after120'),'final row exact join')
        require(vector(receipt['FlatReferences'],0,2**64-1,'final refs',4096)==orefs[selected],'final reference join')
        charge=[integer(t[k],0,limit,k) for k,limit in [('PayloadCharge',8*2**20),('CopyCharge',16*2**20),('ReadCharge',32*2**20)]]
        require(all(now>=old for now,old in zip(charge,lastcharge)),'shared cumulative ledger monotonic');lastcharge=charge
        stripped['samples'][ordinal].pop('scoreTrace')
    require(keys==expected_keys,'exact ordered unique sample/chosen keys')
    # JSON canonical comparison distinguishes true from1 and rejects NaN.
    canon=lambda x:json.dumps(x,sort_keys=True,separators=(',',':'),allow_nan=False)
    require(canon(stripped)==canon(plain),'paired all existing fields exact')
    return {'Status':'PASS_DATA_ONLY_140B90_AND_OBSERVED_MAP_APPLICATION','Cases':6,'NormalizedRows':total,'MaxExponentialULP':maxulp,'RankEqualityIndependent':False,'RankDecisionIndependent':False,'AugmentationPoolReplay':False,'ParentObserved':False,'NativeCRTBitIdentity':False,'FinalCharges':lastcharge}

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('trace',type=Path);p.add_argument('no_trace',type=Path);p.add_argument('fresh_report',type=Path);a=p.parse_args()
    require(not a.fresh_report.exists(),'fresh output required')
    require(0<a.trace.stat().st_size<=64*2**20 and 0<a.no_trace.stat().st_size<=64*2**20,'bounded capture files')
    tb=a.trace.read_bytes();pb=a.no_trace.read_bytes()
    t=json.loads(tb);plain=json.loads(pb);report=audit(t,plain)
    report.update(TraceSHA256=hashlib.sha256(tb).hexdigest(),NoTraceSHA256=hashlib.sha256(pb).hexdigest())
    with a.fresh_report.open('x',encoding='utf8') as f:f.write(json.dumps(report,indent=2)+'\n')
    print(json.dumps(report))
if __name__=='__main__':main()
