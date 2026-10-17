"""Independent raw compact/graph21FFD0 -> complete72 append-event oracle."""
import base64,hashlib,importlib.util,json,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('read_glyph_alphabet',ROOT/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
if len(sys.argv)!=3:raise SystemExit('AK.res capture.json')
ak=Path(sys.argv[1]).read_bytes();p=json.loads(Path(sys.argv[2]).read_bytes())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
texts=module.read_alphabet(ak);labels={v:k for k,v in texts.items()}
marker=labels.get('\x03',65535);joiner=labels.get('\u200d',65535);soft=labels.get('\xad',65535);unit=struct.unpack_from('<H',ak,105)[0]
compact=base64.b64decode(p['compact'])[:p['recordCount']*40];starts=p['starts'];graph=p['graph']
def u16(row,at):return struct.unpack_from('<H',row,at)[0]
def i32(row,at):return struct.unpack_from('<i',row,at)[0]
def u64(row,at):return struct.unpack_from('<Q',row,at)[0]
def row(index):assert 0<=index<p['recordCount'];return compact[index*40:(index+1)*40]
def reference(raw):
    value=u64(raw,8);node=value&0xffffff;node=node-0x1000000 if node&0x800000 else node
    return node,(value>>24)&65535
def advance(cursor):
    index,node,arc,alternative=cursor;current=row(index);prior=i32(current,16)
    if prior==-1:return (-1,-1,65535,alternative)
    parent=row(prior);priorNode,priorArc=reference(parent)
    if u16(parent,30)!=65535 or u16(current,28)==marker and priorNode<node:
        if alternative<65534:
            assert 0<=priorNode<len(starts)-1;prior=starts[priorNode]+alternative;parent=row(prior)
        alternative=u16(parent,30)
    priorNode,priorArc=reference(parent);return prior,priorNode,priorArc,alternative
def pack(node,arc):return (node&0xffffff)|((arc&65535)<<24)
def boundary(node,arc,which):
    assert 0<=node<len(graph) and 0<=arc<len(graph[node]);return graph[node][arc][which]
def span(base,other,extra=False):return pack(base,other-base)|((1<<56) if extra else 0)
def key(kind,label,previous,current,earlier,previousArc,currentArc,earlierArc,first):
    if kind<3:return pack(current,currentArc),False
    if kind==3 and (((label+7)&65535)<4 or label==soft) and previous>=0 and earlier>=0:
        a=boundary(earlier,earlierArc,2);b=boundary(previous,previousArc,3)
        if a==-1:a,b=b,boundary(previous,previousArc,2)
        return span(a,b),first
    other=not first or previous==-1;selected,arc=(current,currentArc) if other else (previous,previousArc)
    return span(boundary(selected,arc,3),boundary(selected,arc,2),other),first
def expected(residue):
    idx=p['selected'];node=len(graph)-1;arc=reference(row(idx))[1]
    alternative=p['alternative'] if p['alternative']!=65535 else u16(row(idx),30)
    cursor=idx,node,arc,alternative
    visited=[];emitted=[];residuals=[];lastNode=earlierNode=-1;lastArc=earlierArc=65535;regular=65535;first=True
    target=bytearray(residue)
    def w16(at,value):struct.pack_into('<H',target,at,value&65535)
    def w32(at,value):struct.pack_into('<I',target,at,value&0xffffffff)
    while cursor[1]!=-1:
        index,node,arc,_=cursor;assert index not in visited;visited.append(index);current=row(index)
        symbol=u16(current,28);kind=current[32]&7;direct=i32(current,16);directNode,directArc=(-1,65535) if direct==-1 else reference(row(direct))
        if (marker==65535 or symbol!=marker) and symbol not in (joiner,65535,65533):
            if symbol==unit:residuals.append(pack(node,arc))
            else:
                assert kind<=3
                if kind<3:regular=symbol
                filtered=advance(cursor);filterSeen=set()
                while filtered[1]!=-1:
                    assert filtered not in filterSeen;filterSeen.add(filtered);candidate=row(filtered[0])
                    if candidate[32]&7<3 and u16(candidate,28) not in (65535,unit):break
                    filtered=advance(filtered)
                normalized=soft if symbol==65524 else 65529 if symbol in (65525,65527,65528) else symbol
                if normalized==193:normalized=labels["'"]
                packed,first=key(kind,normalized,filtered[1],node,earlierNode,filtered[2],arc,earlierArc,first)
                for offset,value in [(0,node),(8,index),(12,directNode),(20,direct),(24,filtered[1]),(32,filtered[0]),(36,earlierNode),(64,0)]:w32(offset,value)
                for offset,value in [(4,arc),(16,directArc),(28,filtered[2]),(40,earlierArc),(56,regular),(58,normalized)]:w16(offset,value)
                struct.pack_into('<Q',target,48,packed);target[60:63]=bytes([kind,(current[32]>>6)&1,current[33]])
                target[68]=0;target[69]=(current[7]>>1)&1;emitted.append(bytes(target))
        if lastNode!=node:earlierNode,earlierArc=lastNode,lastArc
        lastNode,lastArc=node,arc;cursor=advance(cursor)
    return b''.join(emitted),visited,residuals

# The numeric oracle above is independent of the runtime flush implementation.
# Packet bytes below are independently reconstructed from raw compact records,
# literal native integer wrap/F32 operations and source boundary coordinates.
import numpy as np

def wrap(n):return (n+2**31)%2**32-2**31

def flush_expected(residue):
    allrows,visits,residuals=expected(residue)
    numeric=[allrows[i:i+72] for i in range(0,len(allrows),72)]
    index=p['selected'];node=len(graph)-1;arc=reference(row(index))[1]
    alternative=p['alternative'] if p['alternative']!=65535 else u16(row(index),30)
    cursor=index,node,arc,alternative
    out=[];pending=[];added=[];q=bytearray(64);origin=[-1]*64
    def write(at,data):q[at:at+len(data)]=data;origin[at:at+len(data)]=[-1]*len(data)
    def w16(at,v):write(at,struct.pack('<H',v&65535))
    def w32(at,v):write(at,struct.pack('<I',v&0xffffffff))
    def wf(at,v):write(at,struct.pack('<f',v))
    w16(12,65534);w16(46,65535);w32(52,-1);w32(56,-1);w32(60,-1)
    for at,src,n in [(14,50,2),(20,60,4),(28,68,16)]:origin[at:at+n]=range(src,src+n)
    total=p['totalCost'];cost18=p['rowCost14'];baseNode=node;item=0;calls=[]
    while cursor[1]!=-1:
        index,node,arc,_=cursor;r=row(index);raw=u16(r,28);direct=i32(r,16)
        priornode=-1 if direct==-1 else reference(row(direct))[0]
        # Current real captures have no compact+24hex terminal identity.
        # This is not field18 at decimal24 and does not invoke source70.
        assert i32(r,36)==-1,'new actual source70 invocation needs independent metadata decoding'
        if marker!=65535 and raw==marker:baseNode=priornode
        elif raw!=joiner:
            if raw in (65531,65529,65525,65530):w32(52,-1)
            elif i32(q,52)==-1:w32(52,boundary(node,arc,3))
            if raw==unit:added.append(pack(node,arc))
            elif raw not in (65535,65533):
                itemrow=numeric[item];item+=1;assert i32(itemrow,8)==index;pending.append(itemrow)
                filtered=advance(cursor)
                while filtered[1]!=-1:
                    other=row(filtered[0]);label=u16(other,28)
                    if other[32]&7<3 and label not in (65535,unit):break
                    if i32(other,36)!=-1:w32(56,i32(other,36))
                    filtered=advance(filtered)
                if u16(q,12)==65534 and r[32]&7!=2:w16(12,r[4])
                symbol=u16(itemrow,58);protocol=(((symbol+7)&65535)&65533)==0 or symbol==65530
                previous=direct!=-1 and u16(row(direct),28) in (65531,65529,65525,65530)
                if previous or protocol:
                    w32(48,-1 if i32(q,52)==-1 else boundary(node,arc,2))
                    if protocol:
                        wf(0,1);wf(4,0);wf(8,0)
                        if direct==-1 or u16(row(direct),28)==65535:w16(12,65535)
                    else:
                        basecost=0 if filtered[1]==-1 else wrap(p['cumulative'][filtered[1]]+i32(row(filtered[0]),20))
                        base18=0 if filtered[1]==-1 else i32(row(filtered[0]),24)
                        rel=np.float32(np.float32(wrap(total-basecost))/np.float32(wrap((baseNode-filtered[1])*8192)))
                        wf(4,rel);wf(8,np.float32(np.float32(wrap(cost18-base18))*np.float32(1/8192)));wf(0,np.float32(np.float32(-5)*rel))
                        write(40,bytes([int(filtered[1]!=-1 and u16(row(filtered[0]),28)==joiner)]))
                        total,cost18=basecost,base18
                    out.append(dict(current=index,filtered=filtered[0],protocol=protocol,rows=b''.join(pending),template=bytes(q),origins=origin.copy(),residualAdditions=added.copy()))
                    prev=q[20:24];prevOrigin=origin[20:24]
                    q=bytearray(64);origin=[-1]*64;w16(12,65534);w32(20,-1);w16(46,u16(pending[0],56));w32(52,-1);w32(56,-1)
                    q[60:64]=prev;origin[60:64]=prevOrigin
                    pending=[];added=[];baseNode=priornode
        cursor=advance(cursor)
    assert not pending and not added and item==len(numeric)
    return allrows,visits,out,calls


def native_empty_word(source):
    word=bytearray(source)
    for at,value in [(16,0x3f800000),(20,0x3f800000),(24,0),(28,0),(40,0),(48,65534),(60,0xffffffff),(84,0xffffffff),(96,0),(112,0)]:struct.pack_into('<I',word,at,value)
    struct.pack_into('<Q',word,8,0xffffffffffffffff);struct.pack_into('<H',word,53,0);word[55]=0
    return bytes(word)

def apply_packet(source,packet):
    result=bytearray(source)
    for src,dest,n in [(0,16,4),(4,24,4),(8,28,4),(12,48,2),(14,50,2),(16,53,1),(17,54,1),(20,60,4),(28,68,4),(32,72,4),(36,76,4),(40,80,4),(44,55,1)]:
        result[dest:dest+n]=packet[src:src+n]
    return bytes(result)

assert len(p['controls'])==64
for n,control in enumerate(p['controls']):
    residue=bytearray((n*13+i*29+7)&255 for i in range(120))
    for at,size in [(32,8),(88,12),(104,12)]:residue[at:at+size]=bytes(size)
    assert base64.b64decode(control['residue120'])==residue
    initial=native_empty_word(residue);assert base64.b64decode(control['initialWord'])==initial
    residue72=bytes((n*73+i*31+11)&255 for i in range(72))
    assert base64.b64decode(control['residue72'])==residue72
    allrows,visits,flushes,calls=flush_expected(residue72)
    assert len(control['flushes'])==len(flushes) and not calls
    word=initial
    for ordinal,(actual,flush) in enumerate(zip(control['flushes'],flushes)):
        assert actual['ordinal']==ordinal and base64.b64decode(actual['beforeWord'])==word
        packet=bytearray(flush['template'])
        for at,origin in enumerate(flush['origins']):
            if origin!=-1:packet[at]=initial[origin]
        assert base64.b64decode(actual['packet'])==packet
        # These real native paths have no residual additions; genuine217DB0
        # count0 is byte-preserving. Nonempty integration controls are separate.
        assert flush['residualAdditions']==[] and actual['residualStorage']==[] and actual['remaining']==0
        assert base64.b64decode(actual['rows'])==flush['rows']
        word=apply_packet(word,packet);assert base64.b64decode(actual['appliedWord'])==word
        word=native_empty_word(word);assert base64.b64decode(actual['resetWord'])==word

# Supplement: explicitly PREPARED residual refs over the same actual graph.
# These do not pretend to be residual events on the actual winning trace.
def normalize_ranges(source):
    values=[tuple(v) for v in source];pending=[(0,len(values)-1)]
    while pending:
        lo,hi=pending.pop();pivot=values[(lo+hi)//2][:2];a,b=lo,hi
        while a<=b:
            while values[a][:2]<pivot:a+=1
            while values[b][:2]>pivot:b-=1
            if a<=b:values[a],values[b]=values[b],values[a];a+=1;b-=1
        if a<hi:pending.append((a,hi))
        if lo<b:pending.append((lo,b))
    i=1
    while i<len(values):
        a,b=values[i-1:i+1]
        if a[0]==b[0] and b[1]==a[2]-1:values[i-1]=(a[0],a[1],b[2]);del values[i]
        else:i+=1
    return [list(v) for v in values]

def decode(key):return key&0xffffff,(key>>24)&65535

def prepared_oracle(case):
    data=bytearray(base64.b64decode(case['before']));residual=case['references'].copy();count=len(residual)
    extras=[[] for _ in graph]
    def merge(keys):
        last,_=decode(keys[-1]);items=[];starts=[];ends=[]
        for keyvalue in keys:
            node,arc=decode(keyvalue);group=graph[node][arc]
            items.extend(p['groupRanges'][group[0]]);starts.append(group[2]);ends.append(group[3])
        ranges=normalize_ranges(items)
        for i,old in enumerate(extras[last]):
            if old==ranges:return pack(last,len(graph[last])+i)
        matching=next((i for i,group in enumerate(p['groupRanges']) if group==ranges),None)
        if matching is not None:
            for node,arcs in enumerate(graph):
                for arc,g in enumerate(arcs):
                    if (g[0],g[2],g[3])==(matching,min(starts),max(ends)):return pack(node,arc)
        keyvalue=pack(last,len(graph[last])+len(extras[last]));extras[last].append(ranges);return keyvalue
    def fact(node,arc):return p['facts'][node][arc]
    for k in range(count):
        reference=residual[k];node,arc=decode(reference);box=fact(node,arc);assert box[6]==0
        best=np.finfo(np.float32).max;selected=None
        for i in range(len(data)//72):
            at=i*72
            if data[at+60]!=0:continue
            n=i32(data,at);a=u16(data,at+4);candidate=fact(n,a)
            if candidate[6]!=0 or candidate[4]!=box[4]:continue
            f=np.float32
            distance=f(f(f(candidate[0])+f(candidate[2]))-f(f(box[0])+f(box[2])));distance=f(distance*f(.5))
            corrected=False;label=u16(data,at+58);baseline,height=map(f,p['guide'])
            if candidate[4]!=-1 and label<len(p['alphabetFlags']) and p['alphabetFlags'][label] and box[5] and distance<0 and f(f(f(box[1])+f(box[3]))*f(.5))<f(baseline-f(height*f(.75))):
                distance=f(abs(distance)-f(height+height));corrected=True
            if not corrected:distance=abs(distance)
            if distance<best:best=distance;selected=i
        if selected is not None:
            insertion=i32(data,selected*72+64)
            for j in range(k-1,insertion-1,-1):residual[j+1]=residual[j]
            residual[insertion]=reference
            for i in range(selected,len(data)//72):struct.pack_into('<i',data,i*72+64,i32(data,i*72+64)+1)
    for i in reversed(range(len(data)//72)):
        at=i*72;start=i32(data,at-8) if i else 0;delta=i32(data,at+64)-start
        if delta==0:continue
        keyvalue=u64(data,at+48)
        if count<len(residual):residual[count]=keyvalue
        else:residual.append(keyvalue)
        count+=1;length=delta+1
        keyvalue=merge(residual[start:start+length]);struct.pack_into('<Q',data,at+48,keyvalue);data[at+68]=1
        if start<count and length:
            if length<count-start:
                leftover=count-length-start
                for j in range(leftover):residual[start+j]=residual[start+length+j]
                count-=length
            else:count=start
    return bytes(data),residual

assert len(p['preparedResidualControls'])==256
changed=0
for case in p['preparedResidualControls']:
    data,storage=prepared_oracle(case)
    assert data==base64.b64decode(case['after']) and storage==case['storage']
    assert case['success'] and case['remaining']==0
    changed+=data!=base64.b64decode(case['before'])
assert changed>100
assert p['guards']==27 and p['associations']==sum(len(c['flushes']) for c in p['controls'])
print(f'PASS independent64 actual WORD residues/{p["associations"]} packet/120-byte prefixes/resets +256 prepared residual controls({changed} merged), same-graph geometry and range ownership; no accepted-word assertion.')
