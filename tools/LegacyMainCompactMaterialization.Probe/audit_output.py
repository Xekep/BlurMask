"""Independent raw-LK query predicate + compact40 publication composition.

Uses captured *inputs*: actual pre-publication88/40 rows, context identities,
graph arcs, initial lexical mapping and actual retained integer candidate costs.
It never imports production C# or uses the selected text as feedback.
"""
import ast,base64,hashlib,json,math,struct,sys
from array import array
from pathlib import Path
if len(sys.argv) not in (4,5):raise SystemExit('AK LK capture.json [--fresh-case]')
case_mode=len(sys.argv)==5
if case_mode:assert sys.argv[4]=='--fresh-case'
ak=Path(sys.argv[1]).read_bytes();lk=Path(sys.argv[2]).read_bytes();c=json.loads(Path(sys.argv[3]).read_bytes())
if case_mode:assert c['factory']==0x292
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def u32(b,p):return struct.unpack_from('<I',b,p)[0]
def u64(b,p):return struct.unpack_from('<Q',b,p)[0]
def s8(b,p):return struct.unpack_from('<b',b,p)[0]
def w16(b,p,v):struct.pack_into('<H',b,p,v&65535)
def w32(b,p,v):struct.pack_into('<I',b,p,v&0xffffffff)
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def wrap(x):return (x+2147483648)%4294967296-2147483648
root=Path(__file__).resolve().parents[2]
#Load only the independent list-based normalizer; never execute its CLI.
source=root/'tools/LegacyCompactWordSequence.Probe/reference_controls.py'
tree=ast.parse(source.read_text(encoding='utf8'));names={'TABLE','u16','i32','w16','w32','normalize','sentinel'};selected=[]
for item in tree.body:
    if isinstance(item,ast.FunctionDef) and item.name in names:selected.append(item)
    elif isinstance(item,ast.Assign) and any(isinstance(t,ast.Name) and t.id in names for t in item.targets):selected.append(item)
ns={'struct':struct};exec(compile(ast.Module(body=selected,type_ignores=[]),str(source),'exec'),ns)
normalize=ns['normalize']
#Independent native2093A8..209512 flag selector; import only its pure definition.
#No production C# translation, output rows or CLI execution enters this model.
flags_source=root/'tools/LegacyMainPrimaryCompactFlags.Probe/audit_output.py'
flags_tree=ast.parse(flags_source.read_text(encoding='utf-8-sig'))
flags_namespace={}
exec(compile(ast.Module(body=[x for x in flags_tree.body if isinstance(x,ast.FunctionDef) and x.name=='choose'],type_ignores=[]),str(flags_source),'exec'),flags_namespace)
choose_flags=flags_namespace['choose']
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
al=read_alphabet(ak);labels={v:k for k,v in al['mapping'].items()};N=al['label_count']
knowledge=al['fields'][3];special_de=labels.get('\u200c',65535);special_e2=labels.get('\u0651',65535);closed=labels['\U000f0001']
reserved={N,*al['fields'][4:7]};zwj=labels.get('\u200d',65535)
#Read native profile201 symbol->start-composite mapping directly from AK.
p=188222;assert u16(ak,p)==0x201;entries=u16(ak,p+2)+3;p+=4
def ids():
    global p
    n=ak[p];p=(p+4)&~3;v=struct.unpack_from('<'+'i'*n,ak,p);p+=4*n;return v
first_ids=ids();first=struct.unpack_from('<'+'b'*entries,ak,p);p+=entries
geometry_ids=ids();start=struct.unpack_from('<'+'b'*entries,ak,p);p+=entries
end=struct.unpack_from('<'+'b'*entries,ak,p);zero=first_ids.index(0);composites=[];startmap=[]
for n in range(entries):
    for key in [(first[n],start[n]),(first[n],end[n]),(zero,start[n]),(zero,end[n])]:
        if -1 not in key and key not in composites:composites.append(key)
    key=(first[n],start[n]);startmap.append(composites.index(key) if -1 not in key else 255)
