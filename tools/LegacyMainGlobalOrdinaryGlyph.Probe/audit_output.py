"""Raw-resource ordinary216BC0, candidate vector and global193860 oracle.
All72 rows are prepared controls on actual groups, not decoded winning rows.
"""
import ast,copy,hashlib,json,math,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'tools/LegacyRussianGlyphSymbol.Probe'))
from audit_output import read_alphabet
if len(sys.argv)!=4:raise SystemExit('AK separation capture')
ak=Path(sys.argv[1]).read_bytes();table=json.loads(Path(sys.argv[2]).read_bytes())['values']
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
def audit(capture):
    groups=cases=partial=requests=maximum=cross=fallback_discriminators=0
    assert len(capture)==8
    expected_cases=expected_partial=0
    for owner,c in enumerate(capture):
        assert c['owner']==owner
        points=[[[f(value)for value in point]for point in stroke]for stroke in c['points']]
        assert [g['group']for g in c['groups']]==list(range(len(c['groups'])))
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
            assert len(g['final'])==195
            assert len({row[0]for row in g['candidates']})==len(g['candidates']), 'candidate symbols duplicate'
            assert all(a[1]<=b[1]for a,b in zip(g['candidates'],g['candidates'][1:])), 'candidate cost order'
            expected_labels=list(dict.fromkeys([row[0]for row in g['candidates']]+[178,179,189,191,193,194,0xfff7,0xfff8,0xfff9]))
            expected_grid=[(label,mode,[0,1,256,257][mode])for label in expected_labels for mode in range(4)]
            assert [tuple(row[:3])for row in g['cases']]==expected_grid, 'full ordered label-mode-conversion grid'
            assert [tuple(row[:2])for row in g['requests']]==[(label,mode)for label in[0,178,179,180,181,182,187,188,189,191,193,194,65535]for mode in range(3)], 'full ordered request grid'
            expected_cases+=len(expected_grid)
            for label,mode,_ in expected_grid:
                if label==178 or 0xfff7<=label<=0xfffc:continue
                score_label={193:134,194:129}.get(label,label)
                geometry=255 if score_label==191 else end[score_label]if mode&1 and end[score_label]>=0 else start[score_label]
                if not(score_label==191 or first_map[score_label]>=0 and geometry>=0):expected_partial+=1
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
    assert(groups,cross,fallback_discriminators)==(631,5,631)
    assert cases==expected_cases and partial==expected_partial
    assert requests==groups*13*3
    return groups,cases,partial,requests,maximum,cross,fallback_discriminators

capture=json.loads(Path(sys.argv[3]).read_bytes())
groups,cases,partial,requests,maximum,cross,fallback_discriminators=audit(capture)
negative=[]
def fails(name,mutate,reason=None):
    changed=copy.deepcopy(capture);mutate(changed)
    try:audit(changed)
    except AssertionError as error:
        if reason is not None:assert reason in str(error),(name,str(error))
        negative.append(name)
    else:raise AssertionError(('negative accepted',name))
fails('delete-case',lambda c:c[0]['groups'][0]['cases'].pop(),'full ordered label-mode-conversion grid')
def swap(c):
    rows=c[0]['groups'][0]['cases'];rows[0],rows[1]=rows[1],rows[0]
fails('swap-modes',swap,'full ordered label-mode-conversion grid')
fails('duplicate-case',lambda c:c[0]['groups'][0]['cases'].__setitem__(1,c[0]['groups'][0]['cases'][0].copy()),'full ordered label-mode-conversion grid')
fails('delete-request',lambda c:c[0]['groups'][0]['requests'].pop(),'full ordered request grid')
def candidate_order(c):
    rows=c[0]['groups'][0]['candidates'];rows[0],rows[-1]=rows[-1],rows[0]
fails('candidate-cost-order',candidate_order,'candidate cost order')
fails('wrong-prefix-score',lambda c:c[0]['groups'][0]['cases'][0].__setitem__(6,c[0]['groups'][0]['cases'][0][6]^1))
fails('wrong-request-cost',lambda c:c[0]['groups'][0]['requests'][0].__setitem__(2,c[0]['groups'][0]['requests'][0][2]+1))
assert len(negative)==7
print(f'PASS independent {groups} actual groups/{cases} ordered-union-derived prepared72/{partial} raw-map-derived preserved-prefix category refusals/{requests} fullgrid request16;{fallback_discriminators} genuine fallback-slot discriminators/{cross} cross union geometries/7negativecaptures; confidence maxULP{maximum}.')
