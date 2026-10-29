"""Exact diagnostic raw attention SHA/shape/float-bit audit, no tolerance."""
from pathlib import Path
import array,hashlib,json,math,struct,sys

def audit(sidecar_path,capture_path):
 raw=sidecar_path.read_bytes();s=json.loads(raw);capture_raw=capture_path.read_bytes();capture=json.loads(capture_raw)
 assert s['Schema']==2 and s['Scope']=='PROBE_DIAGNOSTIC_ONLY_NO_ENGINE_INGRESS_OR_NUMERIC_TOLERANCE'
 assert s['Complete']and s['LittleEndian']and s['CaptureSha256']==hashlib.sha256(capture_raw).hexdigest()
 assert len(s['Inputs'])==6 and [p['Argument']for p in s['Inputs']]==list(range(6))
 assert len(s['Cases'])==12;seen=set();rows=[]
 for case in s['Cases']:
  route,index=case['Route'],case['Case'];assert route in('Unchanged','Actual')and 0<=index<6 and(route,index)not in seen;seen.add((route,index))
  source=capture[route][index];nn=source['NewNn']
  assert(case['Width'],case['Reversed'],case['InputFrames'],case['DecoderCalls'])==(source['Width'],source['Reversed'],nn['Frames'],nn['DecoderCalls'])
  assert case['Width']==nn['Width']
  assert len(case['Frames'])==nn['DecoderCalls']==len(nn['RawHashes'])
  finite=nan=infinite=negative_zero=values=0
  for index,frame in enumerate(case['Frames']):
   assert frame['Frame']==index and frame['Segments']>0 and 0<frame['Beams']<=nn['Width']
   bits=frame['FloatBits'];assert len(bits)==frame['Segments']*frame['Beams']and all(type(v)is int and 0<=v<=0xffffffff for v in bits)
   words=array.array('I',bits);assert words.itemsize==4
   if sys.byteorder!='little':words.byteswap()
   data=words.tobytes()
   assert hashlib.sha256(data).hexdigest()==frame['Sha256']==nn['RawHashes'][index]
   for value, in struct.iter_unpack('<f',data):
    if math.isnan(value):nan+=1
    elif math.isinf(value):infinite+=1
    else:finite+=1
   negative_zero+=bits.count(0x80000000);values+=len(bits)
  rows.append(dict(Route=route,Case=case['Case'],Width=case['Width'],Reversed=case['Reversed'],Frames=len(case['Frames']),FloatCount=values,Finite=finite,NaN=nan,Infinite=infinite,NegativeZero=negative_zero))
 assert seen=={(r,i)for r in('Unchanged','Actual')for i in range(6)}
 # Fresh arrays are the already-executed source, not a recomputed baseline or
 # supplied tensor. Equality with the fully rehashed unchanged cohort checks
 # every bit/shape/hash and has no cross-platform numeric tolerance.
 assert len(s['Fresh'])==6 and sorted(c['Case']for c in s['Fresh'])==list(range(6))
 fresh_rows=[]
 for case in s['Fresh']:
  other=next(c for c in s['Cases']if c['Route']=='Unchanged'and c['Case']==case['Case'])
  assert case['Route']=='Fresh'
  for key in ['Width','Reversed','InputFrames','DecoderCalls','Frames']:
   assert case[key]==other[key],('SAME_PROCESS_FRESH_UNCHANGED_MISMATCH',case['Case'],key)
  fresh_rows.append(dict(Case=case['Case'],ExactFreshUnchanged=True,DecoderCalls=case['DecoderCalls']))
 return s,dict(SidecarSha256=hashlib.sha256(raw).hexdigest(),CaptureSha256=s['CaptureSha256'],Architecture=s['Architecture'],Framework=s['Framework'],Runtime=s['Runtime'],Rows=rows,FreshUnchanged=fresh_rows)

if __name__=='__main__':
 primary,report=audit(Path(sys.argv[1]),Path(sys.argv[2]));others=[]
 for sidecar in sys.argv[3:]:
  path=Path(sidecar);assert path.name.endswith('.attention.json')
  other,receipt=audit(path,Path(str(path)[:-len('.attention.json')]))
  assert other['Cases']==primary['Cases'],'Same-platform actual float bits or dimensions differ'
  assert other['Fresh']==primary['Fresh'],'Same-platform fresh float bits or dimensions differ'
  assert other['Inputs']==primary['Inputs']and other['CaptureSha256']==primary['CaptureSha256']
  assert other['Runtime']==primary['Runtime']and other['Architecture']==primary['Architecture']
  others.append(receipt)
 print(json.dumps(dict(PASS=True,Scope='DIAGNOSTIC_EXACT_FLOAT_BITS_SHA_AND_SAME_PLATFORM_REPLAY_NO_TOLERANCE',Primary=report,ExactOtherCaptures=others)))
