#!/usr/bin/env python3
"""Reproducible coordinate diagnostic, explicitly NOT an exact pen-up benchmark.

Reads only CSV data from the pinned public author archive; never executes its
scanner, extracts paths, guesses stroke breaks or rewrites Latin/Cyrillic text.
"""
import argparse
import csv
import hashlib
import io
import json
import math
from pathlib import Path
import re
import zipfile

COMMIT = "c6629026b1fdef51fd08e3d2fddffd82f97e60a5"
ARCHIVE_SHA = "2960052698884e00d8fa428295f538d52c30f47ea3f7695e5c7be610fd943edf"
ROOT = f"russian_handwritings_tracked-{COMMIT}"
DEFAULT_SYMBOLS = "а,и,м,р,А,И,М,Р,0,1,2"
WORDS = ("cъешь", "ещё", "этих", "мягких", "французских", "булок", "да", "выпей", "чаю")
METADATA = {
    "repository": "https://github.com/Skvayzer/russian_handwritings_tracked",
    "commit": COMMIT,
    "archiveUrl": f"https://codeload.github.com/Skvayzer/russian_handwritings_tracked/zip/{COMMIT}",
    "archiveSha256": ARCHIVE_SHA,
    "dataset": "https://data.mendeley.com/datasets/3h6h5d7xg2/2",
    "dataLicense": "CC BY 4.0 (Mendeley data release)",
    "repositoryCodeLicense": "GPL-3.0; no downloaded scanner code is executed",
    "penUpRecorded": False,
    "adaptation": "one flat trace; x=x-minX, y=maxY-y; no scale, smoothing or inferred breaks",
    "warning": "Missing pen-up state can create false connecting segments. This is NOT an exact handwriting accuracy benchmark.",
    "expectedTextUse": "offline metrics only; names preserved including author's Latin c in cъешь",
}


def parse_row(data):
    rows = list(csv.reader(io.StringIO(data.decode("utf-8-sig"))))
    if len(rows) != 1 or not rows[0]:
        raise ValueError("Expected one nonempty CSV row")
    values = [float(v) for v in rows[0]]
    if any(not math.isfinite(v) for v in values):
        raise ValueError("Nonfinite CSV value")
    return values


def adapt(coordinate_bytes, time_bytes):
    xy, times = parse_row(coordinate_bytes), parse_row(time_bytes)
    if len(xy) % 2:
        raise ValueError("Odd XY count")
    if len(times) != len(xy) // 2:
        raise ValueError("XY/time count mismatch")
    if not 1 <= len(times) <= 4096:
        raise ValueError("Outside portable first-source point budget 1..4096")
    if any(t < 0 for t in times):
        raise ValueError("Negative time delta")
    min_x, max_y = min(xy[0::2]), max(xy[1::2])
    points = [[xy[i] - min_x, max_y - xy[i + 1]] for i in range(0, len(xy), 2)]
    return points, times


def write_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=True, indent=2) + "\n", encoding="utf-8")


def import_archive(args):
    archive = Path(args.archive)
    if archive.stat().st_size > 64 * 1024 * 1024:
        raise ValueError("Archive exceeds diagnostic size budget")
    digest = hashlib.sha256(archive.read_bytes()).hexdigest()
    if digest != ARCHIVE_SHA:
        raise ValueError("Public archive fingerprint mismatch")
    sessions = args.sessions.split(",")
    symbols = args.symbols.split(",")
    if not 1 <= len(sessions) <= 37 or any(not re.fullmatch(r"w_\d+_\d+", s) for s in sessions):
        raise ValueError("Invalid explicit session IDs")
    if len(set(sessions)) != len(sessions) or len(set(symbols)) != len(symbols):
        raise ValueError("Duplicate selection")
    if any(len(s) != 1 or not (s.isdigit() and s.isascii() or "А" <= s <= "я" or s in "Ёё") for s in symbols):
        raise ValueError("Expected single Russian letter/digit selection")
    samples, errors = [], []
    with zipfile.ZipFile(archive) as z:
        for session in sessions:
            for relative, expected in [(s, s) for s in symbols] + [("extra/" + s, s) for s in WORDS]:
                member = f"{ROOT}/{session}/{relative}"
                identifier = f"{session}/{relative}"
                try:
                    data, times = z.read(member), z.read(member + "_times")
                    if len(data) > 1024 * 1024 or len(times) > 1024 * 1024:
                        raise ValueError("CSV entry exceeds portable budget")
                    points, deltas = adapt(data, times)
                    samples.append({"id": identifier, "expected": expected, "strokes": [points],
                                    "timeDeltasMs": deltas, "penUpRecorded": False,
                                    "coordinateCsvSha256": hashlib.sha256(data).hexdigest(),
                                    "timeCsvSha256": hashlib.sha256(times).hexdigest()})
                except (KeyError, ValueError, UnicodeError) as e:
                    errors.append({"id": identifier, "expected": expected, "error": str(e)})
    write_json(args.output, {"provenance": METADATA, "sessions": sessions, "symbols": symbols,
                             "selectedRecords": len(sessions) * (len(symbols) + len(WORDS)),
                             "samples": samples, "importErrors": errors})
    print(f"Imported {len(samples)} flat-coordinate records; import errors={len(errors)}; exact pen-up benchmark=FALSE")


