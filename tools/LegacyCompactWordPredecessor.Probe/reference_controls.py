"""Independent prepared compact graph controls from21FFD0's packed field rules."""
import json,random,struct,sys
from pathlib import Path
rng=random.Random(0x21ffd0)
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def put16(b,p,n):struct.pack_into('<H',b,p,n)
def put32(b,p,n):struct.pack_into('<i',b,p,n)
def coordinate(b,index):
    packed=int.from_bytes(b[40*index+8:40*index+16],'little')
    node=packed&0xffffff
    return node-(1<<24) if node&(1<<23) else node,(packed>>24)&65535
def pack(b,index,node,arc):
    #High24 bits are unrelated residue; arc deliberately crosses a byte lane.
    value=(rng.getrandbits(24)<<40)|(arc<<24)|(node&0xffffff)
    struct.pack_into('<Q',b,40*index+8,value)
def expected(b,starts,record,node,alt,marker):
    selected=alt
    prev=i32(b,record*40+16)
    if prev==-1:return [-1,-1,65535,alt],'root'
    pn,pa=coordinate(b,prev)
    has_alt=u16(b,prev*40+30)!=65535
    marker_redirect=u16(b,record*40+28)==marker and pn<node
    branch=has_alt or marker_redirect
    if branch:
        if alt<=65533:
            if pn<0 or pn>=len(starts):return None,None
            prev=starts[pn]+alt
            if prev<0 or prev>=len(b)//40:return None,None
        alt=u16(b,prev*40+30)
    n,a=coordinate(b,prev)
    branch_name=('alternate' if has_alt else 'marker')+('-selected' if selected<65534 else '-retained') if branch else 'direct'
    return [prev,n,a,alt],branch_name
groups=[];coverage={}
for size,total in [(256,4096),(65560,256)]:
    pool=bytearray(rng.randbytes(size*40));starts=[rng.randrange(0,16) for _ in range(8)]
    for index in range(size):
        node=rng.choice(list(range(8))+[-1,-8388608,8388607]) if size==256 else rng.randrange(8)
        pack(pool,index,node,rng.randrange(65536))
        put32(pool,index*40+16,rng.choice([-1,rng.randrange(min(size,250))]))
        put16(pool,index*40+28,rng.choice([65535,65533,7,9,178]))
        put16(pool,index*40+30,rng.choice([65535,65534,rng.randrange(32)]))
    cases=[];begins=[]
    while len(cases)<total:
        record=rng.randrange(size);node=rng.choice([-8388608,0,1,5,8388607])
        alt0=rng.choice([0,1,7,65534,65535] if size==256 else [65533,65534,65535])
        marker=rng.choice([65535,65533,7,9,178])
        result,branch=expected(pool,starts,record,node,alt0,marker)
        if result is None:continue
        cases.append(dict(input=[record,node,0,alt0],marker=marker,expected=result))
        coverage[branch]=coverage.get(branch,0)+1
    for _ in range(128):
        node=rng.randrange(8);relative=rng.randrange(32);index=starts[node]+relative
        selected=rng.choice([0,1,65533,65534,65535]);_,arc=coordinate(pool,index)
        alt=u16(pool,40*index+30) if selected==65535 else selected
        begins.append(dict(node=node,relative=relative,selected=selected,expected=[index,node,arc,alt]))
    groups.append(dict(pool=pool.hex(),starts=starts,advance=cases,begin=begins))
assert all(coverage.get(k,0)>0 for k in ['root','direct','alternate-selected','alternate-retained','marker-selected','marker-retained']),coverage
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(groups=groups,coverage=coverage),separators=(',',':'))+'\n',encoding='utf-8')
print('PASS generated4352 predecessor/256 initialization controls; signed24 extremes, FFFD/FFFE/FFFF and every predecessor branch',coverage)
