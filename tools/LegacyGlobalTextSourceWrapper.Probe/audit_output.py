"""Independent global024F30 keys, CHAR metadata, UTF8 and source positions.

No predictions or expected words enter recognition or this algorithm oracle.
Recorded coordinates are genuine; absent pen-up metadata is not invented.
"""
import hashlib,importlib.util,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=3:raise SystemExit('MODELS CAPTURE')
models=Path(sys.argv[1]);d=json.loads(Path(sys.argv[2]).read_bytes())
ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('raw_alphabet',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
al=importlib.util.module_from_spec(spec);spec.loader.exec_module(al);alphabet=al.read_alphabet(ak)
data=json.loads((models/'text-unicode/legacy-text-publication-data.json').read_bytes())
assert hashlib.sha256((models/'text-unicode/legacy-text-publication-data.json').read_bytes()).hexdigest()=='6daddbc9ce3d1d3d2e9e179ce008db60887f2c6a13c52ab8bb47dcb215890673'
literal={bytes.fromhex(r[0]):r[1]for r in data['literalRows']}
script=lambda cp:next((v for a,b,v in data['scriptRanges']if a<=cp<=b),0)
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
single=lambda b,o:struct.unpack_from('<f',b,o)[0]
def position(source,x,mode,count):
    integer=int(x);remainder=f(x-f(integer))
    if remainder==0 and integer==count-1:fraction=255 if mode in(3,5)else 0
    else:
        fraction=int(f(f(remainder*200)+f(.5)))
        if fraction==200:integer+=1;fraction=255 if mode in(3,5)and integer==count-1 else 0
    return(source<<32)|(integer<<16)|(fraction<<8)|mode
def same(a,b):return a>>16==b>>16 and(a>>8&255==b>>8&255 or a>>8&255==255 or b>>8&255==255)

spec=importlib.util.spec_from_file_location('raw_ranges',root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py')
ops=importlib.util.module_from_spec(spec);spec.loader.exec_module(ops)

def projection(source,residual,counts):
    result=[]
    for i in range(len(source)//24):
        group,first,point,end_group,last,end_point=struct.unpack_from('<6i',source,i*24)
        assert group==end_group==0 and 0<=first<=last<len(counts)
        for index in range(first,last+1):
            count=counts[index]
            a=f(f(point)-f(f(residual[i*2])/f(200)))if index==first else f(0)
            b=f(f(end_point)-f(f(residual[i*2+1])/f(200)))if index==last else f(count-1)
            assert 0<=a<=b<=count-1
            result.append([position(index,a,3,count),position(index,b,5,count)])
    original_end=position(len(counts)-1,counts[-1]-1,5,counts[-1])
    return [[a,(b&~255)|(5 if same(b,original_end)or b>>8&255==255 or b>>16&65535==0 and b>>8&255==0 or same(a,b)else 1)]for a,b in result]

def classify(raw,flags):
    cp=ord(raw.decode());flags&=1023;sc=script(cp)
    if sc in(7,8,9)or sc!=10 and 0x3000<=cp<=0x303f:flags|=0x40000
    elif sc==10:flags|=0x80000
    special={13:0x2000,10:0x2000,0x2028:0x2000,0x2029:0x2000,9:0x1000,11:0x1000,12:0x1000,32:0x1000,160:0x1000,0xad:0x4000,0x200c:0x8000}.get(cp,0)
    if cp in[0x61c,0x200e,0x200f,*range(0x202a,0x202f),*range(0x2066,0x206a)]:special=0x20000
    if special:flags|=special
    elif literal.get(raw,-1)>=0:flags|=0x10000
    if cp in(0xf0000,0xf0001):flags|=0x100000
    return flags

def emit(g,strings):
    owner=bytes.fromhex(g['owner']);selection=bytes.fromhex(g['selection4']);before=bytes.fromhex(g['before'])
    assert owner[:len(before)]==before
    rows=[owner[i:i+40]for i in range(0,len(owner),40)]
    identity=lambda r:(u16(r,4)&7,u16(r,8),u16(r,6)&0xff00)
    winners={}
    for n,row in enumerate(rows):
        k=identity(row)
        if k not in winners or single(row,12)>single(rows[winners[k]],12):winners[k]=n
    expected=sorted(winners.values(),key=lambda n:(-single(rows[n],12),n))
    assert list(struct.unpack('<'+'i'*(len(selection)//4),selection))==expected
    signed=g['reference']>>40&65535;signed=signed-65536 if signed>=32768 else signed
    assert signed>=0 and g['beforeMarker']==expected.index(winners[identity(rows[signed])])
    marker=g['beforeMarker'];out=[]
    for offset in range(0,len(selection),4):
        row=rows[u16(selection,offset)];assert u16(row,4)&7==0
        raw=alphabet[u16(row,8)].encode()
        if not raw:
            if len(out)<marker:marker-=1
            continue
        if raw=='\u2639'.encode():raw='\U0001f641'.encode()
        elif raw=='\u263a'.encode():raw='\U0001f642'.encode()
        identity=strings.setdefault(raw,len(strings)+1)
        out.append((raw,classify(raw,u16(row,6)),identity))
    return out,marker

def avl(keys):
    nodes={};tree={};root=0
    def height(n):return tree[n][2]if n else 0
    def update(n):tree[n][2]=1+max(height(tree[n][0]),height(tree[n][1]))
    def rotate_left(n):
        r=tree[n][1];tree[n][1]=tree[r][0];tree[r][0]=n;update(n);update(r);return r
    def rotate_right(n):
        l=tree[n][0];tree[n][0]=tree[l][1];tree[l][1]=n;update(n);update(l);return l
    def add(n,new):
        if not n:return new
        side=0 if keys[new-1]<keys[n-1]else 1
        tree[n][side]=add(tree[n][side],new);update(n)
        balance=height(tree[n][1])-height(tree[n][0])
        if balance<-1:
            l=tree[n][0]
            if height(tree[l][1])>height(tree[l][0]):tree[n][0]=rotate_left(l)
            return rotate_right(n)
        if balance>1:
            r=tree[n][1]
            if height(tree[r][0])>height(tree[r][1]):tree[n][1]=rotate_right(r)
            return rotate_left(n)
        return n
    for i,key in enumerate(keys,1):tree[i]=[0,0,1];root=add(root,i)
    return [[i,*tree[i][:2],height(tree[i][1])-height(tree[i][0]),i-1,i+1 if i<len(keys)else 0]for i in sorted(tree)]

total=alternatives=reused=overlaps=0
for s in d['samples']:
    assert s['stage']=='complete','Bounded refusal must be reported separately'
    counts=s['rawCounts'];original=[[position(0,0,3,counts[0]),position(len(counts)-1,counts[-1]-1,5,counts[-1])]]
    assert s['original']==original
    word=bytes.fromhex(s['word26']);n=len(word)//26;assert len(word)==n*26
    clip=projection(word[:n*24],word[n*24:],counts);assert s['wordClip']==clip
    glyphs=s['glyphs'];ranges=[];skip=[];positive=[];joiner=[]
    for g in glyphs:
        source=bytes.fromhex(g['source24']);residual=bytes.fromhex(g['residual2'])
        projected=projection(source,residual,counts);assert g['positions']==projected
        ranges.append(ops.intersect(projected,clip))
        row=bytes.fromhex(g['descriptorBeforeRefresh']);skip.append(u16(row,4)&7==1 and u16(row,8)&0x7ffc!=0 and u16(row,8)&3==3)
        positive.append(g['reference']>>24&0x8000==0);joiner.append(u16(row,4)&7==0 and alphabet[u16(row,8)]=='\u200d')
    assert not any(skip)and all(positive),'Actual13 ordinary scope only, prepared branches separately'
    assert s['sourceFlags']==0
    nodes={};strings={};pending=[];keys=[];next_id=s['nextBefore'];ordinary=[];selected_flags=0
    for i,g in enumerate(glyphs):
        assert i==g['index'];key=ranges[i];ordinary_source=True
        if len(key)==1 and same(*key[0])and not any(ops.n(a)<=ops.n(key[0][0])<=ops.n(b)and key[0][1]<=b for a,b in original):
            ordinary_source=False;key=[[(key[0][0]&~255)|4,(key[0][1]&~255)|4]]
        elif joiner[i]:
            end=key[0][1]
            if end>>8&255==255:end&=~0xff00
            end=(end&~255)|4;key=[[end,end]];ordinary_source=False
        else:
            for later in range(i+1,len(glyphs)):
                overlap=ops.intersect(ops.union([],ranges[later]),key)
                if not overlap:continue
                overlaps+=1
                if key!=overlap:key=ops.subtract(key,overlap,counts)
                else:ranges[later]=ops.subtract(ranges[later],overlap,counts)
                break
        ranges[i]=key;assert g['processed']==key and g['ordinary']==ordinary_source
        packed=tuple(tuple(r)for r in key);existing=nodes.get(packed);assert g['reused']==(existing is not None)
        records,marker=emit(g,strings)
        source=bytes.fromhex(g['source24']);visits=0
        for o in range(0,len(source),24):
            group,first,point,last_group,last,end=struct.unpack_from('<6i',source,o)
            assert group==last_group==0 and 0<=first<len(counts)and 0<=last<len(counts)
            visits+=max(0,last-first+1)
        prior=1 if ordinary_source and visits==0 else 0
        if existing is None:
            node=len(nodes)+1;keys.append(packed)
            selection=next_id if ordinary_source else -1
            if ordinary_source:
                pending.append(dict(id=next_id,name='CHAR',command=len(pending)+1,previous=len(pending),operation=4,enabled=1,category=100,ranges=key));next_id+=1
            aggregate=0
            for raw,flags,identity in records:aggregate|=flags
            flags=(((prior<<24)^((0 if ordinary_source else 0x200000)|(aggregate&0xce1fffff)|(0x800000 if ordinary_source and len(key)==1 and same(*key[0])else 0)))&0xfdffffff)
            descriptor=bytes.fromhex(s['sourceDescriptors'][s['groups'][g['group']]])
            line=i32(descriptor,16)
            existing=dict(node=node,records=records,selection=selection,flags=flags,line=line,prior=prior,visits=visits)
            nodes[packed]=existing
        else:
            reused+=1;raise AssertionError('Natural duplicate global coverage requires explicit independent21760 extension')
        assert g['node']==existing['node']and g['complete']and g['written']==127
        assert g['prior']==existing['prior']and g['priorVisits']==existing['visits']and g['priorRangeCount']==0
        assert g['scalars']==[existing['selection'],-1,-1,0,0,0,-1,existing['line'],existing['flags']]
        assert len(g['records'])==len(existing['records'])
        for (raw,flags,identity),actual in zip(existing['records'],g['records']):
            scalar=bytearray(40);struct.pack_into('<I',scalar,8,flags);struct.pack_into('<I',scalar,32,len(raw))
            assert bytes.fromhex(actual['scalar40'])==scalar and actual['intern']==identity and actual['utf8']==raw.hex();alternatives+=1
        assert g['marker']==marker and g['selected']==existing['records'][marker][0].hex()
        selected_flags|=existing['records'][marker][1]
        if ordinary_source:ordinary=ops.union(ordinary,key)
        total+=1
    assert s['nextAfter']==next_id and s['commands']==pending and s['ordinaryRanges']==ordinary and s['selectedFlags']==selected_flags
    assert s['insertion']==[b''.join(struct.pack('<QQ',*r)for r in key).hex()for key in keys]
    assert s['tree']==avl(keys)
    expected=['claim']
    for i in range(len(glyphs)):expected += ['descriptor:'+str(i),'refresh:'+str(i),'clip:'+str(i)]
    expected+=['first-pass-complete']
    for i,g in enumerate(glyphs):expected += [part+':'+str(i)for part in ['select','process','intern-new','fresh','payload','glyph-complete']]
    assert s['trace']==expected+['complete']
assert len(d['samples'])==d['success']==13 and d['refused']==d['actualReuse']==0 and reused==0
for i,p in enumerate(d['preparedCharPrefixes'],1):
    assert p['preparedCharFailure']==i and p['faulted']and p['nextAfter']==p['nextBefore']+1 and p['commands']==[]
    assert len(p['payloads'])==1
    q=p['payloads'][0];assert q['written']==22 and not q['complete']and q['scalars']==[None,-1,-1,0,0,0,-1,-1,None]
    assert q['prior']==q['priorVisits']==q['priorRangeCount']==0
    assert p['trace']==['claim','descriptor:0','refresh:0','clip:0','first-pass-complete','select:0','process:0','intern-new:0','fresh:0','payload:0','fault']
assert d['guards']==115 and d['preparedPriorCases']==3
assert d['preparedSkip']==dict(label='prepared post-inference kind1/run7',sourceFlags=0x200000,
    trace=['claim','descriptor:0','skip:0','first-pass-complete','complete'])
print(f'PASS independent13global sources/{total} processed keys/{alternatives} full40B records/CHAR/scalars/priorvisits/originalLine/AVL; overlaps{overlaps}; preparedCHARprefix4; actual reuse0, not claimed')
