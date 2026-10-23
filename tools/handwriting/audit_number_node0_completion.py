"""PE/data/source-only audit for the proposed number skipped-language stage.
CLI: MODELS NATIVE_REPO CONSUMER_CAPTURE OBSERVATION_CAPTURE REFERENCE_OUTPUT
No vendor execution, C# invocation, NN invocation or live continuation authority.
"""
import copy,hashlib,json,runpy,struct,sys
from pathlib import Path
import pefile
sha=lambda b:hashlib.sha256(b).hexdigest()
HERE=Path(__file__).resolve().parent;REPO=HERE.parents[1]
def exact_consumer_preimage(data):
 # Preserve the historical STATIC source pin; recognize only the reviewed
 # exact runtime successor through its independently authenticated reversal.
 b=data.replace(b'\r\n',b'\n')
 if sha(b)=='25be1dce91a32902c2b29fcc24c14b3f28ebac2a5f18e7fbd4d87b70620a7935':return b
 oracle=HERE.parent/'LegacyFst28CNumberSkippedLanguageCompletion.Probe/audit_output.py'
 raw=oracle.read_bytes().replace(b'\r\n',b'\n')
 assert sha(raw)=='3ec7282a8bcd7b44c7ba2c8a4b023dba2c14c38b5651ffe99f5110fb1b70ff46'
 module=runpy.run_path(str(oracle),run_name='number_static_exact_preimage_only')
 result=module['source_preimage'](b)
 assert sha(result)=='25be1dce91a32902c2b29fcc24c14b3f28ebac2a5f18e7fbd4d87b70620a7935'
 return result

def alphabet(raw,start):
 pos=start
 def read(fmt,n=1):
  nonlocal pos
  x=struct.unpack_from('<'+fmt*n,raw,pos);pos+=struct.calcsize('<'+fmt)*n
  return x[0] if len(x)==1 else list(x)
 head=read('H',8);count=head[-1];n=read('H')
 for extra in (32,64,1):
  table=read('H',n);table=[table] if n==1 else table;n=next(v+extra for v in reversed(table) if v!=65535)
 spans=read('H',n+1);entries=[read('II') for _ in range(spans[-1]+1)]
 strings=raw[pos:pos+entries[-1][0]];pos+=entries[-1][0]
 tokens={label&65535:strings[off:entries[i+1][0]] for i,(off,label) in enumerate(entries[:-1])}
 return count,tokens
