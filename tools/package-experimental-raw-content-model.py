"""Exact bundled RawContent2 fallback for opt-in experimental AUTO. No inference."""
import hashlib, importlib.util, json, os, struct
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
RAW_SHA='9bba7902c752fe937fc36f06573bd128c6b08d21a1b378dc460bc61608407739'
PACK_SHA='b3bfe175da6627d8f8281675b5d61aba979478444ccffa0b6eec976fce102d24'
GRAPH_SHA='788aaed9ed4b4eb2f62f0806695f1353d92826889c2e6ce8f503eba56c9c8741'
sha=lambda b:hashlib.sha256(b).hexdigest()
def check(data):
 if len(data)!=5341832 or sha(data)!=PACK_SHA:raise ValueError('Exact bundled raw pack required')
 if data[:8]!=b'IKRRAW01' or struct.unpack_from('<II',data,8)!=(1,len(data)-48):raise ValueError('Raw pack header differs')
 if sha(data[48:])!=data[16:48].hex() or struct.unpack_from('<6I',data,48)!=(11,16,111,14,1,5341760):raise ValueError('Raw profile differs')
 if data[76:80]!=b'ORTM' or sha(data[72:])!=GRAPH_SHA:raise ValueError('Raw graph differs')
 return dict(bytes=len(data),sha256=sha(data),graphSHA256=GRAPH_SHA)
def verify(output):
 path=output/'models/raw-content2.ikrraw'
 with path.open('rb') as f:
  if os.fstat(f.fileno()).st_size!=5341832:raise ValueError('Raw pack extent differs')
  data=f.read(5341833)
 info=check(data)
 proof=json.loads((output/'models/raw-content-model-provenance.json').read_bytes())
 if proof.get('rawSHA256')!=RAW_SHA or proof.get('pack')!=info:raise ValueError('Raw provenance differs')
 return info
def package(output):
 spec=importlib.util.spec_from_file_location('raw_export',ROOT/'tools/export-raw-content-model.py')
 module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
 path=ROOT/'models/raw-content/raw-content2.res'
 with path.open('rb') as f:
  if os.fstat(f.fileno()).st_size!=5345803:raise ValueError('Raw resource extent differs')
  raw=f.read(5345804)
 if sha(raw)!=RAW_SHA:raise ValueError('Raw resource fingerprint differs')
 data=module.pack(module.parse(raw));info=check(data);dest=output/'models';dest.mkdir(parents=True,exist_ok=True)
 (dest/'raw-content2.ikrraw').write_bytes(data)
 (dest/'raw-content-model-provenance.json').write_text(json.dumps(dict(rawSHA256=RAW_SHA,pack=info,scope='Experimental category/region classifier; category score is not formula confidence'),indent=2)+'\n',encoding='utf-8')
 return verify(output)
