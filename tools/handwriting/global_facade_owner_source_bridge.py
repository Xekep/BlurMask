"""Strict current935 owner SOURCE bridge; no target/model/native execution."""
from pathlib import Path
import hashlib,json
PINS_SHA='eab868d8a30484aa1dbaa5774fca170dcf5f6cf30174a465d583b5a0b460f3d3'
LEGACY_PUBLISH_SHA='08ec74d0d054cf61203db5e36a8cb755043bffd9a964c073b7a574d54bab7ccd'
BASELINE_COHORT_SHA={'LegacyFreshMainGlobalGlyphUtf8Emission.cs': '9d14fde88eecd8606f0c6f7269e831244ca748d33f56dacd3dd6d6e8a12ac354', 'LegacyFreshMainGlobalTextGlyphChoices.cs': '0b21a980113ea33fd049b12ce831e495f10ef995d22b80e13789870535d72cb6', 'LegacyFreshMainGlobalGlyphDocumentPositions.cs': '23f89edce18a479416b7e93823ed31d10d0faf78ffe00eb7898ee9fd7f3904e2', 'LegacyFreshMainGlobalTextSourceWrapper.cs': '834765cd9935bbd7cbe291e479e529343974b6bb0578d7da2fb82ee0b39b04b7', 'LegacyFreshMainGlobalOrdinaryGlyphRefresh.cs': '45c40ac3a9d4ebe46c53307ad65eb2256cc3b7a520eed6eff03521bf5ec923cd'}
CURRENT_COHORT_SHA={'LegacyFreshMainGlobalGlyphUtf8Emission.cs': 'bec9b294c4684dcdbb662029e39981403dda131c12a5c06fe0fd403954578a06', 'LegacyFreshMainGlobalTextGlyphChoices.cs': 'cf4daa3c5cf7bf8e175bb084d7610e4e3be8e59b0d9c68591ad48a3fea81acbe', 'LegacyFreshMainGlobalGlyphDocumentPositions.cs': 'b1b708cbe9edb197723564cd6973c7c77e468234fb23245c493b7658541b88be', 'LegacyFreshMainGlobalTextSourceWrapper.cs': '0d254c3fdc41a946f4be31ee7b8465b637ca9702e59b89238696c1ec0950f8a4', 'LegacyFreshMainGlobalOrdinaryGlyphRefresh.cs': '806bb92a84bcfce2cf8334eb2d4418fe8f67348f8dfcdddda7454164657e91b5'}
LEGACY_SCOPE_SHA='c92eb81a1702fa6ed0c262989f993f4350a5ddbb4cb5e999983f8404463167d5'
PARTIAL_SCOPE_SHA='6b0c09df62c29958738b5b0f19eb3dbef0ddb5c5b2e8ee723a796dc8e6645ca6'
CURRENT_SCOPE_SHA='2bf46179d723cfb34a1c72f219b119625fead32bf240d831ee8f72de15b86673'

def _sha(t):return hashlib.sha256(t.encode('utf-8')).hexdigest()
def _body(t,anchor):
    start=t.index('{',t.index(anchor));depth=1;end=start+1
    while depth:depth+=(t[end]=='{')-(t[end]=='}');end+=1
    return t[start:end]
def _inverse_file(text,entry):
    restored=text
    for edit in entry['inverseEdits'][::-1]:
        assert restored.count(edit['new'])==1,'Missing/ambiguous current source inverse'
        restored=restored.replace(edit['new'],edit['old'],1)
    assert _sha(restored)==entry['main930LfSha256'],'Whole source inverse differs from exact main930'
    return restored
