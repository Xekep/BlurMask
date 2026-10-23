"""Independent raw-map and synthetic arithmetic checks, without model inference."""
from pathlib import Path
import hashlib,json,struct,sys
import numpy as np

NAMES=['empty-boundary-frame-target','forward-three-ids','reverse-interior-only','four-input-frames',
 'next-position-targets','caller-ordinal-snapshot','output-id-snapshot','maximum4095','above4095',
 'negative-ordinal','ordinal290','input-negative','target-end','foreign-record-owner-no-forward',
 'read-pre-cancel','prepare-pre-cancel','evaluate-pre-cancel-no-forward','resource-sha',
 'resource-truncated','retained-model-after-input-mutation','parallel-immutable-preparation',
 'historical-default-policy-metadata','historical-versus-sequential-different',
 'historical-normalized-finite','scalar-sequential-leading-one','unclamped-minus90-versus-old',
 'signed-zero','positive-negative-infinity','nan-remains-nan']

def audit(p,raw):
    assert p['inputSha']==hashlib.sha256(raw).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
    assert p['actualNetworkForwards']==0 and p['syntheticNetworkForwards']==2
    assert p['mode']=='prepared-and-synthetic-only' and p['checks']==NAMES
    for reverse,offset,key in [(False,592004,'forwardIds'),(True,1004804,'reverseIds')]:
        mapping=struct.unpack_from('<290i',raw,offset)
        ordinals=[2,1,0] if reverse else [0,1,2]
        assert p['prepared'][key]==[240]+[mapping[i] for i in ordinals]+[240]
    # Independent NumPy transcendental + reduction reference for a hand-written
    # affine. One ULP permits MathF.Exp versus NumPy expf approximation only.
    # Leading-probability golden is exact, and reduction policy is distinguished.
    logits=np.full(241,np.float32(-17.32868),dtype=np.float32);logits[0]=0
    exponentials=np.exp(logits,dtype=np.float32)
    historical=exponentials/np.sum(exponentials,dtype=np.float32)
    sequential=np.float32(0)
    for e in exponentials:sequential=np.float32(sequential+e)
    scalar=exponentials/sequential
    ulps={}
    for name,expected in [('historical',historical),('scalar',scalar)]:
        actual=np.array(p['synthetic'][name],dtype=np.float32)
        assert actual.shape==(241,) and np.all(np.isfinite(actual)) and np.all(actual>0)
        assert actual[0].view('u4')==expected[0].view('u4')
        delta=np.abs(actual.view('u4').astype('i8')-expected.view('u4').astype('i8'))
        assert np.max(delta)<=1
        ulps[name]=int(np.max(delta))
    assert p['synthetic']['oldMinus90']>0 and p['synthetic']['unclampedMinus90']==0
    return ulps

def main():
    if len(sys.argv)!=4:raise SystemExit('MATH_AK PREPARED_JSON REPORT')
    raw=Path(sys.argv[1]).read_bytes();p=json.loads(Path(sys.argv[2]).read_text());ulps=audit(p,raw)
    negatives=[]
    for name,change in [
      ('guard-order',lambda q:q['checks'].reverse()),
      ('boundary-target',lambda q:q['prepared']['forwardIds'].__setitem__(-1,239)),
      ('interior-reverse',lambda q:q['prepared']['reverseIds'].__setitem__(1,0)),
      ('policy-regression',lambda q:q['synthetic'].__setitem__('scalar',q['synthetic']['historical'])),
      ('probability-corruption',lambda q:q['synthetic']['historical'].__setitem__(100,.001)),
      ('unclamped-regression',lambda q:q['synthetic'].__setitem__('unclampedMinus90',1e-39)),
      ('unexpected-actual-forward',lambda q:q.__setitem__('actualNetworkForwards',1))]:
        q=json.loads(json.dumps(p));change(q)
        try:audit(q,raw)
        except AssertionError:negatives.append(name)
        else:raise AssertionError('mutation accepted: '+name)
    report=dict(guards=len(NAMES),negativeControls=negatives,syntheticMaxUlp=ulps,actualNetworkForwards=0,
                transcendentalScope='MathF versus NumPy one ULP for this synthetic constant, not native CRT parity')
    Path(sys.argv[3]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print('PASS29 guards/7 negatives/old exact leading golden;0 actual forwards')
if __name__=='__main__':main()
