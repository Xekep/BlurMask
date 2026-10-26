"""Strict current WORD/TEXT SOURCE bridge; no target/model/native execution."""
from pathlib import Path
import hashlib,json
PINS_SHA='fc5da1e6d3832120a3f2a5e725b149821e6d8387be27c32b932a7532acffbe80'
HISTORICAL_WORD_COHORT_SHA={'LegacyFreshMainGlobalWordTextPublication.cs': 'fe771f03179129050f49897a30fc8be21aa4007750563d775c366b6b44d48208', 'LegacyFreshMainGlobalTextSourceWrapper.cs': '11c0cd659edc195326e94c109bb5e81a557cd761babbbb0c4cfe5acdc8825ecb', 'LegacyOwnedMainTextSourceInterner.cs': '397f7e9acc8723e4f88e4f4e950850fb321676e4c27bec349743fd732a7b3902', 'LegacyOwnedFreshTextSourceContext.GlobalWordText.cs': '098dcbb7586af93b8218aa59992f91529b4439e5ab11cb7416e6f35966afbd8b', 'LegacyFreshMainGlobalWordSourceReceipt.cs': '6d5e5c92bfb7d478bea038990c06e34f48a9db5c29288887b7c29c0fa36cdb91'}
CURRENT_WORD_COHORT_SHA={'LegacyFreshMainGlobalWordTextPublication.cs': '5d1f8d47de5affa7dfba52a57a03e2a2afda75105431d60bddb246ad3021cd29', 'LegacyFreshMainGlobalTextSourceWrapper.cs': '0d254c3fdc41a946f4be31ee7b8465b637ca9702e59b89238696c1ec0950f8a4', 'LegacyOwnedMainTextSourceInterner.cs': 'e2ead5311b9f52bd49ccb68ec2d39fbc3027af7240425cae21d7d0ab9a2d9ed3', 'LegacyOwnedFreshTextSourceContext.GlobalWordText.cs': '098dcbb7586af93b8218aa59992f91529b4439e5ab11cb7416e6f35966afbd8b', 'LegacyFreshMainGlobalWordSourceReceipt.cs': 'db6adf45b23a9ca3d05b3b6d2e63e98fb50fc614d8c9b039b8eb407b1c78c5df'}
def _sha(t):return hashlib.sha256(t.encode('utf-8')).hexdigest()
def _body(t,anchor):
    start=t.index('{',t.index(anchor));d=1;end=start+1
    while d:d+=(t[end]=='{')-(t[end]=='}');end+=1
    return t[start:end]
