"""Independent data-only coordinate oracle for the static EF4 caller report.

Does not call transferred numeric helpers, execute an SDK or load ONNX Runtime.
The cases are prepared coordinate controls, not actual completed output.
"""
from collections import Counter
import json
from pathlib import Path
import sys

if len(sys.argv) != 2:
    raise SystemExit('static-report.json')
data = json.loads(Path(sys.argv[1]).read_bytes())
assert data['nativeDefaultEmptyExistingSourceProved'] is False
assert data['replacementLabel'] == 299 and data['separatorLabel'] == 65535
assert data['serializedCounter'] == 297
observable = set(range(2, 262)) | {268, 276, 296}
different_tail_rows = different_tail_values = deleted_positions = 0
for case in data['controls']:
    labels, matrix, segments = case['primary'], case['matrix'], case['segments']
    visible = [index for index, label in enumerate(labels) if label in observable]
    assert case['observableRows'] == visible
    assert case['filtered'] == [matrix[index] for index in visible]
    # Column max over ORIGINAL rows, with an explicit earliest-index tie key.
    before = [max(visible, key=lambda index: (matrix[index][column], -index))
              for column in range(segments)]
    assert before == case['assignmentsBefore']
    uses = Counter(before)
    retained = [index for index, label in enumerate(labels)
                if uses[index] or label not in observable]
    deleted = [int(index not in retained) for index in range(len(labels))]
    assert deleted == case['deletionMask']
    assert case['initialMask'] == [0] * len(labels)
    assert retained == case['retainedOriginalRows']
    remap = [retained.index(index) if index in retained else -1
             for index in range(len(labels))]
    assert remap == case['oldToNew']
    after = [retained.index(index) for index in before]
    assert after == case['assignmentsAfter']
    # The caller supplies ORIGINAL activation + INITIAL zero mask. It does not
    # supply the post-deletion projection, despite the assignments' new basis.
    assert case['tailRows'] == after
    literal = [matrix[index][column] for column, index in enumerate(after)]
    assert literal == case['tailValues']
    intuitive_rows = [retained[index] for index in after]
    intuitive_values = [matrix[index][column] for column, index in enumerate(intuitive_rows)]
    different_tail_rows += int(intuitive_rows != after)
    different_tail_values += int(intuitive_values != literal)
    deleted_positions += sum(deleted)
assert len(data['controls']) == 256
assert different_tail_rows > 0 and different_tail_values > 0 and deleted_positions > 0

for case in data['preservationTruth']:
    # CMP initial,0; JNE false is reached only for a nonzero deletion byte.
    assert case['success'] == (case['deleted'] == 0 or case['initial'] == 0)
assert len(data['preservationTruth']) == 16
for case in data['sourceCases']:
    match = (case['kind'], case['typeId']) == (4, 0xc25)
    assert case['entersC38'] == match and case['emptyFastReturn'] != match
assert len(data['sourceCases']) == 16
print(f'PASS256 prepared coordinate cases/{deleted_positions} unused original units; '
      f'{different_tail_rows} row-basis and{different_tail_values} numeric counterexamples; '
      '16 insertion-preservation truth cases/16 source-selection cases; native empty-source producer unresolved')
