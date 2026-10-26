"""Narrow compile-default SOURCE reader; never a compiled-mode or owner issuer.
Only two source bodies consumed by the reviewed strict16 closure may be projected.
"""
from pathlib import Path
import hashlib,runpy
CONTRACT_SHA='fc5cf8632da0e9070557355e78012bfa85393a94ee6c471cb33c37a2aa090144'
SYMBOL=b'BLURMASK_PUBLICATION_DIAGNOSTICS'
CORE=('LegacyFreshMainGlobalPlainRecognition.cs','LegacyFreshMainGlobalWordAugmentation.cs',
      'LegacyFreshMainGlobalWordTextPublication.cs','LegacyNativeGlyphRecordPool.Cleanup.cs',
      'LegacyNativePlainWordPublisher.Augmentation.cs','LegacyNativePlainWordPublisher.cs')
# Exact whole-source historical/current LF bodies; these do not replace existing cohort/guard checks.
UNCONDITIONED_LF={
 'LegacyFreshMainGlobalPlainRecognition.cs':frozenset((
  'e5fcb64fb1a95531814e60cf675698e7caef3fb2609abe0a64040c85527bed43',
  '7f369aa1f148889025e79fdeb3711b1c91c8ce4634142045742a297ee8f82029')),
 'LegacyFreshMainGlobalWordTextPublication.cs':frozenset((
  'fe771f03179129050f49897a30fc8be21aa4007750563d775c366b6b44d48208',
  'ce65071e363bb958454c0ca6945d9e52c1ca171995592fdbe19d59e0f8af6031',
  '5d1f8d47de5affa7dfba52a57a03e2a2afda75105431d60bddb246ad3021cd29'))}
def _sha(raw):return hashlib.sha256(raw).hexdigest()
def _text(raw):return raw.decode('utf-8-sig').replace('\r\n','\n').replace('\r','\n')
def inspect_default_cohort(root):
    root=Path(root);present={}
    for name in CORE:
        path=root/'src/InkRecognition'/name
        if path.is_file():present[name]=path.read_bytes()
    conditional=any(SYMBOL in raw or any(line.lstrip().startswith((b'#if',b'#elif',b'#else',b'#endif',b'#define',b'#undef')) for line in raw.splitlines()) for raw in present.values())
    if not conditional:return dict(Mode='UnconditionedSource',FlavorProof=False)
    # Any conditional member requires the ENTIRE exact flavor/project/tool/meterV2 cohort.
    helper=root/'tools/handwriting/publication_diagnostic_flavor_source_contract.py'
    raw=helper.read_bytes();assert _sha(raw)==CONTRACT_SHA,'Unknown compile-default source contract'
    contract=runpy.run_path(str(helper));proof=contract['verify'](root,require_meter_v2=True)
    scope=(root/'src/InkRecognition/LegacyLibraryGlobalTextPublicationScope.cs').read_bytes()
    assert _sha(_text(scope).encode('utf-8'))=='2bf46179d723cfb34a1c72f219b119625fead32bf240d831ee8f72de15b86673','Conditional935 flavor requires exact reviewed current Scope'
    assert helper.read_bytes()==raw,'Source contract changed during verification'
    return dict(Mode='CompileDefaultSymbolFalse',FlavorProof=True,ContractSha256=CONTRACT_SHA,
                Cohort=proof,RawCoreSha256={name:_sha(raw) for name,raw in present.items()})
def read_default_text(root,name):
    assert name in UNCONDITIONED_LF,'Only two reviewed conditional readers allowed'
    root=Path(root);state=inspect_default_cohort(root) # Never trusts a caller-supplied receipt/cache.
    path=root/'src/InkRecognition'/name;raw=path.read_bytes();observed=_sha(raw)
    if state['FlavorProof']:
        assert observed==state['RawCoreSha256'][name],'Conditional source changed after authentication'
        helper=root/'tools/handwriting/publication_diagnostic_flavor_source_contract.py'
        helper_raw=helper.read_bytes();assert _sha(helper_raw)==CONTRACT_SHA
        contract=runpy.run_path(str(helper));projected=contract['preprocess'](raw,False)
        # verify() already authenticated this exact member's raw projected SHA/length.
        assert helper.read_bytes()==helper_raw and path.read_bytes()==raw,'Source changed during projection'
    else:projected=raw
    text=_text(projected);lf=_sha(text.encode('utf-8'))
    assert lf in UNCONDITIONED_LF[name],'Unknown whole default/historical source body'
    return text,dict(Mode=state['Mode'],Source=name,ObservedRawSha256=observed,
                    ProjectedRawSha256=_sha(projected),ProjectedLfSha256=lf,
                    ConditionalCohort=state if state['FlavorProof'] else None,
                    CompiledFlavorClaim=False,HistoricalOrCurrentGuardsStillRequired=True)
