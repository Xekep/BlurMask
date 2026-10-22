from pathlib import Path
import struct,hashlib,json,argparse
p=argparse.ArgumentParser();p.add_argument('models',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
b=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();assert hashlib.sha256(b).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657';start=184155;end=545680;at=start;fields=[];values={};networks=[]
def take(n,name):
 global at
 assert 0<=n<=end-at,(name,at,n);lo=at;at+=n;raw=b[lo:at];fields.append(dict(name=name,offset=lo,end=at,sha256=hashlib.sha256(raw).hexdigest()));return raw
def scalar(fmt,name):
 v=struct.unpack('<'+fmt,take(struct.calcsize(fmt),name))[0];values[name]=v;return v
def flag(name):
 v=scalar('B',name);assert v in [0,1],(name,v,at);return v
magic,size,kind,enc,version=struct.unpack('<IIIHH',take(16,'header'));assert(magic,kind,enc,version,start+size+8)==(0x101fe90,0x21d,0,0x119,end)
def network(name):
 lo=at;h=take(8,name+'.header');magic,v,length=struct.unpack('<HHI',h);assert magic==0xfea0 and v==0x107 and 3<=length<=1024*1024
 raw=take(length,name+'.body');networks.append(dict(name=name,start=lo,end=at,version=v,sha256=hashlib.sha256(h+raw).hexdigest()))
def shorts(name,copies=1):
 n=scalar('H',name+'.count');assert n<=4096;take(n*2*copies,name+'.values');return n
scalar('B','8');scalar('B','9');scalar('H','A');scalar('f','C.costMultiplier')
if flag('40.present'):
 network('40.initial')
 for name,fmt in [('48','f'),('4C','f'),('50','f'),('54','B'),('55','B'),('56','H'),('58','f'),('5C','B'),('60','B'),('64','B'),('65','B')]:scalar(fmt,name)
if flag('10.dynamic.present'):
 scalar('H','14');
 for x in range(0x18,0x39,4):scalar('f',hex(x)[2:])
count=scalar('B','68.profileCount');assert count==1
for p in range(count):
 name='70.'+str(p)
 assert flag(name+'.grid')==0
 take(2+4+5*2+7*4,name+'.14..44')
 for suffix,copies in [('48',1),('58',2),('70',2)]:shorts(name+'.'+suffix,copies)
 take(2+2+4+4+2+2,name+'.88..9E')
 assert flag(name+'.A0.nested')==0
 if flag(name+'.A8.present'):shorts(name+'.A8',2)
 for slot in range(7):
  n=name+'.slot'+str(slot)
  if flag(n+'.map.present'):shorts(n+'.map',2)
  flag(n+'.first');flag(n+'.second');mode=scalar('B',n+'.mode');assert mode<=2
  if mode==0:scalar('f',n+'.weight')
  for k in range(3):
   if flag(n+'.network'+str(k)+'.present'):network(n+'.network'+str(k))
scalar('B','78');scalar('f','7C');scalar('B','80');scalar('f','88');scalar('f','84');lists=scalar('H','90.listCount');assert lists<=512
for i in range(lists):shorts('98.'+str(i))
#Version119 omits legacyA0 (only versions<116).
for x in range(0xa4,0xb5,4):scalar('f',hex(x)[2:])
scalar('B','B8');scalar('f','BC');scalar('f','C0')
#Obsolete nested flag is read only10C..113, absent119.
groups=scalar('h','C8.groupCount');assert 0<=groups<=512
if groups:
 offsets=struct.unpack('<'+'H'*(groups+1),take((groups+1)*2,'D0.groupOffsets'));assert offsets[0]==0 and all(x<=y for x,y in zip(offsets,offsets[1:]))
 symbols=struct.unpack('<'+'H'*offsets[-1],take(offsets[-1]*2,'D8.groupSymbols'));assert all(x<217 for x in symbols)
 weights=struct.unpack('<'+'f'*groups,take(groups*4,'E0.groupWeights'))
else:offsets=[0];symbols=[];weights=[]
groupEnd=at
scalar('i','E8');scalar('H','EC');scalar('H','EE')
for name in ['F0','F4','F8']:scalar('i',name)
scalar('B','FC');scalar('f','100');assert scalar('i','108')==0
#119 skips obsolete pre119 conversion flag; no pending-grid payload exists.
assert scalar('i','128')==0;assert flag('148/150.present')==0
scalar('f','120');scalar('f','124');assert at==end,('complete119end',at,end)
#Independent full107 matrix boundaries, preserving original stream alignment.
for n in networks:
 lo=n['start'];magic,v,length=struct.unpack_from('<HHI',b,lo);assert b[lo+8]==0x12
 kind=struct.unpack_from('<H',b,lo+9)[0];inputs,hidden,zero1,zero2,outputs=struct.unpack_from('<IIIII',b,lo+11);assert zero1==zero2==0 and struct.unpack_from('<H',b,lo+31)[0]==10
 cursor=lo+33;matrices=[]
 for columns,rows in [(inputs,hidden-1),(hidden,outputs-1)]:
  cursor=(cursor+15)&~15;stride=(columns+15)&~15;scales=cursor+stride*rows;assert b[scales]==1 and all(x!=0 for x in b[scales+1:scales+1+rows]);matrices.append(dict(weights=cursor,scales=scales,columns=columns,stride=stride,rows=rows));cursor=scales+rows+1
 assert cursor==n['end'];n.update(kind=kind,inputWidth=inputs,hiddenWidth=hidden-1,outputWidth=outputs-1,matrices=matrices)
result=dict(scope='STATIC complete actual21D119 wire partition +mask group ordinal rows; network bytes parsed but not evaluated',sourceStart=start,sourceEnd=end,consumedEnd=at,groupEnd=groupEnd,resourceSha256=hashlib.sha256(b).hexdigest(),fields=fields,values=values,networks=networks,groupOffsets=offsets,groupSymbols=symbols,groupWeights=weights,groups=[dict(ordinal=i,weight=weights[i],symbols=symbols[offsets[i]:offsets[i+1]])for i in range(groups)],nativeExecuted=False)

#Independent bounded raw alphabet reader: preserve reverse/virtual flags,
#case-related triplets and final metadata separately from contextual arrays88.
def alphabet(raw,start,limit):
 pos=start
 def get(fmt,n=1):
  nonlocal pos
  size=struct.calcsize('<'+fmt)*n;assert 0<=n<=65535 and pos+size<=limit
  v=struct.unpack_from('<'+fmt*n,raw,pos);pos+=size;return v
 header=get('H',8);count=header[-1];n=get('H')[0]
 for step in [32,64,1]:
  table=get('H',n);n=[v+step for v in table if v!=65535][-1]
 spans=get('H',n+1);entries=[get('II')for _ in range(spans[-1]+1)];length=entries[-1][0];assert pos+length<=limit;text=raw[pos:pos+length];pos+=length
 mapping={label&65535:text[lo:hi].decode('utf8')for(lo,label),(hi,_)in zip(entries,entries[1:])};reverse=get('H',count);bitFlag=get('B')[0]
 if bitFlag:get('B',(count+7)//8)
 related=get('H')[0];triplets=[get('H',3)for _ in range(related)];options=get('I')[0]if related else 0
 maps={}
 for bit in [1,2,4]:
  if options&bit:maps[bit]=get('b'if related<128 else'h',count)
 final=get('B')[0];assert pos==limit
 return dict(count=count,header=header,text=mapping,reverse=reverse,relatedTriplets=related,relationFlags=options,finalMetadata=final,triplets=triplets,maps=maps)
lk=(a.models/'en-US/en_US-lk-text.lite.res').read_bytes();assert hashlib.sha256(lk).hexdigest()=='9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9'
ac=alphabet(b,99,7907);lex=alphabet(lk,294,2568);assert(ac['count'],lex['count'])==(217,104)
reverse={text:label for label,text in ac['text'].items()};primary=[];auxiliary=[]
for label in range(104):
 assert lex['reverse'][label]!=65535 #actual owner needs no allocator residue
 text=lex['text'][label]
 value=0xfffb if text==' 'else reverse.get(text,65535)
 primary.append(value);auxiliary.append(value if value>=217 or text in ['\u200c','\u200d']else 65535)
contexts=sorted(set(v for v in primary if v not in [65535,65533]));assert len(contexts)==86
#Verify complete pinned script-data checksum before the Arabic/contextual branch.
pub=json.loads((a.models/'text-unicode/legacy-text-publication-data.json').read_bytes());assert pub['tableSha256']=='dd431ff067120a92b87cb35cb4d34253516740e4e0e9fedf841f8d258317edcc'
h=hashlib.sha256()
def hi(v):h.update(struct.pack('<i',v))
for name in ['scriptRanges','controlRanges','extendRanges','prependRanges','joinerRanges']:
 rows=pub[name];hi(len(rows))
 for row in rows:
  for v in row:hi(v)
hi(len(pub['literalRows']))
for row in pub['literalRows']:
 v=bytes.fromhex(row[0]);hi(len(v));h.update(v)
 for i in row[1:]:hi(i)
assert h.hexdigest()==pub['tableSha256']
def script(cp):
 for lo,hi,value in pub['scriptRanges']:
  if lo<=cp<=hi:return value
 return 0
arabic=[]
for label,text in ac['text'].items():
 last=1;eligible=bool(text)
 for char in text:
  cp=ord(char)
  if 0x660<=cp<0x66a or 0x6f0<=cp<0x6fa:eligible=False;break
  value=script(cp)
  if value!=1:last=value
  if last!=5:eligible=False;break
 if eligible:arabic.append(label)
assert not arabic
#No contextual88 array is needed: the native Arabic detector finds no candidate.
#Wide eligibility restrictions are immaterial when no wide scalar has a base.
wide=[]
for label,text in sorted(ac['text'].items()):
 if len(text)!=1:continue
 cp=ord(text);base=cp-0xfee0 if 0xff01<=cp<=0xff5e else {0x3000:0x20,0xff5f:0x2985,0xff60:0x2986,0xffe0:0xa2,0xffe1:0xa3,0xffe2:0xac,0xffe3:0xaf,0xffe4:0xa6,0xffe5:0xa5,0xffe6:0x20a9}.get(cp)
 if base is not None and chr(base)in reverse:wide.append((reverse[chr(base)],label))
assert not wide
caseTokens=[]
for label in primary:
 if label>=217 or ac['maps'][1][label]<0:caseTokens.append(label);continue
 forms=list(ac['triplets'][ac['maps'][1][label]])
 if label not in forms:
  if forms[0]==65535:forms[0]=label
  elif forms[1]==65535:forms[1]=label
 #Only actual map0 is stored. Native mode1/2 both fall back to it.
 caseTokens.extend([v for v in forms if v!=65535]*3)
assert len(caseTokens)==544
seed=[int(i in caseTokens)for i in range(217)];first=seed.copy();second=[1]*217
for group in result['groups']:
 if group['weight']>0 and any(first[i]for i in group['symbols']):
  for i in group['symbols']:first[i]=1
for text in ['-','\u00ad']:
 if text in reverse:first[reverse[text]]=1
virtual=[i for i,v in enumerate(ac['reverse'])if v==65535]
for i in virtual:first[i]=second[i]=0
header=ac['header'];dash=['-','_','\U000f0002','\U000f0003','\u30fc','\u4e00']
for mask in [first,second]:
 if header[1]!=65535:mask[header[1]]=int(any(mask[reverse[t]]for t in dash if t in reverse))
 if header[2]!=65535:mask[header[2]]=int('\U000f0001'in reverse and mask[reverse['\U000f0001']])
 for i in [3,0,4,5,6]:
  if header[i]!=65535:mask[header[i]]=1
#The native fresh selected-node80=1 child count is the packed raw count at1.
countBits=lk[669176:1252408];assert((int.from_bytes(countBits[:2],'little')>>7)&127)==0
order=struct.unpack_from('<I',lk,1252514)[0];priority=struct.unpack_from('<I',lk,1252530)[0];assert(order,priority)==(3,1024)
result['sourceMaps']=dict(primary=primary,auxiliary=auxiliary,contextMapNonzero=[v not in [65535,65533]for v in primary],contextKeys=contexts,contextCount=86,virtualLexical=[],absentExactPrimary=sum(v==65535 for v in primary),descriptor=0xc00|2|(order*4),order=order,selectedPriority=priority)
result['caseTokens']=caseTokens
result['maskDiagnostic']=dict(scope='PREPARED exact resource rows under explicit independent main13B-only/non-bypassed policy; not native configured fullroot owner',seed=seed,primary=first,secondary=second,seedEnabled=sum(seed),primaryEnabled=sum(first),secondaryEnabled=sum(second),virtualAcoustic=virtual,arabicCandidateLabels=arabic,widePairs=wide,scriptDataTableSha256=pub['tableSha256'])
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(dict(start=start,end=end,consumed=at,groups=groups,symbols=len(symbols),networks=len(networks),contextCount=86,descriptor=hex(result['sourceMaps']['descriptor']),seed=sum(seed),primary=sum(first),secondary=sum(second),virtual=len(virtual))))
