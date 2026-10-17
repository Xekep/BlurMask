"""Independent raw-order native62 replay; library entropy is not native092CF0."""
import base64,hashlib,importlib.util,json,sys
from pathlib import Path
s=importlib.util.spec_from_file_location('oracle',Path(__file__).with_name('reference_controls.py'));o=importlib.util.module_from_spec(s);s.loader.exec_module(o)
a=json.loads(Path(sys.argv[1]).read_bytes());controls=json.loads(Path(sys.argv[2]).read_bytes())['sequences'];b64=base64.b64decode
count=0
for row,expected in zip(a['prepared'],controls):
 assert row==expected
 state=row['seed']
 for actual in row['values']:
  state,raw=o.draw(state);assert b64(actual['raw'])==raw and actual['text']==o.format_raw(raw)
  assert raw[6]>>4==4 and raw[8]>>6==2
  count+=1
 assert state==row['finalState']
assert len(a['prepared'])==len(controls)==1024
for row in a['sameIdRetries']:
 state,first=o.draw(row['seed']);state,second=o.draw(state)
 assert b64(row['previous'])==first and b64(row['actual'])==second and row['stateAfter']==state
assert len(a['sameIdRetries'])==128
for actual in a['actualSeeds']:
 state,raw=o.draw(actual['libraryInitialState'])
 assert actual['stateAfter']==state and actual['alternatives']==2
 assert b64(actual['identifier']['raw'])==raw and actual['identifier']['text']==o.format_raw(raw)
assert len(a['actualSeeds'])==3
stable=json.dumps(dict(prepared=a['prepared'],sameIdRetries=a['sameIdRetries']),separators=(',',':')).encode()
print(f'PASS independent1024 prepared sequences/{count} UUIDs/128 same-ID retries/3 actual library seeds; stable prepared SHA256 {hashlib.sha256(stable).hexdigest()}; actual entropy intentionally nondeterministic.')
