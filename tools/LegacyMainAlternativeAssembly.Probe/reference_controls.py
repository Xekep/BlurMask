"""Original stream machine; no vendor code execution or expected OCR labels."""
import json, random, sys
from pathlib import Path
r = random.Random(1421)

def scalar(g, s):
    kind, label, flag = g
    if kind == 3:
        if label & 0x7ffc:
            if label & 3 != 1:
                mapped = 10 if label & 3 == 0 else 32 if label & 3 == 2 else 160
                s[:2] = [mapped, 0]
            s[2] = 0
        return
    label = ((label & 3) - 7) & 65535 if kind == 1 else label if kind in (0,2,4) else 65535
    s[:3] = [label & 255, label >> 8, int(kind == 4)]

def compare(a,b,sa,sb):
    ai=bi=aj=bj=0
    while True:
        while ai<len(a) and aj>=len(a[ai]['g']): ai+=1;aj=0
        while bi<len(b) and bj>=len(b[bi]['g']): bi+=1;bj=0
        if ai==len(a) or bi==len(b):
            if ai!=len(a) or bi!=len(b): return False
            break
        x,y=a[ai]['g'][aj],b[bi]['g'][bj]
        scalar(x,sa);scalar(y,sb)
        al=sa[0]+256*sa[1];bl=sb[0]+256*sb[1]
        if al==65532 and a[ai]['f']!=-1:aj+=1
        elif bl==65532 and b[bi]['f']!=-1:bj+=1
        elif al==65530:aj+=1
        elif bl==65530:bj+=1
        elif sa[:3]!=sb[:3] or (x[2]^y[2])&64:return False
        else:aj+=1;bj+=1
    allw=a+b
    if not any(w['aux'] for w in allw):return True
    if not all(w['aux'] for w in allw):return False
    return 'unsupported'

def word(gs,f=-1,aux=False):return dict(g=gs,f=f,aux=aux)
ordinary=lambda n:[0,n,0]
manual=[([word([ordinary(1)])],[word([ordinary(1)])]),
 ([word([ordinary(1),ordinary(65530)])],[word([ordinary(1)])]),
 ([word([ordinary(65530),ordinary(1)])],[word([ordinary(1)])]),
 ([word([ordinary(65532),ordinary(1)],0)],[word([ordinary(1)])]),
 ([word([ordinary(65532),ordinary(1)],-1)],[word([ordinary(1)])]),
 ([word([]),word([ordinary(1)])],[word([ordinary(1)]),word([])]),
 ([word([ordinary(1)],aux=True)],[word([ordinary(1)],aux=True)]),
 ([word([ordinary(1)],aux=True)],[word([ordinary(1)])]),
 ([word([ordinary(1)],aux=True)],[word([ordinary(2)],aux=True)])]
controls=[]
for i in range(1024):
    if i<len(manual):a,b=manual[i]
    else:
        a=[word([[r.randrange(8),r.choice([1,2,4,5,7,65530,65532]),r.randrange(256)] for _ in range(r.randrange(5))],r.choice([-1,0,4]),r.randrange(9)==0) for _ in range(r.randrange(1,4))]
        b=json.loads(json.dumps(a)) if i%3 else [word([[r.randrange(8),r.choice([1,2,4,5,7,65530,65532]),r.randrange(256)] for _ in range(r.randrange(5))])]
        if i%5==0 and b[0]['g']:b[0]['g'][0][2]^=64
    sa=[r.randrange(256) for _ in range(4)];sb=[r.randrange(256) for _ in range(4)]
    ea=sa.copy();eb=sb.copy();result=compare(a,b,ea,eb)
    controls.append(dict(a=a,b=b,sa=sa,sb=sb,expected=result,ea=ea,eb=eb))
Path(sys.argv[1]).write_text(json.dumps(controls)+'\n',encoding='utf-8')
print('Generated1024 independent native stream/skip/aux/scratch controls.')
