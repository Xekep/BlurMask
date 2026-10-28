"""One read-only Ghidra session for an explicit batch of existing function entries.

Uses Ghidra's own LaunchSupport VM arguments and Java launcher without a shell.
Does not execute the analyzed binary. No export cache: project analysis state matters.
"""
import argparse
import json
import os
import re
import struct
import subprocess
import time
from pathlib import Path

from run_port_checks import digest, execute, write_json


def normalize_request(request):
    if request.get("schema") != 1 or not re.fullmatch(r"[0-9a-f]{64}", request.get("binarySha256", "")):
        raise ValueError("Expected schema=1 and lowercase binarySha256")
    addresses = request.get("addresses")
    if not isinstance(addresses, list) or not addresses:
        raise ValueError("An explicit nonempty address list is required")
    if any(not isinstance(value, str) or not re.fullmatch(r"0x[0-9a-fA-F]{1,16}", value) for value in addresses):
        raise ValueError("Addresses must be hexadecimal strings with a 0x prefix")
    seconds = request.get("decompileSeconds", 45)
    if type(seconds) is not int or not 1 <= seconds <= 120:
        raise ValueError("decompileSeconds must be an integer in [1,120]")
    return dict(schema=1, binarySha256=request["binarySha256"],
                addresses=[hex(value) for value in sorted({int(value, 16) for value in addresses})],
                decompileSeconds=seconds)


def pe_image(binary):
    data = binary.read_bytes()
    try:
        if data[:2] != b"MZ":
            raise ValueError("Expected PE image")
        pe = struct.unpack_from("<I", data, 0x3c)[0]
        if data[pe:pe + 4] != b"PE\0\0":
            raise ValueError("Invalid PE signature")
        count = struct.unpack_from("<H", data, pe + 6)[0]
        extent = struct.unpack_from("<H", data, pe + 20)[0]
        optional = pe + 24
        magic = struct.unpack_from("<H", data, optional)[0]
        if magic != 0x20b:
            raise ValueError("Native byte verifier currently requires PE32+")
        base = struct.unpack_from("<Q", data, optional + 24)[0]
        sections = []
        for i in range(count):
            _, rva, size, offset = struct.unpack_from("<IIII", data, optional + extent + i * 40 + 8)
            if offset + size > len(data):
                raise ValueError("PE section outside image")
            sections.append((rva, size, offset))
        return data, base, sections
    except struct.error as error:
        raise ValueError("Truncated PE image") from error


def verify_instructions(path, expected_count, image):
    data, base, sections = image
    lines = path.read_text(encoding="utf-8").splitlines()
    if not lines or lines[0] != "address\tbytes\tinstruction" or len(lines) - 1 != expected_count:
        raise ValueError("Instruction table count/header mismatch")
    previous_end = -1
    for line in lines[1:]:
        columns = line.split("\t")
        if (len(columns) != 3 or not re.fullmatch(r"[0-9a-fA-F]{1,16}", columns[0])
                or not re.fullmatch(r"(?:[0-9a-f]{2})+", columns[1]) or not columns[2]):
            raise ValueError("Invalid instruction row")
        address, raw = int(columns[0], 16), bytes.fromhex(columns[1])
        if address < previous_end:
            raise ValueError("Overlapping or unsorted instruction rows")
        previous_end = address + len(raw)
        rva = address - base
        offsets = [offset + rva - start for start, size, offset in sections
                   if start <= rva and rva + len(raw) <= start + size]
        if len(offsets) != 1 or data[offsets[0]:offsets[0] + len(raw)] != raw:
            raise ValueError("Exported instruction differs from original PE bytes")


