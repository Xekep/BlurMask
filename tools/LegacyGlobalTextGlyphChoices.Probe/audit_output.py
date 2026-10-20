"""Independent raw-AK selections/UTF8 classification/interns/original positions.

No predictions or expected words enter recognition or this algorithm oracle.
Recorded coordinates are genuine; absent pen-up metadata is not invented.
"""
import hashlib,importlib.util,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=3:raise SystemExit('MODELS CAPTURE')
models=Path(sys.argv[1]);d=json.loads(Path(sys.argv[2]).read_bytes())
ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('raw_alphabet',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
al=importlib.util.module_from_spec(spec);spec.loader.exec_module(al);alphabet=al.read_alphabet(ak)
data=json.loads((models/'text-unicode/legacy-text-publication-data.json').read_bytes())
assert hashlib.sha256((models/'text-unicode/legacy-text-publication-data.json').read_bytes()).hexdigest()=='6daddbc9ce3d1d3d2e9e179ce008db60887f2c6a13c52ab8bb47dcb215890673'
literal={bytes.fromhex(r[0]):r[1]for r in data['literalRows']}
script=lambda cp:next((v for a,b,v in data['scriptRanges']if a<=cp<=b),0)
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
single=lambda b,o:struct.unpack_from('<f',b,o)[0]
def position(source,x,mode,count):
    integer=int(x);remainder=f(x-f(integer))
    if remainder==0 and integer==count-1:fraction=255 if mode in(3,5)else 0
    else:
        fraction=int(f(f(remainder*200)+f(.5)))
        if fraction==200:integer+=1;fraction=255 if mode in(3,5)and integer==count-1 else 0
    return(source<<32)|(integer<<16)|(fraction<<8)|mode
def same(a,b):return a>>16==b>>16 and(a>>8&255==b>>8&255 or a>>8&255==255 or b>>8&255==255)
glyph_count=alternative_count=source_count=selected_bytes=0
for sample in d['samples']:
    if sample['stage']!='complete':
        assert sample['name'].endswith('.track.json') and sample['refused'];continue
    assert sample['nn']>0
    interns={};reverse={};counts=sample['rawCounts'];owners={};references=[]
    for glyph in sample['glyphs']:
        glyph_count+=1;ref=glyph['reference'];references.append(ref);key=ref&((1<<40)-1|1<<56)
        before=bytes.fromhex(glyph['before']);owner=bytes.fromhex(glyph['owner'])
        assert len(before)%40==len(owner)%40==0 and len(owner)>=len(before)
        #Repeated WORD references retain the same owner, not cloned/reordered rows.
        assert owners.setdefault(key,owner)==owner
        assert owner[:len(before)]==before,'Retained initial owner rows must survive current compact appends'
        rows=[owner[i:i+40]for i in range(0,len(owner),40)]
        identity=lambda r:(u16(r,4)&7,u16(r,8),u16(r,6)&0xff00)
        winners={}
        for n,row in enumerate(rows):
            k=identity(row)
            if k not in winners or single(row,12)>single(rows[winners[k]],12):winners[k]=n
        expected=sorted(winners.values(),key=lambda n:(-single(rows[n],12),n))
        selection=bytes.fromhex(glyph['beforeSelections'])
        assert list(struct.unpack('<'+'i'*(len(selection)//4),selection))==expected
        signed=(ref>>40)&65535;signed=signed-65536 if signed>=32768 else signed
        assert signed>=0 and glyph['beforeMarker']==expected.index(winners[identity(rows[signed])])
        marker=glyph['beforeMarker'];retained=bytearray();emitted=[]
        for offset in range(0,len(selection),4):
            ordinal=u16(selection,offset);row=rows[ordinal];kind=u16(row,4)&7;label=u16(row,8)
            assert kind==0 and label in alphabet,'Actual bounded fixture must use real scalar acoustic labels'
            raw=alphabet[label].encode()
            if not raw:
                if len(emitted)<marker:marker-=1
                continue
            if raw=='\u2639'.encode():raw='\U0001f641'.encode()
            elif raw=='\u263a'.encode():raw='\U0001f642'.encode()
            retained+=selection[offset:offset+4]
            text=raw.decode();assert len(text)==1;cp=ord(text);flags=u16(row,6)&1023;sc=script(cp)
            if sc in(7,8,9)or sc!=10 and 0x3000<=cp<=0x303f:flags|=0x40000
            elif sc==10:flags|=0x80000
            special={13:0x2000,10:0x2000,0x2028:0x2000,0x2029:0x2000,9:0x1000,11:0x1000,12:0x1000,32:0x1000,160:0x1000,0xad:0x4000,0x200c:0x8000}.get(cp,0)
            if cp in[0x61c,0x200e,0x200f,*range(0x202a,0x202f),*range(0x2066,0x206a)]:special=0x20000
            if special:flags|=special
            elif literal.get(raw,-1)>=0:flags|=0x10000
            if cp in(0xf0000,0xf0001):flags|=0x100000
            emitted.append((raw,flags))
        assert retained==bytes.fromhex(glyph['retainedSelections'])and marker==glyph['afterMarker']
        assert len(emitted)==len(glyph['published'])
        for(raw,flags),actual in zip(emitted,glyph['published']):
            assert raw==bytes.fromhex(actual['utf8'])and flags==actual['flags']and len(raw)==actual['count']
            assert interns.setdefault(raw,actual['intern'])==actual['intern']
            assert reverse.setdefault(actual['intern'],raw)==raw
            alternative_count+=1
        selected=None if not(0<=marker<len(emitted))else emitted[marker][0].hex()
        assert glyph['selected']==selected
        selected_bytes+=0 if selected is None else len(bytes.fromhex(selected))
        source=bytes.fromhex(glyph['source24']);residual=bytes.fromhex(glyph['residual2']);assert len(source)%24==0 and len(residual)==len(source)//12
        projected=[]
        for i in range(len(source)//24):
            group,first,point,end_group,last,end_point=struct.unpack_from('<6i',source,i*24)
            assert group==end_group==0 and 0<=first<=last<len(counts)
            for source_index in range(first,last+1):
                count=counts[source_index]
                a=f(f(point)-f(f(residual[i*2])/f(200)))if source_index==first else f(0)
                b=f(f(end_point)-f(f(residual[i*2+1])/f(200)))if source_index==last else f(count-1)
                assert 0<=a<=b<=count-1
                projected.append([position(source_index,a,3,count),position(source_index,b,5,count)])
        original_end=position(len(counts)-1,counts[-1]-1,5,counts[-1]);finished=[]
        for a,b in projected:
            closed=same(b,original_end)or b>>8&255==255 or b>>16&65535==0 and b>>8&255==0 or same(a,b)
            finished.append([a,(b&~255)|(5 if closed else 1)])
        assert finished==glyph['positions'];source_count+=len(finished)
assert len(d['samples'])in(4,13)and d['success']+d['refused']==len(d['samples'])
assert d['success']==len(d['samples'])and d['refused']==0
assert d['guards']==7*d['success']+2
assert d['foreignSameDocumentGuards']==2
assert d['directStringLifetimeGuards']==d['success']
prefix=d['enteredSelectionFailure'];before=bytes.fromhex(prefix['before']);after=bytes.fromhex(prefix['after'])
assert after[:len(before)]==before and len(after)==len(before)+80 and prefix['faulted']and prefix['nn']>0
print(f'PASS {d["success"]} actual sources/{glyph_count} glyphs/{alternative_count} classified UTF8 alternatives/{source_count} exact original packed ranges/{selected_bytes} selected bytes/{d["guards"]} guards; no expected text or CHAR/TEXT acceptance claim.')
