"""Independent actual graph/raw-key/fixed-map reverse-span/count/refresh audit."""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
c=json.loads(Path(sys.argv[1]).read_bytes())
actual=prepared=refused=references=two=0
branches=set();modes=set();reordered=False
for source in c['inputs']:
    graph=source['graph'];groups=source['groupRanges'];counts=source['counts'];ticks=source['map'];factor=source['factor']
    #The original analytic raw trajectories have distinct adjacent coordinates.
    #Their source167210 tables therefore contain every supplied raw point.
    #This expected table is independent of the C# endpoint export/ending helper.
    keys=[[0,s,p]for s,n in enumerate(source['rawCounts'])for p in range(n)]
    assert source['keys']==keys and source['endingKeys']==keys
    point=[];offset=0
    for n in counts:
        physical=[]
        for p in range(n):
            tick=ticks[offset+p] if ticks else offset+p
            q,r=divmod(tick,factor);index=q+(r!=0)
            physical.append(index)
        point.append(physical);offset+=n
    assert not ticks or len(ticks)==offset
    reordered|=any(keys[row[0]][1]!=s for s,row in enumerate(point))
    singleton=[(group,start,end)for node in graph for group,span,start,end in node if span==0]
    def lookup(context,side):
        if context<0 or context>>7>=len(graph):return None
        return next((group for group,span,start,end in graph[context>>7] if span==0 and (start if side=='start' else end)==context),None)
    def resolve(key):
        low=key&0xffffff;last=low-0x1000000 if low&0x800000 else low
        delta=struct.unpack('<h',struct.pack('<H',(key>>24)&65535))[0]
        first=last+delta;mode=(key>>56)&1
        assert delta<0 and last>=0 and first>=-1
        startgroup=lookup(last,'start')
        def firstpoint(group):
            s,p,_=groups[group][0];return point[s][p]
        if startgroup is None and mode:
            group=lookup(first,'start')
            if group is None:return None
            index=firstpoint(group);branches.add('mode1-no-start');result=[(index,index)]
        else:
            if startgroup is None:
                group=lookup(last,'end')
                if group is None:return None
                physical=groups[group][-1][0];endindex=point[physical][-1];branches.add('mode0-physical-end')
            else:endindex=firstpoint(startgroup);branches.add('matched-start')
            if first==-1:result=[(endindex,endindex)];branches.add('before-source')
            else:
                previous=lookup(first,'end')
                if previous is None:return None
                s,_,stop=groups[previous][-1];previousindex=point[s][stop-1]
                adjacent=previousindex+1<len(keys) and keys[previousindex+1]==keys[endindex]
                if keys[previousindex]==keys[endindex] or adjacent:result=[(previousindex,endindex)];branches.add('coalesced')
                else:result=[(previousindex,previousindex),(endindex,endindex)];branches.add('two-degenerate')
        modes.add(mode)
        return b''.join(struct.pack('<6i',*keys[a],*keys[b])for a,b in result)
    for observed in source['actual']+source['prepared']:
        isactual='requested'not in observed
        key=observed['key']if isactual else observed['requested']
        expected=resolve(key)
        if observed.get('refused',False):assert expected is None;refused+=1;continue
        assert expected is not None
        value=base64.b64decode(observed['source24'])
        assert value==expected,(source['input'],key,value.hex(),expected.hex())
        assert len(value)in(24,48)
        assert observed['mode']==(key>>56)&1 and observed['last']==key&0xffffff
        assert observed['first']==observed['last']+struct.unpack('<h',struct.pack('<H',(key>>24)&65535))[0]
        references+=len(value)//24;two+=len(value)==48
        if isactual:
            assert observed['scalar']==0x3f800000 and observed['before40']==observed['after40']
            rows=base64.b64decode(observed['before40']);signed=(observed['packed']<<2)&65535
            if signed&32768:signed-=65536
            assert len(rows)==(signed>>4)*40
            actual+=1
        else:prepared+=1
assert (actual,prepared,refused,references,two)==tuple(c[k]for k in ['actual','prepared','refused','references','twoReferenceOutputs'])
assert actual>0 and prepared>0 and two>0 and modes=={0,1} and c['guards']==12*len(c['inputs'])
assert {'mode1-no-start','mode0-physical-end','matched-start','before-source','coalesced','two-degenerate'}<=branches
assert reordered,'Actual reverse physical ordering was not exercised.'
print(f'PASS independent {actual} actual negative owners/{prepared} prepared exports/{refused} exact bounded refusals/{references} raw metadata references/{two} two-record outputs/all6 branches/physical-reorder/{c["guards"]} guards.')
