"""Independent byte-addressed13D360 ranking oracle; prepared input only.

WORD equality in these numeric controls is an explicit integer identity at+0.
The actual276.V60 same-pool comparator is a separate integration requirement.
No vendor code is loaded or executed.
"""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
dec=base64.b64decode
geti=lambda b,p:struct.unpack_from('<i',b,p)[0]
geth=lambda b,p:struct.unpack_from('<H',b,p)[0]
getf=lambda b,p:struct.unpack_from('<f',b,p)[0]
sentinel=struct.unpack('<f',bytes.fromhex('ffff7fff'))[0]
def puti(b,p,v):struct.pack_into('<i',b,p,v)
def putf(b,p,v):struct.pack_into('<f',b,p,v)
def sort(rows,lo,hi,offset,descending):
    if hi<=lo:return
    at=getf if descending else geti
    pivot=at(rows[(hi-lo)//2+lo],offset);i,j=lo,hi
    while i<=j:
        while (pivot<at(rows[i],offset)) if descending else (at(rows[i],offset)<pivot):i+=1
        while (at(rows[j],offset)<pivot) if descending else (pivot<at(rows[j],offset)):j-=1
        if j<i:break
        rows[i],rows[j]=rows[j],rows[i];i+=1;j-=1
    sort(rows,lo,j,offset,descending);sort(rows,i,hi,offset,descending)

cases=json.loads(Path(sys.argv[1]).read_text());full_words=full_rows=duplicates=swaps=ties=category_selected_discriminators=0
for case in cases:
    words=list(map(lambda x:bytearray(dec(x)),case['words']));n=len(words);r=[bytearray(20)for _ in words]
    for i in range(n):
        puti(r[i],0,i);puti(r[i],4,-2147483647);r[i][8:12]=words[i][16:20]
        r[i][14]=int(case['special']and geth(words[i],50)&64!=0 and geth(words[i],48)==65534)
    for pos,ordinal in enumerate(case['selected']):
        r[ordinal][13]=1
        if pos==0:r[ordinal][12]=1
    group=999
    for i in range(n):
        if r[i][17]:continue
        group+=1;puti(r[i],4,group);r[i][15]=int(geth(words[i],50)&768!=0)
        others=[]
        for j in range(i+1,n):
            if not r[j][17]and geti(words[i],60)==geti(words[j],60)and geti(words[i],0)==geti(words[j],0)and r[i][14]==r[j][14]:
                puti(r[j],4,group);others.append(j)
        best=i;cost=getf(r[i],8);selected=r[i][13];first=r[i][12]
        for j in others:
            selected|=r[j][13];first|=r[j][12]
            if cost==getf(r[j],8):ties+=1
            if cost<getf(r[j],8):best=j;cost=getf(r[j],8)
        if best!=i:words[i],words[best]=words[best],words[i];putf(r[i],8,cost);swaps+=1
        r[i][12]=first
        for j in others:r[j][17]=1;putf(r[j],8,sentinel);duplicates+=1
        r[i][16]=int(bool(selected or r[i][15]))
    if case['special']:
        for row in r:
            if row[14]:row[16]=1;break
    protected=sum(bool(row[16])and not row[17]for row in r)
    alive=n-sum(bool(row[17])for row in r)
    flag_count=sum(bool(row[15])for row in r);selected_count=sum(bool(row[13])for row in r)
    if flag_count!=selected_count:category_selected_discriminators+=1
    quota=case['quota']+flag_count
    if quota<alive:
        prefix=0
        for i in range(n):
            if r[i][16]:
                r[i],r[prefix]=r[prefix],r[i];prefix+=1
                if prefix>=protected:break
        sort(r,protected,n-1,8,True);alive=protected
        for i in range(protected,n):
            if not r[i][17]:
                if alive>=quota:r[i][17]=1;putf(r[i],8,sentinel)
                else:alive+=1
    selected_first=next(i for i,row in enumerate(r)if row[12]);r[0],r[selected_first]=r[selected_first],r[0]
    first=geti(r[0],4)
    for row in r:
        if geti(row,4)==first:puti(row,4,0)
    sort(r,1,n-1,8,True)
    if case['special']:
        #Independent stable partition of the finite live tail, after checking
        #native bubble13CE00 only exchanges special/ordinary inversions.
        end=2
        while end<n and getf(r[end],8)!=sentinel:end+=1
        r[1:end]=[row for row in r[1:end]if not row[14]]+[row for row in r[1:end]if row[14]]
    for i in range(1,n):
        if getf(r[i],8)==sentinel:break
        old=geti(r[i],4)
        for row in r:
            if geti(row,4)==old:puti(row,4,i)
    output=[]
    for row in r:
        if geti(row,4)==-1 or getf(row,8)==sentinel:break
        word=words[geti(row,0)].copy()
        if row[14]:putf(word,16,sentinel)
        output.append(bytes(word))
    assert len(output)==alive
    assert output==list(map(dec,case['output']))
    r.sort(key=lambda row:geti(row,0))
    assert list(map(bytes,r))==list(map(dec,case['rows']))
    assert [geti(row,4)for row in r]==case['map']
    assert [geti(row,0)for row in r if row[17]]==case['released']
    assert quota==case['quotaAfter']
    full_words+=len(output);full_rows+=n
assert len(cases)==2048 and swaps>100 and ties>100 and category_selected_discriminators>1000
print(f'PASS independent{len(cases)} cases/{full_words} complete120B payloads/{full_rows} complete20B ranking rows/{duplicates} duplicates/{swaps} representative swaps/{ties} ties/{category_selected_discriminators} quota discriminators')
