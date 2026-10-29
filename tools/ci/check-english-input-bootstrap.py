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


run("bootstrap-source-restoration-controls", [sys.executable, "-I", "-B", str(root / "tools" /
    "LegacyEnglishOriginalInput.ActualEntry" / "test_bootstrap_preimage.py"), str(root),
    str(output / "bootstrap-source-restoration-controls.json")], 45)

run("original-input-source-contracts", [sys.executable, "-I", "-B", str(root / "tools" /
    "LegacyEnglishOriginalInput.ActualEntry" / "source_contract.py"), str(root),
    str(output / "original-input-source-contracts.json")], 45)

for name in ("LegacyEnglishEarlySelected.Probe", "LegacyEnglishEarlySelected.ActualEntry",
             "LegacyEnglishSelectedSources.Probe", "LegacyEnglishSelectedSources.ActualEntry",
             "LegacyEnglishSourceIndices.Probe", "LegacyEnglishSourceIndices.ActualEntry",
             "LegacyEnglishOriginalInput.ActualEntry"):
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
name = "LegacyEnglishSourceIndices.Probe"
capture = output / "source-indices-prepared.json"
run("source-indices-model-free", ["dotnet", str(output / "build" / "bin" / name /
    "release" / (name + ".dll")), "--prepared", str(capture)], 45)
run("source-indices-independent-audit", [sys.executable, "-I", "-B", str(root / "tools" /
    name / "audit.py"), "--prepared", str(capture), str(output / "source-indices-prepared-audit.json"), "--tamper"], 45)
fixtures = root / "tools" / name / "fixtures"
run("source-indices-stored-actual-data", [sys.executable, "-I", "-B", str(root / "tools" /
    name / "audit.py"), "--actual", str(fixtures / "selected-prior.json"),
    str(fixtures / "source-indices-actual.json"), str(output / "source-indices-actual-audit.json"), "--tamper"], 45)
name = "LegacyEnglishOriginalInput.ActualEntry"
fixtures = root / "tools" / name / "fixtures"
run("original-input-stored-actual-data", [sys.executable, "-I", "-B", str(root / "tools" / name / "audit.py"),
    str(fixtures / "prior-source-indices.json"), str(fixtures / "original-input-actual.json"),
    str(fixtures / "original-raw-index4.json"), str(output / "original-input-actual-audit.json"), "--tamper", "--fixed-slice"], 45)
print("PASS model-free bootstrap/selected/neutral-index helpers +stored actual DATA; four ActualEntries built only")
