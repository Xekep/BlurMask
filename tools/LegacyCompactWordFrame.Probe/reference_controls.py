"""Independent207420 and ordinary2075E0 producer composition, output only."""
import ast,base64,json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise ValueError('output.json required')
#Reuse the separately checked independent list oracle, without executing its
#CLI or writing its files. No production C# implementation is imported.
source=Path(__file__).resolve().parent.parent/'LegacyCompactWordSequence.Probe/reference_controls.py'
tree=ast.parse(source.read_text(encoding='utf-8'));names={'TABLE','u16','i32','w16','w32','normalize'}
selected=[]
for item in tree.body:
 if isinstance(item,ast.FunctionDef) and item.name in names:selected.append(item)
 elif isinstance(item,ast.Assign) and any(isinstance(t,ast.Name) and t.id in names for t in item.targets):selected.append(item)
namespace={'struct':struct};exec(compile(ast.Module(body=selected,type_ignores=[]),str(source),'exec'),namespace)
normalize=namespace['normalize'];w16=namespace['w16'];w32=namespace['w32'];u16=namespace['u16']
rng=random.Random(0x2075e0);cases=[]
for index in range(768):
 current=[rng.randrange(100) for _ in range(rng.randrange(8))]
 previous=current[:rng.randrange(len(current)+1)] if index%3 else [rng.randrange(100)]
 associated=bool(index&1);quotes=bool(index&2);sentinels=bool(index&4)
 apostrophe=10;quote=11 if index%13 else 10;special1=60000;special2=60001;knowledge=178;specialde=183
 row=bytearray(rng.randbytes(88));node=rng.randrange(0x20000)
 if index%3:node=rng.randrange(65536);w16(row,24,node)
 else:w16(row,24,node+1)
 row[5]=(row[5]&127)|(128 if index%4==0 else 0)
 w16(row,36,[apostrophe,quote,12][index%3]);row[26]=rng.randrange(256)
 category1=row[26]&63 if index%5 else rng.randrange(256)
 category2=row[26]&63 if index%7 else rng.randrange(256)
 rewritten=list(current);calls=[]
 if u16(row,24)==node:
  if associated and quotes:
   sym=u16(row,36)
   if sym==apostrophe:
    calls.append(special1)
    if row[26]&63==category1:rewritten=current[:-1]+[special1]
   if rewritten==current and sym==quote:
    calls.append(special2)
    if row[26]&63==category2:rewritten=current[:-1]+[special2]
 elif row[5]&128:rewritten=[knowledge]
 frame=bytearray(944)
 if u16(row,24)!=node and row[5]&128:descriptors=[knowledge]
 else:
  common=0
  for a,b in zip(rewritten,previous):
   if a!=b:break
   common+=1
  descriptors=rewritten[common:]
 control=0
 for i,sym in enumerate(descriptors):
  at=4+12*i;w16(frame,at,sym);kind=0
  is_sentinel=sym==0xfff9 or (sym+9)&0xfffa==0
  if not(sentinels and is_sentinel) and (((sym+0x15)&65535)<0x13 or sym in(specialde,65535)):
   kind=3;control+=1
  frame[at+2]=kind;w32(frame,at+4,-1)
 w32(frame,0,len(descriptors));w32(frame,0x3ac,control)
 arc=u16(row,28);struct.pack_into('<Q',frame,0x260,(node&0xffffff)|(arc<<24));w32(frame,0x25c,1)
 glyph=rng.randrange(256);w32(frame,0x3a4,glyph)
 expected=normalize(frame,sentinels)
 cases.append(dict(current=current,previous=previous,associated=associated,quotes=quotes,sentinels=sentinels,
  row=base64.b64encode(row).decode(),node=node,category1=category1,category2=category2,
  apostrophe=apostrophe,quote=quote,special1=special1,special2=special2,knowledge=knowledge,specialde=specialde,
  glyph=glyph,rewritten=rewritten,calls=calls,expected=base64.b64encode(expected).decode()))
Path(sys.argv[1]).write_text(json.dumps(cases),encoding='utf-8')
print('wrote768 independent complete2075E0 frames/context-call controls')
