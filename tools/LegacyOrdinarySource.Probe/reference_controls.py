"""Vendor-free native167210 adjacency filter/map reference with raw float32 ULP controls."""
import argparse,json,random,struct
from pathlib import Path
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<I',struct.pack('<f',v))[0]
def frombits(v):return struct.unpack('<f',struct.pack('<I',v))[0]
def main():
    ap=argparse.ArgumentParser();ap.add_argument('output');a=ap.parse_args();r=random.Random(219167);cases=[]
    for i in range(512):
        strokes=[];indices=[]
        for s in range(r.randint(1,4)):
            base=f(r.uniform(10,10000));p=[[base,f(base/2)]]
            for j in range(r.randint(3,25)):
                if j%5==0:p.append(p[-1].copy())
                elif j%5 in [1,2,3]:
                    # Adjacent one-ULP steps often disappear; a retained-last-point
                    # implementation incorrectly retains later accumulated steps.
                    p.append([frombits(bits(p[-1][0])+r.randint(0,3)),p[-1][1]])
                else:p.append([f(p[-1][0]+r.uniform(-5,5)),f(p[-1][1]+r.uniform(-5,5))])
            out=[0]
            for j in range(1,len(p)):
                if any(abs(p[j][k])+abs(p[j-1][k])<abs(f(p[j][k]-p[j-1][k]))*2**23 for k in range(2)):out.append(j)
            strokes.append(p);indices.append(out)
        cases.append({'Strokes':strokes,'Indices':indices})
    out=Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps({'Cases':cases}),encoding='utf8');print('Generated',len(cases),'vendor-free raw input controls')
if __name__=='__main__':main()
