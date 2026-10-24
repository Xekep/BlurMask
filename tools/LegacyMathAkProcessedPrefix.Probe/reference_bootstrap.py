from pathlib import Path
import importlib.util,json,struct,sys
OLD=Path(__file__).resolve().with_name('reference_prefix.py')
spec=importlib.util.spec_from_file_location('old_prefix',OLD);r=importlib.util.module_from_spec(spec);spec.loader.exec_module(r)
def bootstrap(strokes):
 output=[];maps=[];base=0
 for raw in strokes:
  row=[(r.F(x),r.F(y))for x,y in raw];kept=[];indices=[]
  for i,p in enumerate(row):
   near=False
   if i:
    near=True
    for j in (0,1):
     tolerance=max(r.mul(max(abs(row[i-1][j]),abs(p[j])),2**-22),r.F(2**-23))
     if tolerance<abs(r.sub(p[j],row[i-1][j])):near=False;break
   if not near:kept.append(p);indices.append(base+i)
   elif i==len(row)-1:
    if len(kept)==1:kept.append(p);indices.append(base+i)
    else:indices[-1]=base+i
  output.append(kept);maps.append(indices);base+=len(row)
 return output,maps
def run(strokes):
 points,maps=bootstrap(strokes);flat=[i for row in maps for i in row];result=r.run(points)
 result['originalPointIndices']=[[flat[i]for i in row]for row in result['originalPointIndices']]
 return dict(bootstrapPoints=points,bootstrapOriginalPointIndices=maps,result=result)
def controls():
 old=[(x['name'],x['input'])for x in r.controls()['cases']]
 eps=2**-24
 extra=[('two-identical-forced-last',[[(0,0),(0,0)]]),('three-identical-forced-last',[[(0,0),(0,0),(0,0)]]),('near-final-map-replaces',[[(0,0),(1,0),(1+2**-23,0)]]),('relative-near-large',[[(1e8,0),(1e8+8,0),(1e8+16,0)]]),('adjacent-original-not-last-kept',[[(i*eps,0)for i in range(5)]]),('relative-threshold-within',[[(1,0),(1+2**-22,0),(2,0)]])]
 eps23=2**-23;above=struct.unpack('<f',struct.pack('<I',0x34000001))[0]
 extra += [('absolute-threshold-equality',[[(0,0),(eps23,0),(1,0)]]),('absolute-threshold-next-f32-above',[[(0,0),(above,0),(1,0)]])]
 cases=[dict(name=n,input=x,output=run(x))for n,x in old+extra]
 by={x['name']:x['output']for x in cases}
 assert by['two-identical-forced-last']['bootstrapOriginalPointIndices']==[[0,1]]
 assert by['three-identical-forced-last']['bootstrapOriginalPointIndices']==[[0,2]]
 assert by['near-final-map-replaces']['bootstrapOriginalPointIndices']==[[0,2]]
 assert by['near-final-map-replaces']['bootstrapPoints'][0][-1][0]==1
 assert by['adjacent-original-not-last-kept']['bootstrapOriginalPointIndices']==[[0,4]]
 assert by['relative-threshold-within']['bootstrapOriginalPointIndices']==[[0,2]]
 for x in cases[:8]:assert x['output']['result']==r.run(x['input']),x['name']
 assert by['absolute-threshold-equality']['bootstrapOriginalPointIndices']==[[0,2]]
 assert by['absolute-threshold-next-f32-above']['bootstrapOriginalPointIndices']==[[0,1,2]]
 return dict(cases=cases,original8Unchanged=True,newBootstrapControls=8,csharpExecution=False,networkForwards=0)
if __name__=='__main__':Path(sys.argv[1]).write_text(json.dumps(controls(),indent=2)+'\n',encoding='utf8');print('PASS 8 old +8 bootstrap controls, no CSharp/NN')
