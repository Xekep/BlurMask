"""Verify byte-exact recognition resources against the committed manifest."""
import hashlib
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1] / "models"
manifest = json.loads((root / "manifest.json").read_text(encoding="utf-8"))
if manifest["schemaVersion"] != 1 or not manifest["resources"]:
    raise ValueError("Unsupported or empty model manifest")
for resource in manifest["resources"]:
    path = (root / resource["path"]).resolve()
    if not path.is_relative_to(root) or path.stat().st_size != resource["bytes"]:
        raise ValueError(f"Model path/size mismatch: {resource['path']}")
    with path.open("rb") as stream:
        digest = hashlib.sha256()
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
        actual = digest.hexdigest()
    if actual != resource["sha256"]:
        raise ValueError(f"Model fingerprint mismatch: {resource['path']}")
print(f"PASS {len(manifest['resources'])} committed model resource fingerprints")
