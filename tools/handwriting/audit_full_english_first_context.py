"""STATIC full selected249 context binding and shared-kernel proof; never loads SDK."""
from pathlib import Path
import argparse,json,hashlib,struct
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser();p.add_argument('--binary',type=Path,required=True);p.add_argument('--repo',type=Path,required=True);p.add_argument('--pins',type=Path,default=Path(__file__).with_name('full-english-first-context-pins.json'));p.add_argument('--output',type=Path,required=True);a=p.parse_args();e=json.loads(a.pins.read_bytes());d=a.binary.read_bytes();sha=lambda x:hashlib.sha256(x).hexdigest();assert sha(d)==e['binarySha256']=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pe=struct.unpack_from('<I',d,60)[0];assert d[pe:pe+4]==b'PE\0\0';s=pe+24+struct.unpack_from('<H',d,pe+20)[0];segments=[struct.unpack_from('<IIII',d,s+i*40+8)for i in range(struct.unpack_from('<H',d,pe+6)[0])]
def offset(v):return next(o+v-0x180000000-r for z,r,n,o in segments if r<=v-0x180000000<r+n)
cs=Cs(CS_ARCH_X86,CS_MODE_64)
for r in e['regions']:
 lo=int(r['start'],16);hi=int(r['endExclusive'],16);assert sha(d[offset(lo):offset(hi)])==r['sha256']
for x in e['closedUnregisteredLeaves']:
 lo=int(x['start'],16);hi=int(x['endExclusive'],16);raw=d[offset(lo):offset(hi)];assert raw.hex()==x['bytes']and sha(raw)==x['sha256'];ins=list(cs.disasm(raw,lo));assert len(ins)==x['instructions']and sum(i.size for i in ins)==len(raw)and ins[-1].mnemonic=='ret'and not any(i.mnemonic.startswith('j')or i.mnemonic=='call'for i in ins)
for x in e['instructions']:
 at=int(x['at'],16);i=next(cs.disasm(d[offset(at):offset(at)+16],at));assert(bytes(i.bytes).hex(),i.mnemonic,i.op_str)==(x['bytes'],x['mnemonic'],x['operands'])
for x in e['bindings']:assert struct.unpack_from('<Q',d,offset(int(x['at'],16)))[0]==int(x['target'],16)
for x in e['constants']:
 at=offset(int(x['at'],16));assert d[at:at+len(bytes.fromhex(x['bytes']))].hex()==x['bytes']
for name,expected in e['sharedSources'].items():assert sha((a.repo/'src/InkRecognition'/name).read_text(encoding='utf-8-sig').encode())==expected,('shared source changed',name)
k=(a.repo/'src/InkRecognition/LegacyFirstBlankContextKernel.cs').read_text(encoding='utf-8-sig');body=k[k.index('        var rows=Enumerable.Range(0,primary.NodeCount)'):k.rfind('\n    }')]
restore={'r.Symbol>blank+2':'r.Symbol>binding.Alphabet.LabelCount+2','nativeModeE0':'binding.NativeModeE0','excludeConsecutiveBlank':'producer.NativeConsecutiveBlankExclusion','r.Symbol==closed':'r.Symbol==literal.ClosedSymbolE4','glyphCost':'(preparedCost??producer.ReadGroupCost)','correct118':'geometry.Correct118','correct140':'geometry.Correct140'}
for x,y in restore.items():body=body.replace(x,y)
assert sha(body.encode())==e['normalizedContextKernelSha256']=='9e6f8543a7f696104598ad3426bee3d62d7e7dbb2a2acefa6a01840ba4cc5cc8'
report=dict(status='PASS',fullFunctions=len(set(x['function']for x in e['regions'])),fullRegions=len(e['regions']),closedUnregisteredLeaves=len(e['closedUnregisteredLeaves']),instructions=len(e['instructions']),dispatchBindings=len(e['bindings']),sharedSources=len(e['sharedSources']),vendorExecuted=False,scope='STATIC exact native bytes and current shared pure kernels; no B4 runtime/ABI/WORD/quality claim');a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
