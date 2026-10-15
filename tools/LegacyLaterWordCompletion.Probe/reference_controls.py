"""Original204290 history/token selector oracle. No vendor code executes."""
from pathlib import Path
import json
import random
import struct
import sys
import ast
from array import array

if len(sys.argv) != 2 or Path(sys.argv[1]).suffix.lower() != '.json':
    raise SystemExit('Usage: reference_controls.py controls-output.json (one output argument only)')

rng = random.Random(0x205B00)
def w16(data, at, value): struct.pack_into('<H', data, at, value & 65535)
def w32(data, at, value): struct.pack_into('<I', data, at, value & 0xffffffff)
cases = []
for index in range(1024):
    current_id = index % 4
    previous_id = current_id if index % 3 else (current_id + 1) % 4
    token = bytearray(rng.randbytes(16))
    token[12] = index % 2
    token[13] = index // 2 % 2
    previous = bytearray(rng.randbytes(88))
    previous[4] = previous_id
    previous[5] = rng.randrange(256)
    is_dash = index % 5 == 0
    w16(previous,36,65534 if is_dash else 0)
    previous[64] = index % 2
    history = rng.getrandbits(32)
    w32(previous,68,history)
    w32(previous,56,5)
    w16(previous,60,2)
    prior_anchor = bytearray(rng.randbytes(88))
    start = [0,2,4,6][index%4]
    prior_anchor[32] = (5-start) & 255
    current_anchor = bytearray(rng.randbytes(88))
    paths = [[22], [0xfffb,70], [0xfff9,70], [0xfff5,70], [0xffeb,70], [0xfff3,70], [0xfff4,70], [0xfffd,70]]
    path = paths[(index//4)%len(paths)]
    subword = current_id if index % 2 else -1
    extended = index % 3 == 0
    f5 = index % 4 != 0
    headers = [rng.getrandbits(16) & ~0x1e00 | (0x1400 if index%7 == 0 else 0xc00) for _ in range(4)]
    lines = [0,0,0,0,0,0,0,0]
    if index%6 == 0: lines[1] = -1
    if index%6 == 1: lines[2] = 1
    if index%6 == 2: lines[3] = -1
    if index%6 == 3: lines[4] = 3
    difference = [-2,0,1,2][(index//2)%4]
    callbacks=[]
    clear=False
    if f5:
        callbacks.append(['header',previous_id])
        if headers[previous_id]&0x1e00 == 0x1400 or is_dash:
            callbacks.append(['anchor',5,2])
            if start == 0: clear=True
            elif start < len(lines):
                callbacks.append(['line',start-1,0])
                if lines[start-1] != -1:
                    callbacks.append(['line',start,0])
                    clear=lines[start] != lines[start-1]
    use=previous_id==current_id and difference<=1
    if use and current_id==subword:
        oldest=path[0]
        marker=oldest in [0xfffb,0xfff9,0xfff5] or extended and ((oldest+0x15)&65535)<=8
        use=not marker
    output=bytearray(token)
    expected_history=[]
    if use:
        output[12]=previous[5]>>2&1
        if previous[64]: expected_history=[history]
    cases.append(dict(currentId=current_id, previous=previous.hex(), anchor=current_anchor.hex(),
        priorAnchor=prior_anchor.hex(), token=token.hex(), path=path, subword=subword,
        extended=extended,f5=f5,headers=headers,lines=lines,difference=difference,
        expectedToken=output.hex(), expectedHistory=expected_history, use=use,clear=clear,callbacks=callbacks))
ROOT = Path(__file__).resolve().parents[2]
lk = (ROOT / 'models/ru-RU/ru_RU-lk-text.lite.res').read_bytes()
def u32(data, at): return struct.unpack_from('<I', data, at)[0]
def f32(value): return struct.unpack('<f', struct.pack('<f', value))[0]
def real_float(at): return struct.unpack_from('<f', lk, at)[0]
def calibrated(raw, slope, offset, scale):
    return int(f32(f32(f32(f32(raw / 8192) * f32(scale * slope)) - f32(scale * offset)) * 8192))

#Direct bit windows, BFS child counts and a separate preorder stack recover
#actual terminal identities independently of the managed index implementation.
dictionary = lk[609980:2513123]
count = u32(dictionary, 20)
width = dictionary[36]
label_bytes = ((count * width + 31)//32)*4
labels = dictionary[37:37+label_bytes]
at = 37 + label_bytes + 1
assert u32(dictionary, at) == 0x1000000 and u32(dictionary, at+4) == count
child_width = dictionary[at+8]
children = dictionary[at+9:at+9+((count*child_width+31)//32)*4]
def packed(data, bits, index):
    byte, shift = divmod(bits*index,8)
    return int.from_bytes(data[byte:byte+5], 'little') >> shift & ((1<<bits)-1)
starts = array('I', [1])
for node in range(count): starts.append(starts[-1]+packed(children,child_width,node))
assert starts[-1] == count
terminal_nodes = array('I')
stack = [(starts[0],starts[1])]
while stack:
    begin,end=stack[-1]
    if begin==end:
        stack.pop(); continue
    stack[-1]=(begin+1,end)
    if packed(labels,width,begin) & (1<<(width-1)): terminal_nodes.append(begin)
    if starts[begin] != starts[begin+1]: stack.append((starts[begin],starts[begin+1]))
assert len(terminal_nodes) == 439826

tree = ast.parse((ROOT/'tools/LegacyWordHigherArc.Probe/reference_controls.py').read_text(encoding='utf-8-sig'))
selected = [node for node in tree.body if isinstance(node,ast.FunctionDef) and node.name=='profile']
scope={'ROOT':ROOT,'struct':struct,'u16':lambda data,at:struct.unpack_from('<H',data,at)[0]}
exec(compile(ast.Module(body=selected,type_ignores=[]),'original-category-reader','exec'),scope)
categories=scope['profile'](188222)
word_levels=struct.unpack_from('<256i',lk,3502860)
uni_levels=struct.unpack_from('<256i',lk,4553492)
bi_levels=struct.unpack_from('<256i',lk,4554516)
profile_start=4555548
ws,wo=real_float(profile_start+80),real_float(profile_start+40)
us,uo=ws,real_float(profile_start+16)
bs,bo=real_float(profile_start+84),real_float(profile_start+20)
boundary_offset=real_float(profile_start+56)
scale=real_float(607064)
punctuation=4565174-9498
category_flags=struct.unpack_from('<H',lk,punctuation)[0]
category_offset,category_slope=real_float(punctuation+10),real_float(punctuation+26)
transitions=struct.unpack_from('<169f',lk,punctuation+34)

formatters=[]
for index in range(256):
    word_id=(index*997)%439826
    node=terminal_nodes[word_id]
    offset,bit=divmod(word_id*10,8)
    word_class=(int.from_bytes(lk[2513246+offset:2513246+offset+3],'little')>>bit)&1023
    word_raw=word_levels[lk[3063030+word_id]]
    rows=[bytearray(rng.randbytes(88)) for _ in range(4)]
    current=bytearray(rng.randbytes(88))
    current[4]=1;current[5]=2 | ((index%2)<<2);current[6]&=254;current[7]&=254
    w32(current,0,node);w16(current,24,8);w16(current,26,index%35);w16(current,28,7);w16(current,30,2)
    current[32]=1;w16(current,36,65534);w32(current,56,8);w16(current,60,0)
    w64 = lambda data,at,value:struct.pack_into('<Q',data,at,value)
    state=0x123456789
    prior_start=[0,2,4,0][index%4]
    for ordinal,row in enumerate(rows):
        row[4]=1 if ordinal%2==0 else (2 if index%3==0 else 1)
        row[33]&=127
        w64(row,40,state);w32(row,56,6)
        w16(row,60,ordinal if ordinal<2 else ordinal-2)
        row[32]=(6-prior_start)&255 if ordinal<2 else 255
        w16(row,30,ordinal if ordinal<2 else ordinal-2)
        w16(row,36,65533 if index%5==0 else 0)
    chosen=2+index%2
    previous=rows[chosen]
    previous[5]=(index%2)*4
    previous[64]=index%2
    previous_class=(index*17)%1024
    old_tag=index%13
    history_value=previous_class|(old_tag<<24)
    w32(previous,68,history_value)
    headers=[0,0xc0a,0x140a if index%3==0 else 0xc0a]
    lines=[0]*10
    difference=[0,1,2,-2][index%4]
    lines[7]=difference
    if index%4==1: lines[2]=1
    path_suffix=[0xfffb,70] if index%7==0 else [22,70]
    subword=1 if index%7==0 else -1
    extended=False
    f5=index%3!=1
    clear=False
    if f5 and ((headers[previous[4]]&0x1e00)==0x1400 or index%5==0):
        clear=prior_start==0 or prior_start<10 and lines[prior_start-1]!=-1 and lines[prior_start]!=lines[prior_start-1]
    use=previous[4]==1 and difference<=1 and not(subword==1 and path_suffix[0]==0xfffb)
    history=[history_value] if use and previous[64] else []
    #094040 clears source70 token+8.204290 copies token into LM request18,
    #and100090 reads its category atrequest20; geometry must not replace it.
    category=0
    suppress=bool(current[5]&4)
    tag=0 if clear or not history else old_tag
    tagged=bool(category_flags and category and tag)
    if suppress: category_cost=0
    elif tagged:
        value=f32(f32(f32(category_slope*transitions[tag*13+category])+category_offset)*8192)
        category_cost=-int(f32(value+(-.5 if value<=0 else .5)))
    else: category_cost=(-2 if clear else 1) if category==3 else 2 if category in (5,6) else 3 if category in (10,11,12) else 0
    cls_raw=bi_levels[lk[3504908+previous_class*1024+word_class]] if history else uni_levels[lk[3503884+word_class]]
    word_cost=calibrated(word_raw,ws,wo,scale)
    class_cost=calibrated(cls_raw,bs if history else us,bo if history else uo,scale)
    boundary=bool(previous[5]&4) if use else False
    boundary_cost=int(f32(f32(f32(scale*boundary_offset)-f32(scale*wo))*-8192)) if boundary else 0
    subtotal=word_cost+class_cost+category_cost+boundary_cost
    residue=bytearray(rng.randbytes(40));output=bytearray(residue)
    w32(output,0,subtotal);w32(output,4,len(history)+1);output[8]=1
    new_tag=tag if suppress and tagged else 0 if suppress else category
    w32(output,12,word_class|(new_tag<<24));w32(output,24,0xffffffff);w32(output,28,word_id)
    candidate_cost=100000+index*17;increment=-700+index;geometry=-91+index
    current_ordinal=65535 if index%2==0 else 0
    context=struct.pack('<iiiHH',candidate_cost,increment,geometry,chosen,current_ordinal)
    packet=bytearray(rng.randbytes(80));w32(packet,0,8);w32(packet,8,6);w32(packet,32,1);w16(packet,12,7)
    correction=8192 if categories[index%35][0] in (2,4) and difference!=0 else 0
    path=candidate_cost+correction
    total=path+subtotal
    budget_root=total+[-1,0,1][index%3]
    accepted=not use or budget_root>=total
    allocated=budget_root>total
    storage=bytearray(rng.randbytes(128))
    root=bytearray(128);root[120:]=storage[120:];w32(root,16,budget_root)
    restart=(1|(12<<40)|(1<<32)) if suppress else (8<<40)
    lineage=((geometry&0xffffffff)<<32)|(chosen<<16)|current_ordinal
    expected=bytearray(root)
    if allocated:
        expected[:88]=current;expected[33]&=0xc0;expected[39]&=254
        w32(expected,20,increment);w16(expected,62,65535);w32(expected,16,total);w16(expected,30,0);expected[32]=255
        expected[64:84]=output[8:28];w64(expected,8,0);w64(expected,40,restart);w64(expected,92,lineage)
        w32(expected,88,0xffffffff);w32(expected,104,0xffffffff);w64(expected,112,0)
    formatters.append(dict(current=current.hex(),rows=[r.hex() for r in rows],headers=headers,lines=lines,context=context.hex(),packet=packet.hex(),
        residue=residue.hex(),storage=storage.hex(),expectedSlot=expected.hex(),output=output.hex(),suffix=path_suffix,subword=subword,
        f5=f5,wordId=word_id,wordClass=word_class,previousClass=previous_class if history else None,use=use,clear=clear,
        boundary=boundary,category=category,wordCost=word_cost,classCost=class_cost,categoryCost=category_cost,boundaryCost=boundary_cost,
        subtotal=subtotal,path=path,budgetRoot=budget_root,accepted=accepted,allocated=allocated,restart=restart,lineage=lineage))

output=Path(sys.argv[1]); output.parent.mkdir(parents=True,exist_ok=True)
output.write_text(json.dumps(dict(cases=cases,formatters=formatters),separators=(',',':')))
print('Generated1024 independent history controls and256 real-resource/full128 terminal formatter controls.')
