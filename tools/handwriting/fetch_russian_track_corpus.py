"""Private, opt-in Russian coordinate audit. Python stdlib only; no dataset code.

Data: Mendeley 10.17632/3h6h5d7xg2.2, CC BY 4.0. The pinned source
does NOT record pen-up events. A coordinate track is not a physical stroke.
"""
import argparse
import csv
import hashlib
import io
import json
from pathlib import Path
import urllib.request
import zipfile

COMMIT = "c6629026b1fdef51fd08e3d2fddffd82f97e60a5"
URL = f"https://codeload.github.com/Skvayzer/russian_handwritings_tracked/zip/{COMMIT}"
SHA = "2960052698884e00d8fa428295f538d52c30f47ea3f7695e5c7be610fd943edf"
PREFIX = f"russian_handwritings_tracked-{COMMIT}/"
SESSIONS = ("w_3_1", "w_7_1", "w_12_1")
CHARACTERS = tuple("аёмртяАЖД17")
WORDS = ("да", "ещё", "мягких")

def integers(data):
    if len(data) > 200_000:
        raise ValueError("Oversized CSV record")
    rows = list(csv.reader(io.StringIO(data.decode("utf-8-sig"))))
    if len(rows) != 1 or not rows[0] or len(rows[0]) > 8192:
        raise ValueError("Expected one bounded numeric row")
    result = [int(cell) for cell in rows[0]]
    if any(value < 0 or value > 10_000_000 for value in result):
        raise ValueError("Unexpected sentinel or out-of-range integer; do not infer pen-up")
    return result

def parse_track(coordinates, timing):
    values, intervals = integers(coordinates), integers(timing)
    if len(values) % 2 or not 2 <= len(values) // 2 <= 4096:
        raise ValueError("Expected bounded XY pairs")
    points = [values[i:i+2] for i in range(0, len(values), 2)]
    if len(intervals) != len(points):
        raise ValueError("Coordinate/timing counts differ")
    return points, intervals

def self_test():
    assert parse_track(b'"1","2","3","4"', b'"15","30"') == ([[1, 2], [3, 4]], [15, 30])
    for xy, times in ((b'"1"', b'"3"'), (b'"1","-1"', b'"3"'),
                      (b'1,2\n3,4', b'1,2'), (b'1,2', b'1,2'),
                      (b'1,NaN', b'1'), (b'10000001,2', b'1')):
        try:
            parse_track(xy, times)
        except ValueError:
            continue
        raise AssertionError("Malformed track accepted")
    print("PASS original CSV guards; timestamps preserved without invented stroke boundaries")

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path, nargs="?")
    parser.add_argument("--self-test", action="store_true")
    args = parser.parse_args()
    if args.self_test:
        self_test()
    if args.output is None:
        if not args.self_test:
            parser.error("Private output directory required")
        return
    args.output.mkdir(parents=True, exist_ok=True)
    archive = args.output / "source.zip"
    if not archive.exists():
        with urllib.request.urlopen(URL, timeout=60) as response:
            payload = response.read(40_000_001)
        if len(payload) > 40_000_000 or hashlib.sha256(payload).hexdigest() != SHA:
            raise ValueError("Pinned archive size/hash mismatch")
        archive.write_bytes(payload)
    if archive.stat().st_size > 40_000_000 or hashlib.sha256(archive.read_bytes()).hexdigest() != SHA:
        raise ValueError("Pinned archive size/hash mismatch")
    samples = []
    with zipfile.ZipFile(archive) as source:
        for session in SESSIONS:
            for kind, labels in (("character", CHARACTERS), ("word", WORDS)):
                for index, expected in enumerate(labels):
                    member = PREFIX + session + ("/extra/" if kind == "word" else "/") + expected
                    parts = []
                    for name in (member, member + "_times"):
                        entry = source.getinfo(name)
                        if entry.file_size > 200_000:
                            raise ValueError("Oversized source member")
                        parts.append(source.read(entry))
                    points, intervals = parse_track(*parts)
                    sample_id = f"{session}-{kind}-{index:02}"
                    record = {"id": sample_id, "coordinateFrame": "pyglet-y-up",
                              "penUpMetadata": "absent", "points": points, "intervalsMs": intervals}
                    path = args.output / (sample_id + ".track.json")
                    path.write_text(json.dumps(record, ensure_ascii=False), encoding="utf-8")
                    samples.append({"id": sample_id, "kind": kind, "expected": expected,
                                    "writerId": int(session.split("_")[1]), "session": session,
                                    "file": path.name, "sourceMember": member,
                                    "sourceCoordinateSha256": hashlib.sha256(parts[0]).hexdigest(),
                                    "sourceTimingSha256": hashlib.sha256(parts[1]).hexdigest(),
                                    "trackSha256": hashlib.sha256(path.read_bytes()).hexdigest()})
    manifest = {"schema": 1, "commit": COMMIT, "archiveSha256": SHA, "source": URL,
                "dataDoi": "10.17632/3h6h5d7xg2.2", "license": "CC-BY-4.0",
                "selection": "Fixed before inference; vendor training writer overlap unknown",
                "penUpMetadata": "absent", "sourceGuidelineYUp": [150, 200, 250, 300],
                "guidelineSemantics": "Unspecified; CLI line frame is an explicit diagnostic hypothesis",
                "samples": samples}
    (args.output / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Imported {len(samples)} unsegmented tracks; no pen-up reconstruction")

if __name__ == "__main__":
    main()
