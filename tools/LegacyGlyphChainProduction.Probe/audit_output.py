"""Independent218560 pool/WORD/request byte oracle. No expected transcript."""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
p=json.loads(Path(sys.argv[1]).read_bytes());MAX=(1<<64)-1
compact=base64.b64decode(p['compact']);starts=p['starts'];graph=p['graph'];flags=p['graphFlags']
def u16(b,n):return struct.unpack_from('<H',b,n)[0]
def i32(b,n):return struct.unpack_from('<i',b,n)[0]
def u64(b,n):return struct.unpack_from('<Q',b,n)[0]
def w16(b,n,v):struct.pack_into('<H',b,n,v&65535)
def w32(b,n,v):struct.pack_into('<I',b,n,v&0xffffffff)
def w64(b,n,v):struct.pack_into('<Q',b,n,v&MAX)
def fbits(v):return struct.unpack('<I',struct.pack('<f',v))[0]
def loadpool(nodes):return {n['key']:{'rows':[bytearray(base64.b64decode(n['records'])[i:i+40])for i in range(0,len(base64.b64decode(n['records'])),40)],'duplicate':bool(n['packed']&0x8000)}for n in nodes}
def key(ref):return ref&0x010000ffffffffff
def resolve(pool,ref):return pool[key(ref)]['rows'][(ref>>40)&65535]
def request(row,residue,ac,b0,storage=compact):
    q=bytearray(residue);adj=i32(row,32);previous=-1;previousarc=65535;sourceclass=0
    if adj!=-1:
        a=storage[adj*40:adj*40+40]
        #Actual DIRECT header0 is0; selected dictionary1 is copied below.
        sourceclass=(p['sourceHeaders'][a[4]]>>9)&15;assert sourceclass!=10
        if u16(a,28)==185:
            index=i32(a,16)
            if index!=-1:
                a=storage[index*40:index*40+40]
                if ((u16(a,28)+7)&65535)<4:
                    index=i32(a,16)
                    if index!=-1:a=storage[index*40:index*40+40]
                if index!=-1:
                    k=u64(a,8);previous=k&0xffffff
                    if previous&0x800000:previous-=0x1000000
                    previousarc=(k>>24)&65535
    w32(q,0,previous);w16(q,4,previousarc)
    q[8:14]=row[24:30];q[16:22]=row[12:18]
    w16(q,22,65535 if adj==-1 else u16(storage,adj*40+28))
    w32(q,24,i32(row,0)+(u16(row,56)==65535));q[28:30]=row[4:6];q[30:32]=row[56:58]
    q[32:38]=row[36:42];w32(q,40,{0xfffc:0,0xfffb:1,0xfffa:2}[u16(row,58)])
    w32(q,44,ac);w32(q,48,b0);w32(q,52,sourceclass);w32(q,56,0);q[60]=1
    return q

def apply(row,record,requestResidue,ac,b0):
    label=u16(row,58);subtype=row[60];count=1
    if subtype==0:
        assert label<195
        g=graph[i32(row,0)][u16(row,4)];candidates={s:(cost,prob)for s,cost,prob in p['groups'][g]['candidates']}
        scoreSymbol={193:134,194:129}.get(label,label)
        cost,score10=candidates.get(scoreSymbol,(0x4000000,0));score0=fbits(row[62]/255)
    elif subtype==3:
        q=request(row,requestResidue,ac,b0);kind=i32(q,40);previous=i32(q,8);current=i32(q,24)
        if previous==-1 or current==len(graph):count=0;label=65535
        elif kind==0:label=0xfffc
        elif kind==2:
            if flags[current][u16(q,28)]&8:label=0xfffb
            else:count=0;label=65535
        else:raise AssertionError('unsupported case fixture')
        score0=score10=0x3f800000;cost=0
    else:return False
    kind=0
    if count==0:kind=1;label=1
    elif 0xfff9<=label<=0xfffc:kind=1;label=(count<<2)|((label-1)&3)
    w16(record,4,((u16(record,4)&0xfff8)|kind)&0xfff7|(8 if row[61] else 0));w16(record,6,0);w16(record,8,label)
    w32(record,12,score0);w32(record,16,score10);w32(record,20,cost)
    return True

