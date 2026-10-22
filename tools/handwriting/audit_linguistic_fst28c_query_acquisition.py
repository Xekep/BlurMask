"""Authenticate reviewed STATIC acquisition/issuer evidence. Never loads vendor code."""
import argparse
import hashlib
import json
from pathlib import Path
import struct


def digest(raw):
    return hashlib.sha256(raw).hexdigest()


def audit(repo):
    here = Path(__file__).resolve().parent
    pins = json.loads((here / "linguistic-fst28c-query-acquisition-pins.json").read_text())
    raw = (repo / pins["nativeFile"]).read_bytes()
    assert digest(raw) == pins["nativeSha256"], "Different native Text binary"
    pe = struct.unpack_from("<I", raw, 0x3c)[0]
    assert raw[pe:pe+4] == b"PE\0\0"
    count, opt_size = struct.unpack_from("<H", raw, pe+6)[0], struct.unpack_from("<H", raw, pe+20)[0]
    opt = pe+24
    assert struct.unpack_from("<H", raw, opt)[0] == 0x20b
    image_base = struct.unpack_from("<Q", raw, opt+24)[0]
    sections = [struct.unpack_from("<IIII", raw, opt+opt_size+i*40+8) for i in range(count)]

    def span(address, size):
        rva = int(address, 16)-image_base
        for virtual_size, va, file_size, file_at in sections:
            if va <= rva and rva+size <= va+file_size:
                return raw[file_at+rva-va:file_at+rva-va+size]
        raise AssertionError("Pinned range not wholly file-backed")

    for row in pins["regions"] + pins["data"] + pins["completeLeaves"]:
        value = span(row["start"], row["bytes"])
        assert len(value) == row["bytes"] and digest(value) == row["sha256"], row
        if "hex" in row:
            assert value.hex() == row["hex"], row
    expected = [0x18023fff0, 0x180240000, 0x18023f650, 0x180155710, 0x18005a400]
    actual = []
    for row in pins["data"]:
        value = span(row["start"], row["bytes"])
        actual.extend(struct.unpack("<"+"Q"*(len(value)//8), value))
    assert actual == expected, "Exact dispatch edges"
    assert len({r["function"] for r in pins["regions"]}) == 12
    assert len(pins["regions"]) == 15
    assert [r["function"] for r in pins["regions"]].count("1801fd240") == 3
    for row in pins["reviewedRuntimeInventory"]:
        assert digest((repo / row["path"]).read_bytes().replace(b"\r\n", b"\n")) == row["lfSha256"], "Reviewed runtime inventory changed: " + row["path"]
    # Pin rejection is checked independently of the whole-DLL fingerprint:
    negative = 0
    for row in pins["completeLeaves"]:
        value = bytearray(span(row["start"], row["bytes"]))
        value[0] ^= 1
        assert digest(value) != row["sha256"]
        negative += 1
    return {"scope": "STATIC reviewed bytes only; no runtime query/recognition claim",
            "nativeSha256": digest(raw), "fullFunctions": 12, "fullBodyRegions": 15,
            "completeForwardingOrReturnLeaves": 4, "dispatchPointers": 5,
            "reviewedRuntimeFiles": len(pins["reviewedRuntimeInventory"]), "negativeBytePins": negative,
            "issuerBoundary": "No existing full-root13A current-controller receipt; selected-only13B owner is excluded",
            "freshDispatch": "1FD240 byte2600 zero -> V40; nonzero -> prepared-record copy, NOT V48",
            "queryRuntimeApproved": False}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--native-repo", type=Path, required=True)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    result = audit(args.native_repo)
    text = json.dumps(result, indent=2)+"\n"
    if args.output:
        args.output.write_text(text, encoding="utf-8")
    print(text, end="")


if __name__ == "__main__":
    main()
