"""Static source70 category transport audit; never executes a vendor DLL."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
b=Binary('MyScriptText.dll');fs=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=Cs(CS_ARCH_X86,CS_MODE_64)
def exact(start,end,expected):
    actual=[(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(start):b.off(end)],start)]
    assert actual==expected,(hex(start),actual)
#Read the complete16 bytes returned by70, not an unrelated category scalar.
exact(0x180204da5,0x180204da8,[('movups','xmm7, xmmword ptr [rax]')])
#History request base=RBP170;16-byte token goes to188 (+18).
exact(0x180205344,0x18020534b,[('lea','r9, [rbp + 0x170]')])
exact(0x180205368,0x18020536f,[('movups','xmmword ptr [rbp + 0x188], xmm7')])
#Fallback/first-word request base=RBP1C0;token goes to1D8 (+18).
exact(0x1802061c2,0x1802061c9,[('lea','r9, [rbp + 0x1c0]')])
exact(0x1802061d4,0x1802061db,[('movups','xmmword ptr [rbp + 0x1d8], xmm7')])
#100090 scalar category argument4 reads request20=token+8.
exact(0x1801003e4,0x1801003ec,[('mov','r9d, dword ptr [r14 + 0x20]'),('mov','byte ptr [rsp + 0x20], al')])
exact(0x1801003ec,0x1801003f1,[('call','0x1800ffcf0')])
exact(0x180100db0,0x180100dc4,[('mov','r9d, dword ptr [r13 + 0x20]'),('mov','dword ptr [rsp + 0x28], edx'),('mov','rdx, rbx'),('mov','byte ptr [rsp + 0x20], cl'),('call','0x1800ffec0')])
#History changes only boundary24, preserving category20 from source70.
exact(0x180205bf6,0x180205bfc,[('mov','byte ptr [rbp + 0x194], dl')])
checks={
'plainClear0':('180094040','*param_1 = 0;'),
'plainClear8':('180094040','param_1[1] = 0;'),
'categoryCost':('180100090','FUN_1800ffcf0(uVar35,param_2,param_3,*(undefined4 *)(param_4 + 0x20)'),
'categoryHistory':('180100090','*(undefined4 *)(local_d8 + 0x20)'),
'firstCopy':('180204290','local_d8 = CONCAT44(uVar49,uVar48);'),
'historyCopy':('180204290','local_128 = CONCAT44(uVar49,uVar48);'),
}
for name,(address,text) in checks.items():assert text in fs[address],name
out=dict(staticBindings=len(checks),exactInstructions=14,
 category='source70 token+8 -> LM request20; geometry is not substituted',
 ordinaryTokenCategory=0,scope='ordinary first/history completion; no full native winner claim')
if len(sys.argv)!=2:raise SystemExit('output.json')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_bytes((json.dumps(out,indent=2)+'\n').encode('utf-8'))
print('PASS source70 category6 static bindings/14 exact ASM instructions; ordinary token+8 iszero')
