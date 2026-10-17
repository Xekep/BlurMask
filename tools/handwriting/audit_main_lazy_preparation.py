"""Static proof of the scoped 217670 frontend preparation dependency.

Reads the private pinned PE only. Never loads or executes vendor code.
The optional Ghidra call graph omits virtual calls, so table slots and exact
instructions are checked here independently of the exported function names.
"""
import argparse
import hashlib
import json
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / "research/myscript/scripts"))
from inspect_text_features import Binary
from capstone import Cs, CS_ARCH_X86, CS_MODE_64


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    binary = Binary()
    assert hashlib.sha256(binary.data).hexdigest() == "18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f"
    regions = [
        (0x180152A70, 0x180152ABF, "76d1163cd6c0c7b6e71a4a3902a70f57efe668c58dd96a0f85fd4adb47fa8608"),
        (0x1801AC0E0, 0x1801AC0FA, "1ab588d01ebbc78bded6c7ff3e08a44513765bc9345e083297d298d195cd374f"),
        (0x1801C2530, 0x1801C253A, "c5ce927855f1a9e5e852a7a78b2d78ed64b717e86348d49f87c4177f013f8a05"),
    ]
    for start, end, expected in regions:
        assert hashlib.sha256(binary.data[binary.off(start):binary.off(end)]).hexdigest() == expected
    bindings = [
        (0x1802DE610, 0x228, 0x180152A70),
        (0x1802E1FF0, 0x138, 0x1801AC0E0),
        (0x1802E1490, 0x100, 0),
        (0x1802E4180, 0x158, 0x1801C2530),
    ]
    for table, slot, target in bindings:
        assert binary.qwords(table + slot, 1)[0] == target
    disassembler = Cs(CS_ARCH_X86, CS_MODE_64)

    def instructions(start, end):
        return [(i.mnemonic, i.op_str) for i in disassembler.disasm(
            binary.data[binary.off(start):binary.off(end)], start)]

    graph = instructions(0x1801AC0E0, 0x1801AC0FA)
    assert graph == [
        ("mov", "rdx, qword ptr [rdx + 0x180]"),
        ("mov", "rax, qword ptr [rdx]"),
        ("mov", "r9, qword ptr [rax + 0x100]"),
        ("test", "r9, r9"), ("je", "0x1801ac0f9"),
        ("jmp", "r9"), ("ret", ""),
    ]
    acoustic = instructions(0x1801C2530, 0x1801C253A)
    assert acoustic == [("mov", "word ptr [rdx + 0x3c2], 0xffff"), ("ret", "")]
    frontend = instructions(0x180152A70, 0x180152ABF)
    ordered_calls = [
        ("mov", "rdx, qword ptr [rdx + 0x70]"),
        ("mov", "r9, qword ptr [rax + 0x138]"), ("call", "r9"),
        ("mov", "rdx, qword ptr [rbx + 0x78]"),
        ("mov", "r9, qword ptr [rax + 0x158]"), ("jmp", "r9"),
    ]
    positions = [frontend.index(call) for call in ordered_calls]
    assert positions == sorted(positions)
    report = dict(
        regions=[dict(start=hex(a), end=hex(z), sha256=h) for a, z, h in regions],
        bindings=[dict(table=hex(t), slot=hex(s), target=hex(v)) for t, s, v in bindings],
        exactGraphAndAcousticInstructions=len(graph) + len(acoustic),
        orderedFrontendPins=len(ordered_calls),
        samplerPreparation="null optional slot on actual fresh v106 sampler",
        acousticPreparation="reset last-group word3C2 to FFFF only",
        scope="Actual fresh ordinary sampler/acoustic configuration only; not arbitrary graph preparations or a completed lazy refresh",
        noVendorExecution=True,
    )
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print("PASS3 static regions/4 virtual bindings/9 exact instructions/6 ordered frontend pins; scoped lazy preparation is cache invalidation only.")


if __name__ == "__main__":
    main()
