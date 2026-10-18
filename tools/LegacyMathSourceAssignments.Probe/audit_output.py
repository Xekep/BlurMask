"""Independent source-aware041020 output composition, no insertion loop."""
from pathlib import Path
import json,sys,struct,base64
def f32(x):return struct.unpack('<f',struct.pack('<f',x))[0]
c=json.loads(Path(sys.argv[1]).read_bytes());assert len(c['Rows'])==4096 and c['Guards']==13
complete=retained=tied=0
for row in c['Rows']:
 s=row['Segments'];n=len(row['Map']);old=row['Initial']
 if len(old)==s:expected=old;done=True;complete+=1
 else:
  done=False;it=iter(old);expected=[]
  for col in range(s):
   if not row['All'] and row['Source'] and row['Source'][col]>>24==0:
    expected.append(next(it));retained+=1
   else:
    values=[f32(row['Probabilities'][r*s+col]) for r in range(n)]
    best=max(values);r=values.index(best);expected.append(row['Map'][r]);tied+=values.count(best)>1
  assert next(it,None) is None
 assert expected==row['Output'] and done==row['Complete']
first,second=c['Compositions']
assert first['Corrected']==[0x01000010,0x01000020,42]
assert first['Assigned']==first['FinalAssignments']==[2,2,0]
assert first['Inserted']==first['Corrected'] and base64.b64decode(first['Mask'])==bytes([0,0,0])
assert second['Corrected']==[0x01000010,42] and second['Assigned']==[1,1,-1,-1,1]
assert second['Inserted']==[0x01000010,42,43] and second['FinalAssignments']==[1,1,2,2,1]
assert base64.b64decode(second['Mask'])==bytes([0,0,1])
assert complete>100 and retained>1000 and tied>1000
print(f'PASS independent4096 source assignment controls/{complete} complete fast paths/{retained} retained indices/{tied} tied columns/2 cross-helper compositions/13 guards')