def validate_export(output, request, program, binary):
    if digest(binary) != request["binarySha256"]:
        raise ValueError("Binary SHA256 differs from request")
    image = pe_image(binary)
    index = json.loads((output / "export-index.json").read_bytes())
    if (index.get("schema") != 1
            or index.get("binarySha256") != request["binarySha256"]
            or index.get("program") != program or index.get("vendorExecution") is not False
            or index.get("programMutation") is not False):
        raise ValueError("Export index identity/status mismatch")
    if index.get("status") != "passed":
        failed = [row.get("address", "?") + "=" + row.get("status", "missing-status")
                  for row in index.get("functions", []) if row.get("status") != "passed"]
        raise ValueError("Incomplete batch: " + (", ".join(failed) or "see export-index.json"))
    if int(index.get("imageBase", "-1"), 16) != image[1]:
        raise ValueError("Rebased program is not admitted by current byte verifier")
    rows = index.get("functions", [])
    if [row.get("address") for row in rows] != request["addresses"]:
        raise ValueError("Exported address sequence differs from complete request")
    for row in rows:
        if row.get("status") != "passed" or type(row.get("instructions")) is not int or row["instructions"] <= 0:
            raise ValueError("Incomplete function: " + row["address"])
        expected = {row["address"][2:] + suffix for suffix in (".c", ".instructions.tsv", ".references.tsv")}
        artifacts = row.get("artifacts", {})
        if set(artifacts) != expected:
            raise ValueError("Missing or unexpected function artifacts")
        for name, sha in artifacts.items():
            path = output / name
            if not path.is_file() or digest(path) != sha:
                raise ValueError("Artifact checksum mismatch: " + name)
        verify_instructions(output / (row["address"][2:] + ".instructions.tsv"), row["instructions"], image)
    index["verifiedInstructions"] = sum(row["instructions"] for row in rows)
    return index


def tree_pins(directory):
    return {p.relative_to(directory).as_posix(): digest(p)
            for p in sorted(directory.rglob("*")) if p.is_file() and not p.name.endswith(".lock")}


