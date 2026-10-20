"""Independent Math field/tag/selected C23 and source-aware output oracle.

Tracked stdlib/NumPy helpers only. No research imports, expected labels or
vendor execution. Same-platform exact parity is an optional final argument.
"""
from pathlib import Path
import ast, base64, hashlib, importlib.util, json, runpy, struct, sys
import numpy as np

if len(sys.argv) not in (3, 4, 5):
    raise SystemExit('capture.json rawMath2.res [same-platform-other.json] [old-sourcebound.json]')
root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root/'tools/LegacyMathSourcePositions.Probe'))
from reference_positions import f, compose, gather
from reference_features import group_extrema_scale, resample_v1, features26

def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); return module

position = load('position', root/'tools/LegacyInkPosition.Probe/reference_controls.py')
union = load('union', root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py').union

def functions(path, names, namespace):
    tree = ast.parse(path.read_text(encoding='utf-8-sig'))
    body = [node for node in tree.body if isinstance(node, ast.FunctionDef) and node.name in names]
    assert len(body) == len(names)
    exec(compile(ast.Module(body=body, type_ignores=[]), str(path), 'exec'), namespace)

ns = dict(struct=struct)
functions(root/'tools/LegacyMathMeanXOrder.Probe/audit_output.py', {'native_order'}, ns)
ns.update(dict(f=f, np=np, compose=compose, gather=gather, position=position,
               group_extrema_scale=group_extrema_scale, resample_v1=resample_v1))
functions(root/'tools/LegacyMathSourcePositions.Probe/audit_output.py', {'bits', 'expected'}, ns)
data = json.loads(Path(sys.argv[1]).read_bytes()); raw = Path(sys.argv[2]).read_bytes()
assert data['Resource'] == hashlib.sha256(raw).hexdigest() == '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
reader = runpy.run_path(str(root/'tools/export-ink-math-model.py'))
r = reader['Reader'](raw, 106, reader['header'](raw, 90)[3]); assert r.read('H', 8) == [65535]*7+[299]
n = r.read('H')
for increment in (32, 64, 1):
    table = r.read('H', n); n = next(x+increment for x in reversed(table) if x != 65535)
spans = r.read('H', n+1); entries = [r.read('II') for _ in range(spans[-1]+1)]; blob = r.bytes(entries[-1][0])
intervals = {label & 65535: blob[offset:entries[i+1][0]] for i, (offset, label) in enumerate(entries[:-1])}
namespace = 'http://www.myscript.com/ns/myscript/page'

def node(i, kind, name, parent, origin=0, tag=0, text=None, number=None, uri=None, prefix=None):
    return dict(Id=i, Kind=kind, Name=name, Parent=parent, OriginalNode=origin,
                ValueTag=tag, StringValue=text, NumericValue=number, NamespaceUri=uri, NamespacePrefix=prefix)

scaffold = [node(1, 4, '', 0), node(2, 3, 'page', 1), node(3, 2, '', 2, uri=namespace, prefix=''),
            node(4, 0, 'version', 2, tag=7, number=5), node(5, 3, 'content', 2), node(6, 3, 'layout', 2)]
field_nodes = scaffold + [node(7, 3, 'mathField', 5), node(8, 0, 'id', 7, tag=2, text='formula'),
                          node(9, 3, 'input', 7), node(10, 0, 'tag', 9, tag=4, number=0)]
selected_nodes = scaffold + [dict(row, OriginalNode=row['Id']) for row in field_nodes[6:]]

def values(ranges):
    return [[row['First']['Value'], row['Last']['Value']] for row in ranges]

def range_rows(row, expected):
    out = [[] for _ in row['Units']]; previous = 0
    for run in row['Runs']:
        end = run['EndSegmentExclusive']; index = run['PrimaryIndex']
        additional = position.select_ranges(expected['positions'], expected['ends'], expected['originals'], previous, end)
        if index >= 0: out[index] = union(out[index], additional)
        previous = end
    return [[dict(First=a, Last=b) for a, b in ranges] for ranges in out]

feature_error = 0.; expected_first = None
for at, case in enumerate(data['Actual']):
    ink = [[[p['X'], p['Y']] for p in stroke['Points']] for stroke in case['Ink']]
    e = ns['expected'](ink); state = case['Field']; domain = e['originals']
    assert state['Complete'] and state['CanInfer'] and state['InferenceClaimed'] and state['SourcePrepared']
    assert not (state['Failed'] or state['Closed'])
    assert (state['Tag'], state['Next'], state['Category']) == (0, 1, 102)
    assert state['OriginalNodes'] == field_nodes and state['SelectedNodes'] == selected_nodes
    assert [values(state[k]) for k in ['OriginalRanges', 'SelectedRanges', 'C23Ranges']] == [domain]*3
    extra = sum(map(len, ink))*8 + 2*12*64 + 2*len('formula') + 2*len(namespace.encode()) + 4*16 + 2*160
    assert state['PayloadReservation'] == extra
    assert (case['Order'], case['Positions'], case['Separators']) == (e['order'], e['positions'], e['ends'])
    assert case['Order'] == ([0, 1] if not case['Reversed'] else [1, 0])
    want = features26(e['sampled'], 297).ravel()
    got = np.asarray(case['FeatureBits'], dtype=np.int64).astype(np.uint32).view(np.float32)
    error = float(np.max(np.abs(got-want))); feature_error = max(feature_error, error)
    assert np.allclose(got, want, rtol=1e-5, atol=1e-6), error
    assert case['Frames'] == len(e['positions'])
    assert case['PositionReservation'] == extra + sum(e['counts'])*8 + len(e['positions'])*8 + len(ink)*128 + 32
    assert case['Before'] == case['After']; row = case['After']; pool = {}
    for unit, part in zip(row['Units'], row['Rows'], strict=True):
        text = intervals[unit & 65535]
        if text not in pool: pool[text] = len(pool)+1
        assert part == dict(Word=unit, Identity=pool[text], Length=len(text), ScalarBits=0, Flags=0,
                            Metadata=4294967295, Bytes=base64.b64encode(text).decode())
    assert row['Ranges'] == range_rows(row, e)
    expected_outer = [dict(Flags=p['Flags'], Metadata=p['Metadata'], Alternatives=[dict(Identity=p['Identity'],
        ByteCount=p['Length'], ScalarBits=p['ScalarBits'], Bytes=p['Bytes'])], Ranges=row['Ranges'][i]) for i, p in enumerate(row['Rows'])]
    assert case['Outer'] == expected_outer
    ranges = sum(map(len, row['Ranges'])); strings = sum(5*len(x)+1 for x in pool); peak = max(map(lambda x:5*len(x), pool), default=0)
    assert case['OuterReservation'] == case['CopyReservation'] + 96 + len(row['Rows'])*72 + ranges*16 + strings + peak
    assert case['RawSteps'] == len(case['RawHashes']) > 0
    if at == 0: expected_first = row
assert data['Actual'][0]['RawHashes'] == data['Actual'][1]['RawHashes']
assert len(data['Actual']) == 2

domain = values(data['Actual'][0]['Field']['OriginalRanges'])
prepared_modes = ['MissingInput', 'MissingTag', 'MissingRegistry', 'ForeignTag', 'EmptyNull', 'EmptyAllocated', 'Partial']
for mode, case in zip(prepared_modes, data['Prepared'], strict=True):
    assert case['Mode'] == mode
    s = case['Snapshot']; assert s['Complete'] and not any(s[k] for k in ['Failed', 'Closed', 'CanInfer', 'InferenceClaimed', 'SourcePrepared'])
    assert s['Category'] == 102
    if mode == 'Partial': assert values(s['C23Ranges']) == [[3, 5]]
    else: assert values(s['C23Ranges']) == []
    if mode == 'ForeignTag':
        assert next(n['NumericValue'] for n in s['SelectedNodes'] if n['Name'] == 'tag') == 2**32
        assert s['Tag'] == 0 and 'DAF:missing-registry:reported-null' in s['Trace']
    if mode == 'EmptyNull': assert '086C65:equal-ptr/count:no-op' in s['Trace']
    if mode == 'EmptyAllocated': assert '086E26:same-C20-domain-copy' in s['Trace']

stages = ['OriginalScaffold', 'OriginalField', 'InputInserted', 'Registered', 'Tagged',
          'SelectedScaffold', 'SelectedField', 'SelectedRegistry', 'C23Constructed', 'C23Selected', 'Ready']
assert len(data['Failures']) == 22
for index, stage in enumerate(stages):
    for canceled, fault in zip((False, True), data['Failures'][index*2:index*2+2], strict=True):
        assert fault['Stage'] == stage and fault['Canceled'] == canceled
        before = fault['Before']; after = fault['After']; assert after == dict(before, Failed=True)
        assert not any(before[k] for k in ['Failed', 'Closed', 'InferenceClaimed', 'SourcePrepared'])
        assert before['Complete'] == before['CanInfer'] == (index == 10)
        original_count = 6 if index == 0 else 8 if index == 1 else 9 if index < 4 else 10
        selected_count = 0 if index < 5 else 6 if index == 5 else 10
        assert before['OriginalNodes'] == field_nodes[:original_count]
        assert before['SelectedNodes'] == selected_nodes[:selected_count]
        assert (before['Tag'], before['Next']) == ((0, 1) if index >= 3 else (-1, 0))
        assert values(before['OriginalRanges']) == (domain if index >= 3 else [])
        assert values(before['SelectedRanges']) == (domain if index >= 7 else [])
        assert values(before['C23Ranges']) == (domain if index >= 9 else [])
assert data['Gc'] == dict(DetachedAllowsSourceCollection=True, SourceBackedRetainsSource=True, Row=expected_first)
assert data['Guards'] == 69
if len(sys.argv) >= 4: assert Path(sys.argv[1]).read_bytes() == Path(sys.argv[3]).read_bytes()
if len(sys.argv) == 5:
    old = json.loads(Path(sys.argv[4]).read_bytes()); first = next(c for c in old['Cases'] if c['Width'] == 1)
    assert data['Actual'][0]['RawHashes'] == first['RawHashes']
    assert data['Actual'][0]['Before']['Units'] == first['Rows'][0]['Units']
print(f'PASS2 actual Math INPUT102/same-document NN-C79/rawAL/copy/outer;7 prepared C23 paths/22 complete prefix faults/69guards/GC; feature error{feature_error}')
