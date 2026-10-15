"""Pin the ordinary source-reference producer and forwarding chain; PE-only."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins={
 0x19a1d0:'48837c2438017220418bc8410fb7c148c1e01881e1ffffff00480bc8488b442430488908c6400800b801000000c3',
 0x152a50:'488b5278488b024c8b90500100000fb64424288844242849ffe2',
 0x1c2510:'488b9280010000488b024c8b90c80000000fb64424288844242849ffe2',
 0x2076a7:'48c744243014000000',
 0x2076c3:'33c989835c02000083c0fe898ba0030000898ba80300004863c8782e488bd148c1e2044881c2680200004803d3f602017408ff83a8030000eb06ff83a00300004883ea104883e90179e38bc62b83a00300008983a4030000',
}
for rva,expected in pins.items():
 data=bytes.fromhex(expected);off=b.off(0x180000000+rva)
 assert b.data[off:off+len(data)]==data,hex(rva)
tables={0x2de610+0x220:0x180152a50,0x2e4180+0x150:0x1801c2510,0x2e1ff0+0xc8:0x18019a1d0}
for rva,target in tables.items():assert b.qwords(0x180000000+rva,1)[0]==target,hex(rva)
report=dict(fullFunctionPins=3,counterInstructionSpans=2,vtableBindings=len(tables),
 chain='222.V220 ->21D.V150 ->21B.VC8',
 representation='one16-byte packed24-bit source node +16-bit arc; flag8zero, padding9..15 untouched',
 scope='ordinary prepared source only; not XY/glyph bounds;2075E0/206960 not executed')
if len(sys.argv)!=2:raise ValueError('report.json required')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_bytes((json.dumps(report,indent=2)+'\n').encode('ascii'))
print('PASS3 entire function pins,2 callback-counter instruction spans and3 vtable bindings; vendor code not executed')
