"""Corrupt genuine sidecars to verify exact fresh/unchanged diagnostic gates."""
from pathlib import Path
import array,copy,hashlib,json,sys,tempfile
from audit_attention import audit

sidecar,capture=map(Path,sys.argv[1:3])
original=json.loads(sidecar.read_bytes())
audit(sidecar,capture)
def bit_change(value):
 frame=value['Fresh'][0]['Frames'][0]
 frame['FloatBits'][0]^=1
 words=array.array('I',frame['FloatBits'])
 if sys.byteorder!='little':words.byteswap()
 # A coherent self-hash still must not equal the other actual route.
 frame['Sha256']=hashlib.sha256(words.tobytes()).hexdigest()
def missing(value):value['Fresh'].pop()
def duplicate(value):value['Fresh'][1]=copy.deepcopy(value['Fresh'][0])
def wrong_route(value):value['Fresh'][0]['Route']='Unchanged'
def wrong_width(value):value['Fresh'][0]['Width']+=1
def wrong_frames(value):value['Fresh'][0]['InputFrames']+=1
def missing_fresh(value):del value['Fresh']
controls=[bit_change,missing,duplicate,wrong_route,wrong_width,wrong_frames,missing_fresh]
with tempfile.TemporaryDirectory(prefix='math-attention-corruption-')as folder:
 path=Path(folder)/'sidecar.json'
 for change in controls:
  value=copy.deepcopy(original);change(value);path.write_text(json.dumps(value),encoding='utf8')
  try:audit(path,capture)
  except(AssertionError,KeyError):pass
  else:raise AssertionError('Accepted corrupted actual evidence: '+change.__name__)
print(json.dumps(dict(PASS=True,Positive=1,RejectedCorruptions=[c.__name__ for c in controls],NoModelOrNeuralExecution=True)))
