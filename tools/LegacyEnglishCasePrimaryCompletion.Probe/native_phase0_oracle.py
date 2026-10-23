"""Independent native phase0 oracle, from pinned PE/C bodies, never C# execution.

1FCA30 field order/signedness was decoded directly from pinned PE instructions.
1FC650 recursion/pivot,1F93A0 cursors/live writes,203DE0,1F73D0 and213410 follow
the existing immutable native exports. This file does not read production C#.
"""
import struct,hashlib,json,itertools
from pathlib import Path
HERE=Path(__file__).resolve().parent
ROOT=HERE.parent
IMP=0x4000000
def u(b,o,n=4):return int.from_bytes(b[o:o+n],'little')
def s(b,o):return int.from_bytes(b[o:o+4],'little',signed=True)
def put(b,o,v,n=4):b[o:o+n]=(v&((1<<(n*8))-1)).to_bytes(n,'little')
def key(b):
    # Exact JB/JA vs JL/JG sequence at1801FCA30..1FCAB9.
    return(u(b,0,8),u(b,0x24,2),u(b,0x18),s(b,0x30),s(b,0x34),u(b,0x1c,2),u(b,0x70,8),s(b,0x68),b[0x21]>>7)
def quicksort(rows,lo,hi):
    if lo>=hi:return
    left,right=lo,hi;pivot=bytes(rows[(lo+hi)//2])
    while True:
        while key(rows[left])<key(pivot):left+=1
        while key(rows[right])>key(pivot):right-=1
        if left>right:break
        rows[left],rows[right]=rows[right],rows[left];left+=1;right-=1
        if left>right:break
    if lo<right:quicksort(rows,lo,right)
    if left<hi:quicksort(rows,left,hi)
def merge(pool,leader,duplicate):
    a=u(leader,0x58);b=u(duplicate,0x58)
    def entry(row,i):return pool[(row*2+i)*40:(row*2+i+1)*40]
    aa=entry(a,0);bb=entry(b,0)
    heap=(aa[31]&bb[31]&32)!=0
    choose_a=(s(leader,16) if heap else s(aa,0))<(s(duplicate,16) if heap else s(bb,0))
    selected=[(a if choose_a else b,0)];ids={u(entry(selected[0][0],0),8,8)}
    ai,bi=(1,0) if choose_a else (0,1)
    while len(selected)<2:
        av=ai<2 and s(entry(a,ai),0)<IMP;bv=bi<2 and s(entry(b,bi),0)<IMP
        if not(av or bv):break
        if av and (not bv or s(entry(a,ai),0)<s(entry(b,bi),0)):row,index=a,ai;ai+=1
        else:row,index=b,bi;bi+=1
        identity=u(entry(row,index),8,8)
        if identity not in ids:ids.add(identity);selected.append((row,index))
    for out_index in range(len(selected)-1,-1,-1):
        row,index=selected[out_index]
        if row==a and index==out_index:continue
        # Read each source at this descending iteration; never snapshot all picks.
        src=entry(row,index);at=(a*2+out_index)*40
        pool[at:at+8]=src[:8];pool[at+30]=src[30]
        pool[at+24:at+28]=src[24:28];pool[at+8:at+24]=src[8:24]
        pool[at+31]=(pool[at+31]&0xc0)|(src[31]&0x3f)
def allocate(backing,count,capacity,record):
    cost=s(record,16)
    if not capacity or cost>=IMP:return count,False
    if count==capacity:
        if s(backing[0],16)<=cost:return count,False
        at=0
        while at*2<count-1:
            child=at*2+1
            if child<count-1 and s(backing[child],16)<=s(backing[child+1],16):child+=1
            if s(backing[child],16)<=cost:break
            backing[at]=bytearray(backing[child]);at=child
    else:
        at=count;count+=1
        while at:
            parent=(at-1)//2
            if cost<=s(backing[parent],16):break
            backing[at]=bytearray(backing[parent]);at=parent
    backing[at]=bytearray(record)
    return count,True
def phase0(slots,active,pool,quota=125):
    rows=[bytearray(slots[i:i+128]) for i in range(0,len(slots),128)];pool=bytearray(pool)
    sorted_active=rows[:active];quicksort(sorted_active,0,active-1);rows[:active]=sorted_active
    sorted_bytes=b''.join(rows);unique=1 if active else 0
    for i in range(1,active):
        if key(rows[unique-1])==key(rows[i]):
            if s(rows[i],16)<s(rows[unique-1],16):rows[unique-1],rows[i]=rows[i],rows[unique-1]
            merge(pool,rows[unique-1],rows[i])
        else:
            if unique!=i:rows[unique]=bytearray(rows[i])
            unique+=1
    compact=b''.join(rows);backing=[bytearray(r) for r in rows];count=0
    #203DE0 consumes the next live row; prior heap writes affect only prefix.
    for i in range(unique):count,_=allocate(backing,count,len(backing),bytes(backing[i]))
    rebuilt=b''.join(backing);old=count
    if old>quota:
        count=quota
        for i in range(quota,old):count,_=allocate(backing,count,quota,bytes(backing[i]))
    result=[]
    for i in range(count):
        row=bytearray(backing[i][:88])
        if u(row,60,2)==65535:put(row,60,i,2)
        ordinal=u(backing[i],88)
        result.append({'Record':row.hex(),'Primary':pool[ordinal*80:(ordinal+1)*80].hex(),'Ordinal':ordinal})
    return {'Sorted':sorted_bytes.hex(),'Compacted':compact.hex(),'Pool':pool.hex(),'Unique':unique,
      'Rebuilt':rebuilt.hex(),'Retained':b''.join(backing).hex(),'Count':count,'Rows':result}
def fixture(name,n,variant):
    slots=bytearray();pool=bytearray()
    for i in range(n):
        row=bytearray((i*17+j*29+variant*11)&255 for j in range(128))
        group=i if variant==0 else (i//3 if variant==1 else 0)
        put(row,0,0x0000000100000000+group,8);put(row,0x24,group%217,2)
        for off in (0x18,0x30,0x34,0x68):put(row,off,0)
        put(row,0x1c,0,2);put(row,0x70,0,8);row[0x21]&=0x7f
        put(row,16,(n-i)*3 if variant!=2 else 77);put(row,20,i-100)
        put(row,0x3c,65535 if i%2==0 else i,2);put(row,0x58,i)
        slots+=row
        for j in range(2):
            alt=bytearray((i*13+j*67+k*7)&255 for k in range(40))
            put(alt,0,s(row,16)+(j*7));put(alt,4,-i-j)
            #Repeated identity and unavailable alternate controls are deliberate.
            put(alt,8,(i%4 if variant==2 else i*2)+j,8)
            if j and i%3==0:put(alt,0,IMP)
            if variant==3 and j:put(alt,8,u(pool[-40:],8,8) if pool else 0,8)
            alt[31]=(alt[31]&0xc0)|0x20|(i&31);pool+=alt
    return {'Name':name,'Active':n,'Slots':slots.hex(),'Pool':pool.hex(),'Expected':phase0(slots,n,pool)}
def generate():
    fs=[fixture('prepared-%d-%d'%(n,v),n,v) for n in (0,1,125,126,250) for v in range(4)]
    #Native field signedness and independent quicksort tie controls.
    for off,width,values in [(0,8,[0,2**64-1,2**63]),(0x24,2,[0,65535,32768]),(0x18,4,[0,2**32-1,2**31]),
       (0x30,4,[0,-1,-2**31]),(0x34,4,[0,2**31-1,-1]),(0x1c,2,[0,65535,32768]),
       (0x70,8,[0,2**64-1,2**63]),(0x68,4,[0,-1,2**31-1]),(0x21,1,[0,128,255])]:
        f=fixture('field-%x'%off,3,2);slots=bytearray.fromhex(f['Slots'])
        for i,v in enumerate(values):put(slots,i*128+off,v,width)
        f['Slots']=slots.hex();f['Expected']=phase0(slots,3,bytes.fromhex(f['Pool']));fs.append(f)
    historical=[]
    for label,digest in [('134','d1ffe91e88cf7e48f7d68cda029790c2d73f9e8c9f9d8c5640890c3ff1c712d0'),
       ('100','f68d9507a6d857235a7bd73394ee931862681274d530f3bc74237a6cad4ce818')]:
        capture=ROOT/('global-en-case-consumer-successor-'+label+'-managed-20261023.json')
        raw=capture.read_bytes();assert hashlib.sha256(raw).hexdigest()==digest
        rows=json.loads(raw)['Rows']
        for r in rows:
            nodes={x[0]:x for x in r['Nodes']};root_al={x[5]:x[0] for x in r['Nodes'] if x[1]==0}
            next_id=len(nodes)+1;slots=bytearray();pool=bytearray(len(r['Heap'])*80)
            for i,h in enumerate(r['Heap']):
                slot=bytearray.fromhex(h);put(slot,88,i);slots+=slot;leaf=nodes[u(slot,8,8)]
                assert leaf[5]<217 and nodes[leaf[1]][5]==0xfffa and nodes[leaf[1]][1]==0
                symbol=leaf[5]
                if symbol not in root_al:root_al[symbol]=next_id;next_id+=1
                at=i*80;pool[at:at+8]=slot[16:24];put(pool,at+8,root_al[symbol],8);put(pool,at+16,u(slot,8,8),8)
                pool[at+24:at+26]=slot[30:32];pool[at+26]=slot[32];pool[at+30]=slot[38];pool[at+31]=32|(slot[33]&31)
                put(pool,at+40,IMP);put(pool,at+44,IMP);pool[at+70]=0
            fs.append({'Name':'historical-'+label+'-'+r['Name'],'Active':len(r['Heap']),'Slots':slots.hex(),'Pool':pool.hex(),
                'Expected':phase0(slots,len(r['Heap']),pool),'ExpectedContextCount':next_id-1})
        historical.append({'path':str(capture),'sha256':digest,'rows':len(rows)})
    out={'Scope':'PREPARED EXISTING NUMERICAL KERNELS ONLY; no owning English execution','Fixtures':fs}
    out['HistoricalInputs']=historical
    (HERE/'fixtures.json').write_text(json.dumps(out,separators=(',',':'))+'\n')
    print(json.dumps({'status':'GENERATED_NATIVE_PREPARED_ORACLE','fixtures':len(fs),'sha256':hashlib.sha256((HERE/'fixtures.json').read_bytes()).hexdigest()}))
if __name__=='__main__':generate()
