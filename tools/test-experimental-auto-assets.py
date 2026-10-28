"""Pure byte checks for bundled AUTO assets. No ORT, NN, SDK or user files."""
import argparse,hashlib,importlib.util,json,struct
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
def cache_source_controls(manual,raw):
 # Syntactic wiring controls only. A real failed-load cache exercise is separate;
 # these do not claim to execute ORT or validate cache behavior dynamically.
 manual=''.join(manual.split());raw=''.join(raw.split())
 for source in (manual,raw):assert 'privatestaticboolRecoverable(Exceptione)=>eisInvalidDataExceptionorIOException' in source
 assert manual.index('if(failed)returnnull;')<manual.index('owner=LoadBundled(token);')
 assert 'if(loading){DisposeLocked();signature=ReadFailureSignature();Volatile.Write(reffailureIdentity,signature);failed=true;}' in manual
 assert raw.index('if(failed==current)returnnull;')<raw.index('RawContentAnalyzer.Load(stream,Runtime)')
 assert 'if(loading){owner?.Dispose();owner=null;failed=signature;}' in raw
 assert 'catch(OperationCanceledException){throw;}' in manual and 'catch(OperationCanceledException){throw;}' in raw
 return ['explicit-invalid-data-recovery','manual-failed-before-load','manual-signature-memoization','raw-failed-before-load','raw-signature-memoization','cancel-not-failure']
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
 spec=importlib.util.spec_from_file_location('auto_assets',ROOT/'tools/package-experimental-raw-content-model.py')
 module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
 info=module.package(a.output);data=(a.output/'models/raw-content2.ikrraw').read_bytes()
 raw=(ROOT/'models/raw-content/raw-content2.res').read_bytes()
 # Separate literal pack construction from authenticated original resource bytes.
 assert len(raw)==5345803 and hashlib.sha256(raw).hexdigest()==module.RAW_SHA
 graph=raw[4043:];assert len(graph)==5341760 and graph[4:8]==b'ORTM'
 body=struct.pack('<6I',11,16,111,14,1,len(graph))+graph
 expected=b'IKRRAW01'+struct.pack('<II',1,len(body))+hashlib.sha256(body).digest()+body
 assert data==expected and data[72:]==graph
 controls=[]
 for name,changed in [('truncated',data[:-1]),('trailing',data+b'0'),('graph',data[:72]+bytes([data[72]^1])+data[73:]),('header',b'X'+data[1:]),('profile',data[:48]+b'X'+data[49:]),('checksum',data[:16]+bytes([data[16]^1])+data[17:])]:
  try:module.check(changed)
  except ValueError:controls.append(name)
  else:raise AssertionError('Malformed pack accepted: '+name)
 manual=(ROOT/'src/NovaPen/AiExperimentalMathModels.cs').read_text(encoding='utf-8');raw_owner=(ROOT/'src/NovaPen/AiExperimentalRawContentModels.cs').read_text(encoding='utf-8')
 source=cache_source_controls(manual,raw_owner);source_negatives=[]
 for name,m,r in [('missing-manual-data-catch',manual.replace('InvalidDataException or ',''),raw_owner),('manual-no-memoization',manual.replace('failed=true;','failed=false;'),raw_owner),('missing-raw-data-catch',manual,raw_owner.replace('InvalidDataException or ','')),('raw-no-memoization',manual,raw_owner.replace('failed=signature;','failed=null;'))]:
  try:cache_source_controls(m,r)
  except (AssertionError,ValueError):source_negatives.append(name)
  else:raise AssertionError('Broken cache wiring admitted: '+name)
 report=dict(pack=info,rawGraphByteEquality=True,independentLiteralPackEquality=True,negativeControls=controls,cacheSourceControls=source,cacheSourceNegativeControls=source_negatives,scope='Static original bytes and syntactic source wiring only; no ORT or NN/cache runtime')
 (a.output/'asset-controls.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
 print(json.dumps(report))
if __name__=='__main__':main()
