"""Pure byte/sequence and synthetic arithmetic evidence; NEVER runs a model forward."""
from pathlib import Path
import argparse, hashlib, importlib.util, json, math, struct, sys

def sha(x): return hashlib.sha256(x).hexdigest()
def f(x):
    try: return struct.unpack('<f', struct.pack('<f', x))[0]
    except OverflowError: return math.copysign(math.inf, x)
def sum32(xs):
    total = 0.0
    for x in xs: total = f(total + x)
    return total
def sigmoid(x, clamp):
    if clamp: x = max(-88.0, min(88.0, x))
    try: exponential = f(math.exp(f(-x)))
    except OverflowError: exponential = math.inf
    return f(1.0 / f(1.0 + exponential))

def main():
    p=argparse.ArgumentParser(); p.add_argument('repo', type=Path); p.add_argument('output',type=Path); a=p.parse_args()
    full=(a.repo/'models/math/math-ak.res').read_bytes()
    assert len(full)==3685881 and sha(full)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
    scripts=Path(__file__).resolve().parents[1]/'handwriting'; sys.path.insert(0,str(scripts))
    import native_network_reference as reference
    records=json.loads((Path(__file__).with_name('actual-networks.json')).read_text())
    maps=[]; reports=[]; tensor_count=0; sequence_controls=0
    for reverse,(offset,end,payload,map_at,record) in enumerate(zip(
        [180366,593168],[593164,1005964],[180512,593312],[592004,1004804],records)):
        assert struct.unpack_from('<I',full,offset+8)[0]==0x5f
        assert struct.unpack_from('<H',full,offset+14)[0]==0x106
        assert record['offset']==offset and record['end']==end
        assert record['networks']==[dict(stored_layers=[dict(kind=1,width=50,opaque_u32=0,storage_code=1),dict(kind=3,width=100,opaque_u32=0,storage_code=1)],edge_records=[])]
        assert record['per_layer_codes']==[0,1,3] and record['global_code']==2
        record['file']='models/math/math-ak.res'
        old_root=reference.ROOT; reference.ROOT=a.repo
        try: parsed=reference.load_network(record)
        finally: reference.ROOT=old_root
        # This independently existing reader partitions matrices and peepholes.
        expected=[(100*56*4)]*4+[(100*104*4)]*4+[104*4]*3+[241*104*4,241*56*4]
        assert [t['bytes'] for t in parsed['tensors']]==expected
        at=payload; tensors=[]
        for t,size in zip(parsed['tensors'],expected):
            assert t['offset']==at-payload
            tensors.append(dict(name=t['name'],offset=at,bytes=size,sha256=sha(full[at:at+size])))
            at+=size; tensor_count+=1
        assert at==map_at-4 and at-payload==411488
        assert struct.unpack_from('<I',full,at)[0]==1160
        mapping=struct.unpack_from('<290i',full,map_at); maps.append(mapping)
        assert min(mapping)==0 and max(mapping)==239 and len(set(mapping))==240
        assert sha(full[map_at:end])=='2c8af703230cede3fb10375a178246f1c04c838af09012f1734a7d93133ab3e1'
        embedding_at=at-241*56*4
        for row in range(241):
            assert struct.unpack_from('<6f',full,embedding_at+(row*56+50)*4)==(0.,)*6
        inputs_digest=hashlib.sha256()
        for ordinals in [[],[0],[1,17,289],list(range(290)),[i%290 for i in range(4095)]]:
            ordered=list(reversed(ordinals)) if reverse else ordinals
            ids=[240]+[mapping[i] for i in ordered]+[240]
            assert len(ids)==len(ordinals)+2
            frames=len(ordinals)+1
            # Every source ordinal maps to exactly one input row after boundary.
            # The final sentinel is target-only; no extra embedded frame is appended.
            feature_bytes=b''.join(full[embedding_at+i*56*4:embedding_at+i*56*4+50*4] for i in ids[:-1])
            expected_features=parsed['input_lookup'][ids[:-1],:50].astype('<f4').tobytes()
            assert feature_bytes==expected_features and len(feature_bytes)==frames*50*4
            assert ids[1:][-1]==240 and len(ids[1:])==frames
            inputs_digest.update(feature_bytes); sequence_controls+=1
        reports.append(dict(reverseInterior=bool(reverse),frame=[offset,end],tensors=tensors,
                            mappingSha256=sha(full[map_at:end]),preparedFeaturesSha256=inputs_digest.hexdigest()))
    assert maps[0]==maps[1]
    # Closed policy choices only. This is not a vendor transcendental/FTZ reference.
    sequential=sum32([1.]+[2**-25]*240)
    regrouped=f(1.+sum32([2**-25]*240))
    assert sequential==1. and regrouped>sequential
    assert sigmoid(-90.,False)==0 and sigmoid(-90.,True)>0
    assert sigmoid(0.,False)==sigmoid(-0.,False)==0.5
    assert sigmoid(math.inf,False)==1 and sigmoid(-math.inf,False)==0
    subnormal=f(2**-149); assert subnormal>0 and f(subnormal+subnormal)>subnormal
    report=dict(scope='raw parsing and prepared inputs; no model inference',resourceSha256=sha(full),
                tensorCount=tensor_count,preparedSequenceControls=sequence_controls,models=reports,
                synthetic=dict(sequential=sequential,regrouped=regrouped,
                               unclampedMinus90=sigmoid(-90.,False),oldClampedMinus90=sigmoid(-90.,True),
                               minSubnormal=subnormal,nativeFtzEmulated=False),
                actualModelForwardCalls=0,vendorExecution=False)
    a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print('PASS: 26 tensor byte partitions, 10 prepared sequences, scalar-policy controls; 0 model forwards')

if __name__=='__main__': main()
