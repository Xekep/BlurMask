"""Independent29B + dense-spline265 + uint32 point-map composition audit."""
import ast,base64,json,struct,sys
from pathlib import Path
import numpy as np
from reference_controls import oracle
if len(sys.argv)!=3:raise ValueError('AK.res actual-output.json')
ak=Path(sys.argv[1]).read_bytes();data=json.loads(Path(sys.argv[2]).read_text(encoding='utf8'));got=data['postprocessing']
ds=[];physical=[]
for encoded in data['descriptors']:
 b=base64.b64decode(encoded)
 if struct.unpack_from('<H',b)[0]&15 not in [0,6]:continue
 ds.append(tuple(struct.unpack_from('<i',b,k)[0] for k in [16,52,56]));physical.append(tuple(struct.unpack_from('<i',b,k)[0] for k in [36,40]))
guides=[struct.unpack_from('<f',base64.b64decode(g),4)[0] for g in data['guides']]
profile=[*ak[188090:188092],*struct.unpack_from('<4f',ak,188092)]
expected=oracle([b[:2] for b in data['shear']['boxes']],ds,guides,-1.,profile)
assert expected is not None
for key in ['gap','assigned','mean','variance','observations']:np.testing.assert_allclose(got[key],expected[key],rtol=2e-7,atol=2e-7,err_msg=key)
#Reuse the original independent dense NumPy spline solver definitions only;
#the generators' sample corpus and module side effects are not executed.
folder=Path(__file__).resolve().parents[1];env={}
source=folder/'LegacyOrientationSmoothing.Probe'/'reference_controls.py';parsed=ast.parse(source.read_text(encoding='utf8'))
defs=[n for n in parsed.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef)) or (isinstance(n,ast.Assign) and isinstance(n.value,ast.Lambda))]
exec(compile(ast.Module(body=defs,type_ignores=[]),str(source),'exec'),env)
source=folder/'LegacySmoothingPointKeys.Probe'/'reference_controls.py';parsed=ast.parse(source.read_text(encoding='utf8'))
exec(compile(ast.Module(body=[n for n in parsed.body if isinstance(n,ast.FunctionDef)],type_ignores=[]),str(source),'exec'),env)
metrics=env['metrics'](data['parentPoints']);np.testing.assert_allclose(got['metrics'],metrics,rtol=2e-7,atol=2e-7)
points=[];keys=[];offset=0
for (line,start,end),(first,last) in zip(ds,physical):
 subset=data['shear']['points'][first:last+1]
 coords,localkeys,derived,weight=env['keys'](subset,got['profile'],guides[line],metrics)
 points.extend(coords);keys.extend(k+offset*200 for k in localkeys);offset+=sum(map(len,subset))
assert len(points)==len(got['points'])
for i,(a,b) in enumerate(zip(got['points'],points)):np.testing.assert_allclose(a,b,rtol=2e-5,atol=2e-4,err_msg=f'stroke{i}')
assert keys==got['inputKeys200'],'Exact265 fractional keys differ.'
previous=got['beforeMap'];composed=[]
for tick in keys:
 whole,remainder=divmod(tick,200)
 delta=(((previous[whole+1]-previous[whole])&0xffffffff)*remainder&0xffffffff)//200 if remainder else 0
 composed.append((previous[whole]+delta)&0xffffffff)
assert composed==got['mappedKeys200'],'Native uint32 point-map composition differs.'
print(f'PASS actual6physical/2descriptor ->29B no-EM gap{got["gap"]}/assigned{got["assigned"]} ->dense265 {len(keys)} points/exact fractional200+original chronology; no suppliedgap or replaced source720.')
