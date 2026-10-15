"""Independent LK metadata/language audit of actual fresh292 scheduler rows."""
import base64,hashlib,json,struct,sys
from array import array
from pathlib import Path
if len(sys.argv)!=5:raise SystemExit('AK LK case-preparation-controls capture')
ak=Path(sys.argv[1]).read_bytes();lk=Path(sys.argv[2]).read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
classes={r[0]:r[1] for r in json.loads(Path(sys.argv[3]).read_bytes())[0]['rows']}
captures=json.loads(Path(sys.argv[4]).read_bytes())
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def u32(b,p):return struct.unpack_from('<I',b,p)[0]
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def u64(b,p):return struct.unpack_from('<Q',b,p)[0]
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def f32(p):return struct.unpack_from('<f',lk,p)[0]
def calibrated(raw,slope,offset,scale):return int(f(f(f(f(raw/8192)*f(scale*slope))-f(scale*offset))*8192))
def bits(b,width,index):
    p,shift=divmod(index*width,8);return int.from_bytes(b[p:p+5],'little')>>shift&((1<<width)-1)
#Independent profile201 composite encounter order from the raw sampler stream.
p=188222;assert u16(ak,p)==0x201;entries=u16(ak,p+2)+3;p+=4
def ids():
    global p
    n=ak[p];p=(p+4)&~3;v=struct.unpack_from('<'+'i'*n,ak,p);p+=4*n;return v
first_ids=ids();first=struct.unpack_from('<'+'b'*entries,ak,p);p+=entries
geometry_ids=ids();start=struct.unpack_from('<'+'b'*entries,ak,p);p+=entries
end=struct.unpack_from('<'+'b'*entries,ak,p);zero=first_ids.index(0);composites=[]
for n in range(entries):
    for key in [(first[n],start[n]),(first[n],end[n]),(zero,start[n]),(zero,end[n])]:
        if -1 not in key and key not in composites:composites.append(key)
node_sets=[];needed=set();language_rows=[]
for capture in captures:
    nodes=[]
    for index,node in enumerate(capture['nodes']):
        assert node['node']==index
        rows=base64.b64decode(node['records']);primary=capture['primary'];assert len(rows)%88==0
        assert primary<=node['retainedEnd']<=node['sourceEnd']<=len(rows)//88
        assert len(base64.b64decode(node['alternatives']))==primary*80
        nodes.append(rows)
        for ordinal in range(primary,node['sourceEnd']):
            row=rows[ordinal*88:(ordinal+1)*88]
            if i32(row,16)>=0x4000000:continue
            assert row[4]==1 and row[32]==255 and row[64]==1
            needed.add(u32(row,0));language_rows.append((capture,nodes,index,ordinal,row))
    node_sets.append(nodes)
assert language_rows
d=lk[609980:2513123];count=u32(d,20);width=d[36];nbytes=((count*width+31)//32)*4;labels=d[37:37+nbytes]
p=37+nbytes+1;assert u32(d,p)==0x1000000 and u32(d,p+4)==count
cw=d[p+8];children=d[p+9:p+9+((count*cw+31)//32)*4];starts=array('I',[1])
for node in range(count):starts.append(starts[-1]+bits(children,cw,node))
assert starts[-1]==count
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
word_levels=struct.unpack_from('<256i',lk,3502860);uni_levels=struct.unpack_from('<256i',lk,4553492)
ws=f32(4555548+80);wo=f32(4555548+40);uo=f32(4555548+16);scale=f32(607064)
categories=bytes.fromhex('090b0104040404040505050a010c040606060505050202020303030500000000')
seen=set()
for capture,nodes,node,ordinal,row in language_rows:
    tag=u64(row,0);symbol=u16(row,36);kind=classes[symbol];state=tag>>58&31
    category=7 if tag&(1<<40) and kind==8 else 8 if kind==9 else categories[state]
    word_id=word_ids[tag&0xffffffff];at,shift=divmod(word_id*10,8)
    word_class=int.from_bytes(lk[2513246+at:2513246+at+3],'little')>>shift&1023
    suppress=bool(tag&(4<<40));word_cost=calibrated(word_levels[lk[3063030+word_id]],ws,wo,scale)
    class_cost=calibrated(uni_levels[lk[3503884+word_class]],ws,uo,scale)
    category_cost=0 if suppress else 1 if category==3 else 2 if category in (5,6) else 3 if category in (10,11,12) else 0
    assert u32(row,68)==word_class|((0 if suppress else category)<<24),('word class/category',node,ordinal)
    parent_ordinal=u16(row,30);assert parent_ordinal<capture['primary']
    parent=nodes[node][parent_ordinal*88:(parent_ordinal+1)*88]
    anchor_node=i32(parent,56);anchor_ordinal=u16(parent,60)
    anchor=nodes[anchor_node][anchor_ordinal*88:(anchor_ordinal+1)*88]
    assert anchor_node-(struct.unpack_from('<b',anchor,32)[0])==0
    previous_category=first_ids[composites[anchor[26]&63][0]]
    correction=8192 if previous_category in (2,4) else 0
    assert i32(row,16)-i32(parent,16)==word_cost+class_cost+category_cost+correction,('LM/path cost',node,ordinal)
    assert i32(row,20)==i32(parent,20) and u32(row,80)==0xffffffff
    restart=1|(12<<40)|(1<<32)|(tag&(0x7c<<56)) if suppress else 8<<40
    assert u64(row,40)==restart,('case restart',node,ordinal)
    seen.add(category)
print(f'PASS independent raw LK/AK: {len(language_rows)} actual language rows, {len(needed)} terminal nodes, categories {sorted(seen)}, all source/retained extents, original anchor geometry, LM costs and case restart tags; no OCR accuracy claim')
