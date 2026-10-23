"""English sparse-query STATIC closure. All SDK access is read-only PE bytes.
Runs the existing full sparse proof and an independent actual-model reference.
This does not execute a vendor module or claim handwriting request provenance.
"""
# Exact reviewed source reversal; no numerical/capture authority is changed.
import hashlib as _reviewed_hashlib,runpy as _reviewed_runpy
from pathlib import Path as _ReviewedPath
_reviewed_loader=_ReviewedPath(__file__).resolve().parents[1]/'handwriting/reviewed_node0_source_restoration.py'
assert _reviewed_hashlib.sha256(_reviewed_loader.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='9f310e7f9397b40d35a623a65e0064a0b9cee709cda3a9c46bd159e37186bc1a','Unreviewed source restoration loader'
_restore_reviewed_source=_reviewed_runpy.run_path(str(_reviewed_loader),run_name='reviewed_node0_source_pin_only')['restore_source_for_pin']
from pathlib import Path
import argparse, hashlib, json, struct, subprocess, sys
p=argparse.ArgumentParser()
for name in ('repo','models','sdk','output'):p.add_argument('--'+name,type=Path,required=True)
a=p.parse_args();here=Path(__file__).resolve().parent;sha=lambda b:hashlib.sha256(b).hexdigest()
proof=json.loads((here/'english-sparse-query-history-plan-pins.json').read_bytes())
raw=(a.sdk/proof['binary']).read_bytes();assert sha(raw)==proof['sha256']
pe=struct.unpack_from('<I',raw,60)[0];assert raw[pe:pe+4]==b'PE\0\0'
optional=pe+24;assert struct.unpack_from('<H',raw,optional)[0]==0x20b
base=struct.unpack_from('<Q',raw,optional+24)[0];assert base==int(proof['base'],16)
table=optional+struct.unpack_from('<H',raw,pe+20)[0];sections=[]
for i in range(struct.unpack_from('<H',raw,pe+6)[0]):
 q=table+i*40;_,rva,size,offset=struct.unpack_from('<IIII',raw,q+8);sections.append((rva,size,offset))
def read(va,n):
 rva=va-base
 for start,size,offset in sections:
  if start<=rva and n<=start+size-rva:return raw[offset+rva-start:offset+rva-start+n]
 raise AssertionError(('bounded PE range',hex(va),n))
assert len(proof['newRegions'])==13 and proof['newFullBodies']==12
for region in proof['newRegions']:
 start=int(region['start'],16);end=int(region['endExclusive'],16)
 assert end-start==region['bytes'] and sha(read(start,end-start))==region['sha256']
for pin in proof['newCalls']+proof['criticalPins']+proof['tables']:
 expected=bytes.fromhex(pin['bytes']);assert read(int(pin['at'],16),len(expected))==expected
assert len(proof['newCalls'])==139 and len(proof['criticalPins'])==8 and len(proof['tables'])==2
for source in proof['unchangedOwners']:
 text=_restore_reviewed_source(source['path'],(a.repo/source['path']).read_bytes()).decode('utf-8-sig').replace('\r\n','\n')
 assert sha(text.encode())==source['lfSha256'],source['path']
upstream=proof['upstreamSparsePins'];assert sha((a.repo/upstream['path']).read_bytes())==upstream['sha256']
a.output.parent.mkdir(parents=True,exist_ok=True)
prefix='esq-'+sha(str(a.output.resolve()).encode())[:12]
native=a.output.with_name(prefix+'-native.json');numeric=a.output.with_name(prefix+'-numeric.json')
subprocess.run([sys.executable,str(a.repo/'tools/handwriting/audit_english_lk_sparse101.py'),'--models',str(a.models),'--sdk',str(a.sdk),'--output',str(native)],check=True)
subprocess.run([sys.executable,str(here/'reference_english_sparse_queries.py'),'--repo',str(a.repo),'--models',str(a.models),'--output',str(numeric)],check=True)
numbers=json.loads(numeric.read_bytes())
assert numbers['checks']==643868 and numbers['numericDigest']=='1cad8793f3a43641a912f581be810347aa1b0bba066cc0f86fba7b820f223ebe'
assert numbers['initialClass']==0 and numbers['reservedInitialWord']==260074
assert numbers['counts']['matchedOrder1']==248913 and numbers['counts']['matchedOrder2']==239471 and numbers['counts']['matchedOrder3']==155484
result=dict(status='PASS static English query/history closure',newFullBodies=12,newRegions=13,newInstructions=sum(r['instructions']for r in proof['newRegions']),newCallPins=139,criticalInstructionPins=8,tables=2,unchangedOwners=4,upstream=json.loads(native.read_bytes()),numerical=numbers,targetsRead=False,vendorExecuted=False,runtimeChanged=False,requestOrigin='Actual native caller/layout proven; portable reference uses separately labelled raw numerical controls, not NN-selected candidates.')
a.output.write_text(json.dumps(result,indent=2)+'\n');print('PASS English query/history static: 12 full bodies, 139 calls, 643868 raw numerical controls')
