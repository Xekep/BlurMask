"""Static completed-record -> EE4 -> unit alternatives/output route.

Vendor PE/resource bytes are data only; no DLL/ORT inference or formula claim.
"""
import hashlib
import json
from pathlib import Path
import runpy
import struct
import subprocess
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv) != 2:
    raise SystemExit('report.json')
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary

b = Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions = [
    (0x1800236b0,0x180023cf0,'14774491e51212504bd64554279b96972fa7e73396161cffa168f97964d58dd4'),
    (0x180024780,0x180024900,'e485c58d52b4990cb57d9b5f3f200bb9fe4fd3adfcfb97b87c666bf299b7dae1'),
    (0x180024a20,0x180024c60,'4938a9eccc50c7ec805a439cbe61f0c559a56064c1a3f9c29af9bcbca1aef5ae'),
    (0x1800251e0,0x180025980,'438532fa5596a3d1295e55e80261ec8f5945d4ef8bf0496419eb306a6177d52a'),
    (0x180025980,0x180026010,'779faa001fcd60bd611a83e94aa4bf914c0c9a7058c3e23e0fde644ee9f6ecfc'),
    (0x18003a260,0x18003b220,'43b6395c140ccc4975bcc1dde81378e5df1745dd5bdd1cbdb88a79007780f19d'),
    (0x180043650,0x180043740,'53993a2824806e286d280d5c89104a0e57c15f05cc784b1e32edf06409ccb6a3'),
    (0x180043ac0,0x180043e00,'a33db9e452ccdfc9255c0dfc34053e278b3c0ebf36ef06d17c96b917cbfb9017'),
    (0x1800450c0,0x180045460,'155f71ec1b5fea2831e30c94fa71dfcd3d3c8753d41bfe22dd27df8b9eb51693'),
    (0x180045c60,0x1800463e0,'087d16b280bec35a09317b4021770f634d031dc632a8a02caaa2f54d1f2ca8cc'),
    (0x180046ae0,0x180046f40,'20cbb279671b9a71d1bf4c426e6f2d028f1fcca709eacb979154629d3ff7f59f'),
    (0x180047c50,0x1800485b0,'7176ded3572da44ea4d47c139eb64fd44af6d8552f45df66c5dbaaa1a357a49b'),
    (0x180043e00,0x180043fa0,'49582d137385cbf02e6ac2a9aa6536fc2d0cf35fcc6b5af2b2a9d460cb946c26'),
    (0x1800486b0,0x180048df0,'74792cb49818c071b5987e92ebbdf1f160288713c5374d7280b399c1f18f6489'),
    (0x1800277b0,0x180027840,'4379afdb74b7d3a8f2c39de533454a84aec09b82420e03859cba5caa54cfebe8'),
]
ins = {}
for start,end,expected in regions:
    raw = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == expected
    ins.update({i.address:(i.mnemonic,i.op_str) for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,start)})
