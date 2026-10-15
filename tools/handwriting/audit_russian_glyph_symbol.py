"""Pinned static PE audit for the ordinary216860 scalar branch. Never loads DLLs."""
import argparse,hashlib,json,pathlib,struct,sys
p=argparse.ArgumentParser();p.add_argument('--resource',required=True);p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
sys.path.insert(0,str(root/'tools/LegacyRussianGlyphSymbol.Probe'))
from audit_output import read_alphabet
text=Binary();engine=Binary('MyScriptEngine.dll');ef=functions('MyScriptEngine.dll')
assert hashlib.sha256(text.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
def data(binary,start,end):return binary.data[binary.off(start):binary.off(end)]
blocks=[(0x180216860,0x180216bbb,'49f0d99b8e9e3c190bdc590e98874885a980244b394780689ba0d34ef3098b25'),
 (0x18021feaa,0x18021fee9,'a142ff6e68501f99f518ae01f13427417cb904897afb82d803468ea5d6a7bb55'),
 (0x180219b1a,0x180219b5a,'800656c649e0fc625e44e14b71f53f637a78938140033b993ca553fd21f195b5')]
for start,end,sha in blocks:assert hashlib.sha256(data(text,start,end)).hexdigest()==sha
#Byte-compare conversion before quote comparisons,99,FFFD and common two-output stores.
pins=[(0x180216878,'80bc24b800000000'),(0x180216897,'663bba1c250000'),
 (0x1802168be,'663bba1e250000'),(0x1802168e9,'80b89900000000'),
 (0x1802168f6,'663bbaf4240000'),(0x180216b89,'488b8424d8000000668938488b8424e0000000668938b001'),
 (0x18020c68f,'6689ab182500008d45016689ab1a250000498d57146689831c2500006683c5026689ab1e250000')]
for address,hexbytes in pins:
    value=bytes.fromhex(hexbytes);assert data(text,address,address+len(value))==value
assert data(engine,0x180052640,0x18005264d).hex()=='33c0483982880000000f95c0c3'
assert data(engine,0x180052d40,0x180052d4c).hex()=='33c03982c80000000f97c0c3'
assert engine.qwords(0x1801de1b0+0x68,1)[0]==0x180052640
assert engine.qwords(0x1801de1b0+0x80,1)[0]==0x180052d40
#No Arabic-derived88: native ctorNULL, builder exits before allocation if script scanempty.
assert '*(undefined8 *)(param_2 + 0x88) = 0;' in ef['180052260']
builder=ef['1800573b0'];assert builder.index('if (uVar6 == 0)')<builder.index('param_2[0x11] = lVar10')
scan=ef['18004e280'];assert 'iVar4 != 5' in scan and 'iVar2 - 0x660U < 10' in scan and 'iVar2 - 0x6f0U < 10' in scan
raw=pathlib.Path(a.resource).read_bytes()
assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
labels=read_alphabet(raw);cps=sorted({ord(c) for textValue in labels.values() for c in textValue})
ranges=[struct.unpack_from('<III',engine.data,engine.off(0x1801fb6d0)+12*i) for i in range(716)]
def script(cp):return next((k for start,end,k in ranges if start<=cp<=end),0)
assert all(script(cp)!=5 and not(0x660<=cp<=0x669 or 0x6f0<=cp<=0x6f9) for cp in cps)
#Complete native104 <wide> codepoint domain; none appears in any actual label.
wide={0x3000,*range(0xff01,0xff61),*range(0xffe0,0xffe7)}
assert len(wide)==104 and not(set(cps)&wide)
report=dict(functionBlocks=[dict(start=hex(s),end=hex(e),sha256=h) for s,e,h in blocks],
 instructionPins=len(pins)+2,vtableBindings=2,acousticLabels=192,storedTextMappings=len(labels),codepoints=len(cps),scriptRanges=716,
 widePairs=0,derivedArabic88=False,syntheticQuotes=[193,194],convertedLabels=[134,129],
 semantics='all ushort unchanged except early low-byte quotes; knowledge99 bypasses FFFD, otherwise frontend1A8 fallback unsupported',
 scope='scalar216860 only, not216BC0 protocol handling, UTF8 publication or a native recognition winner')
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,separators=(',',':'))+'\n',encoding='utf8');print(json.dumps(report))
