"""Pinned Math2 EED beam-width producers; static PE/ORT FlatBuffer reads only.
No vendor DLL, ORT session, model inference or generic beam implementation.
"""
import hashlib
import json
import struct
import subprocess
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) not in (2, 3):
    raise SystemExit('report.json [models/math2/math2-unwrapped.res]')
out = Path(sys.argv[1]).resolve()
resource_path = Path(sys.argv[2]) if len(sys.argv) == 3 else ROOT/'models/math2/math2-unwrapped.res'
b = Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions = [
    (0x180044080, 0x180044180, '9d517a7a291fa99c60acb2e690ac1d65b7dd7604eafa6410c05aaafd59d5091a'),
    (0x180044180, 0x180044630, '1375d6401023ec114aff9e145fc11b501b0201451e633c839852e97014243997'),
    (0x180044630, 0x1800448b0, 'bf03c90bfc43d2c819d3ecfccc89cdb75f47910aa5a6b58bd2798978079a241e'),
    (0x180044ac0, 0x180044fb0, 'f7cbc430d27bd819ff2a0d4346def61ff3939f7a8c238cd7f5ede03c104e4265'),
    (0x180044fb0, 0x1800450c0, '7adb96403839d9becbb1d9556c4125441fa60be05f2f84c4e9a9225aa4442331'),
    (0x180045460, 0x180045b90, '8a9d69ac15350ad940b5871b5753d679ea833dafffe8eba010d8699cb47ff908'),
    (0x1800486b0, 0x180048df0, '74792cb49818c071b5987e92ebbdf1f160288713c5374d7280b399c1f18f6489'),
    (0x1800277b0, 0x180027840, '4379afdb74b7d3a8f2c39de533454a84aec09b82420e03859cba5caa54cfebe8'),
    (0x180027970, 0x180027c20, 'ef572e99b5e6f65d4bc94e8b77a9ae65edb867b41b64ed3ab804199122c2eef9'),
]
md = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {}
for lo, hi, expected in regions:
    raw = b.data[b.off(lo):b.off(hi)]
    assert hashlib.sha256(raw).hexdigest() == expected
    instructions.update((i.address, (i.mnemonic, i.op_str)) for i in md.disasm(raw, lo))
#Consumer/persistence pins are in a separate function, with its complete hash
#and signed width-selection proof retained by audit_math2_decoder_route.
for i in md.disasm(b.data[b.off(0x1800450c0):b.off(0x180045460)], 0x1800450c0):
    instructions[i.address] = (i.mnemonic, i.op_str)
pins = {
    0x180044125: ('add', 'rdi, 0x68'),
    0x18004412b: ('xor', 'edx, edx'),
    0x18004412d: ('mov', 'r8d, 0x170'),
    0x180044136: ('call', '0x1801ac460'),
    0x18004453e: ('movups', 'xmm0, xmmword ptr [r14 + 0x90]'),
    0x180044546: ('movups', 'xmmword ptr [rcx + 0x90], xmm0'),
    0x180044649: ('cmp', 'r9, 0x188'),
    0x180044656: ('call', '0x180044080'),
    0x180044720: ('lea', 'rax, [rdi + 0x68]'),
    0x180044ae0: ('mov', 'rsi, qword ptr [r9 + 8]'),
    0x180044dbf: ('lea', 'r9, [rip + 0x1d589a]'),
    0x180044deb: ('jne', '0x180044e05'),
    0x180044df6: ('mov', 'r9, qword ptr [rax + 0x78]'),
    0x180044dfd: ('mov', 'dword ptr [rsi + 0x28], eax'),
    0x180044fdb: ('lea', 'r9, [rip + 0x1d56e2]'),
    0x180045005: ('cmp', 'byte ptr [rbp + 2], 1'),
    0x18004504c: ('lea', 'r9, [rip - 0x593]'),
    0x18004505a: ('mov', 'qword ptr [rsp + 0x20], r14'),
    0x180045480: ('mov', 'rax, qword ptr [rdx + 0x1c0]'),
    0x180045492: ('mov', 'rax, qword ptr [rdx + 0x1c8]'),
    0x1800454a3: ('mov', 'rax, qword ptr [rdx + 0x1d0]'),
    0x1800454b3: ('mov', 'rax, qword ptr [rdx + 0x170]'),
    0x1800455d0: ('cmp', 'esi, 4'),
    0x1800455ea: ('lea', 'rax, [rdi + 0x68]'),
    0x1800455ee: ('mov', 'qword ptr [rsp + 0x38], rax'),
    0x1800455fb: ('lea', 'r9, [rip - 0x652]'),
    0x180045618: ('mov', 'qword ptr [rsp + 0x20], rax'),
    0x1800456c2: ('mov', 'rbp, qword ptr [r15 + 0x10]'),
    0x180045712: ('lea', 'r9, [rip + 0x1d4f47]'),
    0x180045723: ('mov', 'r10, qword ptr [rax + 0x158]'),
    0x180045730: ('je', '0x18004574b'),
    0x18004573b: ('mov', 'r9, qword ptr [rcx + 0x78]'),
    0x180045745: ('mov', 'dword ptr [rdi + 0x90], eax'),
    0x180045938: ('cmp', 'qword ptr [rdi + 0x220], r12'),
    0x18004593f: ('jne', '0x180045980'),
    0x18004537e: ('mov', 'dword ptr [rdi + 0x90], r14d'),
    0x1800277fb: ('mov', 'dword ptr [rdi + 0x18], 1'),
    0x180027806: ('mov', 'qword ptr [rdi + 0x48], rcx'),
    0x180027829: ('mov', 'dword ptr [rdi + 0x38], 0xffffffff'),
    0x180027aa9: ('call', 'rbx'),
    0x180027ab2: ('mov', 'qword ptr [rcx + r14*8], rax'),
    0x180027abe: ('mov', 'edx, 0xeed'),
    0x180027ad9: ('lea', 'r8, [rbp + 0x38]'),
    0x180027ae0: ('mov', 'rax, qword ptr [rax + 8]'),
    0x180027ae8: ('call', 'rax'),
}
for address, expected in pins.items():
    assert instructions[address] == expected, (hex(address), instructions[address], expected)
