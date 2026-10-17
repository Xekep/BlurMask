"""Actual glyph-pool inputs -> independent021760 complete scalar-row oracle."""
import argparse
import base64
import importlib.util
import json
from pathlib import Path
import struct

root=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('raw_alphabet',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
alphabet_oracle=importlib.util.module_from_spec(spec);spec.loader.exec_module(alphabet_oracle)
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('acoustic',type=Path);p.add_argument('publication',type=Path);p.add_argument('capture',type=Path)
a=p.parse_args();alphabet=alphabet_oracle.read_alphabet(a.acoustic.read_bytes());data=json.loads(a.publication.read_bytes())
literal={bytes.fromhex(r[0]):r[1] for r in data['literalRows']}
special={65535:'\ufffd',65532:'\u00a0',65531:' ',65529:'\n'}
records=[];intern={};aggregate=0;calls=0;glyphs=0

def emit(row):
    flags,symbol=struct.unpack_from('<H',row,4)[0],struct.unpack_from('<H',row,8)[0];kind=flags&7
    if kind==0:return alphabet[symbol].encode()
    assert kind==1
    return special.get(((symbol&3)-7)&65535,'\ufffd').encode()*((symbol>>2)&8191)

def publish(raw,glyphflags):
    f=glyphflags&1023
    if not raw:return f
    cp=ord(raw.decode()[0]);script=next((z for x,y,z in data['scriptRanges']if x<=cp<=y),0)
    if script in [7,8,9] or script!=10 and 0x3000<=cp<=0x303f:f|=0x40000
    elif script==10:f|=0x80000
    bit={10:0x2000,32:0x1000,160:0x1000,0x200c:0x8000}.get(cp,0)
    if bit:f|=bit
    elif literal.get(raw,-1)>=0:f|=0x10000
    if raw in ['\U000f0000'.encode(),'\U000f0001'.encode()]:f|=0x100000
    return f

for source in json.loads(a.capture.read_bytes())['sources']:
    raw=base64.b64decode(source['actualGlyph40']);selection=base64.b64decode(source['selection4'])
    assert len(raw)%40==len(selection)%4==0
    inputrows=[raw[i:i+40]for i in range(0,len(raw),40)];glyphs+=len(inputrows)
    for result in source['calls']:
        original=len(records);marker=-1;appended=0
        for i in range(len(selection)//4):
            ordinal=struct.unpack_from('<H',selection,i*4)[0];row=inputrows[ordinal];text=emit(row)
            newly_interned=text not in intern
            if newly_interned:intern[text]=len(intern)+1
            identity=intern[text]
            match=None if newly_interned else next((j for j in range(original)if records[j][1]==identity),None)
            if match is None:
                match=len(records);f=publish(text,struct.unpack_from('<H',row,6)[0]);scalar=bytearray(40)
                struct.pack_into('<I',scalar,8,f);struct.pack_into('<I',scalar,32,len(text))
                records.append((scalar,identity,text));appended+=1;aggregate|=f
            if i==len(selection)//4-1:marker=match
        assert(result['original'],result['marker'],result['appended'],result['aggregate'])==(original,marker,appended,aggregate)
        assert len(result['records'])==len(records)
        for actual,(scalar,identity,text) in zip(result['records'],records):
            assert base64.b64decode(actual['scalar40'])==scalar
            assert actual['identity']==identity and base64.b64decode(actual['utf8'])==text
        calls+=1
print(f'PASS {glyphs} genuine glyph records/{calls} actual-owner calls, complete40-byte storage/identity/marker/aggregate oracle; initial source-node records remain explicitly prepared.')
