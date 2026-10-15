"""Independent raw-LK/case-table/full16/40/128 prepared later completion oracle."""
import hashlib,json,struct,sys
from array import array
from pathlib import Path
if len(sys.argv)!=4:raise ValueError('LK case-preparation-controls capture.json')
lk=Path(sys.argv[1]).read_bytes()
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
classes={r[0]:r[1] for r in json.loads(Path(sys.argv[2]).read_text(encoding='utf-8-sig'))[0]['rows']}
got=json.loads(Path(sys.argv[3]).read_text(encoding='utf-8-sig'))
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def u32(b,p):return struct.unpack_from('<I',b,p)[0]
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def u64(b,p):return struct.unpack_from('<Q',b,p)[0]
def w16(b,p,v):struct.pack_into('<H',b,p,v&65535)
def w32(b,p,v):struct.pack_into('<I',b,p,v&0xffffffff)
def w64(b,p,v):struct.pack_into('<Q',b,p,v&0xffffffffffffffff)
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def real(p):return struct.unpack_from('<f',lk,p)[0]
def cost(raw,slope,offset,scale):return int(f(f(f(f(raw/8192)*f(scale*slope))-f(scale*offset))*8192))
def bits(b,width,index):
    p,shift=divmod(index*width,8)
    return int.from_bytes(b[p:p+5],'little')>>shift&((1<<width)-1)
d=lk[609980:2513123];count=u32(d,20);width=d[36]
size=((count*width+31)//32)*4;labels=d[37:37+size];p=37+size+1
assert count==895571 and u32(d,p)==0x1000000 and u32(d,p+4)==count
cw=d[p+8];children=d[p+9:p+9+((count*cw+31)//32)*4]
starts=array('I',[1])
for node in range(count):starts.append(starts[-1]+bits(children,cw,node))
assert starts[-1]==count
needed={u32(bytes.fromhex(c['query']),0) for c in got['captures']}
word_ids={};ordinal=0;stack=[(starts[0],starts[1])]
while stack:
    begin,end=stack[-1]
    if begin==end:stack.pop();continue
    stack[-1]=(begin+1,end)
    if bits(labels,width,begin)&(1<<(width-1)):
        if begin in needed:word_ids[begin]=ordinal
        ordinal+=1
    if starts[begin]!=starts[begin+1]:stack.append((starts[begin],starts[begin+1]))
assert ordinal==439826 and set(word_ids)==needed
word_levels=struct.unpack_from('<256i',lk,3502860)
uni=struct.unpack_from('<256i',lk,4553492);bi=struct.unpack_from('<256i',lk,4554516)
profile=4555548;ws=real(profile+80);wo=real(profile+40);us=ws;uo=real(profile+16)
bs=real(profile+84);bo=real(profile+20);boundary_offset=real(profile+56);scale=real(607064)
punctuation=4565174-9498;flags=u16(lk,punctuation)
category_offset=real(punctuation+10);category_slope=real(punctuation+26)
transitions=struct.unpack_from('<169f',lk,punctuation+34)
categories=bytes.fromhex('090b0104040404040505050a010c040606060505050202020303030500000000')
seen=set();histories=set();nonterminal=0
for c in got['captures']:
    query=bytes.fromhex(c['query']);current=bytes.fromhex(c['current']);previous=bytes.fromhex(c['previous'])
    context=bytes.fromhex(c['context']);assert got['primaryLimit']<=u16(context,12)<got['previousSourceEnd'];tag=u64(query,0);symbol=u16(query,20);state=tag>>58&31
    category=7 if tag&(1<<40) and classes[symbol]==8 else 8 if classes[symbol]==9 else categories[state]
    seen.add(category);word_id=word_ids[tag&0xffffffff]
    token=bytearray(16);w32(token,0,1);w32(token,4,word_id);w32(token,8,category);token[13]=tag>>42&1
    assert token==bytes.fromhex(c['token'])
    use=previous[4]==query[4] #same assigned source descriptor line in this fixture
    assert use==c['useHistory'];histories.add(use)
    history=[u32(previous,68)] if use and previous[64] else []
    assert previous[64]<=1
    if use:token[12]=previous[5]>>2&1
    assert token==bytes.fromhex(c['historyToken'])
    p,shift=divmod(word_id*10,8);word_class=int.from_bytes(lk[2513246+p:2513246+p+3],'little')>>shift&1023
    wc=cost(word_levels[lk[3063030+word_id]],ws,wo,scale)
    cls=history[0]&0xffffff if history else None
    cc=cost(bi[lk[3504908+cls*1024+word_class]],bs,bo,scale) if history else cost(uni[lk[3503884+word_class]],us,uo,scale)
    old_tag=history[0]>>24&15 if history else 0
    prefix=bool(flags and category and old_tag)
    if token[13]:category_cost=0
    elif prefix:
        value=f(f(f(category_slope*transitions[old_tag*13+category])+category_offset)*8192)
        category_cost=-int(f(value+(-.5 if value<=0 else .5)))
    else:category_cost=1 if category==3 else 2 if category in (5,6) else 3 if category in (10,11,12) else 0
    bc=int(f(f(f(scale*boundary_offset)-f(scale*wo))*-8192)) if token[12] else 0
    subtotal=wc+cc+category_cost+bc
    output=bytearray.fromhex(c['residue']);w32(output,0,subtotal);w32(output,4,len(history)+1);output[8]=1
    new_tag=old_tag if token[13] and prefix else 0 if token[13] else category
    w32(output,12,word_class|(new_tag<<24));w32(output,24,-1);w32(output,28,word_id)
    assert output==bytes.fromhex(c['language']),('full40',c['kind'],state,symbol,use)
    path=i32(context,0)+i32(query,16);assert c['path']==path
    restart=(1|(12<<40)|(1<<32)|(tag&(0x7c<<56))) if tag&(4<<40) else 8<<40
    restart=(restart&~(0x70<<40))|((query[23]<<4&0x70)<<40)
    assert c['restart']==restart
    lineage=(u32(context,8)<<32)|(u16(context,12)<<16)|u16(context,14);assert c['lineage']==lineage
    storage=bytes.fromhex(c['storage']);slot=bytearray(128);slot[120:]=storage[120:];slot[:88]=current
    w64(slot,0,tag);w64(slot,8,u64(query,8));w32(slot,16,path+subtotal);w32(slot,20,i32(context,4));w16(slot,30,0);slot[32]=255
    slot[33]=(slot[33]&192)|(((query[23]>>5&3)+query[22])&63);slot[39]&=254
    w64(slot,40,restart);w16(slot,62,65535);slot[64:84]=output[8:28];w32(slot,88,-1);w64(slot,92,lineage)
    w32(slot,104,i32(current,56)-1);w64(slot,112,u64(previous,40))
    assert slot==bytes.fromhex(c['slot']),('full128',c['kind'],state,symbol,use)
    if c['kind']=='prepared-nonterminal-current-terminal-query':
        assert current[5]&2==0 and query[5]&2;nonterminal+=1
assert got['actualFirstTerminals']>0 and got['preparedHistoryCases']==339 and got['guards']>=35
assert got['mapSpan']!=got['anchorSpan'] and got['snapshotChecks']==1 and len(got['captures'])==339
assert seen==set(range(1,13)) and histories=={False,True} and nonterminal==1
print('PASS independent339 prepared-history full16/40/128 records; all12 case categories, unigram/bigram histories, fulltag restart/lineage and actualquery/nonterminal control. No native V48-positive or later-winner claim.')
