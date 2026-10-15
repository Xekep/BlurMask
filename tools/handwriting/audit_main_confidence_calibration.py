"""Static mode1 original-symbol calibration proof; never loads vendor code."""
import hashlib,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('output.json')
b=Binary();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def block(start,end,sha):
    data=b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(data).hexdigest()==sha,hex(start)
block(0x18013e8e0,0x18013e985,'3032c7779a0fb43f759332a2ccd983e9bcfac2fb2badb56d0e33e3aac333f037')
block(0x1801c40b0,0x1801c43c4,'dee1a06b000cb2a902235c5304ba3fb77929b1fdf2a1acbcad3439261d451fb9')
actual=[(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(0x18013e8f5):b.off(0x18013e922)],0x18013e8f5)]
assert actual==[
 ('cmp','r9d, dword ptr [rdx + 4]'),('jae','0x18013e922'),('mov','ecx, dword ptr [rdx]'),
 ('cmp','r8d, ecx'),('jae','0x18013e922'),('imul','ecx, r9d'),('add','ecx, r8d'),('add','ecx, ecx'),
 ('movss','xmm2, dword ptr [r10 + rcx*4]'),('lea','eax, [rcx + 1]'),
 ('movss','xmm1, dword ptr [r10 + rax*4]'),('cvtps2pd','xmm1, xmm1'),
 ('cvtps2pd','xmm2, xmm2'),('jmp','0x18013e92d')]
#1BCBD0 retains the original shortlist label inDI, whileBX supplies the
#separately resolved NN alias. Pin both consumers instead of treating mapped
#network probabilities as already calibrated for the original symbol.
def exact(start,end,wanted):
    assert [(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(start):b.off(end)],start)]==wanted
exact(0x1801bcd73,0x1801bcd7e,[('movzx','edi, word ptr [rax + r14*8 + 8]'),('mov','word ptr [rsp + 0x32], di')])
exact(0x1801bcf98,0x1801bcf9f,[('movzx','r8d, bx'),('inc','r8d')])
exact(0x1801bd118,0x1801bd133,[('movzx','r9d, r15w'),('lea','rdx, [rsi + 0x110]'),
    ('movzx','r8d, di'),('movss','dword ptr [rsp + 0x20], xmm12'),('call','0x18013e8e0')])
for address,value in [(0x1802ed250,.575),(0x1802ed458,3.4028234663852886e38),(0x1802ed290,1.)]:
    assert struct.unpack_from('<d',b.data,b.off(address))[0]==value
resource=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(resource).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IHH',resource,566408)==(1,192,256)
matrix=resource[566416:572560]
assert hashlib.sha256(matrix).hexdigest()=='c6566cb776435b35bd6159959c192f527dd40422d3601e21453892a24e80729d'
profile=json.loads((root/'models/ru-RU/legacy-engine-profile.json').read_bytes())
for family in range(4):
    for label in range(180):
        center,slope=struct.unpack_from('<ff',matrix,8*(family*192+label))
        assert struct.pack('<f',profile['centers'][family][label])==struct.pack('<f',center)
        assert struct.pack('<f',profile['slopes'][family][label])==struct.pack('<f',slope)
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_bytes((json.dumps(dict(mode=1,storedLabels=192,activeFamilies=4,neuralOutputs=180,
    storedMatrixSha256=hashlib.sha256(matrix).hexdigest(),getter='13E8E0 original label, not neural alias',
    fallback='double0.575 only when label/family exceeds stored matrix',
    remaining='owned main group vectors, channel88, geometry/primitive services,216BC0',
    noVendorExecution=True),indent=2)+'\n').encode())
print('PASS static getter/loader blocks,23 exact bounds/index/promotion/original-vs-alias instructions, DOUBLE constants, all768 stored pairs and720 existing profile pairs')
