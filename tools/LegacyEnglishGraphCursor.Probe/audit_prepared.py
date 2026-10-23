"""Independent finite ring/mask summary + native171-store field/control oracle."""
import json,sys,struct,hashlib
from pathlib import Path
def validate(d):
 assert all(type(d[k]) is int for k in ["Nodes","Transitions","Refusals","RetainedWindow","SourceEnd","RetainedEnd","DefinednessCases","LanguageAnchorControls","LanguageMaskRefusals","NNCalls"])
 assert (d['Nodes'],d['Transitions'],d['Refusals'],d['RetainedWindow'],d['SourceEnd'],d['RetainedEnd'],d['DefinednessCases'],d['LanguageAnchorControls'],d['LanguageMaskRefusals'],d['NNCalls'])==(122,121,5,120,156,125,4,3,23,0)
 cases=d['FirstWriter']['Cases'];assert len(cases)==64
 for n,c in enumerate(cases):
  assert all(type(c[k]) is int for k in ["Tag","Cost","Ordinal","Previous"])
  assert (c["Tag"],c["Cost"],c["Ordinal"],c["Previous"])==(0x3900000100000000+n,[-67108864,-1,0,1,67108863][n%5],n%125,-1)
  assert bytes.fromhex(c["Before"])==bytes((i*37+n*11)&255 for i in range(128))
  assert bytes.fromhex(c["Parent"])==bytes((i*13+n*7)&255 for i in range(88))
  assert bytes.fromhex(c["Language"])==bytes((i*23+n*3)&255 for i in range(20))
  b=bytearray.fromhex(c['Before']);p=bytes.fromhex(c['Parent']);l=bytes.fromhex(c['Language']);assert len(b)==128 and len(p)==88 and len(l)==20
  b[:88]=p;b[33]&=192;struct.pack_into('<Q',b,0,c['Tag']);b[39]&=254
  struct.pack_into('<H',b,62,65535);struct.pack_into('<i',b,16,c['Cost']);struct.pack_into('<H',b,30,c['Ordinal']);b[32]=255
  b[64:84]=l;struct.pack_into('<Q',b,8,0);struct.pack_into('<Q',b,40,8<<40);struct.pack_into('<Q',b,92,(0x4000000<<32)|0xffffffff)
  struct.pack_into('<I',b,88,0xffffffff);struct.pack_into('<i',b,104,c['Previous']);struct.pack_into('<Q',b,112,0)
  assert bytes(b)==bytes.fromhex(c['After'])
  assert b[120:128]==bytes.fromhex(c['Before'])[120:128]
 return len(cases)
raw=Path(sys.argv[1]).read_bytes();d=json.loads(raw.decode("utf-16") if raw.startswith(b"\xff\xfe") else raw)
validate(d)
negatives=0
if "--tamper" in sys.argv:
 import copy
 changes=[lambda x:x.update(NNCalls=False),lambda x:x.update(DefinednessCases=4.0),lambda x:x.update(Nodes=121),lambda x:x.update(NNCalls=1),lambda x:x.update(DefinednessCases=3),lambda x:x["FirstWriter"]["Cases"].pop(),lambda x:x["FirstWriter"]["Cases"].append(copy.deepcopy(x["FirstWriter"]["Cases"][0]))]
 for key in ["Cost","Tag","Ordinal","Previous"]:changes.append(lambda x,k=key:x["FirstWriter"]["Cases"][0].update({k:False}))
 for key in ["Before","Parent","Language","After"]:changes.append(lambda x,k=key:x["FirstWriter"]["Cases"][0].update({k:"FF"+x["FirstWriter"]["Cases"][0][k][2:]}))
 for change in changes:
  bad=copy.deepcopy(d);change(bad)
  try:validate(bad)
  except (AssertionError,KeyError,TypeError,ValueError):negatives+=1;continue
  raise AssertionError("Prepared tamper survived")
print(json.dumps({'scope':'PREPARED only; no actual generic owner evidence','writerControls':64,'negatives':negatives,'opaqueTailControls':64,'definednessCounts':[0,1,124,125],'NNCalls':0,'captureSha256':hashlib.sha256(raw).hexdigest()},sort_keys=True))
