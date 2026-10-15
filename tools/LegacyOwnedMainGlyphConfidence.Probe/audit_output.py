"""Independent raw192x4 calibration,3A0 and final-rank confidence oracle.
Actual NN/cache rows and explicitly prepared tuples are reported separately.
"""
import hashlib,json,math,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'tools/LegacyRussianGlyphSymbol.Probe'))
from audit_output import read_alphabet
if len(sys.argv)!=3:raise SystemExit('AK.res capture.json')
data=Path(sys.argv[1]).read_bytes();p=json.loads(Path(sys.argv[2]).read_bytes())
assert hashlib.sha256(data).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IHH',data,566408)==(1,192,256)
assert struct.unpack_from('<h',data,566380)==(0,) # actual ordered knowledge group count
texts=read_alphabet(data);labels={text:label for label,text in texts.items()}
header=struct.unpack_from('<8H',data,99);assert header==(179,140,65535,178,182,180,181,192)
aliases={}
for text in ['-','\uff0d','_','\uff3f','\u00ad','\u30fc','\u4e00','\U000f0002','\U000f0003']:
    if text in labels and not(text=='\u30fc' and labels[text]<180):aliases.setdefault(labels[text],header[1])
aliases.setdefault(193,labels["'"]);aliases.setdefault(194,labels['"'])
assert aliases=={185:140,186:140,183:140,187:140,188:140,193:134,194:129}
closed=labels['\U000f0001'];assert closed==191 and header[2]==65535
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.unpack('<I',struct.pack('<f',x))[0]
def value(bitsValue):return struct.unpack('<f',struct.pack('<I',bitsValue&0xffffffff))[0]
def calibrated(symbol,family,probability):
    assert 0<=probability<=32767
    center,slope=struct.unpack_from('<ff',data,566416+8*(family*192+symbol)) if symbol<192 else (0.,.575)
    return f(1/(1+math.exp((probability/32768-center)*-slope)))
def expected(symbol,ordinal,probabilities,sourceProbability):
    if ordinal>=20:return 0.
    if symbol==closed:return sourceProbability
    output=aliases.get(symbol,symbol);assert output<180
    values=[calibrated(symbol,family,probabilities[family][output]) for family in range(4)]
    result=f(f(f(values[1]+values[0])+values[2])+values[3])
    return min(1.,f(result/4))
maximumUlp=0;actualCount=0;extended=0;zeros=0;preparedCount=0
for source in p['sources']:
    assert source['name'] in ('onePhysical','twoPhysicalOneLine','directOnePhysical','directTwoPhysicalOneLine')
    for group in source['groups']:
        assert len(group['probabilities'])==4 and all(len(row)==180 for row in group['probabilities'])
        sourceProbability=value(group['sourceProbabilityBits'])
        for ordinal,(symbol,cost,resultBits) in enumerate(group['candidates']):
            wanted=bits(expected(symbol,ordinal,group['probabilities'],sourceProbability))
            ulp=abs((resultBits&0xffffffff)-wanted);assert ulp<=2,(source['name'],group['group'],symbol,ordinal,ulp)
            maximumUlp=max(maximumUlp,ulp);actualCount+=1;extended+=symbol>=180;zeros+=ordinal>=20
assert len(p['sources'])==4 and sum(len(s['groups']) for s in p['sources'])==p['groups']==92
assert (p['candidates'],p['extendedLabels'],p['zeroedAfter20'],p['guards'])==(actualCount,extended,zeros,36)
#These tuples are explicitly prepared kernel controls. They are not real NN
#observations or evidence that a native graph selects a primitive/extended label.
assert len(p['preparedNumericalControls'])==128
for row in p['preparedNumericalControls']:
    n=row['seed'];probabilities=[[(i*197+family*7919+n*173)%32768 for i in range(180)] for family in range(4)]
    symbols=list(range(25))
    for offset,symbol in enumerate([185,186,191,193,194]):symbols[(n+offset)%25]=symbol
    sourceProbability={5:-.125,6:1.125}.get(n%7,(n%7)*.25)
    assert row['sourceProbabilityBits']&0xffffffff==bits(sourceProbability)
    assert len(row['rows'])==25
    for ordinal,(symbol,cost,resultBits) in enumerate(row['rows']):
        assert symbol==symbols[ordinal] and cost==-ordinal*17
        wanted=bits(expected(symbol,ordinal,probabilities,sourceProbability))
        ulp=abs((resultBits&0xffffffff)-wanted);assert ulp<=2,('prepared',n,ordinal,ulp)
        maximumUlp=max(maximumUlp,ulp);preparedCount+=1
assert preparedCount==3200
print(f'PASS independent actual{actualCount} rows/{p["groups"]} owned NN groups (extended{extended},post20zero{zeros}); separate{preparedCount} prepared tuples incl primitive/aliases/quotes/top20; maxULP{maximumUlp}.')
