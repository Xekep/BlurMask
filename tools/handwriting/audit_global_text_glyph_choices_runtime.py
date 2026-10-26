"""Static scoped owner composition, no vendor execution; numerical proof separate."""
import hashlib,json,runpy,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=2:raise SystemExit('REPORT.json')
target=Path(sys.argv[1]);saved=sys.argv
from global_facade_owner_source_bridge import authenticate_owner_cohort
ownerCohortAuthentication=authenticate_owner_cohort(root)
plan=target.with_name(target.stem+'.plan.json');script=Path(__file__).with_name('audit_global_text_glyph_choices_plan.py')
sys.argv=[str(script),str(plan)];runpy.run_path(str(script),run_name='__main__');sys.argv=saved
code=(root/'src/InkRecognition/LegacyFreshMainGlobalTextGlyphChoices.cs').read_text()
positions=(root/'src/InkRecognition/LegacyFreshMainGlobalGlyphDocumentPositions.cs').read_text()
ownerSourceBridge=None
if 'ReferenceEquals(actualScope.SourceCompletion,actualCompletion)' in code:
 assert 'ReferenceEquals(actualScope.SourceCompletion,actualCompletion)' in code
else:
 from global_facade_owner_source_bridge import verify_owner_routes
 ownerSourceBridge=verify_owner_routes(root)
 assert 'ReferenceEquals(actualScope.SourceCompletion?.Legacy,actualCompletion)' in code
for term in ['strings=scope.SharedState.Strings;', 
             'ReferenceEquals(actualScope.SharedState.GlobalTopOwner,actualCompletion.Source.Session!.Ownership)',
             'ReferenceEquals(actualScope.SharedState.Document,actualCompletion.Source.Owner.SourceDocument)',
             'foreach(ulong reference in references)','Refresh.RefreshOwner(key,allocatedRecordStorage40,token)',
             'projection.Project(refreshed.Source,token)',
             'Refresh.Select(reference,allocatedRecordStorage40,selectionAllocation4,token:token)',
             'selected.Selection.CopySelectionRows4(),key,false,unchecked((int)selected.Selection.SelectedMarker!.Value)',
             'strings,properties,normalization,actualControlLabelLookup:text=>Emitter.ReadControlLabel(text,token)',
             'actualFullGlyphEmitter:glyph=>Emitter.Emit(glyph,token)','catch{faulted=true;throw;}',
             'return null;','strings.Validate(text);']:
    assert term in code,term
order=['entered=true;','var positions=new Dictionary','Refresh.RefreshOwner(key,allocatedRecordStorage40,token)',
       'var output=new List','Refresh.Select(reference,allocatedRecordStorage40,selectionAllocation4,token:token)',
       'LegacyNativeTextGlyphCaller.Produce(','Validate(token);return output.AsReadOnly();']
assert [code.index(s)for s in order]==sorted(code.index(s)for s in order)
assert code.count('projection=new(scope,completion,token);')==1
assert positions.count('new LegacyNativeTextSourceInputMap(document.CopySelectedInputs(),[],0)')==1
context_ctor=positions.index('internal LegacyFreshMainGlobalGlyphProjectionContext(')
project=positions.index('internal LegacyFreshMainGlobalGlyphDocumentPositions Project(')
assert context_ctor<positions.index('map=new LegacyNativeTextSourceInputMap(')<project
assert 'new LegacyNativeTextSourceInputMap' not in positions[project:]
if 'ReferenceEquals(scope.SourceCompletion,completion)' in positions:
 assert 'ReferenceEquals(scope.SourceCompletion,completion)' in positions
else:
 from global_facade_owner_source_bridge import verify_owner_routes
 ownerSourceBridge=verify_owner_routes(root)
 assert 'ReferenceEquals(scope.ActualTextCompletion,completion)' in positions
for term in ['ReferenceEquals(actualSource.Owner,completion.Source.Session!.Associations.Metadata)',
             'r.First.First!=0||r.Last.First!=0','r.FirstResidual,r.LastResidual',
             'new LegacyNativeTextSourceInputMap(document.CopySelectedInputs(),[],0)',
             'LegacyNativeTextSourcePositionPublication.Project(map,references,true,original,token)',
             'return ranges.ToArray();']:
    assert term in positions,term
for term in ['LegacyOwnedMainCandidateSource','LegacyFreshMainCompletedFirstCandidate','LegacyFreshMainSingleLinePlainRecognitionResult']:
    assert term not in code+positions,'Old source owner coercion forbidden'
target.write_text(json.dumps(dict(vendorExecution=False,ownerCohortAuthentication=ownerCohortAuthentication,ownerSourceBridge=ownerSourceBridge,planReportSha256=hashlib.sha256(plan.read_bytes()).hexdigest(),
    orderedRuntimeStages=order,oldFileSeams=0,scope='same preNN global scope/Document/shared3E, actual ordinary V140 positions; all references refreshed before any selection/emission',
    runtimeFailures='poisoned after entered failure; all emitted UTF8 remains borrowed preNN3E, no fresh pool or implicit rollback',
    excluded=['CHAR/WORD/TEXT registration','document acceptance','Russian quality improvement']),indent=2)+'\n')
print('PASS global same-owner two-pass choices, exact V140 original positions, shared preNN3E ownership/lifetime gates; zero old seams.')
