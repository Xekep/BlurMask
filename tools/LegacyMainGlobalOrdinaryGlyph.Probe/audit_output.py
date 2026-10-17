"""Raw-resource ordinary216BC0, candidate vector and global193860 oracle.
All72 rows are prepared controls on actual groups, not decoded winning rows.
"""
import ast,hashlib,json,math,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'tools/LegacyRussianGlyphSymbol.Probe'))
from audit_output import read_alphabet
if len(sys.argv)!=4:raise SystemExit('AK separation capture')
ak=Path(sys.argv[1]).read_bytes();table=json.loads(Path(sys.argv[2]).read_bytes())['values'];capture=json.loads(Path(sys.argv[3]).read_bytes())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
texts=read_alphabet(ak);labels={v:k for k,v in texts.items()};assert labels['\ufffd']==189
header=struct.unpack_from('<8H',ak,99);assert header==(179,140,65535,178,182,180,181,192)
at=188222;version,base=struct.unpack_from('<HH',ak,at);at+=4;assert(version,base)==(0x201,192)
def ids():
    global at
    n=ak[at];at=(at+4)&~3;result=struct.unpack_from('<'+'i'*n,ak,at);at+=n*4;return result
first=ids();first_map=struct.unpack_from('<195b',ak,at);at+=195
geometry=ids();start=struct.unpack_from('<195b',ak,at);at+=195;end=struct.unpack_from('<195b',ak,at)
at=189165;gap=struct.unpack_from('<f',ak,at+25)[0];slope=struct.unpack_from('<f',ak,at+185)[0];cap=struct.unpack_from('<i',ak,at+198)[0]
tree=ast.parse((ROOT/'tools/LegacyCursiveSeparation.Probe/reference_controls.py').read_text());ns={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'independent-round','exec'),ns)
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
table=[f(value)for value in table]
assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
bits=lambda x:struct.unpack('<I',struct.pack('<f',x))[0]
real=lambda x:struct.unpack('<f',struct.pack('<I',x))[0]
wrap=lambda x:(x+2**31)%2**32-2**31
bypass={191,164,65535,187,188,182,180,181}
aliases={185:140,186:140,183:140,187:140,188:140,193:134,194:129}
groups=cases=partial=requests=maximum=cross=fallback_discriminators=0
assert len(capture)==8
for owner,c in enumerate(capture):
    assert c['owner']==owner
    points=[[[f(value)for value in point]for point in stroke]for stroke in c['points']]
    for g in c['groups']:
        height=f(g['primaryHeight'])
        if 'secondaryHeight'in g:
            baseline=max(f(g['primaryBaseline']),f(g['secondaryBaseline']))
            height=f(baseline-min(f(g['primaryBaseline']-g['primaryHeight']),f(g['secondaryBaseline']-g['secondaryHeight'])));cross+=1
        assert height==f(g['height'])
        strokes=[[p for p in points[s][a:z]]for s,a,z in g['ranges']];total=0
        for boundary in range(1,len(strokes)):
            left=[p[0]for s in strokes[:boundary]for p in s];right=[p[0]for s in strokes[boundary:]for p in s]
            distance=max(f(min(right)-max(left)),f(min(left)-max(right)))
            value=f(f(f(distance/height)-gap)*f(-slope));log=0 if value>4 else table[int(f(f(f(value+4)*255)+.5))>>3]if value>=-4 else value
            scaled=f(-log*8192);cost=int(ns['native_round_float'](scaled))if len(strokes)<9 else int(f(scaled+(.5 if scaled>0 else -.5)))
            total=total+cost if len(strokes)>=9 or f(f(total)+f(cost))<0x4000000 else 0x4000000
        geom=min(total,cap);assert geom==g['geometry']
        by={}
        for ordinal,(symbol,cost,observed)in enumerate(g['candidates']):
            assert cost==g['final'][symbol]
            if ordinal>=20:expected=0.
            elif symbol==191:expected=real(g['sourceProbabilityBits'])
            else:
                output=aliases.get(symbol,symbol);assert output<180
                values=[]
                for family in range(4):
                    center,steep=struct.unpack_from('<ff',ak,566416+8*(family*192+symbol))
                    values.append(f(1/(1+math.exp((g['probabilities'][family][output]/32768-center)*-steep))))
                expected=min(1.,f(f(f(f(values[1]+values[0])+values[2])+values[3])/4))
            distance=abs(bits(expected)-observed);assert distance<=2;maximum=max(maximum,distance);by.setdefault(symbol,(cost,observed,ordinal))
        fallback_discriminators+=g['final'][192]!=g['final'][179]
        for label,mode,observed in g['requests']:
            cost,score,ordinal=by.get(label,(0x4000000,0,len(by)))
            if mode==2:cost=40000
            elif label==179:cost=g['final'][192]
            elif mode==1 and ordinal>19:cost=0x4000000
            if cost<0x4000000 and label not in bypass:cost=wrap(cost+geom)
            assert observed==cost,(owner,g['group'],label,mode,cost,observed);requests+=1
        for label,mode,conversion,success,emitted,remaining,score0,score10,cost,component,external,pub,geometry_byte in g['cases']:
            cursor=1 if mode==3 else 256;score_label={193:134,194:129}.get(label,label)
            assert emitted==({193:134,194:129}.get(label,label)if conversion&255 else label)
            assert(remaining,component,external)==(cursor-1,0,0)
            if label==178 or 0xfff7<=label<=0xfffc:
                expected0=expected10=0x3f800000;expected_cost=0;expected_geometry=-1;valid=True
            else:
                expected0=bits(f([0,1,127,255][mode]/255));expected_cost,expected10,_=by.get(score_label,(0x4000000,0,-1))
                expected_geometry=255 if score_label==191 else end[score_label]if mode&1 and end[score_label]>=0 else start[score_label]
                valid=score_label==191 or first_map[score_label]>=0 and expected_geometry>=0
                if not valid:
                    assert success==0 and pub==0xa55a and geometry_byte==-1
                    assert(score0,score10,cost)==(expected0,expected10,-123456789);partial+=1;cases+=1;continue
                if score_label==179:expected_cost=g['final'][192]
                if expected_cost<0x4000000 and score_label not in bypass:expected_cost=wrap(expected_cost+geom)
            assert(success,pub,geometry_byte)==(1,0,expected_geometry)
            assert(score0,score10,cost)==(expected0,expected10,expected_cost),(label,mode,cost,expected_cost)
            cases+=1
        groups+=1
assert(groups,cases,partial,cross,fallback_discriminators)==(631,47720,2524,5,631)
print(f'PASS independent {groups} actual groups/{cases} prepared72/{partial} preserved-prefix category refusals/{requests} request16;631 genuine fallback-slot discriminators/5 cross union geometries; confidence maxULP{maximum}.')
