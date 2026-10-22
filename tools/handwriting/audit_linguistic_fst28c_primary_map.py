"""STATIC pinned28C primary source-alphabet mapping; never loads native code.
No cursor/history/28C query result is invented by this projection.
"""
import argparse,hashlib,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
sha=lambda b:hashlib.sha256(b).hexdigest();pins=json.loads(Path(__file__).with_name('linguistic-fst28c-primary-map-pins.json').read_bytes())
readers={};functions=regions=tables=0
for module in pins['modules']:
 raw=(a.sdk/module['binary']).read_bytes();assert sha(raw)==module['sha256'];pe=struct.unpack_from('<I',raw,60)[0];op=pe+24;assert raw[pe:pe+4]==b'PE\0\0'and struct.unpack_from('<H',raw,op)[0]==0x20b
 base=struct.unpack_from('<Q',raw,op+24)[0];at=op+struct.unpack_from('<H',raw,pe+20)[0];sections=[]
 for i in range(struct.unpack_from('<H',raw,pe+6)[0]):_,v,n,f=struct.unpack_from('<IIII',raw,at+i*40+8);sections.append((v,n,f))
 def make_reader(raw,base,sections):
  def read(at,n):
   rva=at-base
   for v,size,f in sections:
    if v<=rva and 0<=n<=size-(rva-v):return raw[f+rva-v:f+rva-v+n]
   raise AssertionError(('PE bound',hex(at),n))
  return read
 read=make_reader(raw,base,sections);readers[module['binary']]=read
 for row in module['regions']:
  start=int(row['start'],16);end=int(row['endExclusive'],16);assert end-start==row['bytes']and sha(read(start,end-start))==row['sha256'];regions+=1
 functions+=len({r['function']for r in module['regions']})
 for row in module['tables']:assert read(int(row['at'],16),len(bytes.fromhex(row['bytes'])))==bytes.fromhex(row['bytes']);tables+=1
t=readers['MyScriptText.dll'];e=readers['MyScriptEngine.dll'];q=lambda read,at:struct.unpack('<Q',read(at,8))[0]
assert q(t,0x1802e9238+16)==0x1802e91b0 and q(t,0x1802e91b0+16)==0x18023dfe0
assert q(t,0x1802e9238+8)==0x1802e9270 and q(t,0x1802e9270+16)==0x18023e380
assert q(t,0x1802badf0+0x30)==0x1800ca630
assert[q(e,0x1801de1b0+i)for i in[0x28,0x30,0x38]]==[0x1800527e0,0x180052a80,0x180052650]

def alphabet(raw,start):
 # Version103 payload in hash-pinned roots, using the existing stored-reader
 # grammar. This is not a generic untrusted alphabet parser/admission API.
 pos=start
 def read(fmt,n=1):
  nonlocal pos
  result=struct.unpack_from('<'+fmt*n,raw,pos);pos+=struct.calcsize('<'+fmt)*n
  return result[0]if len(result)==1 else list(result)
 header=read('H',8);count=header[-1];n=read('H')
 for extra in[32,64,1]:
  values=read('H',n);values=[values]if n==1 else values;n=next(v+extra for v in reversed(values)if v!=65535)
 spans=read('H',n+1);entries=[read('II')for _ in range(spans[-1]+1)];strings=raw[pos:pos+entries[-1][0]];pos+=entries[-1][0]
 tokens={label&65535:strings[off:entries[i+1][0]]for i,(off,label)in enumerate(entries[:-1])};reverse=read('H',count)
 flag=read('B');pos+=(count+7)//8 if flag else 0;triplets=read('H');pos+=triplets*6
 if triplets:
  flags=read('I')
  for bit in[1,2,4]:pos+=count*(1 if triplets<128 else 2)if flags&bit else 0
 return dict(start=start,end=pos,header=header,count=count,tokens=tokens,reverse=reverse)
lk=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(a.models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IH',lk,299104)==(0x2b,0x103)and struct.unpack_from('<I',lk,299096)[0]==0x101
assert struct.unpack_from('<I',ak,5233)[0]==0x135
source=alphabet(lk,299110);acoustic=alphabet(ak,99)
assert(source['count'],source['end'],acoustic['count'],acoustic['end'])==(16,299573,192,5233)
assert all(x!=65535 for x in source['reverse'])and len(source['tokens'])==16
lookup={token:label for label,token in acoustic['tokens'].items()};mapping=[];details=[]
specials={b'\0':0xfffd,t(0x1802eabf8,1):0xfffb,t(0x1802eabf4,1):0xfff9,t(0x1802eb394,2):0xfffc,t(0x1802eb398,4):0xfffa,t(0x1802eb388,3):0xffff}
for label in range(source['count']):
 token=source['tokens'][label];virtual=source['reverse'][label]==65535
 if virtual:mapped=65535;branch='virtual'
 elif token in specials:mapped=specials[token];branch='special-before-lookup'
 else:mapped=lookup.get(token,65535);branch='exact-byte-lookup'
 mapping.append(mapped);details.append(dict(sourceLabel=label,rawToken=token.hex(),virtual=virtual,primaryCode=mapped,branch=branch))
assert mapping==[65533,0,1,2,3,4,5,6,7,8,9,65531,141,139,138,185]
assert lookup[b' ']==180 and mapping[11]!=lookup[b' ']and lookup[b'0']==0 and mapping[0]!=0
# Consumer proof projection ONLY:23F6F0 uses map[sourceLabel] after V80.
# Actual state0 special dispatch/history/paging are not executed here.
projected=[]
for state in range(98):
 first,last=struct.unpack_from('<I',lk,299588+state*8)[0],struct.unpack_from('<I',lk,299588+(state+1)*8)[0]
 for ordinal,at in enumerate(range(first,last)):
  target=struct.unpack_from('<I',lk,300384+at*4)[0];source_label=lk[299588+target*8+4];assert source_label<16
  projected.append(dict(state=state,ordinal=ordinal,target=target,sourceLabel=source_label,primaryCode=mapping[source_label]))
assert len(projected)==299 and projected[0]['target']==97 and projected[0]['primaryCode']==65533
report=dict(status='PASS STATIC primary-map prerequisite only',completeFunctions=functions,completeRegions=regions,tableAndLeafPins=tables,sourceAlphabetSpan=[299110,299573],sourceFlags68=257,acousticAlphabetSpan=[99,5233],normalizationFallbackFresh=False,sourceLabels=details,primaryMap=mapping,projectedActualRows=projected,primaryMapRawSHA256=sha(struct.pack('<16H',*mapping)),secondaryMapImplemented=False,cursorsImplemented=False,auxiliaryHistoryAssumedEmpty=False,nativeExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n');print(f'PASS STATIC {functions} fullfunctions/{regions} regions/{tables} data+leafpins; actual16 primary mappings/299 projected labels; specialNUL/space beforelookup; no28Cquery/history')
