"""Independent root-row case variants and context-symbol ownership, all TCM states."""
import ast,json,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise ValueError('case-preparation-controls.json capture.json')
reference=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'))[0]
classes={r[0]:r for r in reference['rows']}
tree=ast.parse((Path(__file__).parent/'reference_controls.py').read_text(encoding='utf-8'))
namespace={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='POLICIES' for t in n.targets)],type_ignores=[]),'numeric-policy-facts','exec'),namespace)
policy=namespace['POLICIES'][512]
capture=json.loads(Path(sys.argv[2]).read_text(encoding='utf-8'));source=bytes.fromhex(capture['source']);contexts={row[0]:row[1:] for row in capture['contexts']}
assert len(source)==132*32 and len(capture['states'])==28
total=0
for state in capture['states']:
 s=state['state'];t=policy['rows'][s];alternate=policy['alternate'][s];actual=bytes.fromhex(state['records']);expected=[]
 for i in range(132):
  row=source[i*32:(i+1)*32];label=struct.unpack_from('<H',row,20)[0];category=classes[label][2 if alternate else 1];forms=classes[label][4 if alternate else 3]
  flags=row[5]&3;plan=[]
  def append(column,form=-1,cost=0,force=False):
   if force or t[column]!=255:plan.append((t[column]&31,form,cost))
  if category==1:
   append(3);append(6,1,1)
   if t[6]!=255 and flags==3:append(11,1,1,True)
  elif category==2:
   append(3);append(7,1,1)
   if t[7]!=255 and flags==3:append(12,1,1,True)
   append(8,2,1)
   if t[8]!=255 and flags==3:append(13,2,1,True)
  elif category==3:append(4);append(10,0,8192 if s==0 else 0)
  elif category==4:
   append(5);append(9,2,1);append(10,0,(1 if t[9]!=255 else 0)+(8192 if s==0 else 0))
  else:plan=[((t[{5:0,6:1}.get(category,2)] if category!=7 else s)&31,-1,0)]
  for case,form,cost in plan:
   v=bytearray(row);v[7]=(v[7]&3)|(case<<2);struct.pack_into('<i',v,16,struct.unpack_from('<i',row,16)[0]+cost)
   if form>=0:struct.pack_into('<H',v,20,forms[form])
   expected.append((v,form>=0))
 assert len(actual)==32*len(expected),(s,len(actual),len(expected))
 for i,(wanted,rewritten) in enumerate(expected):
  value=actual[i*32:(i+1)*32];context=struct.unpack_from('<Q',value,8)[0]
  assert context in contexts and contexts[context][0]==0
  assert contexts[context][1]==struct.unpack_from('<H',value,20)[0]
  if rewritten:struct.pack_into('<Q',wanted,8,context)
  assert value==wanted,(s,i,value.hex(),wanted.hex())
 total+=len(expected)
#Exact signed-byte category list at native2E8998, independent of managed accessors.
categories=[9,11,1,4,4,4,4,4,5,5,5,10,1,12,4,6,6,6,5,5,5,2,2,2,3,3,3,5,0,0,0,0]
assert len(capture['metadata'])==512
for sample in capture['metadata']:
 kind=classes[sample['label']][1]
 expected=0 if sample['ignored'] else 7 if sample['start'] and kind==8 else 8 if kind==9 else categories[sample['state']]
 data=bytes.fromhex(sample['bytes'])
 assert len(data)==16 and struct.unpack_from('<i',data,8)[0]==expected,(sample,expected)
 assert data[:4]==bytes(4) and data[12:]==bytes([7,0,0,0])
print(f'PASS independent actual132 dictionary rows -> {total} ordered case variants over28 states; symbols, costs, flags, same-owner root contexts;512 terminal categories')
