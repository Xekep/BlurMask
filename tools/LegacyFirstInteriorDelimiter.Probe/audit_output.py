"""Independent actual-first alphabet/category and owned-range geometry oracle."""
import hashlib,json,pathlib,struct,sys
raw=pathlib.Path(sys.argv[1]).read_bytes();capture=json.loads(pathlib.Path(sys.argv[2]).read_bytes())
assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
p=6219;header=struct.unpack_from('<8H',raw,p);p+=16;assert header[-1]==192
n=struct.unpack_from('<H',raw,p)[0];p+=2
for step in (32,64,1):
    values=struct.unpack_from('<'+'H'*n,raw,p);p+=n*2;n=[v for v in values if v!=65535][-1]+step
spans=struct.unpack_from('<'+'H'*(n+1),raw,p);p+=2*(n+1)
rows=[struct.unpack_from('<II',raw,p+i*8) for i in range(spans[-1]+1)];p+=8*(spans[-1]+1)
strings=raw[p:p+rows[-1][0]]
labels={strings[off:rows[i+1][0]].decode('utf8'):label for i,(off,label) in enumerate(rows[:-1])}
p=11571;assert struct.unpack_from('<HH',raw,p)==(0x201,192);p+=4
n=raw[p];p=(p+1+3)&~3;ids=struct.unpack_from('<'+'i'*n,raw,p);p+=4*n
first=struct.unpack_from('<195b',raw,p)
policies=[0,1,4,2,4,1,0, 2,2,2,2,2,0,0, 1,1,2,1,2,0,0,
 3,1,2,2,2,0,0, 1,1,2,1,2,0,0, 2,0,0,0,0,0,0, 0,0,0,0,0,0,0]
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<f',struct.pack('<I',v&0xffffffff))[0]
calls=emitted=groups=0
for source in capture['sources']:
    points=[[bits(x),bits(y)] for x,y in source['points']];starts=[0]
    for n in source['counts']:starts.append(starts[-1]+n)
    assert len(points)==starts[-1]
    boxes=[]
    for group in source['groups']:
        selected=[pt for stroke,a,z in group['ranges'] for pt in points[starts[stroke]+a:starts[stroke]+z]]
        box=[min(p[0] for p in selected),min(p[1] for p in selected),max(p[0] for p in selected),max(p[1] for p in selected)]
        assert box==list(map(bits,group['box']));boxes.append(box);groups+=1
    h=bits(source['heightBits'])
    for before,current,previous,symbol,start,flags,count,output,component,external in source['requests']:
        assert first[symbol]>=0 and (previous==65535 or first[previous]>=0)
        policy=policies[ids[first[symbol]]+7*(0 if previous==65535 else ids[first[previous]])]
        dash=previous in [labels['-'],labels['/']] or symbol in [labels['-'],labels['/']]
        if symbol in [labels['#'],labels['@']] or dash:policy=5
        elif policy==4:policy=2
        if previous==labels['-'] and start==0:emit=True
        else:
            if policy==3:policy=2 if start else 1
            a,b=boxes[current],boxes[before]
            distance=f(b[0]-a[2]) if a[0]<=b[0] else f(a[0]-b[2])
            wide=f(h*f(2.95));narrow=f(h*f(.29))
            emit=wide<distance if policy==2 else narrow<=distance if policy==1 else f(f(wide+narrow)*f(.5))<distance if policy==5 else True
            #Both extra neighbors are -1 in the actual1F42 request, so their
            #native FLT_MAX distances preserve a successful dash decision.
        emit=emit or bool(flags&8)
        assert count==int(emit) and output==(65531 if emit else 65535)
        assert component==external==0
        calls+=1;emitted+=emit
assert calls==capture['calls']==6144 and capture['guards']==27
print(f'PASS independent raw first AL/category policy: {calls} prepared requests/{emitted} FFFB; {groups} owned range bounds from point buffers; no selected-text claim')
