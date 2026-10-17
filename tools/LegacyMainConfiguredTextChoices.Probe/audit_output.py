"""Independent actual glyph-choice emission/classification/intern ownership.

Reads the raw AK alphabet and committed Unicode data. No expected word labels
or model predictions enter this audit or the inference that produced capture.
"""
import hashlib
import importlib.util
import json
import struct
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[2]
ak = (root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest() == '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec = importlib.util.spec_from_file_location('raw_alphabet', root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
alphabet = module.read_alphabet(ak)
data = json.loads((root/'models/text-unicode/legacy-text-publication-data.json').read_bytes())
literal = {bytes.fromhex(r[0]): r[1] for r in data['literalRows']}
script = lambda cp: next((v for start,end,v in data['scriptRanges'] if start <= cp <= end), 0)
d = json.loads(Path(sys.argv[1]).read_bytes())
u16 = lambda b,o: struct.unpack_from('<H',b,o)[0]
f = lambda x: struct.unpack('<f',struct.pack('<f',x))[0]
def position(source, coordinate, mode, count):
    integer = int(coordinate); remainder = f(coordinate-f(integer))
    if remainder == 0 and integer == count-1: fraction = 255 if mode in (3,5) else 0
    else:
        fraction = int(f(f(remainder*200)+f(.5)))
        if fraction == 200: integer += 1; fraction = 255 if mode in (3,5) and integer == count-1 else 0
    return (source<<32)|(integer<<16)|(fraction<<8)|mode
def same(a,b):
    return a>>16 == b>>16 and ((a>>8&255)==(b>>8&255) or (a>>8&255)==255 or (b>>8&255)==255)
def finish_normal(ranges, original_end):
    result=[]
    for a,b in ranges:
        fraction=b>>8&255; point=b>>16&65535
        closed=same(b,original_end) or fraction==255 or point==0 and fraction==0 or same(a,b)
        result.append([a,(b&~255)|(5 if closed else 1)])
    return result

def span_projection(refs, rows):
    result=[]
    def add(row,coordinate):
        source,count,first,last,ff,lf=row
        value=position(source,coordinate,4,count);result.append([value,value])
    if len(refs)==1:
        first,point,last,end,fr,lr=refs[0]; row=rows[first]
        add(row,f(f(row[1]-1 if row[3]==-1 else row[3])+f(row[5])))
        if first!=last: row=rows[last];add(row,f(f(row[2])+f(row[4])))
    else:
        for i,(first,point,last,end,fr,lr) in enumerate(refs):
            row=rows[first]; source,count,start,stop,ff,lf=row
            stop=count-1 if stop==-1 else stop
            relative=f(f(point)-f(f(fr)/f(200)))
            length=f((1 if lf>0 else 0)-start+stop)
            absolute=f(f(stop)+f(lf)) if i==0 and relative==length else f(f(start)+f(ff if relative==0 else relative))
            add(row,absolute)
    return result

glyphs = alternatives = total_bytes = 0
assert len(d['inputs']) == 6
for sample in d['inputs']:
    strings = {}; reverse = {}
    for word in sample['words']:
        selected_word = bytearray()
        for glyph in word['glyphs']:
            glyphs += 1
            owner = bytes.fromhex(glyph['owner']); rows = bytes.fromhex(glyph['beforeSelections'])
            pool = [owner[n:n+40] for n in range(0,len(owner),40)]
            assert all(len(row)==40 for row in pool)
            #Independent finite score winner-by-identity formulation. Equal
            #scores retain the first insertion, then distinct symbols are
            #ranked by score and winning insertion ordinal.
            key=lambda r:(u16(r,4)&7,u16(r,8),u16(r,6)&0xff00)
            score=lambda r:struct.unpack_from('<f',r,12)[0]
            winners={}
            for n,row in enumerate(pool):
                assert score(row)==score(row)
                identity=key(row)
                if identity not in winners or score(row)>score(pool[winners[identity]]):
                    winners[identity]=n
            expected_selection=sorted(winners.values(),key=lambda n:(-score(pool[n]),n))
            assert list(struct.unpack('<'+'i'*(len(rows)//4),rows))==expected_selection
            input_index=glyph['reference']>>40&65535
            assert glyph['beforeMarker']==expected_selection.index(winners[key(pool[input_index])])
            retained=bytearray();emitted=[];marker=glyph['beforeMarker']
            for offset in range(0,len(rows),4):
                row=pool[u16(rows,offset)];kind=u16(row,4)&7;label=u16(row,8)
                assert kind==0,'Actual configured WORD fixture has ordinary labels; other kinds remain separately audited'
                raw=alphabet[label].encode()
                if not raw:
                    if len(emitted)<marker:marker-=1
                    continue
                if raw=='\u2639'.encode():raw='\U0001f641'.encode()
                elif raw=='\u263a'.encode():raw='\U0001f642'.encode()
                retained+=rows[offset:offset+4]
                text=raw.decode();assert len(text)==1
                cp=ord(text);flags=u16(row,6)&1023;sc=script(cp)
                if sc in (7,8,9) or sc!=10 and 0x3000<=cp<=0x303f:flags|=0x40000
                elif sc==10:flags|=0x80000
                special={13:0x2000,10:0x2000,0x2028:0x2000,0x2029:0x2000,
                         9:0x1000,11:0x1000,12:0x1000,32:0x1000,160:0x1000,
                         0xad:0x4000,0x200c:0x8000}.get(cp,0)
                if cp in [0x61c,0x200e,0x200f,*range(0x202a,0x202f),*range(0x2066,0x206a)]:special=0x20000
                if special:flags|=special
                elif literal.get(raw,-1)>=0:flags|=0x10000
                if cp in (0xf0000,0xf0001):flags|=0x100000
                emitted.append((raw,flags))
            assert retained==bytes.fromhex(glyph['retainedSelections']) and marker==glyph['afterMarker']
            assert len(emitted)==len(glyph['published'])
            for (raw,flags),published in zip(emitted,glyph['published']):
                assert raw==bytes.fromhex(published['utf8']) and flags==published['flags']
                assert published['count']==len(raw)
                identity=published['intern']
                assert strings.setdefault(raw,identity)==identity and reverse.setdefault(identity,raw)==raw
                alternatives+=1
            selected=emitted[marker][0] if 0<=marker<len(emitted) else None
            assert (None if glyph['selected']is None else bytes.fromhex(glyph['selected']))==selected
            if selected is not None:selected_word+=selected;total_bytes+=len(selected)
            raw=bytes.fromhex(glyph['source24']);refs=[]
            residual=bytes.fromhex(glyph.get('residual2',''))
            assert glyph['ordinary'] and len(raw)%24==0 and len(residual)==len(raw)//12
            projected=[]
            for i in range(len(raw)//24):
                group,first,point,end_group,last,end_point=struct.unpack_from('<6i',raw,i*24)
                assert group==end_group==0
                for source in range(first,last+1):
                    count=sample['rawCounts'][source]
                    start=f(f(point)-f(f(residual[i*2])/f(200))) if source==first else f(0)
                    end=f(f(end_point)-f(f(residual[i*2+1])/f(200))) if source==last else f(count-1)
                    projected.append([position(source,start,3,count),position(source,end,5,count)])
            original_end=position(len(sample['rawCounts'])-1,sample['rawCounts'][-1]-1,5,sample['rawCounts'][-1])
            assert glyph['positions']==finish_normal(projected,original_end)
        assert selected_word==bytes.fromhex(word['selected'])
        #The captured raw prewrapper bytes are diagnostic, not a recognition
        #target, and are deliberately not used to select a winning row.
assert (glyphs,alternatives,total_bytes,d['guards'])==(18,49,35,42)
assert (glyphs,alternatives,total_bytes)==tuple(d[k]for k in ['glyphs','records','selectedBytes'])
print('PASS independent18 actual configured glyphs/49 finite-score selections/rawAK UTF8 classifications/localintern ownership/35 selected bytes/originalraw positions/42 guards; no target strings or finalacceptance.')