def distance(a, b):
    previous = list(range(len(b) + 1))
    for i, x in enumerate(a, 1):
        current = [i]
        for j, y in enumerate(b, 1):
            current.append(min(current[-1] + 1, previous[j] + 1, previous[j - 1] + (x != y)))
        previous = current
    return previous[-1]


def summarize(args):
    corpus = json.loads(Path(args.corpus).read_text(encoding="utf-8"))
    capture = json.loads(Path(args.capture).read_text(encoding="utf-8"))
    external = [s for s in capture if s["external"]]
    ids = [s["id"] for s in corpus["samples"]]
    if [s["id"] for s in external] != ids:
        raise ValueError("Capture does not contain every imported sample in order")
    supported = [s for s in external if "error" not in s]
    rows = []
    for actual, supplied in zip(external, corpus["samples"]):
        if actual["expected"] != supplied["expected"]:
            raise ValueError("Expected metadata changed; expected text must be metrics-only")
        row = {k: actual[k] for k in ("id", "expected")}
        if "error" in actual:
            row["error"] = actual["error"]
        else:
            text = actual["publishedLiteralText"]
            row.update({"published": text, "allocated": actual["allocatedLiteralText"],
                        "groups": actual["groups"], "cost": actual["cost"],
                        "exact": text == actual["expected"],
                        "caseInsensitive": text is not None and text.casefold() == actual["expected"].casefold(),
                        "editDistance": distance(actual["expected"], text or "")})
        rows.append(row)
    report = {"provenance": corpus["provenance"], "selected": corpus["selectedRecords"],
              "importErrors": corpus["importErrors"], "supported": len(supported),
              "unsupported": len(external) - len(supported),
              "exact": sum(r.get("exact", False) for r in rows),
              "caseInsensitive": sum(r.get("caseInsensitive", False) for r in rows),
              "noPublishedWord": sum(s["publishedLiteralText"] is None for s in supported),
              "rows": rows,
              "interpretation": "Only coordinate adaptation diagnostics; no confusable rewrites, unsupported cases retained; first23F before main240 linguistic winner."}
    write_json(args.output, report)
    print(json.dumps({k: report[k] for k in ("selected", "supported", "unsupported", "exact", "caseInsensitive", "noPublishedWord")}))


def self_test():
    points, times = adapt(b'"-2","10","6","2"\n', b'"10","25"\n')
    assert points == [[0, 0], [8, 8]] and times == [10, 25]
    for xy, ts in ((b'"1","2","3"', b'"1"'), (b'"NaN","1"', b'"1"'), (b'"1","2"', b'"-1"'), (b'"1","2"', b'"1","2"')):
        try:
            adapt(xy, ts)
        except ValueError:
            continue
        raise AssertionError("Malformed coordinate/time input accepted")
    assert distance("мир", "мир") == 0 and distance("мир", "MИP") == 3
    assert WORDS[0][0] == "c" and WORDS[0][0] != "с"
    print("PASS coordinate axis/time ownership, malformed CSV guards, literal metrics and authored Latin c preservation")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="command", required=True)
    imp = sub.add_parser("import")
    imp.add_argument("archive")
    imp.add_argument("output")
    imp.add_argument("--sessions", default="w_0_1,w_1_1,w_2_1")
    imp.add_argument("--symbols", default=DEFAULT_SYMBOLS)
    summary = sub.add_parser("summarize")
    summary.add_argument("corpus")
    summary.add_argument("capture")
    summary.add_argument("output")
    sub.add_parser("self-test")
    args = parser.parse_args()
    {"import": import_archive, "summarize": summarize, "self-test": lambda _: self_test()}[args.command](args)
