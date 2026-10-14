"""Independent scalar fold/full-packet118 oracle over captured source inputs.

Expected values use the original AK bytes and pinned separation table. Managed
output fields are compared but never used to compute expected states/costs.
"""
import json
import math
import struct
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
IMP = 0x4000000
def f(value): return struct.unpack('<f',struct.pack('<f',value))[0]
def wrap(value): return (value+0x80000000)%0x100000000-0x80000000
def bitround(value):
    bits=struct.unpack('<I',struct.pack('<f',value))[0]; magnitude=bits&0x7fffffff
    if magnitude>=0x4b000000: return value
    if magnitude<0x3f800000: bits=(bits&0x80000000)|(0x3f800000 if magnitude>=0x3f000000 else 0)
    else:
        shift=150-(magnitude>>23); bits=((bits>>shift)+((bits>>(shift-1))&1))<<shift
    return struct.unpack('<f',struct.pack('<I',bits&0xffffffff))[0]
def logcost(value,table,rounded):
    if value>4: v=0
    elif value<-4: v=value
    else: v=table[math.trunc(float(f(value+4))*255+.5)//8]
    cost=f(-v*8192)
    return math.trunc(bitround(cost) if rounded else f(cost-.5 if cost<=0 else cost+.5))

data=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
at=189165
def scalar(offset): return struct.unpack_from('<f',data,at+offset)[0]
assert data[at+8]==0 and math.isnan(scalar(21))
profile=[scalar(p) for p in [29,37,185,190,194]]
matrix=[scalar(41+i*4) for i in range(36)]
join_max=struct.unpack_from('<i',data,at+206)[0]
matrix_max=struct.unpack_from('<i',data,at+210)[0]
original_table=list(map(f,json.loads((ROOT/'models/ru-RU/legacy-separation-profile.json').read_text(encoding='utf-8'))['values']))
captured=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'))
output=[]; all_states=all_calls=0
for fixture in captured:
    assert list(map(f,fixture['profile']))==profile and list(map(f,fixture['matrix']))==matrix
    assert fixture['joinMaximum']==join_max and fixture['matrixMaximum']==matrix_max
    assert list(map(f,fixture['table']))==original_table
    table=original_table
    height=f(fixture['height']); source_gap=f(fixture['sourceGap']); line=fixture['line']
    graph=fixture['graph']; future=[[None]*len(row) for row in graph]
    points=[list(map(f,p)) for p in fixture['points']]
    offsets=[0]
    for count in fixture['pointCounts']: offsets.append(offsets[-1]+count)
    boxes=[]
    for group in fixture['groups']:
        selected=[]
        for stroke,start,end in group: selected.extend(points[offsets[stroke]+start:offsets[stroke]+end])
        boxes.append([min(p[0] for p in selected),min(p[1] for p in selected),max(p[0] for p in selected),max(p[1] for p in selected)])
    for row in graph:
        for item in row: assert list(map(f,item['box']))==boxes[item['group']], ('owned range bbox',item)
    prefix=[[None]*len(row) for row in graph]; join=[[None]*len(row) for row in graph]
    triple=[[None]*len(row) for row in graph]; costs=[[None]*len(row) for row in graph]
    gap0=f(source_gap*profile[3]) if source_gap>=0 else profile[0]
    scale0=1 if source_gap<0 or matrix[0]==0 else f(f(source_gap*profile[4])/matrix[0])
    for node in range(len(graph)-1,-1,-1):
        for arc,item in enumerate(graph[node]):
            value=f(item['box'][0])
            if node!=item['span'] and line!=-1:
                for successor in item['succ']: value=min(value,future[node+1][successor])
            future[node][arc]=value
    expected_states=[]
    for node,row in enumerate(graph):
        expected_row=[]
        for arc,item in enumerate(row):
            box=list(map(f,item['box']))
            if node==item['span'] or line==-1:
                prefix[node][arc]=box[2]; join[node][arc]=triple[node][arc]=IMP; costs[node][arc]=[IMP]*36
            else:
                previous=node-item['span']-1
                high=max((prefix[previous][p] for p in item['pred']),default=box[2])
                prefix[node][arc]=max(high,box[2]); gap=f(f(future[node][arc]-high)/height)
                join[node][arc]=min(logcost(f(f(gap-gap0)*-profile[2]),table,True),join_max)
                triple[node][arc]=min(logcost(f(f(gap-f(gap0*3))*-profile[2]),table,True),join_max)
                entries=[]
                for i,value in enumerate(matrix):
                    c=logcost(f(f(gap-f(scale0*value))*profile[2]),table,False)
                    entries.append(c-1 if i//6==5 else c if i%6==5 else min(c,matrix_max))
                costs[node][arc]=entries
            state=[future[node][arc],prefix[node][arc],join[node][arc],triple[node][arc]]
            assert state==item['state'], ('prepared state',node,arc,state,item['state'])
            expected_row.append(state); all_states+=1
        expected_states.append(expected_row)
    expected_calls=[]
    for call in fixture['calls']:
        node,arc,previous,prior_arc,kind,source_kind,first,prior,before,acc,observed,observed_acc,observed_ok=call
        if kind==2 and not graph[node][arc]['wholeStarts']:
            after=after_acc=IMP; accepted=False
        else:
            correction=(triple if source_kind in (7,8) else join)[node][arc] if kind==0 else costs[node][arc][first+prior*6]
            a=list(map(f,graph[node][arc]['box'])); b=list(map(f,graph[previous][prior_arc]['box']))
            denominator=100 if line==-1 else height
            movement=f(max(f(a[0]-b[0]),f(a[2]-b[2]))/denominator)
            term=f(f(movement-profile[1])*profile[2]) if movement<=profile[1] else 0
            after=wrap(before-math.trunc(term)*8192+correction); accepted=after<IMP
            after_acc=wrap(acc+after-before) if accepted else IMP
        assert [after,after_acc,int(accepted)]==[observed,observed_acc,observed_ok], ('callback',call,after,after_acc,accepted)
        expected_calls.append([after,after_acc,accepted]); all_calls+=1
    output.append(dict(states=expected_states,calls=expected_calls))
Path(sys.argv[2]).write_text(json.dumps(output,separators=(',',':')),encoding='utf-8')
print(f'PASS independent original main297 scalar reader / {all_states} native cache states / {all_calls} full80 callbacks')
