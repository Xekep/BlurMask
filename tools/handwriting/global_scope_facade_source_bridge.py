"""Strict additive SOURCE contract bridge, no C#/SDK/model execution.

Historical baseline is retained. Current935 is accepted only with exact source
identity, whole-file inverse, unchanged selected-context preparation and direct
current closed-owner/preflight/residue/single-use checks. No old tree is written.
"""
from pathlib import Path
import hashlib,json
LEGACY_SCOPE_LF_SHA='c92eb81a1702fa6ed0c262989f993f4350a5ddbb4cb5e999983f8404463167d5'
PINS_SHA='946bbf50079fe19daecfbf0178cb3f13be356eee53f5a82deda45ccde7f82e0c'
def _sha(text):return hashlib.sha256(text.encode('utf-8')).hexdigest()
def _body(text,anchor):
    start=text.index(anchor);opening=text.index('{',start);depth=1;end=opening+1
    while depth:
        depth+=(text[end]=='{')-(text[end]=='}');end+=1
    return text[start:end]
def _inverse_current(scope,pins):
    restored=scope
    for edit in pins['inverseEdits'][::-1]:
        assert restored.count(edit['new'])==1,'Current Scope inverse hunk missing or ambiguous'
        restored=restored.replace(edit['new'],edit['old'],1)
    assert _sha(restored)==LEGACY_SCOPE_LF_SHA,'Scope inverse must equal the original baseline exactly'
    return restored
def _verify_current(scope,pins,support):
    assert _sha(scope)==pins['candidateLfSha256'],'Unreviewed current Scope body'
    restored=_inverse_current(scope,pins)
    anchor='    public static LegacyLibraryGlobalTextPublicationScope PrepareOriginalSelected('
    assert _body(scope,anchor)==_body(restored,anchor),'Original selected/context bootstrap changed'
    assert _sha(_body(scope,anchor))==pins['prepareOriginalSelectedLfSha256']
    for name,expected in pins['supportLfSha256'].items():
        assert _sha(support[name])==expected,'Unreviewed support body: '+name
    texts={'LegacyLibraryGlobalTextPublicationScope.cs':scope,**support};count=0
    for name,terms in pins['guards'].items():
        for term in terms:
            assert term in texts[name],(name,'Current direct guard missing',term);count+=1
    complete=_body(scope,'    public LegacyFreshMainGlobalSourceCompletion CompleteSources(')
    preentry=['Validate();token.ThrowIfCancellationRequested();','if(allocation400.Length!=400',
              'if(result is null||!ReferenceEquals(result.Owner.SourceDocument,Document)',
              'if(enteredCompletion)throw','enteredCompletion=true;']
    offsets=[complete.index(term) for term in preentry];assert offsets==sorted(offsets)
    # Scope lock closes before the cancellable session preflight/continuation.
    released=complete.index('enteredCompletion=true;\n        }')
    prepared=complete.index('var completion=LegacyFreshMainGlobalSourceCompletion.Prepare')
    installed=complete.index('lock(gate){Validate();SourceCompletion=completion;}')
    continued=complete.index('completion.Run(completionResidue!')
    bound=complete.index('shared.BindActualGlobalTop(result!.Session!.Ownership)')
    assert released<prepared<installed<continued<bound
    return dict(Branch='ReviewedActual935Facade',ObservedScopeLfSha256=_sha(scope),
                InverseLegacyLfSha256=_sha(restored),InverseHunks=len(pins['inverseEdits']),
                OriginalSelectedPreparationExact=True,CurrentGuardPins=count,
                SupportLfSha256={name:_sha(text) for name,text in support.items()},
                Authority='Actual current source read in place; inverse only in memory. No native numerical, padding knownness, model/quality or general concurrent queue claim.')
def verify_scope_source(root,expected_legacy_hash):
    assert expected_legacy_hash==LEGACY_SCOPE_LF_SHA,'Original raw-kind Scope assertion must remain pinned'
    from publication_compile_default_source import inspect_default_cohort,read_default_text
    root=Path(root);compileDefault=inspect_default_cohort(root)
    scope=(root/'src/InkRecognition/LegacyLibraryGlobalTextPublicationScope.cs').read_text(encoding='utf-8')
    if _sha(scope)==LEGACY_SCOPE_LF_SHA:
        return dict(Branch='HistoricalExactBaseline',ObservedScopeLfSha256=LEGACY_SCOPE_LF_SHA,
                    NewFacadeProof=False,Authority='Original baseline only; no current facade claim')
    raw=Path(__file__).with_name('global-scope-facade-source-bridge-pins.json').read_bytes()
    assert hashlib.sha256(raw).hexdigest()==PINS_SHA,'Bridge proof metadata changed'
    pins=json.loads(raw);assert pins['legacyLfSha256']==LEGACY_SCOPE_LF_SHA
    support={};projections=[]
    for name in pins['supportLfSha256']:
        if name=='LegacyFreshMainGlobalPlainRecognition.cs':
            support[name],receipt=read_default_text(root,name);projections.append(receipt)
        else:support[name]=(root/'src/InkRecognition'/name).read_text(encoding='utf-8')
    receipt=_verify_current(scope,pins,support);receipt['CompileDefaultSources']=projections
    return receipt