pins = [
    (0x180027830,'mov','word ptr [rdi + 0x3c], 0x101'),
    (0x18004517e,'cmp','byte ptr [r8 + 5], r15b'),
    (0x180045182,'je','0x1800451d1'),
    (0x180045184,'mov','rax, qword ptr [rdi + 0x1d8]'),
    (0x18004518b,'mov','edx, 0xee4'),
    (0x1800451b9,'mov','r9d, 8'),
    (0x1800451c7,'mov','qword ptr [rdi + 0x228], rax'),
    (0x18003a68a,'cmp','qword ptr [rax + 0xc0], r14'),
    (0x18003a691,'je','0x18003a6ac'),
    (0x18003ad5a,'movups','xmm0, xmmword ptr [r12]'),
    (0x18003ad5f,'movaps','xmm1, xmmword ptr [rsp + 0x70]'),
    (0x18003ad64,'movups','xmmword ptr [rsp + 0x70], xmm0'),
    (0x18003ad69,'movups','xmmword ptr [r12], xmm1'),
    (0x18003ad6e,'test','r13b, r13b'),
    (0x180048168,'call','r10'),
    (0x18004816b,'mov','byte ptr [rsp + 0x50], 1'),
    (0x180048170,'test','al, al'),
    (0x180048172,'jne','0x180048179'),
    (0x180048174,'mov','byte ptr [rsp + 0x50], 0'),
    (0x180048428,'cmp','byte ptr [rsp + 0x50], 0'),
    (0x180048435,'je','0x180048498'),
    (0x18004844c,'call','0x180046ae0'),
    (0x180048453,'je','0x180048498'),
    (0x18004846f,'call','0x180043ac0'),
    (0x18004847b,'je','0x18004849a'),
    (0x180048486,'movups','xmm0, xmmword ptr [rax]'),
    (0x180048489,'movaps','xmm1, xmmword ptr [rsp + 0x70]'),
    (0x18004848e,'movups','xmmword ptr [rsp + 0x70], xmm0'),
    (0x180048493,'movups','xmmword ptr [rax], xmm1'),
    (0x180048498,'xor','bl, bl'),
    (0x1800484a2,'call','0x180043e00'),
    (0x180043f15,'mov','rdx, qword ptr [rsi + 0x70]'),
    (0x180043f25,'call','r8'),
    (0x180043f5d,'lea','rsi, [rsi + 0xa8]'),
    (0x180043f64,'sub','rbp, 1'),
    (0x180043f68,'jne','0x180043e40'),
    (0x180043f7a,'mov','rdx, qword ptr [r14]'),
    (0x180043f9d,'jmp','r8'),
    (0x180046bf7,'lea','r12, [r15 + 0x50]'),
    (0x180046c18,'mov','r10, qword ptr [rax + 0x18]'),
    (0x180046c2e,'mov','qword ptr [rsp + 0x20], r12'),
    (0x180046c33,'call','r10'),
    (0x180046c46,'mov','rax, qword ptr [r15 + 0x68]'),
    (0x180046c52,'cmp','eax, -1'),
    (0x180046c5e,'movzx','ecx, word ptr [rax + rcx*2]'),
    (0x180046c62,'mov','dword ptr [r8], ecx'),
    (0x180046d23,'cmp','byte ptr [r11 + rax*4 + 2], 0'),
    (0x180046d2f,'cmp','word ptr [r8], r14w'),
    (0x180046d3a,'cmp','byte ptr [r11 + rcx*4 + 2], 0'),
    (0x180046d46,'cmp','byte ptr [r10 + 2], 0'),
    (0x180046d52,'cmp','cx, bx'),
    (0x180046d57,'lea','r9d, [rbx + 9]'),
    (0x180046d5b,'cmp','ecx, r9d'),
    (0x180046d64,'cmp','ax, bx'),
    (0x180046d69,'cmp','eax, r9d'),
    (0x180046d6e,'mov','word ptr [r8], si'),
    (0x180046d87,'movups','xmm0, xmmword ptr [r12]'),
    (0x180046d94,'movups','xmmword ptr [r12], xmm1'),
    (0x180046da2,'call','0x180045c60'),
    (0x180045c8b,'movzx','eax, word ptr [rdx + 0x1e2]'),
    (0x180045ce3,'lea','r12, [rdi + 0x78]'),
    (0x180045ce7,'mov','r15, qword ptr [rdi + 0x88]'),
    (0x180045cf8,'mov','ebp, dword ptr [rcx + 0x5c]'),
    (0x180045d06,'jle','0x180045d18'),
    (0x180045d18,'mov','dword ptr [rsp + 0x44], r14d'),
    (0x180045d27,'call','0x180043650'),
    (0x180045db1,'mov','r8d, 0xffffffff'),
    (0x180045dbf,'mov','byte ptr [rax + rsi*8 + 0x20], 0'),
    (0x180045dc8,'mov','dword ptr [rax + rsi*8 + 0x24], r8d'),
    (0x180045dcd,'test','dl, dl'),
    (0x180045dcf,'jne','0x1800461f5'),
    (0x180045dde,'cmp','bx, word ptr [rsp + 0x148]'),
    (0x180045de6,'jae','0x180046145'),
    (0x180045dfd,'mov','rdx, qword ptr [r11 + 0x1d8]'),
    (0x180045e13,'mov','r10, qword ptr [rax + 0x30]'),
    (0x180045e17,'call','r10'),
    (0x180048795,'mov','ecx, 0x103'),
    (0x18004879a,'cmp','bx, cx'),
    (0x1800487a6,'lea','r8, [rsi + 0x1e2]'),
    (0x1800487ad,'mov','r9d, 2'),
    (0x1800487c3,'call','r10'),
    (0x180025f69,'call','0x1800251e0'),
    (0x180025fe3,'call','0x180024a20'),
]
for address,mnemonic,operands in pins:
    assert ins[address] == (mnemonic,operands), (hex(address),ins.get(address))
bindings = [(0x1801b7200+8,0x400000ee4),(0x1801b7200+0x18,0x1801b7248),
    (0x1801b7248+8,0x1801b7280),(0x1801b7248+0x10,0x1801b71e8),
    (0x1801b71e8+0x10,0x1800236b0),(0x1801b7280+0x10,0x180024a20),
    (0x1801b7280+0x18,0x180025980),(0x1801fd0d8+0x20,0x180047c50),
    (0x1801ed930+0x28,0x18003a260)]
for address,expected in bindings:
    assert b.qwords(address,1)[0] == expected
engine=Binary('MyScriptEngine.dll')
assert hashlib.sha256(engine.data).hexdigest() == 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert engine.qwords(0x18023cfc8,3) == (0x180031390,0x18023cf80,0x1800315e0)
for address,text in [(0x18020fb34,'\u22c5'),(0x18020ca80,'.'),(0x18020d8a8,'0')]:
    p=b.off(address);encoded=text.encode('utf-8')+b'\0'
    assert b.data[p:p+len(encoded)]==encoded

