"""Independent217DB0 control flow over explicitly prepared source services."""
import json,struct,sys
from pathlib import Path
p=json.loads(Path(sys.argv[1]).read_bytes());assert p['Guards']==3
merges=failures=corrections=0
for case in p['Cases']:
    rows=bytearray.fromhex(case['Input']);residual=list(case['References'])
    count=len(residual);row_count=len(rows)//72;calls=[];reads=0;success=True;remaining=count
    def fact(node):
        global reads
        reads+=1;return case['Facts'][node]
    def source(at):return struct.unpack_from('<i',rows,at)[0]
    def i32(at):return struct.unpack_from('<i',rows,at)[0]
    def label(at):return struct.unpack_from('<H',rows,at)[0]
    def set32(at,value):struct.pack_into('<i',rows,at,value)
    if count and not case['Bypass']:
        for current in range(count):
            ref=residual[current];node=ref&0xffffff
            if node&0x800000:node-=0x1000000
            kind=fact(node)['Kind'];selected=None
            best=float.fromhex('0x1.fffffep+127') if kind==0 else 2147483647
            if kind not in (0,1):success=False;break
            box=fact(node) if kind==0 else None
            for row_index in range(row_count):
                off=row_index*72
                if rows[off+60]:continue
                candidate=source(off)
                if fact(candidate)['Kind']!=kind:continue
                if kind==1:
                    distance=abs(node-candidate-1)
                else:
                    line=fact(candidate)['Line']
                    if line!=fact(node)['Line']:continue
                    cbox=fact(candidate)
                    if case['Ordered']:
                        distance=node-candidate if candidate<node else float.fromhex('0x1.fffffep+127')
                    else:
                        distance=((cbox['X0']+cbox['X1'])-(box['X0']+box['X1']))*.5
                        corrected=False
                        if line!=-1:
                            sym=label(off+58);baseline=2.+line;height=3.+line
                            if sym<9 and sym%2==0 and fact(node)['Top'] and distance<0 and (box['Y0']+box['Y1'])*.5<baseline-height*.75:
                                distance=abs(distance)-2*height;corrected=True;corrections+=1
                        if not corrected:distance=abs(distance)
                if distance<best:best=distance;selected=row_index
            if selected is not None:
                insertion=i32(selected*72+64)
                for j in range(current-1,insertion-1,-1):residual[j+1]=residual[j]
                residual[insertion]=ref
                for j in range(selected,row_count):set32(j*72+64,i32(j*72+64)+1)
        else:
            for row_index in reversed(range(row_count)):
                off=row_index*72;start=i32(off-8) if row_index else 0
                difference=i32(off+64)-start
                if not difference:continue
                key=struct.unpack_from('<Q',rows,off+48)[0]
                if count==len(residual):residual.append(key)
                else:residual[count]=key
                count+=1;length=difference+1;calls.append(residual[start:start+length]);merges+=1
                if len(calls)==case['MergeFailure']:success=False;failures+=1;break
                struct.pack_into('<Q',rows,off+48,0x5a000000+len(calls));rows[off+68]=1
                if start<count and length:
                    if length<count-start:
                        residual[start:count-length]=residual[start+length:count];count-=length
                    else:count=start
            remaining=0
    assert case['Succeeded']==success,(case['Id'],'success')
    assert case['Remaining']==remaining,(case['Id'],'remaining')
    assert case['Rows']==rows.hex().upper(),(case['Id'],'rows')
    assert case['Storage']==residual,(case['Id'],'retained vector storage')
    assert case['MergeCalls']==calls,(case['Id'],'merge requests')
    assert case['Reads']==reads,(case['Id'],'source callback order/count')
assert len(p['Cases'])==768 and merges>100 and failures>10 and corrections>0
print(f'PASS independent768 prepared217DB0 cases/{merges} merge calls/{failures} retained-prefix failures/{corrections} top-mark corrections; all row/vector bytes and callback counts; source adapter separate')
