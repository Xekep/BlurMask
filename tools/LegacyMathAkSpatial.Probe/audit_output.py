from pathlib import Path
import sys,hashlib,struct,json,copy
sys.path.insert(0,str(Path(__file__).resolve().parent))
from reference_temporal import normalize
from reference_spatial import spatial
repo,capture,output=map(Path,sys.argv[1:]);raw=(repo/'models/math/math-ak.res').read_bytes();H=lambda b:hashlib.sha256(b).hexdigest()
assert H(raw)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
assert struct.unpack_from('<H',raw,1006005)[0]==0x1211 and struct.unpack_from('<4H',raw,1006011)==(30,16,8,10)
assert struct.unpack_from('<I',raw,1151097+11)[0]==204
mi=struct.unpack_from('<412h',raw,1006085);ma=struct.unpack_from('<412h',raw,1006909)
names=['horizontal','vertical','nonsquare','fractional','duplicates','zigzag','closed-loop','reverse-slope','cross','triangle','diagonal-ties','parallel-bars','full-raster']
guards=['equal-data-foreign-issuer','values-detached','cancel-exact-token','degenerate-refused']
def check(r):
 assert r['modelExecutions']==0 and r['vendorExecution']is False and r['guards']==guards
 assert [q['name']for q in r['rows']]==names
 assert [q['value']for q in r['countControls']]==[0,8,256,512,528,1024]
 for q in r['countControls']:
  v=((q['value']*64-1024+32768)%65536)-32768
  assert q['result']==max(-1024,min(1024,v))
 assert r['countControls'][-1]['result']==-1024 #late32bit clamp would be+1024
 for row in r['rows']:
  points=normalize(row['original']);assert points==row['normalized']
  values,count,_=spatial(points,mi,ma)
  assert row['values']==values,(row['name'],[(i,x,y)for i,(x,y)in enumerate(zip(row['values'],values))if x!=y][:15])
  assert row['RasterPixelCount']==count and row['FeatureCount']==204 and row['ModelStart']==1151097
  assert row['HasCandidateAuthority']is False and row['HasClassifierOutput']is False
 #Actual both-nonzero source extent8192 -> integergrid factor15 ->max120.
 #This dense source covers every reachable row/column, not all128bitmapcells.
 target=struct.unpack_from('<H',raw,1005998)[0];assert target==8192
 gridMaximum=target*((127*1024)//target)>>10
 assert gridMaximum==120
 assert r['rows'][-1]['RasterPixelCount']==(gridMaximum+1)**2
r=json.loads(capture.read_text());check(r);negative=[]
for name,mut in [('source',lambda q:q['rows'][0]['original'][0].__setitem__(0,2)),('grid',lambda q:q['rows'][0]['normalized'][0].__setitem__(0,2)),('global',lambda q:q['rows'][0]['values'].__setitem__(0,123)),('counts',lambda q:q['rows'][0]['values'].__setitem__(10,123)),('distance',lambda q:q['rows'][0]['values'].__setitem__(110,123)),('boundary',lambda q:q['rows'][0]['values'].__setitem__(203,123)),('raster-bound',lambda q:q['rows'][-1].__setitem__('RasterPixelCount',14640)),('model',lambda q:q['rows'][0].__setitem__('ModelStart',1007739)),('authority',lambda q:q['rows'][0].__setitem__('HasCandidateAuthority',True)),('count-short-before-clamp',lambda q:q['countControls'][-1].__setitem__('result',1024)),('guard',lambda q:q['guards'].pop())]:
 q=copy.deepcopy(r);mut(q)
 try:check(q)
 except AssertionError:negative.append(name)
 else:raise AssertionError(name)
report=dict(captureSHA256=H(capture.read_bytes()),rows=13,features=2652,countControls=6,guards=4,corruptions=negative,maximumCoveredRasterPixels=14641,conservativeRasterCapacity=16384,modelExecutions=0,vendorExecution=False,scope='Actual same-source configured1211 absent-guide spatial204 inputs only; no model/candidate outputs')
output.write_text(json.dumps(report,indent=2)+'\n');print('PASS13vectors/2652features/6countcontrols/4guards/11corruptions/derived14641raster endpoint/0NN')
