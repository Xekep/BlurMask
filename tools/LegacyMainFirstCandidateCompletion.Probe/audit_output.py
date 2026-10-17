"""Original raw AK/glyph byte oracle for bounded first-candidate completion."""
import base64
import hashlib
import importlib.util
import json
import struct
import sys
from pathlib import Path

if len(sys.argv) != 3:
    raise SystemExit('AK.res actual-capture.json')
root = Path(__file__).resolve().parents[2]
ak = Path(sys.argv[1]).read_bytes()
assert hashlib.sha256(ak).hexdigest() == '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec = importlib.util.spec_from_file_location('raw_al', root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
alphabet = module.read_alphabet(ak)
o = json.loads(Path(sys.argv[2]).read_bytes())
assert len(o['inputs']) == 5 and o['guards'] == 38
def u16(row, at): return struct.unpack_from('<H', row, at)[0]
def f32(value): return struct.unpack('<f', struct.pack('<f', value))[0]
def emit(row):
    kind, label = u16(row, 4)&7, u16(row, 8)
    count = (label>>2)&8191 if kind in (1, 3) else 1
    if count == 0: return b''
    assert kind not in (3, 4)
    if kind == 1: label = ((label&3)-7)&65535
    elif kind not in (0, 2): label = 65535
    special = {65532:'\xa0',65531:' ',65529:'\n',65526:chr(0xf0000),65523:chr(0xf0010),
               65522:chr(0xf0011),65521:chr(0xf0012),65520:chr(0xf0013),65519:chr(0xf0014),
               65518:chr(0xf0015),65517:chr(0xf0016),65516:chr(0xf0017),65515:chr(0xf0018)}
    text = alphabet[label] if label < 192 else special.get(label, '\ufffd')
    assert chr(0xf0000) not in text
    return text.encode('utf8')*count
words = glyphs = 0
for case in o['inputs']:
    assert case['sourceNodes'] > 0 and case['reference'] == case['total']
    allocation = bytes.fromhex(case['allocation']); expected = bytearray(allocation)
    expected[:40] = bytes(40)
    struct.pack_into('<f', expected, 8, 1.)
    struct.pack_into('<f', expected, 20, f32(f32(case['row14'])/8192))
    assert bytes.fromhex(case['scored']) == expected
    struct.pack_into('<f', expected, 12, 1.)
    assert bytes.fromhex(case['final']) == expected
    for word in case['nativeHeadOrderWords']:
        storage = bytes.fromhex(word['word120']); refs = [g['reference'] for g in word['glyphs']]
        assert len(storage) == 120 and struct.unpack_from('<i', storage, 40)[0] == len(refs)
        assert struct.unpack_from('<Q', storage, 8)[0] == refs[-1]
        pieces = []
        for glyph in word['glyphs']:
            row = bytes.fromhex(glyph['record']); assert len(row) == 40
            assert glyph['source'] == glyph['reference'] & (0xffffff | 0xffff000000 | (1<<56))
            assert glyph['associatedRows']
            if glyph['protocol']: assert glyph['original'] is None and glyph['ranges'] == []
            else:
                assert glyph['original'] == glyph['source'] and glyph['ranges']
                assert all(0 <= a < b for stroke,a,b in glyph['ranges'])
            raw = emit(row); assert base64.b64decode(glyph['rawUtf8']) == raw
            pieces.append(raw); glyphs += 1
        assert base64.b64decode(word['rawUtf8']) == b''.join(pieces)
        words += 1
fault = o['preparedPostInferenceFault']; before, after = bytes.fromhex(fault['before']), bytes.fromhex(fault['after'])
assert u16(before, 8) == 193 and u16(after, 8) == 134 and fault['retryRefused']
assert before[:8] == after[:8] and before[10:] == after[10:]
assert words == o['words'] == 5 and glyphs == o['glyphs']
print(f'PASS independent5 actual completions/{words} separate WORD/{glyphs} raw glyph UTF8/source keys/38 guards; prepared quote-prefix failure retained.')
