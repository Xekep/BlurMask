"""Raw Math239/240/factor constants + independent synthetic negative-log arithmetic."""
from pathlib import Path
import hashlib,json,struct,sys
import numpy as np

GUARDS=['negative-probability','probability-above-one','nan-probability','infinite-probability',
        'target-below-zero','target-above240','zero-factor','nan-factor',
        'same-mapped-ids-not-source-identity','exact-source-ordinals-match']
def f(x):return np.float32(x)
def token(cost,p,target,factor):
    p=f(p)
    if target==239:p=f(p*factor)
    log=f(np.log(p,dtype=np.float32)) if p>=f(2**-126) else f(struct.unpack('<f',bytes.fromhex('50acaec2'))[0])
    return f(f(cost)-log)
def bits(x):return int(np.array([x],dtype=np.float32).view(np.uint32)[0])

def audit(q,raw):
    assert q['inputSha']==hashlib.sha256(raw).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
    assert q['actualNetworkForwards']==0 and q['mode']=='pure-rescore-controls' and q['guards']==GUARDS
    mappings=[struct.unpack_from('<290i',raw,x) for x in [592004,1004804]]
    counts=[sum(v==239 for v in x) for x in mappings]
    assert counts==[51,51] and all(240 not in m for m in mappings)
    factor=f(f(1)/f(counts[0]))
    assert q['IgnoreOrdinalCount']==counts[0] and bits(q['IgnoreFactor'])==bits(factor)==bits(q['backwardFactor'])
    mn=f(2**-126);below=np.nextafter(mn,f(0),dtype=np.float32)
    inputs=[(0,1,0),(0,.5,0),(0,.5,239),(0,.5,240),(0,0,0),
            (0,mn,0),(0,below,0),(0,mn,239),(0,-0.,0),(0,f(mn*64),239),(16777216,.5,0),(0,1,239)]
    assert len(q['cases'])==len(inputs)
    ulps=[]
    for c,(cost,p,target) in zip(q['cases'],inputs):
        assert bits(c['running'])==bits(cost) and bits(c['probability'])==bits(p) and c['target']==target
        expected=token(cost,p,target,factor);delta=abs(bits(c['result'])-bits(expected));assert delta<=1
        if p<=below or (p==mn and target==239):assert delta==0
        ulps.append(delta)
    order=q['ordered'];assert order['countPerRecord']==4096 and order['firstProbability']==0 and bits(order['secondProbability'])==bits(f(.9999))
    first=f(0);second=f(0)
    for _ in range(4096):first=token(first,0,0,factor)
    continuous=first
    for _ in range(4096):
        continuous=token(continuous,f(.9999),0,factor);second=token(second,f(.9999),0,factor)
    expected=dict(first=first,second=second,jointMean=f(continuous/f(2)),separateMean=f(f(first+second)/f(2)))
    for name,v in expected.items():assert abs(bits(order[name])-bits(v))<=1
    assert order['jointMean']!=order['separateMean']
    return dict(factor=float(factor),factorBits=f'{bits(factor):08x}',ignoreOrdinals=counts[0],scalarMaxUlp=max(ulps),
                orderedMean=float(expected['jointMean']),incorrectSeparateMean=float(expected['separateMean']))

def main():
    if len(sys.argv)!=4:raise SystemExit('MATH_AK SCORE_CONTROLS_JSON REPORT')
    raw=Path(sys.argv[1]).read_bytes()
    # System.Text.Json emits the valid numeric token -0 for Single negative zero.
    # Python's default integer parser would erase its IEEE sign before inspection.
    q=json.loads(Path(sys.argv[2]).read_text(),parse_int=lambda s:-0.0 if s=='-0' else int(s))
    result=audit(q,raw)
    negatives=[]
    for name,change in [
      ('factor',lambda x:x.__setitem__('IgnoreFactor',.02)),
      ('class239-count',lambda x:x.__setitem__('IgnoreOrdinalCount',50)),
      ('boundary-is-not-ignore',lambda x:x['cases'][3].__setitem__('result',x['cases'][2]['result'])),
      ('floor',lambda x:x['cases'][4].__setitem__('result',87.0)),
      ('multiply-before-floor',lambda x:x['cases'][7].__setitem__('result',1.0)),
      ('separate-subtotals',lambda x:x['ordered'].__setitem__('jointMean',x['ordered']['separateMean'])),
      ('guard-order',lambda x:x['guards'].reverse())]:
        a=json.loads(json.dumps(q));change(a)
        try:audit(a,raw)
        except AssertionError:negatives.append(name)
        else:raise AssertionError(name+' accepted')
    report=dict(rawFactor=result,scalarCases=12,guards=10,negativeControls=negatives,actualNetworkForwards=0,
                arithmeticScope='FP32 ordered arithmetic; MathF.Log vs NumPy logf within one ULP, not native CRT bits')
    Path(sys.argv[3]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS12scalar/10guards/7corruptions;0model forwards')
if __name__=='__main__':main()
