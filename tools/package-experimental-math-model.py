"""Package/verify real bundled Math2 assets. Static stdlib export; no NN/SDK load."""
import argparse,hashlib,json,os,struct,subprocess,sys,tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
import importlib.util
_spec=importlib.util.spec_from_file_location('raw_bundle',ROOT/'tools/package-experimental-raw-content-model.py')
_raw=importlib.util.module_from_spec(_spec);_spec.loader.exec_module(_raw)
RAW_SHA='2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
PACK_SHA='08226987e872d1d81b4c691659286ca4fe4c3e8ce55dda699fddd597340d3ed7'
LABEL_SHA='4366c61258cd74b6520f654898f94b846cf772b5fdcd975b6a95a1cdaf154dce'
GRAPHS=('3c1a5b691e34385cdaf18f4bdbdb8c8f8cd2cb757f4faed081858f17ea69f286','1393a3c67eafbee9f0bf80a6c3442119bf78b26901986587a2442b1c2d4a0c4a','925a3302b612e634a226ddc907d6ac52c8e908b3d931040cd49b63f4875e4638')
RUNTIMES={'win-x64':'onnxruntime.dll','linux-x64':'libonnxruntime.so','osx-x64':'libonnxruntime.dylib','osx-arm64':'libonnxruntime.dylib'}
sha=lambda b:hashlib.sha256(b).hexdigest()
def read_bounded(path,maximum,exact=None):
 with path.open('rb')as f:
  size=os.fstat(f.fileno()).st_size
  if size>maximum or exact is not None and size!=exact:raise ValueError('Asset extent differs: '+str(path))
  data=f.read(maximum+1)
 if len(data)!=size:raise ValueError('Asset changed: '+str(path))
 return data

def check_pack(data):
 if len(data)<112 or data[:8]!=b'IKRMATH1' or struct.unpack_from('<I',data,8)[0]!=1:raise ValueError('Bad real Math2 pack')
 size=struct.unpack_from('<I',data,12)[0];body=data[48:]
 if size!=len(body)or size>64*1024*1024 or sha(body)!=data[16:48].hex():raise ValueError('Pack body checksum/size')
 if struct.unpack_from('<5I',body)!=(26,297,0,1,1):raise ValueError('Math2 profile')
 at=20;labels=[];h=hashlib.sha256()
 def u32():
  nonlocal at
  if at+4>len(body):raise ValueError('Truncated Math2 tuple')
  n=struct.unpack_from('<I',body,at)[0];at+=4;return n
 for _ in range(297):
  n=u32()
  if n>4096 or n>len(body)-at:raise ValueError('Bad label extent')
  value=body[at:at+n];at+=n;labels.append(value.decode('utf8','strict'));h.update(struct.pack('<I',n));h.update(value)
 if labels[:2]!=['[bos]','[eos]']or h.hexdigest()!=LABEL_SHA or u32()!=3:raise ValueError('Actual vocabulary required')
 for expected in GRAPHS:
  n=u32()
  if n<8 or n>32*1024*1024 or n>len(body)-at:raise ValueError('Bad graph extent')
  graph=body[at:at+n];at+=n
  if graph[4:8]!=b'ORTM'or sha(graph)!=expected:raise ValueError('Actual3graph hashes required')
 if at!=len(body):raise ValueError('Trailing Math2 tuple')
 if len(data)!=18693607 or sha(data)!=PACK_SHA:raise ValueError('Exact bundled export required')
 return dict(bytes=len(data),sha256=sha(data),graphs=list(GRAPHS),labelsSHA256=LABEL_SHA)

def verify(output,rid):
 models=output/'models';raw=read_bounded(models/'math2-unwrapped.res',18704269,18704269)
 if sha(raw)!=RAW_SHA:raise ValueError('Actual raw Math2 source required')
 pack=check_pack(read_bounded(models/'math2-mode0.ikrmath',64*1024*1024+48))
 native=output/'runtimes'/rid/'native';name=RUNTIMES[rid]
 if not(native/name).is_file()or not(native/'runtime-provenance.json').is_file():raise ValueError('Bundled official ORT/provenance missing')
 provenance=json.loads((native/'runtime-provenance.json').read_bytes())
 runtime=read_bounded(native/name,100000000)
 if provenance['rid']!=rid or provenance['version']!='1.23.2'or provenance['files'][name]['sha256']!=sha(runtime):raise ValueError('Bundled ORT provenance differs')
 if rid=='win-x64'and not {'msvcp140.dll','msvcp140_1.dll','vcruntime140.dll','vcruntime140_1.dll'}<=provenance['files'].keys():raise ValueError('App-local VC runtime missing')
 for leaf,item in provenance['files'].items():
  if Path(leaf).name!=leaf or leaf in ('.','..'):raise ValueError('Unsafe runtime provenance leaf')
  value=read_bounded(native/leaf,100000000)
  if len(value)!=item['bytes']or sha(value)!=item['sha256']:raise ValueError('Incomplete runtime dependency: '+leaf)
 _raw.verify(output)
 proof=json.loads((models/'math-model-provenance.json').read_bytes())
 if proof.get('rawSHA256')!=RAW_SHA or proof.get('pack')!=pack:raise ValueError('Bundled Math2 provenance differs')
 return pack

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output',type=Path,required=True);p.add_argument('--rid',choices=RUNTIMES,required=True);p.add_argument('--verify-only',action='store_true');a=p.parse_args()
 if not a.verify_only:
  _raw.package(a.output)
  source=ROOT/'models/math2/math2-unwrapped.res';raw=read_bounded(source,18704269,18704269)
  if sha(raw)!=RAW_SHA:raise ValueError('Unreviewed Math2 source')
  dest=a.output/'models';dest.mkdir(parents=True,exist_ok=True)
  with tempfile.TemporaryDirectory(prefix='math-bundle-',dir=dest)as temporary:
   temp=Path(temporary);pack=temp/'math2-mode0.ikrmath'
   subprocess.run([sys.executable,'-I','-B',str(ROOT/'tools/export-ink-math-model.py'),str(source),'--output',str(pack)],check=True)
   info=check_pack(read_bounded(pack,64*1024*1024+48))
   (temp/'math2-unwrapped.res').write_bytes(raw)
   proof=dict(scope='Experimental manual Math2 and classified AUTO; formula confidence=null, classification is not formula calibration',rawSHA256=RAW_SHA,pack=info)
   (temp/'math-model-provenance.json').write_bytes((json.dumps(proof,indent=2)+'\n').encode())
   for name in('math2-unwrapped.res','math2-mode0.ikrmath','math-model-provenance.json'):os.replace(temp/name,dest/name)
 info=verify(a.output,a.rid);print('PASS bundled experimental Math2 source/vocabulary/3graphs/ORT:',info['bytes'],'bytes; no inference')
if __name__=='__main__':main()
