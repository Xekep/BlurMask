"""Independent actual four-node secondary transport using raw row/context data."""
import base64,json,struct,sys
from pathlib import Path
d=json.loads(Path(sys.argv[1]).read_text())
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
s8=lambda n:n if n<128 else n-256
wrap=lambda n:(n+2**31)%2**32-2**31
contexts={i:(p,k) for i,p,k in d['contexts']}
paths={0:()}
def path(i):
    if i not in paths:paths[i]=path(contexts[i][0])+(contexts[i][1],)
    return paths[i]
by_path={path(i):i for i in contexts};by_path[()]=0
actions=[-1,-2,-1,-2,3,3,-1,-1,-1,-2,3,3,-2,-2,-1,-2,3,3,-1,-1,-1,-1,-1,-1,3,3,3,-2,3,3,3,3,3,-2,3,3]
def combine(prefix,suffix,mapped):
    out=list(path(prefix))
    for symbol in path(suffix):
        if not out:
            if mapped and 0xfff7<=symbol<=0xfffc:continue
        else:
            if symbol==0xfffd:continue
            if 0xfff7<=out[-1]<=0xfffc and 0xfff7<=symbol<=0xfffc:
                action=actions[(0xfffc-out[-1])*6+0xfffc-symbol]
                if action==-1:continue
                out.pop()
                if action!=-2:symbol=(-action-4)&65535
        out.append(symbol)
    return by_path[tuple(out)]
rows=[base64.b64decode(n['records']) for n in d['nodes']]
actual=[base64.b64decode(n['secondary']) for n in d['nodes']]
expected=[];checked=0
for node,n in enumerate(d['nodes']):
    wanted=bytearray(len(actual[node]));r=rows[node]
    for ordinal in range(len(r)//88):
        at=ordinal*80
        if ordinal<125:struct.pack_into('<ii',wanted,at,0x4000000,0x4000000)
        if ordinal>=n['sourceEnd']:continue
        rec=r[ordinal*88:ordinal*88+88]
        if i32(rec,16)>=0x4000000:continue
        out=bytearray(80)
        struct.pack_into('<ii',out,0,i32(rec,16),i32(rec,20))
        struct.pack_into('<ii',out,40,0x4000000,0x4000000)
        if ordinal<125:
            previous=node-s8(rec[32])-1
            if previous<0:
                mapped=combine(0,u64(rec,8),True);raw=combine(0,u64(rec,8),False)
            else:
                parent=u16(rec,30)
                parent_rec=rows[previous][parent*88:parent*88+88]
                parent_alt=expected[previous][parent*80:parent*80+80]
                mapped=u64(parent_alt,8);raw=u64(parent_alt,16)
                reset=path(u64(parent_rec,8))[-1] in (0xfff5,0xfff9,0xfffa,0xfffb)
                append=u16(rec,24)==node or s8(rec[5])>=0
                if reset or append:
                    mapped=combine(mapped,u64(rec,8),not reset);raw=combine(raw,u64(rec,8),False)
            struct.pack_into('<QQH',out,8,mapped,raw,u16(rec,30))
            out[26]=rec[32];out[30]=rec[38];out[31]=rec[33]&31
        else:
            assert u16(rec,62)==65535,'Anchored actual language branch needs its separate oracle.'
            parent=u16(rec,30)
            parent_rec=r[parent*88:parent*88+88]
            parent_alt=wanted[parent*80:parent*80+80]
            assert i32(parent_alt,0)==i32(parent_rec,16)
            mapped=combine(u64(parent_alt,8),u64(rec,8),True)
            raw=combine(u64(parent_alt,16),u64(rec,8),False)
            struct.pack_into('<QQH',out,8,mapped,raw,parent)
            out[26]=255;struct.pack_into('<H',out,28,65535)
            out[31]=rec[33]&31;struct.pack_into('<H',out,32,65535);out[34]=255
        wanted[at:at+80]=out;checked+=1
    if wanted!=actual[node]:
        differences=[(i,wanted[i],actual[node][i]) for i in range(len(wanted)) if wanted[i]!=actual[node][i]]
        raise AssertionError((node,len(differences),differences[:24]))
    expected.append(wanted)
assert checked==d['finite'] and d['second']==0
for c in d['preparedPeriodic']:
    rows=bytearray(base64.b64decode(c['beforeRecords']));secondary=bytearray(base64.b64decode(c['beforeSecondary']))
    for ordinal in range(c['sourceEnd']):
        for k in range(2):
            at=(ordinal*2+k)*40;cost=i32(secondary,at)
            if cost>=0x4000000:break
            struct.pack_into('<i',secondary,at,wrap(cost-c['minimum']))
        at=ordinal*88+16;cost=i32(rows,at)
        if cost<0x4000000:struct.pack_into('<i',rows,at,wrap(cost-c['minimum']))
    assert rows==base64.b64decode(c['afterRecords'])
    assert secondary==base64.b64decode(c['afterSecondary'])
assert len(d['preparedPeriodic'])==64 and d['guards']==12
print(f'PASS{checked} actual finite secondary rows/{sum(map(len,expected))} complete2410 bytes/64 prepared full-storage periodic controls; original context composition, no expected text.')
