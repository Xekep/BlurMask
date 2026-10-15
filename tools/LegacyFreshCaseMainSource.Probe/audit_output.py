"""Independent wire/source-type routing and immutable actual-cache audit."""
import hashlib,json,struct,sys
from pathlib import Path
if len(sys.argv) not in (4,5):raise ValueError('AK LK capture [independently audited case-span capture]')
ak,lk=map(lambda p:Path(p).read_bytes(),sys.argv[1:3]);got=json.loads(Path(sys.argv[3]).read_text(encoding='utf8'))
assert got['acoustic'].lower()==hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert got['lexical'].lower()==hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
word=lk[606964:4565174];magic,length,kind,version=struct.unpack_from('<4I',word)
assert magic==0x0101fe90 and length==len(word)-8 and kind==0x13b and version==0x03000000
assert struct.unpack_from('<2H',word,16)==(0x301,0x100)
name=bytes(v^[20,18,25,119][i&3] for i,v in enumerate(word[24:80])).split(b'\0')[0].decode('utf8')
assert name==got['identifier']=='ru_RU-lk-text.lite.lex'
order=struct.unpack_from('<I',lk,2513123+90)[0]
assert got['flags']&0x1e00==0xc00 and got['flags']&0x1c==order*4 and got['flags']&2
#Native descriptors are separately pinned by audit_fresh_case_main_source.py.
#The associated13D is a sibling of27B, not a parent of selected13B.
parents={0x13b:0x139,0x139:0x27b,0x27b:0x138,0x13d:0x138}
chain=[];current=kind
while current in parents:chain.append(current);current=parents[current]
assert 0x27b in chain and 0x13d not in chain
configured=[];associated=None
for selected in [kind]:
 if selected in chain and 0x27b in chain:configured.append(selected) #first native branch
 elif selected==0x13d:associated=selected
assert got['count']==len(configured)==1 and got['type']==configured[0] and got['associated']==(associated is not None)==False
assert got['v60'] is False
truth=[(associated is not None or got['v60']) and prior and not skip for prior in (False,True) for skip in (False,True)]
assert not any(truth) and got['secondary'] is False and got['gateCombinations']==len(truth)==4
assert got['nodes']==4 and got['groups']==len(got['scores'])==10 and all(len(row)==195 for row in got['scores']) and got['guards']==8
if len(sys.argv)==5:
 old=json.loads(Path(sys.argv[4]).read_text(encoding='utf8'))
 actual=sorted(tuple(row) for row in got['scores']);expected=sorted(tuple(row) for scope in old['spans'] for row in scope['costs'])
 assert actual==expected,'Source policy cache rows differ from independently audited actual case span scores'
print('PASS independent actual13B wire/inheritance/27B-first source routing/no13D/noAssociated/V60 gate; actual10x195 cache rows and8 ownership guards')
