"""Independent prepared-request oracle; raw AK supplies labels and category IDs."""
import hashlib,json,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('AK.res capture.json')
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'tools/LegacyRussianGlyphSymbol.Probe'))
from audit_output import read_alphabet
raw=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
labels={text:label for label,text in read_alphabet(raw).items()}
p=188222;assert struct.unpack_from('<HH',raw,p)==(0x201,192);p+=4
n=raw[p];p+=1;p=(p+3)&~3;ids=struct.unpack_from('<'+'i'*n,raw,p);p+=n*4
first=struct.unpack_from('<195b',raw,p)
table=[0,1,4,2,4,1,0,2,2,2,2,2,0,0,1,1,2,1,2,0,0,3,1,2,2,2,0,0,1,1,2,1,2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0]
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def fb(v):return struct.unpack('<f',struct.pack('<I',v&0xffffffff))[0]
def policy(t,h,g,d,b,a,p,c):
    wide=f(h*f(2.95)) if g<=0 else max(f(h*f(2.35)),f(g*f(1.87)))
    narrow=f(h*f(.29)) if g<=0 else min(f(g*f(.55)),f(g*f(.12)))
    if t==2:return wide<d
    if t==1:return narrow<=d
    if t!=5:return True
    threshold=f(f(narrow+wide)*f(.5));accepted=threshold<d
    if accepted and p:return threshold<b or f(threshold*f(1.2))<d
    if accepted and c:return threshold<a or f(threshold*f(1.2))<d
    return accepted
capture=json.loads(Path(sys.argv[2]).read_bytes())
for row in capture['preparedNumericalControls']:
    t,*tail=row;h,g,d,b,a=map(fb,tail[:5]);assert policy(t,h,g,d,b,a,*tail[5:7])==tail[7]
calls=0;emissions=0
for source in capture['sources']:
    height=fb(source['heightBits'])
    for case in source['requests']:
        r=bytes.fromhex(case['request']);I=lambda p:struct.unpack_from('<i',r,p)[0];U=lambda p:struct.unpack_from('<H',r,p)[0]
        current=U(30);previous=U(22);s=case['priorStart'];emit=False
        if case['cursor']:
            if I(44)==2:emit=True
            else:
                assert first[current]>=0 and (previous==65535 or first[previous]>=0)
                t=table[ids[first[current]]+7*(0 if previous==65535 else ids[first[previous]])]
                priorDash=previous in (labels['-'],labels['/']);currentDash=current in (labels['-'],labels['/'])
                if current in (labels['#'],labels['@']) or priorDash or currentDash:t=5
                elif t==4:t=1 if I(44)==1 and current in [labels[x] for x in ':;!?'] else 2
                special=I(52)==10 or previous==labels['-'] and s==0
                if special and (I(56)==10 or not(previous==labels['-'] and s!=0)):emit=True
                else:
                    if not special and t==3:t=2 if s else 1
                    boxes=[[fb(x) for x in b] for b in case['boxes']]
                    def gap(a,b):return f(b[0]-a[1]) if a[0]<=b[0] else f(a[0]-b[1])
                    a,b,before,after=boxes
                    emit=policy(t,height,fb(I(48)),gap(a,b),gap(b,before),gap(a,after),priorDash,currentDash)
            emit=emit or bool(case['flags']&8)
        assert case['count']==int(emit) and case['remaining']==case['cursor']-int(emit)
        assert case['symbol']==(65531 if emit else 0)
        calls+=1;emissions+=emit
assert calls==capture['calls']==2560 and capture['guards']==20
assert capture['unusedNeighborChecks']==4
print(f'PASS independent1024 numerical controls, raw-category/label oracle {calls} preparedrequests overactualsourcegeometry/{emissions} FFFB outputs; no actualwinner claim')
