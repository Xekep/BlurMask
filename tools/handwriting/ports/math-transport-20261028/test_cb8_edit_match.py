"""Prepared independent branch expectations from032800 instruction addresses.

No SDK engine or reconstructed owner is run. The trace records required service
calls; it distinguishes false from a result obtained after forbidden later work.
"""
import unittest

from cb8_edit_match import Pending, matches


class Adapter:
    def __init__(self, *, old_id=41, new_id=-2, old_range=(1, 2), old_name="MATH_SYMBOL"):
        self.old_id, self.new_id = old_id, new_id
        self.old_range, self.old_name = old_range, old_name
        self.trace = []

    def compare_name(self, left, right):
        self.trace.append("compare:" + str(left) + ":" + str(right))
        return 0 if left == right else 1

    def attribute_text(self, value):
        self.trace.append("convert:" + value)
        return value

    def parse_tag(self, text):
        self.trace.append("parse:" + text)
        return self.old_id if text == "old" else self.new_id

    def ink_range(self, selection_id):
        self.trace.append("range:" + str(selection_id))
        return self.old_range

    def ink_name(self, selection_id):
        self.trace.append("name:" + str(selection_id))
        return self.old_name

    def range_count(self, source_range):
        self.trace.append("count")
        return len(source_range)

    def ranges_equal(self, left, right):
        self.trace.append("equal")
        return left == right


class EditMatchTests(unittest.TestCase):
    def check(self, result, suffix, *, kind=7, name="tag", entries=None, **kwargs):
        adapter = Adapter(**kwargs)
        if entries is None:
            entries = [Pending(2, "MATH_SYMBOL", (1, 2))]
        actual = matches(kind, name, "tag", "old", "new", entries, adapter)
        self.assertEqual(actual, result)
        self.assertEqual(adapter.trace, suffix)

    def test_independent_disassembly_branch_cases(self):
        #32850 kind,3288A name compare,328B4/328DE BOTH conversions,
        #3291A old parse,32956 new parse; no implicit old-ID sign gate.
        prefix = ["compare:tag:tag", "convert:old", "convert:new"]
        parsed = prefix + ["parse:old", "parse:new"]
        lookup = parsed + ["range:41", "name:41", "compare:MATH_SYMBOL:MATH_SYMBOL"]
        cases = [
            (False, [], dict(kind=6)),
            (False, [], dict(name=None)),
            (False, ["compare:range:tag"], dict(name="range")),
            (False, prefix + ["parse:old"], dict(old_id=None)),
            (False, parsed, dict(new_id=None)),
            (False, parsed, dict(new_id=0)),
            (False, parsed, dict(new_id=2)),
            (False, parsed, dict(new_id=-99)),
            (False, parsed, dict(entries=[])),
            (False, parsed + ["range:41"], dict(old_range=None)),
            (False, parsed + ["range:41", "name:41"], dict(old_name=None)),
            (False, parsed + ["range:41", "name:41", "compare:OTHER:MATH_SYMBOL"], dict(old_name="OTHER")),
            (True, lookup + ["equal"], dict()),
            (False, lookup + ["equal"], dict(old_range=(2, 1))),
            #32A50 null pending C22 tests actual old count, without equality.
            (True, lookup + ["count"], dict(entries=[Pending(2, "MATH_SYMBOL", None)], old_range=())),
            (False, lookup + ["count"], dict(entries=[Pending(2, "MATH_SYMBOL", None)])),
            #32980 list search picks FIRST matching ID, never a later good one.
            (False, lookup + ["equal"], dict(entries=[Pending(2, "MATH_SYMBOL", (9,)), Pending(2, "MATH_SYMBOL", (1, 2))])),
            (True, lookup + ["equal"], dict(entries=[Pending(8, "OTHER", None), Pending(2, "MATH_SYMBOL", (1, 2))])),
        ]
        for index, (result, trace, kwargs) in enumerate(cases):
            with self.subTest(case=index):
                self.check(result, trace, **kwargs)

    def test_old_id_sign_is_not_filtered(self):
        adapter = Adapter(old_id=-7)
        self.assertTrue(matches(7, "tag", "tag", "old", "new", [Pending(2, "MATH_SYMBOL", (1, 2))], adapter))
        self.assertIn("range:-7", adapter.trace)

    def test_negation_has_native_signed64_wrap(self):
        adapter = Adapter(new_id=-(1 << 63))
        self.assertTrue(matches(7, "tag", "tag", "old", "new", [Pending(-(1 << 63), "MATH_SYMBOL", (1, 2))], adapter))

    def test_callback_does_not_mutate_pending(self):
        entries = [Pending(2, "MATH_SYMBOL", (1, 2))]
        before = entries.copy()
        self.assertTrue(matches(7, "tag", "tag", "old", "new", entries, Adapter()))
        self.assertEqual(entries, before)


if __name__ == "__main__":
    unittest.main()
