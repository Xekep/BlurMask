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
assert len(p['controls'])==128
for n,c in enumerate(p['controls']):
    residue=bytes((n*73+i*31+11)&255 for i in range(72));assert base64.b64decode(c['residue'])==residue
    wanted,visited,residuals=expected(residue)
    assert base64.b64decode(c['rows'])==wanted,(n,'full72 mismatch')
    assert c['visited']==visited and c['residuals']==residuals
    #The invariant padding must survive every numeric native append.
    for i in range(0,len(wanted),72):
        for padding in [6,7,18,19,30,31,42,43,44,45,46,47,63,70,71]:assert wanted[i+padding]==residue[padding]
assert p['guards']==9 and p['rowCount']==sum(len(base64.b64decode(c['rows']))//72 for c in p['controls'])
print(f'PASS independent128 actual selected compact paths, {p["rowCount"]} full72 rows and padding, real source-key/filtered-parent/carry metadata; no expected text or segment publication.')
