"""Raw AK/LK final compact bytes then independent postpublication2257C0.

The shared Python oracle is a resource/list implementation, not production C#.
Only its old selected-terminal assertion and one-shot caller extent guard are omitted: the new selector
below accepts an explicit no-winner and reads the actual final rows.
"""
import ast
import base64
import json
import struct
import sys
import tempfile
from pathlib import Path
if len(sys.argv) != 6:
    raise SystemExit('AK LK CASE PUBLICATION capture.json')
root = Path(__file__).resolve().parents[2]
capture = Path(sys.argv[5]).resolve()
rows = json.loads(capture.read_bytes())
assert [c['nodeCount'] for c in rows] == [124,140,240,4,4]
assert [c['startingNode2490'] for c in rows] == [120,120,120,3,0]
common = root / 'tools/LegacyMainCompactMaterialization.Probe/audit_output.py'
tree = ast.parse(common.read_text(encoding='utf-8'))
removed = []
caller_guards = []
keep = []
for node in tree.body:
    if isinstance(node, ast.If) and ast.unparse(node.test) == "c.get('publicationKind') == 'live'":
        removed.append(node)
    elif isinstance(node, ast.If) and ast.unparse(node.test) == 'case_mode' and 'intermediatePublicationDistance' in ast.unparse(node):
        caller_guards.append(node)
    else:
        keep.append(node)
assert len(removed) == len(caller_guards) == 1
numeric = compile(ast.Module(body=keep,type_ignores=[]), str(common), 'exec')
#Read the selected2D1 profile/codebook ownership independently from raw LK.
lk=Path(sys.argv[2]).read_bytes();d=lk[2513123:4565174];position=90
def take(fmt):
    global position
    values=struct.unpack_from('<'+fmt,d,position);position+=struct.calcsize('<'+fmt)
    return values[0] if len(values)==1 else values
order,flags,words,classes,uc,bc=take('6I');assert (order,flags,uc)==(2,3,classes)
packed,n=take('2I');width=take('B');assert packed==0x1000000 and n==words
position+=((words*width+31)//32)*4+words
word_levels=take('i'*take('I'));position+=classes+classes*classes
nu,nb=take('2I');uni=take('i'*nu);bi=take('i'*nb);assert take('2I')==(0,0)
def f32(v): return struct.unpack('<f',struct.pack('<f',v))[0]
slope=struct.unpack_from('<f',d,position+80)[0];offset=struct.unpack_from('<f',d,position+16)[0]
scale=struct.unpack_from('<f',lk,607064)[0]
penalty=int(f32(f32(f32(f32(uni[-1])*f32(1/8192))*f32(scale*slope))-f32(scale*offset))*8192)
assert penalty==17153
def i32(b,p): return struct.unpack_from('<i',b,p)[0]
def u16(b,p): return struct.unpack_from('<H',b,p)[0]
def wrap(v): return (v+2**31)%2**32-2**31
with tempfile.TemporaryDirectory(prefix='global-final-',dir=capture.parent) as temporary:
    for index,c in enumerate(rows):
        path=Path(temporary)/f'{index}.json'
        path.write_text(json.dumps(c),encoding='utf-8')
        previous=sys.argv
        try:
            sys.argv=[str(common),previous[1],previous[2],str(path),'--global-publication',previous[3],previous[4]]
            exec(numeric,{'__file__':str(common),'__name__':'__main__'})
        finally:
            sys.argv=previous
        count=c['nodeCount'];start=c['startingNode2490']
        assert c['intermediatePublicationDistance']==120 and c['boundedLogicalNodeLimit']==240
        assert 0<len(c['inputNodes'])==count<=240
        assert c['nextNode2490']==count
        assert c['prefixOwners2488']==[-1]*start+[start-1]*(count-start)+[count-1]
        assert c['publicationCount']==(2 if start else 1)
        assert c['penalty']==penalty
        raw=base64.b64decode(c['nodes'][-1]['rows'])
        assert c['sourceEnd']<=len(raw)//88
        chosen=-1;best=67108864
        for ordinal in range(c['sourceEnd']):
            row=raw[ordinal*88:(ordinal+1)*88]
            cost=i32(row,16)
            if cost>=67108864: continue
            assert row[4]<2
            #Actual freshFilter4=false, noF0009, noRequire6, inactive rescue.
            if ordinal<c['primary'] or row[5]&4: continue
            if cost<best: best=cost;chosen=ordinal
        if chosen<0:
            expected=[False,-1,67108864,67108864,67108864,4294967295,0,0]
            assert c['selected'] is None and base64.b64decode(c['selected88'])==b''
        else:
            row=raw[chosen*88:(chosen+1)*88]
            expected=[True,chosen,best,wrap(best+c['cumulative']),i32(row,20),u16(row,34),0,0]
            assert c['selected']==c['starts'][-2]+u16(row,34)
            assert c['selected']<c['count'] and base64.b64decode(c['selected88'])==row
        assert c['winner']==expected,(index,c['winner'],expected)
        if index<3: assert c['count']==0 and chosen==-1
        else: assert chosen>=0 and c['count']==1057 and c['selected']==1019
print('PASS five final-pool BEFORE scan routes: actual124/140/240 noWinner, separate prepared4 carry and natural short4; all compact/beam bytes and nullable selection independently checked.')
