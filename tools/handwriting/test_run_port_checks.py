"""Exercise actual subprocesses, invalidation, artifact tampering and refusals."""
import json
import os
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from run_port_checks import run_plan, validate


class RunnerTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.source = self.root / "check.py"
        self.source.write_text("import sys\nfrom pathlib import Path\n"
                               "Path(sys.argv[1], 'result.txt').write_text('checked')\n", encoding="utf-8")
        self.manifest = self.root / "plan.json"
        self.plan = {"schema": 1, "steps": [{"id": "one", "argv": ["{python}", "{root}/check.py", "{out}"],
                     "inputs": ["check.py"], "outputs": ["result.txt"], "timeoutSeconds": 5,
                     "cache": True, "inputsComplete": True}]}
        self.count = 0
        # Real subprocess tests use a fixed runtime identity; full runtime hashing
        # is exercised separately by the repository profile, not mocked there.
        self.runtime = patch("run_port_checks.python_runtime", return_value={"testRuntime": 1})
        self.runtime.start()
        self.addCleanup(self.runtime.stop)

    def run_case(self, no_cache=False):
        self.count += 1
        self.manifest.write_text(json.dumps(self.plan), encoding="utf-8")
        return run_plan(self.root, self.manifest, self.root / str(self.count),
                        self.root / "cache", no_cache)

    def test_hit_then_content_environment_arguments_output_and_runtime_invalidation(self):
        first = self.run_case()
        self.assertEqual(first["status"], "passed")
        self.assertEqual(self.run_case()["steps"][0]["status"], "cached")
        artifact = next(p for p in first["steps"][0]["artifacts"] if p.endswith("result.txt"))
        Path(artifact).write_text("tampered")
        self.assertEqual(self.run_case()["steps"][0]["status"], "passed")
        self.source.write_text(self.source.read_text() + "# changed\n")
        self.assertEqual(self.run_case()["steps"][0]["status"], "passed")
        with patch.dict(os.environ, {"BLURMASK_TEST_INPUT": "changed"}):
            self.assertEqual(self.run_case()["steps"][0]["status"], "passed")
        self.plan["steps"][0]["argv"].append("new-argument")
        self.assertEqual(self.run_case()["steps"][0]["status"], "passed")
        with patch("run_port_checks.python_runtime", return_value={"testRuntime": 2}):
            self.assertEqual(self.run_case()["steps"][0]["status"], "passed")
        self.assertEqual(self.run_case(no_cache=True)["steps"][0]["status"], "passed")

    def test_failure_timeout_and_missing_output_are_not_cached(self):
        for body, timeout in [("raise SystemExit(7)", 5), ("import time; time.sleep(20)", 0.2), ("pass", 5)]:
            self.source.write_text(body)
            self.plan["steps"][0]["timeoutSeconds"] = timeout
            report = self.run_case()
            self.assertEqual(report["status"], "failed")
            self.assertFalse(list((self.root / "cache").glob("*.json")))
        self.assertTrue(self.run_case()["steps"][0]["status"] != "cached")

    def test_glob_membership_and_missing_inputs(self):
        (self.root / "a.dat").write_text("a")
        self.plan["steps"][0]["inputs"].append("*.dat")
        self.assertEqual(self.run_case()["status"], "passed")
        self.assertEqual(self.run_case()["steps"][0]["status"], "cached")
        (self.root / "b.dat").write_text("b")
        self.assertEqual(self.run_case()["steps"][0]["status"], "passed")
        self.plan["steps"][0]["inputs"].append("absent.dat")
        self.assertEqual(self.run_case()["status"], "failed")

    def test_mutation_during_check_refused(self):
        self.source.write_text("from pathlib import Path\nPath(__file__).write_text('pass')")
        self.plan["steps"][0]["outputs"] = []
        self.assertEqual(self.run_case()["status"], "failed")
        self.assertFalse(list((self.root / "cache").glob("*.json")))

    def test_corrupt_or_deleted_receipt_artifacts_force_execution(self):
        report = self.run_case()
        receipt = next((self.root / "cache").glob("*.json"))
        receipt.write_text("not json")
        self.assertEqual(self.run_case()["steps"][0]["status"], "passed")
        report = self.run_case()
        artifact = next(p for p in report["steps"][0]["artifacts"] if p.endswith("result.txt"))
        Path(artifact).unlink()
        self.assertEqual(self.run_case()["steps"][0]["status"], "passed")

    def test_failed_dependency_stops_children_and_publishes_no_partial_cache(self):
        one = self.plan["steps"][0]
        bad = dict(one, id="bad", argv=["{python}", "-c", "raise SystemExit(8)"], dependsOn=["one"])
        later = dict(one, id="later", dependsOn=["bad"])
        self.plan["steps"] = [later, bad, one]
        report = self.run_case()
        self.assertEqual(report["status"], "failed")
        self.assertEqual([s["id"] for s in report["steps"]], ["one", "bad"])
        self.assertFalse(list((self.root / "cache").glob("*.json")))

    def test_python_optimization_cannot_disable_assertions(self):
        self.source.write_text("assert False, 'must fail'")
        with patch.dict(os.environ, {"PYTHONOPTIMIZE": "2"}):
            self.assertEqual(self.run_case()["status"], "failed")

    def test_runtime_mutation_does_not_publish_cache(self):
        with patch("run_port_checks.python_runtime", side_effect=[{"version": 1}, {"version": 2}]):
            self.assertEqual(self.run_case()["status"], "failed")
        self.assertFalse(list((self.root / "cache").glob("*.json")))

    def test_later_step_cannot_mutate_prior_artifacts(self):
        one = self.plan["steps"][0]
        body = ("import sys; from pathlib import Path; "
                "(Path(sys.argv[1]).parent/'one/result.txt').write_text('changed')")
        two = dict(one, id="two", argv=["{python}", "-c", body, "{out}"],
                   outputs=[], dependsOn=["one"])
        self.plan["steps"] = [one, two]
        self.assertEqual(self.run_case()["status"], "failed")
        self.assertFalse(list((self.root / "cache").glob("*.json")))

    def test_dependency_order_cycle_missing_and_cache_review(self):
        one = self.plan["steps"][0]
        two = dict(one, id="two", dependsOn=["one"])
        self.plan["steps"] = [two, one]
        self.assertEqual([s["id"] for s in validate(self.plan)], ["one", "two"])
        report = self.run_case()
        self.assertEqual([s["id"] for s in report["steps"]], ["one", "two"])
        one["dependsOn"] = ["two"]
        with self.assertRaisesRegex(ValueError, "cycle"):
            validate(self.plan)
        one["dependsOn"] = ["missing"]
        with self.assertRaisesRegex(ValueError, "Missing dependency"):
            validate(self.plan)
        del one["dependsOn"]
        one["inputsComplete"] = False
        with self.assertRaisesRegex(ValueError, "inputsComplete"):
            validate(self.plan)


if __name__ == "__main__":
    unittest.main()
