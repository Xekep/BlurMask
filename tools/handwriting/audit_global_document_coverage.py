"""Static composition for bounded genuine-global post-TEXT coverage.
Read-only SDK evidence; numerical outputs have a separate portable oracle.
"""
from pathlib import Path
import argparse,json,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,default=Path(__file__).resolve().parents[2]);a=p.parse_args()
root=Path(__file__).resolve().parents[2];reports=[]
for i,name in enumerate(('audit_main_document_line_consistency.py','audit_main_document_source_coverage.py','audit_main_document_character_coverage.py')):
 out=a.output.with_name('gdc-native-'+str(i)+'.json')
 subprocess.run([sys.executable,str(a.native_repo/'tools/handwriting'/name),str(out)],check=True)
 reports.append(json.loads(out.read_bytes()))
runtime=(root/'src/InkRecognition/LegacyFreshMainGlobalDocumentCoverage.cs').read_text(encoding='utf8')
rules=('!actual.IsComplete||actual.IsFaulted','actual.AlternativeCount!=1||actual.SelectedWordIndex!=0',
 '(payload.Flags&0x201000)!=0','ReferenceEquals(payload.Node.FirstGlobalWordSource,actual.OriginalWordSource)',
 'ReferenceEquals(pending[^1],actual.Text.Selection)','ReferenceEquals(pending[^2],payload.Selection)',
 'links.Length is<1 or>4096','descriptors.Length!=1','actual.LineCount!=1||lines.Length!=56',
 'source.SourceLineId!=last||last!=0','ReferenceEquals(source.Node.FirstGlobalSource,source.Source)',
 'LegacyOwnedRawInkRangeOperations.Equal(normalizedWord,text)','Unequal actual WORD/TEXT coverage requires geometric238B0',
 'normalizedWord=operations.Union([],word,token)','initial=useOrdinary?ordinary:word.ToArray()',
 'CopyNormalizedWordCoverageRanges()','long unionWork=PreflightUnionWork(word.Length)','comparisons=unionWork',
 'work>16_777_216','source.PriorRangeCount!=0','keys[i]=source.Source.CopyRanges(token)',
 'comparisons>16_777_216','remaining=operations.Subtract(remaining,keys[i],token)',
 'actual.Scope.SharedState.Selections.Failed','Publication.Scope.SharedState.Selections.NextSelectionId!=next',
 'Publication.CopyPendingSelections().SequenceEqual(commands)',
 'Publication.WordSource.CopyCharacterLinks().SequenceEqual(links)')
for rule in rules:assert rule in runtime,rule
assert 'LegacyOwnedMainWordSourceChoicesResult'not in runtime and 'LegacyLibraryTextPublicationScope'not in runtime
assert runtime.index('LegacyOwnedRawInkRangeOperations.Equal(normalizedWord,text)')<runtime.index('operations.Subtract(')
a.output.write_text(json.dumps(dict(dependencies=reports,runtimeContracts=len(rules)+3,vendorExecution=False,
 scope='Actual global1/oneWORD/line0 post-TEXT equal coverage and sole-choice subtraction only; no238B0/DOM/quality'),indent=2)+'\n',encoding='utf8')
print(f'PASS{len(rules)+3} actual global bounded postcoverage contracts +native025E40/0248D0/01C740 dependencies; no vendor execution.')
