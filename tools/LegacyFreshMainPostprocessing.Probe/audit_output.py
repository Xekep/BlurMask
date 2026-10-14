"""Original composition audit using independent dense-spline/Bezier/key oracle."""
import base64
import json
import math
import struct
import sys
from pathlib import Path

import numpy as np

KEYS = Path(__file__).resolve().parents[1] / "LegacySmoothingPointKeys.Probe/reference_controls.py"
environment = {"__file__": str(KEYS)}
#Definitions only; never generate or modify the existing oracle corpus.
exec(KEYS.read_text(encoding="utf-8").split("random.seed(173171);result=[]")[0], environment)
keys_oracle = environment["keys"]
f = environment["f"]


def audit(path):
    data = json.loads(Path(path).read_bytes())
    assert data["groups"] > 0 and data["sourceRows"] == 1
    assert struct.pack("<f", data["shear"]) == b"\0" * 4
    assert data["gap"] == 1.5
    guide = base64.b64decode(data["guide"])
    baseline, height = struct.unpack_from("<ff", guide)
    assert height > 0 and math.isfinite(baseline)
    before = [[f(x), f(y)] for x, y in data["before265"]]
    original = [[f(x), f(y)] for x, y in data["original"]]
    n = len(original) - 1
    orthogonal = sum(a[0] == b[0] or a[1] == b[1] for a, b in zip(original, original[1:]))
    corners = sum((a[0] == b[0] and b[1] == c[1]) or (a[1] == b[1] and b[0] == c[0])
                  for a, b, c in zip(original, original[1:], original[2:]))
    metrics = [f(orthogonal / n) if n else 0, f(corners / (n - 1)) if n > 1 else 0]
    assert struct.pack("<ff", *metrics) == struct.pack("<ff", *data["metrics"])
    storage = base64.b64decode(data["after720"])
    original_storage = base64.b64decode(data["before720"])
    selected = []
    for a0, stroke0, p0, a1, stroke1, p1 in data["ranges"]:
        assert a0 == a1 and stroke0 == stroke1 == 0
        selected.extend(before[p0:p1+1])
    assert selected
    expected = bytearray(original_storage)
    for offset, value in zip((24, 28, 32, 36),
                             (min(p[0] for p in selected), max(p[0] for p in selected),
                              min(p[1] for p in selected), max(p[1] for p in selected))):
        struct.pack_into("<f", expected, offset, value)
    assert storage == expected, "24D changes exceed exact published cropped bounds"
    profile = list(map(f, data["profile"]))
    expected_points, expected_keys, derived, weight = keys_oracle([before], profile, f(height), metrics)
    actual_points = np.asarray(data["smoothed"], dtype=np.float32)
    assert len(expected_points) == 1 and actual_points.shape == np.asarray(expected_points[0]).shape
    error = float(np.max(np.abs(actual_points - np.asarray(expected_points[0]))))
    assert error < .0005, error
    assert data["keys200"] == expected_keys
    raw_indices = [0, 2, 3, 4, 5]
    assert data["sourceKeys"] == [[0, 0, raw_indices[(key + 199) // 200]] for key in expected_keys]
    assert struct.pack("<f", data["weight"]) == struct.pack("<f", weight)
    print(f"PASS independent actual native zero-gap/720 bounds/original metrics/265 spline/fractional keys: {len(expected_keys)} points, max error {error:g}")


if __name__ == "__main__":
    audit(sys.argv[1])
