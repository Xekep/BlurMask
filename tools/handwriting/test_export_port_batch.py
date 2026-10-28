"""Independent fixtures for completeness and PE byte validation; no Ghidra required."""
import copy
import json
import struct
import tempfile
import unittest
from pathlib import Path

from export_port_batch import normalize_request, validate_export
from run_port_checks import digest


class BatchTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        raw = bytearray(1024)
        raw[:2] = b"MZ"
        struct.pack_into("<I", raw, 0x3c, 0x80)
        raw[0x80:0x84] = b"PE\0\0"
        struct.pack_into("<H", raw, 0x86, 1)
        struct.pack_into("<H", raw, 0x94, 0xf0)
        struct.pack_into("<H", raw, 0x98, 0x20b)
        struct.pack_into("<Q", raw, 0x98 + 24, 0x180000000)
        struct.pack_into("<IIII", raw, 0x98 + 0xf0 + 8, 1, 0x1000, 0x200, 0x200)
        raw[0x200] = 0xc3
        self.binary = self.root / "fixture.dll"
        self.binary.write_bytes(raw)
        self.request = normalize_request(dict(schema=1, binarySha256=digest(self.binary),
                                             addresses=["0x180001000"]))
        self.artifacts = {
            "180001000.c": "void example(void) { return; }\n",
            "180001000.instructions.tsv": "address\tbytes\tinstruction\n180001000\tc3\tRET\n",
            "180001000.references.tsv": "direction\tfrom\tto\ttype\n",
        }
        for name, content in self.artifacts.items():
            (self.root / name).write_text(content, encoding="utf-8")
        self.index = dict(schema=1, status="passed", program="fixture.dll", imageBase="180000000",
                          binarySha256=self.request["binarySha256"], vendorExecution=False,
                          programMutation=False, functions=[dict(address="0x180001000", status="passed",
                          instructions=1, artifacts={name: digest(self.root / name) for name in self.artifacts})])

    def check(self, index=None, request=None):
        (self.root / "export-index.json").write_text(json.dumps(index or self.index), encoding="utf-8")
        return validate_export(self.root, request or self.request, "fixture.dll", self.binary)

    def test_complete_export_and_deduplication(self):
        self.assertEqual(self.check()["verifiedInstructions"], 1)
        duplicate = dict(self.request, addresses=["0x180001000", "0x180001000", "0x180000FFF"])
        self.assertEqual(normalize_request(duplicate)["addresses"], ["0x180000fff", "0x180001000"])

    def test_missing_duplicate_failed_and_wrong_binary_rows_refused(self):
        variants = []
        for key, value in [("status", "failed"), ("program", "other.dll"),
                           ("binarySha256", "0" * 64), ("imageBase", "190000000"),
                           ("vendorExecution", True), ("programMutation", True)]:
            variants.append(dict(self.index, **{key: value}))
        variants.append(dict(self.index, functions=[]))
        variants.append(dict(self.index, functions=self.index["functions"] * 2))
        broken = copy.deepcopy(self.index)
        broken["functions"][0]["status"] = "missing-function-entry"
        variants.append(broken)
        for variant in variants:
            with self.subTest(variant=variant), self.assertRaises(ValueError):
                self.check(variant)

    def test_tampered_artifact_and_missing_artifact_refused(self):
        path = self.root / "180001000.c"
        path.write_text("changed")
        with self.assertRaisesRegex(ValueError, "checksum"):
            self.check()
        path.unlink()
        with self.assertRaisesRegex(ValueError, "checksum"):
            self.check()

    def test_matching_artifact_hash_cannot_hide_wrong_native_bytes(self):
        path = self.root / "180001000.instructions.tsv"
        path.write_text("address\tbytes\tinstruction\n180001000\t90\tNOP\n")
        self.index["functions"][0]["artifacts"][path.name] = digest(path)
        with self.assertRaisesRegex(ValueError, "original PE bytes"):
            self.check()

    def test_table_count_and_raw_section_bounds(self):
        path = self.root / "180001000.instructions.tsv"
        for content in ["address\tbytes\tinstruction\n",  # truncated
                        "address\tbytes\tinstruction\n180003000\tc3\tRET\n",  # unmapped
                        "address\tbytes\tinstruction\n180001000\tc3\tRET\n180001000\tc3\tRET\n"]:
            path.write_text(content)
            self.index["functions"][0]["artifacts"][path.name] = digest(path)
            with self.subTest(content=content), self.assertRaises(ValueError):
                self.check()

    def test_malformed_request_refused(self):
        for patch in [dict(addresses=[]), dict(addresses=["-postScript"]), dict(addresses=["180001000"]),
                      dict(binarySha256="0"), dict(decompileSeconds=0), dict(decompileSeconds=True)]:
            with self.subTest(patch=patch), self.assertRaises(ValueError):
                normalize_request(dict(self.request, **patch))


if __name__ == "__main__":
    unittest.main()
