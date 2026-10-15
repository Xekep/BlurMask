"""Static PE contract for139C90 scalar residue and21FBF0 quote writes. No loading."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
b=Binary();fs=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins=[(0x18021fbf0,0x18021fe04,'b90b1c22b0c468c1b2dd6d4cacf647e5e6d3209b500b8cff246136304cd21d89'),
 (0x180139c90,0x180139d4b,'d3b3a681b6bba98b6429180a86fe5f47ce60c0adc086c6a09f6bf01b3d652cd7'),
 (0x18013f180,0x18013f1ee,'e02ddf2860efd75ac888cb8aec79af55ff3162544c8d616b3f8b4b60a45bf8fc')]
for start,end,digest in pins:assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()==digest
def ins(start,end,wanted):
 actual=[(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(start):b.off(end)],start)]
 assert actual==wanted,(hex(start),actual)
ins(0x18021fd4a,0x18021fd70,[('cmp','ax, word ptr [rsi + 0x251c]'),('jne','0x18021fd5c'),
 ('movzx','eax, word ptr [rsi + 0x24ba]'),('jmp','0x18021fd6c'),('cmp','ax, word ptr [rsi + 0x251e]'),
 ('jne','0x18021fd70'),('movzx','eax, word ptr [rsi + 0x24bc]'),('mov','word ptr [rbp + 8], ax')])
ins(0x18021fde0,0x18021fdfc,[('movzx','eax, word ptr [rsi + 0x251c]'),('cmp','word ptr [rsp + 0x88], ax'),
 ('jne','0x18021fdfc'),('movzx','eax, word ptr [rsi + 0x24ba]'),('mov','word ptr [rdi + 8], ax')])
assert b.qwords(0x1802dc680+0x58,1)[0]==0x180139c90
f=fs['180139c90']
assert 'if ((uVar1 & 0x7ffc) == 0)' in f and 'goto LAB_180139d3f' in f
assert '*(undefined1 *)(param_5 + 1) = 0' in f
assert '*(short *)(*(longlong *)(param_2 + 0x28) + 0xe)' in fs['18021fbf0']
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(pinnedRanges=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in pins],
 exactQuoteInstructions=13,scalarVirtual58='139C90',
 scope='Prepared WORD276 list/shared274 pool; leading headerE skipping, two synthetic leading quotes, trailing apostrophe only; explicit separate scratch, no vendor execution'),indent=2)+'\n',encoding='utf-8')
print('PASS static3ranges/13quoteinstructions/274.V58 binding/headerE; scalar no-write and label-residue branches')
