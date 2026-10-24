"""PURE native044000 segment projection design; ordinary DATA, never an issuer.
No vendor/C#/NN execution. Native layout authority is frozen37a1ae.
"""
import struct,math,json,sys,importlib.util,base64,hashlib
from pathlib import Path

def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<i',struct.pack('<f',v))[0]

def metric(points):
 assert points and all(math.isfinite(v)for p in points for v in p)
 minimum=points[0].copy();maximum=points[0].copy();arc=sx=sy=0.
 for p in points[1:]:
  for j in [0,1]:
   if maximum[j]<=p[j]:maximum[j]=p[j]
   if not minimum[j]<=p[j]:minimum[j]=p[j]
 for p,q in zip(points,points[1:]):
  dx=f(q[0]-p[0]);dy=f(q[1]-p[1]);length=math.sqrt(f(f(dx*dx)+f(dy*dy)))
  arc+=length;sx+=float(f(q[0]+p[0]))*length;sy+=float(f(q[1]+p[1]))*length
 center=[f(sx*.5/arc),f(sy*.5/arc)]if arc>0 else points[0].copy()
 return minimum+maximum+center

def project(original_id,start,end_inclusive,values,end_of_original):
 # No managed/native pointer is invented. Opaque slots carry zero placeholders
 # with mask0; RangeOwnerPresent is separate logical DATA.
 b=bytearray(96);mask=bytearray(12)
 def known(at,n):
  for i in range(at,at+n):mask[i>>3]|=1<<(i&7)
 known(0,16);struct.pack_into('<II',b,0x18,1,1);known(0x18,8)
 b[0x20]=end_of_original;known(0x20,1)
 struct.pack_into('<6f',b,0x24,*values);known(0x24,24)
 assert sum(x.bit_count()for x in mask)==49
 return dict(recordHex=b.hex(),definedMaskHex=mask.hex(),rangeOwnerPresent=True,range=[original_id,start,end_inclusive],metricBits=list(map(bits,values)),endsOriginal=end_of_original)

def build(strokes,markers,original_ids):
 # Arguments are numerical fixture DATA. They cannot grant actual source identity.
 assert len(strokes)==len(original_ids)and original_ids==list(range(len(strokes)))
 out=[];members=[[]for _ in strokes]
 for i,(tag,stroke,start)in enumerate(markers):
  assert 0<=stroke<len(strokes)and 0<=start<len(strokes[stroke])
  if i:assert markers[i-1][1]<stroke or(markers[i-1][1]==stroke and markers[i-1][2]<start)
  end=markers[i+1][2]+1 if i+1<len(markers)and markers[i+1][1]==stroke else len(strokes[stroke])
  assert start<end<=len(strokes[stroke]);values=metric(strokes[stroke][start:end])
  # Literal writes previous flag when next marker arrives; last finalized below.
  if out:
   prev=out[-1];byte_=1 if start==0 else 0;record=bytearray.fromhex(prev['recordHex']);record[0x20]=byte_;prev['recordHex']=record.hex();prev['endsOriginal']=byte_
  out.append(project(original_ids[stroke],start,end-1,values,0));members[original_ids[stroke]].append(i)
 if out:
  record=bytearray.fromhex(out[-1]['recordHex']);record[0x20]=1;out[-1]['recordHex']=record.hex();out[-1]['endsOriginal']=1
 offsets=[0]
 for entries in members:
  assert entries==list(range(offsets[-1],offsets[-1]+len(entries)));offsets.append(offsets[-1]+len(entries))
 assert offsets[-1]==len(out)
 return dict(segments=out,perOriginalMembership=members,compressedOffsets=offsets,totalMembership=len(out),requiredWorkWords=(len(out)>>5)+1,allocatedQueryMasks=False,sourceAuthority=False)


