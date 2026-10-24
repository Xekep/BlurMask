import copy,json,runpy
from pathlib import Path
D=Path(__file__).resolve().parent;m=runpy.run_path(str(D/'audit_structured.py'));fixture=runpy.run_path(str(D/'prepared_fixture.py'))['fixture'];old,actual,raw=fixture()
assert len(__import__('sys').argv)==2,'Fresh writable report path required'
report=m['audit'](old,actual,raw);names=[]
def reject(name,edit):
 c=copy.deepcopy(actual);edit(c)
 try:m['audit'](old,c,raw)
 except (AssertionError,UnicodeError,ValueError,KeyError,TypeError,IndexError):names.append(name);return
 raise AssertionError('tamper accepted '+name)
def top(k,v):return lambda c:c['StructuredWordConsumer'].__setitem__(k,v)
def use(k,v):return lambda c:c['StructuredWordConsumer']['Uses'][0].__setitem__(k,v)
for k in ['WordCount','ProjectedPointReads','MaximumPointReads']:reject('numeric-bool-'+k,top(k,True));reject('numeric-float-'+k,top(k,float(actual['StructuredWordConsumer'][k])))
for k in ['FullOriginalCoverage','MeetsExperimentalGlyphPolicy','SameOriginalDocument']:reject('bool-int-'+k,top(k,int(actual['StructuredWordConsumer'][k])));reject('wrong-'+k,top(k,not actual['StructuredWordConsumer'][k]))
for k in ['Index','SourceSelectedIndex','ProcessedSelectedIndex','SourceChoiceCount','ProcessedChoiceCount','PointReads']:reject('use-bool-'+k,use(k,True));reject('use-wrong-'+k,use(k,actual['StructuredWordConsumer']['Uses'][0][k]+1))
for k,v in [('CandidateScore',True),('CandidateScore',.5),('CandidateScore',float('nan')),('MeetsExperimentalGlyphPolicy',False),('MeetsExperimentalGlyphPolicy',0),('SelectedUtf8','wrong'),('SelectedUtf8',b'bytes'),('SelectedUtf8','\ud800')]:reject('score-policy-text-'+str(len(names)),use(k,v))
reject('no-uses',top('Uses',[]));reject('duplicate-use',top('Uses',actual['StructuredWordConsumer']['Uses']*2));reject('unknown-field',top('Unknown',0));reject('missing-field',lambda c:c['StructuredWordConsumer'].pop('Coverage'))
for k in ['Left','Top','Right','Bottom']:reject('bounds-'+k,lambda c,k=k:c['StructuredWordConsumer']['Uses'][0]['Bounds'].__setitem__(k,1e9))
for k in ['Index','OwnerTag','OriginX','OriginY','Angle','XHeight','UnadjustedBaseline']:reject('line-'+k,lambda c,k=k:c['StructuredWordConsumer']['Uses'][0]['Line'].__setitem__(k,True))
reject('coverage-tail',lambda c:c['StructuredWordConsumer']['Coverage'][0].__setitem__('Last',3));reject('range-source-id',lambda c:c['StructuredWordConsumer']['Uses'][0]['OriginalRanges'][0].__setitem__('First',100<<32));reject('range-integer-bool',lambda c:c['StructuredWordConsumer']['Uses'][0]['OriginalRanges'][0].__setitem__('First',True))
reject('unexplained-meter',lambda c:c.__setitem__('ValidationLoopBodies',c['ValidationLoopBodies']+1));reject('old-meter',lambda c:c.__setitem__('ValidationLoopBodies',old['ValidationLoopBodies']));reject('canonical-int-bool',lambda c:c.__setitem__('SourceInvocations',True))
for value in ['{"x":1,"x":2}','{"x":NaN}']:
 p=Path(__import__('sys').argv[1]).resolve().parent/'invalid-json.tmp';p.write_text(value)
 try:m['load'](p)
 except AssertionError:names.append('strict-json-'+str(len(names)))
 else:raise AssertionError('invalid JSON accepted')
 p.unlink()
Path(__import__('sys').argv[1]).write_text(json.dumps({'ActualDataAudit':report,'TamperControls':len(names),'OrderedUniqueNames':names,'ValidationDelta':m['validation_delta'](old),'Scope':'Generated synthetic DATA validation controls only; no model/corpus/NN or genuine source proof'},indent=2)+'\n');print(json.dumps({'PASS':report,'TamperControls':len(names)}))
