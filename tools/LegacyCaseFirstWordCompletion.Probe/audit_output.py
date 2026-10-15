"""Independent actual-resource metadata/first-language/full128 case completion audit."""
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
def stored_float(p):return struct.unpack_from('<f',lk,p)[0]
def calibrated(raw,slope,offset,scale):
    return int(f(f(f(f(raw/8192)*f(scale*slope))-f(scale*offset))*8192))
def bits(b,width,index):
    p,shift=divmod(index*width,8)
    return int.from_bytes(b[p:p+5],'little')>>shift&((1<<width)-1)
#Recover only requested terminal preorder ordinals, without importing C# or
#allocating a model-sized Python dictionary of every decoded word.
needed={u32(bytes.fromhex(c['query']),0) for c in got['captures']}
d=lk[609980:2513123];count=u32(d,20);width=d[36]
nbytes=((count*width+31)//32)*4;labels=d[37:37+nbytes]
p=37+nbytes+1;assert u32(d,p)==0x1000000 and u32(d,p+4)==count
cw=d[p+8];children=d[p+9:p+9+((count*cw+31)//32)*4]
starts=array('I',[1])
for node in range(count):starts.append(starts[-1]+bits(children,cw,node))
assert starts[-1]==count
ordinal=0;word_ids={};stack=[(starts[0],starts[1])]
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
uni_levels=struct.unpack_from('<256i',lk,4553492)
profile=4555548;ws=stored_float(profile+80);wo=stored_float(profile+40)
us=ws;uo=stored_float(profile+16);scale=stored_float(607064)
categories=bytes.fromhex('090b0104040404040505050a010c040606060505050202020303030500000000')
seen=set()
for c in got['captures']:
    query=bytes.fromhex(c['query']);parent=bytes.fromhex(c['parent']);tag=u64(query,0)
    assert query[4]==1 and parent[4]==1
    symbol=u16(query,20);state=tag>>58&31;kind=classes[symbol]
    category=7 if tag&(1<<40) and kind==8 else 8 if kind==9 else categories[state]
    seen.add(category);word_id=word_ids[tag&0xffffffff]
    token=bytearray(16);w32(token,0,1);w32(token,4,word_id);w32(token,8,category);token[13]=tag>>42&1
    assert token==bytes.fromhex(c['token']),('metadata',c['kind'],state,symbol)
    p,shift=divmod(word_id*10,8)
    word_class=int.from_bytes(lk[2513246+p:2513246+p+3],'little')>>shift&1023
    word_cost=calibrated(word_levels[lk[3063030+word_id]],ws,wo,scale)
    class_cost=calibrated(uni_levels[lk[3503884+word_class]],us,uo,scale)
    category_cost=0 if token[13] else 1 if category==3 else 2 if category in (5,6) else 3 if category in (10,11,12) else 0
    subtotal=word_cost+class_cost+category_cost
    expected_score=dict(wordClass=word_class,wordCost=word_cost,classCost=class_cost,categoryCost=category_cost,boundaryCost=0,lexicalSubtotal=subtotal)
    assert c['score']==expected_score,('score',c['kind'],state,symbol)
    output=bytearray.fromhex(c['residue']);w32(output,0,subtotal);w32(output,4,1);output[8]=1
    w32(output,12,word_class|((0 if token[13] else category)<<24));w32(output,24,-1);w32(output,28,word_id)
    assert output==bytes.fromhex(c['language']),('full40',c['kind'],state,symbol)
    path=i32(parent,16)+i32(query,16);assert path==c['path']
    restart=(1|(12<<40)|(1<<32)|(tag&(0x7c<<56))) if tag&(4<<40) else 8<<40
    restart=(restart&~(0x70<<40))|((query[23]<<4&0x70)<<40)
    assert restart==c['restart'],('restart',state,symbol)
    storage=bytes.fromhex(c['storage']);slot=bytearray(128);slot[120:]=storage[120:]
    slot[:88]=parent;w64(slot,0,tag);w64(slot,8,u64(query,8))
    w32(slot,16,path+subtotal);w32(slot,20,i32(parent,20));w16(slot,30,c['ordinal']);slot[32]=255
    slot[33]=(slot[33]&192)|(((query[23]>>5&3)+query[22])&63);slot[39]&=254
    w64(slot,40,restart);w16(slot,62,65535);slot[64:84]=output[8:28]
    w32(slot,88,-1);w64(slot,92,(0x4000000<<32)|0xffffffff);w32(slot,104,-1);w64(slot,112,0)
    assert slot==bytes.fromhex(c['slot']),('full128',c['kind'],state,symbol)
assert got['actualCount']==70 and got['preparedCount']==168 and got['guards']>=13 and len(got['captures'])==238
assert seen==set(range(1,13))
print('PASS independent238 full16 metadata/full40 language/full128 allocations; actual terminal preorder/profile2, all12 categories, full-tag restart and residue preservation;70 actual +168 prepared cases')
