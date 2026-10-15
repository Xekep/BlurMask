"""Pinned native first WORD publication argument ordering; static PE only."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();fs=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
ins=list(md.disasm(b.data[b.off(0x1801f5d55):b.off(0x1801f5d94)],0x1801f5d55))
byaddr={i.address:(i.mnemonic,i.op_str) for i in ins}
assert byaddr[0x1801f5d76]==('mov','dword ptr [rsp + 0x38], r12d')
assert byaddr[0x1801f5d7b]==('mov','dword ptr [rsp + 0x30], 0xffffffff')
assert byaddr[0x1801f5d91]==('call','r10')
publisher=fs['1801416b0'];assert 'local_120 = param_7;' in publisher and 'local_124 = param_8;' in publisher
span=fs['18013f520'];assert '*param_4 = uVar3;' in span and '*param_5 = uVar3;' in span
assert '+ 0x168)' in span and '+ 0x170)' in span
report=dict(instructions=[f'{i.address:x} {i.mnemonic} {i.op_str}' for i in ins],
    nativeCall='1F5D91: param7=-1, param8=wordEnd;1416B0 p7 overrides first,p8 last;13F520 returns first then last',
    scope='Fresh single plain-word publication; no aliases, terminal choice, glyph scores or NN tables changed')
pathlib.Path(args.output).write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS pinned first WORD native argument7/8 ordering +publisher/13F520 output ownership; static only')
