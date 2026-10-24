"""English actual DATA audit adapted from pinned native-derived RU composition; no production C# import, no labels/NN."""
import ast,base64,hashlib,json,math,struct,sys,runpy
from array import array
from pathlib import Path
if len(sys.argv)!=7:raise SystemExit('AK LK capture prior-graph prior-first fresh-report.json')
ak=Path(sys.argv[1]).read_bytes();lk=Path(sys.argv[2]).read_bytes();actual=json.loads(Path(sys.argv[3]).read_bytes())
assert hashlib.sha256(ak).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
assert hashlib.sha256(lk).hexdigest()=='9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9'
assert actual['Completed']is True and actual['Admission']=='NONE' and actual['OriginalIndex']==4 and actual['GraphNodes']==2
comp=actual['Compact'];assert comp['Nodes']==2
assert set(actual)=={'Scope','Completed','InputSha','OriginalIndex','RawPoints','RawStrokes','Resolution','GraphNodes','ParentChildren','Stages','Metrics','Compact','NoExpectedLabels','Admission','GlobalReadinessGateUnchanged','TimeoutMs'}
assert set(comp)=={'Nodes','Records','Storage','Masks','Starts','PrefixOwners','Winner','SelectedRow','InputNodes','OutputNodes','Graph','Contexts','Scores','FinalWorkingMap'}
assert all(type(comp[n])is int for n in ['Nodes','Records'])
assert all(type(v)is int for k in ['Starts','PrefixOwners']for v in comp[k])
assert set(comp['Winner'])=={'HasWinner','Ordinal','RelativeCost','TotalCost','RowCost14','RowField22','FallbackFlag','ResultFlag8'}
assert type(comp['Winner']['HasWinner'])is bool and all(type(v)is int for k,v in comp['Winner'].items()if k!='HasWinner')
assert all(set(v)=={'Identity','ParentLink','ChildTreeRoot','Left','Right','Symbol','Balance','PageIndex','PageSlot'}and all(type(x)is int for x in v.values())for v in comp['Contexts'])
assert len(comp['InputNodes'])==len(comp['OutputNodes'])==len(comp['Graph'])==2
assert len(comp['Scores'])==len({s['Group']for s in comp['Scores']})
for s in comp['Scores']:
 assert set(s)=={'Group','Span','PowerScaleBits','Symbols','Costs','WeightBits'}
 assert all(type(s[k])is int for k in ['Group','Span','PowerScaleBits'])
 assert len(s['Symbols'])==len(s['Costs'])==len(s['WeightBits'])and 0<len(s['Costs'])<=150
 assert all(type(v)is int for k in ['Symbols','Costs','WeightBits']for v in s[k])
for node in comp['InputNodes']+comp['OutputNodes']:
 assert set(node)=={'SourceEnd','RetainedEnd','Rows','RowMasks','Primary','PrimaryMasks'}
 assert type(node['SourceEnd'])is int and type(node['RetainedEnd'])is int
 assert len(base64.b64decode(node['Rows']))==len(base64.b64decode(node['RowMasks']))==156*88
 assert len(base64.b64decode(node['Primary']))==len(base64.b64decode(node['PrimaryMasks']))==125*80

c=dict(primary=125,maximumAtoms=2,quota=2,penalty=115101,contexts=[[v['Identity'],v['ParentLink'],v['Symbol']]for v in comp['Contexts']],graph=[[[v['Group'],v['Span'],v['End']]for v in node]for node in comp['Graph']],inputNodes=[dict(rows=v['Rows'],alternatives=v['Primary'],sourceEnd=v['SourceEnd'])for v in comp['InputNodes']],nodes=[dict(rows=v['Rows'],alternatives=v['Primary'])for v in comp['OutputNodes']],compact=comp['Storage'],count=comp['Records'],starts=comp['Starts'],prefixOwners2488=comp['PrefixOwners'],scores=[None]*(1+max(s['Group']for s in comp['Scores'])))
for group in comp['Scores']:
 c['scores'][group['Group']]=dict(span=group['Span'],scaleBits=group['PowerScaleBits'],candidates=list(zip(group['Symbols'],group['Costs'])),weightBits=group['WeightBits'])
