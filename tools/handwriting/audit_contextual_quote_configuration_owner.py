"""Read-only PE proof of the live F4 property/configuration path.

No vendor module is loaded. This does not enable the prepared21F320 driver or
claim that the English v110 resource ingress/controller has been ported.
"""
from pathlib import Path
import hashlib,json,struct,sys
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64

if len(sys.argv)!=2:raise SystemExit('report.json')
text=Binary();engine=Binary('MyScriptEngine.dll')
assert hashlib.sha256(text.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
md=Cs(CS_ARCH_X86,CS_MODE_64)
regions=[
 (text,0x18000eaf0,0x18000eb20,'f5e049a2e6a4c255d20ff65391a5cbcb897f3b06738995f8ba95e5876fdee1bf'),
 (text,0x18000eb50,0x18000eb96,'9a629bdc9af5107957494b88cf8a716799bccaf2eff70835def42fa34d595bce'),
 (text,0x18000eba0,0x18000eca0,'abc90fccdf1fd04b4f6ce5f2f4fe105c6130ced6869a0eda96c4613022f7c7ce'),
 (text,0x1801e7690,0x1801e7a10,'ef58d7d2bb7a4b7450a47744fd964fc49f17f27a86b466afd51621600a769e08'),
 (text,0x1801e87c0,0x1801e93a0,'1c27a681531ce4bbe69b8afd82a867e821a1b52e99d64a3a37fa383759b7eadb'),
 (text,0x1801e9cf0,0x1801e9e00,'88e4b63fd1e3107203f698f471d61d67ef15c22946da74f124f55c3c18f50b36'),
 (text,0x1801eb690,0x1801eb6e0,'4de7fee8b26d6ee127de54fa2712931be70287c310c2585865dda9a3cdbcc8d2'),
 (text,0x1801d4950,0x1801d51e0,'b228844e9a7159567052ffc453e3b1df1fdc61c03d54e31d70e280627ab807c9'),
 (text,0x1801d3210,0x1801d3550,'5792fc2cbbacca92a6448c019e92d4c6d3a3e4d5e198529d0bb62e1e3707b798'),
 (text,0x180196500,0x180196f40,'d70df666938d2f080c39a0e0c2650668ac688cfae7c0c357083bc6609dc8dd83'),
 (engine,0x1800f6d00,0x1800f6e30,'14ff55e29c66b943d3a8218ee3aad2ee89545b156e16897973924fa815c6764a'),
]
for b,a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h,hex(a)

# Instructions pin ownership and order; table/data checks below pin dispatch.
pins=[
 (text,0x18000c8a8,'xor','eax, eax'),
 (text,0x18000c923,'mov','word ptr [rdi + 0xf4], ax'),
 (text,0x18000eaf0,'movzx','eax, byte ptr [rdx + 0xf4]'),
 (text,0x18000eb00,'test','r8, r8'),
 (text,0x18000eb05,'movzx','eax, byte ptr [r8]'),
 (text,0x18000eb09,'mov','byte ptr [rdx + 0xf4], al'),
 (text,0x18000eb12,'mov','byte ptr [rdx + 0xf4], 0'),
 (text,0x18000eb72,'cmp','rax, 0x34'),
 (text,0x18000ebd4,'mov','r14, r8'),
 (text,0x18000ebd7,'mov','r12, rdx'),
 (text,0x18000ec0e,'mov','rdx, r14'),
 (text,0x18000ec14,'call','rax'),
 (text,0x18000ec1a,'mov','rax, qword ptr [rsi + r13 + 0x10]'),
 (text,0x18000ec24,'mov','rdx, r12'),
 (text,0x18000ec2a,'call','rax'),
 (text,0x18000ec2c,'test','r15b, r15b'),
 (text,0x18000ec33,'je','0x18000ec97'),
 (text,0x18000ec37,'cmp','ebx, 0x34'),
 (text,0x1801e7903,'mov','edx, 0x2b3'),
 (text,0x1801e7928,'mov','qword ptr [rsi + 0xf0], rax'),
 (text,0x1801e9d18,'mov','rdx, qword ptr [rdx + 0xe0]'),
 (text,0x1801e9d34,'mov','r10, qword ptr [rax + 0x28]'),
 (text,0x1801e9d4a,'mov','rdx, qword ptr [rbp + 0xf0]'),
 (text,0x1801e9d64,'mov','r10, qword ptr [rax + 0x28]'),
 (text,0x1801eb6ac,'cmp','rax, 1'),
 (text,0x1801e91d0,'mov','edx, 0x20e'),
 (text,0x1801e91e7,'mov','r9, qword ptr [r15 + 0xf0]'),
 (text,0x1801e91ee,'lea','r8, [r15 + 0xe8]'),
 (text,0x1801e91f5,'mov','rdx, qword ptr [r15 + 0xe0]'),
 (text,0x1801e91fe,'mov','r10, qword ptr [rax + 8]'),
 (text,0x1801e920c,'call','r10'),
 (text,0x18000cab0,'mov','qword ptr [rdx + 0x60], r8'),
 (text,0x18000cabb,'mov','qword ptr [r8 + 0x18], rdx'),
 (text,0x18021f337,'mov','rax, qword ptr [rdx + 0x18]'),
 (text,0x18021f344,'cmp','byte ptr [rax + 0xf4], 0'),
 (engine,0x1800f6d21,'mov','byte ptr [rsp + 0x30], 1'),
 (engine,0x1800f6d8b,'cmp','eax, 0xf03'),
 (engine,0x1800f6d92,'cmp','eax, 0x12f'),
 (engine,0x1800f6da6,'mov','qword ptr [rsp + 0x20], 1'),
 (engine,0x1800f6daf,'mov','r8d, 0x12d0017'),
 (engine,0x1800f6dc3,'call','r10'),
 (engine,0x1800f6ddc,'mov','byte ptr [rax + 0x46], 1'),
 (engine,0x1800f6de9,'mov','byte ptr [rcx + 0x47], al'),
 (engine,0x1800fa275,'call','0x1800f99a0'),
 (engine,0x1800fa284,'cmp','ebp, 0x27'),
 (text,0x1801d4ac6,'mov','eax, 0x10f'),
 (text,0x1801d4c07,'mov','eax, 0x10e'),
 (text,0x1801d33e0,'mov','eax, 0x300'),
 (text,0x180196d29,'mov','eax, 0x107'),
]
for b,a,m,o in pins:
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a))
 assert(i.mnemonic,i.op_str)==(m,o),(b.name,hex(a),i.mnemonic,i.op_str,m,o)

