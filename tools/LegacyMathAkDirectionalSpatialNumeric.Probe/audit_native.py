from pathlib import Path
import json,struct,hashlib,sys
repo,output=map(Path,sys.argv[1:]);H=lambda b:hashlib.sha256(b).hexdigest()
def pe(path):
 b=path.read_bytes();p=struct.unpack_from('<I',b,60)[0];n=struct.unpack_from('<H',b,p+6)[0];o=struct.unpack_from('<H',b,p+20)[0];sections=[struct.unpack_from('<4I',b,p+24+o+40*i+8) for i in range(n)]
 def read(a,n):
  r=a-0x180000000
  for vs,va,rs,ro in sections:
   if va<=r and r+n<=va+rs:return b[ro+r-va:ro+r-va+n]
  raise AssertionError('unmapped native range')
 return b,read
modules={m:pe(repo/'research/myscript/sdk/build/x64'/m) for m in ['MyScriptText.dll','MyScript2D.dll']}
pins=json.loads((Path(__file__).parent/'native-pins.json').read_text());count=0
for pin in pins:
 b,read=modules[pin['module']];assert H(b)==pin['moduleSHA256'];assert H(read(int(pin['start'],16),pin['bytes']))==pin['sha256'];count+=1
text=modules['MyScriptText.dll'][1];math=modules['MyScript2D.dll'][1]
assert struct.unpack('<Q',text(0x1802ba2c0+0x50,8))[0]==0x1800eaf90
assert struct.unpack('<Q',text(0x1802ba2c0+0x78,8))[0]==0x18004a2d0
assert struct.unpack('<Q',text(0x1802ba2c0+0x80,8))[0]==0x18004a2d0
assert text(0x18004a2d0,3)==bytes.fromhex('c20000')
data=(repo/'models/math/math-ak.res').read_bytes();assert H(data)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
header=struct.unpack_from('<8H',data,34);assert header[0]==256 and header[3]==65535 and header[7]==259
start,end=1459255,1628210
assert struct.unpack_from('<HHI',data,start)==(0xfea0,0x107,end-start-8)
assert data[start+8]==2 and struct.unpack_from('<H',data,start+9)[0]==0x4f
assert struct.unpack_from('<I',data,start+11)[0]==279
assert struct.unpack_from('<I',data,start+15)[0]==201
assert struct.unpack_from('<I',data,start+27)[0]==258
assert data[start-6:start]==bytes([0,0,1,2,0,1])
regions=[dict(start=hex(0x180011bf0),bytes=348,sha256=H(math(0x180011bf0,348)),role='complete shared network postprocess dispatcher')]
output.write_text(json.dumps(dict(priorNativePins=count,additionalBindings=3,modelKind=79,inputWidth=279,outputRows=257,virtualSymbol=256,fullAlphabetCount=259,unmaskedExp=True,V78Noop=True,regions=regions,modelExecutions=0),indent=2)+'\n')
print(f'PASS {count} inherited native spans /3 backend bindings /actual4F envelope /ALspecial256 /0NN')
