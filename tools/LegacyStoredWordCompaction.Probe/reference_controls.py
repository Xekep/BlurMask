"""Independent literal88-row quicksort/dedup/payload-swap oracle."""
import argparse
import json
import random
import struct
from pathlib import Path

r = random.Random(0x212B10)

def read(b, offset, fmt):
    return struct.unpack_from('<' + fmt, b, offset)[0]

def compare(a, b, headers):
    ka = (read(a, 40, 'Q'), a[33] & 63, a[39] & 1, read(a, 36, 'H'), read(a, 24, 'I'), read(a, 28, 'H'), a[33] >> 7)
    kb = (read(b, 40, 'Q'), b[33] & 63, b[39] & 1, read(b, 36, 'H'), read(b, 24, 'I'), read(b, 28, 'H'), b[33] >> 7)
    if ka != kb:
        return -1 if ka < kb else 1
    if a[4] == b[4]:
        return 0
    if headers[a[4]] & 0x1E00 == 0x1400:
        return -1
    return int(headers[b[4]] & 0x1E00 == 0x1400)

comparisons = []
for index in range(4096):
    a, b = bytearray(r.randbytes(88)), bytearray(r.randbytes(88))
    headers = [0, 0x1400, 0x1400, 0xC00, 0x200, 0xA00]
    a[4], b[4] = index % 6, index // 6 % 6
    # Progressively force all equal keys to reach every later comparison.
    for offset, size in ((40, 8), (33, 1), (39, 1), (36, 2), (24, 4), (28, 2))[:index % 7]:
        b[offset:offset + size] = a[offset:offset + size]
    if index % 7 == 6:
        b[33] = a[33]
    comparisons.append(dict(a=a.hex(), b=b.hex(), headers=headers, expected=compare(a, b, headers)))

def compact(rows, payloads, primary, active, headers):
    def swap(i, j):
        rows[i], rows[j] = rows[j], rows[i]
        payloads[i], payloads[j] = payloads[j], payloads[i]

    def quicksort(lower, upper):
        while True:
            left, right = lower, upper
            pivot = rows[(upper - lower) // 2 + lower][:]
            while True:
                while compare(rows[left], pivot, headers) < 0:
                    left += 1
                    if left > upper:
                        raise ValueError('partition')
                while compare(rows[right], pivot, headers) > 0:
                    right -= 1
                    if right < lower:
                        raise ValueError('partition')
                if right < left:
                    break
                swap(left, right)
                left += 1
                if right == 0:
                    raise ValueError('ushort-wrap')
                right -= 1
                if left > right:
                    break
            if lower < right:
                quicksort(lower, right)
            lower = left
            if upper <= left:
                return

    if active <= primary:
        return active
    quicksort(primary, active - 1)
    leader = primary
    for next_row in range(primary + 1, active):
        if compare(rows[leader], rows[next_row], headers) == 0:
            if read(rows[next_row], 16, 'i') < read(rows[leader], 16, 'i'):
                swap(leader, next_row)
        else:
            leader += 1
            swap(leader, next_row)
    if not any(0x200 <= headers[rows[i][4]] & 0x1E00 <= 0xA00 for i in range(primary, leader + 1)):
        for i in range(leader + 1, active):
            if 0x200 <= headers[rows[i][4]] & 0x1E00 <= 0xA00:
                swap(leader + 1, i)
                leader += 1
                break
    return leader + 1

cases = []
for index in range(1024):
    primary, active, alternatives = 1 + index % 4, 1 + index % 4 + 1 + index % 23, 2 + index % 3
    capacity = active + 2
    headers = [0, 0xC00, 0x200, 0xA00]
    rows = [bytearray(r.randbytes(88)) for _ in range(capacity)]
    for i in range(primary, active):
        source = 1 + i % 3
        rows[i][4] = source
        rows[i][33] = rows[i][39] = 0
        struct.pack_into('<Q', rows[i], 40, (i // 3) % 4)
        struct.pack_into('<H', rows[i], 36, i // 5 % 3)
        struct.pack_into('<I', rows[i], 24, i // 7 % 3)
        struct.pack_into('<H', rows[i], 28, 0)
        struct.pack_into('<i', rows[i], 16, r.choice([0, 100, -2**31, 2**31 - 1, -100]))
    payloads = [r.randbytes(alternatives * 40) for _ in range(capacity)]
    original, original_payloads = b''.join(rows), b''.join(payloads)
    retained = compact(rows, payloads, primary, active, headers)
    cases.append(dict(rows=original.hex(), payloads=original_payloads.hex(), primary=primary,
                      active=active, count=alternatives, headers=headers, expectedRows=b''.join(rows).hex(),
                      expectedPayloads=b''.join(payloads).hex(), expectedCount=retained))

# Cheapest equivalent C00 would discard the0200 dictionary. The native fallback
# must recover it, including its distinct owned80-byte alternative payload.
rows = [bytearray(88) for _ in range(4)]
rows[1][4], rows[2][4] = 1, 2
struct.pack_into('<i', rows[1], 16, 0)
struct.pack_into('<i', rows[2], 16, 100)
payloads = [bytes([i]) * 80 for i in range(4)]
original, original_payloads = b''.join(rows), b''.join(payloads)
retained = compact(rows, payloads, 1, 3, [0, 0xC00, 0x200])
cases.append(dict(rows=original.hex(), payloads=original_payloads.hex(), primary=1, active=3, count=2,
                  headers=[0, 0xC00, 0x200], expectedRows=b''.join(rows).hex(), expectedPayloads=b''.join(payloads).hex(), expectedCount=retained))

for index in range(64):
    active = 3 + index % 9
    headers = [0, 0x1400, 0x1400, 0xC00]
    rows = [bytearray(88) for _ in range(active + 1)]
    for i in range(1, active):
        rows[i][4] = 1 + (i + index) % 3
        struct.pack_into('<i', rows[i], 16, i * 17 - 40)
        rows[i][84] = i
    payloads = [bytes([i]) * 80 for i in range(active + 1)]
    original, original_payloads = b''.join(rows), b''.join(payloads)
    try:
        retained = compact(rows, payloads, 1, active, headers)
    except ValueError:
        cases.append(dict(rows=original.hex(), payloads=original_payloads.hex(), primary=1, active=active,
                          count=2, headers=headers, unsupported=True))
    else:
        cases.append(dict(rows=original.hex(), payloads=original_payloads.hex(), primary=1, active=active,
                          count=2, headers=headers, expectedRows=b''.join(rows).hex(),
                          expectedPayloads=b''.join(payloads).hex(), expectedCount=retained))

p = argparse.ArgumentParser()
p.add_argument('output', type=Path)
a = p.parse_args()
a.output.parent.mkdir(parents=True, exist_ok=True)
a.output.write_text(json.dumps(dict(comparisons=comparisons, compactions=cases), separators=(',', ':')), encoding='utf-8')
print(f'Generated{len(comparisons)} comparisons and{len(cases)} full-storage/payload compactions')
