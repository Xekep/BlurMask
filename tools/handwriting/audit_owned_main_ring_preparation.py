"""Pinned prepared-fresh policy scalar producers consumed by1FA300; no DLL load."""
import argparse,hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
b=Binary();fs=functions();checks={}
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def terms(address,name,*values):
    assert all(v in fs[address] for v in values),(name,values)
    checks[name]=dict(address=address,terms=values)
terms('18000c860','fresh-request50-and3C-sentinels',
      '*(undefined8 *)(param_2 + 0x50) = 0xffffffffffffffff;',
      '*(undefined8 *)(param_2 + 0x38) = 0xffffffffffffffff;')
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
actual=[]
for i in md.disasm(b.data[b.off(0x180151bf0):b.off(0x180151bf0)+32],0x180151bf0):
    actual.append(i.mnemonic+' '+i.op_str)
    if i.mnemonic=='ret':break
assert actual==['mov rax, qword ptr [rdx + 8]','mov dword ptr [rax + 0x54], 0x78',
               'mov rcx, qword ptr [rdx + 8]','mov eax, dword ptr [rax + 0x28]',
               'mov dword ptr [rcx + 0x58], eax','ret ']
checks['151BF0-54-and58']=actual
terms('1801d60d0','selected-28-variant-table',
      '*(int *)(param_3 + 0x28) == -1',
      'param_2 + 0x214 + (ulonglong)*(byte *)(param_3 + 0x1b) * 2')
terms('18020c8c0','actual68-default-conditional',
      "*(char *)(param_2 + 0x22f6) == '\\0'", '1 < ((byte)*(undefined4 *)(param_2 + 0x22e8) & 7)',
      'iVar2 = *(int *)(*(longlong *)(param_2 + 0x18) + 0x3c);','if (iVar2 == -1)',
      'iVar2 = 0x78;', '*(int *)(*(longlong *)(param_2 + 0x18) + 0x68) = iVar2;')
terms('180150800','property-copy-reads-parent-and-writes-children',
      'param_1,*(undefined8 *)(param_2 + 8),param_3,&local_res8,&local_res10',
      'local_res8 + 8','local_res20 + 0x10', 'param_2 + 0x60', 'param_2 + 0x78')
assert b.qwords(0x1802de610+0x88,1)[0]==0x180151fc0
assert b.qwords(0x1802e1ff0+0x20,1)[0]==0x1801a73c0
checks['node-count-vtable-chain']=['222.V88=151FC0','21B.V20=1A73C0']
terms('1801a73c0','actual-compact-node-count',
      'if (0 < *(int *)(param_2 + 0x220))','return *(int *)(param_2 + 0x220);')
terms('1802144f0','count-to-ring-consumer',
      'if (iVar22 == -1)', '**(longlong **)(param_2 + 0x20) + 0x88',
      'FUN_1801fa300(param_1,param_2,iVar22)')
Path(args.output).write_bytes((json.dumps(dict(checks=checks,scope=
    'Fresh no-property DIRECT prepared policy ownership only;50 remains-1, '
    '54/58/68 have their verified producers. This is not automatic20D8F0 '
    'factory reconstruction or native grouped allocation/rollback.'),indent=2)+'\n').encode('utf-8'))
print(f'PASS {len(checks)} prepared-fresh scalar/node-count to native ring bindings; no vendor execution.')
