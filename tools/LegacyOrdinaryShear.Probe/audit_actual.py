"""Recompute owned actual-NN72024D result with the independent NumPy oracle."""
import base64,json,struct,sys
from pathlib import Path
from reference_controls import alphabet,oracle
if len(sys.argv)!=3:raise ValueError('AK.res actual-output.json')
ak=Path(sys.argv[1]).read_bytes();data=json.loads(Path(sys.argv[2]).read_text(encoding='utf8'))
descriptors=[]
for encoded in data['descriptors']:
 b=base64.b64decode(encoded)
 if struct.unpack_from('<H',b)[0]&15 not in [0,6]:continue
 descriptors.append(tuple(struct.unpack_from('<i',b,k)[0] for k in [16,52,56,36,40]))
guides=[struct.unpack_from('<ff',base64.b64decode(b)) for b in data['guides']]
rows=[]
for b,ranges in zip(data['source720'],data['ranges']):
 b=base64.b64decode(b);rows.append(dict(ranges=ranges,box=list(struct.unpack_from('<4f',b,24)),label=struct.unpack_from('<h',b,40)[0],weight=struct.unpack_from('<f',b,80)[0]))
minimum,maximum=ak[188028:188030];floor,normalizer,a,b,angle=struct.unpack_from('<5f',ak,188030)
import numpy as np,math
f=np.float32;lo=f(math.tan(a));hi=f(math.tan(b));step=f(f(hi-lo)/f(f(f(b)-f(a))/f(angle)))
expected=oracle(data['currentPoints'],descriptors,guides,rows,[-1]*len(data['currentPoints']),[minimum,maximum,floor,normalizer,lo,hi,step],alphabet(ak))
got=data['shear']
for key in ['blend','low','high','gap','direction','shear','iterations','boxes']:
 np.testing.assert_allclose(got[key],expected[key],rtol=4e-6,atol=4e-6,err_msg=key)
assert len(got['points'])==len(expected['points'])
for i,(a,b) in enumerate(zip(got['points'],expected['points'])):np.testing.assert_allclose(a,b,rtol=4e-6,atol=4e-6,err_msg=f'physical{i}')
print(f'PASS actual {len(rows)} NN720/{len(descriptors)} descriptor input -> owned24D: {got["iterations"]} grid steps, shear {got["shear"]:.8g}; memberships/labels unmodified.')
