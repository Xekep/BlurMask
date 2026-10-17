"""Independent raw-float193860 geometry over actual owned global groups."""
import ast,hashlib,json,struct,sys
from pathlib import Path
if len(sys.argv)!=4:raise ValueError('AK separation-profile capture')
ak=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
root=Path(__file__).resolve().parents[2];ns={}
tree=ast.parse((root/'tools/LegacyCursiveSeparation.Probe/reference_controls.py').read_text())
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'independent-bit-round','exec'),ns)
rounded=ns['native_round_float'];f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
table=[f(v)for v in json.loads(Path(sys.argv[2]).read_text())['values']]
assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
at=189165;gap,slope,cap=struct.unpack_from('<f',ak,at+25)[0],struct.unpack_from('<f',ak,at+185)[0],struct.unpack_from('<i',ak,at+198)[0]
owners=json.loads(Path(sys.argv[3]).read_text());groups=selected=cross=discriminators=0
for owner in owners:
    assert list(map(f,owner['profile']))==[gap,slope,cap]
    points=[[[f(v)for v in p]for p in s]for s in owner['points']]
    def evaluate(ranges,height):
        strokes=[points[s][start:end]for s,start,end in ranges];total=0
        for boundary in range(1,len(strokes)):
            left=[p[0]for s in strokes[:boundary]for p in s];right=[p[0]for s in strokes[boundary:]for p in s]
            separation=max(f(min(right)-max(left)),f(min(left)-max(right)))
            x=f(f(f(separation/height)-gap)*f(-slope))
            value=0 if x>4 else table[int(f(f(f(x+4)*255)+.5))>>3]if x>=-4 else x
            scaled=f(-value*8192);cost=int(rounded(scaled))if len(strokes)<9 else int(f(scaled+(.5 if scaled>0 else -.5)))
            total=total+cost if len(strokes)>=9 or f(f(total)+f(cost))<0x4000000 else 0x4000000
        return min(total,cap)
    for group in owner['groups']:
        height=f(group['primaryHeight'])
        if 'secondaryHeight'in group:
            baseline=max(f(group['primaryBaseline']),f(group['secondaryBaseline']))
            height=f(baseline-min(f(group['primaryBaseline']-group['primaryHeight']),f(group['secondaryBaseline']-group['secondaryHeight'])))
            cross+=1
        assert height==f(group['height'])
        expected=evaluate(group['ranges'],height);assert group['cost']==expected,(groups,expected,group['cost'])
        discriminators+=('secondaryHeight'in group and expected!=evaluate(group['ranges'],f(group['primaryHeight'])))
        groups+=1
    for row in owner['selected']:
        assert row['width']==19 and row['geometry']==owner['groups'][row['group']]['cost'];selected+=1
assert len(owners)==8 and groups==631 and cross==5 and selected==631 and discriminators==3
print(f'PASS independent8 raw-float owners/{groups} stored-range geometry groups/{cross}cross/{selected}selected19-cache rows;{discriminators} genuine primary-vs-union geometry discriminators')
