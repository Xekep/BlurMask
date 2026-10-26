"""Independent LIVE publication and prepared final-append byte audit.

The final subcapture keeps actual final input/output rows and terminal ordinal.
Rebuilding its fresh one-shot equivalent independently verifies that carrying
the already published prefix produces the same full compact and row bytes.
"""
import json
import subprocess
import sys
import tempfile
from pathlib import Path

global_mode=len(sys.argv)==7 and sys.argv[4]=='--global-publication'
if len(sys.argv) not in (4,7) or len(sys.argv)==7 and not global_mode:
    raise SystemExit('AK LK capture.json [--global-publication CASE PUBLICATION]')
root = Path(__file__).resolve().parents[2]
capture = Path(sys.argv[3]).resolve()
common = root / 'tools/LegacyMainCompactMaterialization.Probe/audit_output.py'
mode_args=['--global-publication',sys.argv[5],sys.argv[6]] if global_mode else ['--fresh-case']
subprocess.run([sys.executable,str(common),sys.argv[1],sys.argv[2],str(capture),*mode_args],check=True)
c = json.loads(capture.read_bytes())
if 'finalCompact' in c:
    assert not c['nativeGate'] and c['actualGraphNodeCount'] == 4
    assert c['finalPrefixOwners2488'] == [-1, -1, -1, 2, 3]
    final = dict(c)
    final.pop('publicationKind')
    if global_mode:final['publicationAllocatorInputs']=c['finalPublicationAllocatorInputs']
    final.update(nodeCount=c['actualGraphNodeCount'], graph=c['finalGraph'], inputNodes=c['finalInputNodes'],
                 nodes=c['finalNodes'], compact=c['finalCompact'], starts=c['finalStarts'],
                 count=c['finalCount'], selected=c['finalSelected'], winner=c['finalWinner'])
    with tempfile.TemporaryDirectory(prefix='live-final-oracle-', dir=capture.parent) as temporary:
        prepared = Path(temporary) / 'actual-final.json'
        prepared.write_text(json.dumps(final), encoding='utf-8')
        subprocess.run([sys.executable, str(common), sys.argv[1], sys.argv[2], str(prepared), *mode_args], check=True)
    print('PASS actual prepared final append equals independent one-shot bytes, including retained prefix; no expected text.')
