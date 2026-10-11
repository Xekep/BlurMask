"""Read-only PE audit of the default global+78 Unicode service. Never loads DLLs.

Requires the pinned engine file and official UnicodeData-9.0.0.txt as inputs.
The complete 5722 decomposition records are checked against that public source.
Outputs metadata and the 104 wide mappings, never executable vendor artifacts.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import struct

ENGINE_SHA = "d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c"
UCD_SHA = "68dfc414d28257b9b5d6ddbb8b466c768c00ebdf6cbf7784364a9b6cad55ee8f"
KINDS = {"canonical": 1, "font": 2, "noBreak": 4, "initial": 8,
         "medial": 16, "final": 32, "isolated": 64, "circle": 128,
         "super": 256, "sub": 512, "vertical": 1024, "wide": 2048,
         "narrow": 4096, "small": 8192, "square": 16384,
         "fraction": 32768, "compat": 65536}


class PE:
    def __init__(self, data):
        self.data = data
        pe = struct.unpack_from("<I", data, 0x3c)[0]
        if data[pe:pe + 4] != b"PE\0\0":
            raise ValueError("Not PE")
        machine, count = struct.unpack_from("<HH", data, pe + 4)
        optional_size = struct.unpack_from("<H", data, pe + 20)[0]
        if machine != 0x8664 or struct.unpack_from("<H", data, pe + 24)[0] != 0x20b:
            raise ValueError("Expected x64 PE32+")
        self.base = struct.unpack_from("<Q", data, pe + 48)[0]
        self.sections = []
        for n in range(count):
            offset = pe + 24 + optional_size + n * 40
            virtual_size, rva, size, raw = struct.unpack_from("<IIII", data, offset + 8)
            self.sections.append((rva, size, raw))

    def read(self, address, fmt):
        rva = address - self.base
        size = struct.calcsize("<" + fmt)
        for start, count, raw in self.sections:
            if start <= rva and rva + size <= start + count:
                return struct.unpack_from("<" + fmt, self.data, raw + rva - start)
        raise ValueError(f"Unmapped bounded PE address {address:x}")


def scalar(cp):
    return 0 <= cp <= 0x10ffff and not 0xd800 <= cp <= 0xdfff


def expected_wide(cp):
    if cp == 0x3000:
        return 0x20
    if 0xff01 <= cp <= 0xff5e:
        return cp - 0xfee0
    if cp in (0xff5f, 0xff60):
        return cp - 0xd5da
    return {0xffe0: 0xa2, 0xffe1: 0xa3, 0xffe2: 0xac, 0xffe3: 0xaf,
            0xffe4: 0xa6, 0xffe5: 0xa5, 0xffe6: 0x20a9}.get(cp)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("engine", type=Path)
    parser.add_argument("unicode_data", type=Path)
    parser.add_argument("report", type=Path)
    args = parser.parse_args()
    engine, ucd = args.engine.read_bytes(), args.unicode_data.read_bytes()
    if hashlib.sha256(engine).hexdigest() != ENGINE_SHA or hashlib.sha256(ucd).hexdigest() != UCD_SHA:
        raise ValueError("Pinned source hash differs")
    pe = PE(engine)
    descriptor = pe.read(0x180252200, "Q")[0]
    vtable = pe.read(descriptor + 0x20, "Q")[0]
    decompose = pe.read(vtable + 8, "Q")[0]
    if (descriptor, vtable, decompose) != (0x180202b00, 0x180202a70, 0x18005ed00):
        raise ValueError("Default service binding differs")
    records, index = pe.read(0x1802521f0, "QQ")
    rows = {}
    previous = -1
    for position in pe.read(index, "5722I"):
        header, cp = pe.read(records + position * 4, "II")
        count, kind = header & 255, header >> 8
        if cp <= previous or not scalar(cp) or not 1 <= count <= 18 or kind not in KINDS.values():
            raise ValueError("Invalid ordered decomposition record")
        parts = pe.read(records + (position + 2) * 4, "I" * count)
        if not all(map(scalar, parts)):
            raise ValueError("Invalid decomposition UTF32")
        rows[cp] = (kind, parts)
        previous = cp
    names, public = {}, {}
    for line in ucd.decode("utf-8").splitlines():
        fields = line.split(";")
        cp = int(fields[0], 16)
        names[cp] = fields[1]
        if not fields[5]:
            continue
        parts = fields[5].split()
        tag = parts.pop(0)[1:-1] if parts[0].startswith("<") else "canonical"
        public[cp] = (KINDS[tag], tuple(int(p, 16) for p in parts))
    if rows != public:
        mismatches = [hex(cp) for cp in rows.keys() | public.keys() if rows.get(cp) != public.get(cp)]
        raise ValueError(f"Native/public Unicode 9.0 decompositions differ: {mismatches[:20]}")
    wide = [(cp, parts[0]) for cp, (kind, parts) in rows.items() if kind == 0x800]
    if len(wide) != 104 or any(len(rows[cp][1]) != 1 or expected_wide(cp) != target for cp, target in wide):
        raise ValueError("Constructive 104-wide mapping differs")
    examples = {}
    for cp in [0x41, 0x410, 0x6d, 0x43c, 0x70, 0x440, 0x451, 0x401, 0x439, 0x419, 0xa0, 0xb5, 0x37e, 0x387]:
        row = rows.get(cp)
        examples[f"U+{cp:04X}"] = dict(name=names.get(cp), tableKind=row[0] if row else None,
                                       tableParts=list(row[1]) if row else None,
                                       serviceDeclines=cp in (0x37e, 0x387) or row is None)
    kinds = Counter(kind for kind, _ in rows.values())
    wide_bytes = b"".join(struct.pack("<II", *pair) for pair in wide)
    result = dict(engineSha256=ENGINE_SHA, unicodeDataSha256=UCD_SHA,
                  unicodeDataSource="https://www.unicode.org/Public/9.0.0/ucd/UnicodeData.txt",
                  serviceBinding=dict(initializer="180031910", globalOffset="0x78", servicePointer="180252200",
                                      descriptor=f"{descriptor:x}", descriptorField="0x20", vtable=f"{vtable:x}",
                                      methodOffset="0x8", implementation=f"{decompose:x}"),
                  tableRecordCount=len(rows), comparison="All 5722 records/kinds/direct UTF32 mappings exactly equal public UCD9.0",
                  kindCounts={tag: dict(value=value, count=kinds[value]) for tag, value in KINDS.items()},
                  serviceExceptions=["U+037E", "U+0387"],
                  hangul=dict(start="U+AC00", count=11172, kind=1, algorithm="L=1100+S//588; V=1161+(S%588)//28; T=11A7+S%28, omit T when 11A7"),
                  widePairs=[list(pair) for pair in wide], widePairsLittleEndianSha256=hashlib.sha256(wide_bytes).hexdigest(),
                  examples=examples)
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Static default Unicode service PASS: {len(rows)} UCD9.0 records, 17 kinds, 104 constructive wide mappings; {args.report}")


if __name__ == "__main__":
    main()