#Literal names are validated at their actual RIP-relative addresses, rather
#than treating unrelated symbolic tensor dimension strings as properties.
assert b.data[b.off(0x180044dc6+0x1d589a):b.off(0x180044dc6+0x1d589a)+10] == b'beam_size\0'
assert b.data[b.off(0x180045719+0x1d4f47):b.off(0x180045719+0x1d4f47)+10] == b'beam_size\0'
assert b.data[b.off(0x180044fe2+0x1d56e2):b.off(0x180044fe2+0x1d56e2)+7] == b'config\0'
bindings = [
    (0x1801fd090+0x18, 0x1801fd050),
    (0x1801fd050+0x10, 0x1801fd010),
    (0x1801fd010, 0x180044080),
    (0x1801fd010+8, 0x180044180),
    (0x1801fd010+0x10, 0x180044630),
    (0x1801fd010+0x18, 0x1800486b0),
    (0x1801fd090+0x20, 0x1801fd0d8),
    (0x1801fd0d8+8, 0x180045460),
    (0x1801b7390+0x18, 0x1801b7458),
    (0x1801b7458+0x10, 0x1801b73d8),
    (0x1801b73d8, 0x1800277b0),
    (0x1801b7390+0x20, 0x1801b7420),
    (0x1801b7420+0x28, 0x180027970),
]
for address, expected in bindings:
    assert b.qwords(address,1)[0] == expected, hex(address)
assert b.qwords(0x1801fd090+8,1)[0] == 0x13300000eed
assert b.qwords(0x1801b7390+8,1)[0] == 0x400000eeb

class Metadata:
    """Bounded ORTM InferenceSession.Model.metadata_props (field1/field9).
    All three complete blobs are independently fingerprinted below.
    This reads FlatBuffer data only and imports no ORT runtime.
    """
    def __init__(self,data):
        self.data=data
    def read(self,fmt,p):
        size=struct.calcsize('<'+fmt)
        assert 0 <= p <= len(self.data)-size
        return struct.unpack_from('<'+fmt,self.data,p)[0]
    def field(self,table,index):
        vt=table-self.read('i',table)
        size=self.read('H',vt);obj=self.read('H',vt+2)
        assert size>=4 and size%2==0 and 4<=obj and table+obj<=len(self.data)
        if 4+2*index>=size:return None
        offset=self.read('H',vt+4+2*index)
        if not offset:return None
        assert 4<=offset and offset+4<=obj
        return table+offset
    def indirect(self,p):
        assert p is not None
        value=p+self.read('I',p)
        assert 0<=value<len(self.data)
        return value
    def string(self,p):
        start=self.indirect(p);size=self.read('I',start)
        assert size<=1048576 and start+4+size<len(self.data)
        assert self.data[start+4+size]==0
        return self.data[start+4:start+4+size].decode('utf-8')
    def properties(self):
        assert self.data[4:8]==b'ORTM'
        model=self.indirect(self.field(self.read('I',0),1))
        vector=self.indirect(self.field(model,9));count=self.read('I',vector)
        assert 0<count<=64
        result={}
        for index in range(count):
            row=self.indirect(vector+4+index*4)
            key=self.string(self.field(row,0));value=self.string(self.field(row,1))
            assert key not in result
            result[key]=value
        return result

