"""Independent list-based2067C0/206960 oracle; one output path only."""
import base64,json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=2: raise ValueError('output.json required')
rng=random.Random(0x206960)
TABLE=[[-1,-2,-1,-2,3,3],[-1,-1,-1,-2,3,3],[-2,-2,-1,-2,3,3],[-1]*6,[3,3,3,-2,3,3],[3,3,3,-2,3,3]]
u16=lambda b,p:struct.unpack_from('<H',b,p)[0]
i32=lambda b,p:struct.unpack_from('<i',b,p)[0]
def w16(b,p,v):struct.pack_into('<H',b,p,v&65535)
def w32(b,p,v):struct.pack_into('<I',b,p,v&0xffffffff)
def enc(b):return base64.b64encode(b).decode()
def sentinel(s):return s==0xfff9 or ((s+9)&0xfffa)==0
def normalize(raw,flag):
 b=bytearray(raw);n=i32(b,0);slots=[bytearray(b[4+i*12:16+i*12]) for i in range(50)]
 def erase(i):
  nonlocal n
  slots[i:n-1]=[bytearray(x) for x in slots[i+1:n]];n-=1
 for i in range(n-1,-1,-1):
  if u16(slots[i],0)==0xfffd:erase(i)
 for i in range(n-2,-1,-1):
  a=0xfffc-u16(slots[i],0);c=0xfffc-u16(slots[i+1],0)
  if 0<=a<6 and 0<=c<6:
   v=TABLE[a][c]
   if v==-1:erase(i+1)
   elif v==-2:erase(i)
   else:
    sym=(-v-4)&65535;w16(slots[i],0,sym)
    if flag and sentinel(sym):slots[i][2]=0
    erase(i+1)
 for i in range(n):
  if u16(slots[i],0) in (0xfff7,0xfff8):w16(slots[i],0,0xfff9)
 ordinary=i32(b,0x3a0);extra=i32(b,0x3a8);leftover=i32(b,0x3a4);pos=0;k=0
 for supplied in range(ordinary+extra):
  while pos<n-1 and slots[pos][2]>=3:w32(slots[pos],4,k);pos+=1
  if b[0x268+16*supplied]&1:
   if n>=50:raise ValueError('capacity')
   slots[pos+1:n+1]=[bytearray(x) for x in slots[pos:n]];n+=1
   w16(slots[pos],0,0);slots[pos][2]=2
  else:slots[pos][2]=1
  w32(slots[pos],4,k);pos+=1;k+=1
 while pos<n-1:
  if leftover>0 and slots[pos][2]<3:slots[pos][2]=4;leftover-=1
  w32(slots[pos],4,k);pos+=1
 w32(slots[pos],4,k)
 for i in range(pos-1,-1,-1):
  if leftover>0 and ((u16(slots[i],0)+7)&0xfffc)==0 and u16(slots[i],0)!=0xfffa:
   slots[i][2]=4;leftover-=1
 for i,x in enumerate(slots):b[4+i*12:16+i*12]=x
 w32(b,0,n);return b
normal=[]
for index in range(1536):
 b=bytearray(rng.randbytes(944));n=rng.randrange(31);w32(b,0,n)
 for i in range(n):
  s=rng.choice([rng.randrange(65536),rng.randrange(0xffeb,65536)])
  w16(b,4+i*12,s);b[6+i*12]=rng.randrange(7)
 if index<72:
  n=2;w32(b,0,n);w16(b,4,0xfffc-(index//2//6));w16(b,16,0xfffc-(index//2%6))
 flags=[rng.randrange(2) for _ in range(rng.randrange(9))]
 if index<72:flags=[]
 for i,f in enumerate(flags):b[0x268+16*i]=(b[0x268+16*i]&254)|f
 w32(b,0x3a0,len(flags)-sum(flags));w32(b,0x3a8,sum(flags));w32(b,0x3a4,rng.randrange(-4,40))
 flag=bool(index%2);expected=normalize(b,flag)
 normal.append(dict(input=enc(b),sentinels=flag,expected=enc(expected)))
append=[]
for index in range(512):
 raw=bytearray(rng.randbytes(944));count=rng.randrange(8);w32(raw,0,count)
 root=[rng.randrange(191) for _ in range(rng.randrange(5))]
 current=root+[rng.choice([rng.randrange(191),rng.randrange(0xffeb,65536)]) for _ in range(rng.randrange(10))]
 previous=root+[rng.choice([rng.randrange(191),rng.randrange(0xffeb,65536)]) for _ in range(rng.randrange(7))]
 if index%5==0:previous=current[:rng.randrange(len(current)+1)]
 if index%7==0:current=[]
 row=bytearray(rng.randbytes(88));node=rng.randrange(0x30000);w16(row,24,node if index%3 else node+1)
 if index%4:row[5]&=127
 knowledge=rng.randrange(65536);special=rng.randrange(65536);flag=bool(index%2)
 out=bytearray(raw)
 if u16(row,24)!=node and row[5]&128:
  at=4+count*12;w16(out,at,knowledge);out[at+2]=0;w32(out,at+4,-1);w32(out,0,count+1)
 else:
  common=0
  for a,c in zip(current,previous):
   if a!=c:break
   common+=1
  delta=current[common:]
  for i,s in enumerate(delta):
   at=4+(count+i)*12;w16(out,at,s)
   kind=0
   if not(flag and sentinel(s)) and (((s+0x15)&65535)<0x13 or s in (special,65535)):
    kind=3;w32(out,0x3ac,i32(out,0x3ac)+1)
   out[at+2]=kind;w32(out,at+4,-1)
  w32(out,0,count+len(delta))
 append.append(dict(input=enc(raw),row=enc(row),node=node,current=current,previous=previous,knowledge=knowledge,special=special,sentinels=flag,expected=enc(out)))
Path(sys.argv[1]).write_text(json.dumps(dict(normalize=normal,append=append)),encoding='utf-8')
print('wrote1536 normalization +512 context-difference full944-byte controls')
