"""Check impact propagation and deliberately incomplete static evidence."""
import tempfile
import unittest
from pathlib import Path

from plan_port_checks import impact, inventory


class ImpactTests(unittest.TestCase):
    def test_transitive_cycle_ambiguous_and_nonexecuting(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            files = {
                "tools/a.py": "raise RuntimeError('must not run')\nrefs=['b.py', 'Kernel.cs', 'duplicate.py']\n",
                "tools/b.py": "refs=['a.py']\n# 'missing.py'\n",
                "tools/c.py": "refs=['b.py', 'tools/left/duplicate.py']\n",
                "tools/left/duplicate.py": "pass\n",
                "tools/right/duplicate.py": "pass\n",
                "src/InkRecognition/Kernel.cs": "// fixture\n",
            }
            for name, text in files.items():
                path = root / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(text, encoding="utf-8")
            report = inventory(root)
            self.assertEqual(impact(report, ["src/InkRecognition/Kernel.cs"]),
                             ["tools/a.py", "tools/b.py", "tools/c.py"])
            self.assertEqual(len(report["unresolvedLiterals"]), 1)
            self.assertEqual(len(report["unresolvedLiterals"][0]["candidates"]), 2)
            self.assertEqual(impact(report, ["unseen.cs"]), [])
            first = report["scripts"]["tools/a.py"]["sha256"]
            (root / "tools/a.py").write_text("pass\n", encoding="utf-8")
            self.assertNotEqual(inventory(root)["scripts"]["tools/a.py"]["sha256"], first)

    def test_syntax_failure_is_retained(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "tools").mkdir()
            (root / "tools/bad.py").write_text("def (", encoding="utf-8")
            self.assertIsNotNone(inventory(root)["scripts"]["tools/bad.py"]["parseError"])


if __name__ == "__main__":
    unittest.main()
