"""Stateless filtered-edge slicing oracle, independent of native cache/block loops."""
import base64
import json
import random
import struct
import sys
from pathlib import Path


def packed(values, width):
    output = bytearray(((len(values) * width + 31) // 32) * 4)
    for i, value in enumerate(values):
        for bit in range(width):
            if value & (1 << bit):
                offset = i * width + bit
                output[offset // 8] |= 1 << (offset % 8)
    return struct.pack('<IIB', 0x01000000, len(values), width) + output


def main():
    labels = [0] + list(range(600)) + list(range(10, 350))
    terminals = [False] + [i % 3 != 0 for i in range(600)] + [i % 2 == 0 for i in range(340)]
    counts = [600, 0, 340] + [0] * (len(labels) - 3)
    raw_labels = [label | (1024 if terminal else 0) for label, terminal in zip(labels, terminals)]
    payload = (struct.pack('<IIIII', 0x2b4, 0x01000000, 0, len(labels), 4)
               + packed(raw_labels, 11) + bytes([10]) + packed(counts, 10)
               + struct.pack('<I', sum(terminals)))
    dictionary = struct.pack('<II', 0x0101fe90, len(payload)) + payload
    children = []
    next_child = 1
    for count in counts:
        children.append(list(range(next_child, next_child + count)))
        next_child += count
    preserved = [i if i % 11 else 65535 for i in range(600)]
    working = [v if i % 4 else 65535 for i, v in enumerate(preserved)]
    auxiliary = [599 - i if i % 13 else 65535 for i in range(600)]
    contexts = [0xAB0000000000 + i for i in range(600)]
    rng = random.Random(0x093B50)
    cases = []
    total = 0
    for case in range(128):
        ring = 0
        owners = {}
        operations = []
        for step in range(40):
            identifier = rng.randrange(12)
            reset = step == 25
            if reset:
                ring = 0
            if identifier not in owners:
                owners[identifier] = ring
                ring = (ring + 1) % 8
            node = rng.choice([0, 2, 1, 600])
            aux = bool(rng.getrandbits(1))
            flags = rng.choice([0, 0x40, 0x140])
            selected = auxiliary if aux else preserved if flags & 0x40 else working
            source = children[node][1:] if node == 0 else children[node]
            accepted = [i for i in source if selected[labels[i]] != 65535 and (not aux or terminals[i])]
            skip = rng.choice([0, 1, 17, 250, 256, 299, 600])
            capacity = rng.choice([1, 3, 17, 256, 512])
            page = accepted[skip:skip + capacity]
            category = rng.randrange(256)
            parent = rng.choice([0, 3, 19])
            root = rng.choice([0, 99])
            records = []
            trace = []
            for state in page:
                symbol = selected[labels[state]]
                if not parent:
                    context = contexts[labels[state]]
                elif not root:
                    context = 0
                else:
                    context = parent * 65536 + symbol + 1
                    trace.append([parent, symbol, context, bool(symbol % 2)])
                record = bytearray(32)
                struct.pack_into('<I', record, 0, state)
                record[5] = (1 if node == 0 else 0) | (2 if terminals[state] else 0)
                struct.pack_into('<Q', record, 8, context)
                struct.pack_into('<H', record, 20, symbol)
                record[22] = category
                records.extend(record)
            count = min(len(accepted), skip + capacity)
            returned = count - skip
            end = children[node][-1] + 1 if children[node] else sum(counts[:node + 1]) + 1
            start = children[node][0] if children[node] else end
            if node == 0:
                start += 1
            next_state = page[-1] + 1 if len(page) == capacity else end
            operations.append(dict(id=identifier, reset=reset, node=node, aux=aux, flags=flags,
                flags26=rng.randrange(65536), p1=rng.getrandbits(32), p2=rng.getrandbits(32),
                skip=skip, capacity=capacity, category=category, parent=parent, root=root,
                count=returned, accepted=count, nextChild=next_state,
                slot=owners[identifier], records=base64.b64encode(bytes(records)).decode(), trace=trace))
            total += len(page)
        cases.append(operations)
    output = Path(sys.argv[1]); output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(dict(dictionary=base64.b64encode(dictionary).decode(),
        preserved=preserved, working=working, auxiliary=auxiliary, contexts=contexts,
        cases=cases), separators=(',', ':')), encoding='utf-8')
    print(f'Wrote 128 independent query streams /5120 operations/{total} complete32-byte records')


if __name__ == '__main__':
    main()
