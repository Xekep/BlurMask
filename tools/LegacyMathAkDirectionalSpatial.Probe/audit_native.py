"""Static PE/raw selected4F producer receipts; no DLL loading or weights execution."""
from pathlib import Path
import sys,struct,hashlib,json
R,O=map(Path,sys.argv[1:]);D=Path(__file__).resolve().parent;assert not O.exists();H=lambda b:hashlib.sha256(b).hexdigest()
pins=json.loads((D/'native-pins.json').read_text(encoding='utf-8-sig'));raw=json.loads((D/'raw-program.json').read_text(encoding='utf-8-sig'))
def pe(name,expected):
 b=(R/'research/myscript/sdk/build/x64'/name).read_bytes();assert H(b)==expected
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
for address,hexbytes in [(0x18000d733,'0fb68c3e89000000'),(0x18000d73b,'888c2498000000'),(0x18000d742,'0fb68c3e88000000'),(0x18000d74a,'888c2490000000')]:assert mr(address,len(bytes.fromhex(hexbytes)))==bytes.fromhex(hexbytes)
assert struct.unpack('<Q',tr(0x1802b6b10+0x48,8))[0]==0x1800b5d90
assert struct.unpack('<Q',tr(0x1802b75a0+0x48,8))[0]==0x1800bff40
ak=(R/'models/math/math-ak.res').read_bytes();assert H(ak)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
assert len(raw['fields'])==12
for f in raw['fields']:assert struct.unpack_from('<'+f['format'],ak,f['at'])[0]==f['value']
assert ak[1458131]==1 and struct.unpack_from('<H',ak,1458132)[0]==279
for name in ['minimum','maximum']:assert H(ak[raw[name+'At']:raw[name+'At']+558])==raw[name+'SHA256']
assert struct.unpack_from('<HHI',ak,1459255)==(0xfea0,0x107,1628210-1459255-8)
assert struct.unpack_from('<H',ak,1459264)[0]==0x4f and struct.unpack_from('<I',ak,1459266)[0]==279
mask=struct.unpack_from('<H',ak,1006019)[0];assert mask==0xa111 and mask&0x1000==0 and mask&0x2000 and mask&0x8000
assert struct.unpack_from('<I',ak,1006033)[0].bit_count()==16 and struct.unpack_from('<I',ak,1006021)[0].bit_count()==17
#BCDB0 uses sorted coordinate order statistics k*N/4, half-open intervals,
#then final-bin fallback; stable incoming sample order is retained inside bins.
controls=[]
for n in [1,2,3,4,5,16,17,100,101,254,255]:
 points=[((i*17)%31,0 if n%2 else (i*7)%23)for i in range(n)]
 axis=[sorted(p[a]<<10 for p in points)for a in (0,1)];bounds=[[v[k*n//4]for k in range(4)]+[v[-1]]for v in axis]
 def locate(c,b):return next((k for k in range(4)if b[k]<=c<b[k+1]),3)
 groups=[[]for _ in range(16)]
 for i,(x,y)in enumerate(points):groups[locate(y<<10,bounds[1])*4+locate(x<<10,bounds[0])].append(i)
 assert sorted(i for g in groups for i in g)==list(range(n)) and all(g==sorted(g)for g in groups)
 assert locate(bounds[0][-1],bounds[0])==3
 controls.append(n)
O.write_text(json.dumps(dict(textMethods=len(pins['textRegions']),mathMethods=len(pins['mathRegions']),registrationSpans=len(pins['tables']),abiOperands=4,rawFields=12,calibrationShorts=558,spatialCells=16,channels=279,quantileControls=controls,modelExecutions=0,scope='Selected4F primary coordinate-quantile feature producer only; no NN,upper-grid output,grammar or candidate'),indent=2)+'\n',encoding='utf-8')
print('PASS26Text/4Math/6bindings/12fields/558calibrationShorts/11quantileControls;0NN')
