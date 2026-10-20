"""Independent actual EN2B4/v102 bit/BFS/roundtrip reference. STATIC data only.
No expected word input, vendor imports or normalization/header coercion.
"""
from pathlib import Path
import argparse,array,hashlib,json,struct
p=argparse.ArgumentParser();p.add_argument('models',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
d=(a.models/'en-US/en_US-lk-text.lite.res').read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest()
assert len(d)==3300812 and sha(d)=='9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9'
def header(at,kind,version,end):
 magic,size,k,e,v=struct.unpack_from('<IIIHH',d,at);assert(magic,k,e,v)==(0x101fe90,kind,0,version)and at+8+size==end
header(0,0x13f,0x102,len(d));header(184,0x13b,0x300,2832716);header(2572,0x2b4,0x102,1252412);header(1252412,0x2d2,0x101,2832716)
assert struct.unpack_from('<I',d,88)[0]==21
children=[];at=92
for i in range(21):
 magic,size,kind,enc,version=struct.unpack_from('<IIIHH',d,at);end=at+8+size;assert magic==0x101fe90 and enc==0 and end<=len(d)
 children.append(dict(offset=at,end=end,kind=kind,version=version));at=(end+3)&~3 if i<20 else end
assert at==len(d) and [c['kind']for c in children]==[0x13d,0x13b]+[0x13a]*19
flags,n,maxLength=struct.unpack_from('<III',d,2588);assert(flags,n,maxLength)==(0,666547,43)
def packed(at):
 start=at;version=struct.unpack_from('<I',d,at)[0];assert version==0x1000001;at+=4
 #Stream28 computes padding; Stream30(mode1) advances by that amount.
 at=(at+3)&~3;count=struct.unpack_from('<I',d,at)[0];width=d[at+4];at+=5
 metadata=d[at];at+=1;assert metadata==1 and count==n and 1<=width<=17
 at=(at+3)&~3;size=((count*width+31)//32)*4;raw=d[at:at+size];assert len(raw)==size
 mask=(1<<width)-1;values=array.array('I')
 #Independent byte-window extraction, not a port of native word cursor.
 for i in range(count):
  bit=i*width;q=bit//8;shift=bit%8;k=(shift+width+7)//8
  values.append((int.from_bytes(raw[q:q+k],'little')>>shift)&mask)
 #Independent32-bit boundary extraction models actual bulk-query storage.
 words=struct.unpack('<'+'I'*(size//4),raw)
 for i,value in enumerate(values):
  bit=i*width;index=bit//32;shift=bit%32;x=words[index]>>shift
  if shift+width>32:x|=words[index+1]<<(32-shift)
  assert x&mask==value
 #Inverse packing must preserve EVERY word, including unused tail bits.
 emitted=bytearray(size)
 for i,value in enumerate(values):
  bit=i*width;q=bit//8;shift=bit%8;x=value<<shift
  for j in range((shift+width+7)//8):emitted[q+j]|=(x>>(8*j))&255
 assert bytes(emitted)==raw
 return dict(start=start,end=at+size,version=version,count=count,width=width,metadata=metadata,dataStart=at,prefixHex=d[start:at].hex(),sha256=sha(raw),roundtripBytes=size),values
labelInfo,labels=packed(2600);offsetWidth=d[labelInfo['end']];countInfo,counts=packed(labelInfo['end']+1)
assert(labelInfo['width'],countInfo['width'],offsetWidth)==(8,7,17)
end=countInfo['end'];storedTerminals=struct.unpack_from('<I',d,end)[0];assert end+4==1252412 and storedTerminals==260074
terminalBit=1<<(labelInfo['width']-1);labelMask=terminalBit-1
starts=array.array('I',[0])*n;depth=bytearray(n);next=1;terminals=0;maxChildren=maxDelta=observedDepth=0
assert labels[0]==0
for i,count in enumerate(counts):
 assert next>i and next+count<=n and next-i<(1<<offsetWidth)
 starts[i]=next;previous=-1
 for child in range(next,next+count):
  label=labels[child]&labelMask;assert previous<label<104;previous=label
  depth[child]=depth[i]+1;observedDepth=max(observedDepth,depth[child])
 terminals+=bool(labels[i]&terminalBit);maxChildren=max(maxChildren,count);maxDelta=max(maxDelta,next-i);next+=count
assert next==n and terminals==storedTerminals and observedDepth==maxLength
#Visit every terminal in dictionary child-order; retain only a streaming digest.
wordHash=hashlib.sha256();path=[];stack=[(0,0)];observed=0
while stack:
 node,offset=stack.pop()
 if offset==0 and node:
  path.append(labels[node]&labelMask)
  if labels[node]&terminalBit:
   wordHash.update(struct.pack('<H',len(path)));wordHash.update(struct.pack('<'+'H'*len(path),*path));observed+=1
 if offset<counts[node]:stack.append((node,offset+1));stack.append((starts[node]+offset,0))
 elif node:path.pop()
assert observed==storedTerminals and not path
#Native dictionary queries skip raw root child1; ordinary other nodes retain
#their complete BFS child interval. These checks do not supply linguistic maps.
queries=[]
for node in sorted({0,1,2,3,n-1,*range(0,n,max(1,n//64))}):
 start=starts[node]+(node==0);stop=starts[node]+counts[node];rows=[(i,labels[i]&labelMask,bool(labels[i]&terminalBit))for i in range(start,stop)]
 queries.append(dict(node=node,start=start,stop=stop,count=len(rows),rowsSha256=sha(b''.join(struct.pack('<IHB',*x)for x in rows))))
#Independent bounded UTF8 alphabet payload reader, data mapping only.
def alphabet(raw,start,limit):
 at=start
 def get(fmt,n=1):
  nonlocal at
  size=struct.calcsize('<'+fmt)*n;assert 0<=n<=65535 and at+size<=limit
  value=struct.unpack_from('<'+fmt*n,raw,at);at+=size;return value
 fields=get('H',8);count=fields[-1];n=get('H')[0]
 for increment in [32,64,1]:
  values=get('H',n);n=[v+increment for v in values if v!=65535][-1]
 spans=get('H',n+1);entries=[get('II')for _ in range(spans[-1]+1)];length=entries[-1][0];assert at+length<=limit
 text=raw[at:at+length];at+=length;mapping={}
 for (begin,label),(end,_)in zip(entries,entries[1:]):assert begin<=end<=length;mapping[label&65535]=text[begin:end].decode('utf8')
 get('H',count);hasBits=get('B')[0]
 if hasBits:get('B',(count+7)//8)
 related=get('H')[0];get('H',related*3);options=get('I')[0]if related else 0
 for bit in [1,2,4]:
  if options&bit:get('B'if related<128 else'H',count)
 final=get('B')[0];return dict(start=start,end=at,count=count,mapping=mapping,hasBits=hasBits,related=related,options=options,final=final)
lexical=alphabet(d,294,2568);ak=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();assert sha(ak)=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
acoustic=alphabet(ak,99,7907);assert(lexical['count'],lexical['end'],acoustic['count'],acoustic['end'])==(104,2568,217,7907)
reverse={text:label for label,text in acoustic['mapping'].items()};missing={label:text for label,text in lexical['mapping'].items()if text not in reverse};assert len(missing)==18
mapping=[dict(lexical=label,text=text,exactAcoustic=reverse.get(text))for label,text in sorted(lexical['mapping'].items())]
result=dict(scope='STATIC actual packed dictionary ONLY; no English LK selection/masks/language owner or handwriting accuracy claim',resourceSha256=sha(d),serializedRootChildren=children,labels=labelInfo,counts=countInfo,nodeCount=n,storedTerminals=storedTerminals,maximumWordLength=maxLength,observedDepth=observedDepth,maximumChildren=maxChildren,maximumChildDelta=maxDelta,offsetWidth=offsetWidth,wordLabelPreorderSha256=wordHash.hexdigest(),queries=queries,packedRoundtripBytes=labelInfo['roundtripBytes']+countInfo['roundtripBytes'],alphabetData=dict(lexicalCount=104,acousticCount=217,exactTextMapping=mapping,unmatchedLexical=missing,caseOrNormalizationOwner=False),nativeExecuted=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n',encoding='utf8');print('PASS full actual EN2B4/v102:',n,'nodes/',storedTerminals,'terminals; two independent full unpack passes, byte-exact pack roundtrip and full terminal enumeration')