#The real packed lexicon supplies child ranges and mapped protocol labels.
d=lk[609980:2513123];nodes_count=u32(d,20);width=d[36];size=((nodes_count*width+31)//32)*4;packed=d[37:37+size]
def bits(b,width,index):
    at,shift=divmod(width*index,8);return int.from_bytes(b[at:at+5],'little')>>shift&((1<<width)-1)
p=37+size+1;assert u32(d,p)==0x1000000 and u32(d,p+4)==nodes_count
cw=d[p+8];children=d[p+9:p+9+((nodes_count*cw+31)//32)*4];child_start=array('I',[1])
for n in range(nodes_count):child_start.append(child_start[-1]+bits(children,cw,n))
assert child_start[-1]==nodes_count
#Fresh092990 descriptor from raw selected13B prefix +language2D1 order.
#Do not confuse PrimaryArcLimit230A (capture['primary']) with quota2402.
prefix=606964
name=bytes(v^(0x14,0x12,0x19,0x77)[i&3] for i,v in enumerate(lk[prefix+24:prefix+80]))
assert name.split(b'\0',1)[0]==b'ru_RU-lk-text.lite.lex' and not any(name[name.index(0):])
order=u32(lk,2513123+90);assert order in (1,2,3)
has_children=child_start[2]!=child_start[1]
kind=0x800 if has_children and b'subword' in name.split(b'\0',1)[0] else 0xc00
source_headers=[0,2|((order*4)&0x1c)|(0x20 if has_children else 0)|kind]
#This CLI admits fresh00C860 knowledge14/16=-1, prepared2402=2 only.
quota=2
if 'sourceHeaders' in c:assert c['sourceHeaders']==source_headers,('actual source headers',c['sourceHeaders'],source_headers)
if 'quota' in c:assert c['quota']==quota,('fresh primary quota',c['quota'])
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
        result.append(identity);identity=context[identity][0]
    return result[::-1]
#Reconstruct final D0's working50 from actual whole-span final195 costs.
allowed=set(al['fields'][4:7])|{labels.get(t,65535) for t in ('\U000f0000','\u200c','\u200d')}
last_span=max(arc[1] for arc in graph[-1])
for group,span,_ in graph[-1]:
    if span==last_span:allowed.update(i for i,v in enumerate(c['scores'][group]['costs'][:N]) if v<67108864)
mapping=[v if v>=N or v in allowed else 65535 for v in c['primaryMap']]
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
    scale=struct.unpack('<f',struct.pack('<I',group['scaleBits']))[0]
    # Actual raw100=0 was independently restored by the owning scorer. Keep
    # this strict scale1 scope instead of inserting an unproved new default.
    assert scale==1.
    candidates=group['candidates']
    if len(candidates)==1:values=[1.]
    else:
        values=[f(math.exp(float(f(-f(cost)/f(scale*8192.))))) for _,cost in candidates[:20]]
        total=0.
        for value in values:total=f(total+value)
        values=[f(value/total) for value in values]+[0.]*(len(candidates)-len(values))
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
    return [(u16(out,4+i*12),out[6+i*12],u64(out,0x260+i32(out,8+i*12)*16)) for i in range(i32(out,0))]
capacity=len(base64.b64decode(c['compact']));storage=bytearray([167])*capacity;count=0;starts=[]
def write(tag,coord,parent,cost,inc,symbol,alt,flags,value,aux):
    global count
    at=count*40;assert at+40<=len(storage)
    struct.pack_into('<QQiiiHHBB',storage,at,tag,coord,parent,wrap(cost),wrap(inc),symbol,alt,flags,value)
    w32(storage,at+36,aux);count+=1;return count-1
def append(items,row,n,tag,parent,cost,inc,alternative,forced,value,bit3=False):
    for index,(symbol,kind,coord) in enumerate(items):
        final=index==len(items)-1;terminal6=False
        if final:
            terminal6=symbol in (N+1,N+2) or not(u16(row,24)!=n and row[5]&128) and symbol not in(special_e2,closed,knowledge) and symbol<65515 and row[26]&63!=startmap[symbol]
        parent=write(tag,coord,parent,cost,inc,symbol,alternative if final else 65535,
            (kind&7)|(64 if terminal6 else 0)|(8 if final and bit3 else 0)|(16 if final and forced else 0),value,-1)
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
            candidates.append(dict(o=o,k=k,cost=i32(p,0),inc=i32(p,4),tag=u64(r,0),ctx=u64(p,16),key=i32(r,52),end=graph[n][arc][2],forced=False,bit3=False))
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
    #Native group quota resets on BOTH key/end changes, after unstable sort.
    flag_rows=[dict(Tag=x['tag'],Context=x['ctx'],Auxiliary=-1,Fallback=bool(x['tag']&(1<<41)),Bit3=False,Bit4=False) for x in candidates]
    group_start=0;selected_distinct=0
    for index,item in enumerate(candidates):
        if index==0 or (item['key'],item['end'])!=(candidates[index-1]['key'],candidates[index-1]['end']):
            group_start=index;selected_distinct=0
        r=beam(n,item['o']);p=pool(n,item['o'],item['k'])
        decision=choose_flags(flag_rows,index,group_start,selected_distinct,quota,source_headers,lambda:continuation(r,n))
        bit3=decision['TerminalBit3'];forced=decision['TerminalBit4'];selected_distinct=decision['SelectedDistinctAfter']
        item['bit3']=bit3;item['forced']=forced
        flag_rows[index]['Bit3']=bit3;flag_rows[index]['Bit4']=forced
        pn=n-s8(p,26)-1;po=u16(p,24);pa=p[27]&127;reset=bool(p[31]&32);assert pn<n and not p[31]&128
        prev_ctx=0 if pn<0 or reset else u64(pool(pn,po,pa),16)
        parent=-1 if pn<0 or reset else previous(pn,u16(pool(pn,po,pa),28))
        glyph=p[31]&31
        if not reset and pn>=0 and po>=primary:glyph=(glyph-(beam(pn,po)[33]&63))&255
        terminal=append(frame(n,r,item['ctx'],prev_ctx,glyph),r,n,item['tag'],parent,item['cost']+(penalty if forced and source_headers[(item['tag']>>32)&255]&0x1c else 0),item['inc'],65535,forced,score_byte(n,r),bit3)
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
expected=base64.b64decode(c['compact']);assert storage==expected, next(((i,a,b) for i,(a,b) in enumerate(zip(storage,expected)) if a!=b),None)
for n,out in enumerate(c['nodes']):
    assert rows[n]==base64.b64decode(out['rows']),('full beam storage',n)
    assert pools[n]==base64.b64decode(out['alternatives']),('full primary storage',n)
if c.get('publicationKind')=='live':
    assert c['startingNode']==0 and c['previousCount']==0
    assert c['actualGraphNodeCount']>len(rows)==c['nextNode2490']
    if c['nativeGate']:assert len(rows)==120
    else:assert c['actualGraphNodeCount']<=120 # explicitly prepared invocation, not a native gate
    assert c['prefixOwners2488']==[-1]*len(rows)+[len(rows)-1]
else:
    assert starts[-2]+u16(rows[-1],c['winner']*88+34)==c['selected']
print(f'PASS independent raw-LK207780, context identity/frame normalization, ordering/dedup/penalty and full {len(storage)} compact bytes, {sum(map(len,rows))+sum(map(len,pools))} beam/pool bytes; {count} genuine records, selected{c.get("selected","not invoked during LIVE publication")}, no expected text')
