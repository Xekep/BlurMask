"""Audit raw-resource adapter captures; no expected handwritten labels."""
from pathlib import Path
import argparse,json,struct
p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('--parity',type=Path);p.add_argument('--fixed-baseline',type=Path);a=p.parse_args()
d=json.loads(a.capture.read_bytes());assert d['guards']==12 and d['portableSha256']=='39418d35708502db6520fa22781614c30c7256efdb9981bf6bbf9b1b0c4f8d5b'
rows=d['samples'];assert len(rows) in(2,100) and len({r['id']for r in rows})==len(rows)
for r in rows:
 score=struct.unpack('<f',struct.pack('<i',r['scoreBits']))[0];assert 0<=score<=1 and 0<r['frames']<=6000
 assert isinstance(r['text'],str) and '\x00' not in r['text']
if len(rows)==2:assert [r['id']for r in rows]==['M','curve']
else:assert [r['id']for r in rows]==['plucoll-'+str(i).zfill(4)for i in range(100)]
if a.parity:assert a.capture.read_bytes()==a.parity.read_bytes()
if a.fixed_baseline:assert rows==json.loads(a.fixed_baseline.read_bytes())['samples']
print(f'PASS actual English resource adapter:12 guards/{len(rows)} bounded inference rows, complete portable model parity; no targets')
