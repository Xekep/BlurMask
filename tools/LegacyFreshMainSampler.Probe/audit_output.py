"""Independent scalar191950 oracle and source200-map/short conversion audit.
Only previously reviewed Python oracle definitions are loaded; no C# expected
output, vendor executable or dataset label is used to correct inference.
"""
import json, math, struct, sys
from pathlib import Path

root=Path(__file__).resolve().parents[2]
source=root/'tools/LegacyCursiveProducer.Probe/reference_controls.py'
text=source.read_text(encoding='utf8')
space={'__file__':str(source)}
exec(compile(text[:text.index('rng=random.Random')],str(source),'exec'),space)
body=text[text.index(' for stroke,raw in enumerate(strokes):'):text.index('\nrng=random.Random')]
initial='''def compute(strokes,height,baseline,confidence):
 tags=[1 if i==0 else 0 for s in strokes for i in range(len(s))]
 state=Stage(strokes,tags,0);whole=False;cap=None;previous=f(-3.4028234663852886e38)
'''
exec(compile(initial+body,'independent-owned-main-cuts','exec'),space)
bits=space['bits'];f=space['f']
p=json.loads(Path(sys.argv[1]).read_text())
assert p['mainGraphCap']==8 and p['target']==100 and p['height']==40 and p['baseline']==90
assert p['sourceProbability']==0 and p['mainGroups']==10
expected=space['compute']([[list(map(f,point)) for point in p['smoothed']]],f(p['height']),f(p['baseline']),[f(p['sourceProbability'])])
rows=expected['expected'];assert len(rows)==len(p['cutRows'])
for actual,reference in zip(p['cutRows'],rows):
 assert [bits(v) for v in actual[:5]]==reference[:5],(actual,reference)
 assert actual[5]==reference[5]
# The incoming sampler map uses integer OLD prepared indices, composing the
# actual265 map; it does not regenerate keys from resampled stroke ordinals.
ticks=[p['keys200'][row[7]] for row in rows]
assert ticks==p['cutMap']
retained_raw=[0,2,3,4,5]
for tick,key in zip(ticks,p['cutSourceKeys']):
 whole,remainder=divmod(tick,200)
 assert key==[0,0,retained_raw[whole+bool(remainder)],200-remainder if remainder else 0],(tick,key)
count=0
for group in p['normalized']:
 assert len(group['ranges'])==1
 stroke,start,end=group['ranges'][0];assert stroke==0
 chosen=p['cutRows'][start:end];left=min(point[0] for point in chosen)
 expected_short=[]
 for point in chosen:
  x=f(f(f(point[0]-left)*p['target'])/p['height'])
  y=f(f(f(point[1]-p['baseline'])*p['target'])/p['height'])
  expected_short.append([min(32767,math.trunc(x)),min(32767,math.trunc(y))])
 assert group['points']==expected_short
 count+=len(expected_short)
print(f'PASS independent complete191950:{len(rows)} rows, composed200 keys, {len(p["normalized"])} actual main groups/{count} normalized shorts')