reader=runpy.run_path(str(ROOT/'tools/export-ink-math-model.py'))
resource=(ROOT/'models/math2/math2-unwrapped.res').read_bytes()
assert hashlib.sha256(resource).hexdigest() == '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
_,_,_,end=reader['header'](resource,90)
r=reader['Reader'](resource,106,end)
fields=r.read('H',8);assert fields == [65535]*7+[299]
n=r.read('H')
for increment in (32,64,1):
    table=r.read('H',n);n=next(v+increment for v in reversed(table) if v!=65535)
spans=r.read('H',n+1);entries=[r.read('II') for _ in range(spans[-1]+1)]
utf8=r.bytes(entries[-1][0])
labels={label & 65535:utf8[offset:entries[i+1][0]].decode('utf-8')
        for i,(offset,label) in enumerate(entries[:-1])}
inverse={text:label for label,text in labels.items()}
assert set(labels)==set(range(299)) and struct.unpack_from('<H',resource,end)[0]==297
rule_start,rule_end=0x180254970,0x180254d60
assert hashlib.sha256(b.data[b.off(rule_start):b.off(rule_end)]).hexdigest() == \
    '0e9aa99e7af9d0b7adcb11f70b9c5a712890b5aed533b0fc5529abe3a16b44b7'
rules=[]
for ordinal in range(14):
    at=rule_start+ordinal*72
    names=[]
    for pointer in b.qwords(at+32,5):
        if not pointer:break
        p=b.off(pointer);names.append(b.data[p:b.data.index(0,p)].decode('utf-8'))
    assert names and all(name in inverse for name in names)
    rules.append(dict(ordinal=ordinal,type=b.qwords(at,1)[0],names=names,labels=[inverse[name] for name in names]))

# Independent prepared046D20 decimal rewrite: no grammar hit and no SDK call.
# Every flag combination is checked; only the LOW16 label store may change.
dot,period,zero=[inverse[s] for s in ('\u22c5','.','0')]
assert [inverse[str(i)] for i in range(10)] == list(range(zero,zero+10))
controls=[]
for flag_bits in range(8):
    original=[(0x71000000 | (0x10000 if flag_bits & 1 else 0) | inverse['1']),
              (0x82000000 | (0x10000 if flag_bits & 2 else 0) | dot),
              (0x93000000 | (0x10000 if flag_bits & 4 else 0) | inverse['2'])]
    output=original.copy()
    for at in range(1,len(output)-1):
        left,current,right=output[at-1:at+2]
        if not any((word>>16)&255 for word in (left,current,right)) and current & 65535 == dot and \
           zero<=left & 65535<=zero+9 and zero<=right & 65535<=zero+9:
            output[at]=(current & 0xffff0000)|period
    assert (output!=original)==(flag_bits==0)
    assert [word & 0xffff0000 for word in output]==[word & 0xffff0000 for word in original]
    controls.append(dict(flags=flag_bits,before=original,after=output))
rule_ids={label for rule in rules for label in rule['labels']}
assert not rule_ids.intersection([inverse['M'],inverse['I'],inverse['1'],dot,inverse['2']])

out=Path(sys.argv[1]).resolve();out.parent.mkdir(parents=True,exist_ok=True)
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in regions],
    instructionPins=len(pins),tableBindings=len(bindings)+1,rules=rules,
    sourceAlphabetCount=299,outputOrdinaryLabelLimit=297,decimalControls=controls,
    library='Same completed-record88 Engine3E.V10=0315E0; reusable portable byte interval interner already exists, owner join missing',
    formatter='Fresh request5=1 ->EE4 ctor236B0/VT1B7280.V18=025980 ->251E0 compact/map ->24A20 grammar check',
    output='EEE3A260 transfers whole168 owners; EED47C50 ->46AE0 ->45C60 per-unit40 with16B alternatives, then optional43AC0 copy +wholevector transfer',
    failure='EEE partial local transfer is not publication;47C50false skipsformatter/firstcopy/externalvector swap andalways43E00destroysalllocalrows',
    portableFailureCleanup='For scoped noConstraint failure, poison+DisposeAll andnooutput is a deliberate observable cleanup adaptation; no need to sort canceled output solely for unobservable native tail order',
    next='Whole-vector no-rule-hit EE4 subset +decimal rewrite +same Library per-unit alternatives; reject any retained grammar token, never filter',
    unresolved=['Full14rule EE4 engine','General existing-source/string lanes','Native external tree/layout/source context','Native allocator/destructor ABI order'],
    vendorExecution=False,runtimeChanges=False)
out.write_text(json.dumps(report,indent=2,ensure_ascii=True)+'\n',encoding='utf-8')
print(f'PASS{len(regions)} regions/{len(pins)} pins/{len(bindings)+1} bindings/14actualALrules/8decimalcontrols; static output/failure closure only')
