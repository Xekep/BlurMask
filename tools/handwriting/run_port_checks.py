"""Run explicit, reviewed Python checks with bounded processes and verified receipts.

Cache is opt-in per step. This runner cannot discover undeclared inputs.
It is not a substitute for compiled-source/PDB or release admission gates.
"""
import argparse
import hashlib
import json
import os
import platform
import re
import signal
import subprocess
import sys
import sysconfig
import time
from pathlib import Path


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def object_hash(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, separators=(",", ":")).encode()).hexdigest()


def write_json(path, value):
    temporary = path.with_suffix(".tmp")
    temporary.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")
    temporary.replace(path)


def inside(root, name):
    path = (root / name).resolve()
    if Path(name).is_absolute() or not path.is_relative_to(root):
        raise ValueError("Path escapes declared root: " + name)
    return path


def input_pins(root, patterns):
    if not patterns:
        raise ValueError("Every step requires explicit inputs")
    files = set()
    for pattern in patterns:
        inside(root, pattern)
        matches = [p for p in root.glob(pattern) if p.is_file()
                   and not {"bin", "obj", "__pycache__"} & set(p.relative_to(root).parts)]
        if not matches:
            raise ValueError("Input matched no files: " + pattern)
        for path in matches:
            inside(root, path.relative_to(root).as_posix())
            files.add(path)
    return {p.relative_to(root).as_posix(): digest(p) for p in sorted(files)}


def python_runtime():
    # Cacheable profiles are stdlib + explicitly declared local sources only.
    # Fingerprint Python implementation, stdlib, extension modules and DLLs;
    # third-party packages require a separate explicit input declaration.
    files = {Path(sys.executable).resolve()}
    library = Path(sysconfig.get_path("stdlib"))
    for directory, folders, names in os.walk(library):
        folders[:] = sorted(set(folders) - {"site-packages", "__pycache__", "test", "tests"})
        files.update(Path(directory) / name for name in names if name.endswith(".py"))
    for directory in {Path(sys.base_prefix), Path(sys.base_prefix) / "DLLs",
                      Path(sysconfig.get_config_var("DESTSHARED") or sys.base_prefix)}:
        if directory.is_dir():
            for pattern in ("*.dll", "*.pyd", "*.so", "python*.zip"):
                files.update(directory.glob(pattern))
    return {"version": sys.version, "platform": platform.platform(),
            "files": {str(p): digest(p) for p in sorted(files) if p.is_file()}}


def validate(plan):
    if plan.get("schema") != 1 or not isinstance(plan.get("steps"), list) or not plan["steps"]:
        raise ValueError("Expected schema=1 and nonempty steps")
    by_id = {}
    for step in plan["steps"]:
        name = step["id"]
        if not re.fullmatch(r"[a-z0-9][a-z0-9-]*", name) or name in by_id:
            raise ValueError("Invalid or duplicate step id: " + name)
        if (not isinstance(step.get("argv"), list) or not step["argv"]
                or step["argv"][0] != "{python}"):
            raise ValueError("Only explicit {python} commands are currently supported")
        if not all(isinstance(arg, str) for arg in step["argv"]):
            raise ValueError("argv must contain strings")
        timeout = step.get("timeoutSeconds", 45)
        if type(timeout) not in (int, float) or not 0 < timeout <= 600:
            raise ValueError("timeoutSeconds must be in (0,600]")
        if step.get("cache", False) and step.get("inputsComplete") is not True:
            raise ValueError("Cache requires explicit inputsComplete=true review")
        by_id[name] = step
    ordered, visiting, done = [], set(), set()

    def visit(name):
        if name in visiting:
            raise ValueError("Dependency cycle: " + name)
        if name in done:
            return
        if name not in by_id:
            raise ValueError("Missing dependency: " + name)
        visiting.add(name)
        for dependency in by_id[name].get("dependsOn", []):
            visit(dependency)
        visiting.remove(name)
        done.add(name)
        ordered.append(by_id[name])

    for name in by_id:
        visit(name)
    return ordered


def terminate_tree(process):
    if os.name == "nt":
        subprocess.run(["taskkill", "/PID", str(process.pid), "/T", "/F"],
                       stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                       timeout=10, creationflags=subprocess.CREATE_NO_WINDOW)
    else:
        try:
            os.killpg(process.pid, signal.SIGKILL)
        except ProcessLookupError:
            pass
    if process.poll() is None:
        process.kill()
    process.wait(timeout=10)


def execute(argv, root, environment, log, timeout):
    with log.open("xb") as stream:
        process = subprocess.Popen(argv, cwd=root, env=environment, stdin=subprocess.DEVNULL,
                                   stdout=stream, stderr=subprocess.STDOUT,
                                   start_new_session=os.name != "nt",
                                   creationflags=subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0)
        try:
            return process.wait(timeout=timeout), False
        except subprocess.TimeoutExpired:
            terminate_tree(process)
            return process.returncode, True
        except BaseException:
            terminate_tree(process)
            raise


def valid_receipt(receipt, key):
    try:
        return (receipt["key"] == key and receipt["status"] == "passed"
                and bool(receipt["artifacts"])
                and all(Path(p).is_file() and digest(p) == sha
                        for p, sha in receipt["artifacts"].items()))
    except (OSError, KeyError, TypeError):
        return False


