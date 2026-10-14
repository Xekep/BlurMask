"""Pinned static owner/callback/readers audit. No native DLL execution."""
import argparse
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "research/myscript/scripts"))
from inspect_text_features import Binary, functions


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    args = parser.parse_args()
    binary = Binary()
    assert hashlib.sha256(binary.data).hexdigest() == "18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f"
    exports = functions()
    assert binary.qwords(0x1802dff50 + 24, 1)[0] == 0x180177280
    assert binary.qwords(0x1802dfcb0 + 24, 1)[0] == 0x180175350
    assert binary.qwords(0x1802dfab8 + 24, 1)[0] == 0x180173c10
    assert binary.qwords(0x1802dfcd0, 1)[0] == 0x180175160
    assert binary.qwords(0x1802dfc88, 1)[0] == 0x1801763e0
    assert binary.qwords(0x1802dff10, 1)[0] == 0x1801781e0
    shear = exports["180177280"]
    assert "fVar44 = fVar45 * *(float *)(param_2 + 0x2c)" in shear
    assert "fVar45 = fVar45 * *(float *)(param_2 + 0x30)" in shear
    assert "fVar42 = fVar44" in shear
    assert "if (fVar42 < fVar45)" in shear
    direction = shear[shear.index("local_res10 = (ulonglong)*(uint *)(param_3 + 4)"):]
    assert "fVar42 <= fVar31" in direction and "fVar31 <= fVar45" in direction
    assert "fVar41 = (fVar41 + local_118) * 0.5" in direction
    gap = exports["180175350"]
    zero = gap[gap.index("if (iVar17 == 0)"):][:220]
    assert "param_3 + 0x9c" in zero and "param_2 + 0x24" in zero
    #175160 fresh29B initialization: mov byte ptr[rdx+4c],0.
    assert binary.data[binary.off(0x1801751ae):binary.off(0x1801751b2)] == bytes.fromhex("c6424c00")
    ctor = exports["18016a130"]
    assert "*(undefined4 *)(param_2 + 0x25) = 200" in ctor
    smoothing = exports["180173c10"]
    assert "if (*(int *)(param_3 + 0x80) == 0)" in smoothing
    assert "*(longlong *)(param_3 + 0x78) + 4" in smoothing
    assert "param_3 + 0xa0" in smoothing and "param_3 + 0xa4" in smoothing
    assert "&local_48,200" in smoothing
    #No720 buffer write occurs in173C10; the point and map replacements are separate.
    assert "param_3 + 200" not in smoothing and "param_3 + 0xc8" not in smoothing
    caller = exports["1801680d0"]
    assert caller.index("FUN_18015abe0") < caller.index("FUN_1801629c0")
    report = dict(
        scope="fresh owned onephysical+one720row beforemainwinner",
        sourceSHA=hashlib.sha256(binary.data).hexdigest(),
        callbacks={"24D": "180177280", "29B": "180175350", "265": "180173c10"},
        proof="zero count blend scales BOTH grid and direction bounds; no prior shear; original metrics; actual guide4; factor200 identity",
        exports={name: hashlib.sha256(exports[name].encode()).hexdigest() for name in (
            "180177280", "1801781e0", "180175350", "1801763e0", "180173c10", "1801728c0", "18016a130", "1801680d0", "18015abe0")},
        missing=["multi-glyph shear/gap estimators", "multi-physical source group/reorder", "main frame150 owner publication", "complete main winner"],
    )
    Path(args.output).write_bytes((json.dumps(report, indent=2) + "\n").encode())
    print("PASS pinned static24D/29B/265 zero-gap bounds, metrics/guide ownership and fractional source-map producer")


if __name__ == "__main__":
    main()
