"""Static PE-only owning main/first submission and ending-key bindings."""
import argparse, hashlib, json, pathlib, sys
import capstone

p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();fs=functions();checks={}
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def terms(address,*parts):
    body=fs[f'{address:x}'];assert all(v in body for v in parts),(hex(address),parts)
    checks[f'{address:x}']=list(parts)
terms(0x1801680d0,'FUN_180161120','FUN_18015eaa0','FUN_18015f050','FUN_18015d920')
terms(0x180158970,'FUN_180156200','FUN_180156630','param_4 + 0x150','uVar7 == 0','uVar7 == 6','FUN_180157c60','uVar10 = uVar10 + 0x58')
terms(0x180157c60,'param_3[0x16] * 3.0','0.02 <=','FUN_180156950','local_1e0','param_3 + 5')
terms(0x180170400,'0x34','0x38','FUN_18016f6f0','FUN_18016f170','param_3 + 0x78','*(undefined4 *)(lVar10 + 0x10) = uStack_74')
terms(0x18016f6f0,'0x248','0xc1a')
terms(0x180173c10,'*(int *)(param_3 + 0x80) == 0','*(longlong *)(param_3 + 0x78) + 4','FUN_1801728c0','fVar14,&local_38,&local_48,200')
terms(0x180167210,'iVar23 = piVar18[(int)uVar19 + 1] + -1','FUN_1801595b0(param_1,*(longlong *)(param_2 + 0x120) + 0x108)')
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
ending=list(md.disasm(b.data[b.off(0x180168e10):b.off(0x180168e10)+0xb3],0x180168e10))
lines=[i.mnemonic+' '+i.op_str for i in ending]
assert 'mov rax, qword ptr [r10 + 0x108]' in lines
assert b.qwords(0x1802df340+0xa0,1)[0]==0x180168e10
checks['ending108getter']={'target':'168E10','lines':lines,
    'bytesSha256':hashlib.sha256(b.data[b.off(0x180168e10):b.off(0x180168e10)+0xb3]).hexdigest()}
ak=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
report=dict(checks=checks,scope='Composition of already restored fresh ordinary main descriptor owners and the same selected first factory. No first multiword materializer, empty-first24A lifecycle, multi-descriptor main sampler or main winner is inferred.')
pathlib.Path(args.output).write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS {len(checks)} pinned main/first submission/720/24B/265/ending108 checks; static PE only')
