"""Independent pinned first alphabet mask reconstruction and owning output check."""
import json,pathlib,struct,sys
resource=pathlib.Path(sys.argv[1]).read_bytes();captures=json.loads(pathlib.Path(sys.argv[2]).read_text(encoding='utf-8'))
p=6219;header=struct.unpack_from('<8H',resource,p);p+=16;count=header[-1];assert count==192
n=struct.unpack_from('<H',resource,p)[0];p+=2
for increment in (32,64,1):
    table=struct.unpack_from('<'+'H'*n,resource,p);p+=2*n;n=[v for v in table if v!=65535][-1]+increment
spans=struct.unpack_from('<'+'H'*(n+1),resource,p);p+=2*(n+1)
slots=[struct.unpack_from('<II',resource,p+i*8) for i in range(spans[-1]+1)];p+=8*(spans[-1]+1)
strings=resource[p:p+slots[-1][0]];p+=slots[-1][0]
texts={label:strings[offset:slots[i+1][0]].decode('utf-8') for i,(offset,label) in enumerate(slots[:-1])};labels={v:k for k,v in texts.items()}
reverse=struct.unpack_from('<'+'H'*count,resource,p)
virtual={i for i,index in enumerate(reverse) if index==65535};assert virtual=={140,178,179}
enabled=set(range(count))-virtual
dash=['-','_','\U000f0002','\U000f0003','\u30fc','\u4e00']
if any(labels.get(v,-1) in enabled for v in dash):enabled.add(header[1])
if header[2]!=65535 and labels.get('\U000f0001',-1) in enabled:enabled.add(header[2])
enabled.update(v for i,v in enumerate(header[:7]) if i in (3,0,4,5,6) and v!=65535)
mask=bytes(int(i in enabled) for i in range(count));assert mask==bytes([1])*192
#An empty actual wide pair list follows from the exact alphabet texts. We do
#not silently use an unrestricted-pair policy when real wide symbols exist.
def wide(v):
    if 0xff01<=v<=0xff5e:return v-0xfee0
    return {0x3000:0x20,0xff5f:0x2985,0xff60:0x2986,0xffe0:0xa2,0xffe1:0xa3,0xffe2:0xac,0xffe3:0xaf,0xffe4:0xa6,0xffe5:0xa5,0xffe6:0x20a9}.get(v)
assert not [(label,labels.get(chr(base))) for label,text in texts.items() if len(text)==1 and (base:=wide(ord(text))) is not None and chr(base) in labels]
own=[c for c in captures if not c['external']];assert len(own)==4
#Authenticated Russian-first mixedQ15/all57group replay corrects stale41268; labels and other fixtures unchanged.
assert [(c['groups'],c['cost'],c['labels'],c['glyphs']) for c in own]==[(42,40843,[22,70,25],3),(3,12111,[134,191],1),(6,14219,[24],1),(6,29007,[18,18,18],3)]
for c in captures:
    if 'error' in c:continue
    assert bytes.fromhex(c['primaryMask'])==mask and bytes.fromhex(c['secondaryMask'])==mask and c['widePairs']==0
    for row in c['costs']:assert len(row)==195 and all(-67108864<=v<=67108864 for v in row)
    assert c['publishedLiteralText']==(c['allocatedLiteralText'] if c['hasWord'] else None)
    rows=[bytes.fromhex(v) for v in c['primary98']];b8=[bytes.fromhex(v) for v in c['b8']]
    for primary,merged in zip(rows,b8):
        assert len(primary)==200 and len(merged)==200
        for at in range(0,200,40):
            pcost=struct.unpack_from('<i',primary,at)[0];mcost=struct.unpack_from('<i',merged,at)[0]
            if mcost<0x4000000:assert pcost<0x4000000 and primary[at+12:at+18]==merged[at+12:at+18]
external=[c for c in captures if c['external']]
print(f'PASS independent192-symbol masks (3 virtuals reconstructed),4 owning raw fixtures/57NNgroups; external-coordinate records={len(external)} (not exact pen-up corpus).')