publication=dict(headers=[0,0xc0e],initialStorage=base64.b64encode(bytes(len(base64.b64decode(c['compact'])))).decode())
case_mode=True;primary_events=frame_events=probe_events=0
choose=runpy.run_path(str(Path(__file__).resolve().parent/'reference/native_primary_flags_oracle.py'))['choose']
cache_power=struct.unpack_from('<f',ak,545659)[0];assert ak[545658]==1 and struct.unpack_from("<I",ak,545659)[0]==0x3dcccccd
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def u32(b,p):return struct.unpack_from('<I',b,p)[0]
def u64(b,p):return struct.unpack_from('<Q',b,p)[0]
def s8(b,p):return struct.unpack_from('<b',b,p)[0]
def w16(b,p,v):struct.pack_into('<H',b,p,v&65535)
def w32(b,p,v):struct.pack_into('<I',b,p,v&0xffffffff)
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def wrap(x):return (x+2147483648)%4294967296-2147483648
# Pinned native normalizer loaded from preserved reviewed Python definitions only.
source=Path(__file__).resolve().parent/'reference/native_sequence.py'
assert hashlib.sha256(source.read_bytes()).hexdigest()=='4587f2d20a5bfbac7209708649fc0b69977f644bda4640fcef8ee14d1ff0ba2c'
tree=ast.parse(source.read_text(encoding='utf8'));names={'TABLE','u16','i32','w16','w32','normalize','sentinel'};selected=[]
for item in tree.body:
 if isinstance(item,ast.FunctionDef)and item.name in names:selected.append(item)
 elif isinstance(item,ast.Assign)and any(isinstance(t,ast.Name)and t.id in names for t in item.targets):selected.append(item)
ns={'struct':struct};exec(compile(ast.Module(body=selected,type_ignores=[]),str(source),'exec'),ns);normalize=ns['normalize']
def read_alphabet(data):
    # Standalone resource parser: CI must not depend on ignored PE research.
    at=99;fields=struct.unpack_from('<8H',data,at);at+=16
    n=u16(data,at);at+=2
    for increment in (32,64,1):
        table=struct.unpack_from('<'+'H'*n,data,at);at+=n*2
        n=next(v+increment for v in reversed(table) if v!=65535)
    spans=struct.unpack_from('<'+'H'*(n+1),data,at);at+=(n+1)*2
    entries=[struct.unpack_from('<II',data,at+8*i) for i in range(spans[-1]+1)];at+=len(entries)*8
    strings=data[at:at+entries[-1][0]]
    mapping={label&65535:strings[offset:entries[i+1][0]].decode('utf8') for i,(offset,label) in enumerate(entries[:-1])}
    return dict(fields=fields,label_count=fields[-1],mapping=mapping)
