"""Read-only C57 ordered IDs and BCC selected field clone prefix proof."""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
regions=[('MyScriptInk.dll',
  '0x180001910',
  '0x180003ba0',
  'a1ba8eb9acefb3e60970d68317f36a699c50dacf587455a7d8dde87387842564',
  [('0x180001f57', 'lea', 'r12, [rip + 0xf3172]'),
   ('0x180001f5e', 'test', 'rdi, rdi'),
   ('0x180001f61', 'je', '0x180001f7f'),
   ('0x180001f7f', 'mov', 'r14, r12'),
   ('0x18000200d', 'mov', 'rax, qword ptr [r14 + 0x20]'),
   ('0x180002011', 'mov', 'qword ptr [rcx + 0x40], rax')]),
 ('MyScriptInk.dll',
  '0x1800b7690',
  '0x1800b7740',
  '5d50a1d32f3ceb609c0ad398e9ebcfd92ec73c2b174d28170b6bd56dcb8440ac',
  [('0x1800b7690', 'mov', 'dword ptr [r8 + 0x200], 0xffffffff'),
   ('0x1800b76b9', 'mov', 'rax, qword ptr [rax]'),
   ('0x1800b76cc', 'mov', 'rax, qword ptr [r8 + rax*8]'),
   ('0x1800b76ef', 'mov', 'rax, qword ptr [rax + 8]'),
   ('0x1800b76fb', 'mov', 'rax, qword ptr [rax]'),
   ('0x1800b7719', 'mov', 'rax, qword ptr [rax]'),
   ('0x1800b7723', 'lea', 'eax, [rcx - 1]'),
   ('0x1800b7738', 'mov', 'rax, qword ptr [r8 + rax*8]')]),
 ('MyScriptInk.dll',
  '0x1800b70a0',
  '0x1800b72e0',
  'd734d1c5810c96f377cc89eb4c241fd419b2beda3008085a259ad33e03b0b17e',
  [('0x1800b7157', 'sub', 'r13, qword ptr [r10 + 0x10]'),
   ('0x1800b715b', 'je', '0x1800b72c0'),
   ('0x1800b716d', 'test', 'r13, r13')]),
 ('MyScriptEngine.dll',
  '0x1800ebe40',
  '0x1800ec260',
  'ed9721a7fe60feb84b883963ec757418d8c2e039f25c641328d2c2b047524855',
  [('0x1800ebee2', 'call', 'r10'),
   ('0x1800ebefa', 'call', '0x1800ea360'),
   ('0x1800ebf57', 'call', 'r10'),
   ('0x1800ebf6f', 'call', '0x1800ea360'),
   ('0x1800ebf90', 'mov', 'byte ptr [rsp + 0x20], al'),
   ('0x1800ebf9a', 'call', '0x1800eb9c0'),
   ('0x1800ebfaf', 'call', '0x1800eb3b0'),
   ('0x1800ebfc4', 'cmp', 'byte ptr [rsp + 0x31], r14b'),
   ('0x1800ebfe0', 'call', '0x1800ebe40'),
   ('0x1800ebff6', 'call', '0x1800eaaf0'),
   ('0x1800ec01f', 'call', '0x1800f0d00'),
   ('0x1800ec02d', 'call', '0x1800ea480')]),
 ('MyScriptEngine.dll',
  '0x1800eb9c0',
  '0x1800ebb30',
  'b274130c5ea89c5e034734b860f54ae89261cb1107b5de7f95d89c70eff65c67',
  [('0x1800eba0c', 'call', '0x1800eb740'), ('0x1800eba88', 'mov', 'byte ptr [rdi + 1], bpl')]),
 ('MyScriptEngine.dll',
  '0x1800eb740',
  '0x1800eb9c0',
  '205b579a3f2f9dac7092458c8e005dafca2e305ebecd129709feeb80ede64c18',
  [('0x1800eb885', 'cmp', 'ebx, 0xf'),
   ('0x1800eb888', 'ja', '0x1800eb89f'),
   ('0x1800eb895', 'mov', 'r15b, 2'),
   ('0x1800eb8de', 'call', '0x1800c0560'),
   ('0x1800eb8eb', 'test', 'rax, rax'),
   ('0x1800eb8ee', 'je', '0x1800eb90f'),
   ('0x1800eb90f', 'xor', 'bl, bl'),
   ('0x1800eb911', 'jmp', '0x1800eb93d'),
   ('0x1800eb935', 'mov', 'byte ptr [rsi + 1], r15b'),
   ('0x1800eb939', 'movups', 'xmmword ptr [rsi + 0x20], xmm0')])]

shas={'MyScriptInk.dll':'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a','MyScriptEngine.dll':'d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'}
bs={n:Binary(n)for n in shas}
for n,b in bs.items():assert hashlib.sha256(b.data).hexdigest()==shas[n]
md=Cs(CS_ARCH_X86,CS_MODE_64);pins=0;output=[]
for n,first,end,sha,expected in regions:
 b=bs[n];first=int(first,16);end=int(end,16);raw=b.data[b.off(first):b.off(end)]
 assert hashlib.sha256(raw).hexdigest()==sha
 rows={(hex(i.address),i.mnemonic,i.op_str)for i in md.disasm(raw,first)}
 for pin in expected:assert pin in rows,(n,pin)
 pins+=len(expected);output.append(dict(dll=n,first=hex(first),end=hex(end),sha256=sha,pins=len(expected)))
bindings=[]
def bind(n,table,slot,target):
 assert bs[n].qwords(table+slot,1)==(target,)
 bindings.append(dict(dll=n,table=hex(table),slot=hex(slot),target=hex(target)))
for slot,target in [(0x58,0x1800b7690),(0x60,0x1800b76e0),(0x70,0x1800b70a0)]:bind('MyScriptInk.dll',0x1800f5120,slot,target)
bind('MyScriptInk.dll',0x1800f50d0,0x20,0x1800f5120)
bind('MyScriptEngine.dll',0x180227080,0x88,0x1800ebe40)
# BCC ordinary deep clone copies attribute values before child recursion.
# Heap reserve failure branches around tag/value publication, not through it.
# C57 bounded0..15 keys have the same signed in-order iteration as ascendingIDs.
dep=Path(__file__).with_name('audit_selected_original_context_bootstrap.py')
subprocess.run([sys.executable,str(dep),str(a.output)+'.context.json'],cwd=root,check=True)
a.output.write_bytes((json.dumps(dict(scope='Fresh selected ordinary full-domain owner, all C51 IDs/Next, chosen field BCC clone; no SDK acceptance',vendorExecution=False,binaries=shas,regions=output,pins=pins,bindings=bindings,dependencies=[dict(path='tools/handwriting/'+dep.name,sha256=hashlib.sha256(dep.read_bytes()).hexdigest())]),indent=2)+'\n').encode())
print(f'PASS{len(output)} regions/{pins} pins/{len(bindings)} bindings +selected-context dependency; no vendor execution')
