"""Read-only full-source FIRST B3 primary native bindings; never loads SDK."""
from pathlib import Path
import argparse,json,struct,hashlib
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser();p.add_argument('--binary',type=Path,required=True);p.add_argument('--pins',type=Path,default=Path(__file__).with_name('full-english-first-primary-pins.json'));p.add_argument('--output',type=Path,required=True);p.add_argument('--repo',type=Path,required=True);a=p.parse_args()
d=a.binary.read_bytes();e=json.loads(a.pins.read_bytes())['binaries']['MyScriptText.dll'];sha=lambda b:hashlib.sha256(b).hexdigest();assert sha(d)==e['sha256'];pe=struct.unpack_from('<I',d,0x3c)[0];assert d[pe:pe+4]==b'PE\0\0';sections=struct.unpack_from('<H',d,pe+6)[0];opt=pe+24;assert struct.unpack_from('<H',d,opt)[0]==0x20b;base=struct.unpack_from('<Q',d,opt+24)[0];s=opt+struct.unpack_from('<H',d,pe+20)[0];table=[]
for i in range(sections):
 q=s+40*i;size,rva,raw,off=struct.unpack_from('<IIII',d,q+8);table.append((rva,raw,off))
def offset(va):
 r=va-base
 return next(o+r-v for v,n,o in table if v<=r<v+n)
for r in e['regions']:
 lo,hi=int(r['start'],16),int(r['endExclusive'],16);assert sha(d[offset(lo):offset(hi)])==r['sha256']
for x in e['bindings']:assert struct.unpack_from('<Q',d,offset(int(x['at'],16)))[0]==int(x['target'],16)
cs=Cs(CS_ARCH_X86,CS_MODE_64)
for leaf in e['closedLeaves']:
 va=int(leaf['start'],16);hi=int(leaf['endExclusive'],16);data=d[offset(va):offset(hi)];assert data.hex()==leaf['bytes']and sha(data)==leaf['sha256'];instructions=list(cs.disasm(data,va));assert instructions[-1].mnemonic=='ret'and sum(i.size for i in instructions)==len(data)
for c in e['constants']:assert d[offset(int(c['at'],16)):offset(int(c['at'],16))+len(bytes.fromhex(c['bytes']))].hex()==c['bytes']
assert [struct.unpack('<f',bytes.fromhex(c['bytes']))[0]for c in e['constants']]==[1.,8192.,.5]
by={i['at']:i for i in e['instructions']}
for at,op in [('0x1801ee433','dword ptr [rbp - 0x48], r14d'),('0x1801ee690','dword ptr [rbp - 0x4c], ebx'),('0x1801ee6a4','word ptr [rbp - 0x42], cx'),('0x1801ee6b1','word ptr [rbp - 0x40], cx')]:assert by[at]['mnemonic']=='mov'and by[at]['operands']==op

for x in e['instructions']:
 va=int(x['at'],16);i=next(cs.disasm(d[offset(va):offset(va)+16],va));assert(bytes(i.bytes).hex(),i.mnemonic,i.op_str)==(x['bytes'],x['mnemonic'],x['operands'])
kernel=(a.repo/'src/InkRecognition/LegacyFirstPrimaryRecurrenceKernel.cs').read_text(encoding='utf-8-sig').replace('\r\n','\n');start=kernel.index('\n    {',kernel.index('internal static LegacyFirstOrdinaryAcousticRecurrence Run('));body=kernel[start:-3].replace('correct118','geometry.Correct118').replace('correct140','geometry.Correct140');assert sha(body.encode())==json.loads(a.pins.read_bytes())['sharedKernel']['normalizedBodySha256']
r=dict(status='PASS',fullFunctions=len(set(x['function']for x in e['regions'])),fullRegions=len(e['regions']),closedUnregisteredLeaves=len(e['closedLeaves']),bindings=len(e['bindings']),instructions=len(e['instructions']),vendorExecuted=False,scope='STATIC native-byte/pure-arithmetic proof only; no SDK execution/vendor ABI/real request or quality claim');a.output.write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r))
