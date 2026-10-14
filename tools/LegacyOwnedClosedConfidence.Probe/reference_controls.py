"""Independent closed rectangle source-score audit, including cross-stroke condition."""
import json,math,pathlib,sys
d=json.loads(pathlib.Path(sys.argv[1]).read_text());groups=0
assert len(d['cases'])==32
for c in d['cases']:
 w,h=c['w'],c['h'];expected=0 if c['isolated'] else 12.566*(w*h)/(4*(w+h)**2)
 assert math.isclose(c['probabilities'][0],expected,abs_tol=.0002),(c,expected)
 assert all(v==0 for v in c['probabilities'][1:])
 groups+=c['groups']
assert d['groupCount']==d['compared']==groups and d['actualRetained']>0 and d['mixedPhysicalGroups']>0
print(f'PASS32 independent rawrectangle area/perimeter/strict-other-stroke scorecases, {groups} preparedgroups and {d["actualRetained"]} actualretainedclosed candidates; noNNclassificationaccuracyclaim.')