bindings=[
 (text,0x180274af0+0x18,0x18000cab0),
 (text,0x180274af0+0x70,0x18000eba0),
 (text,0x180274b88,0x18000eb50),
 (text,0x1802751e8,0x18000eaf0),(text,0x1802751f0,0x18000eb00),
 (text,0x1802e7128,0x1801e7690),(text,0x1802e7160,0x1801e9cf0),
 (text,0x1802e7420,0x1801eb340),(text,0x1802e7470,0x1801eb690),
 (engine,0x180252c30,0x180228b58),(engine,0x180252c38,0x1800f6d00),
]
for b,a,v in bindings:assert b.qwords(a,1)[0]==v,(b.name,hex(a))
assert text.data[text.off(0x1802751e0):text.off(0x1802751e0)+32].hex()=='17002d0101000000f0ea00800100000000eb0080010000000000000000000000'
assert text.data[text.off(0x1802e7488):text.off(0x1802e7488)+4].hex()=='0b002d01'
assert hashlib.sha256(text.data[text.off(0x180274ba0):text.off(0x180275220)]).hexdigest()=='af88a033bb95e941dca5f41e50dcfb00e50018c010f79e7457fc4005ebd93b38'
assert hashlib.sha256(engine.data[engine.off(0x180252b80):engine.off(0x180252df0)]).hexdigest()=='c122fab83801ea032aeb84f6b94a5503eff547f99abdbed0bbe9dde8f0f1812a'
name=b'EnableCurlyQuotesMapping\0';o=engine.off(0x180228b58);assert engine.data[o:o+len(name)]==name
# The current NEW20E has one property and cannot intercept12D0017. This does
# not assert that arbitrary custom/associated sources have the same closure.
assert (0x1802751e0-0x180274ba0)//32==50

resources=[]
manifest={r['path']:r['sha256'] for r in json.loads((ROOT/'models/manifest.json').read_bytes())['resources']}
for name,sampler in [('en-US/en_US-ak-cur.lite.res',182412),('en-US/en_US-ak-cur.res',374182),
 ('en-US/en_US-ak-superimposed.lite.res',207622),('en-US/en_US-ak-superimposed.res',207622),
 ('ru-RU/ru_RU-ak-cur.lite.res',188157)]:
 d=(ROOT/'models'/name).read_bytes();h=hashlib.sha256(d).hexdigest();assert h==manifest[name]
 rootVersion,baseVersion=struct.unpack_from('<HH',d,14)
 samplerVersion=struct.unpack_from('<H',d,sampler+14)[0]
 assert (rootVersion,baseVersion,samplerVersion)==((0x110,0x300,0x108) if name.startswith('en-US') else(0x10d,0x201,0x106))
 assert struct.unpack_from('<I',d,0)[0]==0x101fe90 and struct.unpack_from('<I',d,8)[0]==0x247
 assert 8+struct.unpack_from('<I',d,4)[0]==len(d)
 assert struct.unpack_from('<I',d,sampler+8)[0]==0x297
 resources.append(dict(path=name,sha256=h,rootVersion=hex(rootVersion),base134Version=hex(baseVersion),
  samplerVersion=hex(samplerVersion),sampler=sampler,category=sampler+65))

report=dict(noVendorExecution=True,readOnlyRuntime=True,
 regions=[dict(binary=b.name,start=hex(a),end=hex(z),sha256=h) for b,a,z,h in regions],
 pins=[dict(binary=b.name,address=hex(a),instruction=m+' '+o) for b,a,m,o in pins],
 bindings=[dict(binary=b.name,address=hex(a),target=hex(v)) for b,a,v in bindings],resources=resources,
 property=dict(id='0x012D0017',name='EnableCurlyQuotesMapping',container='2B3',offset='F4',fresh=0,
  setter='00EB00: NULL clears; nonNULL copies one byte without Boolean normalization',copyOrdinal=50),
 owner='12F1E7690 creates2B3@F0;1E9CF0 falls back fromNEW20E tosameF0;1E87C0 passesF0 to20E.V8/1EB340;00CAB0 attaches samecontainer tocontroller18',
 copy='00EBA0 copies52 ordered registered properties, getter failures skip, strict setter failure stops after prior writes; no rollback; nonstrict ignores setter failure and clears error at end',
 scope='Static configured-owner path only; current fresh APIs stillF4zero. Command deferred-record46/47 replay not proved. No public enabled recognizer or full Englishv110/v108 resource ingress is established.')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS {len(regions)} PE ranges/{len(pins)} instruction pins/{len(bindings)} bindings/5 raw resources; live configuredF4 owner, no runtime enable')