def main(args):
 if len(args)!=5:raise SystemExit(__doc__)
 models,native_repo,consumer_path,observation_path,output_path=map(Path,args)
 pins=json.loads((HERE/'linguistic-fst28c-number-node0-completion-pins.json').read_bytes())
 raw=(native_repo/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes()
 assert sha(raw)==pins['nativeSHA256'];pe=pefile.PE(data=raw);base=pe.OPTIONAL_HEADER.ImageBase
 def data(a,n):return pe.get_data(int(a,16)-base,n)
 for r in pins['regions']+pins['leaves']:assert sha(data(r['start'],r['bytes']))==r['sha256'],r
 for p in pins['pointers']:
  b=data(p['start'],8);assert b.hex()==p['hex'] and int.from_bytes(b,'little')==int(p['target'],16)
 for p in pins['scalars']:assert data(p['start'],p['bytes']).hex()==p['hex']
 for p in pins['source']:
  b=(REPO/p['path']).read_bytes().replace(b'\r\n',b'\n')
  if p['path']=='src/InkRecognition/LegacyFst28CNumberInitialConsumer.cs':
   current=b;b=exact_consumer_preimage(current)
   assert exact_consumer_preimage(b)==b
   for suffix in (b' ',b'\n',b'//unapproved\n'):
    try:exact_consumer_preimage(current+suffix)
    except AssertionError:pass
    else:raise AssertionError('Unapproved STATIC source successor accepted')
  assert sha(b)==p['lfSHA256'] and len(b)==p['bytesLF']
 for p in pins['inputs']:
  b=Path(p['path']).read_bytes();assert sha(b)==p['sha256'] and len(b)==p['bytes']
 ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
 assert sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
 n,al=alphabet(ak,99);assert n==192
 replacement=[i for i,t in al.items() if t==b'\xef\xbf\xbd'];assert len(replacement)==1
 assert replacement[0] not in list(range(10))+[138,185]
 c=json.loads(consumer_path.read_bytes());o=json.loads(observation_path.read_bytes())
 assert sha(consumer_path.read_bytes())=='1d9a13433273975ae3065a6af983f1d4584a7a4111069e24cf8f81d49d5cb2f1'
 assert sha(observation_path.read_bytes())=='860cb7f4016f9493777605e9442c7c344a101fccb9f61da0f266efb5cdd0a4fd'
 m=runpy.run_path(str(HERE/'reference_number_node0_completion.py'))
 actual=m['actual'](c,o);prepared=m['prepared']()
 expected=json.loads(output_path.read_bytes());assert actual==expected['cases'] and prepared==expected['prepared']
 mask_controls=m['mask_controls']();assert expected['maskControls']==mask_controls and len(mask_controls)==10
 assert expected['logicalEndSourceReset']=='internal library state adaptation; no discarded Engine/live query authority'
 # Immutable historical reference proves this correction did not silently
 # change numeric values: only prepared mask outputs were incomplete before.
 oldpin=pins['maskCorrection']['oldReference'];oldpath=Path(oldpin['path'])
 assert sha(oldpath.read_bytes())==oldpin['sha256'];old=runpy.run_path(str(oldpath))
 assert old['actual'](c,o)==actual
 for nrows in (0,1,125,126,250):
  for duplicate in (False,True):
   rows,pool,second=m['prepared_inputs'](nrows,duplicate)
   before=old['phase0'](rows,nrows,pool,second)
   after=m['phase0'](rows,nrows,pool,second,[bytes([255])*128 for _ in rows],bytes([255])*len(pool),bytes([255])*len(second))
   assert {k:v for k,v in before.items() if not k.endswith('Defined')}=={k:v for k,v in after.items() if not k.endswith('Defined')}
 assert [x['admitted'] for x in actual]==[True,False,True,False]
 for case in c['cases']:
  for row in case['records']:
   b=bytes.fromhex(row);assert b[4]==1 and m['u'](b,36,2) in list(range(10))+[138,185]
   assert m['u'](b,56)==0 and m['u'](b,60,2)==65535 and m['s'](b,52)==0
 # Scalar source helper projection, not packet/API equivalence. Source reset
 # is25 direct stores/164 covered bytes. It never clears preserved58 or maps.
 reset=[(0xc8,4,0xffffffff)]
 for i in range(8):
  reset.extend([(0x100+i*80,8,0xffffffffffffffff),(0x110+i*80,8,0xffffffffffffffff),
       (0x118+i*80,4,0 if i==7 else 0xffffffff)])
 assert len(reset)==25 and sum(x[1] for x in reset)==164
 assert all(not(a<0x60 and a+n>0x50) for a,n,_ in reset)
 controls=[]
 def control(name,fn):
  fn();controls.append(name)
 def raw_tamper():
  r=pins['regions'][0];b=bytearray(data(r['start'],r['bytes']));b[-1]^=1;assert sha(b)!=r['sha256']
 control('full-native-region-last-byte',raw_tamper)
 def pointer_tamper():
  p=pins['pointers'][0];b=bytearray(data(p['start'],8));b[0]^=1
  assert b.hex()!=p['hex'] and int.from_bytes(b,'little')!=int(p['target'],16)
 control('binding-target-byte',pointer_tamper)
 def flag_control():
  graph=copy.deepcopy(o['graphs'][0]);graph[1][0]['flags']|=4;assert m['boundary'](graph)[0]
 control('admitted-graph-real-flag-domain',flag_control)
 def skipped_flag():
  graph=copy.deepcopy(o['graphs'][0]);graph[2][2]['flags']=0 #span2 is outside node0 lineage
  assert not m['boundary'](graph)[0]
 control('unrelated-span-does-not-invent-gate',skipped_flag)
 def history_excluded():
  a=bytearray(128);b=bytearray(a);m['put'](b,112,2**64-1,8);m['put'](b,104,-1)
  assert m['key'](a)==m['key'](b)
 control('number-key-excludes-English-history',history_excluded)
 def signed_aux():
  a=bytearray(128);b=bytearray(a);m['put'](a,48,-1);assert m['key'](a)<m['key'](b)
 control('number-key-signed30',signed_aux)
 def masks():
  r=actual[0]['result'];b=bytearray.fromhex(r['stored']);mask=bytes.fromhex(r['storedDefined'])
  assert mask[125*88]==0 and mask[16]==255
  a=bytearray(b);a[125*88]^=1;assert all(not((x^y)&z) for x,y,z in zip(a,b,mask))
  a[16]^=1;assert any((x^y)&z for x,y,z in zip(a,b,mask))
 control('native-defined-vs-unwritten-allocation',masks)
 def end_counts():
  r=actual[0]['result'];assert r['retained']==8 and r['sourceEnd']==125 and r['phase1Count']==0
  assert r['retained']!=r['retainedEnd'] and r['phase1Capacity']==124 and r['allocatedHeapCapacity']==250
 control('no-finite-count-retune-no-phase1-heap-loss',end_counts)
 def lookup_identity():
  rows,p,q=m['pools_from_case'](c['cases'][0]);assert p!=q
  for i,row in enumerate(rows[:8]):
   assert m['u'](p,i*80+8,8)!=m['u'](p,i*80+16,8)
   assert p[i*80+31]&32 and not(q[i*80+31]&32)
 control('two-pools-raw-and-mapped-contexts-distinct',lookup_identity)
 B=2*250*128+4*250*80+2*250*80+4*250*128+250*128+2*250*80+3*250*128+2*(156*88+125*80+156*80)+125*(88+160)+250*4+8+250*8+65536+8192+(156*88+125*80+156*80)
 W=2*B+250**2*(2*50+3*128)+4*250*30*80+250*2048+16*250*128+2*125*(88+160)+2*256*16+8192
 assert (B,W)==(696360,35145104)
 mask_payload=2*250*128+4*250*80+2*250*80+4*250*128+2*250*80+3*250*128+125*(88+160)
 assert mask_payload==479000
 B+=mask_payload
 W+=2*mask_payload+250**2*3*128+250*2048+16*250*128+2*125*(88+160)
 assert (B,W)==(1175360,61189104)
 print(json.dumps(dict(status='STATIC_ONLY_PASS',completeRegions=len(pins['regions']),boundedLeafs=len(pins['leaves']),
  bindings=len(pins['pointers']),sourcePins=len(pins['source']),replacementLabel=replacement[0],admitted=2,preclaimRefused=2,
  prepared=len(prepared),negativeControls=controls,resetStores=len(reset),resetCoveredBytes=164,
  maskControls=len(mask_controls),preparedNumericUnchanged=10,actualOutputsByteExactToOriginal=True,
  logicalPayload=B,admissionWork=W,referenceSHA256=sha(output_path.read_bytes()))))
if __name__=='__main__':main(sys.argv[1:])
