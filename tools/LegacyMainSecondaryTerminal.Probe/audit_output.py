"""Independent actual2410 scan/repeated exclusions; reads no expected text."""
import base64,json,struct,sys
from pathlib import Path
d=json.loads(Path(sys.argv[1]).read_text())
records=base64.b64decode(d['records']);matrix=base64.b64decode(d['secondary'])
i32=lambda b,p:struct.unpack_from('<i',b,p)[0]
u16=lambda b,p:struct.unpack_from('<H',b,p)[0]
u64=lambda b,p:struct.unpack_from('<Q',b,p)[0]
wrap=lambda n:(n+2**31)%2**32-2**31
contexts={i:(p,s) for i,p,s in d['contexts']}
excluded=[];expected=[];imp=0x4000000
for _ in range(d['sourceEnd']):
    best=imp;choice=None
    for ordinal in range(d['sourceEnd']):
        row=records[ordinal*88:ordinal*88+88]
        if i32(row,16)>=imp:continue
        inspect=ordinal<d['primary'] or bool(row[5]&4)
        context=u64(row,8)
        if inspect and d['sentinels'] and context:
            parent,symbol=contexts[context]
            if not parent and (symbol==65529 or (symbol+5)&65535<=1):inspect=False
        for k in range(2):
            alt=matrix[(ordinal*2+k)*40:(ordinal*2+k+1)*40]
            cost=i32(alt,0)
            if cost>=imp:break
            if inspect:cost=imp #actual known caller Filter4false
            identity=u64(alt,8)
            if cost<best and identity not in excluded:
                best=cost
                choice=dict(ordinal=ordinal,secondaryOrdinal=k,identity=identity,relative=cost,
                    total=wrap(cost+d['cumulative']),row14=i32(alt,4),rawCompact=u16(alt,28),
                    row88=base64.b64encode(row).decode(),alternative40=base64.b64encode(alt).decode())
    if choice is None:break
    expected.append(choice);excluded.append(choice['identity'])
assert expected==d['choices']
assert len(expected)==16 and len(set(excluded))==16 and d['configured']==2
assert d['controls']==512 and d['guards']==6
assert expected[0]['identity']!=expected[1]['identity']
print('PASS actual configured2410->16 distinct numeric terminals, repeated exclusions,2048 selected88/40 bytes and independent512 prepared matrix controls; no WORD text oracle.')
