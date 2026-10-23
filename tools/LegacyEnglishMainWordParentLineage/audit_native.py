"""Read-only PE/native/source closure and CURRENT raw-field numeric evidence.
No vendor execution, production decoder or borrowed old owner is claimed.
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
for n in ('repo','sdk','models','output'):p.add_argument('--'+n,type=Path,required=True)
for n in (134,100):
 for kind in ('d0','acoustic'):p.add_argument('--'+kind+'-'+str(n),type=Path,required=True)
a=p.parse_args();here=Path(__file__).resolve().parent;proof=json.loads((here/'native-pins.json').read_bytes());sha=lambda x:hashlib.sha256(x).hexdigest()
raw=(a.sdk/proof['binary']).read_bytes();assert sha(raw)==proof['sha256'];pe=struct.unpack_from('<I',raw,60)[0];assert raw[pe:pe+4]==b'PE\0\0';op=pe+24;base=struct.unpack_from('<Q',raw,op+24)[0];assert base==int(proof['base'],16)
st=op+struct.unpack_from('<H',raw,pe+20)[0];sections=[]
for i in range(struct.unpack_from('<H',raw,pe+6)[0]):
 _,rva,n,offset=struct.unpack_from('<IIII',raw,st+i*40+8);sections.append((rva,n,offset))
def read(at,n):
 v=at-base
 for start,size,off in sections:
  if start<=v and n<=start+size-v:return raw[off+v-start:off+v-start+n]
 raise AssertionError(('bounded PE',hex(at),n))
for r in proof['regions']:
 st=int(r['start'],16);n=int(r['endExclusive'],16)-st;assert n==r['bytes'] and sha(read(st,n))==r['sha256']
for r in proof['unregisteredCompleteRegions']:
 st=int(r['start'],16);b=bytes.fromhex(r['bytes']);assert int(r['endExclusive'],16)-st==len(b) and read(st,len(b))==b
for r in proof['calls']+proof['tables']:
 b=bytes.fromhex(r['bytes']);assert read(int(r['at'],16),len(b))==b
for r in proof['unchangedPureKernelSources']:
 b=_restore_reviewed_source(r['path'],(a.repo/r['path']).read_bytes());assert len(b)==r['bytes'] and sha(b)==r['sha256'],r['path']
# All body/call/leaf bytes above are reread from the actual PE, never ctypes.
assert proof['fullBodies']==39 and len(proof['calls'])==259 and len(proof['unregisteredCompleteRegions'])==9 and len(proof['tables'])==7
results=[];a.output.parent.mkdir(parents=True,exist_ok=True);stem='ewp-'+sha(str(a.output.resolve()).encode())[:12]
for n in (134,100):
 ac=getattr(a,'acoustic_'+str(n));d0=getattr(a,'d0_'+str(n));nn=json.loads(ac.read_bytes());dd=json.loads(d0.read_bytes())
 assert nn['TargetsRead'] is False and dd['TargetsRead'] is False and nn['InputSha256']==dd['InputSha256']
 assert [r['Name']for r in nn['Rows']]==[r['Name']for r in dd['Rows']]
 # This is numerical projection from supplied CURRENT captures, not a replacement
 # for their complete N/D0 independent producer audits. Never trust a stale PASS.
 gp=a.output.with_name(stem+'-geometry-'+str(n)+'.json')
 subprocess.run([sys.executable,'-I','-B',str(here/'reference_geometry.py'),'--repo',str(a.repo),'--models',str(a.models),'--acoustic',str(ac),'--output',str(gp)],check=True)
 g=json.loads(gp.read_bytes());k=json.loads((here/('existing-kernel-'+str(n)+'.json')).read_bytes())
 assert len(g['rows'])==len(k)
 for x,y in zip(g['rows'],k):assert x['Name']==y['Name'] and x['Groups']==y['Groups']
 bp=a.output.with_name(stem+'-backend-'+str(n)+'.json')
 subprocess.run([sys.executable,'-I','-B',str(here/'reference_backend.py'),'--repo',str(a.repo),'--models',str(a.models),'--d0',str(d0),'--geometry',str(gp),'--actual',str(here/('backend-existing-'+str(n)+'.json')),'--output',str(bp)],check=True)
 results.append(dict(owners=len(g['rows']),groups=sum(len(x['Groups'])for x in g['rows']),geometryCases=g['counts'],backend=json.loads(bp.read_bytes()),productionOwnerReady=False))
report=dict(status='PASS STATIC native39/39/calls259/leaves9/tables7 plus independent CURRENT geometry/backend numeric projections',nativeBodies=39,nativeRegions=39,linearInstructions=sum(r['instructions']for r in proof['regions']),calls=259,unregisteredCompleteRegions=9,unregisteredInstructions=sum(r['instructions']for r in proof['unregisteredCompleteRegions']),bindings=7,unchangedPureKernelFiles=len(proof['unchangedPureKernelSources']),numeric=results,nativeExecuted=False,runtimeChanged=False,genuineQueryRows=False,wordDecoderReady=False,targetsRead=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS STATIC native closure and numerical projections; not a decoder')
