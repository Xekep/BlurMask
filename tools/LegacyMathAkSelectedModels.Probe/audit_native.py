from pathlib import Path
import hashlib,struct,json,sys
repo=Path(sys.argv[1]);out=Path(sys.argv[2]);H=lambda b:hashlib.sha256(b).hexdigest()
pe=(repo/'research/myscript/sdk/build/x64/MyScript2D.dll').read_bytes();p=struct.unpack_from('<I',pe,60)[0];n=struct.unpack_from('<H',pe,p+6)[0];op=struct.unpack_from('<H',pe,p+20)[0];sec=p+24+op
assert H(pe)=='c633012619b4db77a73de117913d4d1eac6f5c340156bfe054f246cfe3701afd'
def read(rva,size):
 for i in range(n):
  a=sec+40*i;virtual,addr,rawsz,raw=struct.unpack_from('<IIII',pe,a+8)
  if addr<=rva and rva+size<=addr+rawsz:return pe[raw+rva-addr:raw+rva-addr+size]
 raise AssertionError('region')
regions=[dict(va=hex(0x180000000+r),bytes=size,sha256=H(read(r,size)))for r,size in [(0x170b0,826),(0x18970,760)]]
b=(repo/'models/math/math-ak.res').read_bytes();assert H(b)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
c=1007733;slots=[]
for i in range(7):
 start=c;present=b[c];c+=1
 if present:
  count=struct.unpack_from('<H',b,c)[0];c+=2+4*count
 else:count=0
 flags=list(b[c:c+2]);c+=2;mode=b[c];c+=1
 if mode==0:c+=4
 nets=[]
 for choice in range(3):
  exists=b[c];c+=1
  if exists:
   assert b[c:c+4]==bytes.fromhex('a0fe0701');end=c+8+struct.unpack_from('<I',b,c+4)[0]
   nets.append([choice,c,end]);c=end
 slots.append(dict(start=start,end=c,calibrationCount=count,flags=flags,mode=mode,networks=nets))
assert [x['networks'][0][1]for x in slots[:5]]==[1007739,1151097,1289175,1459255,1629111]
assert all(x['mode']==2 and x['flags']==[0,1] and len(x['networks'])==1 and x['networks'][0][0]==1 for x in slots[:5])
assert all(not x['networks'] and x['mode']==0 for x in slots[5:]);assert c==1772489
out.write_text(json.dumps(dict(regions=regions,slots=slots,selectedModels=5,rawModels=6,modelExecutions=0),indent=2)+'\n')
print('PASS 2 complete native regions / 7 source slots / 5 selected models / 0NN')
