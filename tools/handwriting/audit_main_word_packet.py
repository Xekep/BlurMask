"""Static PE reads only: the21FE30 packet-to-WORD stores."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x18021fe4c,0x18021fec0
h='3169ad28b74dd2a15d3a6dd36509cae181d4259e1eb2791c797adaa0d929579b'
assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins=[(0x18021fe6b,'894510'),(0x18021fe72,'894518'),(0x18021fe79,'89451c'),
 (0x18021fe81,'66894530'),(0x18021fe8a,'66894532'),(0x18021fe93,'884535'),
 (0x18021fe9b,'884536'),(0x18021fea2,'89453c'),
 (0x18021fea5,'410f10401c'),(0x18021feb4,'0f114544'),(0x18021febd,'884537'),
 (0x18021fec7,'418b403c'),(0x18021fecf,'418b4038'),(0x18021fed7,'410fb7402e')]
for addr,raw in pins:assert b.data[b.off(addr):b.off(addr)+len(raw)//2].hex()==raw
f=functions()
blocks=[(0x18013b870,0x18013b9d3,'5b6e42f5c7fadcdb0ccfa09a8442923554c5e87369d23ac3b87138a17beadf0f'),
 (0x18013ab80,0x18013ab92,'c2f749f5253e3cc2bfed3a0a16c6c1060f64c1fc76324185d3759df88da80148'),
 (0x18013f110,0x18013f180,'226c0c304932be58448a7788fb61a798d7016b5f37d0fc1712c569264f4d8123')]
for first,last,expected in blocks:assert hashlib.sha256(b.data[b.off(first):b.off(last)]).hexdigest()==expected
for name in ['180155da0','180155ef0']:assert '*(undefined1 *)(param_2 + 0x220) = 0;' in f[name]
assert 'local_res20[0] = *(undefined1 *)(param_2 + 0x220);' in f['180157c60']
assert '(param_1,local_188,local_res20,1)' in f['180157c60']
assert '*(undefined1 *)(param_2 + 0x41) = *param_3;' in f['18013b870']
assert '(param_1,uVar2,param_2 + 0x41,1)' in f['18013b870']
assert '*(undefined2 *)(param_2 + 0xc4) = 0;' in f['18000c860']
assert b.data[b.off(0x18000c8f3):b.off(0x18000c8f3)+7].hex()=='668987c4000000'
Path(sys.argv[1]).write_text(json.dumps(dict(start=hex(a),end=hex(z),sha256=h,
 pins=len(pins),scalarStores=9,vectorStoreBytes=16,
 factoryBlocks=[dict(start=hex(x),end=hex(y),sha256=h) for x,y,h in blocks],
 capture='fresh driver220=0 ->157C60 shared TOP273 ctor13B870 field41 ->274 ctor13AB80 field10',
 ownerC4='fresh00C860 U16+C4=0; no property/reused-driver overrides',
 reset='13F110 fields only; untouched WORD44..53 retained; native pointer vectors excluded',
 boundaryHints='packet38/3C are218560 arguments; not TOP.AC/B0',
 scope='explicit packet fields only; no segment or accepted publication',noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS pinned21FE30 field-store block/14 pins,3 factory/reset ranges and fresh capture/C4 chain; TOP fields separate')
