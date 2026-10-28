"""Independent raw-resource confidence, normalized weights and numeric2257C0.
Post-result query rows are explicit prepared consumer requests, not a full
publication/scheduler fixture and not OCR accuracy evidence.
"""
import ast,hashlib,json,math,struct,sys
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

def definitions(path,expose_heap=False):
    tree=ast.parse(path.read_text(encoding='utf-8-sig'))
    tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
    if expose_heap:
        #Same independent final-cache reference used by ingress/global acoustic
        #oracles. Retain its independently built heap order before native sort;
        #the historical public reference returns symbol-sorted membership only.
        fn=next(n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='cache')
        ret=next(n for n in fn.body if isinstance(n,ast.Return))
        ret.value.elts.append(ast.Name(id='heap',ctx=ast.Load()))
        ast.fix_missing_locations(tree)
    ns={};exec(compile(tree,str(path),'exec'),ns);return ns

read=definitions(ROOT/'tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py')['read']
cache_reference=definitions(ROOT/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',True)['cache']
a=read(ak[99:]);lex=read(lk[607074:]);bytext={v:k for k,v in a['text'].items()}
raw_al=ak[99:];at=a['positions'][0]-4;form_flags=struct.unpack_from('<I',raw_al,at)[0]
triplets=[list(struct.unpack_from('<3H',raw_al,at-a['forms']*6+i*6))for i in range(a['forms'])];maps={};it=iter(a['maps'])
for mode in range(3):
    if form_flags&(1<<mode):maps[mode]=next(it)
def related(label,mode):
    m=maps.get(mode,maps.get(0))
    if m is None or m[label]<0:return None
    out=triplets[m[label]].copy()
    if label not in out:
        for i in(0,1):
            if out[i]==65535:out[i]=label;break
    return out
boundary=max(i for i in range(192)if related(i,0)is not None)
assert boundary==127
def category(label,v):
    lo,up,title=v
    return 0 if lo==65535 else(0 if up==65535 or title==65535 else 1+int(up!=title))if label==lo else 3+int(title!=65535 and label!=title)
case_rows={}
for label in list(range(192))+list(range(65507,65536)):
    signed=label if label<192 else label-65536;p0=p1=0;f0=f1=None;zero=related(label,0)if label<=boundary else None
    if zero is not None:
        f0=zero;p0=category(label,zero);one=related(label,1);two=related(label,2);assert one is not None and two is not None;f1=one.copy()
        if label==one[0]and one[1]==65535 and one[2]!=65535:f1=[one[0],one[2],65535];p1=1
        elif one[0]==65535:p1=0
        elif label==one[0]:p1=category(label,one)
        else:p1=3+int(label!=one[2]and zero[2]!=65535)
        if p0==0:
            if two[0]==65535:p1=0
            else:
                p0=category(label,two)
                if p0!=0:f0=two
    elif label==65532 or label==labels.get('-'):p0=p1=6
    elif -9<=signed<=-5:p0=p1=5
    elif -29<=signed<=-22:p0=p1=7
    if label in[labels.get(t)for t in',:;']:p0=8
    if label in[labels.get(t)for t in'.!?']:p0=9
    case_rows[label]=[label,p0,p1,f0,f1]
control={'\0':65533,' ':65531,'\n':65529,'\u00a0':65532,'\ufffd':65535,'\U000f000c':65530}
preserved=[65535 if lex['reverse'][i]==65535 else control.get(lex['text'][i],bytext.get(lex['text'][i],65535))for i in range(lex['count'])]
callbacks=[]
for label in preserved:
    first=related(label,0)if label<192 else None
    if first is None:callbacks.append(label);continue
    for mode in range(3):
        forms=related(label,mode)
        if forms is not None:callbacks.extend(v for v in forms if v!=65535)
mask=[0]*192
for value in callbacks:
    if 65507<=value<=65514:mask=[1]*192
    elif value<192:mask[value]=1
#Exact source v110 tail after pinned four-family prefix. This independently
#reads both profile and disabled knowledge list, never a captured numeric gate.
at=566330
def profile_take(fmt):
    global at
    value=struct.unpack_from('<'+fmt,ak,at)[0];at+=struct.calcsize('<'+fmt);return value
at+=1;fallback=profile_take('f');at+=9
for _ in range(profile_take('H')):at+=profile_take('H')*2
at+=4;coefficients=[profile_take('f')for _ in range(5)];assert profile_take('B')==0;at+=8;assert profile_take('B')==0
knowledge=profile_take('h');assert knowledge==0
threshold=profile_take('i');minimum=profile_take('H');capacity=profile_take('H')
profile=[fallback,*coefficients,threshold,minimum,capacity]
for text in['-','\u00ad']:
    if text in bytext:mask[bytext[text]]=1
for i in range(192):
    if a['reverse'][i]==65535:mask[i]=0
mask[header[1]]=int(any(mask[bytext[t]]for t in['-','_','\U000f0002','\U000f0003','\u30fc','\u4e00']if t in bytext))
for slot in[3,0,4,5,6]:
    if header[slot]!=65535:mask[header[slot]]=1
assert sum(mask)==145
def native_sort(items):
    items=[list(v)for v in items]
    def partition(first,last):
        left,right=first,last;pivot=items[(first+last)//2][1]
        while left<=right:
            while items[left][1]<pivot:left+=1
            while items[right][1]>pivot:right-=1
            if left>right:break
            items[left],items[right]=items[right],items[left];left+=1;right-=1
        if first<right:partition(first,right)
        if left<last:partition(left,last)
    if len(items)>1:partition(0,len(items)-1)
    return items
#Packed2B4 root edges derive directly from the raw LK packed labels/counts.
packed=lk[609980:2513123];node_count=struct.unpack_from('<I',packed,20)[0];assert struct.unpack_from('<II',packed,28)==(0x1000000,node_count)
label_width=packed[36];label_start=37;at_counts=label_start+((node_count*label_width+31)//32)*4+1
assert struct.unpack_from('<II',packed,at_counts)==(0x1000000,node_count);count_width=packed[at_counts+8]
def packed_value(raw,at,width,index):
    bit=index*width;offset=at+bit//8;return(int.from_bytes(raw[offset:offset+(bit%8+width+7)//8],'little')>>(bit%8))&((1<<width)-1)
root_count=packed_value(packed,at_counts+9,count_width,0)
root_edges=[(node,packed_value(packed,label_start,label_width,node))for node in range(1,1+root_count)]
label_mask=(1<<(label_width-1))-1;terminal_bit=1<<(label_width-1)
assert root_edges[0]==(1,0)and lex['text'][0]=='\u200d'and preserved[0]==bytext['\u200d']==184
policy_tree=ast.parse((ROOT/'tools/LegacyCaseQuery.Probe/reference_controls.py').read_text(encoding='utf-8'))
ns={};exec(compile(ast.Module(body=[n for n in policy_tree.body if isinstance(n,ast.Assign)and any(isinstance(t,ast.Name)and t.id=='POLICIES'for t in n.targets)],type_ignores=[]),'native-case-policies','exec'),ns)
assert u16(d,p+128)==512
case_policy=ns['POLICIES'][512]['rows'][0];assert ns['POLICIES'][512]['alternate'][0]
def query_expected(c,honor_native_root_prefix=True):
    working=c['retainedWorkingMap'];assert c['retainedPreservedMap']==preserved and len(working)==len(preserved)
    assert all(current in(value,65535)for current,value in zip(working,preserved))
    contexts={n[0]:(n[1],n[2])for n in c['contextNodes']};bykey={value:key for key,value in contexts.items()}
    initial=c['initialContexts'];assert len(initial)==len(preserved)
    for lexical,identity in enumerate(initial):
        if preserved[lexical]in(65535,65533):assert identity==0
        else:assert contexts[identity]==(0,preserved[lexical])
    wanted=[];reserved_prefix=0
    for node,value in root_edges:
        lexical=value&label_mask;symbol=working[lexical]
        #Account for EVERY packed root child first. Native source query093B50
        #calls091CE0 at093BF3, then cmp query.Node,0 at093BF8 /jne093BFB /
        #inc slot.start at093BFD..093C03. This happens BEFORE map filtering.
        #See docs/agents/research/legacy-prepared-dictionary-query.md. It is structural prefix
        #suppression, not a guessed NUL or missing/denied-alphabet condition:
        #actual child1 is lexical0 ZWJ, mapped184 and notFFFF in every owner.
        if node==1 and honor_native_root_prefix:
            assert lexical==0 and symbol==184 and initial[lexical]!=0
            reserved_prefix+=1;continue
        if symbol==65535:continue
        flags=1|int(bool(value&terminal_bit))*2;meta=case_rows[symbol];kind,forms=meta[2],meta[4];plan=[]
        def emit(column,form=-1,delta=0,force=False):
            if force or case_policy[column]!=255:plan.append((case_policy[column]&31,form,delta))
        if kind==1:
            emit(3);emit(6,1,1)
            if case_policy[6]!=255 and flags==3:emit(11,1,1,True)
        elif kind==2:
            emit(3);emit(7,1,1)
            if case_policy[7]!=255 and flags==3:emit(12,1,1,True)
            emit(8,2,1)
            if case_policy[8]!=255 and flags==3:emit(13,2,1,True)
        elif kind==3:emit(4);emit(10,0,8192)
        elif kind==4:emit(5);emit(9,2,1);emit(10,0,8192+int(case_policy[9]!=255))
        else:plan=[((0 if kind==7 else case_policy[{5:0,6:1}.get(kind,2)])&31,-1,0)]
        for state,form,delta in plan:
            out=bytearray(32);struct.pack_into('<I',out,0,node);out[4]=1;out[5]=flags;out[7]=state<<2
            mapped=symbol if form<0 else forms[form];context=initial[lexical]if form<0 else bykey[(0,mapped)]
            struct.pack_into('<QiH',out,8,context,0,0);struct.pack_into('<iH',out,16,delta,mapped);wanted.append(bytes(out))
    assert reserved_prefix==int(honor_native_root_prefix)
    return b''.join(wanted)
rows=groups=queries=maximum=0
for owner,c in enumerate(captures):
    assert c['owner']==owner and c['penalty']==penalty and c['sourceHeaders']==[0,0xc0a]
    assert c['acousticEvaluations']==len(c['groups'])
    ids=[g['group']for g in c['groups']];assert ids==sorted(set(ids))
    assert c['acousticMask']==mask and c['caseVersion']==512 and c['caseBoundary']==boundary
    assert c['caseRows']==list(case_rows.values())
    for g in c['groups']:
        assert len(g['probabilities'])==4 and all(len(v)==180 and all(type(p)is int and 0<=p<=(32768 if i==header[3] else 32767)for i,p in enumerate(v))for v in g['probabilities']),'family-probability-domain'
        assert len(g['rawCosts'])==180 and len(g['finalCosts'])==195
        probabilities=g['probabilities'];source_probability=real(g['sourceProbabilityBits'])
        assert source_probability==0.,'This fixture has no enclosing primitive'
        rebuilt,membership,heap=cache_reference(g['rawCosts'],g['normalizedCacheInput'],mask,header,labels,profile,g['whole'],g['zero'])
        assert rebuilt==g['finalCosts'],('raw-cache',owner,g['group'])
        candidates=native_sort([(symbol,rebuilt[symbol])for symbol,_ in heap if symbol<192])
        assert candidates==[row[:2]for row in g['candidates']],('candidate-membership/order',owner,g['group'])
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
    expected_query=query_expected(c);assert query==expected_query,('prepared-retained-map-query',owner,len(query),len(expected_query))
    no_native_skip=query_expected(c,False)
    assert len(no_native_skip)==len(query)+32 and i32(no_native_skip,0)==1
    assert u16(no_native_skip,20)==184 and no_native_skip!=query,('native-root-prefix-negative-witness',owner)
    for at in range(0,len(query),32):assert query[at+4]==1
    queries+=len(query)//32
assert groups==631
print(f'PASS independent raw AK/LK {groups} actual groups/{rows} calibrated+normalized scores/maxULP{maximum};8 strict terminal scans/2534penalty;{queries} prepared retained-map query32 rows;8 explicit native root-prefix skip/negative witnesses. No text or winner-quality claim.')
