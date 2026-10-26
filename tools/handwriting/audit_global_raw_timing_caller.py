"""STATIC constructor dispatch only. Does not load/execute an SDK module."""
import argparse,hashlib,json,runpy,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path)
p.add_argument('--native-repo',type=Path,required=True);p.add_argument('--ordinary-plan-root',type=Path,required=True);p.add_argument('--capture',type=Path,required=True)
p.add_argument('--models',type=Path);p.add_argument('--child',type=Path);a=p.parse_args()
if (a.models is None)!=(a.child is None):p.error('--models and --child must be supplied together')
paired=[] if a.models is None else ['--models',str(a.models),'--child',str(a.child)];root=a.native_repo.resolve();here=Path(__file__).resolve().parent
dependency=a.output.with_name('grt-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]+'.json')
script=a.ordinary_plan_root/'tools/handwriting/audit_global_geometric_ordinary_plan.py';saved=sys.argv
try:
 sys.argv=[str(script),str(dependency),'--native-repo',str(root),'--capture',str(a.capture),*paired]
 runpy.run_path(str(script),run_name='__main__')
finally:sys.argv=saved
sys.path.insert(0,str(root/'research/myscript/scripts'));from inspect_text_features import Binary
spec=json.loads((here/'global-raw-timing-caller-pins.json').read_bytes());binaries={n:Binary(n)for n in spec['dllSha256']}
md=Cs(CS_ARCH_X86,CS_MODE_64);decoded={};calls={};regions=[]
for n,b in binaries.items():assert hashlib.sha256(b.data).hexdigest()==spec['dllSha256'][n]
for r in spec['regions']:
 b=binaries[r['dll']];raw=b.data[b.off(r['start']):b.off(r['end'])]
 assert raw.hex()==r['bytes'] and hashlib.sha256(raw).hexdigest()==r['sha256']
 ins=list(md.disasm(raw,r['start']));assert sum(i.size for i in ins)==len(raw)
 decoded.update({(r['dll'],i.address):(i.mnemonic,i.op_str)for i in ins})
 calls[r['dll']+':'+r['function']]=[[i.address,i.op_str]for i in ins if i.mnemonic=='call']
 regions.append([r['dll'],r['function'],r['start'],r['end'],len(raw)])
assert calls==spec['calls']
for dll,at,mn,op in spec['pins']:assert decoded[dll,at]==(mn,op)
for dll,at,target in spec['bindings']:assert binaries[dll].qwords(at,1)[0]==target
rows=(here/'global-raw-timing-caller-bodies.tsv').read_text().splitlines()[1:]
assert [[dll,fn,int(s,16),int(e,16)+1,int(n)]for dll,fn,s,e,n in (r.split('\t')for r in rows)]==regions
e=binaries['MyScriptEngine.dll'];ink=binaries['MyScriptInk.dll']
assert e.data[e.off(0x180001f30):e.off(0x180001f35)]==bytes.fromhex('e95bfeffff')
# Actual descriptor chain, not a guessed constructor class/name.
assert ink.descriptor(0xc1c)[0]==0x1800e7130
assert ink.qwords(0x1800e7130+24,1)==(0x1800e6fd0,)
assert ink.qwords(0x1800e6fd0+16,1)==(0x1800e6fb8,)
assert ink.qwords(0x1800e6fb8,3)==(0,0x180097c10,0x1800978d0)
for at,value in {
 0x180038010:('mov','rax, qword ptr [rdx - 0x10]'),
 0x180038014:('mov','rax, qword ptr [rax + 0x18]'),
 0x18003801d:('mov','rax, qword ptr [rax + 0x10]')}.items():assert decoded['MyScriptEngine.dll',at]==value
report=dict(status='PASS STATIC typed constructor dispatch; no default timing receipt created',vendorExecution=False,
 dependencySha256=hashlib.sha256(dependency.read_bytes()).hexdigest(),
 fullFunctionRanges=len(regions),linearInstructions=len(spec['pins']),bindings=len(spec['bindings']),directCallGraph=calls,
 C1C=dict(descriptor='1800E7130',metadata='1800E6FD0',constructors='1800E6FB8',default=0,clone='180097C10',extended='1800978D0',
  defaultPath='voCreateObject1F30→1D90 requires nonnull ctor0; NULL fails1040002',
  extendedPath='1F40 uses ctor10 and forwards supplied descriptor/size; size0 delegates1D90 and fails forC1C',
  clonePath='097C10 copies actual+28/+2c/+30/+34 and channel/layout, not default initialization'),
 C1E=dict(descriptor='1800E6C60',classMethodTable='1800E6980',createOrdinary='180090CF0',
  constructorTable='1800E6CA8',baseConstructor='180090C90',
  caveat='C1E type2 class-method table is NOT voGetInterface type0 and NOT its constructor table',
  ordinaryFifthArgument='caller baseTag forwarded toC49+30; no default invented'),
 conclusion='The admitted native C1C/C1E raw constructor route requires explicit physical metadata. Fresh INPUT/C20 ownership does not recover it fromXY.',
 futureTypedAPI=dict(oldXYUnchanged=True,explicitPhysicalRateAndBaseTagRequired=True,actualNativeProvenanceRequired=True,
  channelAdmission='initially explicitXY-only descriptor; no synthetic time channel',
  ready2DB=False),
 remaining=['existing rawXY owners do not gain native timing receipts from this proof','higher-level iink/UI ingestion defaults not proved or assumed','numerical timing/crop/overflow oracle and Managed/Native implementation not performed'])
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f"PASS STATIC{len(regions)} full function ranges/{len(spec['pins'])} instructions/{len(spec['bindings'])} bindings; C1C ctor0 NULL, no synthetic timing")
