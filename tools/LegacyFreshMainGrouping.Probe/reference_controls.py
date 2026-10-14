"""Independent actual2E3 model/float32 memberships oracle; no vendor code execution."""
import importlib.util,json,hashlib,sys
from pathlib import Path
import numpy as np
f=np.float32
spec=importlib.util.spec_from_file_location('whole_reference',Path(__file__).parents[1]/'LegacyLineSweep.Probe'/'reference_controls.py')
ref=importlib.util.module_from_spec(spec);spec.loader.exec_module(ref)

def main():
    if len(sys.argv)!=4 or Path(sys.argv[1]).suffix.lower()!='.json':
        raise ValueError('Supply one output.json followed by spatial30.res and physical50.res')
    output=Path(sys.argv[1]); model_paths=[Path(v) for v in sys.argv[2:4]]
    models=[ref.proposal_ref.network.load(p) for p in model_paths]
    printed=[[[0,90],[0,10],[25,60],[50,10],[50,90]],[[70,10],[70,90],[120,10],[120,90]],[[140,90],[140,10],[180,10],[190,20],[190,40],[180,50],[140,50]]]
    cases=[]
    for i in range(32):
        rows=1+i%3; raw=[]
        for row in (list(range(rows))[::-1] if i%2 else range(rows)):
            for stroke in printed[:1+i%3]:
                translated=[[float(f(x+float(f(i*.125)))),float(f(y+[0,250,8000][i%3]*row))] for x,y in stroke]
                if i%4==0: translated.insert(2,translated[1][:])
                raw.append(translated)
        if i%5==0:raw=raw[::-1]
        clean=[ref.source_ref.clean(s) for s in raw]
        samples=[v for s in clean for v in ref.source_ref.sample.extract(s,False)]
        scales=ref.source_ref.geometry.scales(samples,0,96,list(map(f,[.96,.85,3,10])))
        cases.append(dict(strokes=raw,clean=clean,primary=float(f(scales[0])),secondary=float(f(scales[1])),
            expected=ref.whole(raw,[False]*len(raw),True,f(.8),models)))
    output.parent.mkdir(parents=True,exist_ok=True)
    output.write_bytes((json.dumps(dict(cases=cases,models=[dict(path=str(p.resolve()),sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in model_paths]),separators=(',',':'))+'\n').encode('utf8'))
    print(f'Wrote {len(cases)} independent actual-model memberships controls.')
if __name__=='__main__':main()
