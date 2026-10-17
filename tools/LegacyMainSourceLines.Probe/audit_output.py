"""Independent021AB0 append/write replay from actual raw descriptor/guide bytes."""
import base64,json,math,struct,sys
from pathlib import Path
capture=json.loads(Path(sys.argv[1]).read_bytes());b64=base64.b64decode;calls=discriminators=0
f32=lambda b,p:struct.unpack_from('<f',b,p)[0]
for sample in capture['samples']:
 descriptor=b64(sample['descriptor88']);guide=b64(sample['guide40'])
 assert len(descriptor)==88 and len(guide)==40 and descriptor[0]&15==0
 line=struct.unpack_from('<i',descriptor,16)[0];assert line==0
 slope=f32(guide,8);baseline=f32(guide,12);height=f32(guide,16)
 corrected_baseline=f32(guide,20);corrected_height=f32(guide,24)
 assert sample['rotation']==0 # Fresh12D000C forwarding/static proof, not guessed fit.
 intercept=corrected_baseline if corrected_height>0 else baseline
 effective_slope=0 if corrected_height>0 else slope
 effective_height=corrected_height if corrected_height>0 else height
 angle=math.atan2(effective_slope,1.0)+0.0
 row=bytearray(56)
 struct.pack_into('<4dq',row,8,-0.0*intercept,intercept*1.0,angle,effective_height,-1)
 struct.pack_into('<d',row,48,baseline)
 state=bytearray();assert len(sample['calls'])==2
 for ordinal,call in enumerate(sample['calls']):
  assert call['before']==ordinal and call['count']==ordinal+1
  assert call['wrapperBefore']==0 and call['wrapperAfter']==1
  state.extend(bytes(56));state[0:56]=row # append index != wrapper write index
  actual=b64(call['storage56']);assert len(actual)==len(state)
  # Portable libm atan2 may vary by a few ULP; every other bit is exact.
  assert actual[:24]==state[:24]and actual[32:]==state[32:]
  actual_angle=struct.unpack_from('<d',actual,24)[0]
  assert math.isclose(actual_angle,angle,rel_tol=2e-15,abs_tol=2e-16)
  calls+=1
 final=b64(sample['storage56']);assert final==b64(sample['calls'][-1]['storage56'])and final[56:]==bytes(56)
 if f32(guide,4)!=height:discriminators+=1
 assert effective_height>0
assert len(capture['samples'])==4 and discriminators==capture['heightDomainDiscriminators']and discriminators>0
print(f'PASS independent4 actual source owners/{calls} native calls/raw88+40/full56B append-write fields/{discriminators} pre-deskew height discriminators; no bbox/expected text.')