def _verify_sources(texts,pins):
    inverse={};count=0
    for name,entry in pins['files'].items():
        text=texts[name];assert _sha(text)==entry['candidateLfSha256'],('Unknown current WORD source',name)
        old=text
        for edit in entry['inverseEdits'][::-1]:
            assert old.count(edit['new'])==1;old=old.replace(edit['new'],edit['old'],1)
        assert _sha(old)==entry['main930LfSha256'];inverse[name]=_sha(old)
    for name,terms in pins['guards'].items():
        for term in terms:assert term in texts[name],('Missing direct WORD guard',name,term);count+=1
    parent=texts['LegacyFreshMainGlobalWordTextPublication.cs']
    outer=_body(parent,'private LegacyFreshMainGlobalWordTextPublication PublishCore(')
    called=_body(parent,'private void PublishCurrentWord(')
    outer_order=['documentChoiceAllocated=true','if(batchWords is null)','PublishCurrentWord(allocator40,selection4,fault,token)',
                 'publishedWords.Add(payload!);publishedSelectedIndices.Add(selectedWordIndex);documentLink=true;',
                 'stage="source-lines"','LegacyNativeSourceLinePublication.Append(lines','text=new(this,textKey)','CreateGlobalTextSelection(text']
    offsets=[outer.index(s) for s in outer_order];assert offsets==sorted(offsets)
    called_order=['stage="word-intern";var node=','payload=new(OriginalWordSource,node.Node)',
                  'if(allWords is null)payload.ChoiceCount=1;', 'stage="characters";',
                  'characters=Wrapper.PublishIntoWord(this,allocator40,selection4,fault.Character,token)',
                  'payload.Flags|=payload.AllChoiceFlags','CreateGlobalWordSelection(payload','Record("word-registered")']
    offsets=[called.index(s) for s in called_order];assert offsets==sorted(offsets)
    # Selected-only count/flag invariant is ACTUAL current code, not an old view.
    receipt=texts['LegacyFreshMainGlobalWordSourceReceipt.cs']
    assert 'AllChoiceFlags=>allChoices is null?ChoiceFlags:allChoiceFlags;' in receipt
    assert 'if(allWords is null)payload.ChoiceCount=1;else payload.InitializeChoices' in called
    append=_body(receipt,'internal void AppendOrdinaryHeader(')
    order=['source.Owner.RequireCurrentSource(source,token)','ReferenceEquals(actual.Payload.Node.FirstGlobalSource,actual.Payload.Source)',
           'if(!source.IsOrdinarySource)return;','var key=actual.Payload.Node.Key;',
           'LegacyMainWordHeaderUnionBudget.BeforeUnion','parent.ReserveWordHeaderUnion(bound,token)',
           'ordinary=parent.RangeOperations.Union(ordinary,key,token)']
    offsets=[append.index(s) for s in order];assert offsets==sorted(offsets)
    # Existing original numeric AVL body assertion stays mandatory on real source.
    interner=texts['LegacyOwnedMainTextSourceInterner.cs'];avl=_body(interner,'private LegacyOwnedMainTextSourceInternResult InternCore')
    avl=avl.lstrip().replace(',globalSource,globalWordSource)',',globalSource)')
    assert _sha(avl)=='fe312e01172a6989f9f9117df9b87910f144d9494c56dda91d9744edd146790a'
    return dict(Branch='ReviewedActual935WordText',ReviewedChangedContracts=pins['reviewedChangedContracts'],CurrentGuardPins=count,
                Main930InverseLfSha256=inverse,CurrentSelectedOnlyOrderVerified=True,
                CurrentExecutionOrder=dict(Outer=outer_order,Called=called_order),
                SelectedOnlyInvariant='allWords/allChoices null: ChoiceCount1 and AllChoiceFlags exactly ChoiceFlags; batch/all275 separate',
                HeaderKeyInvariant='Same private immutable interner key from FirstGlobalSource.CopyRanges, same-wrapper guards before budget then Union; AVL unchanged',
                Authority='Actual current source and call graph; no old tree substitution or all-mode count1 claim')
def _cohort_mode(scope,texts,pins):
    assert pins['historical']['SourceLfSha256']==HISTORICAL_WORD_COHORT_SHA
    assert pins['currentContractCohort']==CURRENT_WORD_COHORT_SHA
    actual={n:_sha(t) for n,t in texts.items()}
    if actual==HISTORICAL_WORD_COHORT_SHA:
        assert _sha(scope)=='c92eb81a1702fa6ed0c262989f993f4350a5ddbb4cb5e999983f8404463167d5'
        return 'HistoricalExact877d615'
    assert actual==CURRENT_WORD_COHORT_SHA,'Unknown/mixed WORD source cohort'
    assert _sha(scope)=='2bf46179d723cfb34a1c72f219b119625fead32bf240d831ee8f72de15b86673'
    return 'Current935'
def authenticate_word_text_cohort(root):
    raw=Path(__file__).with_name('global-wordtext-source-bridge-pins.json').read_bytes();assert hashlib.sha256(raw).hexdigest()==PINS_SHA
    pins=json.loads(raw);root=Path(root)
    from publication_compile_default_source import inspect_default_cohort,read_default_text
    compileDefault=inspect_default_cohort(root);projections=[]
    def read(name):
        if name=='LegacyFreshMainGlobalWordTextPublication.cs':
            text,receipt=read_default_text(root,name);projections.append(receipt);return text
        return(root/'src/InkRecognition'/name).read_text(encoding='utf-8')
    scope=(root/'src/InkRecognition/LegacyLibraryGlobalTextPublicationScope.cs').read_text(encoding='utf-8')
    contract_texts={n:read(n) for n in pins['currentContractCohort']}
    mode=_cohort_mode(scope,contract_texts,pins)
    if mode=='HistoricalExact877d615':
        return dict(Branch=mode,NewFacadeProof=False,Authority='Exact original historical WORD cohort only; no current-facade assertion')
    from global_facade_owner_source_bridge import authenticate_owner_cohort
    owner=authenticate_owner_cohort(root);assert owner['NewFacadeProof'] is True
    texts={n:read(n) for n in pins['files']}
    receipt=_verify_sources(texts,pins);receipt['OwnerSourceBridge']=owner;receipt['CompileDefaultSources']=projections;return receipt
def verify_word_text_source(root):return authenticate_word_text_cohort(root)
