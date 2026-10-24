"""STATIC exact full-FIRST rich WORD producer/dispatch proof. Never loads SDK."""
from pathlib import Path
import argparse,hashlib,json,struct
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser();p.add_argument('--binary',type=Path,required=True);p.add_argument('--repo',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();e=json.loads(Path(__file__).with_name('full-english-first-plain-text-pins.json').read_bytes());d=a.binary.read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest();assert sha(d)==e['binarySha256']=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pe=struct.unpack_from('<I',d,60)[0];s=pe+24+struct.unpack_from('<H',d,pe+20)[0];ss=[struct.unpack_from('<IIII',d,s+i*40+8)for i in range(struct.unpack_from('<H',d,pe+6)[0])]
def off(v):return next(o+v-0x180000000-r for z,r,n,o in ss if r<=v-0x180000000<r+n)
md=Cs(CS_ARCH_X86,CS_MODE_64);boundaries={}
for r in e['regions']:
 lo=int(r['start'],16);hi=int(r['endExclusive'],16);b=d[off(lo):off(hi)];assert sha(b)==r['sha256']
 for i in md.disasm(b,lo):boundaries[i.address]=i
closedInstructions=0
for r in e['closedUnregistered']:
 lo=int(r['start'],16);hi=int(r['endExclusive'],16);b=d[off(lo):off(hi)];assert sha(b)==r['sha256'];ins=list(md.disasm(b,lo));assert sum(i.size for i in ins)==len(b);assert [dict(at=hex(i.address),bytes=i.bytes.hex(),mnemonic=i.mnemonic,operands=i.op_str)for i in ins]==r['instructions'];closedInstructions+=len(ins)
 if 'direct-CFG'in r['kind']:
  assert ins[-1].mnemonic=='ret'and not any(i.mnemonic=='call'for i in ins);addresses={i.address for i in ins}
  for i in ins:
   if i.mnemonic.startswith('j'):assert int(i.op_str,16)in addresses
 elif 'tail-dispatch'in r['kind']:assert ins[-1].mnemonic=='jmp'
 else:assert ins[-1].mnemonic=='ret'and not any(i.mnemonic.startswith('j')or i.mnemonic=='call'for i in ins)
for x in e['instructions']:
 at=int(x['at'],16);i=boundaries[at];assert(i.bytes.hex(),i.mnemonic,i.op_str)==(x['bytes'],x['mnemonic'],x['operands'])
for x in e['bindings']:assert struct.unpack_from('<Q',d,off(int(x['at'],16)))[0]==int(x['target'],16)
for x in e['constants']:
 at=off(int(x['at'],16));assert d[at:at+len(bytes.fromhex(x['bytes']))].hex()==x['bytes']
def restored_source(repo,path,expected):
 import runpy
 helper=repo/'tools/LegacyEnglishCharacters.Probe/source_preimage.py'
 assert hashlib.sha256(helper.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='3839cc530b618f397eb1757f02455f124053561f3ab8f1f77a46d391e4b76d12'
 return runpy.run_path(str(helper))['restore'](Path(path).name,(repo/path).read_bytes(),expected)
for name,h in e['sharedSources'].items():
 data=(a.repo/'src/InkRecognition'/name).read_text(encoding='utf-8-sig').encode()
 if name in ('LegacyOrdinaryCursiveSeedGraph.cs','LegacyNativeWordBuilder.cs','LegacyNativePlainWordPublisher.cs'):data=restored_source(a.repo,'src/InkRecognition/'+name,h)
 assert sha(data)==h
r=dict(status='PASS',fullFunctions=len(set(r['function']for r in e['regions'])),fullRegions=len(e['regions']),closedUnregistered=len(e['closedUnregistered']),closedInstructions=closedInstructions,instructionPins=len(e['instructions']),bindings=len(e['bindings']),sharedSources=len(e['sharedSources']),vendorExecuted=False,scope='Native bytes and owned pure-kernel contracts only; no B6 production/nativeABI/WORD720/guide completion claim');a.output.write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r))
