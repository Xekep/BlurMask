"""Independent raw-resource lazy scoring audit; prepared symbols, actual owners.

Never reads expected text and never executes the private native engine.
"""
import base64, hashlib, json, math, struct, sys
from pathlib import Path

if len(sys.argv)!=4:raise SystemExit('AK.res separation.json capture.json')
ak=Path(sys.argv[1]).read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
table=json.loads(Path(sys.argv[2]).read_bytes())['values']
data=json.loads(Path(sys.argv[3]).read_bytes())
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<I',struct.pack('<f',v))[0]
at=188222
version,count=struct.unpack_from('<HH',ak,at);at+=4
assert (version,count)==(0x201,192)
def ids():
    global at
    n=ak[at];at=(at+4)&~3
    result=struct.unpack_from('<'+'i'*n,ak,at);at+=4*n;return result
firstIds=ids();first=struct.unpack_from('<195b',ak,at);at+=195
geometryIds=ids();start=struct.unpack_from('<195b',ak,at);at+=195
end=struct.unpack_from('<195b',ak,at);at+=195
at+=13*len(geometryIds)
gap=struct.unpack_from('<f',ak,at+25)[0]
slope=struct.unpack_from('<f',ak,at+185)[0]
cap=struct.unpack_from('<i',ak,at+198)[0]
bypass={191,164,65535,187,188,182,180,181}
aliases={185:140,186:140,183:140,187:140,188:140,193:134,194:129}
strokes=[];offset=0
for n in data['counts']:strokes.append(data['points'][offset:offset+n]);offset+=n
assert offset==len(data['points'])
def separation(group):
    if len(group['ranges'])==1:return 0
    xs=[[p[0] for p in strokes[s][a:b]] for s,a,b in group['ranges']]
    total=0
    for i in range(1,len(xs)):
        left=[x for line in xs[:i] for x in line];right=[x for line in xs[i:] for x in line]
        distance=max(f(min(right)-max(left)),f(min(left)-max(right)))
        value=f(f(f(distance/data['height'])-gap)*f(-slope))
        log=0 if value>4 else table[int(f(f(f(value+4)*255)+.5))>>3] if value>=-4 else value
        cost=f(f(-log)*8192)
        rounded=int(math.floor(cost+.5)) if len(xs)<9 else int(f(cost+(.5 if cost>0 else -.5)))
        total=total+rounded if len(xs)>=9 or f(f(total)+f(rounded))<0x4000000 else 0x4000000
    return min(total,cap)
def confidence(symbol,ordinal,probabilities):
    if ordinal>=20:return 0
    #The primitive191 path is not present in the actual selected NN lists.
    assert symbol!=191
    output=aliases.get(symbol,symbol);assert output<180
    values=[]
    for family in range(4):
        center,steep=struct.unpack_from('<ff',ak,566416+8*(family*192+symbol))
        values.append(f(1/(1+math.exp((probabilities[family][output]/32768-center)*-steep))))
    return min(1.,f(f(f(f(values[1]+values[0])+values[2])+values[3])/4))
groups={g['group']:g for g in data['groups']}
expected={};maxulp=0;nonzero=0
for key,group in groups.items():
    bysymbol={}
    for index,(symbol,cost,observed) in enumerate(group['candidates']):
        calculated=bits(confidence(symbol,index,group['probabilities']))
        assert abs(observed-calculated)<=2,(key,symbol,index,observed,calculated)
        maxulp=max(maxulp,abs(observed-calculated));bysymbol.setdefault(symbol,(cost,observed))
    geom=separation(group);nonzero+=geom!=0
    expected[key]=(bysymbol,geom,group['candidates'][0][2] if group['candidates'] else 0)
controls=refusals=higher=0
for source in data['sources']:
    candidates,geometry,firstScore=expected[source['group']]
    assert source['first']==firstScore
    higher+=source['higher']
    assert source['base']&0xffffff==source['node'] and (source['base']>>24)&65535==source['arc']
    for case in source['cases']:
        symbol=case['symbol'];cost,score=candidates.get(symbol,(0x4000000,0))
        assert case['symbolBits']==score
        #Native composite intern retains the raw geometry-list INDEX, not its
        #serialized external ID. An absent first-category makes either map -1.
        composite=(end[symbol] if case['end'] and end[symbol]!=-1 else start[symbol]) if first[symbol]!=-1 else -1
        if case.get('categoryRefused',False):
            assert symbol!=191 and not 0<=composite<len(geometryIds),(symbol,composite)
            refusals+=1;continue
        assert symbol==191 or 0<=composite<len(geometryIds),(symbol,composite)
        request=struct.pack('<iHHHBBBBBB',source['node'],source['arc'],65535,symbol,
                            255 if symbol==191 else composite,0,0,0,0,0)
        assert base64.b64decode(case['request16'])==request
        #There is no216BC0 unit-score fast path in this native caller.
        if cost<0x4000000 and symbol not in bypass:cost=((cost+geometry+2**31)%2**32)-2**31
        assert case['cost']==cost,(source['group'],symbol,case['cost'],cost)
        controls+=1
assert controls==data['controls'] and refusals==data['categoryRefusals']
assert higher==data['higherReads'] and data['guards']==12 and higher>0 and refusals>0
print(f'PASS independent actual refresh {len(data["sources"])} sources/{higher} higher/{controls} costs/{refusals} category refusals/{len(groups)} NN groups/{nonzero} nonzero geometry/maxULP{maxulp}/12 guards.')
