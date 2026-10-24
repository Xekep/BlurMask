"""Portable source/DATA and interval checks; no recognition inference."""
import json
import os
from pathlib import Path
import subprocess
import sys
import time


def main():
    if len(sys.argv) != 2:
        raise SystemExit("FRESH_OUTPUT_DIRECTORY")
    root = Path(__file__).resolve().parents[2]
    output = Path(sys.argv[1]).resolve()
    output.mkdir(parents=True, exist_ok=False)
    environment = os.environ.copy()
    environment.update(DOTNET_PROCESSOR_COUNT="2", DOTNET_CLI_TELEMETRY_OPTOUT="1")
    build = output / "build"
    audit = root / "tools/LegacyCompletedStructuredWords.Audit"
    stages = [
        ("source", [sys.executable, "-I", "-B", str(audit / "audit_source.py"), str(root)], 45),
        ("prepared", [sys.executable, "-I", "-B", str(audit / "test_prepared.py"), str(output / "prepared.json")], 45),
        ("build", ["dotnet", "build", "tools/LegacyCompletedPostTextWork.Probe/LegacyCompletedPostTextWork.Probe.csproj",
                   "-c", "Release", "--artifacts-path", str(build), "-m:1",
                   "-p:UseSharedCompilation=false", "--disable-build-servers"], 360),
        ("interval", ["dotnet", str(build / "bin/LegacyCompletedPostTextWork.Probe/release/LegacyGlobalWordText.Probe.dll")], 45),
    ]
    results = []
    error = None
    try:
        for name, command, timeout in stages:
            started = time.monotonic()
            with (output / (name + ".log")).open("wb") as log:
                result = subprocess.run(command, cwd=root, env=environment, stdout=log,
                                        stderr=subprocess.STDOUT, timeout=timeout)
            results.append({"stage": name, "exitCode": result.returncode,
                            "seconds": time.monotonic() - started})
            if result.returncode:
                raise RuntimeError(f"Structured WORD {name} failed; see {output / (name + '.log')}")
    except BaseException as exception:
        error = str(exception)
        raise
    finally:
        (output / "report.json").write_text(json.dumps({
            "stages": results, "error": error, "processorCount": 2,
            "passed": len(results) == len(stages) and error is None,
            "scope": "Source pins, synthetic DATA and interval controls; no owning recognition, models or desktop lifetime",
        }, indent=2) + "\n", encoding="utf8")


if __name__ == "__main__":
    main()
