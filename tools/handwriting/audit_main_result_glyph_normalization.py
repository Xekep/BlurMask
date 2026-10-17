"""Pinned21F320 entry guards, liveF4 property and same-container attachment.
Static PE reads only; this is not execution/validation of the enabled rewrite.
"""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x18021f320, 0x18021fbf0, '0b5ca736469edf51655bbbaad4f629afc31cda0122fccddc50365bbc55fb5e7c'),
    (0x18021f210, 0x18021f320, '7f77864c48059eea26486fe94e310f63695229d8651564a8781406050a4f1494'),
    (0x18000cab0, 0x18000cac0, 'f92bdc6ba779a355e448989b79fa42c997ce6a3c59e739b2cbabe48f45a62341'),
    (0x18000eaf0, 0x18000eb20, 'f5e049a2e6a4c255d20ff65391a5cbcb897f3b06738995f8ba95e5876fdee1bf'),
    (0x18000eca0, 0x18000ed90, '86cd7866fbf374997811e5fe53671f5379b6778ad5603f6a31afe339fd55f614'),
    (0x18020c8c0, 0x18020c9d0, '83fb4f5184c36e45f8494abc50864677ddfd99152e49e6b02fa2a694bf10da74'),
]
for start, end, sha in regions:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == sha
pins = [
    (0x18000c8a8, '33c0'), (0x18000c923, '668987f4000000'),
    (0x18000cab0, '4c894260b0014d85c0740449895018c3'),
    (0x1801eb464, '498b04244c8bc6498bd4488bcd4c8b481841ffd1'),
    (0x18000eaf0, '0fb682f4000000418800b001c3'),
    (0x18000eb05, '410fb6008882f4000000b001c3'),
    (0x18021f337, '488b4218'),
    (0x18021f344, '80b8f4000000000f8488080000'),
    (0x18021f35f, '664439a216250000'),
    (0x18021f369, '664439a214250000'),
    (0x18021f373, '664439a212250000'),
    (0x18021f393, '498b7918'),  #First WORD dereference comes AFTER both guards.
    (0x18022198e, 'e85de2ffff'),
    (0x180221993, '4c8b57184180ba99000000007543'),
    (0x1802219ad, 'e86ed9ffff'),
    (0x18020c605, '66898310250000'), (0x18020c62a, '66898312250000'),
    (0x18020c649, '66898314250000'), (0x18020c668, '66898316250000'),
    (0x18020c27a, '668983be240000'),
]
for address, raw in pins:
    actual = b.data[b.off(address):b.off(address)+len(raw)//2].hex()
    assert actual == raw, (hex(address), actual, raw)
assert b.qwords(0x180274af0+0x18, 1)[0] == 0x18000cab0
assert b.qwords(0x180274af0+0x70, 1)[0] == 0x18000eba0
assert b.qwords(0x1802751e8, 2) == (0x18000eaf0, 0x18000eb00)
assert b.data[b.off(0x1802751e0):b.off(0x1802751e0)+4].hex() == '17002d01'
for address, raw in [(0x1802ecbbc,'e2809c'),(0x1802e9c14,'e2809d'),
                     (0x1802ecbc8,'e28098'),(0x1802e9c10,'e28099'),(0x1802e9c18,'e2809e')]:
    assert b.data[b.off(address):b.off(address)+3].hex() == raw
report = dict(regions=[dict(start=hex(s),end=hex(e),sha256=h) for s,e,h in regions], pins=len(pins),
              propertyId='0x012D0017', freshF4=0, setter='00EB00', propertyCopy='00EBA0',
              ownership='00CAB0 stores identical2B3 into controller18; fresh typed APIs accept no property packets',
              firstWordRead='21F393 after F4 and replacement-availability guards',
              enabledBody='Not ported;21F210 iterator, three-label lookahead, source-kind and category predicates remain separate',
              noVendorExecution=True)
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS6 native regions/{len(pins)} pins/4 bindings/5 UTF8 constants; enabled21F320 remains unported.')
