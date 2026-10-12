"""Own immutable snapshots for 212d80 reset and 016a20 release ordering."""
import base64
import copy
import json
import random
import struct
import sys
from pathlib import Path

rng = random.Random(0x212D80)
def b64(data):
    return base64.b64encode(data).decode()
def raw(size):
    return rng.randbytes(size)

cases = []
for case in range(512):
    phase = rng.choice([0, 1, -1, 17])
    trace = case % 4 != 0
    before = []
    for index in range(rng.randrange(0, 7)):
        capacity = rng.randrange(0, 9)
        allocation = capacity + rng.randrange(0, 5)
        count = rng.randrange(capacity + 1)
        data = bytearray(raw(allocation * 128))
        for slot in range(count):
            struct.pack_into('<i', data, slot * 128 + 16, 1000 - slot)
        limits = [rng.randrange(16) for _ in range(6)] + [rng.randrange(-1, 5)]
        before.append(dict(cap=capacity, count=count, tag=index + 71,
                           backing=b64(data), limits=limits))
    traces = []
    for vector in range(2):
        count = (case + vector * 3) % 13
        traces.append(dict(count=count, tag=vector + 981, used=rng.randrange(10000),
                           backing=b64(raw((count + rng.randrange(3)) * 16))))
    after = copy.deepcopy(before)
    after_traces = copy.deepcopy(traces)
    can_fail = [i for i, item in enumerate(before)
                if item['limits'][2 if phase == 0 else 5] > len(base64.b64decode(item['backing'])) // 128]
    fail = rng.choice(can_fail) if can_fail and case % 3 == 0 else -1
    growth = []
    succeeded = True
    for index, item in enumerate(after):
        item['count'] = 0
        item['cap'] = item['limits'][2 if phase == 0 else 5]
        data = base64.b64decode(item['backing'])
        if item['cap'] > len(data) // 128:
            if index == fail:
                growth.append(dict(index=index, capacity=item['cap'], failed=True))
                succeeded = False
                break
            data += raw(item['cap'] * 128 - len(data))
            item['backing'] = b64(data)
            item['tag'] = index + 100001
            growth.append(dict(index=index, capacity=item['cap'], failed=False,
                               backing=item['backing'], tag=item['tag']))
    released = []
    if succeeded and trace:
        for vector, item in enumerate(after_traces):
            data = bytearray(base64.b64decode(item['backing']))
            if item['count'] < 10:
                for slot in range(item['count']):
                    struct.pack_into('<H', data, slot * 16 + 8, 0)
                item['backing'] = b64(data)
            else:
                for slot in range(item['count']):
                    released.append([vector, slot, struct.unpack_from('<Q', data, slot * 16)[0]])
                released.append([vector, -1, item['tag']])
                item.update(backing='', count=0, tag=0)
            item['used'] = 0
    cases.append(dict(phase=phase, trace=trace, before=before, traces=traces,
                      after=after, afterTraces=after_traces, growth=growth,
                      released=released, succeeded=succeeded))

out = Path(sys.argv[1])
out.parent.mkdir(parents=True, exist_ok=True)
out.write_bytes((json.dumps(dict(cases=cases), separators=(',', ':')) + '\n').encode())
print('Generated 512 complete reset snapshots with allocation failures and 9/10 trace boundary')
