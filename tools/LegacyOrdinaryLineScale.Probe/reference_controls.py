"""Independent raw-adjacent ingress→unlimited source extrema→source-profile scale controls."""
import importlib.util
import json
from pathlib import Path
import random
import struct
import sys

def module(name,folder):
    spec=importlib.util.spec_from_file_location(name,Path(__file__).parents[1]/folder/'reference_controls.py')
    out=importlib.util.module_from_spec(spec);spec.loader.exec_module(out);return out
sample=module('sample_reference','LegacyLineSamples.Probe')
geometry=module('geometry_reference','LegacyLineGeometry.Probe')
f=sample.f
def clean(points):
    return [p for i,p in enumerate(points) if i==0 or any(abs(p[k])+abs(points[i-1][k])<abs(f(p[k]-points[i-1][k]))*8388608 for k in range(2))]
def main():
    output=Path(sys.argv[1]);output.parent.mkdir(parents=True,exist_ok=True)
    rng=random.Random(219159);controls=[]
    for index in range(384):
        strokes=[]
        for s in range(1+index%5):
            points=[[f(rng.uniform(-1000,1000)),f(rng.uniform(-1000,1000))] for _ in range(1+index%17)]
            if index%7==0:
                for p in points:p[1]=f(20)
            if index%11==0:points=[p for p in points for _ in range(3)]
            strokes.append(points)
        profile=list(map(f,[[.96,.85,3,10],[0,.5,0,1],[1,1,5,4]][index%3]));resolution=[0,96,1200][index%3]
        samples=[x for stroke in strokes for x in sample.extract(clean(stroke),False)]
        controls.append(dict(strokes=strokes,profile=profile,resolution=resolution,samples=samples,scales=geometry.scales(samples,0,resolution,profile)))
    output.write_text(json.dumps(controls,separators=(',',':')),encoding='utf8')
    print(f'Wrote {len(controls)} independent raw-source scale controls.')
if __name__=='__main__':main()
