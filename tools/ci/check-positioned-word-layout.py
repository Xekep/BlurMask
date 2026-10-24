"""Check positioned WORD delivery without loading recognition models or user settings.

Run after publishing the native app: EXECUTABLE FRESH_OUTPUT_DIRECTORY.
Linux callers must provide a display, for example through xvfb-run.
"""
import json
import os
from pathlib import Path
import subprocess
import sys
import time


def main():
    if len(sys.argv) != 3:
        raise SystemExit("EXECUTABLE FRESH_OUTPUT_DIRECTORY")
    executable = Path(sys.argv[1]).resolve(strict=True)
    output = Path(sys.argv[2]).resolve()
    output.mkdir(parents=True, exist_ok=False)
    environment = os.environ.copy()
    environment["DOTNET_PROCESSOR_COUNT"] = "2"
    commands = [
        ("layout", [str(executable), "--positioned-word-layout-test"]),
        ("font", [str(executable), "--positioned-word-font-test", str(output / "words.png")]),
    ]
    stages = []
    try:
        for name, command in commands:
            started = time.monotonic()
            with (output / (name + ".log")).open("wb") as log:
                result = subprocess.run(command, env=environment, stdout=log,
                                        stderr=subprocess.STDOUT, timeout=45)
            stages.append({"stage": name, "exitCode": result.returncode,
                           "seconds": time.monotonic() - started})
            if result.returncode:
                raise RuntimeError(f"Positioned WORD {name} failed; see {output / (name + '.log')}")
        if not (output / "words.png").is_file():
            raise RuntimeError("Font check did not produce its rendered artifact.")
    finally:
        (output / "report.json").write_text(json.dumps({
            "stages": stages, "stageTimeoutSeconds": 45, "processorCount": 2,
            "scope": "Prepared layout and actual font rendering; no recognition inference, user settings or desktop lifetime",
        }, indent=2) + "\n", encoding="utf8")


if __name__ == "__main__":
    main()
