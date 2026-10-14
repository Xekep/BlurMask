"""Independent actual-NN qualifying gaps ->29B EM ->265 audit."""
import ast,base64,json,struct,sys
from pathlib import Path
import numpy as np
from reference_controls import active
if len(sys.argv)!=3:raise ValueError('AK.res actual-output.json')
ak=Path(sys.argv[1]).read_bytes();got=json.loads(Path(sys.argv[2]).read_text(encoding='utf8'));intervals=[];physical=[]
for encoded in got['descriptors']:
 b=base64.b64decode(encoded)
 if struct.unpack_from('<H',b)[0]&15 not in [0,6]:continue
 intervals.append(tuple(struct.unpack_from('<i',b,k)[0] for k in [16,52,56]));physical.append(tuple(struct.unpack_from('<i',b,k)[0] for k in [36,40]))
heights=[struct.unpack_from('<f',base64.b64decode(b),4)[0] for b in got['guides']]
node=ak[188050:188108];profile=[*node[40:42],*struct.unpack_from('<4f',node,42)];gaussian=list(struct.unpack_from('<6f',node,16))
expected=active(profile,gaussian,[b[:2] for b in got['boxes']],intervals,heights)
for key in ['gap','blend','mean','variance','qualifying','observations']:np.testing.assert_allclose(got[key],expected[key],rtol=2e-6,atol=2e-6,err_msg=key)
np.testing.assert_allclose(got['intersection'],expected['mixture']['intersection'],rtol=2e-6,atol=2e-6)
assert len(got['attempts'])==len(expected['mixture']['attempts'])
for a,b in zip(got['attempts'],expected['mixture']['attempts']):
 for key in b:np.testing.assert_allclose(a[key],b[key],rtol=2e-6,atol=2e-6,err_msg=key)
folder=Path(__file__).resolve().parents[1];env={};source=folder/'LegacyOrientationSmoothing.Probe'/'reference_controls.py'
tree=ast.parse(source.read_text(encoding='utf8'));defs=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef)) or(isinstance(n,ast.Assign)and isinstance(n.value,ast.Lambda))]
exec(compile(ast.Module(body=defs,type_ignores=[]),str(source),'exec'),env)
source=folder/'LegacySmoothingPointKeys.Probe'/'reference_controls.py';tree=ast.parse(source.read_text(encoding='utf8'))
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)],type_ignores=[]),str(source),'exec'),env)
metrics=env['metrics'](got['parentPoints']);np.testing.assert_allclose(got['metrics'],metrics,rtol=2e-7,atol=2e-7)
smoothing=list(struct.unpack_from('<8f',ak,188124));points=[];keys=[];offset=0
for (line,start,end),(first,last) in zip(intervals,physical):
 subset=got['beforePoints'][first:last+1];coords,ticks,_,_=env['keys'](subset,smoothing,heights[line],metrics)
 points.extend(coords);keys.extend(tick+offset*200 for tick in ticks);offset+=sum(map(len,subset))
assert len(points)==len(got['points'])
for i,(a,b) in enumerate(zip(got['points'],points)):np.testing.assert_allclose(a,b,rtol=2e-5,atol=2e-4,err_msg=f'stroke{i}')
assert keys==got['inputKeys200'];previous=got['beforeMap'];mapped=[]
for tick in keys:
 whole,remainder=divmod(tick,200);delta=(((previous[whole+1]-previous[whole])&0xffffffff)*remainder&0xffffffff)//200 if remainder else 0
 mapped.append((previous[whole]+delta)&0xffffffff)
assert mapped==got['mappedKeys200']
print(f'PASS real14 NN720 -> qualifying{len(got["qualifying"])} gaps/blend{got["blend"]:.5g} ->{len(got["attempts"])} nativeEM attempts/gap{got["gap"]:.8g} ->dense265 {len(keys)} points and exact original keys.')
