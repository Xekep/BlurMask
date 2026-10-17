"""Read-only exact ENlite source/preprocessing/noInitial feature ownership plan."""
from pathlib import Path
import hashlib,json,runpy,struct,sys
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv;sys.argv=[str(Path(__file__).with_name('audit_english_acoustic119_plan.py')),str(target.with_suffix('.inventory.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved;b=g['b'];f=g['f'];root=g['ROOT']
regions=[
(0x1801680d0,0x180168a70,'cce3a2fd29ba6cd3b73a7bc17eb930c50515c3884754eb7a39d35f93db177b2e'),
(0x180167210,0x180167bf0,'0989c9d0109004e12287dbcfc5916332c2920659f1a344863a48677386ed39dd'),
(0x180163970,0x180163cf0,'a4df3ec4e6f02d4f59cb221c4d6b4cd5908d27ce1a9caac9f37cec639d4db2d9'),
(0x18016e850,0x18016eb90,'6262b8fcda933597d1dafe89aa162c9cf1aabdf94f91faf8d57a66792b5561c4'),
(0x18016e570,0x18016e850,'88c398ad5840e81f65d344f5468db1e9874c349943ef3ce3e01cdac9f68af118'),
(0x180169750,0x180169950,'52eede5576adb4cb556256243aae178b139fc0e10a4b6f494a9940c4fbfe00b5'),
(0x18000ac70,0x18000aef0,'9571108b868b6fdcf4c3c5b412c21f4870de2a47fc4073e9fb16a54ca0c8aa3c'),
(0x18000aef0,0x18000b270,'0e103159fd78ffd3b2a91a0ba5d71d465cc6a66688939fe947b27c49f4328a80'),
(0x18000b270,0x18000b300,'a2ebb429d3532e6257cc5863b0d1bcd38af677781c377c91003b9ab0d94011f0'),
(0x180173b90,0x180173c10,'5d88c5d07663ae6a28c52fddb068de1cfd7e8714a328f86a07d76dc739f5e470')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h,(hex(a),hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest())
assert b.qwords(0x1802df6b8+0x18,1)[0]==0x18016e850
assert b.qwords(0x1802dfe20+0x18,1)[0]==0x180176a10
assert b.qwords(0x1802df0a0+0x88,1)[0]==0x180169750
assert 'uVar12 = 0' in f['1801680d0']and '*plVar5 + 0x18' in f['1801680d0']
for text in('param_2 + 0x9c','uVar17 = 0xffffffff','*local_res10 & 0xf','lVar11 + 4 + lVar16) = uVar17'):assert text in f['180167210']
assert "*(undefined1 *)(param_2 + 0x9c) = 0" in f['180163970']
assert '*(int *)(puVar9 + 8) =' in f['18016e850'] and 'param_3 + 0x58' not in f['18016e850']
assert 'FUN_18016e570' in f['18016e850']and'FUN_18016e0c0' in f['18016e850']
#24A callback169750 receives the new guide, and16E570 receives source+28
#XY/prefix view plus the guide. Neither receives/writes physical1C metadata.
assert 'param_3 + 0x28' in f['18016e850']and'param_2 + 0x58'not in f['180169750']
for text in('*param_6 = fVar17','param_6[1] = fVar10','*pfVar5 =','pfVar5[1] ='):assert text in f['18016e570']
for text in('*piVar11 == -1','piVar11 = piVar11 + 7','(int)uVar15 < 0x14','bVar10 == 1'):assert text in f['180176a10']
assert 'param_2[0x4b] == 0' in f['1801b0760'] and 'param_7 + 0xa0' in f['1801b75b0'] and 'param_7 + 0xe0' in f['1801b75b0']
assert '(short)param_2[6] * 7' in f['18000aef0']and'param_3 * 7' in f['18000b270']
models={x['path']:x for x in g['models']};en=models['en-US/en_US-ak-cur.lite.res'];ru=models['ru-RU/ru_RU-ak-cur.lite.res']
ed=(root/'models'/en['path']).read_bytes();rd=(root/'models'/ru['path']).read_bytes()
def processors(item,data):
 result=[]
 for c in item['first']['children']:
  if c['destination'].startswith('parametersC0.58['):result.append(dict(kind=hex(c['kind']),offset=c['offset'],end=c['end'],payload=data[c['offset']+16:c['end']].hex()))
 return result
ec=next(c for c in en['first']['children']if c['destination']=='247.130');rc=next(c for c in ru['first']['children']if c['destination']=='247.130')
assert ed[ec['offset']+16:ec['offset']+65]==rd[rc['offset']+16:rc['offset']+65]
p=en['firstAcoustic']['profiles'][0]['offset'];raw=struct.unpack_from('<BHI5H7f',ed,p)
assert raw==(0,0x1011,0x3f,30,16,8,10,0,0.,0.,0.,0.,0.,0.,0.)
assert ed[16818:16822]==b'\0'*4
assert [s['first']for s in en['firstAcoustic']['profiles'][0]['slots'][:2]]==[0,0]
assert [s['second']for s in en['firstAcoustic']['profiles'][0]['slots'][:2]]==[1,1]
report=dict(noVendorExecution=True,readOnly=True,
 dependencies='english-acoustic119-plan-static; prepared107 activation/caller proof',
 regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],
 englishProcessors=processors(en,ed),russianProcessors=processors(ru,rd),same49ByteSamplerPrefix=True,
 englishFeatureProfile=dict(flags='0x1011',mask='0x3f',samples=30,rasterBins=16,rasterFactor=8,exponent=10,
 trajectoryGeometry=30*7+7,rasterGeometry=16*12+7,contextChannels=0,boundaryChannels=2,trajectoryInput=219,rasterInput=201),
 ownership='Fresh unscoped kind0/9C0 ingress physical+4=-1.24A writes descriptor+10 and guide/XY, not physical+4.24C counter reads same post24A XY.',
 nextBoundedJoin='NEW ownedENlite preprocessing24A→24C counter<20 unchanged→265; temporary refusal>=20; future positive24C mandatory. No source classifier until range/normalization/boundary ownership is joined.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS10additionalPEranges/3bindings/physical-minus1 closure/actual24A24C265/profile217+199/noInitial0+boundary2; read-only plan')
