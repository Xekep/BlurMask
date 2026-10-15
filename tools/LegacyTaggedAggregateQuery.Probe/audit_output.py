"""Independent full-tag, record stamping and finite-state case stream oracle."""
import ast,json,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise ValueError('case-preparation-controls.json captures.json')
classification=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'))[0]
symbols={row[0]:row for row in classification['rows']}
tree=ast.parse((Path(__file__).parents[1]/'LegacyCaseQuery.Probe/reference_controls.py').read_text(encoding='utf-8'));namespace={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='POLICIES' for t in n.targets)],type_ignores=[]),'state-policy-facts','exec'),namespace)
policies=namespace['POLICIES'][512];capture=json.loads(Path(sys.argv[2]).read_text(encoding='utf-8'))
raw=bytes.fromhex(capture['raw']);assert len(raw)==132*32 and capture['nativeAuxiliaryEmptyStates']==32
contexts={r[0]:r[1:] for r in capture['contexts']};count=0
assert len(capture['normal'])==28
for sample in capture['normal']:
 state=sample['state'];tag=(0x83<<56)|(state<<58)|(0xa5<<48)|(0x89<<40)
 assert sample['tag']==sample['forwarded']==tag and ((tag>>32)&255)==0
 row=policies['rows'][state];alt=policies['alternate'][state];expected=[]
 for i in range(132):
  original=raw[i*32:(i+1)*32];label=struct.unpack_from('<H',original,20)[0];kind=symbols[label][2 if alt else 1];forms=symbols[label][4 if alt else 3];flags=original[5]&3;plan=[]
  def emit(column,form=-1,delta=0,force=False):
   if force or row[column]!=255:plan.append((row[column]&31,form,delta))
  if kind==1:
   emit(3);emit(6,1,1)
   if row[6]!=255 and flags==3:emit(11,1,1,True)
  elif kind==2:
   emit(3);emit(7,1,1)
   if row[7]!=255 and flags==3:emit(12,1,1,True)
   emit(8,2,1)
   if row[8]!=255 and flags==3:emit(13,2,1,True)
  elif kind==3:emit(4);emit(10,0,8192 if state==0 else 0)
  elif kind==4:
   emit(5);emit(9,2,1);emit(10,0,(1 if row[9]!=255 else 0)+(8192 if state==0 else 0))
  else:plan=[((state if kind==7 else row[{5:0,6:1}.get(kind,2)])&31,-1,0)]
  for changed,form,cost in plan:
   target=bytearray(original);target[4]=2;target[7]=(target[7]&3)|(changed<<2);target[22]=77
   struct.pack_into('<i',target,16,struct.unpack_from('<i',original,16)[0]+cost)
   if form>=0:struct.pack_into('<H',target,20,forms[form])
   expected.append((target,form>=0))
 actual=bytes.fromhex(sample['records']);assert len(actual)==len(expected)*32
 for i,(target,rewritten) in enumerate(expected):
  value=actual[i*32:(i+1)*32];identity=struct.unpack_from('<Q',value,8)[0]
  assert contexts[identity]==[0,struct.unpack_from('<H',value,20)[0]]
  if rewritten:struct.pack_into('<Q',target,8,identity)
  assert value==target,(state,i)
 count+=len(expected)
aux=bytes.fromhex(capture['auxiliaryBase']);assert 'NOT native092990' in capture['auxiliaryStressScope'] and len(capture['auxiliary'])==32
for sample in capture['auxiliary']:
 state=sample['state'];tag=(1<<32)|(0x83<<56)|(state<<58)|(0x5a<<48)
 assert sample['tag']==sample['forwarded']==tag and ((tag>>32)&255)==1
 actual=bytes.fromhex(sample['records']);assert len(actual)==len(aux)
 for i in range(0,len(aux),32):
  target=bytearray(aux[i:i+32]);target[4]=1;target[7]=(target[7]&0x83)|(state<<2)
  assert actual[i:i+32]==target
 count+=len(aux)//32
print(f'PASS independent full-tag unchanged forwarding, normalV40 vsauxV48 and source stamps over{count} records; actualnativeauxiliary empties32states')
