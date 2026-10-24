from pathlib import Path
import sys,struct,json,hashlib,tempfile,subprocess
R,O=map(Path,sys.argv[1:]);D=Path(__file__).resolve().parent;assert not O.exists();H=lambda b:hashlib.sha256(b).hexdigest()
pins=json.loads((D/'native-pins.json').read_text(encoding='utf-8'));raw=json.loads((D/'raw-program.json').read_text(encoding='utf-8'))
def pe(name,sha):
 b=(R/'research/myscript/sdk/build/x64'/name).read_bytes();assert H(b)==sha
 p=struct.unpack_from('<I',b,60)[0];n=struct.unpack_from('<H',b,p+6)[0];o=struct.unpack_from('<H',b,p+20)[0];ss=[struct.unpack_from('<4I',b,p+24+o+40*i+8)for i in range(n)]
 def read(a,n):
  v=a-0x180000000
  for vs,va,size,at in ss:
   if va<=v and v+n<=va+size:return b[at+v-va:at+v-va+n]
  raise AssertionError('PE span')
 return read
tr=pe('MyScriptText.dll',pins['textSHA']);mr=pe('MyScript2D.dll',pins['mathSHA'])
for spans in pins['textRegions'].values():
 for p in spans:assert H(tr(int(p['start'],16),p['bytes']))==p['sha256']
for p in pins['mathRegions']:assert H(mr(int(p['start'],16),p['bytes']))==p['sha256']
for p in pins['tables']:assert tr(int(p['at'],16),len(bytes.fromhex(p['bytes'])))==bytes.fromhex(p['bytes'])
for p in json.loads((D/'critical-operands.json').read_text(encoding='utf-8')):
 b=tr(int(p['start'],16),p['bytes']);assert b.hex()==p['hex']and H(b)==p['sha256']
assert struct.unpack('<Q',tr(0x1802b6b10+0x50,8))[0]==0x1800b5e20
assert struct.unpack('<Q',tr(0x1802b75a0+0x50,8))[0]==0x1800c0da0
for a,h in [(0x18000d733,'0fb68c3e89000000'),(0x18000d73b,'888c2498000000'),(0x18000d742,'0fb68c3e88000000'),(0x18000d74a,'888c2490000000')]:assert mr(a,len(bytes.fromhex(h)))==bytes.fromhex(h)
ak=(R/'models/math/math-ak.res').read_bytes();assert H(ak)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
for f in raw['fields']:assert struct.unpack_from('<'+f['format'],ak,f['at'])[0]==f['value']
assert ak[1628211]==1 and struct.unpack_from('<H',ak,1628212)[0]==223
for name in ['minimum','maximum']:assert H(ak[raw[name+'At']:raw[name+'At']+446])==raw[name+'SHA256']
assert struct.unpack_from('<I',ak,raw['upperMask']['at'])[0]==255
assert struct.unpack_from('<H',ak,raw['separate2AEMask']['at'])[0]==0
assert struct.unpack_from('<HHI',ak,1629111)==(0xfea0,0x107,1772466-1629111-8)
assert ak[1629119]==2 and struct.unpack_from('<H',ak,1629120)[0]==0x5f
assert struct.unpack_from('<5I',ak,1629122)==(223,201,0,0,258)
O.write_text(json.dumps(dict(textMethods=len(pins['textRegions']),mathRegions=len(pins['mathRegions']),registrationSpans=len(pins['tables']),criticalASMSpans=2,abiOperands=4,rawProgramFields=14,calibrationShorts=446,upperCells=25,channelsPerCell=8,calibratedPrefix=218,modelExecutions=0),indent=2)+'\n',encoding='utf-8')
print('PASS29Text/4Math/5F223raw/upper25x8/nativepolicy;0NN')
