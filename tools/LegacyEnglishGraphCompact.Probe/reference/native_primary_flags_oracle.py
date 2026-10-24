"""Pure native2093A8..209512 branch model. No owner, NN or writing on import."""
import struct,base64,json,sys

def choose(rows,current,group_start,selected,quota,headers,probe):
    # Scalar p3/p4 represent native56-byte candidate+3/+4; p10 increments
    # only a new context admitted under primary-alternative quota2402.
    row=rows[current];calls=0
    def query():
        nonlocal calls
        calls+=1;return bool(probe())
    b3 = query() if selected < quota and row['Fallback'] else False
    header=headers[(row['Tag']>>32)&255]
    b4=False
    if selected < quota:
        if not query():
            k=header&0x1e00
            b4=not(0x200<=k<=0xa00 or ((k-0xe00)&0xfffffdff)==0)
    new_context=b3
    if b3 or b4:
        for i in range(current-1,group_start-1,-1):
            old=rows[i]
            if old['Bit3'] and old['Context']==row['Context']:
                new_context=False
                if old['Auxiliary']==row['Auxiliary']:b3=False
            if old['Bit4'] and old['Context']==row['Context']:b4=False
    return dict(TerminalBit3=bool(b3),TerminalBit4=bool(b4),SelectedDistinctAfter=selected+int(new_context),ProbeCalls=calls)

def full40(tag,b3,b4):
    # Direct206D30 one-symbol nontracing layout. Preserve allocator22/23.
    x=bytearray([0xa7]*40)
    struct.pack_into('<QQiiiHH',x,0,tag,0,-1,100+8*int(b4),7,1,65535)
    x[32]=8*int(b3)+16*int(b4);x[33]=255
    struct.pack_into('<i',x,36,-1)
    return bytes(x)

def audit(inputs,output):
    assert output['NNCalls']==0 and len(inputs)==5760
    rows=output['Reports'];assert len(rows)==len(inputs) and [r['Id'] for r in rows]==list(range(len(inputs)))
    for x,r in zip(inputs,rows):
        cursor=iter(x['Replies'])
        e=choose(x['Rows'],len(x['Rows'])-1,x['GroupStart'],x['Selected'],x['Quota'],x['Headers'],lambda:next(cursor))
        for k,v in e.items():assert r[k]==v,(x['Id'],k,r[k],v)
        assert r['ActualProbeCalls']==e['ProbeCalls']
        assert base64.b64decode(r['Final40'],validate=True)==full40(x['Rows'][-1]['Tag'],e['TerminalBit3'],e['TerminalBit4'])
    return dict(cases=len(rows),full40=len(rows),bit3=sum(r['TerminalBit3'] for r in rows),bit4=sum(r['TerminalBit4'] for r in rows))

if __name__=='__main__':
    from copy import deepcopy
    x=json.loads(open(sys.argv[1],encoding='utf-8-sig').read());r=json.loads(open(sys.argv[2],encoding='utf-8-sig').read());report=audit(x,r)
    mutations=[]
    for field in ['TerminalBit3','TerminalBit4','SelectedDistinctAfter','ProbeCalls','ActualProbeCalls','Final40','Id']:
        y=deepcopy(r)
        if field=='Final40':a=bytearray(base64.b64decode(y['Reports'][0][field]));a[34]^=1;y['Reports'][0][field]=base64.b64encode(a).decode()
        elif field.startswith('Terminal'):y['Reports'][0][field]=not y['Reports'][0][field]
        else:y['Reports'][0][field]+=1
        try:audit(x,y)
        except AssertionError:mutations.append(field)
        else:raise AssertionError('tamper accepted: '+field)
    for mode in ['empty','drop','duplicate']:
        y=deepcopy(r);y['Reports']=[] if mode=='empty' else y['Reports'][:-1] if mode=='drop' else y['Reports']+[y['Reports'][-1]]
        try:audit(x,y)
        except AssertionError:mutations.append(mode)
        else:raise AssertionError('denominator tamper accepted')
    report.update(scope='PREPARED numeric native flags/full40 only; no actual eligible owner evidence',negativeControls=len(mutations),NNCalls=0)
    print(json.dumps(report,indent=2))
