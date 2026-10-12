"""Independent scalar quota model over immutable record/permutation heap oracle."""
import ast
import base64
import json
import random
import struct
import sys
from pathlib import Path

rng = random.Random(0x212ec0)


def b64(v):
    return base64.b64encode(v).decode()


def read(v, offset, fmt):
    return struct.unpack_from('<' + fmt, v, offset)[0]


path = Path(__file__).resolve().parent.parent / 'LegacyWordOrdering.Probe' / 'reference_controls.py'
module = ast.parse(path.read_text(encoding='utf8'))
exec(compile(ast.Module(body=[n for n in module.body if isinstance(n, ast.FunctionDef) and n.name == 'heap_add'],
                        type_ignores=[]), str(path), 'exec'))


def f32(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]


def capture(states):
    return [dict(cap=s['cap'], count=s['count'], tag=s['tag'], limits=s['limits'],
                 backing=b64(b''.join(s['storage']))) for s in states]


def apply(states, budget, share, phase, failure_at):
    requests = []
    drops = []

    def insert(index, record, callback=None):
        s = states[index]
        ok, s['count'], removed = heap_add(s['storage'], s['count'], record, s['cap'])
        if removed is not None and callback:
            callback(removed)
        return ok

    def promote(target, record):
        drops.append(dict(target=target, record=b64(record)))
        insert(target, record)

    def resize(index, cap, callback=None):
        s = states[index]
        s['cap'] = cap
        if len(s['storage']) < cap:
            old = b''.join(s['storage'])
            request = dict(index=index, capacity=cap, previous=b64(old), previousTag=s['tag'])
            requests.append(request)
            if len(requests) == failure_at:
                request.update(failed=True)
                return False
            tail = rng.randbytes((cap - len(s['storage'])) * 128)
            s['storage'] += [tail[i:i + 128] for i in range(0, len(tail), 128)]
            s['tag'] += 0x10000 + index
            request.update(failed=False, storage=b64(old + tail), tag=s['tag'])
        old_count = s['count']
        s['count'] = min(old_count, cap)
        for i in range(cap, old_count):
            source = s['storage'][i]
            if not insert(index, source, callback) and callback:
                callback(source)
        return True

    if sum(s['count'] for s in states) <= budget:
        return True, requests, drops
    positive = share > 0
    half = len(states) // 2
    primary_end = half if positive else len(states)

    def fallback(record):
        drops.append(dict(target=half, record=b64(record)))
        if not insert(half, record, lambda root: promote(0, root)):
            promote(0, record)

    if positive:
        for index in range(half + 1, len(states)):
            minimum, transition = states[index]['limits'][0 if phase == 0 else 3:2 if phase == 0 else 5]
            if minimum < states[index]['count']:
                if not resize(index, transition, lambda record, target=index - half: promote(target, record)):
                    return False, requests, drops
                if minimum < transition and not resize(index, minimum, fallback):
                    return False, requests, drops
    for index in range(1, primary_end):
        minimum, transition = states[index]['limits'][0 if phase == 0 else 3:2 if phase == 0 else 5]
        if minimum < states[index]['count']:
            if not resize(index, transition):
                return False, requests, drops
            if minimum < transition and not resize(index, minimum, lambda record: promote(0, record)):
                return False, requests, drops
    remaining = budget - sum(s['count'] for i, s in enumerate(states) if i and (not positive or i != half))
    while remaining < 1:
        candidates = [(read(s['storage'][0], 16, 'i'), -i, i)
                      for i, s in enumerate(states) if i and (not positive or i != half) and s['count']]
        worst = max(candidates)
        assert worst[0] > -0x4000000
        remaining += 1
        if not resize(worst[2], states[worst[2]]['count'] - 1):
            return False, requests, drops
    if positive:
        secondary_cap = int(f32(f32(remaining) * share))
        remaining -= secondary_cap
        if not resize(half, secondary_cap, lambda record: promote(0, record)):
            return False, requests, drops
    return resize(0, remaining), requests, drops


cases = []
for case in range(512):
    share = f32(rng.choice([0, -0.25, 0.25, 0.3, 0.5, 0.75, 1]))
    n = rng.choice([2, 4, 6, 8]) if share > 0 else rng.randrange(1, 8)
    states = []
    for index in range(n):
        cap = rng.randrange(1, 9)
        allocated = cap + rng.randrange(3)
        storage = [rng.randbytes(128) for _ in range(allocated)]
        count = 0
        for ordinal in range(rng.randrange(cap + 1)):
            record = bytearray(rng.randbytes(128))
            struct.pack_into('<i', record, 16, rng.choice([0, 1, 50, rng.randrange(-100, 101)]))
            struct.pack_into('<I', record, 120, (case << 16) | (index << 8) | ordinal)
            _, count, _ = heap_add(storage, count, bytes(record), cap)
        limits = [rng.randrange(4), rng.randrange(1, 13), cap,
                  rng.randrange(3), rng.randrange(1, 13), cap, index]
        states.append(dict(cap=cap, count=count, storage=storage, tag=0x10000000 + index, limits=limits))
    initial = capture(states)
    arc_limit = rng.randrange(8, 65)
    factor = f32(rng.choice([-0.25, 0.25, 0.3, 0.33333334, 0.5, 0.75, 1]))
    flag = rng.choice([0, 16])
    phase = rng.choice([0, 1, 2])
    budget = arc_limit if phase == 0 else int(f32(f32((2 if flag else 1) * factor) * f32(arc_limit))) & 0xffffffff
    failure_at = rng.choice([0, 0, 0, 1, 2])
    ok, requests, drops = apply(states, budget, share, phase, failure_at)
    cases.append(dict(initial=initial, arcLimit=arc_limit, factor=factor, flag=flag, phase=phase,
                      share=share, success=ok, growth=requests, drops=drops, expected=capture(states)))

# Fixed global root ties: quota excess is removed from the earliest nonzero
# bucket until empty before the next equal-root bucket. Minima are inactive.
tie_states = []
for index in range(3):
    storage = [rng.randbytes(128) for _ in range(6)]
    count = 0
    for ordinal in range(0 if index == 0 else 4):
        record = bytearray(rng.randbytes(128))
        struct.pack_into('<i', record, 16, 10)
        struct.pack_into('<I', record, 120, (index << 8) | ordinal)
        _, count, _ = heap_add(storage, count, bytes(record), 4)
    tie_states.append(dict(cap=4, count=count, tag=0x50000000 + index, storage=storage,
                           limits=[0xffffffff, 0xffffffff, 4, 0xffffffff, 0xffffffff, 4, index]))
tie_initial = capture(tie_states)
tie_ok, tie_requests, tie_drops = apply(tie_states, 2, 0, 0, 0)
cases[0] = dict(initial=tie_initial, arcLimit=2, factor=0.25, flag=0, phase=0, share=0,
                success=tie_ok, growth=tie_requests, drops=tie_drops, expected=capture(tie_states))

views = []
for _ in range(128):
    image = rng.randbytes(56)
    views.append(dict(image=b64(image), expected=[read(image, offset, 'I') for offset in range(24, 48, 4)] +
                      [read(image, 48, 'i')]))
with open(sys.argv[1], 'w', encoding='utf8') as stream:
    json.dump(dict(cases=cases, views=views), stream, separators=(',', ':'))
print(f'Wrote {len(cases)} native quota/failure/growth streams, '
      f'{sum(len(c["growth"]) for c in cases)} allocator requests, {len(views)} quota descriptor views')
