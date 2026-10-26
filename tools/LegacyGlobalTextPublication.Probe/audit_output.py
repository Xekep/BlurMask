"""Portable independent compact-DFS/source26N/full-WORD120 oracle.
No research imports, vendor execution, target labels or correctness threshold.
"""
import base64,hashlib,importlib.util,json,math,struct,sys
from pathlib import Path
if len(sys.argv) not in(3,5):raise SystemExit('AK default_capture [diagnostic_trace diagnostic_no_trace]')
ak=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
root=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('raw_al',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py');al=importlib.util.module_from_spec(spec);spec.loader.exec_module(al)
labels=al.read_alphabet(ak);assert not set('\u2610\u2611\u2612')&set(labels.values())
dec=base64.b64decode;f32=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
d=json.loads(Path(sys.argv[2]).read_bytes());assert len(d['samples'])==6
has_active=any(any(base64.b64decode(c['compact'])[at+32]&8 for at in range(0,len(base64.b64decode(c['compact'])),40))for c in d['samples'])
positive_report=None
if has_active:
    assert len(sys.argv)==5,'Actual bit8 augmentation requires paired compile-time diagnostic evidence; no companion is a refusal'
    contract_path=root/'tools/handwriting/publication_diagnostic_flavor_source_contract.py'
    contract_spec=importlib.util.spec_from_file_location('publication_flavor_source_contract',contract_path)
    contract=importlib.util.module_from_spec(contract_spec);contract_spec.loader.exec_module(contract)
    flavor_source_proof=contract.verify(root,require_meter_v2=True)
    spec=importlib.util.spec_from_file_location('positive_publication',Path(__file__).with_name('positive')/'audit_candidates.py')
    positive=importlib.util.module_from_spec(spec);spec.loader.exec_module(positive)
    diagnostic=json.loads(Path(sys.argv[3]).read_bytes());no_trace=json.loads(Path(sys.argv[4]).read_bytes())
    reverse={v:k for k,v in labels.items()};lookup=lambda text:reverse.get(text,65535)
    special=[lookup('\u0003'),lookup('\u200d'),lookup('\U000f0001'),struct.unpack_from('<H',ak,105)[0],lookup('\u00ad')]
    positive_report=positive.audit(diagnostic,no_trace,special,lookup('\u200d'))
    # Same-flavor pairing is exact inside the independent oracle; across builds
    # only the established140B90 exponential4ULP allowance applies.
    left=json.loads(json.dumps(d));right=json.loads(json.dumps(no_trace));assert len(right['samples'])==6
    for a,b in zip(left['samples'],right['samples']):
        wa,wb=map(base64.b64decode,(a['after120'],b['after120']))
        assert len(wa)==len(wb)==120 and wa[:16]==wb[:16]and wa[24:]==wb[24:],'Default/diagnostic opaque WORD120 differs'
        for offset in(16,20):
            va,vb=struct.unpack_from('<f',wa,offset)[0],struct.unpack_from('<f',wb,offset)[0]
            assert math.isfinite(va)and math.isfinite(vb)and va>=0 and vb>=0
            assert abs(struct.unpack_from('<I',wa,offset)[0]-struct.unpack_from('<I',wb,offset)[0])<=4,'Default/diagnostic normalization4ULP'
        a['after120']=b['after120']
    canonical=lambda value:json.dumps(value,sort_keys=True,separators=(',',':'),allow_nan=False)
    assert canonical(left)==canonical(right),'All original default/diagnostic6 fields must agree'
else:
    assert len(sys.argv)==3,'Legacy no-bit8 audit uses its unchanged capture route'

merges=refs=payload=0
for c in d['samples']:
 assert c['beforeNnSameBinding']and c['sameDocument']and c['rawRouteCompactEqual']and c['retainedAfterContextDispose']
 assert c['registry']==[0,1]and c['registryNext']==2 and c['sourceTag']==c['chosen']
 compact=dec(c['compact']);assert len(compact)%40==0
 before,after=map(dec,(c['before120'],c['after120']));assert len(before)==len(after)==120
 if not any(compact[i+32]&8 for i in range(0,len(compact),40)):
  # Preserve the original no-active-augmentation assertions exactly.
  assert all(not compact[i+32]&8 for i in range(0,len(compact),40))
  assert c['selected']==0 and c['quota']==2
  expected=bytearray(before);struct.pack_into('<ff',expected,16,1.0,1.0);assert after==expected
 else:
  assert positive_report is not None,'No unverified positive branch'
 payload+=120
 nodes,keys,metadata=c['nodes'],c['keys'],c['metadata']
 def walk(n,a):
  group,start,end,span,children=nodes[n][a];assert span==0 and start>>7==n
  if end>c['last']:return None
  if end==c['last']:return[group]
  for child in children:
   path=walk(n+1,child)
   if path is not None:return[group]+path
  return None
 path=None
 for a,arc in enumerate(nodes[c['first']>>7]):
  if arc[1]==c['first']and arc[3]==0:
   path=walk(c['first']>>7,a)
   if path is not None:break
 assert path is not None
 rows=[]
 for group in path:
  for stroke,start,end in c['groups'][group]:rows.append(dict(first=metadata[stroke][start],last=metadata[stroke][end-1],tail=end==len(metadata[stroke])))
 def order(row):return tuple(row['first'][0])+(-row['first'][3],)+tuple(row['last'][1])+(-row['last'][3],)
 #Separate bubble sorting and reverse coalescing over immutable captured keys.
 for end in range(len(rows)-1,0,-1):
  for i in range(end):
   if order(rows[i])>order(rows[i+1]):rows[i],rows[i+1]=rows[i+1],rows[i]
 for i in range(len(rows)-1,0,-1):
  a,b=rows[i-1],rows[i];last,first=a['last'],b['first']
  if(first[0]==last[0]and first[3]==last[3])or(a['tail']and last[2]+1<len(keys)and first[0]==keys[last[2]+1]):
   a['last']=b['last'];a['tail']=b['tail'];rows.pop(i);merges+=1
 expected=[(r['first'][0],r['last'][1 if r['tail']else 0],r['first'][3],r['last'][3])for r in rows]
 source=dec(c['source26']);assert len(source)==26*len(expected)>0
 wire=b''.join(struct.pack('<6i',*first,*last)for first,last,_,_ in expected)+bytes(b for _,_,left,right in expected for b in(left,right))
 assert source==wire;refs+=len(expected)
 #Ordinary raw input has full, closed per-object selections. Reimplement
 #053F90/00DB00 float32 quantization, then05AFA0 endpoint closure.
 def pack(obj,coordinate,mode):
  count=c['counts'][obj];integral=math.floor(coordinate);remainder=f32(coordinate-integral)
  fraction=255 if remainder==0 and integral==count-1 else int(f32(f32(remainder*200)+.5))
  if fraction==200:integral+=1;fraction=255 if integral==count-1 else 0
  return obj<<32|integral<<16|fraction<<8|mode
 positions=[]
 for first,last,left,right in expected:
  assert first[0]==last[0]==0
  for obj in range(first[1],last[1]+1):
   a=f32(first[2]-f32(left/200))if obj==first[1]else 0.0
   b=f32(last[2]-f32(right/200))if obj==last[1]else float(c['counts'][obj]-1)
   positions.append([pack(obj,a,3),pack(obj,b,5)])
 assert positions==c['positions'],(positions,c['positions'])
 assert dec(c['inputRanges'])==struct.pack('<QQ',3,((len(c['counts'])-1)<<32)|((c['counts'][-1]-1)<<16)|(255<<8)|5)
assert d['preparedFlagControls']==256
assert refs==d['references']==6 and sum(c['nn']for c in d['samples'])==d['nn']==126 and d['guards']>=82 and merges>0
if positive_report is not None:print('PASS paired positive219580 inputs/decisions with external producer boundaries:',json.dumps(positive_report,sort_keys=True))
print(f'PASS independent6 actual global owners/{refs} complete26N/{payload} full WORD120 bytes/{merges} reverse merges/raw AL guard/same Document/binding; no acceptance probability')
