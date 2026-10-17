"""Independent full80 ordinary main118 status/cost/accumulator interpreter.
Prepared packets use actual owned graph/cache facts, not scheduler/winner output.
"""
import ast,hashlib,json,struct,sys,subprocess
from pathlib import Path
if len(sys.argv)!=5:raise ValueError('AK separation-profile global-transition-cache-capture word-transitions-capture')
root=Path(__file__).resolve().parents[2];ns={}
subprocess.run([sys.executable,str(root/'tools/LegacyMainGlobalTransition.Probe/audit_output.py'),sys.argv[1],sys.argv[2],sys.argv[3]],check=True)
tree=ast.parse((root/'tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text())
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'raw-al-reader','exec'),ns)
ak=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
a=ns['read'](ak[99:]);labels={v:k for k,v in a['text'].items()};replacement=labels['\ufffd'];quotes={labels["'"],labels['"']}
matrix=list(struct.unpack_from('<72i',ak,189379+18));strict=bool(ak[189379+17]);assert strict
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
wrap=lambda x:(x+2**31)%2**32-2**31
slope=struct.unpack_from('<f',ak,189165+185)[0];threshold=struct.unpack_from('<f',ak,189165+37)[0];IMP=0x4000000
cases=json.loads(Path(sys.argv[4]).read_text());caches=json.loads(Path(sys.argv[3]).read_text());total=pending=cross=clip=earlier=pathTotal=pathAccepted=mutated=0
for case,cache in zip(cases,caches):
    descriptors={r[0]:r[1:]for r in case['descriptors']};groups=case['groups'];contexts=[bytes.fromhex(v)for v in case['contexts']];arcs=case['arcs']
    assert len(groups)==len(cache['groups']) and len(arcs)==len(cache['nodes']) and case['rootGroup']<len(groups)
    for group,proven in zip(groups,cache['groups']):
        assert group['descriptor']==proven['descriptor'] and descriptors[group['descriptor']][0]==proven['line']
        assert group['wholeStarts']==all(start==0 for _,start,_ in proven['ranges'])
    for row,proven in zip(arcs,cache['nodes']):
        assert len(row)==len(proven) and all(a[:2]==[p['group'],p['span']]for a,p in zip(row,proven))
    assert all(facts[2]!=0x2000000 for facts in descriptors.values())
    assert all(p==bytes([1])+bytes(15)for p in contexts)
    def status(request,arc,current,previous,cost):
        kind=request[28];symbol,prior=struct.unpack_from('<HH',request,36);limit=struct.unpack_from('<i',request,52)[0]
        aux=(arc[3]+32768)%65536-32768
        if kind==0:
            if arc[2]&8:return 4,IMP
            if aux>=0:
                request[60]=1;cost=wrap(cost+8192);return 3+(cost>=limit),cost
            if(symbol==replacement)!=(prior==replacement):return 4,IMP
            if current['descriptor']==previous['descriptor']:return 0,cost
            cost=wrap(cost+8192);return 3+(cost>=limit),cost
        if kind==5:
            return (0,cost)if not arc[2]&8 and aux<0 and current['descriptor']==previous['descriptor']else(4,IMP)
        if arc[2]&8 or aux>=0:return 3,cost
        if(symbol==replacement)!=(prior==replacement):return 3,cost
        if current['descriptor']==previous['descriptor']:
            return(4,IMP)if descriptors[current['descriptor']][1]==0x2000001 else(0,cost)
        if symbol in quotes or prior in quotes:
            first,last=struct.unpack_from('<ii',request,20);value=matrix[first+6*last]
            if not strict:value=min(value,8192)
            elif value>=IMP:return 3+(IMP>=limit),IMP
            cost=wrap(cost+value);return 3+(cost>=limit),cost
        return(0 if descriptors[current['descriptor']][0]==descriptors[previous['descriptor']][0]else 3),cost
    for call in case['calls']:
        before=bytes.fromhex(call['before']);expected=bytearray(before);u16=lambda at:struct.unpack_from('<H',before,at)[0];i32=lambda at:struct.unpack_from('<i',before,at)[0]
        node,previousSource=i32(0),i32(4);ordinal=u16(12);arc=arcs[node][ordinal];current=groups[arc[0]];kind=before[28];cost=call['costBefore'];start=cost;limit=i32(52)
        if kind==2 and not current['wholeStarts']:cost=IMP;accepted=False;clip+=1
        elif i32(64)==9 or kind==6:accepted=True
        else:
            previous=groups[case['rootGroup']]if previousSource==-1 else groups[arcs[previousSource][u16(14)][0]]
            s,cost=status(expected,arc,current,previous,cost)
            cross+=current['descriptor']!=previous['descriptor'];pending+=arc[3]<32768
            if kind==5 and s==0:cost=wrap(cost+cache['nodes'][node][ordinal]['costs'][2]);accepted=cost<limit
            elif kind==0 and s in(0,1)or kind==2 and s==0:
                assert previousSource>=0
                currentState=cache['nodes'][node][ordinal];previousState=cache['nodes'][previousSource][u16(14)]
                ca=cache['groups'][currentState['group']];pa=cache['groups'][previousState['group']]
                ab=list(map(f,ca['bounds']));bb=list(map(f,pa['bounds']));height=f(ca['height'])
                movement=f(max(f(ab[0]-bb[0]),f(ab[2]-bb[2]))/height);term=f(f(movement-threshold)*slope)if movement<=threshold else 0
                if kind==0:correction=currentState['costs'][1 if i32(64)in(7,8)else 0]
                else:correction=currentState['matrix'][i32(20)+6*(3 if i32(68)==10 else i32(24))]
                cost=wrap(cost-int(term)*8192+correction);accepted=cost<limit
            else:accepted=s!=4
            earlier+=previousSource!=i32(8)
        accumulator=wrap(i32(48)+cost-start)if accepted else IMP;struct.pack_into('<i',expected,48,accumulator)
        assert call['accepted']==accepted and call['costAfter']==cost and bytes.fromhex(call['after'])==expected,(total,node,ordinal,kind)
        total+=1
    boxes=[list(map(f,g['bounds']))for g in cache['groups']]
    decode=lambda k:wrap(k<<1)>>8
    selectors=case['selectors']
    def path_search(arc,end,path):
        if arc[5]>end:return False
        saved=len(path)
        if groups[arc[0]]['flags6']==0:path.append(arc[0])
        if arc[5]==end:return True
        next_node=decode(arc[4])+1
        #Arc identity supplies its own selector vector, independently found.
        where=[(n,a)for n,row in enumerate(arcs)for a,v in enumerate(row)if v is arc]
        assert len(where)==1
        n,a=where[0]
        for index in selectors[n][a]:
            if path_search(arcs[next_node][index],end,path):return True
        del path[saved:]
        return False
    def intersect(a,b):return a[0]<=b[2]and b[0]<=a[2]and a[1]<=b[3]and b[1]<=a[3]
    def rounded(v):return int(f(v+f(-.5 if v<=0 else .5)))
    for call in case['pathCalls']:
        packet=bytes.fromhex(call['request']);node,key,end_node=struct.unpack_from('<iii',packet);key=struct.unpack_from('<i',packet,4)[0];ordinal=struct.unpack_from('<H',packet,12)[0];end_arc=struct.unpack_from('<H',packet,16)[0]
        end=arcs[end_node][end_arc][5];path=[];found=False
        for arc in arcs[decode(key)]:
            if arc[4]==key and path_search(arc,end,path):found=True;break
        if not found or not path:expectedAccepted=False;cost=0
        else:
            pathbox=boxes[path[0]].copy()
            for g in path[1:]:
                old=boxes[g].copy();boxes[g]=[min(boxes[g][0],pathbox[0]),min(boxes[g][1],pathbox[1]),max(boxes[g][2],pathbox[2]),max(boxes[g][3],pathbox[3])];mutated+=old!=boxes[g]
            group=arcs[node][ordinal][0];box=boxes[group]
            if groups[group]['flags6']or not intersect(box,pathbox):expectedAccepted=False;cost=IMP
            else:
                a=f(f(pathbox[0]-box[0])/15)if box[0]<=pathbox[0]else f(box[0]-pathbox[0])
                b=f(f(box[2]-pathbox[2])/15)if pathbox[2]<=box[2]else f(pathbox[2]-box[2])
                c=f(f(pathbox[1]-box[1])/15)if box[1]<=pathbox[1]else f(box[1]-pathbox[1])
                d=f(f(box[3]-pathbox[3])/15)if pathbox[3]<=box[3]else f(pathbox[3]-box[3])
                height=f(cache['groups'][group]['height']);total_distance=f(f(f(a+b)+c)+d);cost=rounded(f(f(f(total_distance*.5)*8192)/height))
                prior=decode(key)-1
                if prior!=-1:
                    extent=max((a[1]+1 for a in arcs[prior]),default=0);overlap=0
                    for span in range(extent):
                        assert prior>=0
                        for arc in arcs[prior]:
                            other=boxes[arc[0]]
                            if arc[1]==span and not groups[arc[0]]['flags6']and intersect(box,other):
                                x=min(f(other[2]-box[0]),f(box[2]-other[0]));overlap=max(overlap,min(x,f(box[3]-box[1]),f(other[3]-other[1])))
                        prior-=1
                    cost=wrap(cost+rounded(f(f(f(overlap*5)*8192)/height)))
                expectedAccepted=cost<struct.unpack_from('<i',packet,52)[0]
        assert (call['accepted'],call['cost'])==(expectedAccepted,cost),('path',pathTotal,node,ordinal,path,call,cost)
        pathTotal+=1;pathAccepted+=expectedAccepted
    assert [list(map(f,b))for b in case['finalBounds']]==boxes
assert len(cases)==len(caches)==8 and total>10000 and pending and cross and clip and earlier
assert pathTotal>1000 and pathAccepted>0 and mutated>0
print(f'PASS path140 {pathTotal}/{pathAccepted} accepted/{mutated} actualmutableboxupdates')
print(f'PASS independent{total} full80 callbacks/{pending} actualpending/{cross} cross-descriptor/{clip} clipped/{earlier} earlier-source references; native status/cost/flags/accumulator/unchanged bytes')
