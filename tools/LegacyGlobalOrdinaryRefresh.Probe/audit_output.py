"""Independent raw-AK, compact/request, full pool and ordered-prefix oracle.

The captured NN probabilities/final class costs are inputs already audited by
the owning global acoustic stage. This audit does not run a second NN.
"""
import argparse,base64,hashlib,importlib.util,json,math,struct
from pathlib import Path

root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('acoustic',type=Path);p.add_argument('separation',type=Path);p.add_argument('capture',type=Path)
a=p.parse_args();ak=a.acoustic.read_bytes();d=json.loads(a.capture.read_bytes())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('raw_alphabet',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
al=importlib.util.module_from_spec(spec);spec.loader.exec_module(al);alphabet=al.read_alphabet(ak)
dec=base64.b64decode
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
real=lambda b,o=0:struct.unpack_from('<f',b,o)[0]
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
bits=lambda x:struct.unpack('<I',struct.pack('<f',x))[0]
frombits=lambda x:real(struct.pack('<I',x))
wrap=lambda x:(x+2**31)%2**32-2**31
assert struct.unpack_from('<8H',ak,99)==(179,140,65535,178,182,180,181,192)
at=188222;assert struct.unpack_from('<HH',ak,at)==(0x201,192);at+=4
def ids():
    global at
    n=ak[at];at=(at+4)&~3;v=struct.unpack_from('<'+'i'*n,ak,at);at+=n*4;return v
first=ids();first_map=struct.unpack_from('<195b',ak,at);at+=195
geometry_ids=ids();start=struct.unpack_from('<195b',ak,at);at+=195;end=struct.unpack_from('<195b',ak,at)
gap=real(ak,189165+25);slope=real(ak,189165+185);cap=i32(ak,189165+198)
table=[f(v)for v in json.loads(a.separation.read_bytes())['values']]
assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
def native_round(v):
    n=bits(v);m=n&0x7fffffff
    if m>=0x4b000000:return v
    if m<0x3f800000:return frombits((n&0x80000000)|(0x3f800000 if m>=0x3f000000 else 0))
    shift=150-(m>>23);return frombits(((n>>shift)+((n>>(shift-1))&1))<<shift)
bypass={191,164,65535,187,188,182,180,181}
aliases={185:140,186:140,183:140,187:140,188:140,193:134,194:129}
def append(residue,descriptor):
    row=bytearray(residue);struct.pack_into('<i',row,0,descriptor)
    struct.pack_into('<H',row,4,u16(row,4)&0xfff0);struct.pack_into('<I',row,6,0xffff0000)
    struct.pack_into('<f',row,12,1);struct.pack_into('<f',row,16,-1);struct.pack_into('<i',row,20,0)
    row[24:32]=bytes(8);row[32:40]=bytes([255])*8;return row
def symbol_update(row,symbol,external=0):
    kind=4 if external else 0
    if not external and 0xfff9<=symbol<=0xfffc:kind=1;symbol=4|((symbol-1)&3)
    struct.pack_into('<H',row,4,(u16(row,4)&0xfff8)|kind);struct.pack_into('<H',row,8,symbol)
synthetic={65532:'\u00a0',65531:' ',65529:'\n',65526:'\U000f0000'}
synthetic.update({65523-i:chr(0xf0010+i)for i in range(9)})
def emit(flags,symbol):
    kind=flags&7
    if kind==4:return None
    count=((symbol>>2)&8191)if kind in(1,3)else 1
    if count==0:return b''
    if kind==3:return None if symbol&3==1 else '\ufffd'.encode()*count
    if kind==1:symbol=((symbol&3)-7)&65535
    elif kind not in(0,2):symbol=65535
    if symbol<192 and symbol not in alphabet:return None
    text=alphabet.get(symbol,synthetic.get(symbol,'\ufffd'))
    assert not('\U000f0000'in text and '\U000f0001'in text)
    return text.encode()*count

owners=visited=pool_bytes=requests=positive=maxulp=geometry_discriminators=0
for sample in d['samples']:
    compact=dec(sample['compact']);assert len(compact)%40==0
    points=[[[f(v)for v in point]for point in stroke]for stroke in sample['points']]
    assert sample['nn']>0
    for owner in sample['owners']:
        owners+=1;base=owner['base'];node=base&0xffffff;arc=(base>>24)&65535
        left,right=sample['starts'][node:node+2]
        matches=[i for i in range(left,right)if compact[i*40+32]&0x27==0x20 and u64(compact,i*40+8)==base]
        assert matches==[q['compact']for q in owner['queries']]
        assert len(matches)==owner['visited'];visited+=len(matches)
        strokes=[points[s][lo:hi]for s,lo,hi in owner['ranges']];height=f(owner['height']);total=0
        for boundary in range(1,len(strokes)):
            x1=[p[0]for stroke in strokes[:boundary]for p in stroke];x2=[p[0]for stroke in strokes[boundary:]for p in stroke]
            distance=max(f(min(x2)-max(x1)),f(min(x1)-max(x2)))
            value=f(f(f(distance/height)-gap)*f(-slope))
            #Actual corrected sequence: F32(+4), then F64 multiply/add.
            log=0 if value>4 else table[int(f(value+4)*255+.5)>>3]if value>=-4 else value
            scaled=f(-log*8192);cost=int(native_round(scaled))if len(strokes)<9 else int(f(scaled+(.5 if scaled>0 else -.5)))
            total=total+cost if len(strokes)>=9 or f(f(total)+f(cost))<0x4000000 else 0x4000000
        geom=min(total,cap);assert geom==owner['geometry'];positive+=geom!=0
        by={}
        for ordinal,(symbol,cost,observed)in enumerate(owner['candidates']):
            assert cost==owner['final'][symbol]
            if ordinal>=20:expected=0.
            elif symbol==191:expected=frombits(owner['sourceProbabilityBits'])
            else:
                output=aliases.get(symbol,symbol);assert output<180
                values=[]
                for family in range(4):
                    center,steep=struct.unpack_from('<ff',ak,566416+8*(family*192+symbol))
                    values.append(f(1/(1+math.exp((owner['probabilities'][family][output]/32768-center)*-steep))))
                expected=min(1.,f(f(f(f(values[0]+values[1])+values[2])+values[3])/4))
            ulp=abs(bits(expected)-observed);assert ulp<=2;maxulp=max(maxulp,ulp)
            by.setdefault(symbol,(cost,observed))
        assert owner['firstBits']==owner['candidates'][0][2]
        #Source26 layout is24N references followed by2N residuals; independent
        #range chronology bytes are covered by the global source-metadata audit.
        source=dec(owner['source26']);assert source and len(source)%26==0
        rows=[bytearray(dec(owner['before'])[i:i+40])for i in range(0,len(dec(owner['before'])),40)]
        before_count=len(rows)
        for index,q in zip(matches,owner['queries']):
            part=compact[index*40:(index+1)*40];label=u16(part,28)
            #These ACTUAL captured compact labels need neither fallback nor quote
            #conversion: no input expected-text or synthetic label substitution.
            assert label!=189 and q['emitted']==q['normalized']==label and q['external']==0
            descriptor=index-left;found=next((n for n,row in enumerate(rows)if i32(row,0)==descriptor),None)
            if owner['beforePacked']&0x8000:found=None
            if found is None:found=len(rows);rows.append(append(dec(owner['residue']),descriptor))
            row=rows[found];symbol_update(row,q['emitted'],q['external'])
            expected_cost,expected_bits=by.get(label,(0x4000000,0))
            if label==179:expected_cost=owner['final'][192]
            if expected_cost<0x4000000 and label not in bypass:expected_cost=wrap(expected_cost+geom)
            assert q['symbolBits']==expected_bits and q['cost']==expected_cost
            geometry=255 if label==191 else end[label]if part[32]&0x40 and end[label]>=0 else start[label]
            assert label==191 or first_map[label]>=0 and geometry>=0
            request=struct.pack('<iHHHB5x',node,arc,65535,label,geometry)
            assert dec(q['request'])==request
            struct.pack_into('<I',row,16,q['symbolBits']);struct.pack_into('<i',row,20,q['cost'])
            struct.pack_into('<f',row,12,part[33]/255)
            struct.pack_into('<H',row,4,(u16(row,4)&0xfff7)|(8 if part[32]&0x40 else 0))
            requests+=1;geometry_discriminators+=geom!=0 and label not in bypass
        expected=b''.join(rows);assert expected==dec(owner['after']);pool_bytes+=len(expected)
        assert len(rows)==before_count+len(matches),'Initial IDs must differ: selected-only emission cannot pass'
        assert owner['afterPacked']==(len(rows)<<2)|(owner['beforePacked']&0x8000)
        identity=lambda row:(u16(row,4)&7,u16(row,8),u16(row,6)&0xff00)
        winners={}
        for n,row in enumerate(rows):
            assert math.isfinite(real(row,12));key=identity(row)
            if key not in winners or real(row,12)>real(rows[winners[key]],12):winners[key]=n
        order=sorted(winners.values(),key=lambda n:(-real(rows[n],12),n))
        selection=dec(owner['selection']);assert list(struct.unpack('<'+'i'*(len(selection)//4),selection))==order
        original=(owner['reference']>>40)&65535;original=original-65536 if original>=32768 else original
        assert original>=0 and owner['marker']==order.index(winners[identity(rows[original])])
        assert len(owner['utf8'])==len(rows)
        for row,observed in zip(rows,owner['utf8']):assert emit(u16(row,4),u16(row,8))==dec(observed)
assert(len(d['samples']),owners,visited,d['guards'])==(8,12,24,92)
assert d['owners']==owners and d['visited']==visited and positive==2 and geometry_discriminators==4
for control in d['preparedPrefixes']:
    fail=control['failure'];initial=dec(control['before']);new=append(dec(control['residue']),0)
    trace=[('resolve',initial+new)]
    if fail!=1:
        symbol_update(new,75);trace.append(('score',initial+new))
        if fail!=2:
            struct.pack_into('<I',new,16,0x3f123456);trace.append(('cost',initial+new))
            if fail!=3:
                struct.pack_into('<i',new,20,12345);struct.pack_into('<f',new,12,127/255)
                struct.pack_into('<H',new,4,u16(new,4)|8)
    assert control['succeeded']==(fail==0) and dec(control['after'])==initial+new
    assert[(e['stage'],dec(e['records']))for e in control['trace']]==trace
assert len(d['preparedPrefixes'])==4
for row in d['preparedEmission']:
    assert emit(row['flags'],row['symbol'])==(None if row['utf8']is None else dec(row['utf8']))
assert len(d['preparedEmission'])==7
print(f'PASS rawAK {owners} actual owners/{requests} requests/{visited} appends/{pool_bytes} pool bytes; {positive} positive geometry owners/{geometry_discriminators} cost discriminators; confidence maxULP{maxulp};4 full write/query prefixes/7 separate prepared emissions. No extra NN or OCR-target input.')
