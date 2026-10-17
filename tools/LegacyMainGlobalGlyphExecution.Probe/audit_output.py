"""Independent218560/21FE30 composition oracle; no expected transcript.

Scalar producer snapshots are upstream-owned inputs, separately audited by the
ordinary/protocol probes. This checks all chain/pool/WORD bytes and native
callback success assignments, including explicitly PREPARED failures.
"""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
d=json.loads(Path(sys.argv[1]).read_bytes());MAX=2**64-1
b64=base64.b64decode
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
fbits=lambda x:struct.unpack('<I',struct.pack('<f',x))[0]
def w16(b,o,x):struct.pack_into('<H',b,o,x&65535)
def w32(b,o,x):struct.pack_into('<I',b,o,x&0xffffffff)
def w64(b,o,x):struct.pack_into('<Q',b,o,x&MAX)
def reset(raw):
    b=bytearray(raw)
    for o,x,f in [(8,MAX,'Q'),(16,0x3f800000,'I'),(20,0x3f800000,'I'),(24,0,'I'),(28,0,'I'),(40,0,'I'),(48,65534,'I'),(53,0,'H'),(55,0,'B'),(60,0xffffffff,'I'),(84,0xffffffff,'I'),(96,0,'I'),(112,0,'I')]:struct.pack_into('<'+f,b,o,x)
    return b
stores=[(0,16,4),(4,24,4),(8,28,4),(12,48,2),(14,50,2),(16,53,1),(17,54,1),(20,60,4),(28,68,4),(32,72,4),(36,76,4),(40,80,4),(44,55,1)]
def apply_packet(word,packet):
    out=bytearray(word)
    for source,target,count in stores:out[target:target+count]=packet[source:source+count]
    return out
def packet_from(template,origins,initial):
    p=bytearray(b64(template));assert len(p)==len(origins)==64
    for i,source in enumerate(origins):
        if source!=-1:assert 0<=source<120;p[i]=initial[source]
    return p
key=lambda ref:ref&0x010000ffffffffff
def loadpool(nodes):
    return {n['key']:dict(rows=[bytearray(b64(n['records'])[i:i+40])for i in range(0,len(b64(n['records'])),40)],duplicate=bool(n['packed']&0x8000))for n in nodes}
def resolve(pool,ref):return pool[key(ref)]['rows'][(ref>>40)&65535]
def chain(pool,head):
    result=[]
    while head!=MAX:
        assert head not in result;result.append(head);head=u64(resolve(pool,head),32)
    return result
def span(pool,refs):
    return sum((u16(resolve(pool,r),8)>>2)&8191 if u16(resolve(pool,r),4)&7 in (1,3) else 1 for r in refs)
def decode(ref):
    n=ref&0xffffff;a=(ref>>24)&65535
    return (n-0x1000000 if n&0x800000 else n,a-65536 if a&32768 else a)