def run_plan(root, manifest, output, cache, no_cache=False):
    started = time.monotonic()
    root, manifest, output, cache = (p.resolve() for p in (root, manifest, output, cache))
    plan_raw = manifest.read_bytes()
    steps = validate(json.loads(plan_raw))
    output.mkdir(parents=True, exist_ok=False)
    cache.mkdir(parents=True, exist_ok=True)
    environment = os.environ.copy()
    environment.update(PYTHONDONTWRITEBYTECODE="1", PYTHONHASHSEED="0")
    environment.pop("PYTHONOPTIMIZE", None)  # Existing audits rely on assertions.
    runtime_started = time.monotonic()
    runtime = python_runtime()
    runtime_seconds = time.monotonic() - runtime_started
    identity = {"runner": digest(Path(__file__)), "manifest": hashlib.sha256(plan_raw).hexdigest(),
                "runtime": object_hash(runtime), "root": str(root),
                # Only hash environment values: do not publish credentials.
                "environment": object_hash(dict(environment))}
    report = {"schema": 1, "status": "running", "identity": identity, "steps": [],
              "runtimeFingerprintSeconds": runtime_seconds,
              "scope": "EXPLICIT_PYTHON_CHECKS_NOT_ENGINE_OR_RELEASE_COMPLETENESS"}
    report_path = output / "report.json"
    write_json(report_path, report)
    receipts = {}
    pending_receipts = {}
    try:
        for step in steps:
            name = step["id"]
            pins = input_pins(root, step["inputs"])
            dependencies = {name: receipts[name] for name in step.get("dependsOn", [])}
            key = object_hash({"identity": identity, "step": step, "inputs": pins,
                               "dependencies": dependencies})
            row = {"id": name, "key": key, "inputPins": pins, "status": "running"}
            report["steps"].append(row)
            write_json(report_path, report)
            cache_path = cache / (key + ".json")
            cached = None
            if step.get("cache", False) and not no_cache and cache_path.is_file():
                try:
                    cached = json.loads(cache_path.read_bytes())
                except (OSError, ValueError):
                    pass
            if cached and valid_receipt(cached, key):
                row.update(status="cached", artifacts=cached["artifacts"],
                           originalSeconds=cached["seconds"], seconds=0)
                receipts[name] = cached
            else:
                stage = output / name
                stage.mkdir()
                argv = [arg.replace("{python}", sys.executable).replace("{root}", str(root))
                        .replace("{out}", str(stage)) for arg in step["argv"]]
                # Do not consume stale local .pyc files outside the pinned sources.
                argv[1:1] = ["-B", "-X", "pycache_prefix=" + str(stage / "pycache")]
                start = time.monotonic()
                code, timed_out = execute(argv, root, environment, stage / "stdout.log",
                                          step.get("timeoutSeconds", 45))
                row.update(seconds=time.monotonic() - start, exitCode=code,
                           timedOut=timed_out, status="failed")
                if code != 0 or timed_out:
                    raise RuntimeError("Check failed: " + name)
                if input_pins(root, step["inputs"]) != pins or manifest.read_bytes() != plan_raw:
                    raise RuntimeError("Inputs changed during check: " + name)
                for expected in step.get("outputs", []):
                    if not inside(stage, expected).is_file():
                        raise RuntimeError("Missing output: " + name + "/" + expected)
                artifacts = {str(p): digest(p) for p in sorted(stage.rglob("*")) if p.is_file()}
                receipt = {"key": key, "status": "passed", "seconds": row["seconds"],
                           "artifacts": artifacts}
                row.update(status="passed", artifacts=artifacts)
                receipts[name] = receipt
                if step.get("cache", False) and not no_cache:
                    pending_receipts[cache_path] = receipt
            write_json(report_path, report)
        # Refuse a completed report if earlier steps' inputs changed later.
        for step, row in zip(steps, report["steps"]):
            if input_pins(root, step["inputs"]) != row["inputPins"]:
                raise RuntimeError("Inputs changed after check: " + step["id"])
            if not valid_receipt(receipts[step["id"]], row["key"]):
                raise RuntimeError("Artifacts changed after check: " + step["id"])
        if (manifest.read_bytes() != plan_raw or digest(Path(__file__)) != identity["runner"]
                or object_hash(python_runtime()) != identity["runtime"]):
            raise RuntimeError("Manifest, runner or Python runtime changed during run")
        for cache_path, receipt in pending_receipts.items():
            write_json(cache_path, receipt)
        report["status"] = "passed"
    except Exception as exc:
        report.update(status="failed", error=str(exc))
    except BaseException as exc:
        report.update(status="failed", error=type(exc).__name__)
        raise
    finally:
        report["totalSeconds"] = time.monotonic() - started
        write_json(report_path, report)
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("manifest", type=Path)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--cache", type=Path, required=True)
    parser.add_argument("--no-cache", action="store_true")
    args = parser.parse_args()
    report = run_plan(args.root, args.manifest, args.output, args.cache, args.no_cache)
    print(json.dumps({"status": report["status"], "totalSeconds": report["totalSeconds"],
                      "steps": [{k: row[k] for k in ("id", "status", "seconds") if k in row}
                                for row in report["steps"]]}))
    return 0 if report["status"] == "passed" else 1


if __name__ == "__main__":
    raise SystemExit(main())
