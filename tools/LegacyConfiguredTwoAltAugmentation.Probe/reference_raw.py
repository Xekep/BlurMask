"""Independent DATA replay; never loads models or executes C#/vendor code.
Ordered glyph reads are replayed literally, not replaced by a reference dictionary.
This checks observed equality/rank/normalization only; walker/cost authority OPEN.
"""
import base64, json, math, struct
U16=lambda b,o:struct.unpack_from('<H',b,o)[0]
U64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
I32=lambda b,o:struct.unpack_from('<i',b,o)[0]
F32=lambda b,o:struct.unpack_from('<f',b,o)[0]
MAX=struct.unpack('<f',bytes.fromhex('ffff7f7f'))[0]
NIL=(1<<64)-1

def f(x):
    try:return struct.unpack('<f',struct.pack('<f',x))[0]
    except OverflowError:return math.copysign(math.inf,x)
def put(b,o,x):struct.pack_into('<f',b,o,f(x))
def ints(b):
    assert len(b)%4==0
    return list(struct.unpack('<'+'i'*(len(b)//4),b))
def strict_json(path):
    def pairs(items):
        d={}
        for k,v in items:
            assert k not in d,('duplicate',k)
            d[k]=v
        return d
    return json.loads(path.read_text(encoding='utf-8-sig'),object_pairs_hook=pairs,
                      parse_constant=lambda x:(_ for _ in ()).throw(AssertionError(x)))
def integer(x,lo,hi):assert type(x) is int and lo<=x<=hi

def scalar(row,scratch,controls):
    kind=U16(row,4)&7;label=U16(row,8)
    if kind==3:
        if label&0x7ffc==0:return
        text={0:'\n',2:' ',3:'\u00a0'}.get(label&3)
        if text is not None:
            raw=controls();assert raw[2:].decode('utf-16-le')==text
            struct.pack_into('<H',scratch,0,U16(raw,0))
        scratch[2]=0;return
    if kind==1:label=((label&3)-7)&65535
    elif kind not in (0,2,4):label=65535
    struct.pack_into('<H',scratch,0,label);scratch[2]=int(kind==4)

def equality(before,events):
    assert len(before)==250
    left,right=before[:120],before[120:240]
    a,b=bytearray(before[240:244]),bytearray(before[244:248]);ignored=U16(before,248)
    index=0;read_count=0
    def read(ref):
        nonlocal index,read_count
        read_count+=1;assert read_count<=131072
        assert index<len(events) and events[index][0]==6,'missing ordered read'
        row=events[index][1];index+=1
        assert len(row)==48 and U64(row,0)==ref,'ordered read reference'
        return row[8:]
    def controls():
        nonlocal index
        assert index<len(events) and events[index][0]==7,'lookup chronology'
        raw=events[index][1];index+=1;return raw
    if (U16(left,50)^U16(right,50))&0x300:result=False
    else:
        l,r=U64(left,8),U64(right,8)
        while True:
            assert index<=262144
            if l==NIL and r==NIL:result=True;break
            ar=br=None;skipped=False
            while True:
                if l!=NIL:
                    ar=read(l)
                    if ar[6]&2:l=U64(ar,32);skipped=True
                if r!=NIL:
                    br=read(r)
                    if br[6]&2:r=U64(br,32);continue
                break
            if skipped:continue
            if l==NIL:result=r==NIL;break
            if r==NIL:result=False;break
            scalar(ar,a,controls);scalar(br,b,controls)
            al,bl=U16(a,0),U16(b,0)
            if al==ignored:l=U64(ar,32)
            elif bl==ignored:r=U64(br,32)
            else:
                if al!=bl or a[2]!=b[2]:result=False;break
                l,r=U64(ar,32),U64(br,32)
    assert index==len(events),'extra read/lookup'
    return bytes([int(result)])+a+b

def sort_rows(rows,low,high,key,descending):
    while low<high:
        i,j=low,high;pivot=rows[low+(high-low)//2][key]
        while True:
            if descending:
                while pivot<rows[i][key]:i+=1
                while rows[j][key]<pivot:j-=1
            else:
                while rows[i][key]<pivot:i+=1
                while pivot<rows[j][key]:j-=1
            if j<i:break
            rows[i],rows[j]=rows[j],rows[i];i+=1;j-=1
            if i>j:break
        sort_rows(rows,low,j,key,descending);low=i

def ranking(words,selected,compare):
    words=list(words);n=len(words);assert 1<=n<=4095 and len(selected)==2
    rows=[];payload=list(range(n))
    for i,w in enumerate(words):
        assert len(w)==120 and math.isfinite(F32(w,16)) and F32(w,16)!=-MAX
        rows.append(dict(original=i,group=-2147483647,cost=F32(w,16),first=False,selected=False,
                         special=False,flags=False,keep=False,removed=False))
    for j,s in enumerate(selected):
        assert 0<=s<n;rows[s]['selected']=True
        if j==0:rows[s]['first']=True
    group=999
    for i,row in enumerate(rows):
        if row['removed']:continue
        group+=1;row['group']=group;row['flags']=bool(U16(words[i],50)&0x300);duplicates=[]
        for j in range(i+1,n):
            if not rows[j]['removed'] and I32(words[i],60)==I32(words[j],60) and compare(words[i],words[j]):
                rows[j]['group']=group;duplicates.append(j)
        selected0,first,best,bestcost=row['selected'],row['first'],i,row['cost']
        for j in duplicates:
            selected0|=rows[j]['selected'];first|=rows[j]['first']
            if bestcost<rows[j]['cost']:bestcost=rows[j]['cost'];best=j
        if best!=i:
            words[i],words[best]=words[best],words[i];payload[i],payload[best]=payload[best],payload[i];row['cost']=bestcost
        row['first']=first
        for j in duplicates:rows[j]['removed']=True;rows[j]['cost']=-MAX
        row['keep']=selected0 or row['flags']
    protected=sum(r['keep'] and not r['removed'] for r in rows);alive=n-sum(r['removed'] for r in rows)
    quota=2+sum(r['flags'] for r in rows)
    if quota<alive:
        prefix=0
        for i in range(n):
            if rows[i]['keep']:
                if i!=prefix:rows[i],rows[prefix]=rows[prefix],rows[i]
                prefix+=1
                if prefix>=protected:break
        sort_rows(rows,protected,n-1,'cost',True);alive=protected
        for i in range(protected,n):
            if not rows[i]['removed']:
                if alive>=quota:rows[i]['removed']=True;rows[i]['cost']=-MAX
                else:alive+=1
    first=next((i for i,r in enumerate(rows) if r['first']),-1)
    if first>0:rows[0],rows[first]=rows[first],rows[0]
    group0=rows[0]['group']
    for r in rows:
        if r['group']==group0:r['group']=0
    sort_rows(rows,1,n-1,'cost',True)
    for i in range(1,n):
        if rows[i]['cost']==-MAX:break
        old=rows[i]['group']
        for r in rows:
            if r['group']==old:r['group']=i
    output=[];source=[]
    for r in rows:
        if r['group']==-1 or r['cost']==-MAX:break
        output.append(words[r['original']]);source.append(payload[r['original']])
    assert len(output)==alive
    sort_rows(rows,0,n-1,'original',False)
    raw=[]
    for r in rows:
        b=bytearray(20);struct.pack_into('<iif',b,0,r['original'],r['group'],r['cost'])
        b[12:18]=bytes(int(r[k]) for k in ('first','selected','special','flags','keep','removed'));raw.append(bytes(b))
    return dict(words=output,rows=raw,mapping=[r['group'] for r in rows],released=[r['original'] for r in rows if r['removed']],
                quota=quota,sources=source,releasedsources=[payload[r['original']] for r in rows if r['removed']])

def normalize(words):
    words=[bytearray(w) for w in words];order=[]
    for i,w in enumerate(words):
        assert len(w)==120 and all(math.isfinite(F32(w,o)) for o in (16,28))
        low,high,mid=0,i,i//2
        while low<high:
            old,current=F32(words[order[mid]],16),F32(w,16)
            if current<=old:
                if old<=current:break
                low=mid+1
            else:high=mid
            mid=(high+low)//2
        order.insert(mid,i)
    if not words:return [],order
    for i in reversed(order):
        d=f(F32(words[i],16)-F32(words[order[0]],16));put(words[i],16,0 if d< -75 else 3.73e32 if d>75 else math.exp(d))
    if len(words)==1:put(words[0],16,1);put(words[0],20,1);return list(map(bytes,words)),order
    prior=F32(words[order[0]],16);kept=0
    if prior>0:
        kept=1;put(words[order[0]],16,f(prior/f(prior+F32(words[order[1]],16))))
        for k in range(1,len(order)):
            current=F32(words[order[k]],16)
            if current==0:break
            put(words[order[k]],16,f(f(current*F32(words[order[k-1]],16))/prior));prior=current;kept+=1
    for i in order[kept:]:put(words[i],16,0)
    minimum=MAX
    for i in order:
        if F32(words[i],28)<=minimum:minimum=F32(words[i],28)
    for i in order:put(words[i],20,math.exp(-f(F32(words[i],28)-minimum)))
    return list(map(bytes,words)),order
