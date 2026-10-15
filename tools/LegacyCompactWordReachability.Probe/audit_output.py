"""Independent packed-row207CB0 predecessor marking; no vendor execution.

The real fixtures have no active finalized-prefix primary node and therefore
make no207780 calls in this phase. Query behavior is not inferred from this
oracle: captures with any such call are explicitly outside its scope.
"""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def s8(b,p):return struct.unpack_from('<b',b,p)[0]
total=0;marked_beams=0;marked_alts=0
for capture in json.loads(Path(sys.argv[1]).read_bytes()):
    assert capture['checks']==capture['backendCalls']==0,'Source-query controls require their separate actual owner'
    rows=[bytearray(base64.b64decode(n['inputRows'])) for n in capture['nodes']]
    pools=[bytearray(base64.b64decode(n['inputAlternatives'])) for n in capture['nodes']]
    limits=[n['sourceEnd'] for n in capture['nodes']];primary=capture['primary']
    active=[int(n>capture['prefixEnd']) for n in range(len(rows))]
    for node in range(len(rows)):
        assert len(rows[node])%88==0 and limits[node]<=len(rows[node])//88
        assert len(pools[node])==primary*80
        for at in range(limits[node]):rows[node][at*88+6]&=253
        for at in range(primary*2):pools[node][at*40+27]&=127
    for node in reversed(range(len(rows))):
        if not active[node]:continue
        for ordinal in range(limits[node]):
            if i32(rows[node],ordinal*88+16)>=67108864:continue
            assert not(ordinal<primary and node<=capture['prefixEnd']),'Missing native source query'
            if ordinal<primary:
                for alternative in range(2):
                    if i32(pools[node],(ordinal*2+alternative)*40)>=67108864:break
                    at_node,at_ordinal,at_alt=node,ordinal,alternative
                    while at_node>=0 and at_ordinal<primary:
                        assert 0<=at_ordinal<primary and 0<=at_alt<2
                        p=(at_ordinal*2+at_alt)*40
                        if pools[at_node][p+27]&128:break
                        pools[at_node][p+27]|=128
                        if pools[at_node][p+31]&32:break
                        current=pools[at_node]
                        at_ordinal=u16(current,p+24)
                        at_node-=s8(current,p+26)+1
                        at_alt=current[p+27]&127
            at_node,at_ordinal=node,ordinal
            while at_node>=0:
                assert 0<=at_ordinal<limits[at_node]
                p=at_ordinal*88
                if at_ordinal<primary:
                    if rows[at_node][p+6]&2:break
                else:active[at_node]=1
                rows[at_node][p+6]|=2
                current=rows[at_node]
                at_ordinal=u16(current,p+30)
                at_node-=s8(current,p+32)+1
    assert bytes(active)==base64.b64decode(capture['selection'])
    for n,output in enumerate(capture['nodes']):
        assert rows[n]==base64.b64decode(output['rows']),('full88storage',n)
        assert pools[n]==base64.b64decode(output['alternatives']),('full40storage',n)
        total+=len(rows[n])+len(pools[n])
        marked_beams+=sum(bool(rows[n][i*88+6]&2) for i in range(limits[n]))
        marked_alts+=sum(bool(pools[n][i*40+27]&128) for i in range(primary*2))
print(f'PASS independent whole storage {total} bytes, {marked_beams} beam/{marked_alts} primary-alternative marks; unchanged outside-source allocator residue included')
