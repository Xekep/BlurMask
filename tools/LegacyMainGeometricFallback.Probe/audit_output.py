"""Independent24A estimates/affine transforms, distinct from actualNN evidence."""
import base64,importlib.util,json,math,pathlib,struct,sys
import numpy as np
root=pathlib.Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('line_oracle',root/'tools/LegacyAutomaticLineEstimator.Probe/reference_controls.py')
oracle=importlib.util.module_from_spec(spec);spec.loader.exec_module(oracle)
f=lambda x:float(np.float32(x))
def close(a,b):
    assert math.isfinite(a) and abs(a-b)<=3e-6*max(1,abs(b)),(a,b)
def transform(points,baseline,height,slope):
    xs=[p[0] for s in points for p in s];mid=f(f(max(xs)+min(xs))*.5)
    cosine=f(1/f(math.sqrt(f(slope*slope)+1)));sine=f(math.sqrt(f(1-f(cosine*cosine))))
    if slope>0:sine=-sine
    anchor=f(f(mid*slope)+baseline);tx=f(f(f(anchor*sine)-f(mid*cosine))+mid);ty=f(f(f(-sine*mid)-f(anchor*cosine))+anchor)
    newbase=f(f(baseline*cosine)+ty);newheight=f(f(f(f(baseline+height)*cosine)+ty)-newbase)
    if abs(newheight)<=max(f(abs(newheight)*f(2**-22)),f(2**-23)):
        fallback=f(f(max(xs)-min(xs))*f(.15));newheight=max(fallback,f(.1));newbase=f(newbase-f(fallback*.5))
    #Homogeneous matrix checks the owned point transform independently of C#.
    matrix=np.array([[cosine,-sine,tx],[sine,cosine,ty],[0,0,1]])
    mapped=[(np.column_stack((np.asarray(s),np.ones(len(s))))@matrix.T)[:,:2].tolist() for s in points]
    return newbase,newheight,mapped
data=json.loads(pathlib.Path(sys.argv[1]).read_text(encoding='utf-8-sig'))
node=bytes.fromhex(data['node']);assert data['processor']==187976
profile=struct.unpack_from('<ifBffBBB',node,16);decode=lambda values:[base64.b64decode(v) for v in values]
for item in data['prepared']:
    old=decode(item['inputDescriptors']);new=decode(item['descriptors']);guides=decode(item['guides'])
    assert old[0]==new[0] and old[2]==new[2] and decode(item['inputGuides'])[0]==guides[0]
    assert item['inputPoints'][0]==item['points'][0]
    for d,physical,line in [(1,1,1),(3,2,2)]:
        expected=bytearray(old[d]);struct.pack_into('<i',expected,16,line);assert bytes(expected)==new[d]
        points=[item['inputPoints'][physical]];estimate=oracle.estimate(points,profile)
        baseline=f(estimate['baseline']);height=f(estimate['baseline']-estimate['upper']);slope=f(estimate['slope'])
        b,h,mapped=transform(points,baseline,height,slope)
        g=guides[line];values=struct.unpack_from('<8f',g)
        for actual,wanted in zip(values,[b,h,slope,baseline,height,0,0,0]):close(actual,wanted)
        assert struct.unpack_from('<ii',g,32)==(-1,-2147483648)
        #Matrix composition is double; the native point kernel rounds each
        #float product/add. Bound cancellation error by the input scale,
        #rather than the possibly near-zero final coordinate alone.
        scale=max(1,max(abs(v) for p in points[0] for v in p))
        for actual,wanted in zip(item['points'][physical],mapped[0]):
            for x,y in zip(actual,wanted):assert math.isfinite(x) and abs(x-y)<=3e-6*max(scale,abs(y)),(x,y,scale)
assert len(data['prepared'])==128 and data['preparedGuides']==256 and data['guards']==19
assert len(data['actual'])==7 and sum(x['source720'] for x in data['actual'])==26 and sum(x['guides'] for x in data['actual'])==8
assert all(x['created']==0 for x in data['actual'])
print('PASS independent256 prepared main24A NumPy estimates/affine guides/intervals;7 actual firstTEXT owners only assigned-guide no-op evidence')
