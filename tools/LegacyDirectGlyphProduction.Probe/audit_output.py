"""Independent DIRECT raw-resource216BC0/geometry/calibration/full40 audit. FFFD actual type0 fallback is unchanged189.

The72 rows are prepared controls over actual graph/NN groups, never labels or
bounds asserted to have been produced by a winning decoder path.
"""
import base64,hashlib,importlib.util,json,math,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('glyph_symbol_alphabet',ROOT/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
if len(sys.argv)!=4:raise SystemExit('AK.res separation.json capture.json')
ak=Path(sys.argv[1]).read_bytes();table=json.loads(Path(sys.argv[2]).read_bytes())['values'];capture=json.loads(Path(sys.argv[3]).read_bytes())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
header=struct.unpack_from('<8H',ak,99);assert header==(179,140,65535,178,182,180,181,192)
texts=module.read_alphabet(ak);labels={v:k for k,v in texts.items()}
def f(value):return struct.unpack('<f',struct.pack('<f',value))[0]
def bits(value):return struct.unpack('<I',struct.pack('<f',value))[0]
def write16(row,at,value):struct.pack_into('<H',row,at,value&65535)
def write32(row,at,value):struct.pack_into('<I',row,at,value&0xffffffff)
#Read the actual main category node independently, retaining absolute padding.
at=188222;version,base=struct.unpack_from('<HH',ak,at);at+=4
assert version==0x201 and base==192
def ids():
    global at
    n=ak[at];at=(at+1+3)&~3
    values=struct.unpack_from('<'+'i'*n,ak,at);at+=n*4;return values
first=ids();firstMap=struct.unpack_from('<'+'b'*(base+3),ak,at);at+=base+3
geometry=ids();start=struct.unpack_from('<'+'b'*(base+3),ak,at);at+=base+3
end=struct.unpack_from('<'+'b'*(base+3),ak,at);at+=base+3
at+=len(geometry)*13
gap=struct.unpack_from('<f',ak,at+25)[0];slope=struct.unpack_from('<f',ak,at+185)[0];cap=struct.unpack_from('<i',ak,at+198)[0]
bypass={191,164,65535,187,188,182,180,181}
aliases={185:140,186:140,183:140,187:140,188:140,193:134,194:129}
def confidence(symbol,ordinal,probabilities):
    if ordinal>=20:return 0.
    #This real fixture has no retained primitive; no source probability is
    #invented to pretend it does. RU confidence probe owns that separate branch.
    assert symbol!=191
    output=aliases.get(symbol,symbol);assert output<180
    values=[]
    for family in range(4):
        center,steep=struct.unpack_from('<ff',ak,566416+8*(family*192+symbol))
        values.append(f(1/(1+math.exp((probabilities[family][output]/32768-center)*-steep))))
    return min(1.,f(f(f(f(values[1]+values[0])+values[2])+values[3])/4))
def separation(source,group):
    if len(group['ranges'])==1:return 0
    strokes=[];offset=0
    for n in source['counts']:strokes.append(source['points'][offset:offset+n]);offset+=n
    xsets=[[p[0] for p in strokes[s][begin:finish]] for s,begin,finish in group['ranges']]
    total=0
    for i in range(1,len(xsets)):
        left=[x for line in xsets[:i] for x in line];right=[x for line in xsets[i:] for x in line]
        distance=max(f(min(right)-max(left)),f(min(left)-max(right)))
        value=f(f(f(distance/source['height'])-gap)*f(-slope))
        log=0 if value>4 else table[int(f(f(f(value+4)*255)+.5))>>3] if value>=-4 else value
        cost=f(f(-log)*8192)
        rounded=int(math.floor(cost+.5)) if len(xsets)<9 else int(f(cost+(.5 if cost>0 else -.5)))
        total=total+rounded if len(xsets)>=9 or f(f(total)+f(rounded))<0x4000000 else 0x4000000
    return min(total,cap)
count=0;groups=0;ulp=0;nonzeroGeometry=0;quoteCases=0;specialCases=0
assert len(capture['sources'])==2
for source in capture['sources']:
    assert source['name'] in ('onePhysical','twoPhysical')
    for group in source['groups']:
        groups+=1;costGeometry=separation(source,group);nonzeroGeometry+=costGeometry!=0
        bySymbol={}
        for ordinal,(symbol,cost,observedConfidence) in enumerate(group['candidates']):
            expected=bits(confidence(symbol,ordinal,group['probabilities']))
            distance=abs(observedConfidence-expected);assert distance<=2,(symbol,ordinal,distance)
            ulp=max(ulp,distance);bySymbol.setdefault(symbol,(cost,observedConfidence))
        for case in group['cases']:
            row=bytes.fromhex(case['source']);symbol=struct.unpack_from('<H',row,0x3a)[0]
            assert len(row)==72 and row[0x3c]==0
            compact=bytes.fromhex(case['compact40']);index=struct.unpack_from('<i',row,0x20)[0]
            assert index==-1 and not compact or index==0 and len(compact)==40
            emitted={193:134,194:129}.get(symbol,symbol) if case['conversion']&255 else symbol
            scoreSymbol={193:134,194:129}.get(symbol,symbol)
            if symbol==178 or 0xfff7<=symbol<=0xfffc:
                expected0=expected10=0x3f800000;expectedCost=0;specialCases+=1
            else:
                expected0=bits(f(row[0x3e]/255));expectedCost,expected10=bySymbol.get(scoreSymbol,(0x4000000,0))
                #Real selected candidates exclude blank179. Never substitute
                #a class cost for its separate fallback group18 cache channel.
                assert scoreSymbol!=179
                if expectedCost<0x4000000 and scoreSymbol not in bypass:
                    index=end[scoreSymbol] if row[0x3d] and firstMap[scoreSymbol]>=0 and end[scoreSymbol]>=0 else start[scoreSymbol]
                    assert firstMap[scoreSymbol]>=0 and 0<=index<len(geometry)
                    expectedCost=((expectedCost+costGeometry+2**31)%2**32)-2**31
            assert (case['score0C'],case['score10'],case['cost'])==(expected0,expected10,expectedCost),(source['name'],group['group'],symbol,case)
            assert (case['remaining'],case['component'],case['external'],case['publication'])==(case['cursor']-1,0,0,0)
            scratch=[0x8000+i*3 for i in range(256)];scratch[case['cursor']-1]=emitted
            assert base64.b64decode(case['labels'])==struct.pack('<256H',*scratch)
            expected40=bytearray.fromhex(case['before40']);flags=struct.unpack_from('<H',expected40,4)[0]
            componentCount=256-case['remaining'];storedSymbol=emitted;kind=0
            if 0xfff9<=storedSymbol<=0xfffc:
                kind=1;storedSymbol=(((componentCount&8191)<<2)|((storedSymbol-1)&3))&65535
            write16(expected40,4,((flags&0xfff8)|kind)&0xfff7|(8 if row[0x3d] else 0))
            write16(expected40,6,0);write16(expected40,8,storedSymbol)
            write32(expected40,12,expected0);write32(expected40,16,expected10);write32(expected40,20,expectedCost)
            assert case['finalized'] and case['after40'].lower()==expected40.hex(),(symbol,case['after40'],expected40.hex())
            quoteCases+=symbol in (193,194);count+=1
#An additional integration uses genuine selected compact -> numeric72 rows.
#Locate their genuine graph group through the independently captured endpoint
#controls, then independently recompute raw byte confidence and actualNN/cost.
assert capture['actualOrdinaryRows']
for observed in capture['actualOrdinaryRows']:
    row=bytes.fromhex(observed['row']);node,arc=struct.unpack_from('<iH',row)
    symbol=struct.unpack_from('<H',row,58)[0];assert row[60]==0
    source=capture['sources'][0]
    matching=[g for g in source['groups'] if struct.unpack_from('<iH',bytes.fromhex(g['cases'][0]['source']))==(node,arc)]
    assert len(matching)==1
    group=matching[0];bySymbol={s:(c,conf) for s,c,conf in group['candidates']}
    scoreSymbol={193:134,194:129}.get(symbol,symbol)
    expectedCost,expected10=bySymbol.get(scoreSymbol,(0x4000000,0))
    if expectedCost<0x4000000 and scoreSymbol not in bypass:
        expectedCost=((expectedCost+separation(source,group)+2**31)%2**32)-2**31
    assert (observed['score0C'],observed['score10'],observed['cost'])==(bits(f(row[62]/255)),expected10,expectedCost)
    scratch=[0x8000+i*3 for i in range(256)];scratch[255]=symbol
    assert observed['remaining']==255 and base64.b64decode(observed['labels'])==struct.pack('<256H',*scratch)
assert count==capture['controls']==3288 and groups==46
assert capture['applied']==count and capture['guards']==25 and capture['partialFailures']==3
assert nonzeroGeometry>0 and quoteCases>0 and specialCases>0
print(f'PASS independent {count} complete scratch/score/cost/full40 outputs, {groups} actualNN groups, {nonzeroGeometry} nonzero separation groups, {quoteCases} synthetic quotes/{specialCases} unit-score cases; confidence maxULP{ulp}; prepared72 only.')
