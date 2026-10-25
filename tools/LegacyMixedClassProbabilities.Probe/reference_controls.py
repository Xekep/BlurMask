"""FAKE mathematical integer-vector controls only. No model/resource/capture,
weights, neural forward, SDK or source owner is read or invoked by this script.
"""
from pathlib import Path
import argparse,decimal,hashlib,json,math,struct
BIAS=4194304;THRESHOLD=(351<<16)-BIAS
def f32(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def sigmoid(raw):
 x=f32(f32(f32(raw)*-2.0)/1048576.0)
 if x>75:return 0,dict(exponentBits=struct.unpack('<I',struct.pack('<f',x))[0],reason='native75cutoff')
 with decimal.localcontext()as ctx:
  ctx.prec=110;D=decimal.Decimal;e=ctx.exp(D.from_float(x));rel=D(2)**-40;absolute=D(2)**-1074
  lower=max(D(0),e*(1-rel)-absolute);upper=e*(1+rel)+absolute
  lo=D(32768)/(1+upper)+D('.5');hi=D(32768)/(1+lower)+D('.5')
  point=D(32768)/(1+e)+D('.5');fraction=point-int(point);margin=min(fraction,1-fraction)
  assert int(lo)==int(hi) and margin>D(2)**-24,'Synthetic case outside explicit rounded-exp admission assumption'
  return int(lo),dict(exponentBits=struct.unpack('<I',struct.pack('<f',x))[0],roundedLower=str(lo),roundedUpper=str(hi),integerMargin=str(margin),reason='scoped-relative2^-40-plus-subnormal-exp')
def table():
 #Independent high-precision values for the EXISTING mathematical EXP table.
 #This is not a native table dump or proof of universal vendor CRT identity.
 with decimal.localcontext()as ctx:
  ctx.prec=100;D=decimal.Decimal
  return [int(D(25)*ctx.exp(D.from_float(-4.0+22.0*i/351.0)))for i in range(352)]
def normalize(raw,special=None,canceled=False):
 mixed=special is not None
 if not (2<=len(raw)<=512 if mixed else 1<=len(raw)<=4096)or(mixed and not 0<=special<len(raw)):
  return dict(Exception='ArgumentException',TokenMatches=False,Q15=[],Offset=None)
 if canceled:return dict(Exception='OperationCanceledException',TokenMatches=True,Q15=[],Offset=None)
 use=[i for i in range(len(raw))if i!=special];offset=max(0,max(raw[i]for i in use)-THRESHOLD)
 if offset>2147483647 or any(raw[i]-offset< -2147483648 for i in use):
  return dict(Exception='NotSupportedException'if mixed else'ArgumentException',TokenMatches=False,Q15=[],Offset=None)
 exp=table();values=[0]*len(raw)
 for i in use:
  shifted=raw[i]-offset+BIAS
  if mixed and shifted>2147483647:return dict(Exception='NotSupportedException',TokenMatches=False,Q15=[],Offset=None)
  values[i]=0 if shifted<0 else exp[min(351,shifted>>16)]
 halvings=0
 while sum(values)>2147483647:values=[v>>1 for v in values];halvings+=1
 maximum=max(values)
 if maximum==0:
  if mixed:return dict(Exception='NotSupportedException',TokenMatches=False,Q15=[],Offset=None)
  return dict(Exception='none',TokenMatches=False,Q15=[0]*len(raw),Offset=offset)
 total=sum(values);ss=max(0,total.bit_length()-1-6);divisor=total>>ss;vs=31-maximum.bit_length()
 output=[min((((v<<vs)&0xffffffff)//divisor)>>((vs+ss-15)&31),32767)for v in values]
 if mixed:output[special]=sigmoid(raw[special])[0]
 return dict(Exception='none',TokenMatches=False,Q15=output,Offset=None if mixed else offset)
def make():
 mixed=[];generic=[];sig=[]
 def add(name,raw,special,cancel=False):mixed.append(dict(Name=name,Raw=raw,Special=special,Canceled=cancel,Expected=normalize(raw,special,cancel)))
 for width in [2,3,7,180,512]:
  for special in sorted({0,width//2,width-1}):
   for raw_special in [-2147483648,-40000000,-1,0,1,40000000,2147483647]:
    raw=[0]*width;raw[special]=raw_special;add(f'fake_w{width}_special{special}_dot{raw_special}',raw,special)
 add('fake_heavy_ordinary_halving',[0]+[THRESHOLD]*511,0)
 add('fake_stabilized_ordinary',[THRESHOLD+1048576,THRESHOLD+65536,0,-1048576],2)
 add('fake_exclude_special_max',[0,2147483647,0],1)
 add('fake_exclude_special_min',[0,-2147483648,0],1)
 for name,raw,special,cancel in [('allzero_ordinary',[-BIAS-1,40000000],1,False),('allzero_tableindex0',[-BIAS,0],1,False),
  ('ordinary_subtract_overflow',[-2147483648,2147483647,0],2,False),('empty',[],0,False),('single',[0],0,False),
  ('invalidSpecial',[0,0],2,False),('negativeSpecial',[0,0],-1,False),('cancel',[0,0],0,True),('shapeBeforeCancel',[],0,True)]:add(name,raw,special,cancel)
 for name,raw,cancel in [('zero',[0,0],False),('unit',[0],False),('ordinary',[0,65536,-65536,1048576],False),
  ('stabilized',[THRESHOLD+1048576,THRESHOLD,0],False),('allzero',[-BIAS-1,-BIAS-1000],False),
  ('overflow',[-2147483648,2147483647],False),('cancel',[0,0],True),('empty',[],False),('emptyBeforeCancel',[],True)]:
  generic.append(dict(Name='generic_'+name,Raw=raw,Canceled=cancel,Expected=normalize(raw,None,cancel)))
 for value in [-2147483648,-40000000,-39321604,-39321600,-16777217,-16777216,-1,0,1,16777216,16777217,40000000,2147483647]:
  q,evidence=sigmoid(value);sig.append(dict(Raw=value,Expected=q,Evidence=evidence))
 assert any(32768 in c['Expected']['Q15']for c in mixed)
 return dict(Version=1,Scope='FAKE_PROBABILITY_ONLY; no resources/weights/observed dots',RoundedExpAssumption='relative2^-40 plus least-subnormal absolute allowance; intervals collapse; integer margin>2^-24',
  MixedRows=mixed,GenericRows=generic,SigmoidRows=sig,MathematicalExpTableSha256=hashlib.sha256(struct.pack('<352i',*table())).hexdigest())
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('output');a=p.parse_args();out=Path(a.output);assert not out.exists(),'fresh fixture required'
 data=(json.dumps(make(),separators=(',',':'))+'\n').encode();out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes(data)
 j=json.loads(data);print(json.dumps(dict(scope=j['Scope'],bytes=len(data),sha256=hashlib.sha256(data).hexdigest(),mixed=len(j['MixedRows']),generic=len(j['GenericRows']),sigmoid=len(j['SigmoidRows']))))
