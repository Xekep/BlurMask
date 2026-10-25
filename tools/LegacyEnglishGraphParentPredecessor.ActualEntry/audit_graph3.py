"""Original graph3 owning completion/structure; not a score-byte or quality oracle."""
from pathlib import Path
import argparse, copy, hashlib, json

def load(path):
    def unique(rows):
        result = {}
        for key, value in rows:
            assert key not in result
            result[key] = value
        return result
    return json.loads(path.read_bytes(), object_pairs_hook=unique,
        parse_constant=lambda value: (_ for _ in ()).throw(AssertionError(value)))

def audit(d):
    assert set(d) == {'Scope','Completed','InputSha','OriginalIndex','RawPoints',
        'RawStrokes','Resolution','GraphNodes','ParentChildren','Stages','PrefixScope',
        'Metrics','GraphNode','NoExpectedLabels','Admission','GlobalReadinessGateUnchanged','TimeoutMs'}
    assert d['Scope'] == 'ACTUAL_OWNING_ENGLISH_GENERIC_LOOP_FIRST_LANGUAGE_PHASE1_NOT_FINAL_WORD'
    assert d['PrefixScope'] == 'ONE_OWNING_GENERIC_NODE_SPAN_LOOP_FIRST_ONLY_NOT_FINAL_WORD'
    assert d['Completed'] is True and d['NoExpectedLabels'] is True
    assert d['Admission'] == 'NONE' and d['GlobalReadinessGateUnchanged'] is True
    assert d['InputSha'] == 'd1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99'
    for key, value in dict(OriginalIndex=6,RawPoints=4,RawStrokes=1,GraphNodes=3,ParentChildren=1,TimeoutMs=60000).items():
        assert type(d[key]) is int and d[key] == value
    assert type(d['Resolution']) in (int,float) and d['Resolution'] == 96
    assert d['Stages'] == ['source-load','parent-prepare','parent-FIRST','parent-post',
        'main-local','node1-prepare-model-index','owning-generic-graph-run','capture']
    m = d['Metrics']
    assert set(m) == {'Nodes','CompletedNodes','QueryRows','QueryPages','ScoringCalls','AcceptedAllocations','SelectedD0Rows'}
    assert all(type(value) is int for value in m.values())
    assert m['Nodes'] == m['CompletedNodes'] == 3
    # Independent stored native geometry has six arcs; node0 consumes one.
    assert m['SelectedD0Rows'] == 5
    assert all(0 < m[key] <= 1048576 for key in ['QueryRows','QueryPages','ScoringCalls'])
    assert 0 < m['AcceptedAllocations'] <= m['ScoringCalls']
    g = d['GraphNode']
    sizes = dict(Rows156=156*88,RowMasks156=156*88,PrimaryAlternatives80=125*80,
        PrimaryAlternativeMasks=125*80,Backing250=250*128,BackingMasks250=250*128)
    assert set(g) == set(sizes) | {'Node','SourceEnd','RetainedEnd','OriginalT','InitializedN','Ordinals156'}
    for key in ['Node','SourceEnd','RetainedEnd','OriginalT','InitializedN']:
        assert type(g[key]) is int
    assert g['Node'] == 2 and g['OriginalT'] == 250
    assert 125 <= g['RetainedEnd'] <= g['SourceEnd'] <= 156
    assert 0 <= g['InitializedN'] <= 250
    for key, size in sizes.items():
        text = g[key]
        assert type(text) is str and len(text) == 2*size
        raw = bytes.fromhex(text)
        assert raw.hex() == text
        if 'Mask' in key:
            assert set(raw) <= {0,255}
    assert type(g['Ordinals156']) is list and len(g['Ordinals156']) == 156
    assert all(type(value) is int and 0 <= value < 250 for value in g['Ordinals156'])
    return dict(PASS=True,graphNodes=3,selectedNoninitialD0Rows=5,metrics=m,
        scope='Owning completion and fixed-buffer structure only; no final-word, numeric-score parity or accuracy claim')

if __name__ == '__main__':
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('capture',type=Path);p.add_argument('report',type=Path)
    p.add_argument('--selftest',action='store_true');a=p.parse_args()
    assert not a.report.exists() and a.capture.stat().st_size <= 16*1024*1024
    d=load(a.capture);r=audit(d)
    if a.selftest:
        changes=[lambda b:b.update(Completed=False),lambda b:b.update(GraphNodes=2),
            lambda b:b['Metrics'].update(CompletedNodes=2),lambda b:b['Metrics'].update(SelectedD0Rows=4),
            lambda b:b['Metrics'].update(ScoringCalls=True),lambda b:b.update(NoExpectedLabels=False),
            lambda b:b['GraphNode'].update(Node=1),lambda b:b['GraphNode'].update(SourceEnd=157),
            lambda b:b['GraphNode'].update(Rows156=b['GraphNode']['Rows156'][:-2]),
            lambda b:b['GraphNode'].update(RowMasks156='7e'+b['GraphNode']['RowMasks156'][2:])]
        for change in changes:
            bad=copy.deepcopy(d);change(bad)
            try:audit(bad)
            except AssertionError:pass
            else:raise AssertionError('Corrupt or incomplete graph accepted')
        r['rejectedMutations']=len(changes)
    r['captureSHA256']=hashlib.sha256(a.capture.read_bytes()).hexdigest()
    a.report.write_text(json.dumps(r,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(r))
