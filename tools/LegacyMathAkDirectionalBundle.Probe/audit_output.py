"""Independent original-geometry checks; no model execution or vendor loading."""
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import sys

R, C, O = map(Path, sys.argv[1:])
assert not O.exists()
ak = (R / 'models/math/math-ak.res').read_bytes()
assert hashlib.sha256(ak).hexdigest() == '8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'

def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module

references = {
    'temporal': load('bundle_temporal', R / 'tools/LegacyMathAkDirectional.Probe/reference_directional.py'),
    'spatial': load('bundle_spatial', R / 'tools/LegacyMathAkDirectionalSpatial.Probe/reference_directional_spatial.py'),
    'upper': load('bundle_upper', R / 'tools/LegacyMathAkDirectionalUpper.Probe/reference_upper.py')
}
cases = [
    ('triangle', [[0,0],[2,1],[4,0]]), ('horizontal', [[0,0],[40,0]]),
    ('vertical', [[0,0],[0,30]]), ('nonsquare', [[0,0],[3,8],[4,2]]),
    ('turning', [[0,0],[5,2],[1,6],[8,3],[2,0]]),
    ('duplicate', [[0,0],[0,0],[2,1],[4,0]]), ('negative', [[8,8],[6,4],[2,3]])
]
expected = [{key: ref.calculate(xy, ak) for key, ref in references.items()} for _, xy in cases]
guards = ['same-profile-foreign-source-refused', 'equal-data-foreign-profile-refused',
          'null-bundle-refused', 'no-classifier-authority', 'pre-cancel-exact-token',
          'single-point-refused', 'degenerate-refused', 'multistroke-refused', 'over4096-refused',
          'shared-profile-after-resource-clear', 'no-public-bundle-constructor', 'bundle-one-frame']
for family in ('temporal', 'spatial', 'upper'):
    guards += ['bundle-one-' + family, 'no-reprepare-' + family]
frame_calls = ['LegacyDirectionalResampling.Prepare', 'LegacyDirectional2B1Landmarks.Extract',
               'LegacyDirectionalIntersections.Extract', 'LegacyDirectionalStatistics.Extract']
guards += ['frame-once-' + call for call in frame_calls]

def check(value):
    assert type(value) is dict and value['guards'] == guards
    assert type(value['modelExecutions']) is int and value['modelExecutions'] == 0
    assert len(value['cases']) == 7
    for row, (name, xy), want in zip(value['cases'], cases, expected):
        assert row['name'] == name and row['original'] == xy
        assert row['HasClassifierOutput'] is False and row['HasCandidateAuthority'] is False
        assert type(row['preparedPointCount']) is int
        for key in references:
            assert type(row[key]) is list and all(type(v) is int for v in row[key])
            assert row[key] == want[key]['values']
            assert row['preparedPointCount'] == want[key]['preparedCount']
    assert value['entryCalls'].count('LegacyMathAkSourceGeometry.PrepareDirectionalFrame') == 1
    for family in ('Temporal', 'Spatial', 'Upper'):
        assert value['entryCalls'].count('LegacyMathAkSourceGeometry.PrepareDirectional' + family + 'Core') == 1
    for call in frame_calls:
        assert value['frameCalls'].count(call) == 1

capture = json.loads(C.read_bytes())
check(capture)
mutations = [
    ('temporal', lambda q: q['cases'][0]['temporal'].__setitem__(100, q['cases'][0]['temporal'][100] ^ 1)),
    ('spatial', lambda q: q['cases'][0]['spatial'].__setitem__(100, q['cases'][0]['spatial'][100] ^ 1)),
    ('upper', lambda q: q['cases'][0]['upper'].__setitem__(100, q['cases'][0]['upper'][100] ^ 1)),
    ('foreign-family', lambda q: q['cases'][0].__setitem__('spatial', q['cases'][0]['temporal'])),
    ('geometry', lambda q: q['cases'][0]['original'][1].__setitem__(0, 99)),
    ('authority', lambda q: q['cases'][0].__setitem__('HasCandidateAuthority', True)),
    ('count', lambda q: q['cases'][0].__setitem__('preparedPointCount', 1)),
    ('guard-order', lambda q: q['guards'].reverse()),
    ('extra-frame', lambda q: q['entryCalls'].append('LegacyMathAkSourceGeometry.PrepareDirectionalFrame')),
    ('extra-intersections', lambda q: q['frameCalls'].append('LegacyDirectionalIntersections.Extract')),
    ('numeric-type', lambda q: q['cases'][0]['upper'].__setitem__(0, 1024.0))
]
rejected = []
for name, mutate in mutations:
    bad = copy.deepcopy(capture)
    mutate(bad)
    try:
        check(bad)
    except AssertionError:
        rejected.append(name)
    else:
        raise AssertionError('Accepted corruption: ' + name)
report = dict(captureSHA256=hashlib.sha256(C.read_bytes()).hexdigest(), cases=7,
              features=7*(279+279+223), guards=len(guards), corruptions=rejected,
              sharedFrameCalls=1, modelExecutions=0,
              scope='Original-geometry feature equality and compiled call edges; not latency or handwriting accuracy')
O.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print('PASS seven bundles/5467 original-geometry features/22 guards/11 corruptions/0NN')
