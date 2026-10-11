"""Fetch independent research ink to a private directory; never package it with the app.

MathWriting data: CC BY-NC-SA 4.0 (Google Research archive README).
Only a pinned small excerpt is downloaded. Expected labels are used for reporting,
never supplied to the recognizer. Requires Python 3.10+, standard library only.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import tarfile
import urllib.request
import xml.etree.ElementTree as ET

URL = "https://storage.googleapis.com/mathwriting_data/mathwriting-2024-excerpt.tgz"
SHA = "cba038def001480a89962b25cb20a60df4c4145e94c86ef9d2af65f192cb82bc"
# Fixed selection made by labels/categories before any recognition runs.
SELECTION = [
    ("valid", "0019bb8e97507f8e", "integral"),
    ("valid", "00245713d08b4f1a", "integral"),
    ("valid", "00235ec3dee919dd", "improper-integral"),
    ("valid", "012f1f23d40a2995", "contour-integral"),
    ("valid", "00146a32fdab9e8e", "derivative"),
    ("valid", "003921302c543b2c", "differential-equation"),
    ("valid", "002b6a98000765fb", "partial-derivative"),
    ("valid", "01927fe0f1d3c23d", "matrix"),
    ("valid", "00a32b900a4aa32f", "matrix"),
    ("valid", "00e00364101abad0", "matrix"),
    ("test", "01752f0f5fba0225", "matrix"),
    ("train", "033d57e0703ab948", "limit"),
    ("valid", "01468b08b993df97", "limit"),
]
NS = {"i": "http://www.w3.org/2003/InkML"}


def fetch(url, target, maximum, expected=None):
    if not target.exists():
        with urllib.request.urlopen(url, timeout=60) as response:
            data = response.read(maximum + 1)
        if len(data) > maximum:
            raise ValueError("Download exceeds bounded size")
        target.write_bytes(data)
    data = target.read_bytes()
    digest = hashlib.sha256(data).hexdigest()
    if len(data) > maximum or expected and digest != expected:
        raise ValueError(f"Unexpected resource: {target}")
    return data, digest


def validate(strokes):
    if not 1 <= len(strokes) <= 256 or not 1 <= sum(map(len, strokes)) <= 20000:
        raise ValueError("Ink exceeds corpus limits")
    for stroke in strokes:
        if not stroke:
            raise ValueError("Empty stroke")
        for point in stroke:
            if len(point) != 2 or any(not math.isfinite(p) or abs(p) > 100000 for p in point):
                raise ValueError("Invalid XY")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    out = args.output.resolve()
    out.mkdir(parents=True, exist_ok=True)
    archive = out / "mathwriting-2024-excerpt.tgz"
    fetch(URL, archive, 2_000_000, SHA)
    samples = []
    with tarfile.open(archive, "r:gz") as tar:
        for split, sample_id, category in SELECTION:
            member = f"mathwriting-2024-excerpt/{split}/{sample_id}.inkml"
            item = tar.getmember(member)
            if not item.isfile() or item.size > 1_000_000:
                raise ValueError("Invalid bounded archive member")
            data = tar.extractfile(item).read()
            # No extractall and no external entities. Reject declarations explicitly.
            if b"<!DOCTYPE" in data or b"<!ENTITY" in data:
                raise ValueError("Unsupported XML declarations")
            root = ET.fromstring(data)
            labels = {a.attrib["type"]: a.text for a in root.findall("i:annotation", NS)}
            if labels.get("inkCreationMethod") != "human":
                raise ValueError("Expected independent human ink")
            channels = [c.attrib["name"] for c in root.findall("i:traceFormat/i:channel", NS)]
            if channels[:2] != ["X", "Y"]:
                raise ValueError("Unexpected channel order")
            strokes = []
            for trace in root.findall("i:trace", NS):
                strokes.append([[float(v) for v in p.split()[:2]] for p in trace.text.split(",")])
            validate(strokes)
            name = f"{sample_id}.json"
            (out / name).write_text(json.dumps(strokes, separators=(",", ":")), encoding="utf-8")
            samples.append(dict(id=sample_id, category=category, strokes=name, expected=labels["normalizedLabel"],
                                authorLabel=labels["label"], source="Google MathWriting 2024 excerpt", split=split,
                                creationMethod="human", sourceMember=member, sourceSha256=hashlib.sha256(data).hexdigest()))
    commit = "4374b0c6d948da4fcf8027d7c4027abaa836d46d"
    base = f"https://raw.githubusercontent.com/MyScript/interactive-ink-additional-examples-android/{commit}/samples/batch-mode/src/main/assets/conf/pointerEvents/math/"
    events, events_hash = fetch(base + "pointerEvents.json", out / "pointerEvents.json", 1_000_000,
                               "821155f336fe9154c97e2ff5d495749c3ce722109d43221cb8a62f3e9a79fd12")
    label, label_hash = fetch(base + "export.tex", out / "export.tex", 10000,
                             "deb1dcbfd33d768dc2abae707cd8761c81f6b40fc35e27a28441bac2c7483d1f")
    strokes = []
    for event in json.loads(events)["events"]:
        if event["pointerType"] != "PEN" or len(event["x"]) != len(event["y"]):
            raise ValueError("Unexpected pointer event")
        strokes.append(list(map(list, zip(event["x"], event["y"]))))
    validate(strokes)
    (out / "android-quadratic.json").write_text(json.dumps(strokes), encoding="utf-8")
    samples.append(dict(id="android-quadratic", category="quadratic-root", strokes="android-quadratic.json",
                        expected=label.decode("utf-8").strip(), source="MyScript official Android batch-mode sample",
                        sourceCommit=commit, sourceSha256=events_hash, labelSha256=label_hash))
    manifest = dict(schemaVersion=1, archiveUrl=URL, archiveSha256=SHA,
                    dataLicense="MathWriting: CC BY-NC-SA 4.0; research audit only, not application assets",
                    conversion="Original XY and trace order preserved; time/pressure ignored; no resampling or rasterization",
                    samples=samples)
    (out / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Prepared {len(samples)} independently labeled ink samples: {out / 'manifest.json'}")


if __name__ == "__main__":
    main()