def _verify_sources(texts,pins):
    restored={}
    for name,entry in pins['files'].items():
        assert _sha(texts[name])==entry['candidateLfSha256'],('Unknown current source',name)
        restored[name]=_inverse_file(texts[name],entry)
    count=0
    for name,terms in pins['guards'].items():
        for term in terms:assert term in texts[name],('Missing current direct owner guard',name,term);count+=1
    choice='LegacyFreshMainGlobalTextGlyphChoices.cs';publish=_body(texts[choice],pins['publishAnchor'])
    assert _sha(publish)==pins['currentPublishLfSha256']
    guard=pins['batchRefusalLine'];assert publish.count(guard)==1
    assert publish.index('Validate(token);')<publish.index(guard.strip())<publish.index('if(allocatedRecordStorage40.Length')<publish.index('entered=true;')
    old_publish=publish.replace(guard,'',1).replace('ActualCompletion','Completion')
    assert _sha(old_publish)==LEGACY_PUBLISH_SHA==pins['legacyPublishLfSha256'],'Only owner alias and added batch refusal may differ from original numeric Publish'
    # Actual projection and emission numeric bodies have NO change at the935 seam.
    for name,anchor in [('LegacyFreshMainGlobalGlyphDocumentPositions.cs','internal LegacyFreshMainGlobalGlyphDocumentPositions Project('),
                        ('LegacyFreshMainGlobalGlyphUtf8Emission.cs','public byte[] Emit(')]:
        assert _body(texts[name],anchor)==_body(restored[name],anchor),'Numerical projection/emission body changed'
    return dict(Branch='ReviewedActual935OwnerRoutes',CurrentGuardPins=count,
                ActualSourceLfSha256={n:_sha(t) for n,t in texts.items()},
                Main930InverseLfSha256={n:_sha(t) for n,t in restored.items()},
                LegacyPublishInverseSha256=_sha(old_publish),
                PublishDelta='Existing typed binding alias plus pre-entry batch refusal only; no selection/NN/emission arithmetic change',
                ProjectionEmissionBodiesExactAt935Seam=True,
                Authority='Read actual current source; inverse stays in memory. Main930 inverse is distinct from older original08 numeric method.')
def _cohort_mode(scope,texts,pins):
    # Constants independently bind metadata and full-file dispatch; old literal
    # occurrence (including a comment) is NEVER an authority to choose a mode.
    assert {n:e['main930LfSha256'] for n,e in pins['files'].items()}==BASELINE_COHORT_SHA
    assert {n:e['candidateLfSha256'] for n,e in pins['files'].items()}==CURRENT_COHORT_SHA
    actual={n:_sha(t) for n,t in texts.items()};scope_sha=_sha(scope)
    if actual==BASELINE_COHORT_SHA:
        if scope_sha==LEGACY_SCOPE_SHA:return 'HistoricalMain930'
        assert scope_sha==PARTIAL_SCOPE_SHA,'Unknown historical Scope'
        before='public sealed partial class LegacyLibraryGlobalTextPublicationScope : IDisposable'
        after='public sealed class LegacyLibraryGlobalTextPublicationScope : IDisposable'
        assert scope.count(before)==1 and _sha(scope.replace(before,after,1))==LEGACY_SCOPE_SHA
        return 'HistoricalMain930'
    assert actual==CURRENT_COHORT_SHA,'Unknown/mixed owner source cohort'
    assert scope_sha==CURRENT_SCOPE_SHA,'Current owner cohort requires exact current Scope'
    return 'Current935'
def authenticate_owner_cohort(root):
    raw=Path(__file__).with_name('global-facade-owner-source-bridge-pins.json').read_bytes()
    assert hashlib.sha256(raw).hexdigest()==PINS_SHA,'Owner proof metadata changed'
    pins=json.loads(raw);root=Path(root)
    scope=(root/'src/InkRecognition/LegacyLibraryGlobalTextPublicationScope.cs').read_text(encoding='utf-8')
    texts={n:(root/'src/InkRecognition'/n).read_text(encoding='utf-8') for n in pins['files']}
    mode=_cohort_mode(scope,texts,pins)
    if mode=='HistoricalMain930':
        publish=_body(texts['LegacyFreshMainGlobalTextGlyphChoices.cs'],pins['publishAnchor'])
        assert publish.count(pins['batchRefusalLine'])==1
        legacy=publish.replace(pins['batchRefusalLine'],'',1).replace('ActualCompletion','Completion')
        assert _sha(legacy)==LEGACY_PUBLISH_SHA
        return dict(Branch='HistoricalMain930',NewFacadeProof=False,
                    ActualSourceLfSha256={n:_sha(t) for n,t in texts.items()},ObservedScopeLfSha256=_sha(scope),
                    HistoricalScopeInverseSha256=LEGACY_SCOPE_SHA,LegacyPublishInverseSha256=_sha(legacy),
                    Authority='Exact five-file main930 baseline plus c92 or partial-only inverse; no current facade claim')
    from global_scope_facade_source_bridge import verify_scope_source,LEGACY_SCOPE_LF_SHA
    scope_receipt=verify_scope_source(root,LEGACY_SCOPE_LF_SHA)
    assert scope_receipt['Branch']=='ReviewedActual935Facade'
    receipt=_verify_sources(texts,pins);receipt['ScopeSourceBridge']=scope_receipt;receipt['NewFacadeProof']=True;return receipt
def verify_owner_routes(root):
    # Compatibility name; every call now authenticates both complete alternatives.
    return authenticate_owner_cohort(root)
