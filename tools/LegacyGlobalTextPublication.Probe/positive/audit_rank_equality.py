"""Independent native13D360/13F970 captured-input replay, ROOT DATA only.
Equality decisions are derived from actual raw40 read events, never trusted.
No augmentation, NN, calibration or whole-native-allocator claim.
"""
import argparse,base64,copy,hashlib,importlib.util,json,struct
from pathlib import Path
D=Path(__file__).resolve().parent
SCORE_SHA="00e1fc4ca401ed56afdd29313f74a3d4601e8427142cef9ad273fab396212b02"
def score_module():
 p=D/'reference-score-auditor-rank-adapter.py'
 assert hashlib.sha256(p.read_bytes()).hexdigest()==SCORE_SHA
 s=importlib.util.spec_from_file_location('score_reference',p);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
require=lambda ok,msg: None if ok else (_ for _ in ()).throw(AssertionError(msg))
u16=lambda b,p:struct.unpack_from('<H',b,p)[0]
u64=lambda b,p:struct.unpack_from('<Q',b,p)[0]
geti=lambda b,p:struct.unpack_from('<i',b,p)[0]
geth=u16
getf=lambda b,p:struct.unpack_from('<f',b,p)[0]
end=2**64-1
dec=base64.b64decode
sentinel=struct.unpack('<f',bytes.fromhex('ffff7fff'))[0]
def puti(b,p,v):struct.pack_into('<i',b,p,v)
def putf(b,p,v):struct.pack_into('<f',b,p,v)
def sort(rows,lo,hi,offset,descending):
    if hi<=lo:return
    at=getf if descending else geti
    pivot=at(rows[(hi-lo)//2+lo],offset);i,j=lo,hi
    while i<=j:
        while (pivot<at(rows[i],offset)) if descending else (at(rows[i],offset)<pivot):i+=1
        while (at(rows[j],offset)<pivot) if descending else (pivot<at(rows[j],offset)):j-=1
        if j<i:break
        rows[i],rows[j]=rows[j],rows[i];i+=1;j-=1
    sort(rows,lo,j,offset,descending);sort(rows,i,hi,offset,descending)

def rank(words_in,selected,quota,same_pool):
    # Reused independently validated prepared13D360 algorithm; only its
    # fabricated identity comparator is replaced by raw native linked equality.
    case={'words':[base64.b64encode(w).decode() for w in words_in],'selected':selected,'quota':quota,'special':False}
    swaps=ties=duplicates=category_selected_discriminators=0
    words=list(map(lambda x:bytearray(dec(x)),case['words']));n=len(words);r=[bytearray(20)for _ in words];sources=list(range(n))
    for i in range(n):
        puti(r[i],0,i);puti(r[i],4,-2147483647);r[i][8:12]=words[i][16:20]
        r[i][14]=int(case['special']and geth(words[i],50)&64!=0 and geth(words[i],48)==65534)
    for pos,ordinal in enumerate(case['selected']):
        r[ordinal][13]=1
        if pos==0:r[ordinal][12]=1
    group=999
    for i in range(n):
        if r[i][17]:continue
        group+=1;puti(r[i],4,group);r[i][15]=int(geth(words[i],50)&768!=0)
        others=[]
        for j in range(i+1,n):
            if not r[j][17]and geti(words[i],60)==geti(words[j],60)and same_pool(bytes(words[i]),bytes(words[j]))and r[i][14]==r[j][14]:
                puti(r[j],4,group);others.append(j)
        best=i;cost=getf(r[i],8);selected=r[i][13];first=r[i][12]
        for j in others:
            selected|=r[j][13];first|=r[j][12]
            if cost==getf(r[j],8):ties+=1
            if cost<getf(r[j],8):best=j;cost=getf(r[j],8)
        if best!=i:
            words[i],words[best]=words[best],words[i];sources[i],sources[best]=sources[best],sources[i];putf(r[i],8,cost);swaps+=1
        r[i][12]=first
        for j in others:r[j][17]=1;putf(r[j],8,sentinel);duplicates+=1
        r[i][16]=int(bool(selected or r[i][15]))
    if case['special']:
        for row in r:
            if row[14]:row[16]=1;break
    protected=sum(bool(row[16])and not row[17]for row in r)
    alive=n-sum(bool(row[17])for row in r)
    flag_count=sum(bool(row[15])for row in r);selected_count=sum(bool(row[13])for row in r)
    if flag_count!=selected_count:category_selected_discriminators+=1
    quota=case['quota']+flag_count
    if quota<alive:
        prefix=0
        for i in range(n):
            if r[i][16]:
                r[i],r[prefix]=r[prefix],r[i];prefix+=1
                if prefix>=protected:break
        sort(r,protected,n-1,8,True);alive=protected
        for i in range(protected,n):
            if not r[i][17]:
                if alive>=quota:r[i][17]=1;putf(r[i],8,sentinel)
                else:alive+=1
    selected_first=next(i for i,row in enumerate(r)if row[12]);r[0],r[selected_first]=r[selected_first],r[0]
    first=geti(r[0],4)
    for row in r:
        if geti(row,4)==first:puti(row,4,0)
    sort(r,1,n-1,8,True)
    if case['special']:
        #Independent stable partition of the finite live tail, after checking
        #native bubble13CE00 only exchanges special/ordinary inversions.
        end=2
        while end<n and getf(r[end],8)!=sentinel:end+=1
        r[1:end]=[row for row in r[1:end]if not row[14]]+[row for row in r[1:end]if row[14]]
    for i in range(1,n):
        if getf(r[i],8)==sentinel:break
        old=geti(r[i],4)
        for row in r:
            if geti(row,4)==old:puti(row,4,i)
    output=[];output_sources=[]
    for row in r:
        if geti(row,4)==-1 or getf(row,8)==sentinel:break
        word=words[geti(row,0)].copy()
        if row[14]:putf(word,16,sentinel)
        output.append(bytes(word));output_sources.append(sources[geti(row,0)])
    assert len(output)==alive

    r.sort(key=lambda row:geti(row,0))
    return {'Words':output,'Rows':list(map(bytes,r)),'Map':[geti(row,4)for row in r],
        'Sources':output_sources,'Released':[geti(row,0)for row in r if row[17]],
        'ReleasedSources':[sources[geti(row,0)]for row in r if row[17]],'Quota':quota,
        'Duplicates':duplicates,'Swaps':swaps,'Ties':ties}

def alphabet(data):
    # Independent raw AK AL parser, reused source-only from RussianGlyphSymbol.
    at=99
    def read(fmt):
        nonlocal at
        value=struct.unpack_from('<'+fmt,data,at)[0];at+=struct.calcsize('<'+fmt);return value
    header=[read('H')for _ in range(8)];count=header[-1];n=read('H')
    for inc in (32,64,1):
        values=[read('H')for _ in range(n)];n=next(v for v in reversed(values)if v!=65535)+inc
    spans=[read('H')for _ in range(n+1)];pairs=[(read('I'),read('I')&65535)for _ in range(spans[-1]+1)]
    strings=data[at:at+pairs[-1][0]]
    labels={pairs[i][1]:strings[pairs[i][0]:pairs[i+1][0]].decode('utf8')for i in range(len(pairs)-1)}
    require(count==192 and len(labels)==189,'raw AK AL scope')
    return labels

def scalar(row,scratch):
    kind=u16(row,4)&7;label=u16(row,8)
    if kind==3:
        if label&0x7ffc==0:return
        require(label&3==1,'UNSUPPORTED: native control lookup is null in current source')
        scratch[2]=0;return
    if kind==1:label=((label&3)-7)&65535
    elif kind not in (0,2,4):label=65535
    struct.pack_into('<H',scratch,0,label);scratch[2]=int(kind==4)

def equality(left_word,right_word,ls,rs,ignored,read):
    if(u16(left_word,50)^u16(right_word,50))&0x300:return False
    left=u64(left_word,8);right=u64(right_word,8)
    # Native nested skip read chronology (13FB10..13FB89), not a reordered
    # prefiltered symbol stream. This makes every required physical read checkable.
    while True:
        if left==end and right==end:return True
        skipped=False;a=b=None
        while True:
            if left!=end:
                a=read(left)
                if a[6]&2:left=u64(a,32);skipped=True
            if right!=end:
                b=read(right)
                if b[6]&2:right=u64(b,32);continue
            break
        if skipped:continue
        if left==end:return right==end
        if right==end:return False
        scalar(a,ls);scalar(b,rs)
        al=u16(ls,0);bl=u16(rs,0) # private2360 identity mapper
        if al==ignored:left=u64(a,32)
        elif bl==ignored:right=u64(b,32)
        else:
            if al!=bl or ls[2]!=rs[2]:return False
            left=u64(a,32);right=u64(b,32)

def replay_case(case,ignored,m):
    t=case['scoreTrace'];require(t.get('EqualityTruncated') is False,'complete equality journal')
    calls=t['Equalities'];require(type(calls) is list and len(calls)<=256,'bounded equality calls')
    pre,post,done=t['Phases'];words=[m.blob(s,120,'rank input')for s in pre['Words120']]
    for w in words:require(m.math.isfinite(getf(w,16)) and getf(w,16)!=sentinel,'rank unsentinelized finite scope')
    call_index=0;read_count=0;physical={};last_scratch=None
    def compare(a,b):
        nonlocal call_index,read_count,last_scratch
        require(call_index<len(calls),'UNOBSERVED: independent rank needs missing comparison')
        c=calls[call_index];require(type(c) is dict and c['Ordinal']==call_index and type(c['Ordinal'])is int,'comparison ordinal')
        call_index+=1
        require(c['Applied'] is True and type(c['ObservedEqual'])is bool,'completed comparison')
        require(c['Mapper']=='Identity2360' and c['ControlLookup']=='Null','closed source equality scope')
        require(m.integer(c['IgnoredPrimaryLabel'],0,65535,'ignored label')==ignored,'raw AL ignored joiner binding')
        require(m.blob(c['LeftWord120'],120,'left input')==a and m.blob(c['RightWord120'],120,'right input')==b,'independent rank comparison call/word order')
        ls=bytearray(m.blob(c['LeftBefore4'],4,'left scratch'));rs=bytearray(m.blob(c['RightBefore4'],4,'right scratch'))
        if last_scratch is not None:require((bytes(ls),bytes(rs))==last_scratch,'same rank scratch continuity')
        events=c['ReadEvents'];require(type(events) is list and len(events)<=8192,'read events');at=0
        def read(ref):
            nonlocal at,read_count
            require(at<len(events),'UNOBSERVED: missing physical row read')
            e=events[at];at+=1;read_count+=1
            actual=m.integer(e['Reference'],0,2**64-1,'physical reference');require(actual==ref,'native physical read order/reference')
            require(actual>>57==0 and ((actual>>40)&65535)<2040,'native physical owner/ordinal domain')
            row=m.blob(e['RawRecord40'],40,'actual pool record')
            if actual in physical:require(physical[actual]==row,'rank pool immutable across reads')
            else:physical[actual]=row
            return row
        expected=equality(a,b,ls,rs,ignored,read)
        require(at==len(events),'no unexplained pool reads')
        require(expected is c['ObservedEqual'],'independent13F970 result')
        last_scratch=(bytes(ls),bytes(rs))
        require(bytes(ls)==m.blob(c['LeftAfter4'],4,'left after') and bytes(rs)==m.blob(c['RightAfter4'],4,'right after'),'independent scalar scratch after')
        return expected
    result=rank(words,pre['SelectedOrdinals'],pre['CarriedQuota'],compare)
    require(call_index==len(calls),'no unexplained comparison calls')
    require(result['Words']==[m.blob(x,120,'rank output')for x in post['Words120']],'independent13D360 full120 output')
    require(result['Rows']==[m.blob(x,20,'ranking rows')for x in pre['RankingByOriginalIndex20']],'independent13D360 full20 ranking rows')
    for k,target in [('Map','OldToNew'),('Sources','OutputSourceOrdinals'),('Released','Released'),('ReleasedSources','ReleasedSourceOrdinals'),('Quota','QuotaAfter')]:require(result[k]==pre[target],'independent13D360 '+target)
    require(read_count<=8192,'case read cap')
    return {'Comparisons':call_index,'ReadEvents':read_count,'UniquePhysicalRows':len(physical),'Duplicates':result['Duplicates'],'Swaps':result['Swaps'],'Ties':result['Ties']}

def audit(trace,plain,ignored):
    m=score_module();base=m.audit(trace,plain);cases=[replay_case(c,ignored,m)for c in trace['samples']]
    require(sum(c['Comparisons']for c in cases)>0,'positive comparison coverage')
    return {'Status':'PASS_INDEPENDENT13D360_13F970_AND140B90_DATA','ScoreAudit':base,'Cases':cases,'IgnoredPrimaryLabel':ignored,'RankDecisionIndependent':True,'EqualityIndependent':True,'Augmentation219580Replay':False,'FullGlyphPoolJournal':False,'SourceGeometryParity':False,'NativeCRTBitIdentity':False}

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('ak',type=Path);p.add_argument('trace',type=Path);p.add_argument('no_trace',type=Path);p.add_argument('fresh_report',type=Path);a=p.parse_args()
    require(not a.fresh_report.exists(),'fresh report')
    raw=a.ak.read_bytes();require(hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978','exact AK raw AL')
    labels=alphabet(raw);joiners=[k for k,v in labels.items()if v=='\u200d'];require(len(joiners)<=1,'unique AL joiner');ignored=joiners[0]if joiners else 65535
    tb=a.trace.read_bytes();pb=a.no_trace.read_bytes();report=audit(json.loads(tb),json.loads(pb),ignored)
    report.update(AKSHA256=hashlib.sha256(raw).hexdigest(),TraceSHA256=hashlib.sha256(tb).hexdigest(),NoTraceSHA256=hashlib.sha256(pb).hexdigest())
    with a.fresh_report.open('x',encoding='utf8')as f:f.write(json.dumps(report,indent=2)+'\n')
    print(json.dumps(report))
if __name__=='__main__':main()
