"""Independent C79 V18/V20 coordinate oracle; prepared data, no SDK execution."""
import importlib.util
import json
import math
from pathlib import Path
import random
import struct
import sys


def f(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]


def compose_one(left, right, fraction):
    """00FF19..00FFA5: do not substitute Pack's different rounding chain."""
    def coordinate(p):
        q = (p >> 8) & 255
        return f(f((p >> 16) & 65535) + (0. if q == 255 else f(q * f(.005))))
    a, b = coordinate(left), coordinate(right)
    value = f(f(f(f(b - a) * f(fraction)) * f(.005)) + a)
    integral = math.floor(value)
    remainder = f(value - f(integral))
    q = int(f(f(remainder + f(.0025)) * f(200))) & 255
    return (left & 0xffffffff000000ff) | ((integral & 65535) << 16) | (q << 8)


def gather(positions, separators, permutation):
    output, ends = [], []
    for index in permutation:
        start = 0 if index == 0 else separators[index - 1]
        output.extend(positions[start:separators[index]])
        ends.append(len(output))
    return output, ends


def compose(positions, separators, per_input_coordinates):
    output, ends = [], []
    for index, coordinates in enumerate(per_input_coordinates):
        start = 0 if index == 0 else separators[index - 1]
        for point, fraction in coordinates:
            left = positions[start + point]
            if fraction in (0, 255):
                output.append(left)  # whole prior position, including its mode
            else:
                output.append(compose_one(left, positions[start + point + 1], fraction))
        ends.append(len(output))
    return output, ends


def controls():
    rng = random.Random(0xc7918)
    rows, difference, carry = [], 0, 0
    for case in range(4096):
        source = rng.randrange(1, 256)
        point = rng.randrange(0, 32000)
        q = rng.randrange(200)
        left = (source << 32) | (point << 16) | (q << 8) | [3, 5, 7][case % 3]
        right = (source << 32) | ((point + 1) << 16) | (rng.randrange(200) << 8) | 5
        fraction = 1 + case % 199
        expected = compose_one(left, right, fraction)
        a = f(f(point) + f(q * f(.005)))
        b = f(f(point + 1) + f(((right >> 8) & 255) * f(.005)))
        value = f(f(f(f(b - a) * f(fraction)) * f(.005)) + a)
        remainder = f(value - f(math.floor(value)))
        alternate = int(f(f(remainder * f(200)) + f(.5))) & 255
        difference += alternate != ((expected >> 8) & 255)
        carry += ((expected >> 8) & 255) == 200
        rows.append([left, right, fraction, expected])
    # Threshold neighbors expose the different native add-before-multiply
    # chain; these are scalar discriminators, not invented NN positions.
    rounding = []
    for q in range(200):
        bits = struct.unpack('<I', struct.pack('<f', f((q + .5) / 200)))[0]
        for delta in range(-8, 9):
            remainder = struct.unpack('<f', struct.pack('<I', bits + delta))[0]
            actual = int(f(f(remainder + f(.0025)) * f(200)))
            alternate = int(f(f(remainder * f(200)) + f(.5)))
            rounding.append([remainder, actual, alternate])
            difference += actual != alternate
    assert difference > 0
    no_carry = compose_one(0xc703, 0x10005, 199)
    assert no_carry == 0xc803
    rows.append([0xc703, 0x10005, 199, no_carry])
    carry += 1
    assert compose([0xff03], [1], [[(0, 255)]]) == ([0xff03], [1])
    assert compose_one(3, 0x10005, 100) == 0x6403
    # Existing independently recovered 010850/00EBE0 expression, not the C# port.
    path = Path(__file__).resolve().parents[1] / 'LegacyInkPosition.Probe/reference_controls.py'
    spec = importlib.util.spec_from_file_location('native_position_oracle', path)
    old = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(old)
    cases, queries, reordered = [], 0, 0
    for case in range(64):
        counts = [rng.randrange(2, 8) for _ in range(1 + case % 7)]
        positions, separators = old.make_snapshot([[i, n, 0, -1, 0., 0., True, True, False] for i, n in enumerate(counts)])
        permutation = list(range(len(counts)))
        rng.shuffle(permutation)
        p, s = gather(positions, separators, permutation)
        reordered += permutation != sorted(permutation)
        coordinates = [[(0, 0)] + [(i, rng.randrange(1, 200)) for i in range(counts[source] - 1)] + [(counts[source] - 1, 255)] for source in permutation]
        projected, ends = compose(p, s, coordinates)
        originals = [[positions[0], positions[-1]]]
        ranges = [[a, b, old.select_ranges(projected, ends, originals, a, b)] for a in range(len(projected) + 1) for b in range(a, len(projected) + 1)]
        queries += len(ranges)
        cases.append(dict(counts=counts, permutation=permutation, positions=positions, separators=separators,
                          coordinates=coordinates, processed=projected, ends=ends, originals=originals, queries=ranges))
    assert reordered > 0
    return dict(preparedOnly=True, scalar=rows, rounding=rounding, snapshots=cases,
                discriminators=dict(roundingOrder=difference, fraction200NoCarry=carry, reorderedCases=reordered, intervalQueries=queries),
                handCases=dict(singlePointModePreserved=0xff03, firstSegmentFractionModePreserved=0x6403))


if __name__ == '__main__':
    if len(sys.argv) != 2:
        raise SystemExit('controls.json')
    result = controls()
    Path(sys.argv[1]).write_text(json.dumps(result, separators=(',', ':')) + '\n', encoding='utf8')
    print(f"PASS{len(result['scalar'])} prepared composition cases/{len(result['snapshots'])} processed snapshots; {result['discriminators']}; no actual NN/source claim")
