"""Independent raw-LK identifier decode and detached two-pass name-table oracle."""
import base64,hashlib,json,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('LK capture.json')
lk=Path(sys.argv[1]).read_bytes();c=json.loads(Path(sys.argv[2]).read_bytes())
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
start=606964
assert struct.unpack_from('<I',lk,start+8)[0]==0x13b and struct.unpack_from('<I',lk,start+80)[0]==0xffffffff
raw=bytes(v^b'\x14\x12\x19\x77'[i%4] for i,v in enumerate(lk[start+24:start+80]))
name,tail=raw.split(b'\0',1);assert not any(tail) and name==b'ru_RU-lk-text.lite.lex'
D=lambda s:None if s is None else base64.b64decode(s,validate=True)
def expected(sources):
 offsets=[-1]*(len(sources)+1);payload=bytearray()
 for i,s in enumerate(sources,1):
  if s is not None:offsets[i]=len(payload);payload.extend(s.split(b'\0')[0])
  payload.append(0)
 return offsets,bytes(payload)
def reads(sources):return [None,(sources[-1].split(b'\0')[0] if sources and sources[-1] is not None else None),None]+[s.split(b'\0')[0] if s is not None else None for s in sources]
checks=0
for row in c['prepared']:
 n=row['count'];sources=list(map(D,row['sources']));assert len(sources)==n
 offsets,payload=expected(sources);assert row['offsets']==offsets and D(row['payload'])==payload
 assert row['allocation']==row['reserved']==(n+1)*8+len(payload)+1
 indices=list(range(1,n))+([-1] if n else [])
 assert row['calls']==indices*2
 assert list(map(D,row['reads']))==reads(sources)
 checks+=1
assert len(c['prepared'])==512 and c['prepared'][-1]['count']==256
assert c['cancellations']==[[1,2,3,-1,1,2,3,-1][:n] for n in range(1,9)]
assert c['copyFailures']==c['cancellations']
word_count=0
for row in c['actual']:
 offsets,payload=expected([name,None]);assert row['count']==2 and row['offsets']==offsets and D(row['payload'])==payload and row['allocation']==24+len(payload)+1
 for word in row['words']:
  storage=D(word['storage']);assert len(storage)==120
  source_id=struct.unpack_from('<h',storage,48)[0];assert source_id==1 and D(word['name'])==name
  word_count+=1
assert len(c['actual'])==8 and word_count==8 and sum(r['nn'] for r in c['actual'])==631 and c['guards']==2109
print(f'PASS {checks} full two-pass tables/8 cancellation+8 failure prefixes/{word_count} actual WORD source IDs/raw LK name; no accepted text')
