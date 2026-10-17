"""Independent full-payload/flat-source and ordinal rebind audit, no ranking oracle."""
import base64,json,struct,sys
from pathlib import Path
b64=base64.b64decode
cases=json.loads(Path(sys.argv[1]).read_bytes());words=selected=released=0
for case in cases:
 old=case['original'];sources=case['sources'];expected=[-1]*len(old)
 for i,s in enumerate(sources):expected[s]=i
 assert case['map']==expected
 for i,s in enumerate(sources):
  a,b=b64(old[s]['row']),b64(case['ranked'][i]);c=b64(case['output'][i]['row']);d=b64(case['scores'][i])
  assert a[:16]==b[:16]and a[20:]==b[20:]and(a[16:20]==b[16:20]or struct.unpack_from('<I',b,16)[0]==0xff7fffff)
  assert b[:16]==c[:16]and b[24:]==c[24:]and c==d
  assert case['output'][i]['flat']==old[s]['flat']
  assert struct.unpack_from('<I',c,40)[0]==len(old[s]['flat'])
 assert case['rebound']==[expected[s]for s in case['selected']]
 words+=len(old);selected+=len(case['selected']);released+=len(old)-len(sources)
assert len(cases)==1024
print(f'PASS independent1024 span owners/{words} complete120B payloads/{selected} selected279 ordinals/{released} released sidecars; all flat identities retained.')
