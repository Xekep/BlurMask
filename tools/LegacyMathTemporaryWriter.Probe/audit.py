from pathlib import Path
import json,sys,hashlib
root=Path(__file__).resolve().parent
fixtures=root/'fixtures.json';freeze=json.loads((root/'fixture-freeze.json').read_bytes())
assert hashlib.sha256(fixtures.read_bytes()).hexdigest()==freeze['sha256']
expected=json.loads(fixtures.read_bytes());capture=json.loads(Path(sys.argv[1]).read_bytes())
assert capture['Scope']=='PREPARED_TYPED_ARENA_ONLY_NO_CURRENT_OWNER_OR_APPLY_AUTHORITY'
assert capture['Guards']==9
assert len(capture['Results'])==len(expected)==freeze['count']==502
assert len({c['Name']for c in expected})==502
for case,result in zip(expected,capture['Results']):
 assert result['Name']==case['Name']
 actual={k:v for k,v in result.items()if k!='Name'}
 assert actual==case['Expected'],case['Name']
print('PASS independent frozen reference: 502 full node/link/scalar/freed/prefix states; prepared-only')
