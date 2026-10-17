"""Pin Document036810 comparison/stride/partition behavior without DLL execution."""
import hashlib
import json
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:
    raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
lo,hi=0x180036810,0x180036c70
assert hashlib.sha256(b.data[b.off(lo):b.off(hi)]).hexdigest()=='218c63272df7047fce18b279372f8e61db3c0478e7b1a16f5e2e5ad951fcfa0d'
md=Cs(CS_ARCH_X86,CS_MODE_64)
instructions={i.address:(i.mnemonic,i.op_str)for i in md.disasm(b.data[b.off(lo):b.off(hi)],lo)}
pins={
    0x180036825:('cmp','rcx, 0xd20'),0x18003682c:('jge','0x1800369f6'),
    0x180036832:('lea','r8, [r9 + 0xa8]'),
    0x1800368d8:('comiss','xmm0, xmm2'),0x1800368db:('jbe','0x180036969'),
    0x180036949:('sub','rdx, 0xa8'),0x180036960:('comiss','xmm0, xmm2'),
    0x180036963:('ja','0x1800368e1'),
    0x1800369f6:('movabs','rax, 0xc30c30c30c30c30d'),
    0x180036a08:('imul','rcx'),0x180036a0e:('add','rdx, rcx'),
    0x180036a16:('sar','rdx, 8'),0x180036a1d:('shr','rax, 0x3f'),
    0x180036a21:('add','rdx, rax'),0x180036a24:('imul','rax, rdx, 0xa8'),
    0x180036aa0:('comiss','xmm2, dword ptr [rbx]'),0x180036aa3:('jbe','0x180036abc'),
    0x180036ac0:('comiss','xmm0, xmm2'),0x180036ac3:('jbe','0x180036ae0'),
    0x180036bc6:('sub','rdx, 0xa8'),0x180036c32:('add','rbx, 0xa8'),
    0x180036c4a:('call','0x180036810'),0x180036c5a:('call','0x180036810'),
}
for address,expected in pins.items():
    assert instructions[address]==expected,(hex(address),instructions[address])
#The signed multiply/high-half/shift sequence selects the middle record,
#not an averaged float pivot. Check the complete portable candidate domain.
magic=0xc30c30c30c30c30d-(1<<64)
for count in range(1,65537):
    distance=(count-1)*168
    high=(magic*distance)>>64
    quotient=(high+distance)>>8
    quotient+=int(quotient<0)
    assert quotient==(count-1)//2
report=dict(binarySha256=hashlib.sha256(b.data).hexdigest(),region=[hex(lo),hex(hi)],
            pins=len(pins),stride=168,inclusiveShortRangeMaximum=20,
            pivot='middle record via signed multiply-high; all65536 portable counts checked',
            comparisons='COMISS/JBE: unordered or equal stops scanning/shifting',
            ownership='Prepared full-record byte permutation only; native EEE beam owners remain unjoined',
            noVendorExecution=True)
output=Path(sys.argv[1]);output.parent.mkdir(parents=True,exist_ok=True)
output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS036810 whole PE body/{len(pins)} instructions/65536 exact pivot controls; prepared candidate ordering only')
