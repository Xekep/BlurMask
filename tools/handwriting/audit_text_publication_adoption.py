"""Read-only native input/selection prerequisites plus portable adoption ordering.

The native audits pin constructors/import/UUID and C4D origins. The separately
named source checks only verify library lifetime/data-flow seams, not a claim
that our explicit configured2 driver is the native049370 scheduler.
"""
import argparse,json,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
dependencies=['audit_text_processing_context_preparation.py','audit_library_text_processing_input.py','audit_selected_input_word_choices.py']
outputs=[]
for index,name in enumerate(dependencies):
    out=Path(str(a.output)+f'.dependency{index}.json')
    subprocess.run([sys.executable,str(Path(__file__).with_name(name)),str(out)],cwd=root,check=True)
    outputs.append(dict(audit=name,report=json.loads(out.read_text(encoding='utf-8'))))
def source(name):return (root/'src/InkRecognition'/name).read_text(encoding='utf-8')
s=source('LegacyLibraryTextPublicationScope.cs')
ordered=['LegacyOwnedRawInkDocument.Create(raw,token)',
    'LegacyFreshTextFieldInputContext.Prepare(', 'LegacyLibraryTextProcessingContext.CreateFresh(input,token)',
    'context.PrepareInput(token);lease=context.RetainForPublication()',
    'scope=new(context,lease,models,table,publication)',
    'models.PrepareConfiguredDocument(document,resolution,table,caseData,publication,token)']
positions=[s.index(v) for v in ordered];assert positions==sorted(positions)
shared=source('LegacyFreshMainTextSourceWrapper.cs')
assert 'Document=context.Document;Strings=context.Strings' in shared
assert 'DocumentSeed=new(this,context.SelectedInput,deferIdentifier:true)' in shared
assert shared.index('topOwner=top;DocumentSeed!.InitializePublicationIdentifier()')<shared.index('CapturePublicationIdentifier(DocumentSeed)')
assert 'if(PublicationScope is null)Strings.Dispose()' in shared
ctx=source('LegacyLibraryTextProcessingContext.cs')
assert 'if(!disposed||publicationLeases!=0||resourcesReleased)return' in ctx
assert 'publicationClaimed || request is null || !request.Succeeded || flags!=1' in ctx
assert 'WeakReference<LegacyFreshMainTextDocumentSeed>' in ctx
seed=source('LegacyFreshMainTextDocumentSeed.cs');choices=source('LegacyFreshMainWordSourceChoices.cs')
assert 'shared.PublicationScope?.Validate()' in seed and 'shared.PublicationScope?.Validate()' in choices
assert 'ReferenceEquals(actualCompletion.Sources.CandidateSource.ConfiguredResult,result)' in s
assert 'if(adopted is not null)' in choices and 'adopted.BindActualTop(source.TopOwner)' in choices
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_text(json.dumps(dict(nativePrerequisites=outputs,portableOrderedSteps=ordered,
    portableLifetimeChecks=10,scope='library-owned configured2 prepublication adoption; not049370 scheduler/C20 acceptance'),indent=2)+'\n',encoding='utf-8')
print('PASS 3 native prerequisite audits + 6 ordered library adoption steps/10 lifetime and identity source checks; no vendor execution.')
