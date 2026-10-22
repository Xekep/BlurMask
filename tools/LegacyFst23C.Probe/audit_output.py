"""Independent raw23C/109 storage oracle; no query or field20 semantics.

Actual descriptors/words come from model bytes. Procedural bytes are constructed
from the declared schema and fixed test specification, not the probe's values.
"""
import base64,hashlib,json,struct,sys
from pathlib import Path

if len(sys.argv)!=3:raise SystemExit('MODELS CAPTURE')
models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes());sha=lambda b:hashlib.sha256(b).hexdigest()
root=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes()
rootsha='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e';assert sha(root)==rootsha
assert struct.unpack_from('<IIIHH',root)==(0x101fe90,len(root)-8,0x13f,0,0x102)
assert struct.unpack_from('<i',root,76)[0]==-1
count,=struct.unpack_from('<I',root,88);assert count==22;at=92;children=[]
for i in range(count):
 magic,n,kind,flags,version=struct.unpack_from('<IIIHH',root,at);end=at+8+n;assert magic==0x101fe90 and n>=8 and flags==0 and end<=len(root)
 children.append((at,end,kind,version));at=(end+3)&~3 if i+1<count else end
assert at==len(root)and children[5]==(299000,301580,0x13a,0x202)
actual=root[299576:301580];assert struct.unpack_from('<HHIi',actual)==(0x109,16,98,299)

def procedural(labels,states,words,origin):
 # The inline block INCLUDES the version word. Header size is12, alignment is
 # relative to the absolute enclosing stream, not relative to local byte zero.
 start=(-origin-12)%4+12;at20=start+(states+1)*8;b=bytearray(at20+4+words*4)
 struct.pack_into('<HHIi',b,0,0x109,labels,states,words)
 for i in range(12,start):b[i]=0xa0+i
 for i in range(states+1):
  at=start+i*8;value=words if i==states else (0xf1230000+i*31337)&0xffffffff
  struct.pack_into('<I',b,at,value)
  if labels<255:struct.pack_into('<4B',b,at+4,255 if i%3==0 else 254 if i%3==1 else 13,0x80+i%128,0x21+i%200,0xe0+i%32)
  else:struct.pack_into('<HBB',b,at+4,65535 if i%3==0 else 255 if i%3==1 else 0x8001,0x80+i%128,0x21+i%200)
 struct.pack_into('<I',b,at20,0xc001d00d)
 for i in range(words):struct.pack_into('<I',b,at20+4+i*4,(0xdeadbeef+i*2654435761)&0xffffffff)
 return b

spec=[('actual-number',299576,actual),('generic-actual-bytes',299576,actual)]
for labels in [0,1,254,255,65535]:
 for origin in range(4):spec.append((f'procedural-{labels}-{origin}',origin,procedural(labels,3,5,origin)))
for origin in range(4):
 for words in [0,3]:spec.append((f'zero-states-{origin}-{words}',origin,procedural(255,0,words,origin)))
origin=2**31-1-64;spec.append(('high-origin',origin,procedural(254,1,1,origin)))
spec.append(('max-states',0,procedural(255,1048576,0,0)));spec.append(('max-words',0,procedural(255,0,4194304,0)))
assert capture['schema']==1 and len(capture['cases'])==len(spec)==capture['caseCount']==33
descriptor_count=word_count=0
for row,(name,origin,raw)in zip(capture['cases'],spec):
 assert row['name']==name and row['origin']==origin and row['end']==origin+len(raw)
 version,labels,states,words=struct.unpack_from('<HHIi',raw);start=12+(-origin-12)%4;at20=start+(states+1)*8;wordat=at20+4
 assert len(raw)==wordat+words*4
 assert(row['version'],row['labels'],row['states'],row['words'],row['field20'])==(version,labels,states,words,struct.unpack_from('<I',raw,at20)[0])
 assert row['fingerprint']==sha(raw)and row['length']==len(raw)and row['padding']==raw[12:start].hex()
 assert row['rootReceipt']==row['childReceipt']==(name=='actual-number')
 if name=='actual-number':assert(row['rootFingerprint'],row['childOrdinal'],row['childOffset'],row['childEnd'])==(rootsha,5,299000,301580)
 else:assert all(k not in row for k in ['rootFingerprint','childOrdinal','childOffset','childEnd'])
 small=len(raw)<=4096
 if small:assert base64.b64decode(row['inline'])==raw
 else:assert 'inline'not in row and row['descriptors']==[]and row['wordValues']==[]
 valuehash=hashlib.sha256()
 for i in range(states+1):
  at=start+i*8;field0,=struct.unpack_from('<I',raw,at)
  if labels<255:
   label,b6,b7,pad=struct.unpack_from('<4B',raw,at+4);label=65535 if label==255 else label
  else:label,b6,b7=struct.unpack_from('<HBB',raw,at+4)
  valuehash.update(struct.pack('<IHBB',field0,label,b6,b7))
  if small:assert row['descriptors'][i]==[field0,label,b6,b7,raw[at:at+8].hex()]
 assert row['descriptorValueSha']==valuehash.hexdigest()
 assert row['rawWordSha']==row['wordValueSha']==sha(raw[wordat:])
 if small:assert row['wordValues']==list(struct.unpack_from('<'+'I'*words,raw,wordat))
 assert row['callerAndOutputMutationSafe']and row['allGettersChecked']
 descriptor_count+=states+1;word_count+=words
assert descriptor_count==capture['descriptorGetters']==1048866 and word_count==capture['wordGetters']==4195015
expected=['distinct-actual-root-receipts','foreign-root-child','wrong-root-fingerprint','root-caller-mutation']
expected += ['version-'+str(v)for v in [0,0x108,0x10a]]
expected += ['truncated-'+str(i)for i in range(len(procedural(254,2,3,1)))]
expected += ['trailing']+['states-'+str(v)for v in [1048577,4294967295]]+['words-'+str(v)for v in [-1,-2147483648,4194305,2147483647]]
expected += ['states-incomplete-cap','words-incomplete-cap']+['sentinel-'+str(v)for v in [2,4,4294967295]]
expected += ['negative-origin','minimum-origin','origin-end-overflow','input-byte-budget','null-root','cancel-inline','cancel-malformed-preflight','cancel-actual','cancel-null-root-first']
for i in [-1,-2147483648,2147483647,3]:expected += ['descriptor-'+str(i),'descriptor-copy-'+str(i)]
expected += ['transition-'+str(i)for i in [-1,-2147483648,2147483647,3]]
expected += ['empty-transition','cancel-inline-copy','cancel-padding-copy','cancel-empty-padding-copy','cancel-words-copy','cancel-empty-words-copy']
assert capture['guards']==expected and len(expected)==101
assert capture['queryImplemented']is False and capture['field20SemanticsAssigned']is False
print(f'PASS independent33 layouts/101 guards; actual99 raw descriptors/299 words plus procedural254/255/65535 and absoluteorigins0..3, full{descriptor_count} descriptor/{word_count} word getter digests; raw field20/storage only')
