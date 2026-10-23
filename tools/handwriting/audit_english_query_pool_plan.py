"""Read-only PE/static producer closure, independent raw pool projection.
Does not execute SDK code or widen the old masks-only pool policy.
"""
# Exact reviewed source reversal; no numerical/capture authority is changed.
import hashlib as _reviewed_hashlib,runpy as _reviewed_runpy
from pathlib import Path as _ReviewedPath
_reviewed_loader=_ReviewedPath(__file__).resolve().parents[1]/'handwriting/reviewed_node0_source_restoration.py'
assert _reviewed_hashlib.sha256(_reviewed_loader.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='9f310e7f9397b40d35a623a65e0064a0b9cee709cda3a9c46bd159e37186bc1a','Unreviewed source restoration loader'
_restore_reviewed_source=_reviewed_runpy.run_path(str(_reviewed_loader),run_name='reviewed_node0_source_pin_only')['restore_source_for_pin']
from pathlib import Path
import argparse,hashlib,json,struct,subprocess,sys
p=argparse.ArgumentParser()
for name in ('repo','models','sdk','output'):p.add_argument('--'+name,type=Path,required=True)
p.add_argument('--acoustic-root',type=Path);a=p.parse_args();here=Path(__file__).resolve().parent;sha=lambda raw:hashlib.sha256(raw).hexdigest()
proof=json.loads((here/'english-dictionary-query-pool-plan-pins.json').read_bytes());raw=(a.sdk/proof['binary']).read_bytes();assert sha(raw)==proof['sha256']
pe=struct.unpack_from('<I',raw,60)[0];assert raw[pe:pe+4]==b'PE\0\0';optional=pe+24;assert struct.unpack_from('<H',raw,optional)[0]==0x20b
base=struct.unpack_from('<Q',raw,optional+24)[0];assert base==int(proof['base'],16);table=optional+struct.unpack_from('<H',raw,pe+20)[0];sections=[]
for i in range(struct.unpack_from('<H',raw,pe+6)[0]):
 q=table+i*40;_,rva,size,offset=struct.unpack_from('<IIII',raw,q+8);sections.append((rva,size,offset))
def read(va,n):
 rva=va-base
 for start,size,offset in sections:
  if start<=rva and n<=start+size-rva:return raw[offset+rva-start:offset+rva-start+n]
 raise AssertionError(('bounded PE range',hex(va),n))
assert proof['fullBodies']==9 and len(proof['regions'])==11 and len(proof['calls'])==62 and len(proof['tables'])==3
for region in proof['regions']:
 begin=int(region['start'],16);end=int(region['endExclusive'],16);assert end-begin==region['bytes']and sha(read(begin,end-begin))==region['sha256']
for pin in proof['calls']+proof['tables']:
 expected=bytes.fromhex(pin['bytes']);assert read(int(pin['at'],16),len(expected))==expected
assert struct.unpack('<d',read(0x1802ed2c0,8))[0]==1.5
for group,root in ((proof['unchangedOwners'],a.repo),(proof['acousticContract']['files'],a.acoustic_root or a.repo)):
 for source in group:assert sha(_restore_reviewed_source(source['path'],(root/source['path']).read_bytes()).decode('utf-8-sig').replace('\r\n','\n').encode())==source['lfSha256'],source['path']
assert sha((a.repo/proof['upstream']['path']).read_bytes())==proof['upstream']['sha256']
a.output.parent.mkdir(parents=True,exist_ok=True);prefix='eqp-'+sha(str(a.output.resolve()).encode())[:12];native=a.output.with_name(prefix+'-upstream.json');reference=a.output.with_name(prefix+'-raw.json')
subprocess.run([sys.executable,str(a.repo/'tools/handwriting/audit_english_sparse_query_history_plan.py'),'--repo',str(a.repo),'--models',str(a.models),'--sdk',str(a.sdk),'--output',str(native)],check=True)
subprocess.run([sys.executable,str(here/'reference_english_query_pool_plan.py'),'--repo',str(a.repo),'--models',str(a.models),'--output',str(reference)],check=True)
data=json.loads(reference.read_bytes());assert data['initialMappedContexts']==86 and data['ordinaryMappedRootRows']==78 and data['allRootGraftsPreparedProjection']['requiredNodes']==165 and data['ordinarySourceTagBit42AlwaysZero']
result=dict(status='PASS fresh English query pool/static caller closure',fullBodies=9,regions=11,instructions=sum(r['instructions']for r in proof['regions']),calls=62,tables=3,unchangedOldOwners=5,acousticContractFiles=2,upstream=json.loads(native.read_bytes()),raw=data,runtimeChanged=False,vendorExecuted=False,targetsRead=False,scope='Fresh source-query ownership proposal; neither masks104-policy widening nor NN/controller winner claimed')
a.output.write_text(json.dumps(result,indent=2)+'\n');print('PASS EN query pool9full/11regions/62calls +same caller/shared pool +raw78/suppress0 +prepared165-node projection')
