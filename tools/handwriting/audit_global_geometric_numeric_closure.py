"""STATIC ONLY full-body numerical2DB dependency/operand/mutation evidence.
Reads DLL bytes and captured results only; never loads/executes vendor code.
"""
import argparse,hashlib,json,runpy,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path)
p.add_argument('--native-repo',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);a=p.parse_args()
root=a.native_repo.resolve();here=Path(__file__).resolve().parent
dependencies={}
for index,name in enumerate(['audit_uncovered_source_owner','audit_raw_ink_current_view']):
 dependency=a.output.with_name('g2n-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]+'-'+str(index)+'.json')
 saved=sys.argv
 try:
  sys.argv=[name,str(dependency)]
  runpy.run_path(str(root/'tools/handwriting'/(name+'.py')),run_name='__main__')
 finally:sys.argv=saved
 dependencies[name]=hashlib.sha256(dependency.read_bytes()).hexdigest()
spec=json.loads((here/'global-geometric-numeric-pins.json').read_bytes())
sys.path.insert(0,str(root/'research/myscript/scripts'));from inspect_text_features import Binary
binaries={n:Binary(n)for n in spec['dllSha256']};md=Cs(CS_ARCH_X86,CS_MODE_64);decoded={};calls={}
for name,b in binaries.items():assert hashlib.sha256(b.data).hexdigest()==spec['dllSha256'][name]
for r in spec['regions']:
 b=binaries[r['dll']];raw=b.data[b.off(r['start']):b.off(r['end'])]
 assert raw.hex()==r['bytes'] and hashlib.sha256(raw).hexdigest()==r['sha256']
 ins=list(md.disasm(raw,r['start']));assert sum(i.size for i in ins)==len(raw)and ins[-1].address+ins[-1].size==r['end']
 calls.setdefault(r['dll']+':'+r['function'],[])
 for i in ins:
  decoded[r['dll'],i.address]=(i.mnemonic,i.op_str)
  if i.mnemonic=='call':calls[r['dll']+':'+r['function']].append([i.address,i.op_str])
assert calls==spec['calls']
for dll,at,mn,op in spec['pins']:assert decoded[dll,at]==(mn,op)
for dll,at,target in spec['bindings']:assert binaries[dll].qwords(at,1)[0]==target
rows=(here/'global-geometric-numeric-bodies.tsv').read_text().splitlines()[1:]
assert [[dll,fn,int(s,16),int(e,16)+1,int(c)]for dll,fn,s,e,c in (r.split('\t')for r in rows)]==[[r['dll'],r['function'],r['start'],r['end'],r['end']-r['start']]for r in spec['regions']]
# The caller supplies both modes explicitly; recovering a decompiler prototype
# with four arguments must not erase native stack arguments5/6.
b=binaries['MyScriptText.dll'];entry={i.address:(i.mnemonic,i.op_str)for i in md.disasm(b.data[b.off(0x1800238b0):b.off(0x180023970)],0x1800238b0)}
for at,value in {0x1800238db:('mov','r12, qword ptr [r8 + 0x10]'),0x1800238df:('mov','r14, rdx'),
 0x180023940:('mov','r9, r14'),0x180023943:('mov','byte ptr [rsp + 0x28], 1'),
 0x180023948:('mov','r8b, 1'),0x18002394e:('mov','qword ptr [rsp + 0x20], r12'),0x18002395a:('call','r10')}.items():assert entry[at]==value
# Each external direct edge is classified, not silently called another
# geometric routine. Indirect service calls remain explicit in calls/pins.
external={}
for owner,edges in calls.items():
 dll,fn=owner.split(':');external[owner]=sorted({op for _,op in edges if op.startswith('0x')and dll+':'+op[2:]not in calls})
allowed={
 'MyScriptText.dll:18004dd50':['0x18025ee50'],
 'MyScriptText.dll:18004e480':['0x18025ba00'],
 'MyScriptText.dll:180050660':['0x180015d30','0x18025ee50','0x18025f210'],
 'MyScriptText.dll:18004bf30':['0x18025e340'],
 'MyScriptText.dll:18004c0c0':['0x18025ee50'],
 'MyScriptText.dll:18024f110':['0x1802537b0'],
 'MyScriptText.dll:1802503c0':['0x180251e70'],
 'MyScriptText.dll:18024e3f0':['0x180252f80','0x180253190','0x180253380','0x180253400'],
 'MyScriptText.dll:18024fe50':['0x180252f80','0x180253190','0x180253380','0x1802534b0'],
 'MyScriptInk.dll:18006b7f0':['0x1800c1b80'],
 'MyScriptInk.dll:1800914e0':['0x1800c1b80'],
 'MyScriptInk.dll:180091700':['0x180052fd0','0x18008ddb0','0x180094940','0x1800ac240','0x1800c1b80'],
 'MyScriptInk.dll:180090cf0':['0x18008ddb0'],
 'MyScriptInk.dll:1800907e0':['0x1800ceab0'],
}
assert {k:v for k,v in external.items()if v}==allowed
capture=a.capture.read_bytes();assert hashlib.sha256(capture).hexdigest()=='1f2d2f6e2bb57e6d249fc11325a5e9736f358a5cd9dce1fe2664dd8ecdebea58'
d=json.loads(capture);links=[(s,l)for s in d['actual']for l in s['links']];assert len(d['actual'])==13 and len(links)==37
selected=[int.from_bytes(bytes.fromhex(l['records'][l['marker']]['scalar40'])[8:12],'little')for s,l in links]
assert sum((v&0x7000)==0x1000 for v in selected)==0
assert sum((v&0x400)!=0 for v in selected)==0
assert all(l['ordinary']and(l['metadataFlags']&0x10600000)==0 for s,l in links)
assert sum(v==0x10000 for v in selected)==1
report=dict(status='PASS STATIC bounded ordinary numerical dependency closure; no runtime port',vendorExecution=False,
 dependencyReports=dependencies,
 functions=len(calls),bodyRanges=len(spec['regions']),linearBytes=sum(r['end']-r['start']for r in spec['regions']),
 pins=len(spec['pins']),bindings=len(spec['bindings']),directCallGraph=calls,classifiedExternalDirectEdges=allowed,
 caller=dict(originalRangeMode=1,geometryMode=1,source='retained Ink separate from selected result2DA'),
 actualCorpus=dict(sources=13,chosenCharacters=37,special400=0,widthClass1000=0,halfHeightClass10000=1,
  caveat='Chosen row gates absent in input; no native geometry execution or new recognition quality evidence'),
 mutations=['04B4C0 special selection updates referenced guide baseline','04E0E0 may OR02000000 into actual CHAR metadata before later helpers can fail'],
 externalEdgeClasses=dict(textMemoryAndArrays=['015D30','25EE50','25F210'],textStack='25BA00',
  textCrt=['25E340 sqrtf','2537B0 log exceptional path','251E70 sqrt exceptional path','252F80/253190/253380/253400/2534B0 trigonometric reduction/exception paths'],
  inkSecurity='0C1B80',inkRawFactory='08DDB0 already composed by raw-current-view proof',
  inkProvenanceCloneExcluded=['052FD0','094940','0AC240'],inkAffineExcluded='0CEAB0'),
 nextAPI='NEW typed selected-owner geometry with explicit ordered mutation receipts; a bounded ordinary entry may prove both mutation gates absent',
 remainingBeforeBroadRuntime=['C25/DOM special services','C4C transformed-source admission','CRT exceptional/nonfinite domains','prepared controlled boundary/failure numerical oracle and Managed/Native implementation'])
a.output.write_text(json.dumps(report,indent=2)+'\n');print(f"PASS STATIC{len(calls)} Ghidra functions/{len(spec['regions'])} exact body ranges/{len(spec['pins'])} PE pins/{len(spec['bindings'])} bindings;43-dependency analysis only, no runtime claim")
