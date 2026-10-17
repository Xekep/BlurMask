"""Pinned main alternative arithmetic and final mass/rank/TOP80 operations.
Read-only PE analysis; no vendor code is loaded or executed.
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
    (0x1802276a4, 0x180227756, '4798ac3aca9a74368ab3ccabffd7b8eab33043e28b9a862e76e3cbb1083e4d73'),
    (0x18013cba0, 0x18013ccb1, '2cc467b21bf2b56304647610120c3caab7cdd3a8303eca016ee59e2ac092ae3e'),
    (0x180227c88, 0x180227d5f, '2f3b553a00a072355cd827f7f443afb17231cc5c8eb1e9cdb119c03498ca375d'),
]
for a, z, sha in regions:
    assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest() == sha
pins = [
    (0x1802276ac, '0f5bc0'), (0x1802276af, 'f3410f59c5'), (0x1802276b4, 'f3410f114514'),
    (0x1802276bd, '2bc3'),             #signed32 SUB before conversion.
    (0x1802276c6, 'c1e00d'),           #Native32 SHL13 wrap, not checked long multiplication.
    (0x1802276c9, '0f5bc9'), (0x1802276d0, '0f5bc0'),
    (0x1802276d3, 'f30f5ec8'), (0x1802276d7, 'f3410f114d10'),
    (0x18022770f, '3d35010000'),       #kind0x135, no script/language heuristic.
    (0x180227716, '410f28c2'), (0x18022771c, '410f28c4'),
    (0x180227720, '410f57c0'), (0x180227724, 'f3410f594510'),
    (0x18022772a, '440f2fd87605'), (0x180227735, '0f2fc77606'), #Inclusive clamps; unordered goes exp.
    (0x180227744, 'e827720200'), (0x18022774c, 'f20f5ac8'), (0x180227750, 'f3410f114d08'),
    (0x18013cba0, '0fbf4260'), (0x18013cbab, '83f801'),
    (0x18013cbef, 'f30f584008'), (0x18013cbf4, 'f30f5ec8'),
    (0x18013cc78, 'f30f584808'), (0x18013cc7d, 'f30f5ec1'),
    (0x18013cc9c, 'f30f5ec1'), (0x18013cca9, 'c740080000803f'),
    (0x180227c9f, 'f30f107808'),       #Raw first mass saved BEFORE273.V60.
    (0x180227cbd, '4c8b4060'), (0x180227cc1, '41ffd0'),
    (0x180227ce0, 'f30f5d31'),         #MINSS over14, including native unordered/source selection.
    (0x180227cfb, 'f30f5cc6'), (0x180227cff, '410f57c0'),
    (0x180227d03, 'e8384b0300'), (0x180227d08, 'f30f11441e0c'),
    (0x180227d20, 'c7400c0000803f'),
    (0x180227d27, '440f2fe7760a'),     #Rawmass<1 multiplies; unordered JBE divides.
    (0x180227d2d, 'f30f593d43540c00'),
    (0x180227d3f, 'f30f5ec7'), (0x180227d47, 'f30f5cf8'),
    (0x180227d53, 'f30f5dc7'), (0x180227d57, 'f30f118780000000'),
]
for at, raw in pins:
    assert b.data[b.off(at):b.off(at) + len(raw)//2].hex() == raw, hex(at)
constants = [
    (0x1802ed7a0, '00000080'), (0x1802ed194, '0000803f'),
    (0x1802ed0a8, '00000039'), (0x1802ed3a8, '0000a040'),
    (0x1802ed520, '000096c2'), (0x1802ed418, '00009642'),
    (0x1802ed46c, '601f9375'), (0x1802ed178, '6666663f'), (0x1802ed0e4, 'cdcccc3d'),
]
for at, raw in constants:
    assert b.data[b.off(at):b.off(at)+4].hex() == raw
assert b.qwords(0x1802dc740 + 0x60, 1)[0] == 0x18013cba0
report = dict(regions=[dict(start=hex(a), end=hex(z), sha256=h) for a, z, h in regions],
              exactPins=len(pins), exactConstants=len(constants), binding='273.V60=13CBA0',
              arithmetic='signed32(total-reference) then float; signed32(nodeCount<<13), no guessed cumulative addition',
              normalization='backward pairwise08; expf(-(14-min)) to0C; raw-first-mass cap of explicit oldTOP80',
              excluded=['273 allocation/list ownership', 'terminal selection', '225FB0 cast order', 'text acceptance'],
              noVendorExecution=True)
Path(sys.argv[1]).write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print(f'PASS3 native blocks/{len(pins)} instruction pins/{len(constants)} constants and273.V60 binding; no vendor execution.')
