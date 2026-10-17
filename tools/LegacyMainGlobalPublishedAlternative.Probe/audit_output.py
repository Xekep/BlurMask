"""Independent published callback bytes and first273 score storage.
The existing glyph oracle keeps all byte/math checks; only its fixed two
no-winner fixture guard total is replaced with this probe's explicit total.
"""
import ast
import base64
import json
import struct
import sys
from pathlib import Path
if len(sys.argv) != 3:
    raise SystemExit('AK capture.json')
root = Path(__file__).resolve().parents[2]
capture = Path(sys.argv[2]).resolve()
d = json.loads(capture.read_bytes())
assert len(d['owners']) == 2 and d['noWinnerGuards'] == 3
assert d['snapshotCount'] == 512 and d['guards'] == 219
source = root/'tools/LegacyMainGlobalPublishedGlyphs.Probe/audit_output.py'
tree = ast.parse(source.read_text()); keep = []; removed = []
for node in tree.body:
    if isinstance(node, ast.Assert) and "d['noWinnerGuards'] == 2" in ast.unparse(node):
        removed.append(node)
    else:
        keep.append(node)
assert len(removed) == 1
exec(compile(ast.Module(body=keep,type_ignores=[]),str(source),'exec'),
     {'__file__':str(source),'__name__':'__main__'})
def raw(x):
    return base64.b64decode(x,validate=True)
def f32(x):
    return struct.unpack('<f',struct.pack('<f',x))[0]
verified = 0
for owner in d['owners']:
    selected = raw(owner['selectedBeam'])
    assert len(selected) == 88 and owner['nodes'] == 4
    index = owner['starts'][-2] + struct.unpack_from('<H',selected,0x22)[0]
    assert index == 1019 and 0 <= index < owner['recordCount']
    row_cost = f32(f32(owner['rowCost14']) * f32(1/8192))
    for variant in owner['cases']:
        assert len(variant['snapshots']) == 64
        for seed, case in enumerate(variant['snapshots']):
            assert case['seed'] == seed and case['reference'] == owner['total']
            residue = raw(case['residue'])
            assert residue == bytes((i*17+seed*29+7)&255 for i in range(400))
            constructed = bytearray(residue)
            constructed[:40] = bytes(40)
            struct.pack_into('<I',constructed,20,0x7f7fffff)
            assert raw(case['constructed']) == constructed
            scored = bytearray(constructed)
            #Signed integer total-reference SUB produces zero for first result.
            #Native source-count division preserves zero, then exp yields one.
            struct.pack_into('<f',scored,8,1.)
            struct.pack_into('<f',scored,16,0.)
            struct.pack_into('<f',scored,20,row_cost)
            assert raw(case['scored']) == scored
            normalized = bytearray(scored)
            struct.pack_into('<f',normalized,12,1.)
            assert raw(case['normalized']) == normalized
            assert raw(case['publishedBeam']) == selected
            assert case['rawFirst08'] == 0x3f800000
            assert case['top80'] & 0xffffffff == 0xbf800000
            verified += 1200
print(f'PASS independent published first273:512 allocations/{verified} bytes/exact post207CB0 row88 and compact row22;219 guards; raw mass is not acceptance confidence')
