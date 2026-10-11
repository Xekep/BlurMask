"""Create and verify the separate release model archive from the catalog."""
import argparse
import hashlib
import json
from pathlib import Path
import runpy
import zipfile

parser = argparse.ArgumentParser()
parser.add_argument("output", type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
runpy.run_path(str(root / "tools/verify-model-resources.py"))
manifest = json.loads((root / "models/manifest.json").read_text(encoding="utf-8"))
paths = [root / "models" / item["path"] for item in manifest["resources"]]
paths += [root / "models/manifest.json", root / "models/README.md"]
args.output.parent.mkdir(parents=True, exist_ok=True)
with zipfile.ZipFile(args.output, "w", zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
    for path in sorted(paths):
        archive.write(path, path.relative_to(root).as_posix())
with zipfile.ZipFile(args.output) as archive:
    if len(archive.namelist()) != len(paths) or archive.testzip() is not None:
        raise ValueError("Invalid model archive members or CRC")
    for item in manifest["resources"]:
        data = archive.read("models/" + item["path"])
        if len(data) != item["bytes"] or hashlib.sha256(data).hexdigest() != item["sha256"]:
            raise ValueError("Archived resource fingerprint differs")
print(f"PASS separate model archive: {len(manifest['resources'])} catalogued assets")
