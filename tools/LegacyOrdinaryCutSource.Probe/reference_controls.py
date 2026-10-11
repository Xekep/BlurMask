"""Own varied ingress/geometry controls; independent shoelace/ray-crossing oracle."""
import argparse,importlib.util,json,math,random,struct
from pathlib import Path

def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def main():
    ap=argparse.ArgumentParser();ap.add_argument('output');a=ap.parse_args()
    path=Path(__file__).resolve().parents[1]/'LegacySourceEnclosure.Probe/reference_controls.py'
    spec=importlib.util.spec_from_file_location('enclosure_oracle',path);oracle=importlib.util.module_from_spec(spec);spec.loader.exec_module(oracle)
    rng=random.Random(106219);cases=[]
    for case in range(256):
        cx=f(rng.uniform(-70,70));cy=f(rng.uniform(-70,70));rx=f(rng.uniform(15,50));ry=f(rng.uniform(15,50));n=rng.randint(12,45)
        ring=[[f(cx+rx*math.cos(math.tau*j/n)),f(cy+ry*math.sin(math.tau*j/n))] for j in range(n+1)]
        ring[-1]=ring[0].copy()
        if case%7==0:ring=ring[::-1]
        inside=[[f(cx+rx*rng.uniform(-.25,.25)),f(cy+ry*rng.uniform(-.25,.25))] for j in range(7)]
        outside=[[f(cx+rx*2),f(cy-ry)],[f(cx+rx*2),f(cy+ry)]]
        strokes=[ring,inside,outside]
        if case%3==0:strokes=strokes[::-1]
        raw=[];indices=[];filtered=[]
        for stroke in strokes:
            points=[]
            for p in stroke:
                points.append(p)
                if rng.random()<.5:points.extend([p.copy(),p.copy()])
            keep=[0]+[j for j in range(1,len(points)) if any(abs(points[j][k])+abs(points[j-1][k])<abs(f(points[j][k]-points[j-1][k]))*2**23 for k in range(2))]
            raw.append(points);indices.append(keep);filtered.append([points[j] for j in keep])
        cases.append({'Strokes':raw,'Indices':indices,'Probabilities':[oracle.evaluate(filtered,s) for s in range(3)],'Baseline':f(cy+ry),'Height':f(2*ry)})
    out=Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps({'Cases':cases}),encoding='utf8');print('Generated',len(cases),'ordinary ingress/cut-source controls')
if __name__=='__main__':main()
