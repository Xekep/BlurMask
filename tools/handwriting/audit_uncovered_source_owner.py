"""Read-only full-body evidence for uncovered-source 238B0 and its 2DB owner.
This does not implement geometric lookup, source edits or document acceptance.
"""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('output',type=Path)
p.add_argument('--native-repo',type=Path,default=Path(__file__).resolve().parents[2])
p.add_argument('--capture',type=Path)
a=p.parse_args();root=a.native_repo.resolve()
spec=json.loads(Path(__file__).with_name('uncovered-source-owner-pins.json').read_bytes())
rows=Path(__file__).with_name('uncovered-source-owner-bodies.tsv').read_text().splitlines()[1:]
assert [[fn,int(first,16),int(last,16)+1,int(count)]for fn,first,last,count in (r.split('\t')for r in rows)]==[[r['function'],r['start'],r['end'],r['end']-r['start']]for r in spec['regions']]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary(spec['dll']);assert hashlib.sha256(b.data).hexdigest()==spec['dllSha256']=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=Cs(CS_ARCH_X86,CS_MODE_64);decoded={}
for region in spec['regions']:
 raw=b.data[b.off(region['start']):b.off(region['end'])]
 assert hashlib.sha256(raw).hexdigest()==region['sha256']
 instructions=list(md.disasm(raw,region['start']))
 assert sum(i.size for i in instructions)==len(raw) and instructions[-1].address+instructions[-1].size==region['end']
 decoded.update({i.address:(i.mnemonic,i.op_str)for i in instructions})
for at,mnemonic,operands in spec['pins']:assert decoded[at]==(mnemonic,operands),(hex(at),mnemonic,operands)
for slot,target in spec['bindings']:assert b.qwords(slot,1)[0]==target
assert any(m=='call' and op=='0x1800238b0'for m,op in decoded.values())
for target in ('0x18004b4c0','0x18004e480','0x1800234d0','0x1800339e0','0x180036630'):
 assert any(m=='call' and op==target for m,op in decoded.values()),target
evidence=None
if a.capture:
 capture=json.loads(a.capture.read_bytes());samples=capture['samples']
 assert len(samples)==13 and all(s['stage']=='complete'for s in samples)
 unequal=[s['name']for s in samples if s['wordClip']!=s['original']]
 assert unequal==['MIR','II']
 evidence=dict(captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),samples=13,
  wordVsInputRootEqual=11,wordVsInputRootUnequal=unequal,
  scope='Existing CHAR capture only; not a produced TEXT/2DB or uncovered-source runtime')
report=dict(status='PASS',vendorExecution=False,completeGhidraBodies=len(spec['regions']),
 instructionPins=len(spec['pins']),bindings=len(spec['bindings']),naturalInputEvidence=evidence,
 scope=spec['scope'],runtimeImplemented=False,
 missing=['Owned nonempty 2DB selected WORD/CHAR/guide index and geometric boundaries',
  'Actual C23/C49/C25 source-object dispatch and grouping',
  '238B0 mutations and 234D0/339E0/36630 exact failed prefixes',
  'Typed global document consumer and original acceptance'])
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f"PASS {report['completeGhidraBodies']} complete bodies/{report['instructionPins']} call/condition pins/{report['bindings']} bindings; uncovered-source runtime remains unimplemented")
