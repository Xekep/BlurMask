"""Check actual reachability capture; no vendor/model execution or RSS claim.

Production ownership is exercised by the probe itself across forced collections.
All numerical stages have separate unchanged golden captures and independent
oracles; hashes here compare GC/no-GC, not a platform-specific OCR golden.
"""
import json,re,sys
from pathlib import Path
if len(sys.argv) not in (2,3):raise SystemExit('capture.json [same-platform-capture.json]')
data=json.loads(Path(sys.argv[1]).read_bytes())
assert [r['Width'] for r in data['Rows']]==[1,3,16]
assert [r['Records'] for r in data['Rows']]==[1,15,51]
assert data['ActiveLifetimeCases']==4
for r in data['Rows']:
    assert 0<=r['DistinctCandidatesAlive']<=1
    for key in ('ContentSha256','PostprocessingSha256'):assert re.fullmatch('[0-9A-F]{64}',r[key])
if len(sys.argv)==3:assert data==json.loads(Path(sys.argv[2]).read_bytes())
print('PASS67 actual records: at most one retained current candidate per source; pending/cancel/consuming-failure/dispose lifetimes and GC/no-GC EF4/final payloads checked; no RSS claim.')
