"""Independent actual C49/C4A/C4B point bytes from original coordinates/ranges.
No expected labels, SDK execution or2DB/source-mutation claim.
"""
from pathlib import Path
import collections,hashlib,json,runpy,struct,sys
root=Path(__file__).resolve().parents[2]
if len(sys.argv)not in(6,7):raise SystemExit('MODELS RECORDED-MANIFEST CHAR-CAPTURE PARENT-CAPTURE CROP-CAPTURE [NATIVE-CAPTURE]')
models,recorded,char_path,parent_path,crop_path=sys.argv[1:6];saved=sys.argv[:]
try:
 sys.argv=['audit_output.py',models,char_path,parent_path]
 runpy.run_path(str(root/'tools/LegacyGlobalWordText.Probe/audit_output.py'),run_name='__main__')
finally:sys.argv=saved
f=lambda v:struct.unpack('<f',struct.pack('<f',v))[0]
bits=lambda v:struct.unpack('<i',struct.pack('<f',v))[0]
m=[[0,90],[0,10],[25,60],[50,10],[50,90]]
inputs={'M':[m],'MIR':[m,[[70,10],[70,90],[120,10],[120,90]],[[140,90],[140,10],[180,10],[190,20],[190,40],[180,50],[140,50]]],
 'I':[[[0,10],[0,90]]],'II':[[[0,10],[0,90]],[[40,10],[40,90]]]}
manifest_path=Path(recorded);manifest=json.loads(manifest_path.read_bytes())
assert manifest['commit']=='c6629026b1fdef51fd08e3d2fddffd82f97e60a5'and manifest['penUpMetadata']=='absent'
for sample in manifest['samples']:
 if sample['kind']!='word':continue
 name=sample['file'];assert Path(name).name==name
 raw=(manifest_path.parent/name).read_bytes();assert hashlib.sha256(raw).hexdigest()==sample['trackSha256']
 inputs[name]=[[[f(x),f(-f(y))]for x,y in json.loads(raw)['points']]]
parents={p['name']:p for p in json.loads(Path(parent_path).read_bytes())['actual']}
capture=json.loads(Path(crop_path).read_bytes());count=points=0;kinds=collections.Counter()
for sample in capture['samples']:
 raw=inputs[sample['name']];assert sample['raw']==[[[bits(x),bits(y)]for x,y in stroke]for stroke in raw]
 parent=parents[sample['name']];sources=[(False,g['node'],g['processed'])for g in parent['links']]+[(True,parent['node'],parent['wordKey'])]
 expected=[]
 for word,node,ranges in sources:
  for ordinal,(a,b)in enumerate(ranges):
   start_obj,end_obj=a>>32,b>>32
   for obj in range(start_obj,end_obj+1):
    source=raw[obj];size=len(source)
    a_point,a_frac,a_mode=(a>>16)&65535,(a>>8)&255,a&255
    b_point,b_frac,b_mode=(b>>16)&65535,(b>>8)&255,b&255
    first_crop=obj==start_obj and(a_point,a_frac,a_mode)!=(0,0,3)
    last_crop=obj==end_obj and b_frac!=255
    start=a_point if first_crop else 0;end=b_point if last_crop else size-1
    f0=f(a_frac*f(.005))if first_crop and a_frac!=255 else 0.
    f1=f(b_frac*f(.005))if last_crop else 0.
    kind=0 if not first_crop and not last_crop else 1 if f0==f1==0 else 2
    def interpolate(index,fraction):
     return[f(f(f(source[index+1][i]-source[index][i])*fraction)+source[index][i])for i in(0,1)]
    values=[interpolate(i,f0)if i==start and f0 else source[i]for i in range(start,end+1)]
    if f1:values.append(interpolate(end,f1))
    expected.append(dict(word=word,node=node,range=ordinal,object=obj,kind=kind,first=start,last=end,
     fraction0=bits(f0),fraction1=bits(f1),include0=not first_crop or a_mode==3,include1=not last_crop or b_mode==5,
     points=[[bits(x),bits(y)]for x,y in values]))
    kinds[kind]+=1;points+=len(values)
 assert sample['views']==expected,sample['name'];count+=len(expected)
assert len(capture['samples'])==13 and(capture['views'],capture['points'],capture['guards'])==(count,points,217)
assert capture['poisonedCounterWithoutCommand']is True
assert(count,points)==(54,3014)and dict(kinds)=={0:23,1:19,2:12}
if len(sys.argv)==7:assert Path(crop_path).read_bytes()==Path(sys.argv[6]).read_bytes()
print(f'PASS13 actual sources/{count} views/{points} exactfloatXY;C49=23/C4A=19/C4B=12, original inputs/intervals/chunk20 provenance;no2DB/238B0.')