def replay(case,partial=False):
    rows=base64.b64decode(case['rows']);residue=base64.b64decode(case['residue']);qres=base64.b64decode(case['requestResidue'])
    pool=loadpool(case['beforePool']);word=bytearray(base64.b64decode(case['beforeWord']))
    def append(k,descriptor,force):
        node=pool.setdefault(k,{'rows':[],'duplicate':False})
        if not force and not node['duplicate']:
            for i,r in enumerate(node['rows']):
                if i32(r,0)==descriptor:return k|(i<<40),True
        d=bytearray(residue);w32(d,0,descriptor);w16(d,4,u16(d,4)&0xfff0);w32(d,6,0xffff0000)
        w32(d,12,0x3f800000);w32(d,16,0xbf800000);w32(d,20,0);d[24:32]=bytes(8);w64(d,32,MAX)
        ref=k|(len(node['rows'])<<40);node['rows'].append(d);node['duplicate']|=force
        if u64(word,8)==MAX:w64(word,8,ref)
        return ref,False
    tail=u64(word,8)
    if tail!=MAX:
        seen=set()
        while u64(resolve(pool,tail),32)!=MAX:
            assert tail not in seen;seen.add(tail);tail=u64(resolve(pool,tail),32)
    previous=MAX;firstnew=MAX;created=reused=0;forcecarry=case['mode']==6
    for i in reversed(range(len(rows)//72)):
        row=rows[i*72:(i+1)*72];k=u64(row,48);descriptor=i32(row,8)-starts[i32(row,0)]
        ref,existing=append(k,descriptor,bool(row[68] or k&(1<<39) or forcecarry))
        if existing and previous!=MAX and i32(row,12)!=-1 and u64(resolve(pool,ref),32)!=previous:ref,existing=append(k,descriptor,True)
        if existing:reused+=1
        else:
            d=resolve(pool,ref);w64(d,32,previous);forcecarry=True
            if not apply(row,d,qres,case['ac'],case['b0']):
                assert partial;return pool,word
            if firstnew==MAX:firstnew=ref
            created+=1
        previous=ref
    if tail==MAX:w64(word,8,previous)
    else:w64(resolve(pool,tail),32,previous)
    assert not partial
    assert (firstnew,previous,created,reused)==(case['firstNew'],case['last'],case['created'],case['reused'])
    return pool,word

def compare(pool,nodes):
    wanted=loadpool(nodes);assert set(pool)==set(wanted)
    for k,v in pool.items():assert v==wanted[k],(k,v,wanted[k])
created=reused=0
for case in p['cases']:
    pool,word=replay(case);compare(pool,case['afterPool']);assert word==base64.b64decode(case['afterWord'])
    if 'protocolRequest'in case:
        row=base64.b64decode(case['rows']);q=request(row,base64.b64decode(case['requestResidue']),case['ac'],case['b0'])
        assert q==base64.b64decode(case['protocolRequest'])
    created+=case['created'];reused+=case['reused']
assert len(p['lookback'])==48
for control in p['lookback']:
    q=request(base64.b64decode(control['row']),base64.b64decode(control['residue']),control['ac'],control['b0'],base64.b64decode(control['compact']))
    assert q==base64.b64decode(control['request'])
    assert i32(q,0)==[0,1,-1][control['mode']]
    assert (control['remaining'],control['score0C'],control['score10'],control['cost'])==(255,0x3f800000,0x3f800000,0)
assert len(p['cases'])==p['controls']==768 and created==p['created']==704 and reused==p['reused']==192 and p['guards']==12
#Unsupported later subtype keeps the allocated reverse prefix and currentrow;
#there is no finalhead replacement and no simulated native false status.
emptyword=bytearray(120);w64(emptyword,8,MAX)
partial={'rows':p['partialRows'],'residue':base64.b64encode(bytes(40)).decode(),'requestResidue':base64.b64encode(bytes(64)).decode(),'beforePool':[],'beforeWord':base64.b64encode(emptyword).decode(),'mode':0,'ac':0,'b0':0}
#CreateEmpty has independent scalar defaults unrelated to this loop.
w32(emptyword,16,0x3f800000);w32(emptyword,20,0x3f800000);w32(emptyword,48,0xfffe);w32(emptyword,60,-1);w32(emptyword,84,-1)
partial['beforeWord']=base64.b64encode(emptyword).decode();pool,word=replay(partial,True);compare(pool,p['partialPool']);assert word==base64.b64decode(p['partialWord'])
print(f'PASS independent768 fullWORD/pool reversechains,{created} new/{reused} reused,240 fullrequest64 padding/rawF32/hyphen-lookback controls and allocated-prefix refusal; explicit segments only.')
