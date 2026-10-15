"""Static PE-only proof of source V80 reset, distinct from source68 refresh."""
import hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def instructions(address,size):
 return {i.address:(i.mnemonic,i.op_str) for i in cs.disasm(b.data[b.off(address):b.off(address)+size],address)}
checks=[]
def pin(address,mnemonic,operands):
 actual=instructions(address,16)[address];assert actual==(mnemonic,operands),(hex(address),actual)
 checks.append([hex(address),mnemonic,operands])
def slot(table,offset,target):
 assert b.qwords(table+offset,1)[0]==target
 checks.append([hex(table),hex(offset),hex(target)])
slot(0x1802e85b0,0x80,0x180232450) #292
slot(0x1802e8fa0,0x80,0x18023bdb0) #28E forwarding
slot(0x1802b4360,0x80,0x180092970) #2B7
slot(0x1802e8bb0,0x80,0x180236dc0) #28D selected sequence
slot(0x1802e8c60,0x80,0x1802372f0) #290 outer epoch, separately owned
pin(0x18023bdb0,'mov','rdx, qword ptr [rdx + 8]')
pin(0x18023bdb7,'mov','r10, qword ptr [rax + 0x80]')
pin(0x18023bdbe,'jmp','r10')
pin(0x180092978,'call','0x180092840')
pin(0x180232498,'mov','r10, qword ptr [rax + 0x80]')
pin(0x1802324a7,'call','r10')
pin(0x1802324ac,'je','0x1802324c0')
pin(0x1802324b4,'call','0x180231ed0')
pin(0x180231efb,'xor','r12d, r12d')
pin(0x180232037,'mov','dword ptr [rsi + 0x8378], r12d')
pin(0x180236e23,'mov','r10, qword ptr [rax + 0x80]')
pin(0x180236e2a,'call','r10')
body=f['180231ed0'];assert 'uVar4 < 8' in body and 'param_2 + 0x106f' in body
assert 'FUN_180092840' not in f['180234840'] and '+ 0x68)' in f['180234840']
driver=f['1802144f0'];assert driver.count('**(longlong **)(param_2 + 0x30) + 0x80')==2
assert 'local_168 + -1,puVar59' in driver
report={'textSha256':hashlib.sha256(b.data).hexdigest(),'pins':checks,
 'nativePolicy':'2144F0 V80 start-1 before node loop, completed node after each node; source68 alone does not reset queries',
 'roundRobin':'231ED0 DOES clear 8378 (232037); an earlier decompiler-index interpretation was incorrect',
 'scope':'Prepared common span source boundary; within-node persistent290 scheduling remains separate'}
if len(sys.argv)>1:
 out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,indent=2)+'\n')
print(f'PASS static PE-only case main span/reset: {len(checks)} instruction/table pins; inner092840 then231ED0,8378zero, source68 separate')
