"""Opt-in static audit of ordinary source ownership. Never executes vendor code."""
import argparse
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "research/myscript/scripts"))
from inspect_text_features import Binary, functions

EXPECTED = "18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    args = parser.parse_args()
    binary = Binary("MyScriptText.dll")
    assert hashlib.sha256(binary.data).hexdigest() == EXPECTED
    exports = functions("MyScriptText.dll")
    required = ("180159810", "180167210", "180161120", "18015eaa0", "18015f050",
                "18016aa00", "18016a6e0", "1801629c0", "1801680d0", "180170400",
                "18016e850", "180177280", "180175350")
    for address in required:
        assert address in exports, address
    ingress = exports["180159810"]
    assert "0x58" in ingress and "puVar6[10] = 0" in ingress
    regroup = exports["180161120"]
    assert "if (*(int *)(lVar13 + 0x20) == 1)" in regroup
    assert "return 1" in regroup[regroup.index("if (*(int *)(lVar13 + 0x20) == 1)"):][:100]
    reorder = exports["18015f050"]
    assert "if (uVar17 < uVar25)" in reorder and "else if (uVar17 <= uVar25)" in reorder
    assert "FUN_18016aa00" in reorder
    guide = exports["1801629c0"]
    assert "0x2000003" in guide and "0x14" in guide and "0x10" in guide
    orchestrator = exports["1801680d0"]
    assert orchestrator.index("FUN_18015f050") < orchestrator.index("FUN_1801629c0")
    dispatch = orchestrator.index("*(longlong *)(param_2 + 0xd8)")
    assert orchestrator.index("FUN_1801629c0") < dispatch
    #Bind actual processor callbacks through runtime tables, not nearby helpers.
    assert binary.qwords(0x1802dff50, 4)[3] == 0x180177280
    assert binary.qwords(0x1802dfcb0, 4)[3] == 0x180175350
    #24D is a shear producer;29B is a normalized-gap producer. Neither can be
    #declared implemented by this stage merely because it preserves line height.
    assert "*(float *)(param_3 + 0x27)" in exports["180177280"]  #qword27 ==byte138
    assert "param_3 + 0x9c" in exports["180175350"]
    report = {
        "sourceSha256": EXPECTED,
        "functions": {a: hashlib.sha256(exports[a].encode()).hexdigest() for a in required},
        "scope": "fresh no-prior/no-pages/no-auxiliary, one descriptor and one physical stroke",
        "singletonOrder": [0],
        "initialGuideCount": 0,
        "initialLineId": -1,
        "callback24D": "180177280",
        "callback29B": "180175350",
        "unresolved": ["multi-physical15F050 grouping/reorder", "full native first winner",
                       "24D shear estimator", "29B gap estimator", "main decoding", "application font integration"],
    }
    Path(args.output).write_bytes((json.dumps(report, indent=2) + "\n").encode())
    print("PASS pinned PE/static singleton ingress, guide ordering and downstream callback binding")


if __name__ == "__main__":
    main()
