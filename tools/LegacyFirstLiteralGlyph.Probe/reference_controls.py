"""Independent pinned resource category parsing and fresh literal glyph flag oracle."""
import base64,json,pathlib,random,struct,sys
data=pathlib.Path(sys.argv[1]).read_bytes();p=11571;version,count=struct.unpack_from('<HH',data,p);p+=4;assert version==0x201 and count==192
def ids():
 global p
 n=data[p];p+=1;p=(p+3)&~3;values=struct.unpack_from('<'+'i'*n,data,p);p+=4*n;return values
firstids=ids();first=list(struct.unpack_from('<'+'b'*(count+3),data,p));p+=count+3
geoids=ids();start=list(struct.unpack_from('<'+'b'*(count+3),data,p));p+=count+3
end=list(struct.unpack_from('<'+'b'*(count+3),data,p));categories=[];lookup={};maps=[[] for _ in range(4)]
for label in range(count+3):
 for dest,(f,g) in zip(maps,[(first[label],start[label]),(first[label],end[label]),(0,start[label]),(0,end[label])]):
  key=(f,g)
  if f<0 or g<0:dest.append(-1);continue
  if key not in lookup:lookup[key]=len(categories);categories.append((firstids[f],g))
  dest.append(lookup[key])
header=struct.unpack_from('<8H',data,6219);assert header[3]==178
q=6219+16;entries=struct.unpack_from('<H',data,q)[0];q+=2
for increment in [32,64,1]:
 values=struct.unpack_from('<'+'H'*entries,data,q);q+=2*entries;entries=[v for v in values if v!=65535][-1]+increment
spans=struct.unpack_from('<'+'H'*(entries+1),data,q);q+=2*(entries+1)
stringslots=[struct.unpack_from('<II',data,q+i*8) for i in range(spans[-1]+1)];q+=8*(spans[-1]+1)
texts={stringslots[i][1]:data[q+stringslots[i][0]:q+stringslots[i+1][0]].decode('utf8') for i in range(len(stringslots)-1)}
replacement=next((label for label,text in texts.items() if text=='\ufffd'),65535)
r=random.Random(0x1f5250);cases=[]
for label in range(count+3):
 for primary in [0,9,17,33,34,63]:
  out=header[3] if label==count else label;cat=maps[0][out];geometry=0 if cat<0 else categories[cat][1]
  flags=0x48 if out==replacement else 0x40;mismatch=0 if out==header[3] or cat==primary else 8
  before=bytearray(r.randbytes(40));after=before[:];f4,f6=struct.unpack_from('<HH',before,4)
  struct.pack_into('<HHH',after,4,((f4&0xfff8)&0xfff7)|mismatch,f6|flags,out)
  cases.append(dict(label=label,primary=primary,published=out,category=cat,geometry=geometry,mismatch=mismatch,flags=flags,before=base64.b64encode(before).decode(),after=base64.b64encode(after).decode()))
target=pathlib.Path(sys.argv[2]);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(cases,separators=(',',':'))+'\n');print(f'{len(cases)} independent actual195-label literal plans/40-byte record controls')