def expected(root,models,fixture):
 # Hash domain is exact bytes with CRLF replaced by LF only. The sampler
 # then verifies its own inherited fourteen source-reference dependencies.
 trusted={'reference-dependencies.json': '7ab75579f8e2304924243b4fd856c112f61fc2d0246d008b5339dd95d32feebc', 'reference_limiter.py': '3525fd383519037635d84abcf8112904da2c8e790754bfc2d03f89535c3dfa51', 'reference_metrics.py': '6909380aafb3641cb9ec47dbe466501e0ec284111cb4281e5b0820018030640d', 'reference_sampler.py': '2c698f26340b8a94da7623293326c01981c7185a08f8d5e9258933d842fc8a97'}
 for name,sha in trusted.items():assert hashlib.sha256((root/'tools/LegacyMathAkOperator13A2.Probe'/name).read_bytes().replace(b'\r\n',b'\n')).hexdigest()==sha
 path=root/'tools/LegacyMathAkOperator13A2.Probe/reference_sampler.py'
 spec=importlib.util.spec_from_file_location('source_sampler_reference',path);sampler=importlib.util.module_from_spec(spec);spec.loader.exec_module(sampler)
 result=sampler.main(root,models,fixture);cases=[]
 for c in result['cases']:
  points=[[struct.unpack('<f',struct.pack('<i',v))[0]for v in c['PointBits'][i:i+2]]for i in range(0,len(c['PointBits']),2)]
  strokes=[];last=0
  for end in c['Ends']:strokes.append(points[last:end]);last=end
  markers=[c['MarkerFields'][i:i+3]for i in range(0,len(c['MarkerFields']),3)]
  tail=build(strokes,markers,list(range(len(strokes))));m=len(tail['segments']);s=len(strokes);visits=sum(r['range'][2]-r['range'][1]+1 for r in tail['segments'])
  cases.append(dict(name=c['name'],sampler=c,recordBytes=base64.b64encode(b''.join(bytes.fromhex(r['recordHex'])for r in tail['segments'])).decode(),definedMasks=base64.b64encode(b''.join(bytes.fromhex(r['definedMaskHex'])for r in tail['segments'])).decode(),rangeTriples=[v for r in tail['segments']for v in r['range']],originalOffsets=tail['compressedOffsets'],segmentCount=m,totalMembership=m,requiredWorkWords=tail['requiredWorkWords'],bridgeBytes=152+48*m+4*(s+1),bridgeWork=14*m+5*s+2*visits+21,diagnosticBytes=128+120*m+4*(s+1),diagnosticWork=320*m+16*s+128))
 return dict(resourceSHA=result['resourceSHA'],fixtureSHA=result['fixtureSHA'],cases=cases,sourceExecution=False,networkForwards=0)

def controls():
 out=[]
 for m in [0,1,2,31,32,33,63,64,65]:
  strokes=[[[float(i),0.]for i in range(m+1)]];markers=[[1 if i==0 else 2,0,i]for i in range(m)];r=build(strokes,markers,[0]);assert r['requiredWorkWords']==m//32+1;assert r['totalMembership']==m and r['compressedOffsets']==[0,m];out.append(dict(name='word-boundary-'+str(m),required=r['requiredWorkWords'],segments=m))
 r=build([[[0.,0.],[1.,0.],[4.,0.]]],[[1,0,0]],[0]);assert r['segments'][0]['metricBits'][4]==bits(2.);out.append(dict(name='arc-weighted-not-point-average',centerXBits=r['segments'][0]['metricBits'][4]))
 r=build([[[-0.,0.],[0.,-0.]]],[[1,0,0]],[0]);assert r['segments'][0]['metricBits']==[bits(-0.),bits(0.),bits(0.),bits(-0.),bits(-0.),bits(0.)];out.append(dict(name='zero-arc-signed-zero-ties',metrics=r['segments'][0]['metricBits']))
 r=build([[[0.,0.],[1.,0.],[2.,0.]],[[3.,1.],[4.,1.]]],[[1,0,0],[2,0,1],[1,1,0]],[0,1]);assert r['compressedOffsets']==[0,2,3]and[r['endsOriginal']for r in r['segments']]==[0,1,1];assert r['segments'][0]['range'][2]==r['segments'][1]['range'][1];out.append(dict(name='previous-end-not-current-start',flags=[v['endsOriginal']for v in r['segments']],offsets=r['compressedOffsets']))
 return dict(status='PASS',cases=out,totalControls=len(out),sourceAuthority=False,sourceExecution=False,networkForwards=0)

if __name__=='__main__':
 report=Path(sys.argv[-1]);assert not report.exists()
 if sys.argv[1]=='--controls':value=controls()
 else:value=expected(Path(sys.argv[1]),Path(sys.argv[2]),Path(sys.argv[3]))
 report.write_text(json.dumps(value,indent=2)+'\n',encoding='utf8');print(json.dumps(dict(sourceExecution=False,networkForwards=0)))
