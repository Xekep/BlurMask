from pathlib import Path
import sys,json,struct,hashlib,copy
repo,capture,output=map(Path,sys.argv[1:]);H=lambda b:hashlib.sha256(b).hexdigest()
b=(repo/'models/math/math-ak.res').read_bytes();assert H(b)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
GUARDS=['cancel-captured-exact-token','cancel-authenticated-exact-token','cancel-calibrated-exact-token','cancel-publish-exact-token','truncated-resource-refused','changed-calibration-resource-refused','caller-mutation-after-private-capture','stroke-count-read-once','caller-points-detached','returned-points-detached','all128-geometries-same-profile','eight-calibration-arrays-not-cloned','models-create-geometry-same-profile','equal-data-foreign-source-cohort-refused','program-value-copy-detached','create-pre-cancel-exact-token','caller-resource-cleared-before-create','parallel-profile-create-inspect']
fields=[('ModeX',1005996,'B'),('ModeY',1005997,'B'),('NormalizationTarget',1005998,'H'),('LegacyMask',1006005,'H'),('LegacyFlags',1006007,'I'),('TemporalPointCount',1006011,'H'),('SpatialBins',1006013,'H'),('GridSide',1006015,'H'),('Quantizer',1006017,'H'),('DirectionalMask',1006019,'H'),('GlobalMask',1006021,'I'),('BaselineMask',1006025,'I'),('TemporalMask',1006029,'I'),('SpatialMask',1006033,'I'),('UpperMask',1006037,'I'),('TargetCount',1006041,'I'),('GridScale',1006045,'I'),('TemporalWindows',1006051,'h'),('SpatialColumns',1006055,'h'),('SpatialRows',1006057,'h'),('UpperColumns',1006061,'h'),('UpperRows',1006063,'h'),('UnitPolicy',1006065,'B'),('Backend',1006066,'B')]
expected={k:struct.unpack_from('<'+fmt,b,at)[0]for k,at,fmt in fields}
pairs=[]
for count,start in [(412,1006082),(279,1288051),(279,1458131),(223,1628211)]:
 assert b[start]==1 and struct.unpack_from('<H',b,start+1)[0]==count
 pairs.append((b[start+3:start+3+2*count],b[start+3+2*count:start+3+4*count]))
calibrations=[{'length':len(v)//2,'sha256':H(v)}for channel in (0,1) for pair in pairs for v in [pair[channel]]]
def check(r):
 assert r['guards']==GUARDS and r['modelExecutions']==0 and type(r['modelExecutions'])is int and r['vendorExecution']is False
 assert type(r['geometries'])is int and r['geometries']==128 and type(r['CalibrationPayloadBytes'])is int and r['CalibrationPayloadBytes']==4772
 assert set(r['Program'])==set(expected) and all(type(v)is int for v in r['Program'].values()) and r['Program']==expected
 assert r['calibrations']==calibrations and all(type(v['length'])is int for v in r['calibrations'])
r=json.loads(capture.read_text());check(r);negative=[]
for name,mut in [('program-target',lambda q:q['Program'].__setitem__('NormalizationTarget',100)),('program-backend',lambda q:q['Program'].__setitem__('Backend',1)),('program-bool',lambda q:q['Program'].__setitem__('ModeX',True)),('minimum-hash',lambda q:q['calibrations'][0].__setitem__('sha256','0'*64)),('maximum-family',lambda q:q['calibrations'].__setitem__(5,q['calibrations'][4])),('guard-order',lambda q:q['guards'].reverse()),('nn-claim',lambda q:q.__setitem__('modelExecutions',1)),('payload',lambda q:q.__setitem__('CalibrationPayloadBytes',1648))]:
 bad=copy.deepcopy(r);mut(bad)
 try:check(bad)
 except AssertionError:negative.append(name)
 else:raise AssertionError(name)
output.write_text(json.dumps(dict(captureSHA256=H(capture.read_bytes()),programFields=24,calibrationShorts=2386,calibrationBytes=4772,calibrationArrays=8,guards=18,sharedGeometryCount=128,tamperControls=negative,modelExecutions=0),indent=2)+'\n')
print('PASS all24 program fields /2386shorts /8arrays /18guards /8corruptions /0NN')
