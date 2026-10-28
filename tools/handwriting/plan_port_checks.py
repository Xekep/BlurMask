"""Static check-impact inventory. Does not import or execute analyzed scripts.

Literal references are evidence for review, not a complete runtime dependency DAG.
Output is deliberately NOT an executable test plan or a recognition coverage score.
"""
import argparse
import ast
import hashlib
import json
from collections import defaultdict, deque
from pathlib import Path


def normalize(value):
    return value.replace("\\", "/").removeprefix("./")


def inventory(root):
    root = root.resolve()
    tracked = []
    for folder, pattern in (("tools", "*.py"), ("src/InkRecognition", "*.cs"),
                            ("models", "*")):
        tracked.extend(p for p in (root / folder).rglob(pattern)
                       if p.is_file() and not {"bin", "obj", "__pycache__"} & set(p.relative_to(root).parts))
    paths = {p.relative_to(root).as_posix(): p for p in sorted(tracked)}
    names = defaultdict(list)
    for name in paths:
        names[Path(name).name].append(name)
    scripts = {}
    unresolved = []
    for name, path in paths.items():
        if path.suffix != ".py":
            continue
        raw = path.read_bytes()
        row = {"sha256": hashlib.sha256(raw).hexdigest(), "references": [],
               "imports": [], "parseError": None}
        scripts[name] = row
        try:
            tree = ast.parse(raw.decode("utf-8-sig"), filename=name)
        except (SyntaxError, UnicodeError) as exc:
            row["parseError"] = str(exc)
            continue
        refs = defaultdict(set)
        for node in ast.walk(tree):
            if isinstance(node, ast.Import):
                row["imports"].extend(alias.name for alias in node.names)
            elif isinstance(node, ast.ImportFrom) and node.module:
                row["imports"].append(node.module)
            if not isinstance(node, ast.Constant) or not isinstance(node.value, str):
                continue
            value = normalize(node.value)
            # Only entire path literals; prose, regular expressions and docstrings
            # cannot establish an execution dependency.
            if "\n" in value or " " in value or not value.endswith((".py", ".cs", ".res", ".json")):
                continue
            relative = (Path(name).parent / value).as_posix()
            candidates = ([value] if value in paths else
                          [relative] if relative in paths else
                          names.get(value, []) if "/" not in value else [])
            if len(candidates) == 1:
                if candidates[0] != name:
                    refs[candidates[0]].add(node.lineno)
            else:
                unresolved.append({"script": name, "line": node.lineno,
                                   "literal": value, "candidates": list(candidates)})
        row["references"] = [{"path": target, "lines": sorted(lines)}
                             for target, lines in sorted(refs.items())]
        row["imports"] = sorted(set(row["imports"]))
    return {"schema": 1, "scope": "STATIC_LITERAL_REFERENCES_NOT_RUNTIME_CLOSURE",
            "vendorExecution": False, "scripts": scripts,
            "unresolvedLiterals": unresolved}


def impact(report, changed):
    reverse = defaultdict(set)
    for script, row in report["scripts"].items():
        for ref in row["references"]:
            reverse[ref["path"]].add(script)
    seen = set(changed)
    queue = deque(sorted(seen))
    while queue:
        for parent in sorted(reverse[queue.popleft()]):
            if parent not in seen:
                seen.add(parent)
                queue.append(parent)
    return sorted(seen & report["scripts"].keys())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--changed", action="append", default=[],
                        help="Repository-relative path; repeat for several files")
    args = parser.parse_args()
    changed = [normalize(p) for p in args.changed]
    if any(Path(p).is_absolute() or ":" in p or ".." in Path(p).parts for p in changed):
        parser.error("--changed requires repository-relative paths without '..'")
    report = inventory(args.root)
    consumers = defaultdict(set)
    for name, row in report["scripts"].items():
        for ref in row["references"]:
            if ref["path"].endswith(".py"):
                consumers[ref["path"]].add(name)
    report["sharedScriptReferences"] = [
        {"path": name, "consumers": sorted(users)}
        for name, users in sorted(consumers.items(), key=lambda item: (-len(item[1]), item[0]))
        if len(users) > 1]
    report["changed"] = changed
    report["candidateAffectedScripts"] = impact(report, changed)
    report["limitations"] = [
        "References may be data, imports or subprocess arguments; execution is not inferred.",
        "Computed paths, directory globbing, implicit build inputs and external captures are incomplete.",
        "No affected scripts does not mean no checks are required.",
        "Shared references do not establish redundant execution or interchangeable arguments."]
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("x", encoding="utf-8") as stream:
        json.dump(report, stream, ensure_ascii=False, indent=2)
        stream.write("\n")
    errors = sum(row["parseError"] is not None for row in report["scripts"].values())
    print(json.dumps({"scripts": len(report["scripts"]), "parseErrors": errors,
                      "sharedReferences": len(report["sharedScriptReferences"]),
                      "affectedCandidates": len(report["candidateAffectedScripts"])}))
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
