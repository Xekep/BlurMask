"""Independent public Unicode decomposition oracle; no vendor data."""
import json,random,sys,unicodedata
from pathlib import Path
wide=[]
for cp in range(0x110000):
 d=unicodedata.decomposition(chr(cp)).split()
 if d and d[0]=='<wide>':
  assert len(d)==2
  wide.append([cp,int(d[1],16)])
assert len(wide)==104
r=random.Random(0x58da0);samples=set([0x3000,*range(0xff00,0xffef+1),*range(0x410,0x450),0xa0,0xb5,ord('m'),ord('p'),0x1d400])
samples.update(r.randrange(0x110000) for _ in range(1024));lookup=dict(wide)
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(unicodeVersion=unicodedata.unidata_version,wide=wide,cases=[[cp,lookup.get(cp,-1)] for cp in sorted(samples)]),separators=(',',':')),encoding='utf8')
print(f'Generated104 public Unicode wide references +{len(samples)} scalar controls (UCD{unicodedata.unidata_version}).')
