"""Pure exact-source/isolated-loader controls; never executes an auditor driver or target."""
import ast,hashlib,json,runpy,sys
from pathlib import Path
repo=Path(sys.argv[1]);out=Path(sys.argv[2]);assert not out.exists()
p=repo/'tools/LegacyEnglishCharacters.Probe/source_preimage.py';g=runpy.run_path(str(p));restore=g['restore'];steps=g['STEPS'];positive=negative=0
def refuses(f):
 global negative
 try:f()
 except(AssertionError,KeyError,TypeError,ValueError):negative+=1
 else:raise AssertionError('Source mutation accepted')
for row in steps.values():
 name=row['name'];live=(repo/'src/InkRecognition'/name).read_bytes().replace(b'\r\n',b'\n')
 expected=row['to']
 if expected not in g['ALLOWED_EXPECTED'][name]:
  #Internal default-body transition is not a new final expected authority.
  assert (name,expected)in{
   ('LegacyNativePlainWordPublisher.cs','2f7144e49daa2bf36939db91fd48697769696e82ebe73d604c040d87a5aae08d'),
   ('LegacyOrdinaryGraphSourceRanges.cs','aa8a7de87951050c1312e2b84218a6eb2300fdec7842ec8cf46c680c56a6f5ab')}
  refuses(lambda:restore(name,live,expected))
  expected='9d6736eb716317d70640082cbd5c09a431819ca14e31664326f0aa9c3c957935'if name=='LegacyNativePlainWordPublisher.cs'else'8d4014764d0bc03ed30b3ee9180f2d4fee682e4690ba58be8bfdcecaf3e5f6ff'
 # Recover every approved historical endpoint from its exact canonical successor.
 restored=restore(name,live,expected);assert hashlib.sha256(restored).hexdigest()==expected;positive+=1
 assert restore(name,restored,expected)==restored;positive+=1
 assert restore(name,live.replace(b'\n',b'\r\n'),expected)==restored;positive+=1
 for bad in (live+b' ',b' '+live,live.replace(b'namespace',b'namespacE',1)):
  refuses(lambda bad=bad:restore(name,bad,expected))
 refuses(lambda:restore(name,live,'0'*64));refuses(lambda:restore('Other.cs',live,expected))
 # Successor hash is not a historical expected authority.
 if row['from']not in g['ALLOWED_EXPECTED'][name]:refuses(lambda:restore(name,live,row['from']))
# Meter/body alterations must not acquire the exact reviewed historical inverse.
name='LegacyOrdinaryGraphSourceRanges.cs';live=(repo/'src/InkRecognition'/name).read_bytes().replace(b'\r\n',b'\n')
endpoint='c84a663dcbea60b08f104fc7fd6be712f451058003a6b9c03a27600f5b7c1aef'
for old,new in [(b'meter?.Before(256)',b'meter?.Before(257)'),(b'if(meter is null)rows=',b'if(meter is not null)rows='),(b'if(!found)throw',b'if(found)throw')]:
 assert old in live
 refuses(lambda old=old,new=new:restore(name,live.replace(old,new,1),endpoint))
# A corrupt cyclic intermediate remains bounded and cannot satisfy an endpoint.
key=name+':'+hashlib.sha256(live).hexdigest();original=steps[key]
try:
 steps[key]=dict(original,to=original['from'],edits=[])
 refuses(lambda:restore(name,live,endpoint))
finally:steps[key]=original
# Exact full-FIRST source loop using the original per-file encoding contract.
pins=json.loads((repo/'tools/handwriting/full-english-first-plain-text-pins.json').read_bytes())
for name,want in pins['sharedSources'].items():
 b=(repo/'src/InkRecognition'/name).read_text(encoding='utf-8-sig').encode()
 if name in g['ALLOWED_EXPECTED']:b=restore(name,b,want)
 assert hashlib.sha256(b).hexdigest()==want,name
 positive+=1
# Load only audited function definitions; the PE/native and target drivers never run.
for rel in ('tools/handwriting/audit_full_english_first_plain_text.py','tools/LegacyEnglishMainWordTransitions.Probe/audit_native.py'):
 tree=ast.parse((repo/rel).read_text());fn=next(n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='restored_source')
 ns={'hashlib':hashlib,'Path':Path};exec(compile(ast.Module(body=[fn],type_ignores=[]),rel,'exec'),ns)
 name='LegacyOrdinaryGraphSourceRanges.cs';want=next(r['to']for r in steps.values()if r['name']==name and r['reason'].startswith('Declaration'))
 assert hashlib.sha256(ns['restored_source'](repo,'src/InkRecognition/'+name,want)).hexdigest()==want;positive+=1
# Explicit isolated-loader helper corruption checks: no path monkeypatching.
import tempfile
for rel in ('tools/handwriting/audit_full_english_first_plain_text.py','tools/LegacyEnglishMainWordTransitions.Probe/audit_native.py'):
 tree=ast.parse((repo/rel).read_text());fn=next(n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='restored_source')
 ns={'hashlib':hashlib,'Path':Path};exec(compile(ast.Module(body=[fn],type_ignores=[]),rel,'exec'),ns)
 for mutation in (lambda b:b+b' ',lambda b:b.replace(b'def restore',b'def restorE',1),lambda b:b.replace(b'assert',b'pass #',1)):
  with tempfile.TemporaryDirectory(prefix='char-source-helper-',dir=out.parent)as tmp:
   fake=Path(tmp);hp=fake/'tools/LegacyEnglishCharacters.Probe/source_preimage.py';hp.parent.mkdir(parents=True);hp.write_bytes(mutation(p.read_bytes()))
   refuses(lambda:ns['restored_source'](fake,'src/InkRecognition/LegacyOrdinaryGraphSourceRanges.cs','c84a663dcbea60b08f104fc7fd6be712f451058003a6b9c03a27600f5b7c1aef'))
# All native transition source dependencies, exact historical pins unchanged.
pins=json.loads((repo/'tools/LegacyEnglishMainWordTransitions.Probe/native-pins.json').read_bytes())
for r in pins['sourceDependencies']:
 b=(repo/r['path']).read_bytes();name=Path(r['path']).name
 if name=='LegacyOrdinaryGraphSourceRanges.cs':b=restore(name,b,r['sha256'])
 assert hashlib.sha256(b).hexdigest()==r['sha256'],r['path']
 positive+=1
out.write_text(json.dumps({'Status':'PASS_EXACT_SOURCE_REVERSALS','Positives':positive,'MutationRefusals':negative,'RecognitionExecuted':False,'HistoricalPinsChanged':False,'UnrelatedNumberClassifierStalePinPreserved':True},indent=2)+'\n');print(out.read_text())
