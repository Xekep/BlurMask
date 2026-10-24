"""Run scalar geometry and independent oracles; no inference or desktop host."""
from pathlib import Path
import argparse
import json
import os
import subprocess
import sys

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("output", type=Path)
parser.add_argument("--build-artifacts", type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
output = args.output.resolve()
output.mkdir(parents=True, exist_ok=False)
artifacts = (args.build_artifacts or output / "build").resolve()
env = os.environ.copy()
env.update(DOTNET_PROCESSOR_COUNT="2", OPENBLAS_NUM_THREADS="1", OMP_NUM_THREADS="1")
results = []


def run(stage, command, timeout):
    with (output / (stage + ".log")).open("wb") as log:
        result = subprocess.run(command, cwd=root, env=env, stdout=log,
                                stderr=subprocess.STDOUT, timeout=timeout)
    results.append({"stage": stage, "exitCode": result.returncode})
    (output / "results.json").write_text(json.dumps(results, indent=2) + "\n", encoding="utf8")
    if result.returncode:
        raise SystemExit(f"{stage} failed; see {output / (stage + '.log')}")


projects = ["LegacyMathAkProcessedPrefix.Probe", "LegacyMathAkOriginalInput.Probe",
            "LegacyMathAkOperator13B6Geometry.Probe", "LegacyMathAkOperator13B6.Probe",
            "LegacyMathAkOperator13A0.Probe", "LegacyMathAkOperator13A2.Probe"]
for name in projects:
    project = root / "tools" / name
    run(name + "-build", ["dotnet", "build", str(project / (name + ".csproj")),
        "-c", "Release", "--artifacts-path", str(artifacts), "--disable-build-servers",
        "-p:UseSharedCompilation=false", "-m:1"], 360)
    fixture = root / "tools" / projects[0] / "fixtures.json" if name == projects[1] else project / "fixtures.json"
    if name == "LegacyMathAkOperator13A2.Probe":
        fixture = root / "tools" / "LegacyMathAkOperator13A0.Probe" / "fixtures.json"
    capture, report = output / (name + ".json"), output / (name + "-audit.json")
    run(name + "-scalar", ["dotnet", str(artifacts / "bin" / name / "release" / (name + ".dll")),
        str(root / "models"), str(fixture), str(capture)], 45)
    oracle_args = [str(root), str(capture), str(report)]
    if name in ["LegacyMathAkOperator13A0.Probe", "LegacyMathAkOperator13A2.Probe"]:
        oracle_args = [str(root), str(root / "models"), str(fixture), str(capture), str(report)]
    run(name + "-oracle", [sys.executable, "-I", "-B", str(project / "audit_output.py"), *oracle_args], 45)
print("PASS six scalar geometry stages and independent oracles; no inference")
