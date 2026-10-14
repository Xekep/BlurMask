"""Pinned first140 replacement ownership/dispatch proof; reads binaries only."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--resource',required=True);p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions,parse_alphabet
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
assert b.data[b.off(0x1802eb388):b.off(0x1802eb388)+4]==bytes.fromhex('efbfbd00')
init=f['1801a6980'];label=init.index('&DAT_1802eb388,3');assert '*(undefined2 *)(param_2 + 0x1be) = uVar7' in init[label:label+160]
actual=parse_alphabet(pathlib.Path(a.resource).read_bytes(),6219);assert actual['mapping'][189]=='\ufffd'
body=f['1801acfa0'];assert body.index('0x1dc')<body.index("(char)param_3[7] == '\\x06'")<body.index('0x192')<body.index('0x1be')
assert '(short)param_3[9] != *(short *)(param_2 + 0x1be)' in body
assert '*(short *)((longlong)param_3 + 0x26) != *(short *)(param_2 + 0x1be)' in body
assert 'param_4[0x20] = cVar10' in body and "if ((cVar10 != '\\0')" in body and 'goto LAB_1801ad839' in body
dispatch=asm(0x1801acfc7,0x1801ad01f)
assert '1801acfc7 cmp byte ptr [r8 + 0x1c], 6' in dispatch
assert '1801acff5 jmp r9' in dispatch and '1801ad012 cmp byte ptr [rdx + 0x192], 0' in dispatch
tail=asm(0x1801ad839,0x1801ad879)
assert '1801ad855 add dword ptr [rax], r12d' in tail and 'setl al' in tail
#Both source special-label guards use this body, before the ordinary sampler98.
assert body.index('if ((cVar10')<body.index('*(short *)(param_4 + 0x50)')
report={'graph1BE':'1A6980 source alphabet018F70(U+FFFD at2EB388) ->1BE; actual first AL label189',
 'order':'1DC early true; kind6 ->191/samplerA0 before192 test; ordinary replacement/opaque flag20 ->zero delta and final strict signed limit',
 'firstScope':'191=1/210=1 and192=1 already proved by first literal context audit; scoped same descriptor ordinary0/2 and firstkind6 only',
 'notClaimed':'Generic main192false, protocol/sourceKind1/2, kind5 full ancestor search, opaque group ownership and automatic word winner'}
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report)+'\n',encoding='utf-8');print(json.dumps(report))
