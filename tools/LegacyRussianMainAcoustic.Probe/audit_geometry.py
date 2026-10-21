"""Independent raw float normalization + trajectory/raster baseline9 oracle.
Directional313 extraction and native geometry-source admission stay separate.
"""
import ast,json,math,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
def defs(path):
 t=ast.parse(path.read_text(encoding='utf8'));t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];g={'__file__':str(path)};exec(compile(t,str(path),'exec'),g);return g
stream=defs(Path(__file__).with_name('audit_output.py'))['samples']
feature=defs(ROOT/'tools/LegacyEnglishNoInitialGlyph.Probe/feature_oracle.py')
#Expose the independent reference raster's pixel set as an additional result;
#the line walk and feature calculations remain the existing Python reference.
text=(ROOT/'tools/LegacyEnglishNoInitialGlyph.Probe/feature_oracle.py').read_text();t=ast.parse(text)
raster=next(n for n in t.body if isinstance(n,ast.FunctionDef)and n.name=='raster')
last=raster.body[-1];assert isinstance(last,ast.Return)
last.value=ast.Tuple(elts=[last.value,ast.Name(id='pixels',ctx=ast.Load())],ctx=ast.Load());ast.fix_missing_locations(raster)
exec(compile(ast.Module(body=[raster],type_ignores=[]),'independent-raster-pixels','exec'),feature)
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def short(v):return(v+32768)%65536-32768
def upper(v):return min(1024,short(v))
def clamp(v):return max(-1024,min(1024,short(v)))
div=feature['div'];groups=points=channels=collapsed=0;minimum=32767;maximum=-32768
for sample in stream(Path(sys.argv[1])):
 for group in sample['mainGroups']:
  source=[[[f(v)for v in p]for p in s]for s in group['rawRanges']];assert group['target']==100 and group['sourceHeight']>0
  assert f(group['minX'])==min(x for s in source for x,y in s)
  normalized=[]
  for stroke in source:
   out=[]
   for p in stroke:
    point=[]
    for v,base in zip(p,[group['minX'],group['sourceBaseline']]):
     v=f(f(f(v-f(base))*100)/f(group['sourceHeight']));assert math.isfinite(v)and v>=-32768;point.append(min(32767,int(v)))
    out.append(point);points+=1;minimum=min(minimum,*point);maximum=max(maximum,*point)
   normalized.append(out)
  assert normalized==group['normalized'],('normalized',sample['id'],group['id'])
  assert group['normalizedBaseline']==0 and group['normalizedHeight']==100
  flat=[p for s in normalized for p in s]
  if len({tuple(p)for p in flat})==1:collapsed+=1;raise AssertionError('Directional MAIN collapsed geometry unexpectedly admitted')
  samples,pen,parts,w,h=feature['geometry'](normalized)
  trajectory=feature['trajectory'](samples,pen,w,h,len(normalized));raster,pixels=feature['raster'](parts)
  baseRaster=trajectory[210:]+raster;assert len(trajectory)==217 and len(baseRaster)==199
  assert trajectory==group['inputs'][0][:217]and baseRaster[:7]==group['inputs'][1][:7]and raster==group['inputs'][1][16:208],('basic',sample['id'],group['id'])
  miny=min(p[1]for p in flat);sx=max(p[0]for p in flat)-min(p[0]for p in flat);sy=max(p[1]for p in flat)-miny;scale=130048//max(sx,sy)
  center=(128-min(w,h)-1)>>1 if sy<sx else 0;y=((-miny*scale)>>10)+center;bh=100*scale>>10
  physical=[p for p,state in zip(samples,pen)if state==1024]
  physicalLength=sum(feature['distance'](a,b)for a,b,pa,pb in zip(samples,samples[1:],pen,pen[1:])if pa==pb==1024)
  above=sum(py<y-bh for px,py in pixels);below=sum(py>y for px,py in pixels);within=len(pixels)-above-below
  if bh==0:baseline=[1024,1024,1024,1024,0,0,0,1024,1024]
  else:baseline=[upper(div(512*(w+1),bh)-1024),upper(div(512*(h+1),bh)-1024),clamp(div(256*(div(sum(p[1]for p in physical),len(physical))-y),bh)),upper(div(256*(physicalLength>>4),bh)-1024),0,0,0,clamp(div(512*(y-bh-center),bh)-1024),clamp(div(512*(h+center-y),bh)-1024)]
  baseline[4:7]=[clamp(div(2048*v,len(pixels))-1024)for v in [above,within,below]]if pixels else[1024]*3
  assert baseline==group['inputs'][0][217:226]==group['inputs'][1][7:16],('baseline9',sample['id'],group['id'])
  groups+=1;channels+=494
 if groups%2000<150:print('geometry',sample['id'],groups,flush=True)
assert groups==27866 and collapsed==0
report=dict(groups=groups,normalizedPoints=points,coordinateMinimum=minimum,coordinateMaximum=maximum,independentTrajectoryRasterWithBaselineChannels=channels,collapsed=collapsed,expectedLabelsRead=False,scope='Raw point minX/float normalization+independent217/199+baseline9; directional313 extraction/native graphmembership not claimed')
if len(sys.argv)>2:Path(sys.argv[2]).write_text(json.dumps(report,indent=2)+'\n')
print('PASS',json.dumps(report))
