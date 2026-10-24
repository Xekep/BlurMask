"""Build the English entrypoint; execute only model-free input helpers."""
from pathlib import Path
import argparse
import json
import os
import subprocess
import sys

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("output", type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
output = args.output.resolve()
output.mkdir(parents=True, exist_ok=False)
env = os.environ.copy()
env["DOTNET_PROCESSOR_COUNT"] = "2"
results = []


def run(stage, command, timeout):
    with (output / (stage + ".log")).open("wb") as log:
        result = subprocess.run(command, cwd=root, env=env, stdout=log,
                                stderr=subprocess.STDOUT, timeout=timeout)
    results.append({"stage": stage, "exitCode": result.returncode})
    (output / "results.json").write_text(json.dumps(results, indent=2) + "\n", encoding="utf8")
    if result.returncode:
        raise SystemExit(f"{stage} failed; see {output / (stage + '.log')}")


for name in ("LegacyEnglishEarlySelected.Probe", "LegacyEnglishEarlySelected.ActualEntry",
             "LegacyEnglishSelectedSources.Probe", "LegacyEnglishSelectedSources.ActualEntry"):
    run(name + "-build", ["dotnet", "build", str(root / "tools" / name),
        "-c", "Release", "--artifacts-path", str(output / "build"),
        "--disable-build-servers", "-p:UseSharedCompilation=false", "-m:1"], 360)

name = "LegacyEnglishEarlySelected.Probe"
capture = output / "prepared.json"
run("model-free-helpers", ["dotnet", str(output / "build" / "bin" / name /
    "release" / (name + ".dll")), str(capture)], 45)
run("independent-data-audit", [sys.executable, "-I", "-B", str(root / "tools" /
    name / "audit_output.py"), str(capture), str(output / "audit.json")], 45)
name = "LegacyEnglishSelectedSources.Probe"
capture = output / "selected-sources-prepared.json"
run("selected-sources-model-free", ["dotnet", str(output / "build" / "bin" / name /
    "release" / (name + ".dll")), str(capture)], 45)
run("selected-sources-independent-audit", [sys.executable, "-I", "-B", str(root / "tools" /
    name / "audit_output.py"), str(capture), str(output / "selected-sources-audit.json")], 45)
print("PASS model-free bootstrap/selected-source helpers and readonly own-IL order; both ActualEntries built only")
