"""Independent actual global post-TEXT exact coverage and C21 subtraction.
Producer evidence is first checked independently against raw AL/source bytes.
No expected word labels, configured-two-alternative owner or SDK execution.
"""
import importlib.util,json,runpy,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)not in(5,6):raise SystemExit('MODELS CHAR-CAPTURE WORD-TEXT-CAPTURE COVERAGE-CAPTURE [NATIVE-CAPTURE]')
models,char_path,word_path,coverage_path=sys.argv[1:5];saved=sys.argv[:]
try:
 sys.argv=['audit_output.py',models,char_path,word_path]
 runpy.run_path(str(root/'tools/LegacyGlobalWordText.Probe/audit_output.py'),run_name='__main__')
finally:sys.argv=saved
spec=importlib.util.spec_from_file_location('global_coverage_ranges',root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py')
ops=importlib.util.module_from_spec(spec);spec.loader.exec_module(ops)
actual=json.loads(Path(coverage_path).read_bytes());parents={s['name']:s for s in json.loads(Path(word_path).read_bytes())['actual']}
completed=refused=subtractions=0
assert len(actual['samples'])==13 and len({s['name']for s in actual['samples']})==13
for s in actual['samples']:
 p=parents[s['name']];assert s['word']==p['wordKey']and s['text']==p['textKey']
 if s['word']!=s['text']:
  assert not s['completed']and s['refusal']=='238B0';refused+=1;continue
 completed+=1;assert s['completed']
 assert(s['lineCount'],s['last'],s['wordVisits'],s['helpers'])==(1,0,1,1)
 assert s['adjacent']==len(p['links'])-1 and s['subtractions']==len(p['links'])
 assert all(g['line']==0 for g in p['links'])
 initial=p['ordinary']or p['wordKey'];assert s['initial']==initial and s['ordinary']==bool(p['ordinary'])
 keys=[g['processed']for g in p['links']];assert s['keys']==keys
 remaining=[r[:]for r in initial]
 for key in keys:remaining=ops.subtract(remaining,key,s['counts']);subtractions+=1
 assert s['remaining']==remaining
assert(completed,refused,actual['completed'],actual['refused'],actual['guards'])==(11,2,11,2,62)
assert actual['poisonedCounterWithoutCommand']is True
if len(sys.argv)==6:assert Path(coverage_path).read_bytes()==Path(sys.argv[5]).read_bytes()
print(f'PASS13 genuine global post-TEXT owners/{completed} exact/{refused} explicit238B0 refusals/{subtractions} actual forward CHAR subtractions; no DOM/quality claim.')
