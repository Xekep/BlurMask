"""Native prerequisite pins and separately identified library document ordering."""
import argparse,json,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
audits=['audit_main_document_text_selection.py','audit_selected_document_input_transforms.py',
    'audit_document_counter_handoff.py','audit_main_detached_text_dom.py',
    'audit_library_text_dom_target.py','audit_main_cache_acceptance_owners.py']
reports=[]
for i,name in enumerate(audits):
    out=Path(str(a.output)+f'.dependency{i}.json')
    subprocess.run([sys.executable,str(Path(__file__).with_name(name)),str(out)],cwd=root,check=True)
    reports.append(dict(audit=name,report=json.loads(out.read_text(encoding='utf-8'))))
source=(root/'src/InkRecognition/LegacyLibraryTextPublicationDocument.cs').read_text(encoding='utf-8')
order=['LegacyFreshMainSourceLinePublication.PublishFresh(words,token)',
    'LegacyFreshMainDocumentTextSelection.Prepare(lines,token);text.Register(token)',
    'LegacyFreshMainDocumentLineConsistency.Apply(text,token)',
    'LegacyFreshMainDocumentSourceCoverage.Prepare(consistency,token)',
    'LegacyFreshMainDocumentCharacterCoverage.Prepare(coverage,token)',
    'LegacyFreshMainDocumentSelectedSources.Prepare(characters,token)',
    'LegacyFreshMainDocumentSourceIndices.Prepare(state,token);indices.Run(token)',
    'LegacyFreshMainDocumentControlCleanup.Prepare(indices,token);cleanup.Run(token)',
    'LegacyFreshMainDocumentInputTransforms.Apply(cleanup,token)',
    'LegacyFreshMainDocumentCounterHandoff.Apply(transforms,token)',
    'LegacyFreshMainSourceRangeUtf8.Prepare(handoff,token)',
    'LegacyLibraryTextDomTarget.CreateFresh(scope.Seed)',
    'LegacyFreshMainDetachedTextDom.PrepareForTarget(sources,target,token)',
    'LegacyLibraryTextDomInsertion.Prepare(dom,token)',
    'LegacyLibraryTextDomTransfer.Prepare(edit,token)',
    'LegacyLibraryTextDomTransportCache.CreateFresh(target)']
offsets=[source.index(s) for s in order];assert offsets==sorted(offsets)
assert 'if(trace[^1]!="exported")faulted=true' in source
assert 'scope.Validate();token.ThrowIfCancellationRequested()' in source
assert 'scope.Seed.Validate(words.Owner,token)' in source
assert 'private LegacyLibraryTextDomTarget? target' in source
assert 'public LegacyLibraryTextDomTarget' not in source and 'public LegacyFreshMainDocumentCounterHandoff' not in source
assert 'try{applied=target.ApplyPrepared(edit,failure,cancellation,token)' in source
assert 'public void Dispose()' in source and 'transfer?.Dispose();cache?.Dispose()' in source
assert 'scope.Dispose()' not in source
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_text(json.dumps(dict(nativePrerequisiteReports=reports,portableOrderedCalls=order,
    portableLifetimeChecks=8,scope='library configured2 document/DOM/BE2/cache join and explicit library-root apply; not native C20 acceptance'),indent=2)+'\n',encoding='utf-8')
print('PASS6 native prerequisite audits +16 portable ordered stages/8 lifetime checks; no SDK execution or acceptance assertion.')
