"""Original204290 auxiliary-page/request/injection oracle; only output argument."""
import json
import random
import struct
import sys
from pathlib import Path

if len(sys.argv) != 2 or Path(sys.argv[1]).suffix.lower() != '.json':
    raise SystemExit('Usage: reference_controls.py controls-output.json (one output argument only)')

rng = random.Random(0x204891)
cases = []
for index in range(512):
    capacity = 1 + index % 8
    row = bytearray(rng.randbytes(88))
    struct.pack_into('<I', row, 0, 1 + index)
    row[4] = 1
    row[5] = (row[5] & ~2) | (2 if index % 3 else 0)
    row[6] = (row[6] & ~1) | (1 if index % 13 == 0 else 0)
    row[7] = (row[7] & ~1) | (1 if index % 11 == 0 else 0)
    span = [-128, -1, 0, 1, 14, 127][index % 6]
    row[32] = span & 255
    row[38] = rng.randrange(256)
    struct.pack_into('<H', row, 36, rng.randrange(65536))
    buffer = bytearray(rng.randbytes(capacity * 32))
    residue = buffer.hex()
    mode, limit = index % 2, rng.randrange(256)
    frontend = index % 3 != 0
    flags = rng.randrange(65536) if frontend else 0
    limited = bool(flags & 3) if frontend else bool(mode)
    boundary = index % 3 != 0
    source_mode = index % 17 == 0
    pending = bool(row[5] & 2) and not row[7] & 1
    bypass = source_mode or bool(row[6] & 1)
    supplied = []
    for count in [capacity, capacity, index % capacity, 0, 0]:
        records = bytearray(rng.randbytes(count * 32))
        #Aggregate owns byte04 stamping; synthetic child owns all other bytes.
        supplied.append(records.hex())
    abort_at = 0 if index % 7 else 1 + index % 3
    pages = []
    reads = []
    skip = 0
    source_reads = 0
    completed = True
    while True:
        records = bytearray()
        if not bypass:
            reads.append([index + 1, skip, capacity, 0,
                          max(0, limit - row[38]) if limited else 0,
                          8 + index, span & 0xffffffff,
                          4 if boundary and flags & 16 else 0, flags, 0])
            records = bytearray.fromhex(supplied[source_reads])
            source_reads += 1
            for offset in range(4, len(records), 32):
                records[offset] = 1
            buffer[:len(records)] = records
        returned = len(records) // 32
        skip += returned
        inject = pending and returned < capacity
        if inject:
            at = returned * 32
            buffer[at:at + 8] = row[:8]
            buffer[at + 8:at + 20] = bytes(12)
            buffer[at + 20:at + 22] = row[36:38]
            buffer[at + 22:at + 24] = bytes(2)
            pending = False
        count = returned + int(inject)
        if count == 0:
            break
        pages.append(dict(returned=returned, skip=skip, injected=inject,
                          records=buffer[:count * 32].hex(), buffer=buffer.hex()))
        if abort_at and len(pages) == abort_at:
            completed = False
            break
    cases.append(dict(row=row.hex(), residue=residue, capacity=capacity,
                      mode=mode, limit=limit, flags=flags, boundary=boundary,
                      frontend=frontend, sourceMode=source_mode, owner=8 + index, supplied=supplied,
                      abortAt=abort_at, reads=reads, pages=pages,
                      completed=completed, finalSkip=skip, readCount=source_reads))
Path(sys.argv[1]).write_text(json.dumps(cases), encoding='utf8')
print(f'Generated{len(cases)} original auxiliary request/page/injection streams.')