resource=resource_path.read_bytes()
assert len(resource)==18704269
assert hashlib.sha256(resource).hexdigest()=='2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
assert struct.unpack_from('<4sIIHH',resource,0)==(b'\x90\xfe\x01\x01',18704261,0xeed,0,0x103)
mode=struct.unpack_from('<I',resource,12665)[0];assert mode==0
model_nodes=[
    (12673,15935209,15922504,'3c1a5b691e34385cdaf18f4bdbdb8c8f8cd2cb757f4faed081858f17ea69f286'),
    (15935213,16295565,360320,'1393a3c67eafbee9f0bf80a6c3442119bf78b26901986587a2442b1c2d4a0c4a'),
    (16295569,18704265,2408664,'925a3302b612e634a226ddc907d6ac52c8e908b3d931040cd49b63f4875e4638'),
]
models=[]
for slot,(offset,end,size,sha) in enumerate(model_nodes):
    magic,length,type_id,flags,version=struct.unpack_from('<4sIIHH',resource,offset)
    assert (magic,type_id,flags,version)==(b'\x90\xfe\x01\x01',0x51,0,0x100)
    assert struct.unpack_from('<I',resource,offset-4)[0]==0x51 and offset+8+length==end
    assert struct.unpack_from('<Q',resource,offset+24)[0]==size
    blob=resource[offset+32:end];assert len(blob)==size and hashlib.sha256(blob).hexdigest()==sha
    properties=Metadata(blob).properties();assert set(properties)=={'config','file_name','onnx.infer'}
    config=json.loads(properties['config']);assert isinstance(config,dict) and 'beam_size' not in config
    models.append(dict(slot=slot,nodeOffset=offset,blobBytes=size,blobSha256=sha,
        metadata=properties,config=config,configHasBeamSize=False,
        otherRawBeamSizeStrings=blob.count(b'beam_size')))
assert struct.unpack_from('<I',resource,18704265)[0]==0
out.parent.mkdir(parents=True,exist_ok=True)
dependency=out.with_name(out.stem+'-decoder-route.json')
subprocess.run([sys.executable,str(ROOT/'tools/handwriting/audit_math2_decoder_route.py'),str(dependency)],check=True)
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in regions],
    instructionPins=len(pins),tableBindings=len(bindings),resourceSha256=hashlib.sha256(resource).hexdigest(),
    resourceMode=mode,models=models,decoderRoute=json.loads(dependency.read_bytes()),
    producers=dict(freshEed='044080 zeroes owner68..1D8, including saved90',
        clone='044180 copies saved90 and may retain decoder220; freshEEB does not erase inherited positive width',
        rawInit='044630 packet188 copies payload18..188 to owner68..1D8; saved90 comes from packet40',
        modelConfig='045460 four source owners1C0/1C8/1D0/170 ->044FB0 config JSON ->044AC0 frame68+28=owner90',
        requestConfig='045460 reads request+10 config and beam_size viaV158/V78; stores owner90 after model callbacks',
        request='EEB027970 clones sourceEED then passes EEB+38; fresh0277B0 requested38=-1, config48=0; worker18=1 is distinct',
        selection='0450C0 signed requested>0 else signed saved90>0 else1; width<=16; selected90 stored at04537E',
        warm='045460 skips0450C0 if decoder220 already nonnull'),
    conclusion='Cold source with known saved90=0, freshEEB request=-1, no caller config and these no-beam JSONs selects1. Existing/copied/configured EED source may select another width; mode0 alone proves none of this.',
    excluded=['all external request caller provenance','arbitrary copied/property-configured sources','EEE beam-search implementation','greedy equivalence','recognition accuracy'],
    appChange=False,noVendorExecution=True,noOrtExecution=True)
out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS{len(regions)} native producer regions/{len(pins)} instruction pins/{len(bindings)} bindings/3 raw model metadata maps; fresh conditional width proof only')
