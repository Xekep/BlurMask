"""Literal STATIC/diagnostic first-normal-page reference; never runtime authority.
Uses caller-independent raw outgoing rows; no vendor execution or learned labels.
"""
def first_page(raw_rows,current,source_aux,ordinary_count=192):
    assert len(current)==len(source_aux)==16
    ring=[];fills=[];calls=[];levels={};visited=[];output=[]
    def backend(tag,skip,depth):
        state=tag&0xffffffff;namespace=(tag>>32)&255;tag_byte5=(tag>>40)&255
        assert (state,namespace,tag_byte5)in[(0,0,8),(97,1,1),(65,1,0)]
        assert 0<=depth<8 and 0<=skip<=12
        if state not in[c['state']for c in ring]:
            assert len(ring)<8;rows=raw_rows(state);assert len(rows)<=12
            cursor={'state':state,'slot':len(ring),'physical':0,'accepted':0,'rows':rows};ring.append(cursor)
            fills.append({'state':state,'slot':cursor['slot'],'rawRows':[list(r)for r in rows]})
        cursor=next(c for c in ring if c['state']==state);before=[cursor['physical'],cursor['accepted']]
        if skip<cursor['accepted']:cursor['physical']=cursor['accepted']=0
        rows=cursor['rows'];out=[]
        while cursor['accepted']<skip and cursor['physical']<len(rows):
            row=rows[cursor['physical']];cursor['physical']+=1
            if current[row[1]]!=65535:cursor['accepted']+=1
        while cursor['physical']<len(rows)and len(out)<128:
            target,label,cost,signed,byte7=rows[cursor['physical']];cursor['physical']+=1;code=current[label]
            if code==65535:continue
            flags=(1 if state==0 else 0)|((byte7&1)<<1)
            # Tag namespace byte4 versus tag flags byte5 versus rowflags byte23.
            out.append([(target|(1<<32)|(flags<<40)),source_aux[label],cost,code,0,0,0])
            cursor['accepted']+=1
        calls.append({'state':state,'tag':tag,'skip':skip,'depth':depth,'before':before,'after':[cursor['physical'],cursor['accepted']],'returned':len(out)})
        return out
    def walk(tag,parent,depth,cost):
        assert depth<8 and parent==-1 # Actual two NUL links have missing aux; not auxempty.
        if tag in visited:return
        visited.append(tag);levels[depth]=tag;skip=0
        rows=backend(tag,skip,depth)
        while True:
            for row in rows:
                next_tag,context,c,symbol,length,case_flags,extra=row
                assert length==case_flags==extra==0
                protocol=symbol>=ordinary_count and ((symbol+29)&65535)>=8
                if protocol:
                    assert symbol==65533 and context==-1
                    walk(next_tag,context,depth+1,cost+c)
                else:
                    assert 0<=symbol<ordinary_count and context>=0
                    result=row.copy();result[2]+=cost;output.append(result)
            skip+=len(rows);rows=backend(tag,skip,depth)
            if len(rows)==0:break
        visited.pop()
    walk(8<<40,-1,0,0)
    return {'records':output,'fills':fills,'calls':calls,'cursorFinal':[{k:c[k]for k in['state','slot','physical','accepted']}for c in ring],'sourceNextCursor':len(ring)&7,'outerDepthUsed':len(levels),'backendCalls':len(calls),'sourceV80Fills':len(fills),'sourceRowsCopied':sum(len(r['rawRows'])for r in fills),'visible':len(output),'contextNodesAdded':0}
