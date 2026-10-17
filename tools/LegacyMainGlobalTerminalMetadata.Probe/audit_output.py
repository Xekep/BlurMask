"""Raw-LK preorder/case-table oracle for actual compact-tag consumer controls.

The tested metadata reads are prepared consumers, not native21FFD0 events:
the current actual compact pools have no nonnegative field36 metadata marker.
"""
import base64
import hashlib
import json
import struct
import sys
from array import array
from pathlib import Path

if len(sys.argv) != 4:
    raise SystemExit('LK case-preparation-controls capture.json')
lk = Path(sys.argv[1]).read_bytes()
assert hashlib.sha256(lk).hexdigest() == 'd26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
classes = {r[0]: r[1] for r in json.loads(Path(sys.argv[2]).read_text(encoding='utf-8-sig'))[0]['rows']}
captures = json.loads(Path(sys.argv[3]).read_text())
assert len(captures) == 8
u32 = lambda b, p: struct.unpack_from('<I', b, p)[0]
i32 = lambda b, p: struct.unpack_from('<i', b, p)[0]
u16 = lambda b, p: struct.unpack_from('<H', b, p)[0]

def bits(data, width, index):
    offset, shift = divmod(index * width, 8)
    return int.from_bytes(data[offset:offset + 5], 'little') >> shift & ((1 << width) - 1)

d = lk[609980:2513123]
count, width = u32(d, 20), d[36]
assert count == 895571
size = ((count * width + 31) // 32) * 4
labels = d[37:37 + size]
position = 37 + size + 1
assert u32(d, position) == 0x1000000 and u32(d, position + 4) == count
child_width = d[position + 8]
children = d[position + 9:position + 9 + ((count * child_width + 31) // 32) * 4]
starts = array('I', [1])
for node in range(count):
    starts.append(starts[-1] + bits(children, child_width, node))
assert starts[-1] == count
needed = {row['tag'] & 0xffffffff for c in captures for row in c['metadata']}
word_ids, ordinal = {}, 0
stack = [(starts[0], starts[1])]
while stack:
    begin, end = stack[-1]
    if begin == end:
        stack.pop()
        continue
    stack[-1] = (begin + 1, end)
    if bits(labels, width, begin) & (1 << (width - 1)):
        if begin in needed:
            word_ids[begin] = ordinal
        ordinal += 1
    if starts[begin] != starts[begin + 1]:
        stack.append((starts[begin], starts[begin + 1]))
assert ordinal == 439826 and set(word_ids) == needed
categories = bytes.fromhex('090b0104040404040505050a010c040606060505050202020303030500000000')
checked = 0
for c in captures:
    compact = base64.b64decode(c['compact'])
    assert c['nativeMetadataMarkers36'] == sum(i32(compact, index * 40 + 36) != -1 for index in range(c['compactCount'])) == 0
    expected_indices = [index for index in range(c['compactCount'])
                        if compact[index * 40 + 4] == 1 and u32(compact, index * 40) in word_ids]
    assert [r['index'] for r in c['metadata']] == expected_indices
    for row in c['metadata']:
        actual = compact[row['index'] * 40:row['index'] * 40 + 40]
        tag, symbol = row['tag'], row['symbol']
        assert tag == int.from_bytes(actual[:8], 'little') and symbol == u16(actual, 28)
        state = tag >> 58 & 31
        classification = classes[symbol]
        category = 7 if tag & (1 << 40) and classification == 8 else 8 if classification == 9 else categories[state]
        token = bytearray(16)
        struct.pack_into('<III', token, 0, 1, word_ids[tag & 0xffffffff], category)
        token[13] = tag >> 42 & 1
        assert token == bytes.fromhex(row['token']), (c['owner'], row['index'], tag, symbol)
        checked += 1
assert checked == 4811 and captures[-1]['guards'] == 96
print(f'PASS independent rawLK/case metadata16 on {checked} actual compact terminal tags/8 owners; native marker36 events0; prepared consumer scope only')
