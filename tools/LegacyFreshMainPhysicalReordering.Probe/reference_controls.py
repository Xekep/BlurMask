"""Independent actual NN per-descriptor state/interval/map composition."""
import importlib.util,json,sys
from pathlib import Path
import numpy as np
f=np.float32

def load(name,folder):
    spec=importlib.util.spec_from_file_location(name,Path(__file__).parents[1]/folder/'reference_controls.py');value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value
ref=load('reorder_sweep','LegacyLineSweep.Probe');split=load('reorder_split','LegacyFreshMainDescriptorSplit.Probe');interval=load('reorder_interval','LegacyPhysicalIntervalOrder.Probe')

def receiver(points,carry,models):
    #The receiver gets an already supplied positive primary scale. This swaps
    #only that input scalar; the original independent NumPy features/BLSTM,
    #internal override/sweep/postselection implementation remains unchanged.
    old=ref.source_ref.geometry.scales
    ref.source_ref.geometry.scales=lambda *args:(carry,carry)
    try:return ref.whole(points,[False]*len(points),True,f(.8),models)
    finally:ref.source_ref.geometry.scales=old

def prepare(case,models):
    first=split.prepare(case);initialorder=first['order'];points=[case['clean'][i] for i in initialorder]
    carry=f(case['expected']['neuralScale']);calls=[];current=[]
    for d,record in enumerate(first['descriptors']):
        b=bytes.fromhex(record)
        if d%2:continue
        import struct
        start,end=struct.unpack_from('<ii',b,36)
        if start==end:current.append(start);continue
        subset=points[start:end+1];initial=carry;groups=receiver(subset,carry,models);carry=f(groups['neuralScale'])
        boxes=[[min(p[0] for p in s),max(p[0] for p in s)] for s in subset]
        ids=interval.order(boxes,groups['groups'],float(case['primary']),.25)
        current += [start+i for i in ids]
        calls.append([d,float(initial),float(carry),len(subset),len(groups['groups']),groups['strips']])
    counts=[len(s) for s in case['clean']];starts=[];total=0
    for n in counts:starts.append(total);total+=n
    original=[initialorder[i] for i in current]
    return dict(original=original,calls=calls,ticks=[200*(starts[i]+p) for i in original for p in range(counts[i])],descriptors=first['descriptors'])

def main():
    if len(sys.argv)!=5 or Path(sys.argv[2]).suffix.lower()!='.json':raise ValueError('Supply group-controls.json output-reorder.json spatial30.res physical50.res')
    data=json.loads(Path(sys.argv[1]).read_bytes());models=[ref.proposal_ref.network.load(Path(v)) for v in sys.argv[3:5]]
    for trial in range(12):
        raw=[]
        for row in ([1,0] if trial%2 else [0,1]):
            for physical in range(3):
                height=[20,90][row];n=8+(trial%3)*2
                raw.append([[float(f(physical*60+p*3)),float(f(row*1000+(height if p%2 else 0)))] for p in range(n)])
        if trial%3==0:raw=raw[::-1]
        clean=[ref.source_ref.clean(s) for s in raw]
        samples=[v for st in clean for v in ref.source_ref.sample.extract(st,False)]
        scales=ref.source_ref.geometry.scales(samples,0,96,list(map(f,[.96,.85,3,10])))
        data['cases'].append(dict(strokes=raw,clean=clean,primary=float(f(scales[0])),secondary=float(f(scales[1])),expected=ref.whole(raw,[False]*len(raw),True,f(.8),models)))
    for case in data['cases']:case['reorder']=prepare(case,models)
    Path(sys.argv[2]).write_bytes((json.dumps(data,separators=(',',':'))+'\n').encode('utf8'))
    print(f'Wrote {len(data["cases"])} independent actual per-descriptor NN carry/order/key controls.')
if __name__=='__main__':main()
