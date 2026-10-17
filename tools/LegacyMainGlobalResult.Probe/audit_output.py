"""Independent raw-resource confidence, normalized weights and numeric2257C0.
Post-result query rows are explicit prepared consumer requests, not a full
publication/scheduler fixture and not OCR accuracy evidence.
"""
import hashlib,json,math,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'tools/LegacyRussianGlyphSymbol.Probe'))
from audit_output import read_alphabet
if len(sys.argv)!=4:raise SystemExit('AK LK capture')
ak,lk=map(lambda p:Path(p).read_bytes(),sys.argv[1:3]);captures=json.loads(Path(sys.argv[3]).read_bytes())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
assert len(captures)==8 and struct.unpack_from('<IHH',ak,566408)==(1,192,256)
texts=read_alphabet(ak);labels={t:i for i,t in texts.items()};header=struct.unpack_from('<8H',ak,99)
assert header==(179,140,65535,178,182,180,181,192) and '\U000f0009'not in labels
aliases={}
for text in ['-','\uff0d','_','\uff3f','\u00ad','\u30fc','\u4e00','\U000f0002','\U000f0003']:
    if text in labels and not(text=='\u30fc'and labels[text]<180):aliases.setdefault(labels[text],header[1])
aliases.setdefault(193,labels["'"]);aliases.setdefault(194,labels['"'])
closed=labels['\U000f0001'];assert closed==191 and header[2]==65535
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.unpack('<I',struct.pack('<f',x))[0]
def real(x):return struct.unpack('<f',struct.pack('<I',x&0xffffffff))[0]
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def wrap(x):return (x+2**31)%2**32-2**31
#Read stored codebook/profile ownership rather than copying a captured penalty.
d=lk[2513123:4565174];p=90
def take(fmt):
    global p
    values=struct.unpack_from('<'+fmt,d,p);p+=struct.calcsize('<'+fmt)
    return values[0]if len(values)==1 else values
order,flags,words,classes,uc,bc=take('6I');assert(order,flags,uc)==(2,3,classes)
packed,count=take('2I');width=take('B');assert packed==0x1000000 and count==words
p+=((words*width+31)//32)*4+words
word_levels=take('i'*take('I'));p+=classes+classes*classes
nu,nb=take('2I');uni=take('i'*nu);bi=take('i'*nb);assert take('2I')==(0,0)
slope=struct.unpack_from('<f',d,p+80)[0];offset=struct.unpack_from('<f',d,p+16)[0]
scale=struct.unpack_from('<f',lk,607064)[0]
penalty=int(f(f(f(f(uni[-1])*f(1/8192))*f(scale*slope))-f(scale*offset))*8192)
assert penalty==17153
def calibrated(symbol,family,probability):
    center,slope=struct.unpack_from('<ff',ak,566416+8*(family*192+symbol))if symbol<192 else(0.,.575)
    return f(1/(1+math.exp((probability/32768-center)*-slope)))
rows=groups=queries=maximum=0
for owner,c in enumerate(captures):
    assert c['owner']==owner and c['penalty']==penalty and c['sourceHeaders']==[0,0xc0a]
    assert c['acousticEvaluations']==len(c['groups'])
    for g in c['groups']:
        assert len(g['probabilities'])==4 and all(len(v)==180 and all(0<=p<=32767 for p in v)for v in g['probabilities'])
        assert len(g['rawCosts'])==180 and len(g['finalCosts'])==195
        probabilities=g['probabilities'];source_probability=real(g['sourceProbabilityBits'])
        costs=[r[1]for r in g['candidates']];weights=[];total=0.
        if len(costs)==1:weights=[1.]
        else:
            for cost in costs[:20]:value=f(math.exp(f(-f(cost)/8192)));weights.append(value);total=f(total+value)
            weights=[f(v/total)for v in weights]+[0.]*max(0,len(costs)-20)
        for ordinal,(symbol,cost,confidence_bits,weight_bits)in enumerate(g['candidates']):
            assert cost==g['finalCosts'][symbol]
            if ordinal>=20:expected=0.
            elif symbol==closed:expected=source_probability
            else:
                output=aliases.get(symbol,symbol);assert output<180
                values=[calibrated(symbol,family,probabilities[family][output])for family in range(4)]
                expected=min(1.,f(f(f(f(values[1]+values[0])+values[2])+values[3])/4))
            distance=abs(bits(expected)-(confidence_bits&0xffffffff));assert distance<=2,(owner,g['group'],symbol,distance)
            maximum=max(maximum,distance)
            distance=abs(bits(weights[ordinal])-(weight_bits&0xffffffff));assert distance<=2
            maximum=max(maximum,distance);rows+=1
        groups+=1
    #FreshFilter4=false/noF0009/noRequire6/norescue. Primary or bit4 rows
    #are ineligible; strict scan keeps the first original secondary tie.
    raw=bytes.fromhex(c['finalRecords']);assert len(raw)%88==0 and c['sourceEnd']<=len(raw)//88
    best=0x4000000;chosen=-1
    for ordinal in range(c['sourceEnd']):
        row=raw[ordinal*88:(ordinal+1)*88];cost=i32(row,16)
        if cost>=0x4000000:continue
        assert row[4]<len(c['sourceHeaders'])
        if ordinal<c['primaryLimit']or row[5]&4:continue
        if cost<best:best=cost;chosen=ordinal
    assert chosen>=0
    row=raw[chosen*88:(chosen+1)*88]
    expected=[True,chosen,best,wrap(best+c['cumulative']),i32(row,20),u16(row,34),0,0]
    assert c['winner']==expected,(owner,c['winner'],expected)
    query=bytes.fromhex(c['queryRows']);assert len(query)%32==0
    for at in range(0,len(query),32):assert query[at+4]==1
    queries+=len(query)//32
assert(groups,rows,queries)==(631,6251,259)
print(f'PASS independent raw AK/LK {groups} actual groups/{rows} calibrated+normalized scores/maxULP{maximum};8 strict terminal scans/2534penalty;{queries} prepared retained-map query32 rows. No text or winner-quality claim.')
