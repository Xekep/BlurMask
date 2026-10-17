"""Independent raw linked-WORD/scalar13F970 audit; no model labels inferred."""
import base64,json,struct,sys
from pathlib import Path
b64=base64.b64decode
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
end=(1<<64)-1
stats=dict(cases=0,equal=0,earlyFlags=0,skips=0,ignored=0,noWrites=0,alternate=0)
for case in json.loads(Path(sys.argv[1]).read_bytes()):
 a,b=b64(case['left']),b64(case['right']);ls=bytearray(b64(case['leftIn']));rs=bytearray(b64(case['rightIn']))
 rows={r['reference']:b64(r['row'])for r in case['rows']};calls=0;lookups=0
 def read(row,scratch):
  global lookups
  kind=u16(row,4)&7;label=u16(row,8)
  if kind==3:
   if label&0x7ffc==0:stats['noWrites']+=1;return
   sub=label&3
   if sub!=1:
    lookups+=1;struct.pack_into('<H',scratch,0,case['controls'][{0:0,2:1,3:2}[sub]])
   scratch[2]=0;return
  if kind==1:label=((label&3)-7)&65535
  elif kind not in(0,2,4):label=65535
  struct.pack_into('<H',scratch,0,label);scratch[2]=int(kind==4)
  if kind==4:stats['alternate']+=1
 def scalar(scratch):
  global calls
  label=u16(scratch,0)
  if scratch[2]==0 and case['map']:calls+=1;label^=case['xor']
  return label
 if(u16(a,50)^u16(b,50))&0x300:
  expected=False;stats['earlyFlags']+=1
 else:
  left,right=u64(a,8),u64(b,8)
  while True:
   #Independent prefix filtering. No scalar invocation occurs for a skipped
   #record, even when the opposing stream has already reached its end.
   while left!=end and rows[left][6]&2:left=u64(rows[left],32);stats['skips']+=1
   while right!=end and rows[right][6]&2:right=u64(rows[right],32);stats['skips']+=1
   if left==end or right==end:expected=left==right;break
   ar,br=rows[left],rows[right];read(ar,ls);read(br,rs);al,bl=scalar(ls),scalar(rs)
   if al==case['ignored']:left=u64(ar,32);stats['ignored']+=1;continue
   if bl==case['ignored']:right=u64(br,32);stats['ignored']+=1;continue
   if al!=bl or ls[2]!=rs[2]:expected=False;break
   left,right=u64(ar,32),u64(br,32)
 assert expected==case['equal'],(stats['cases'],'equal')
 assert ls==b64(case['leftOut'])and rs==b64(case['rightOut']),(stats['cases'],'scratch')
 if 'mapCalls'in case:assert(calls,lookups)==(case['mapCalls'],case['lookupCalls']),(stats['cases'],'callbacks')
 stats['cases']+=1;stats['equal']+=expected
print('PASS independent raw linked-WORD, scalar-residue and mapper/control ordering:',stats)
