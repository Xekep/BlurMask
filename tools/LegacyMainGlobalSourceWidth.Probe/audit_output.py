"""Independent2188F0 width accumulator and actual protocol-packet binding.

Inputs are recorded raw rows/compact bytes and live source bounds, not expected
text. This audits a consumer of the current partial controller, not its winner.
"""
import base64, json, struct, sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('capture.json')
u16 = lambda b, o: struct.unpack_from('<H', b, o)[0]
i32 = lambda b, o: struct.unpack_from('<i', b, o)[0]
u32 = lambda b, o: struct.unpack_from('<I', b, o)[0]
f32 = lambda x: struct.unpack('<f', struct.pack('<f', x))[0]
fbits = lambda x: struct.unpack('<I', struct.pack('<f', x))[0]
dbits = lambda x: struct.unpack('<Q', struct.pack('<d', x))[0]
decode = base64.b64decode
data = json.loads(Path(sys.argv[1]).read_bytes())
assert len(data) == 8
ordinary = protocol = contributions = 0
different_means = set()
for owner, c in enumerate(data):
    assert c['owner'] == owner and c['topAC'] == 2 and c['metadataEvents'] == 0
    compact = decode(c['compact'])
    assert c['recordCount'] <= len(compact) // 40
    count = 0
    value = 0.0
    assert len(c['flushes']) == 2
    for flush in c['flushes']:
        rows = decode(flush['rows72'])
        assert len(rows) % 72 == 0 and len(flush['bounds']) == len(rows) // 72
        assert [p['ordinal'] for p in flush['producers']] == list(reversed(range(len(rows) // 72)))
        for p in flush['producers']:
            ordinal = p['ordinal']
            row = rows[ordinal * 72:(ordinal + 1) * 72]
            index = i32(row, 8)
            assert index >= 0
            if index < c['recordCount']:
                subtype = compact[index * 40 + 32] & 7
                excluded = subtype == 2 or (subtype not in (4, 6) and u16(row, 56) != 65535 and ((u16(row, 58) + 6) & 65535) <= 2)
                if not excluded:
                    lo, hi = map(f32, flush['bounds'][ordinal])
                    value += f32(hi - lo)
                    count = ((count + 1 + 2**31) % 2**32) - 2**31
                    contributions += 1
            assert p['subtype'] == row[60] and p['label'] == u16(row, 58)
            if row[60] == 0:
                assert p['remaining'] == 255 and p['emitted'] == u16(row, 58)
                assert p['score0C'] == fbits(f32(row[62] / 255.0))
                assert p['request64'] == ''
                ordinary += 1
            else:
                assert row[60] == 3 and u16(row, 58) == 65530
                assert p['remaining'] == 256 and p['emitted'] == 65535
                assert p['score0C'] == p['score10'] == fbits(1.0) and p['cost14'] == 0
                request = decode(p['request64'])
                assert len(request) == 64
                assert u32(request, 44) == c['topAC'] and u32(request, 48) == c['topB0Bits']
                protocol += 1
    assert c['count'] == count and dbits(c['sum']) == dbits(value)
    expected = fbits(value / count if count > 0 else 0)
    assert c['topB0Bits'] == expected
    different_means.add(expected)
assert ordinary == 23 and protocol == 8 and len(different_means) >= 5
print(f'PASS independent8 global width consumers/{contributions} exact float-width/double contributions/{ordinary} ordinary and{protocol} real FFFA packet bindings; partial controller, no WORD/publication claim')
