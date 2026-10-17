"""Replay raw keys independently; actual labels are never inference inputs."""
import base64,json,struct,sys
from pathlib import Path
from reference_controls import replay
a=json.loads(Path(sys.argv[1]).read_bytes());controls=json.loads(Path(sys.argv[2]).read_bytes())['cases'];b64=base64.b64decode
for got,case in zip(a['prepared'],controls):
 expected=replay(case['rows'],case['fail'],case['final'])
 for key,value in expected.items():assert got[key]==value,(key,got,expected)
 raw=b64(got['utf8']);caller=raw
 refused=got['succeeded']and not raw and bool(case['flags']&0x400000)
 assert got['callerRefused']==refused
 if got['succeeded']and raw and case['flags']&0x400000:caller=(b'{'if raw[:1]==b'['else b'}')+raw[1:]
 assert b64(got['callerUtf8'])==caller
assert len(a['prepared'])==len(controls)==2053
total=0
for actual in a['actual']:
 sources=actual['sources'];assert [s['kind']for s in sources[-2:]]==['WORD','TEXT']
 assert all(s['kind']=='CHAR'for s in sources[:-2]);assert len({s['id']for s in sources[:-2]})==len(sources)-2
 for source in sources:
  raw=b64(source['original16']);rows=list(map(list,struct.iter_unpack('<QQ',raw)));expected=replay(rows)
  assert b64(source['normalized16'])==b''.join(struct.pack('<QQ',*r)for r in expected['rows'])
  assert source['utf8']==expected['utf8']
  for row in rows:
   for x in row:assert x>>32<len(actual['pointCounts'])and (x>>16&65535)<actual['pointCounts'][x>>32]and (x>>8&255)in list(range(200))+[255]
  text=b64(source['utf8']);caller=text
  if text and source['flags']&0x400000:caller=(b'{'if text[:1]==b'['else b'}')+text[1:]
  assert b64(source['callerUtf8'])==caller;total+=1
 assert actual['registryBefore']==actual['registryAfter']and actual['pendingBefore']==actual['pendingAfter']
assert len(a['actual'])==4
print(f'PASS independent2053 raw keys/allocation prefixes/finalize failures;4 real retained documents/{total} CHAR-WORD-TEXT strings, original IDs distinct from selection registry.')
