"""Independent actual WORD275/pool40 -> raw UTF8 and source-key projection."""
import base64,hashlib,importlib.util,json,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
if len(sys.argv)!=3:raise SystemExit('AK.res capture.json')
ak=Path(sys.argv[1]).read_bytes();capture=json.loads(Path(sys.argv[2]).read_bytes())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('raw_alphabet',ROOT/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);alphabet=m.read_alphabet(ak)
count=struct.unpack_from('<H',ak,113)[0]
def u16(b,at):return struct.unpack_from('<H',b,at)[0]
def i32(b,at):return struct.unpack_from('<i',b,at)[0]
def u64(b,at):return struct.unpack_from('<Q',b,at)[0]
def canonical(ref):return (ref&0xffffff)|(ref&0xffff000000)|(ref&(1<<56))
def emit(record):
    kind=u16(record,4)&7;label=u16(record,8)
    repetitions=(label>>2)&8191 if kind in (1,3) else 1
    if repetitions==0:return b''
    assert kind not in (3,4),'actual control/alternate owner is not inferred'
    if kind==1:label=((label&3)-7)&65535
    elif kind not in (0,2):label=65535
    if label<count:part=alphabet[label].encode('utf8')
    else:
        special={65532:'\xa0',65531:' ',65529:'\n',65526:chr(0xf0000),65523:chr(0xf0010),65522:chr(0xf0011),65521:chr(0xf0012),65520:chr(0xf0013),65519:chr(0xf0014),65518:chr(0xf0015),65517:chr(0xf0016),65516:chr(0xf0017),65515:chr(0xf0018)}
        part=special.get(label,'\ufffd').encode('utf8')
    assert chr(0xf0000).encode() not in part
    return part*repetitions
words=empty=glyphs=0
assert len(capture['controls'])==len(capture['readingOrderUtf8'])==32
for control_index,control in enumerate(capture['controls']):
    produced=[]
    for callback in control:
        full=base64.b64decode(callback['utf8']);assert callback['text']==full.decode('utf8')
        if callback['word120'] is None:
            empty+=1;assert full==b'' and callback['glyphs']==[];continue
        words+=1;word=base64.b64decode(callback['word120']);refs=callback['references'];pool=callback['pool']
        assert len(word)==120 and i32(word,40)==len(refs) and u64(word,8)==refs[-1]
        assert [entry['reference'] for entry in pool]==refs==[g['reference'] for g in callback['glyphs']]
        associated=base64.b64decode(callback['associatedRows']);combined=[]
        for ref,entry,glyph in zip(refs,pool,callback['glyphs']):
            record=base64.b64decode(entry['record']);key=canonical(ref);matches=[]
            for i in range(len(associated)//72):
                row=associated[i*72:(i+1)*72];node=i32(row,0)
                if u64(row,48)==key and i32(row,8)-capture['starts'][node]==i32(record,0):matches.append(i)
            assert matches and glyph['rows']==matches and glyph['key']==key
            assert base64.b64decode(glyph['record'])==record and len(record)==40
            assert glyph['kindFlags']==u16(record,4) and glyph['symbol']==u16(record,8)
            protocol={associated[i*72+60]==3 for i in matches};assert len(protocol)==1
            assert glyph['protocol']==next(iter(protocol))
            if glyph['protocol']:
                assert glyph['original'] is None and glyph['ranges']==[]
            else:
                # Current genuine selected paths have zero residual events;
                # no higher source is invented to make this check pass.
                base_keys=set()
                for i in matches:
                    row=associated[i*72:(i+1)*72];node=i32(row,0);arc=u16(row,4);base_keys.add(node|(arc<<24))
                assert base_keys=={key} and glyph['original']==key
                node=key&0xffffff;arc=(key>>24)&65535;group=capture['graph'][node][arc][0]
                assert glyph['ranges']==capture['groupRanges'][group]
            expected=emit(record);assert base64.b64decode(glyph['utf8'])==expected
            combined.append(expected);glyphs+=1
        assert full==b''.join(combined);produced.append(full)
    #279 prepends the words encountered on the reverse backtrack. This is
    #actual published-node order, not sorting/guessing from expected labels.
    assert base64.b64decode(capture['readingOrderUtf8'][control_index])==b''.join(reversed(produced))
assert len(set(capture['readingOrderUtf8']))==1
assert capture['words']==words and capture['emptyCallbacks']==empty
assert capture['guards']==2*(words+empty)
print(f'PASS independent rawAK/pool40 ->{words} actual word UTF8/{glyphs} glyph/source-key maps/{empty} zero-span callbacks, exact reading order; no expected transcription.')
