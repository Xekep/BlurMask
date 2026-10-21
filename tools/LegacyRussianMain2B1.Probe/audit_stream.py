"""Generated JSON chunk-boundary/strict delimiter/truncation controls.
Optionally compare entire old capture stream to stdlib whole-file parse.
"""
import ast,hashlib,json,sys,tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
p=ROOT/'tools/LegacyRussianMainAcoustic.Probe/audit_output.py';t=ast.parse(p.read_text());t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];g={'__file__':str(p)};exec(compile(t,str(p),'exec'),g);samples=g['samples']
base=Path(sys.argv[1]).resolve();base.mkdir(parents=True,exist_ok=True);passed=0
prefix='{"inputSha256":"c8fe150f3ca960cf7216da88c1dd90e1045aed2ef4652a8fa66c9e7a85353dce","expectedLabelsRead":false,"samples":['
rows=[dict(id='one',nested=[1,2,dict(text='comma, escape\\ slash',number=-1.25e-4)]),dict(id='two',values=[17,42])];payload=prefix+','.join(json.dumps(v,separators=(',',':'))for v in rows)+']}'
with tempfile.TemporaryDirectory(prefix='stream-',dir=base)as temp:
 folder=Path(temp).resolve();assert folder.parent==base;path=folder/'capture.json'
 for size in [1,2,3,7,17,64,65535,65536,65537]:
  path.write_text(payload);assert list(samples(path,size))==rows;passed+=1
 # Put the comma in the FIRST character of a fresh default-sized chunk.
 for target in [65535,65536,65537,131072]:
  head=prefix+'{"pad":"';pad=target-len(head)-2;first={'pad':'x'*pad};text=prefix+json.dumps(first,separators=(',',':'))+','+json.dumps(rows[1],separators=(',',':'))+']}';assert text[target]==',';path.write_text(text);assert list(samples(path))==[first,rows[1]];passed+=1
 for tail in ['', '{', '{"x":', '{"x":"unterminated', '{"x":1}', '{"x":1},', '{"x":1},]', '{"x":1}{"y":2}]', '{"x":1},,{}]']:
  path.write_text(prefix+tail)
  try:list(samples(path,7))
  except(ValueError,AssertionError):passed+=1
  else:raise AssertionError(('Malformed/truncated accepted',tail))
 path.write_text(prefix+']}');assert list(samples(path,1))==[];passed+=1
report=dict(generatedControls=passed,oldCaptureRows=None)
if len(sys.argv)>2:
 old=Path(sys.argv[2]);whole=json.loads(old.read_bytes())['samples'];expected=hashlib.sha256()
 for row in whole:expected.update(json.dumps(row,separators=(',',':'),sort_keys=True).encode())
 report['oldCaptureRows']=len(whole);del whole
 import gc;gc.collect();actual=hashlib.sha256();count=0
 for row in samples(old):actual.update(json.dumps(row,separators=(',',':'),sort_keys=True).encode());count+=1
 assert count==report['oldCaptureRows'] and actual.digest()==expected.digest();report['oldCaptureCanonicalRowDigest']=actual.hexdigest();report['oldCaptureSha256']=hashlib.file_digest(old.open('rb'),'sha256').hexdigest()
if len(sys.argv)>3:Path(sys.argv[3]).write_text(json.dumps(report,indent=2)+'\n')
print('PASS',json.dumps(report))
