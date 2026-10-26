"""Current paired-parent DATA admission for static timed closures.
No C#/SDK/NN evaluator is invoked. Existing independent raw/CHAR/WORD oracle
is retained byte-exact; this receipt is not runtime provenance authority.
"""
from pathlib import Path
import hashlib,json,runpy,sys
PINS_SHA='d0112f53142becd48801c7d6da66504c7e24a20b05382550994f7f3bfaf61e18'
def _sha(data):return hashlib.sha256(data).hexdigest()
def _pair_mode(models,child):
    if (models is None)!=(child is None):raise ValueError('Both models and child are required for current paired mode')
    return models is not None
def _sources(root,pins):
    observed={}
    for rel,expected in pins['OracleSourceSha256'].items():
        data=(Path(root)/rel).read_bytes();assert _sha(data)==expected,('Independent oracle source changed',rel);observed[rel]=expected
    return observed
def verify_current_parent(root,models,child,parent):
    assert _pair_mode(models,child),'Explicit current pair required'
    raw=Path(__file__).with_name('global-current-parent-data-bridge-pins.json').read_bytes()
    assert _sha(raw)==PINS_SHA,'Current-pair proof metadata changed'
    pins=json.loads(raw);root=Path(root);models=Path(models);child=Path(child);parent=Path(parent)
    sources=_sources(root,pins)
    # Admit only the actually reviewed current source route, unconditionally.
    from global_wordtext_source_bridge import authenticate_word_text_cohort
    source=authenticate_word_text_cohort(root);assert source['Branch']=='ReviewedActual935WordText'
    for path in (child,parent):assert path.is_file() and 0<path.stat().st_size<=32*1024*1024,'Bounded genuine paired DATA required'
    child_raw=child.read_bytes();parent_raw=parent.read_bytes();resource_before={}
    for rel,expected in pins['ModelResourcePins'].items():
        data=(models/rel).read_bytes();assert len(data)==expected['bytes'] and _sha(data)==expected['sha256'],rel
        resource_before[rel]=expected['sha256']
    oracle=root/'tools/LegacyGlobalWordText.Probe/audit_output.py';saved=sys.argv
    try:
        sys.argv=[str(oracle),str(models),str(child),str(parent)]
        runpy.run_path(str(oracle),run_name='__main__')
    finally:sys.argv=saved
    assert child.read_bytes()==child_raw and parent.read_bytes()==parent_raw,'Pair inputs changed during oracle'
    assert _sources(root,pins)==sources
    for rel,expected in pins['ModelResourcePins'].items():
        data=(models/rel).read_bytes();assert len(data)==expected['bytes'] and _sha(data)==expected['sha256'],'Model bytes changed during metadata oracle'
    return dict(Mode='CurrentModelsChildParent',ParentSha256=_sha(parent_raw),ChildSha256=_sha(child_raw),
                IndependentOracle='Exact unchanged raw AL/V140/CHAR then WORD/TEXT/guide/commands/prefix oracle',
                OracleSourceSha256=sources,ModelResourceSha256=resource_before,CurrentSourceBranch=source['Branch'],
                InputsUnchanged=True,HistoricalHashBranchRetained=True,NoRuntimeProvenanceClaim=True,
                NativeOrCSharpExecuted=False,NeuralForwardEvaluations=0,QualityClaim=False)
