"""Independent B8 predecessor/word-boundary and scalar/275 publication audit."""
import base64,json,pathlib,struct,sys
root=json.loads(pathlib.Path(sys.argv[1]).read_bytes())
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<I',struct.pack('<f',v))[0]
def signed(v,width):return (v^(1<<(width-1)))-(1<<(width-1))
def integer(b,at):return struct.unpack_from('<i',b,at)[0]
def half(b,at):return struct.unpack_from('<H',b,at)[0]
real=delimiters=multi=0
for sample in root['samples']:
    b8=[base64.b64decode(x) for x in sample['b8']];columns=sample['columns'];beam=sample['beam'];assert columns==1
    assert len(b8)==sample['nodes'] and all(len(b)==40*columns*beam for b in b8)
    node=len(b8)-1;row=sample['terminalRow'];col=sample['terminalColumn'];reverse=[]
    while node>=0:
        record=b8[node][40*(row*columns+col):40*(row*columns+col+1)]
        symbol=half(record,12);packed=half(record,14);arc=half(record,16);prev=node-1-((packed>>1)&7)
        reverse.append(dict(node=node,arc=arc,symbol=symbol,boundary=bool(packed&1),record=record,prev=prev))
        node=prev;row=record[9];col=record[10]
    trace=reverse[::-1]
    assert trace and trace[0]['prev']==-1
    assert integer(trace[-1]['record'],0)==sample['cost'] and integer(trace[-1]['record'],4)==sample['acoustic']
    visible=lambda c:c['symbol'] not in (191,192)
    boundaries=[i for i,c in enumerate(trace) if visible(c) and (i==0 or c['boundary'])]
    expected=[]
    for ordinal,start in enumerate(boundaries):
        end=boundaries[ordinal+1]-1 if ordinal+1<len(boundaries) else len(trace)-1
        prefix=trace[start-1] if start else None;terminal=trace[end]
        cost=signed((integer(terminal['record'],0)-(integer(prefix['record'],0) if prefix else 0))&0xffffffff,32)
        acoustic=signed((integer(terminal['record'],4)-(integer(prefix['record'],4) if prefix else 0))&0xffffffff,32)
        span=terminal['node']-(prefix['node'] if prefix else -1)
        selected=[c for c in trace[start:end+1] if visible(c)]
        expected.append((cost,acoustic,span,selected,terminal))
    actual=[w for w in sample['words'] if not w['delimiter']]
    assert len(actual)==len(expected)
    detached={g['reference'] for g in sample['detached']}
    for word,(cost,acoustic,span,cells,last) in zip(actual,expected):
        assert (word['delta'],word['acousticDelta'],word['span'])==(cost,acoustic,span)
        storage=base64.b64decode(word['storage'])
        want=[f(f(f(-5)*f(cost))*f(.00012207031)),1.,f(f(cost)/f(span*8192)),f(f(acoustic)*f(.00012207031))]
        assert list(struct.unpack_from('<4I',storage,16))==[bits(x) for x in want]
        assert len(word['glyphs'])==len(cells)==integer(storage,40)
        for glyph,cell in zip(word['glyphs'],cells):
            reference=int(glyph['reference'],16);record=base64.b64decode(glyph['record'])
            assert signed(reference&0xffffff,24)==cell['node'] and signed((reference>>24)&65535,16)==cell['arc']
            assert half(record,8)==cell['symbol'] and half(record,4)&7==0
            assert glyph['reference'] not in detached
        first=cells[0];assert word['first']==sample['arcs'][first['node']][first['arc']][0]
        assert word['last']==sample['arcs'][last['node']][last['arc']][1]
        real+=1
    for word in sample['words']:
        if not word['delimiter']:continue
        delimiters+=1;assert len(word['glyphs'])==1
        storage=base64.b64decode(word['storage']);assert struct.unpack_from('<4f',storage,16)==(1,1,0,0)
        glyph=word['glyphs'][0];reference=int(glyph['reference'],16);record=base64.b64decode(glyph['record'])
        assert signed((reference>>24)&65535,16)<0 and half(record,4)&7==1
        assert half(record,8)==6 #actual13AF30 count1/FFFB macro, not a literal guessed space
        assert glyph['reference'] not in detached
    for glyph in sample['detached']:
        record=base64.b64decode(glyph['record']);assert half(record,4)&7==1 and half(record,8)==1
    if len(actual)>1:multi+=1
assert (real,delimiters,multi,root['guards'])==(root['realWords'],root['delimiterWords'],root['multipleInputs'],10)==(10,3,3,10)
print('PASS independent actual B8 ->10 word boundaries/cost deltas/scalar bits/source spans;3 actual protocol macro words;7 inputs; no text accuracy assertion')