prepared=runpy.run_path(str(Path(__file__).resolve().parent/'audit_output.py'))
al,startmap=prepared['metadata'](ak);labels={v:k for k,v in al['mapping'].items()};N=al['label_count']
knowledge=al['fields'][3];special_de=labels.get('\u200c',65535);special_e2=labels.get('\u0651',65535);closed=labels['\U000f0001'];reserved={N,*al['fields'][4:7]};zwj=labels.get('\u200d',65535)
#The real packed lexicon supplies child ranges and mapped protocol labels.
assert u32(lk,2572)==0x101fe90 and u32(lk,2580)==0x2b4 and u32(lk,2584)==0x01020000
nodes_count=666547;width=8;packed=lk[2612:669160];assert len(packed)==((nodes_count*width+31)//32)*4
assert lk[669160]==17 and u32(lk,669161)==0x01000001 and u32(lk,669168)==nodes_count and lk[669172:669174]==bytes([7,1])
cw=7;children=lk[669176:1252408]
def bits(b,width,index):
 at,shift=divmod(width*index,8);return int.from_bytes(b[at:at+5],'little')>>shift&((1<<width)-1)
child_start=array('I',[1])
for n in range(nodes_count):child_start.append(child_start[-1]+bits(children,cw,n))
assert child_start[-1]==nodes_count
rows=[bytearray(base64.b64decode(n['rows'])) for n in c['inputNodes']]
pools=[bytearray(base64.b64decode(n['alternatives'])) for n in c['inputNodes']]
limits=[n['sourceEnd'] for n in c['inputNodes']];primary=c['primary'];graph=c['graph']
assert len(rows)==len(graph)
if case_mode:assert 0<len(rows)<=c.get('intermediatePublicationDistance',c['maximumAtoms'])
else:assert len(rows)==4
context={n[0]:(n[1],n[2]) for n in c['contexts']}
def path(identity):
    result=[]
    while identity:
        assert identity in context and identity not in result and len(result)<50
        result.append(identity);identity=context[identity][0]
    return result[::-1]
# SAME actual working map is an exported source INPUT, not independently proven mapping production.
mapping=actual['Compact']['FinalWorkingMap'];assert len(mapping)==104 and all(type(v)is int and 0<=v<=65535 for v in mapping)
def continuation(row,n):
    if row[6]&1 or i32(row,48)!=-1:return False
    if row[5]&2:return True
    def follow(node,depth,kind):
        for child in range(child_start[node]+(node==0),child_start[node+1]):
            value=bits(packed,width,child);symbol=mapping[value&((1<<(width-1))-1)]
            if symbol in (65535,zwj):continue
            protocol=symbol>=N and ((symbol+29)&65535)>=8
            if protocol or symbol==special_de:
                next_kind=kind;take=True
                if symbol in (65527,65529):
                    if n==s8(row,32):take=False
                    else:next_kind=1
                elif symbol==65528:next_kind=1
                elif symbol==65531:
                    if kind!=1:next_kind=2
                elif symbol==65532:
                    if kind!=1 and ((kind-2)&253)!=0:next_kind=3
                if take and depth<7 and follow(child,depth+1,next_kind):return True
            elif kind in (1,2):return True
        return False
    return follow(u32(row,0),0,0)
#Independent active-node frontier and predecessor marking. Earlier nodes become
#active only through a retained secondary predecessor; a primary outside the
#lookahead also requires the actual raw-LK continuation predicate.
prefix=len(rows)-1-min(len(rows),c['maximumAtoms'] if case_mode else len(rows))
active=[n>prefix for n in range(len(rows))]
for n in range(len(rows)):
    for o in range(limits[n]):rows[n][o*88+6]&=253
    for o in range(primary*2):pools[n][o*40+27]&=127
for n in reversed(range(len(rows))):
    if not active[n]:continue
    for o in range(limits[n]):
        row=memoryview(rows[n])[o*88:(o+1)*88]
        if i32(row,16)>=67108864:continue
        if o<primary and n<=prefix and not continuation(row,n):continue
        if o<primary:
            for k in range(2):
                if i32(pools[n],(o*2+k)*40)>=67108864:break
                pn,po,pa=n,o,k
                while pn>=0 and po<primary:
                    at=(po*2+pa)*40;b=pools[pn]
                    if b[at+27]&128:break
                    b[at+27]|=128
                    if b[at+31]&32:break
                    po,pn,pa=u16(b,at+24),pn-s8(b,at+26)-1,b[at+27]&127
        pn,po=n,o
        while pn>=0:
            at=po*88;b=rows[pn]
            if po<primary:
                if b[at+6]&2:break
            else:active[pn]=True
            b[at+6]|=2;po,pn=u16(b,at+30),pn-s8(b,at+32)-1

weights=[]
for group in c['scores']:
    if group is None:weights.append({});continue
    scale=struct.unpack('<f',struct.pack('<I',group['scaleBits']))[0]
    # Actual raw100=0 was independently restored by the owning scorer. Keep
    # this strict scale1 scope instead of inserting an unproved new default.
    assert scale==f(math.pow(group["span"]+1,cache_power))
    candidates=group['candidates']
    if len(candidates)==1:values=[1.]
    else:
        values=[f(math.exp(float(f(-f(cost)/f(scale*8192.))))) for _,cost in candidates[:20]]
        total=0.
        for value in values:total=f(total+value)
        values=[f(value/total) for value in values]+[0.]*(len(candidates)-len(values))
    assert len({v[0]for v in candidates})==len(candidates)
    assert [struct.unpack('<i',struct.pack('<f',v))[0]for v in values]==group['weightBits'],('actual normalized weight bits',group['span'])
    weights.append(dict(zip((row[0] for row in candidates),values)))

def score(n,arc,symbol):
    group=graph[n][arc][0]
    if symbol in weights[group]:return weights[group][symbol]
    return 0.
def score_byte(n,row):
    symbol=u16(row,36)
    if symbol==65535:return 255
    if u16(row,24)!=n and row[5]&128:symbol=N
    return max(1,min(255,int(f(score(n,u16(row,28),symbol)*255))))
def frame(n,row,current,previous,difference):
    raw=bytearray(944)
    if u16(row,24)!=n and row[5]&128:delta=[knowledge]
    else:
        a,b=path(current),path(previous);i=0
        while i<min(len(a),len(b)) and a[i]==b[i]:i+=1
        delta=[context[v][1] for v in a[i:]]
    w32(raw,0,len(delta));control=0
    for i,symbol in enumerate(delta):
        at=4+i*12;w16(raw,at,symbol)
        kind=3 if ((symbol+21)&65535)<19 or symbol in (special_de,65535) else 0
        raw[at+2]=kind;control+=kind==3;w32(raw,at+4,-1)
    w32(raw,0x3ac,control);w32(raw,0x3a4,difference)
    w32(raw,0x25c,1);struct.pack_into('<Q',raw,0x260,n|(u16(row,28)<<24))
    out=normalize(raw,False);assert 1<=i32(out,0)<=50
    global last_frame
    last_frame=bytes(out)
    return [(u16(out,4+i*12),out[6+i*12],u64(out,0x260+i32(out,8+i*12)*16)) for i in range(i32(out,0))]
capacity=len(base64.b64decode(c['compact']));storage=bytearray(base64.b64decode(publication['initialStorage']));assert len(storage)==capacity;count=0;starts=[]
def write(tag,coord,parent,cost,inc,symbol,alt,flags,value,aux):
    global count
    at=count*40;assert at+40<=len(storage)
    struct.pack_into('<QQiiiHHBB',storage,at,tag,coord,parent,wrap(cost),wrap(inc),symbol,alt,flags,value)
    w32(storage,at+36,aux);count+=1;return count-1
def append(items,row,n,tag,parent,cost,inc,alternative,forced,value,bit3=False):
    global frame_events
    frame_events+=1
    for index,(symbol,kind,coord) in enumerate(items):
        final=index==len(items)-1;terminal6=False
        if final:
            terminal6=symbol in (N+1,N+2) or not(u16(row,24)!=n and row[5]&128) and symbol not in(special_e2,closed,knowledge) and symbol<65515 and row[26]&63!=startmap[symbol]
        parent=write(tag,coord,parent,cost,inc,symbol,alternative if final else 65535,
            (kind&7)|(64 if terminal6 else 0)|(16 if final and forced else 0)|(8 if final and bit3 else 0),value,-1)
    return parent
def pool(n,o,k):return memoryview(pools[n])[(o*2+k)*40:(o*2+k+1)*40]
def beam(n,o):return memoryview(rows[n])[o*88:(o+1)*88]
def previous(n,relative):return -1 if relative==65535 else starts[n]+relative
penalty=int(f(f(f(c['penalty'])*f(.8))+.5))
for n in range(len(rows)):
    starts.append(count);used=set();candidates=[]
    for o in range(primary):
        for k in range(2):
            p=pool(n,o,k)
            if not p[27]&128:continue
            r=beam(n,o);arc=u16(r,28);assert r[4]==1 and i32(r,48)==-1
            assert (r[7]&1)==0 if case_mode else r[7]==0
            candidates.append(dict(o=o,k=k,cost=i32(p,0),inc=i32(p,4),tag=u64(r,0),ctx=u64(p,16),key=i32(r,52),end=graph[n][arc][2],forced=False))
    def compare(a,b):
        for field in ('key','end','cost'):
            delta=wrap(a[field]-b[field])
            if delta:return delta
        return (context[a['ctx']][1] if a['ctx'] else 65535)-(context[b['ctx']][1] if b['ctx'] else 65535)
    def sort(first,last):
        while True:
            left,right=first,last;pivot=candidates[(first+last)//2]
            while True:
                while compare(candidates[left],pivot)<0:left+=1
                while compare(candidates[right],pivot)>0:right-=1
                if left>right:break
                candidates[left],candidates[right]=candidates[right],candidates[left];left+=1;right-=1
                if left>right:break
            if first<right:sort(first,right)
            if last<=left:return
            first=left
    if candidates:sort(0,len(candidates)-1)
    group_start=0;selected=0
    for index,item in enumerate(candidates):
        if index==0 or (item['key'],item['end'])!=(candidates[index-1]['key'],candidates[index-1]['end']):group_start=index;selected=0
        r=beam(n,item['o']);p=pool(n,item['o'],item['k'])
        primary_events+=1
        def probe():
            global probe_events
            probe_events+=1
            return continuation(r,n)
        native_rows=[dict(Tag=x['tag'],Context=x['ctx'],Auxiliary=-1,Fallback=bool(x['tag']&(1<<41)),Bit3=x.get('bit3',False),Bit4=x['forced']) for x in candidates]
        decision=choose(native_rows,index,group_start,selected,c['quota'],publication['headers'],probe)
        forced=decision['TerminalBit4'];bit3=decision['TerminalBit3'];selected=decision['SelectedDistinctAfter']
        item['forced']=forced;item['bit3']=bit3
        pn=n-s8(p,26)-1;po=u16(p,24);pa=p[27]&127;reset=bool(p[31]&32);assert pn<n and not p[31]&128
        prev_ctx=0 if pn<0 or reset else u64(pool(pn,po,pa),16)
        parent=-1 if pn<0 or reset else previous(pn,u16(pool(pn,po,pa),28))
        glyph=p[31]&31
        if not reset and pn>=0 and po>=primary:glyph=(glyph-(beam(pn,po)[33]&63))&255
        terminal=append(frame(n,r,item['ctx'],prev_ctx,glyph),r,n,item['tag'],parent,item['cost']+(penalty if forced else 0),item['inc'],65535,forced,score_byte(n,r),bit3)
        w16(p,28,terminal-starts[n]);used.add(u16(r,28))
    for o in range(limits[n]):
        r=beam(n,o)
        if not r[6]&2:continue
        arc=u16(r,28);used.add(arc);pn=n-s8(r,32)-1;po=u16(r,30)
        alt=65535 if o<primary else 65534;parent=-1 if pn<0 else previous(pn,u16(beam(pn,po),34))
        if pn==n:
            assert u64(r,8)==0 and alt==65534 and i32(r,80)==-1 and u16(r,62)==65535
            terminal=write(u64(r,0),n|(arc<<24),parent,i32(r,16),i32(r,20),65535,alt,3,255,-1)
        else:
            glyph=r[33]&63
            if pn>=0 and po>=primary:glyph=(glyph-(beam(pn,po)[33]&63))&255
            terminal=append(frame(n,r,u64(r,8),0,glyph),r,n,u64(r,0),parent,i32(r,16),i32(r,20),alt,False,score_byte(n,r))
        w16(r,34,terminal-starts[n])
    for arc in sorted(used):
        group=graph[n][arc][0];emitted={}
        for symbol,value in c['scores'][group]['candidates']:
            if symbol==65535:break
            if symbol in reserved:continue
            delta=int(f(weights[group][symbol]*255))
            if symbol in emitted:
                at=emitted[symbol]*40+33;storage[at]=max(1,min(255,storage[at]+delta))
            else:
                emitted[symbol]=write(18446744073709551615,n|(arc<<24),-1,0,0,symbol,65535,32,max(1,min(255,delta)),-1)
                if len(emitted)==2:break
starts.append(count)
assert count==c['count'] and starts==c['starts'],('native node allocation counts',count,starts)
expected=base64.b64decode(c['compact']);assert storage==expected,next(((i,a,b)for i,(a,b)in enumerate(zip(storage,expected))if a!=b),None)
for n,out in enumerate(c['nodes']):
 assert rows[n]==base64.b64decode(out['rows']),('full beam storage',n)
 assert pools[n]==base64.b64decode(out['alternatives']),('full primary storage',n)
# Native2257C0, actual fresh filter4=false: primary and flag5.bit2 rows are excluded.
IMP=0x4000000;best=IMP;winner=-1
for o in range(limits[-1]):
 row=memoryview(rows[-1])[o*88:(o+1)*88];cost=i32(row,16)
 if cost>=IMP:continue
 if o<125 or row[5]&4:cost=IMP
 if cost<best:best=cost;winner=o
assert winner>=0
row=memoryview(rows[-1])[winner*88:(winner+1)*88]
w=dict(HasWinner=True,Ordinal=winner,RelativeCost=best,TotalCost=best,RowCost14=i32(row,20),RowField22=u16(row,34),FallbackFlag=0,ResultFlag8=0)
assert actual['Compact']['Winner']==w and base64.b64decode(actual['Compact']['SelectedRow'])==row
assert c['prefixOwners2488']==[-1,-1,1]
assert actual['Compact']['Masks']==base64.b64encode(bytes(0 if i%40 in(34,35)else 255 for i in range(count*40))).decode()
# Frozen previously independently audited actual graph/context prefix is regression DATA only.
prior_path=Path(sys.argv[4])
prior_bytes=prior_path.read_bytes();assert hashlib.sha256(prior_bytes).hexdigest()=='655f50b9ad1dd993fb0ac51735504aac905b4500ae676b91bfe4c94f5c724e8d'
prior=json.loads(prior_bytes);assert actual['Metrics']==prior['Metrics']
assert all(type(v)is int for v in actual['Metrics'].values())
last=comp['InputNodes'][-1]
for name,oldname in [('Rows','Rows156'),('RowMasks','RowMasks156'),('Primary','PrimaryAlternatives80'),('PrimaryMasks','PrimaryAlternativeMasks')]:
 assert base64.b64decode(last[name])==bytes.fromhex(prior['GraphNode'][oldname]),('unchanged actual graph prefix',name)
oldpath=Path(sys.argv[5]);oldbytes=oldpath.read_bytes()
assert hashlib.sha256(oldbytes).hexdigest()=='0cee3661348b9db5be53f7fd694c5d1997c8d27c923109db90e5cb921854eb0b'
old=json.loads(oldbytes)
for name,oldname in [('Rows','ParentRows88'),('RowMasks','ParentRowMasks'),('Primary','ParentAlternatives80'),('PrimaryMasks','ParentAlternativeMasks')]:
 original=bytes.fromhex(old[oldname]);assert base64.b64decode(comp['InputNodes'][0][name])[:len(original)]==original
fields=['Identity','ParentLink','ChildTreeRoot','Left','Right','Symbol','Balance','PageIndex','PageSlot']
assert [[v[k]for k in fields]for v in comp['Contexts']]==old['Contexts']
for n,(before,after)in enumerate(zip(comp['InputNodes'],comp['OutputNodes'],strict=True)):
 assert after['SourceEnd']==before['SourceEnd']==limits[n]and after['RetainedEnd']==before['RetainedEnd']
 mask=bytearray(base64.b64decode(before['RowMasks']))
 for o in range(limits[n]):
  if rows[n][o*88+6]&2:mask[o*88+34:o*88+36]=bytes([255,255])
 assert mask==base64.b64decode(after['RowMasks']),('full row definedness',n)
 assert before['PrimaryMasks']==after['PrimaryMasks'],('conservative primary definedness unchanged',n)
assert all(type(v)is int for k,v in w.items()if k!='HasWinner')and type(w['HasWinner'])is bool
report=dict(scope='DATA-only raw-LK207780 + independently rebuilt944normalization/fullusedcompact40/full88+80/winner; no NN, no WORD',records=count,primaryFrames=primary_events,probes=probe_events,frames=frame_events,bit3=sum(bool(storage[i*40+32]&8)for i in range(count)),fullCompactBytes=len(storage),fullBeamPoolBytes=sum(map(len,rows))+sum(map(len,pools)),winner=w,weights='all30 exported normalized-weight bits exact from actual cache costs/raw .1 exponent',priorPrefix='metrics,last-node rows/masks/primary,node0 prefix and all243context records exact',masks='all compact/fullnode masks exact; primary conservatively unchanged',workingMap='captured same-source INPUT; map production not independently re-proved here')
Path(sys.argv[6]).write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report,indent=2))
