"""Independent scoped raw-source70/LM40/207B00 DATA oracle. Import is inert.
No production C#, recognizer, model evaluator, PE loader or captured token ingress.
"""
import base64,hashlib,json,struct
from array import array

MODE='global-language-207cb0-v1'
AK_SHA='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
LK_SHA='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
CASE_NUMERIC='5e99b64e4e7c2893a50e60552eb5c6a11894e466e18b8c4945071b5699916c04'
PUBLICATION_NUMERIC='dd431ff067120a92b87cb35cb4d34253516740e4e0e9fedf841f8d258317edcc'
ENGINE='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
TEXT='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
IMP=0x4000000
def sha(b):return hashlib.sha256(b).hexdigest()
def f32(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def u32(b,p):return struct.unpack_from('<I',b,p)[0]
def wrap(x):return (x+2**31)%2**32-2**31
def integer(x,lo,hi):assert type(x) is int and lo<=x<=hi;return x
def decoded(x):
    assert type(x) is str
    out=base64.b64decode(x,validate=True);assert base64.b64encode(out).decode()==x
    return out
def unique_pairs(pairs):
    result={}
    for key,value in pairs:assert key not in result,'duplicate JSON key';result[key]=value
    return result
def raw_json(raw):
    assert len(raw)<=131072
    return json.loads(raw.decode('utf-8-sig'),object_pairs_hook=unique_pairs)

def authenticate_unicode(case_raw,publication_raw):
    """Authenticate complete canonical numeric payloads, never supplied classes.
    Raw file SHA additionally joins the exact producer inputs in admit().
    """
    case=raw_json(case_raw);pub=raw_json(publication_raw)
    assert (case['format'],case['engineSha256'],case['tableSha256'])==('legacy-native-case-data-v1',ENGINE,CASE_NUMERIC)
    h=hashlib.sha256()
    def word(value):h.update(struct.pack('<i',integer(value,-2**31,2**31-1)))
    for name,count,width in [('alternatingRanges',58,2),('lowerRanges',34,3),('lowerSingles',89,2),('upperRanges',35,3),('upperSingles',110,2),('titleSingles',8,2),('specialRows',130,9),('sequenceScalars',348,1)]:
        rows=case[name];assert type(rows) is list and len(rows)==count;word(count)
        for row in rows:
            assert type(row) is list and len(row)==width
            for value in row:word(value)
    assert h.hexdigest()==CASE_NUMERIC,'raw complete case numeric checksum'
    assert (pub['format'],pub['engineSha256'],pub['textSha256'],pub['tableSha256'])==('legacy-native-text-publication-data-v1',ENGINE,TEXT,PUBLICATION_NUMERIC)
    h=hashlib.sha256()
    for name,count,width in [('scriptRanges',716,3),('controlRanges',19,2),('extendRanges',225,2),('prependRanges',11,2),('joinerRanges',77,2)]:
        rows=pub[name];assert type(rows) is list and len(rows)==count;word(count);last=-1
        for row in rows:
            assert type(row) is list and len(row)==width
            assert integer(row[0],0,0x10ffff)>last and integer(row[1],row[0],0x10ffff)>=row[0]
            if width==3:integer(row[2],1,147)
            for value in row:word(value)
            last=row[1]
    literals=pub['literalRows'];assert type(literals) is list and len(literals)==120;word(120);seen=set()
    for ordinal,row in enumerate(literals):
        assert type(row) is list and len(row)==6 and type(row[0]) is str
        raw=bytes.fromhex(row[0]);assert 1<=len(raw)<=64 and b'\0' not in raw and raw not in seen;seen.add(raw)
        word(len(raw));h.update(raw)
        for field in row[1:]:word(field)
        integer(row[1],0,3)
        for relative in row[2:]:integer(ordinal+relative,0,119)
    assert h.hexdigest()==PUBLICATION_NUMERIC,'raw complete publication numeric checksum'
    return pub['scriptRanges']

def script_id(rows,cp):
    lo,hi=0,len(rows)
    while lo<hi:
        mid=(lo+hi)//2;a,b,script=rows[mid]
        if cp<a:hi=mid
        elif cp>b:lo=mid+1
        else:return script
    return 0

def terminal_category(tag,text):
    #2324E0 ordinary classifications are0..7 before its literal overrides.
    #Only comma/colon/semicolon become8; dot/exclamation/question become9.
    #The no-Arabic-map guard is proved from authenticated script ranges above.
    categories=bytes.fromhex('090b0104040404040505050a010c040606060505050202020303030500000000')
    state=tag>>58&31
    return 7 if tag&(1<<40) and text in (',',':',';') else 8 if text in ('.','!','?') else categories[state]

def bits(raw,width,index):
    at,shift=divmod(index*width,8);return int.from_bytes(raw[at:at+5],'little')>>shift&((1<<width)-1)

def word_index(lk,needed):
    """Raw2489E0 DFS terminal preorder, not numeric node rank or captured wordID."""
    d=memoryview(lk)[609980:2513123];count=u32(d,20);width=d[36];assert count==895571 and 1<=width<=32
    size=((count*width+31)//32)*4;labels=d[37:37+size];assert len(labels)==size;p=38+size
    assert (u32(d,p),u32(d,p+4))==(0x1000000,count)
    cw=d[p+8];assert 1<=cw<=32
    child_bytes=((count*cw+31)//32)*4
    children=d[p+9:p+9+child_bytes];assert len(children)==child_bytes
    starts=array('I',[1])
    for node in range(count):starts.append(starts[-1]+bits(children,cw,node))
    assert starts[-1]==count and all(0<=n<count for n in needed)
    return preorder_terminals(starts,labels,width,needed,439826)

def preorder_terminals(starts,labels,width,needed,expected_words):
    """General pure finite-tree kernel; the actual caller binds raw sizes/hash."""
    count=len(starts)-1;assert count>0 and len(labels)*8>=count*width
    found={};ordinal=0;visits=0;stack=[(starts[0],starts[1])]
    while stack:
        begin,end=stack[-1]
        if begin==end:stack.pop();continue
        assert 0<=begin<end<=count;visits+=1;assert visits<count
        stack[-1]=(begin+1,end)
        if bits(labels,width,begin)&(1<<(width-1)):
            if begin in needed:found[begin]=ordinal
            ordinal+=1
        if starts[begin]!=starts[begin+1]:
            assert begin<starts[begin]<starts[begin+1]<=count
            stack.append((starts[begin],starts[begin+1]))
    assert ordinal==expected_words and visits==count-1
    return found

class RawLanguage:
    def __init__(self,lk):
        assert sha(lk)==LK_SHA and struct.unpack_from('<5I',lk,2513213)==(2,3,439826,1024,1024)
        self.raw=lk;self.classes=memoryview(lk)[2513246:3063030];profile_base=4565174-(2+16+16+0x2a4+0x2254)-128;profile=2
        assert u16(lk,profile_base+128)==0x200,'raw selected TCM200 category table scope'
        p=606964+92;fmt=u32(lk,p);p+=4
        if fmt&0xff000000:p+=8
        assert u32(lk,p)==1
        rf=lambda at:struct.unpack_from('<f',lk,at)[0]
        scale=rf(p+4);assert scale==1.0
        ws,wo,uo,bs,bo,boundary=(rf(profile_base+q) for q in [64+8*profile,32+4*profile,8*profile,68+8*profile,4+8*profile,48+4*profile])
        def levels(at,slope,offset):
            factor=f32(scale*slope);bias=f32(scale*offset);out=[]
            for level in struct.unpack_from('<256i',lk,at):
                x=f32(f32(level)*f32(1/8192));x=f32(x*factor);x=f32(x-bias);x=f32(x*8192)
                assert -2**31<=x<2**31;out.append(int(x))
            return out
        self.words=levels(3502860,ws,wo);self.unigrams=levels(4553492,ws,uo)
        self.boundary=int(f32(f32(f32(scale*boundary)-f32(scale*wo))*-8192.0))
    def evaluate(self,token,residue):
        assert len(token)==16 and len(residue)==40
        source,word,cat=struct.unpack_from('<iii',token)
        assert source==1 and 0<=word<439826 and 0<=cat<=12 and token[12]==0 and token[13] in (0,1)
        cls=bits(self.classes,10,word);assert cls<1024
        wc=self.words[self.raw[3063030+word]];cl=self.unigrams[self.raw[3503884+cls]]
        #Literal empty history and knowledgeF5=false: no pair-codebook lookup.
        cc=0 if token[13] else 1 if cat==3 else 2 if cat in (5,6) else 3 if cat in (10,11,12) else 0
        subtotal=wc+cc+cl;integer(subtotal,-2**31,2**31-1)
        if subtotal>=IMP:return None
        output=bytearray(residue)
        struct.pack_into('<iI',output,0,subtotal,1);output[8]=1
        struct.pack_into('<I',output,12,(cls&0xffffff)|((0 if token[13] else cat)&15)<<24)
        struct.pack_into('<Ii',output,24,0xffffffff,word)
        return bytes(output)

class Heap56:
    """Native207B00 strict-cost max heap with full moved-row residue."""
    def __init__(self,alternatives,residue):
        assert type(alternatives) is int and alternatives==2 and len(residue)==(alternatives*10+1)*56
        self.capacity=alternatives*10;self.rows=[bytearray(residue[i*56:(i+1)*56]) for i in range(self.capacity+1)];self.count=0
        struct.pack_into('<i',self.rows[0],8,IMP)
    def reset(self):self.count=0
    def allocate(self,cost):
        integer(cost,-2**31,2**31-1)
        if cost>=IMP:return 0
        n=self.count
        if n==self.capacity:
            if i32(self.rows[1],8)<=cost:return 0
            last=self.rows[n][:];last_cost=i32(last,8);n-=1;hole=1
            while 2*hole<=n:
                child=hole*2
                if child!=n and i32(self.rows[child],8)<i32(self.rows[child+1],8):child+=1
                if i32(self.rows[child],8)<=last_cost:break
                self.rows[hole][:]=self.rows[child];hole=child
            self.rows[hole][:]=last
        n+=1;self.count=n;slot=n
        while i32(self.rows[slot//2],8)<cost:self.rows[slot][:]=self.rows[slot//2];slot//=2
        struct.pack_into('<i',self.rows[slot],8,cost);return slot
    def fill(self,slot,ordinal,alternative,cost,increment,tag,context,key,end,auxiliary):
        r=self.rows[slot];struct.pack_into('<HBH',r,0,ordinal,alternative,1);r[5]=0
        struct.pack_into('<ii',r,8,cost,increment);struct.pack_into('<QQiIi',r,24,tag,context,key,end,auxiliary)
    def candidates(self):
        out=[]
        for r in self.rows[1:self.count+1]:
            out.append(dict(o=u16(r,0),k=r[2],cost=i32(r,8),inc=i32(r,12),tag=int.from_bytes(r[24:32],'little'),ctx=int.from_bytes(r[32:40],'little'),key=i32(r,40),end=u32(r,44),aux=i32(r,48),fallback=bool(r[3]),forced=False,bit3=False))
        return out

def admit_capture(c,ak,lk,case_raw,publication_raw):
    assert c.get('publicationOracleMode')==MODE and type(c.get('publicationOracleInputVersion')) is int and c['publicationOracleInputVersion']==1
    assert c['factory']==0x292 and c['globalPublicationAlternatives']==2
    assert c['publicationResourceSHA256']==[sha(ak),sha(lk),sha(case_raw),sha(publication_raw)]
    assert sha(ak)==AK_SHA and sha(lk)==LK_SHA
    assert type(c['graph']) is list and 1<=len(c['graph'])<=240
    assert type(c['inputNodes']) is list and type(c['nodes']) is list and len(c['inputNodes'])==len(c['nodes'])==len(c['graph'])
    entries=c['publicationAllocatorInputs'];assert type(entries) is list and len(entries)==2*len(c['graph'])<=480
    pools={};identities=set()
    for index,row in enumerate(entries):
        assert set(row)=={'Kind','Node','Identity','Bytes'} and row['Kind']==('PublicationLanguageHeap' if index%2==0 else 'PublicationCandidateHeap')
        node=integer(row['Node'],0,len(c['graph'])-1);assert node==index//2
        identity=integer(row['Identity'],1,2**64-1);assert identity not in identities;identities.add(identity)
        expected=440 if index%2==0 else 1176
        assert type(row['Bytes']) is str and len(row['Bytes'])==4*((expected+2)//3)
        value=decoded(row['Bytes']);assert len(value)==expected
        pools[node,row['Kind']]=value
    return pools

class GlobalReference:
    def __init__(self,c,ak,lk,case_raw,publication_raw,alphabet):
        self.inputs=admit_capture(c,ak,lk,case_raw,publication_raw)
        tail=189734+8+i32(ak,189738)
        assert 8+i32(ak,4)-tail==190 and i32(ak,tail)==0
        #Actual mode13 frontendC8 becomes2144F0's primary230A; fresh
        #knowledge14/16=-1 supplies header/quota2 independently of this field.
        primary=u16(ak,tail+4+64+13*2)
        assert type(c['primary']) is int and c['primary']==primary and primary>0
        scripts=authenticate_unicode(case_raw,publication_raw)
        self.text=alphabet['mapping'];self.count=alphabet['label_count'];assert self.count==192
        self.special_marker=next((k for k,v in self.text.items() if v=='\u0003'),65535)
        for text in self.text.values():
            for char in text:assert script_id(scripts,ord(char))!=5,'unsupported Arabic map'
        needed=set()
        for item in c['inputNodes']:
            raw=decoded(item['rows']);assert len(raw)>=primary*88
            for ordinal in range(primary):
                tag=int.from_bytes(raw[ordinal*88:ordinal*88+8],'little')
                if tag&(1<<41) and (tag>>32)&255==1 and (tag&0xffffffff)<895571:needed.add(tag&0xffffffff)
        self.words=word_index(lk,needed);self.language=RawLanguage(lk);self.node=-1;self.heap=None;self.residue=None
        self.language_calls=self.language_accepted=self.heap_rejected=0
    def begin_node(self,node):
        assert node==self.node+1;self.node=node
        self.residue=self.inputs[node,'PublicationLanguageHeap'][40:80]
        self.heap=Heap56(2,self.inputs[node,'PublicationCandidateHeap'])
    def reset_ordinal(self):self.heap.reset()
    def terminal(self,tag,symbol):
        assert type(tag) is int and 0<=tag<2**64 and (tag>>32)&255==1 and tag&(1<<41) and not tag&(1<<56)
        assert type(symbol) is int and (symbol in self.text or 65507<=symbol<=65535)
        category=terminal_category(tag,self.text.get(symbol,''))
        node=tag&0xffffffff;assert node in self.words,'source70 requires actual raw-LK terminal'
        token=bytearray(16);struct.pack_into('<III',token,0,1,self.words[node],category);token[13]=tag>>42&1
        return bytes(token)
    def offer(self,item,symbol):
        self.language_calls+=1;token=self.terminal(item['tag'],symbol);record=self.language.evaluate(token,self.residue)
        if record is None:return False
        self.language_accepted+=1;self.residue=record
        cost=wrap(item['cost']+i32(record,0));slot=self.heap.allocate(cost)
        if slot:self.heap.fill(slot,item['o'],item['k'],cost,item['inc'],item['tag'],item['ctx'],item['key'],item['end'],i32(record,24))
        else:self.heap_rejected+=1
        return True #Accepted page suppresses fallback even if heap rejected.