def export(args):
    started = time.monotonic()
    output = args.output.resolve()
    original = args.request.resolve()
    request = normalize_request(json.loads(original.read_bytes()))
    binary = args.binary.resolve(strict=True)
    if digest(binary) != request["binarySha256"]:
        raise ValueError("Requested binary SHA256 mismatch")
    if not re.fullmatch(r"[A-Za-z0-9_.-]+", args.project) or not re.fullmatch(r"[A-Za-z0-9_.-]+", args.program):
        raise ValueError("Project and program names must be literal names, not paths or patterns")
    project_dir = args.project_dir.resolve(strict=True)
    project_file = project_dir / (args.project + ".gpr")
    project_repo = project_dir / (args.project + ".rep")
    if not project_file.is_file() or not project_repo.is_dir():
        raise ValueError("Existing Ghidra project required")
    ghidra = args.ghidra_home.resolve(strict=True)
    java_home = args.java_home.resolve(strict=True)
    java = java_home / "bin" / ("java.exe" if os.name == "nt" else "java")
    utility = ghidra / "Ghidra/Framework/Utility/lib/Utility.jar"
    launcher = ghidra / "support/LaunchSupport.jar"
    script = Path(__file__).with_name("ghidra") / "ExportPortBatch.java"
    output.mkdir(parents=True, exist_ok=False)
    report = dict(schema=1, status="running", vendorExecution=False, invocations=0,
                  scope="READ_ONLY_STATIC_EXPORT_NOT_PORT_OR_RUNTIME_EQUIVALENCE")
    report_file = output / "report.json"
    write_json(report_file, report)
    try:
        project_pins = {"gpr": digest(project_file), "repository": tree_pins(project_repo)}
        tool_pins = {"ghidra": tree_pins(ghidra), "java": tree_pins(java_home),
                     "script": digest(script), "wrapper": digest(Path(__file__)),
                     "processHelper": digest(Path(__file__).with_name("run_port_checks.py")),
                     "request": digest(original), "binary": digest(binary)}
        report.update(projectPins=project_pins, toolPins=tool_pins, request=request)
        normalized = output / "request.json"
        write_json(normalized, request)
        normalized_sha = digest(normalized)
        environment = os.environ.copy()
        # Avoid inherited injection of arbitrary JVM arguments; no values are logged.
        for name in ("JAVA_TOOL_OPTIONS", "JDK_JAVA_OPTIONS", "_JAVA_OPTIONS"):
            environment.pop(name, None)
        vm = subprocess.run([str(java), "-cp", str(launcher), "LaunchSupport", str(ghidra), "-vmargs"],
                            env=environment, stdin=subprocess.DEVNULL, stdout=subprocess.PIPE,
                            stderr=subprocess.PIPE, text=True, encoding="utf-8", timeout=20,
                            creationflags=subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0,
                            check=True)
        vmargs = [line for line in vm.stdout.splitlines() if line]
        if "-Djava.system.class.loader=ghidra.GhidraClassLoader" not in vmargs:
            raise ValueError("Ghidra required class loader argument missing")
        argv = [str(java), *vmargs, "-Xmx2G", "-XX:ActiveProcessorCount=2", "-Djava.awt.headless=true",
                "-cp", str(utility), "ghidra.Ghidra", "ghidra.app.util.headless.AnalyzeHeadless",
                str(project_dir), args.project, "-process", args.program, "-readOnly", "-noanalysis",
                "-max-cpu", "2", "-scriptPath", str(script.parent.resolve()),
                "-postScript", script.name, str(normalized), str(output),
                "-log", str(output / "ghidra.log"), "-scriptlog", str(output / "script.log")]
        report.update(command=argv, invocations=1)
        write_json(report_file, report)
        code, timed_out = execute(argv, ghidra, environment, output / "stdout.log", args.timeout)
        report.update(exitCode=code, timedOut=timed_out)
        if code != 0 or timed_out:
            raise RuntimeError("Ghidra failed or timed out; inspect stdout.log")
        report["projectUnchanged"] = {"gpr": digest(project_file), "repository": tree_pins(project_repo)} == project_pins
        if not report["projectUnchanged"]:
            raise RuntimeError("Ghidra project content changed during read-only export")
        if (digest(binary) != tool_pins["binary"] or digest(original) != tool_pins["request"]
                or digest(normalized) != normalized_sha
                or digest(script) != tool_pins["script"]
                or digest(Path(__file__)) != tool_pins["wrapper"]
                or digest(Path(__file__).with_name("run_port_checks.py")) != tool_pins["processHelper"]
                or tree_pins(ghidra) != tool_pins["ghidra"] or tree_pins(java_home) != tool_pins["java"]):
            raise RuntimeError("Export inputs changed during execution")
        report["inputsUnchanged"] = True
        index = validate_export(output, request, args.program, binary)
        report.update(status="passed", functions=len(index["functions"]),
                      projectUnchanged=True, verifiedInstructions=index["verifiedInstructions"],
                      artifactPins={p: sha for p, sha in tree_pins(output).items() if p != "report.json"})
    except Exception as error:
        report.update(status="failed", error=str(error))
    except BaseException as error:
        report.update(status="failed", error=type(error).__name__)
        raise
    finally:
        report["seconds"] = time.monotonic() - started
        write_json(report_file, report)
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("request", "binary", "output"):
        parser.add_argument("--" + name, type=Path, required=True)
    for name in ("ghidra-home", "java-home", "project-dir"):
        parser.add_argument("--" + name, type=Path)
    parser.add_argument("--project")
    parser.add_argument("--program", required=True)
    parser.add_argument("--verify-only", action="store_true",
                        help="Verify an existing export and original PE without starting Ghidra")
    parser.add_argument("--timeout", type=int, default=300)
    args = parser.parse_args()
    if not 1 <= args.timeout <= 600:
        parser.error("--timeout must be in [1,600]")
    if args.verify_only:
        request = normalize_request(json.loads(args.request.read_bytes()))
        index = validate_export(args.output, request, args.program, args.binary)
        print(json.dumps(dict(status="passed", verifiedInstructions=index["verifiedInstructions"],
                              functions=len(index["functions"]), vendorExecution=False)))
        return 0
    if any(getattr(args, key) is None for key in ("ghidra_home", "java_home", "project_dir", "project")):
        parser.error("Export requires --ghidra-home, --java-home, --project-dir and --project")
    report = export(args)
    print(json.dumps({k: report[k] for k in ("status", "seconds", "functions", "error") if k in report}))
    return 0 if report["status"] == "passed" else 1


if __name__ == "__main__":
    raise SystemExit(main())
