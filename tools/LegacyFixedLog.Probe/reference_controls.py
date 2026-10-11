"""Own mathematical reference, or private static table validation with an explicit local PE.

No native engine execution. Resource-derived controls belong in ignored artifacts.
Usage: reference_controls.py output.json [local-MyScriptText.dll]
"""
import json
import math
import struct
import sys
from pathlib import Path

table = [-0x4000000] + [int(8192 * math.log(i / 2048)) for i in range(1, 2049)]
source = "Independent math log truncation and nearest probability bucket (tie down); own math only."
if len(sys.argv) == 3:
    data = Path(sys.argv[2]).read_bytes()
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    assert data[pe:pe + 4] == b"PE\0\0"
    count = struct.unpack_from("<H", data, pe + 6)[0]
    opt_size = struct.unpack_from("<H", data, pe + 20)[0]
    opt = pe + 24
    assert struct.unpack_from("<H", data, opt)[0] == 0x20B
    image_base = struct.unpack_from("<Q", data, opt + 24)[0]
    rva = 0x180261290 - image_base
    sections = opt + opt_size
    for section in range(count):
        pos = sections + section * 40
        size, start, raw_size, raw = struct.unpack_from("<IIII", data, pos + 8)
        if start <= rva and rva + 2049 * 4 <= start + raw_size:
            static = list(struct.unpack_from("<2049i", data, raw + rva - start))
            assert static == table, "Mathematical table differs from inspected native bytes"
            table = static
            source = "Private local static table at180261290; all2049 entries independently match mathematical reconstruction. No execution."
            break
    else:
        raise ValueError("Inspected static table absent from PE")


def native_ref(p):
    if p == 0:
        return -0x4000000
    if p < 16:
        return table[p] - 0x58B9
    # Original nearest representative calculation, avoiding C# quotient/remainder.
    bucket = (p + 7) // 16
    return table[bucket]


cases = [dict(probability=p, log=native_ref(p), cost=-native_ref(max(1, p))) for p in range(32769)]
output = Path(sys.argv[1])
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(dict(source=source, cases=cases), separators=(",", ":")) + "\n", encoding="utf-8")
