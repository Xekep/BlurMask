"""Static EF4 insertion mask and rejection polarity, no vendor execution."""
from pathlib import Path
import hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll')
regions=[
 (0x1800401b0,0x180040270,'41f82fed120475fadb8d721a57c1529359e57fbdb57e1435ebfbbf0b77a9b88b'),
 (0x180041240,0x180041660,'a05eaf260d8cba544af148a65ed157ec81db6790af15008fae9416209de8d9df'),
 (0x180042100,0x1800429fa,'5759f6dc6cc730baa03b865f1ba5a75a659b75a47801ef520f5fa8041d726599'),
]
ins={}
for start,end,digest in regions:
 raw=b.data[b.off(start):b.off(end)];assert hashlib.sha256(raw).hexdigest()==digest
 ins.update({i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,start)})
pins=[
 (0x1800401d4,'mov','ebp, r9d'),
 (0x18004023f,'mov','r8, rbp'),
 (0x180040242,'xor','edx, edx'),
 (0x180040244,'call','0x1801ac460'),
 (0x180041285,'mov','r9d, dword ptr [rsi + 8]'),
 (0x180041289,'call','0x1800401b0'),
 (0x1800428a6,'cmp','edi, eax'),
 (0x1800428ac,'cmp','byte ptr [rcx + r13], 0'),
 (0x1800428b1,'je','0x1800428c0'),
 (0x1800428b3,'mov','rax, qword ptr [rbp - 0x68]'),
 (0x1800428b7,'cmp','byte ptr [rcx + rax], 0'),
 (0x1800428bb,'jne','0x18004291d'),
 (0x18004291d,'xor','bl, bl'),
 (0x1800428ef,'mov','r8, qword ptr [rbp - 0x38]'),
 (0x1800428fd,'lea','r9, [rbp - 0x68]'),
 (0x18004290c,'call','0x180041cf0'),
]
for a,m,o in pins:assert ins[a]==(m,o),(hex(a),ins.get(a))
report=dict(regions=3,instructionPins=len(pins),
 initialMask='041240 ->0401B0 appends zero bytes; r8 tensor argument is not read as fill',
 reject='postDeletion[index] != 0 AND initialInsertion[index] != 0',
 polarity='0428B7 cmp0;0428BB JNE04291D;04291D xorBL,BL (failure)',
 scoreInput='Original activated tensor rbp-38 and initial insertion mask rbp-68, not post-pruning mask',
 scope='Prepared preservation predicate only; actual full EF4 coordinate/record join unfinished')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS3 regions/{len(pins)}pins; native JNE failure protects inserted nonzero-mask units; no vendor execution')
