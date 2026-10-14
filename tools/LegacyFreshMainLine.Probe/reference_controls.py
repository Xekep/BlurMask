"""Original raw-structure controls; no model labels or expected OCR corrections."""
import argparse
import base64
import json
import random
import struct
from pathlib import Path


def descriptor(x, y):
    data = bytearray(88)
    struct.pack_into("<H", data, 0, 0x10)
    struct.pack_into("<i", data, 4, -2147483648)
    for offset in (8, 12, 16, 20, 24, 32):
        struct.pack_into("<i", data, offset, -1)
    struct.pack_into("<ff", data, 44, x, y)
    data[72] = 1
    data[76:84] = b"\xff" * 8
    return data


def make(path):
    rng = random.Random(0x1629C0)
    cases = []
    for index in range(64):
        x = struct.unpack("<f", struct.pack("<f", rng.uniform(-500, 500)))[0]
        y = struct.unpack("<f", struct.pack("<f", rng.uniform(-500, 500)))[0]
        points = [[x, y], [x + 30, y - 60], [x + 60, y]]
        if index % 2 == 0:
            points.insert(1, points[0])
        cases.append({"points": points, "descriptor": base64.b64encode(descriptor(x, y)).decode()})
    Path(path).write_bytes((json.dumps(cases, separators=(",", ":")) + "\n").encode())


def audit(path):
    data = json.loads(Path(path).read_bytes())
    initial = base64.b64decode(data["initialDescriptor"])
    assert initial == descriptor(*data["raw"][0]), "full fresh descriptor differs"
    expected = bytearray(initial)
    struct.pack_into("<i", expected, 16, 0)
    struct.pack_into("<i", expected, 56, data["sourceRows"])
    assert base64.b64decode(data["descriptor"]) == expected, "unexpected descriptor mutation"
    guide = base64.b64decode(data["guide"])
    assert len(guide) == 40
    assert guide[12:20] == struct.pack("<ff", data["baseline"], data["height"])
    assert struct.unpack_from("<f", guide, 24)[0] == 0
    assert struct.unpack_from("<ii", guide, 32) == (-1, -2147483648)
    assert data["sourceRows"] == len(data["source720"]) > 0
    for row in data["source720"]:
        decoded = base64.b64decode(row)
        assert len(decoded) == 720
        left, right, top, bottom = struct.unpack_from("<ffff", decoded, 24)
        assert left <= right and top <= bottom
    print("PASS independent actual singleton descriptor/line-owner/source720 interval/publication audit")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("output")
    parser.add_argument("--audit", action="store_true")
    args = parser.parse_args()
    (audit if args.audit else make)(args.output)
