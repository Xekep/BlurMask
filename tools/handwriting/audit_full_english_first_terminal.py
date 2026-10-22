"""STATIC full FIRST terminal proposal proof; SDK bytes only, never load or execute."""
from pathlib import Path
import argparse,hashlib,json,struct
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser();p.add_argument('--binary',type=Path,required=True);p.add_argument('--repo',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();e=json.loads(Path(__file__).with_name('full-english-first-terminal-pins.json').read_bytes());d=a.binary.read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest();assert sha(d)==e['binarySha256']=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pe=struct.unpack_from('<I',d,60)[0];s=pe+24+struct.unpack_from('<H',d,pe+20)[0];segments=[struct.unpack_from('<IIII',d,s+i*40+8)for i in range(struct.unpack_from('<H',d,pe+6)[0])]
def off(v):return next(o+v-0x180000000-r for z,r,n,o in segments if r<=v-0x180000000<r+n)
md=Cs(CS_ARCH_X86,CS_MODE_64)
for r in e['regions']+e['boundedWindows']:
 lo=int(r['start'],16);hi=int(r['endExclusive'],16);assert sha(d[off(lo):off(hi)])==r['sha256']
for r in e['instructions']:
 at=int(r['at'],16);i=next(md.disasm(d[off(at):off(at)+16],at));assert(i.bytes.hex(),i.mnemonic,i.op_str)==(r['bytes'],r['mnemonic'],r['operands'])
assert len(e['forwardedPropertyIds'])==19 and 0x12d007e not in e['forwardedPropertyIds']
for r in e['bindings']:assert struct.unpack_from('<Q',d,off(int(r['at'],16)))[0]==int(r['target'],16)
for name,h in e['sharedSources'].items():assert sha((a.repo/'src/InkRecognition'/name).read_text(encoding='utf-8-sig').encode())==h
r=dict(status='PASS',fullFunctions=len(set(x['function']for x in e['regions'])),fullRegions=len(e['regions']),boundedWindows=len(e['boundedWindows']),instructionPins=len(e['instructions']),bindings=len(e['bindings']),sharedSources=len(e['sharedSources']),vendorExecuted=False,scope='STATIC exact bytes plus existing pure source checks only; no native ABI/runtime terminal/WORD claim')
a.output.write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r))
