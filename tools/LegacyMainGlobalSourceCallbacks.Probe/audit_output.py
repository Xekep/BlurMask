"""Independent actual descriptors/guides, pending metadata and protocol oracle."""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture')
capture=json.loads(Path(sys.argv[1]).read_bytes());assert len(capture)==8
assert len(capture[0]['preparedCursorControls'])==24
for cursor,count,remaining,written in capture[0]['preparedCursorControls']:
    assert(remaining,written)==((cursor-count,min(cursor,count))if cursor else(0,0))
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def bits(b,p):return struct.unpack_from('<i',b,p)[0]
arcs=requests=pending=references=0
for owner,c in enumerate(capture):
    assert c['owner']==owner
    descriptors={r['ordinal']:base64.b64decode(r['raw'])for r in c['descriptors']}
    guides=[base64.b64decode(r)for r in c['guides']]
    contexts=[]
    for context in c['contexts']:
        prefix=base64.b64decode(context['prefix']);controls=context['controls']
        assert prefix[:6]==bytes([1,0,0,0,0,0])and i32(prefix,8)==0 and all(v==-7 for v in controls)
        contexts.append(controls)
    for a in c['arcs']:
        descriptor=descriptors[a['descriptor']];line=i32(descriptor,16)
        assert len(descriptor)==88 and a['line']==line
        assert len(guides[line])==40 and a['baselineBits']==bits(guides[line],0)and a['heightBits']==bits(guides[line],4)
        assert a['topMark']==all(start==0 for physical,start,end in a['ranges'])
        assert a['sourceReferences']
        for first_kind,first_physical,first_point,last_kind,last_physical,last_point in a['sourceReferences']:
            assert first_kind==last_kind==0
            assert 0<=first_physical<len(c['originalPointCounts'])and 0<=last_physical<len(c['originalPointCounts'])
            assert 0<=first_point<c['originalPointCounts'][first_physical]and 0<=last_point<c['originalPointCounts'][last_physical]
            references+=1
        controls=contexts[a['auxiliary']]if a['auxiliary']!=65535 else[]
        for cursor,kind,previous,remaining,symbols in a['requests']:
            if cursor==0:expected_remaining=0;expected=[]
            elif controls:
                #Fresh graph1D9=1, I32−7 has highshortFFFF:1A96D0flags0,
                #each stored control writesFFF9 whilecursor>0, thenjustdecrements.
                expected_remaining=cursor-len(controls);expected=[65529]*min(cursor,len(controls))
            elif previous==-1:expected_remaining=cursor;expected=[]
            elif kind==0:expected_remaining=cursor-1;expected=[65532]
            elif kind==1:expected_remaining=cursor-1;expected=[65531] # request2C==2/ordinary0
            elif kind==2 and a['flags']&8:expected_remaining=cursor-1;expected=[65531]
            else:expected_remaining=cursor;expected=[]
            assert(remaining,symbols)==(expected_remaining,expected),(owner,a['node'],a['arc'],cursor,kind,previous)
            requests+=1;pending+=symbols.count(65529)
        arcs+=1
assert(arcs,requests,pending)==(631,20192,480)
print(f'PASS independent8 owners/{arcs} actual descriptor-guide/range bindings/{references} original source refs;{requests} protocol controls/{pending} genuine pendingFFF9 writes. No UTF8/recognition claim.')