callbacks=words=zero=created=reused=0
assert len(d['owners'])==8
for owner in d['owners']:
    starts=owner['starts'];graph=owner['graph'];assert owner['nnEvaluations']>0 and len(owner['cases'])==4
    for case in owner['cases']:
        initial=reset(b64(case['allocator120']));assert initial==b64(case['initial120'])
        #This region is actual allocator provenance, never an implicitzero seed.
        assert initial[68:84]==b64(case['allocator120'])[68:84]
        previous=initial;pool={};intern={};record_residue=b64(case['recordResidue40'])
        for part in case['flushes']:
            packet=packet_from(part['template64'],part['origins'],initial);assert packet==b64(part['packet64'])
            assert b64(part['before120'])==previous
            word=apply_packet(previous,packet);rows=b64(part['rows72']);assert len(rows)%72==0
            scalar={p['ordinal']:p for p in part['producerScalars']};assert len(scalar)==len(rows)//72
            tail_refs=chain(pool,u64(word,8));tail=tail_refs[-1]if tail_refs else MAX
            last=MAX;firstnew=MAX;new=old=0;forcecarry=False
            def append(k,descriptor,force):
                assert key(k)==k
                node=pool.setdefault(k,dict(rows=[],duplicate=False))
                if not force and not node['duplicate']:
                    for ordinal,r in enumerate(node['rows']):
                        if i32(r,0)==descriptor:return k|(ordinal<<40),True
                r=bytearray(record_residue);w32(r,0,descriptor);w16(r,4,u16(r,4)&0xfff0);w32(r,6,0xffff0000)
                w32(r,12,0x3f800000);w32(r,16,0xbf800000);w32(r,20,0);r[24:32]=bytes(8);w64(r,32,MAX)
                ref=k|(len(node['rows'])<<40);node['rows'].append(r);node['duplicate']|=force
                if u64(word,8)==MAX:w64(word,8,ref)
                return ref,False
            for i in reversed(range(len(rows)//72)):
                row=rows[i*72:(i+1)*72];node=i32(row,0);index=i32(row,8);assert starts[node]<=index<starts[node+1]
                k=u64(row,48);ref,existing=append(k,index-starts[node],bool(row[68]or k&(1<<39)or forcecarry))
                if existing and last!=MAX and i32(row,12)!=-1 and u64(resolve(pool,ref),32)!=last:ref,existing=append(k,index-starts[node],True)
                if existing:old+=1
                else:
                    r=resolve(pool,ref);w64(r,32,last);forcecarry=True;p=scalar[i];remaining=p['remaining'];count=256-remaining
                    label=p['labels'][0 if remaining<0 else remaining]if remaining<256 else 65535
                    if row[60]==0:
                        assert remaining==255 and label==u16(row,58) and p['external']==0 and p['component']==0
                        if u16(row,58)==178 or ((u16(row,58)+9)&65535)<=5:assert(p['score0C'],p['score10'],p['cost14'])==(0x3f800000,0x3f800000,0)
                        else:assert p['score0C']==fbits(row[62]/255)
                    else:
                        assert row[60]==3 and p['score0C']==p['score10']==0x3f800000 and p['cost14']==0
                    if p['external']:kind=4
                    elif count==0:kind=1;label=1
                    elif 0xfff9<=label<=0xfffc:
                        assert count<8192;kind=1;packed=((p['component']<<13)|(count&8191))&65535;label=((packed<<2)|((label-1)&3))&65535
                    else:kind=0
                    w16(r,4,((u16(r,4)&0xfff8)|kind)&0xfff7|(8 if row[61]else 0));w16(r,6,p['publication']);w16(r,8,label)
                    w32(r,12,p['score0C']);w32(r,16,p['score10']);w32(r,20,p['cost14'])
                    if firstnew==MAX:firstnew=ref
                    new+=1
                last=ref
            if tail==MAX:w64(word,8,last)
            else:w64(resolve(pool,tail),32,last)
            assert(firstnew,last,new,old)==(part['firstNew'],part['last'],part['created'],part['reused'])
            assert pool==loadpool(part['pool']) and word==b64(part['completed120']) and part['succeeded']
            refs=chain(pool,u64(word,8));native_span=span(pool,refs);assert part['span']==native_span
            if native_span>0:
                pub=part['published'];assert pub is not None
                left=refs[0]
                for ref in refs:
                    if decode(ref)[1]>=0:left=ref
                ln,la=decode(left);rn,ra=decode(refs[0]);first=graph[ln][la][0]if la>=0 else ln+la;last_boundary=graph[rn][ra][1]if ra>=0 else rn
                if la<0 and key(left)==key(refs[0]):first,last_boundary=last_boundary,first
                if i32(packet,48)!=-1:first=i32(packet,48)
                if i32(packet,52)!=-1:last_boundary=i32(packet,52)
                assert(pub['first'],pub['last'])==(first,last_boundary)
                saved=bytearray(word);capacity=0;flags=u16(word,50)
                for count,ref in enumerate(refs,1):
                    flags|=u16(resolve(pool,ref),6)
                    if capacity<count:capacity=max(count,int(capacity*1.5))
                w64(saved,32,0);w32(saved,40,len(refs));w32(saved,44,capacity);w16(saved,50,flags&~0x400)
                old_words=intern.setdefault((first,last_boundary),[]);match=next((i for i,b in enumerate(old_words)if u64(b,8)==u64(word,8)),None)
                if match is None:match=len(old_words);old_words.append(saved)
                assert pub['index']==match and b64(pub['word120'])==old_words[match] and pub['flat']==list(reversed(refs));words+=1
            else:assert part['published']is None;zero+=1
            previous=reset(word);assert previous==b64(part['reset120']);callbacks+=1;created+=new;reused+=old
    failed_initial=b64(owner['failedInitial120']);failed_packet=packet_from(owner['failedTemplate64'],owner['failedOrigins'],failed_initial)
    assert b64(owner['failedAfterPacket120'])==apply_packet(failed_initial,failed_packet)
assert len(d['preparedOutcomes'])==160
for c in d['preparedOutcomes']:
    mode=c['mode'];before=b64(c['before120']);exception=mode in(6,7,8)
    assert c['exception']==exception
    log=[]if mode==8 else ['entry']
    if mode not in(0,8,9):log+=['chain']
    if mode in(4,5,7):log+=['publish']
    assert c['log']==log
    assert b64(c['after120'])==(before if exception else reset(before))
    if not exception:
        assert c['span']==(1 if mode in(3,4,5,9)else 0)
        assert c['success']==(mode in(2,5)) and c['published']==(mode==5) and b64(c['completed120'])==before
assert callbacks==64 and words==zero==32 and created>0 and d['guards']==392
print(f'PASS independent{callbacks} actualcallbacks/{words}275words/{zero}zeros/{created}new/{reused}reused glyphrecords; packetseed/fullpool/WORD/reset/spans;160PREPARED outcome/exception cases, no expected text')
