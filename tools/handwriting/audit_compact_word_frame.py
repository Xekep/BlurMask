"""Pinned read-only207420/2075E0+AL layout audit; never executes vendor code."""
import argparse,hashlib,json,struct
from pathlib import Path
parser=argparse.ArgumentParser();parser.add_argument('output')
root=Path(__file__).resolve().parents[2]/'research/myscript/sdk/build/x64'
parser.add_argument('--text',default=str(root/'MyScriptText.dll'));parser.add_argument('--engine',default=str(root/'MyScriptEngine.dll'))
args=parser.parse_args()
def image(path,expected):
 b=Path(path).read_bytes();assert hashlib.sha256(b).hexdigest()==expected
 pe=struct.unpack_from('<I',b,0x3c)[0];count=struct.unpack_from('<H',b,pe+6)[0];head=pe+24+struct.unpack_from('<H',b,pe+20)[0]
 def raw(va,size):
  rva=va-0x180000000
  for i in range(count):
   start,length,offset=struct.unpack_from('<III',b,head+i*40+12)
   if start<=rva and rva+size<=start+length:return b[offset+rva-start:offset+rva-start+size]
  raise ValueError(hex(va))
 return raw
text=image(args.text,'18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f')
engine=image(args.engine,'d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c')
regions=[(0x180207420,0x1802075e0,'179a4f862dc90d5d6019d21276ed76f971a585d1b32c311d71bbed2ad50f1a91'),
 (0x1802075e0,0x180207780,'0ab2ee126c0fc9311ca58990bf916706eb6a3b811061f043ee6987b39dc3ac08')]
for start,end,sha in regions:assert hashlib.sha256(text(start,end-start)).hexdigest()==sha
for at,value in [(0x180053f57,'4c8d4308'),(0x180053f71,'4c8d430a'),(0x180053f9d,'4c8d430c'),
 (0x180053fc9,'4c8d430e'),(0x180053ff5,'4c8d4310'),(0x180054021,'4c8d4312'),(0x18005404d,'4c8d4314'),
 (0x180054079,'488d731a'),(0x180052790,'0fb7421ac3')]:assert engine(at,len(value)//2).hex()==value
assert struct.unpack('<Q',engine(0x1801de1b0+0x10,8))[0]==0x180052790
assert text(0x18020c1ec,4).hex()=='488b7060' and text(0x18020c21d,4).hex()=='48897328'
assert text(0x1802eaa80,3)==bytes.fromhex('e2808c')
assert text(0x1802e9b70,1)==b"'" and text(0x1802e9b68,1)==b'"'
report=dict(fullFunctions=[dict(start=hex(s),end=hex(e),sha256=h) for s,e,h in regions],
 headerRuntimeOffsets=[8,10,12,14,16,18,20,26],knowledgeSerializedIndex=3,
 labelCountGetter='052790 reads1A',descriptorAlphabetPointer='20C1EC +60 ->20C21D controller28',
 literals24BA="'",literal24BC='"',literal24DE='U+200C',scope='prepared ordinary2075E0 only; not207CB0 ownership/backtrack')
Path(args.output).write_text(json.dumps(report,indent=2),encoding='utf-8')
print('PASS2 fullfunction hashes +9 Engine instruction pins/ALcountVT +2 ALowner pins +3 literal pins')
