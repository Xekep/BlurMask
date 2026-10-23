"""Read-only Engine PE caller/ABI/chronology pins. No SDK code is loaded/executed.
The pin set establishes local selected caller relations, not a concrete live
resource stream/environment, complete parser semantics or imported weights.
"""
from pathlib import Path
import hashlib,json,struct,argparse
from capstone import Cs,CS_ARCH_X86,CS_MODE_64,CS_OP_MEM,CS_OP_REG,CS_OP_IMM
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
d=(a.sdk/'MyScriptEngine.dll').read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest();assert sha(d)=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
pe=struct.unpack_from('<I',d,60)[0];op=pe+24;base=struct.unpack_from('<Q',d,op+24)[0];start=op+struct.unpack_from('<H',d,pe+20)[0];sections=[]
for k in range(struct.unpack_from('<H',d,pe+6)[0]):
 off=start+40*k;vs,rva,n,raw=struct.unpack_from('<IIII',d,off+8);sections.append((rva,n,raw))
def read(addr,n):
 for r,s,f in sections:
  if base+r<=addr and addr+n<=base+r+s:return d[f+addr-base-r:f+addr-base-r+n]
 raise AssertionError(('unbacked',hex(addr),n))
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True;by={};bodies=[];full=[]
# Complete Ghidra getBody ranges are embedded and independently PE-hashed.
BODIES = [{'function': '0x180106dd0', 'start': '0x180106dd0', 'endInclusive': '0x1801090e3', 'bytes': 8980, 'instructions': 2410, 'sha256': '0abf310b6958b37024de4175bec0e1e3221ea4bb4b3166d03cd9f039d865a1aa'}, {'function': '0x18010e410', 'start': '0x18010e410', 'endInclusive': '0x18010eb90', 'bytes': 1921, 'instructions': 507, 'sha256': '7ee8d7efefa86b6ec5858599e009ddbac6bbcfc7fcad93d74fc71544ad0121c6'}, {'function': '0x1801941b0', 'start': '0x1801941b0', 'endInclusive': '0x1801946f7', 'bytes': 1352, 'instructions': 381, 'sha256': 'c9a84b82b5e81426fbce1cce534a4bdacf7a30e243b89a0fb467f77106859bd7'}, {'function': '0x180106360', 'start': '0x180106360', 'endInclusive': '0x180106919', 'bytes': 1466, 'instructions': 393, 'sha256': '2308f01d9a4308d0b23c9d11d18209ad5516c348f77ee200a9f34595f341cb13'}, {'function': '0x1801062c0', 'start': '0x1801062c0', 'endInclusive': '0x1801062fe', 'bytes': 63, 'instructions': 18, 'sha256': '7d07c49b08e235795106cc1ddd59ca1ba3db42b61a167a4942587155a8879d92'}, {'function': '0x18010e3c0', 'start': '0x18010e3c0', 'endInclusive': '0x18010e3d0', 'bytes': 17, 'instructions': 5, 'sha256': '04dab6a3c1e99c8bcebe10ad10e000f1b0bb850c1b19c313962ddcdf9a54fc42'}, {'function': '0x18010ec10', 'start': '0x18010ec10', 'endInclusive': '0x18010ee67', 'bytes': 600, 'instructions': 137, 'sha256': 'beccbd905f39655c31b8aa9c514b1e0e6ae75b3f147add0b35efe0a8d0c85cfb'}, {'function': '0x180109120', 'start': '0x180109120', 'endInclusive': '0x180109b26', 'bytes': 2567, 'instructions': 638, 'sha256': 'cddca9b60bf9e5bd1f1cd1a61e7b5e44fd63297cfaf09a00bf2deedc80e3fe02'}]
for row in BODIES:
 fn=int(row['function'],16);st=int(row['start'],16);en=int(row['endInclusive'],16);n=int(row['bytes']);assert en-st+1==n
 code=read(st,n);assert sha(code)==row['sha256'];ins=list(md.disasm(code,st));assert sum(i.size for i in ins)==n and len(ins)==row['instructions'];assert st==fn
 bodies.append(dict(function=hex(fn),start=hex(st),endInclusive=hex(en),bytes=n,instructions=len(ins),sha256=sha(code)))
 by[fn]={i.address:i for i in ins};full.extend((fn,i)for i in ins)
def pin(fn,at,m,o):
 i=by[fn][at];assert (i.mnemonic,i.op_str)==(m,o),(hex(at),i.mnemonic,i.op_str);return dict(function=hex(fn),pc=hex(at),bytes=i.bytes.hex(),mnemonic=m,operands=o)
P=[]
def q(fn,at,m,o):P.append(pin(fn,at,m,o))
L=base+0x106dd0;B=base+0x10e410;C=base+0x1941b0;V=base+0x106360;I=base+0x10e3c0
# A genuine selected reader's untouched entry environment and exact local frame.
for at,m,o in [(0x106de0,'lea','rbp, [rsp - 0x5d0]'),(0x106e02,'mov','qword ptr [rsp + 0x78], rdx'),(0x106e2d,'mov','r15, r8'),(0x106e35,'mov','r13, rcx'),(0x106e21,'movzx','r12d, r9w'),(0x107465,'mov','ebx, 0x106'),(0x1078b0,'cmp','r12w, bx'),(0x1078b4,'jb','0x180107e33'),(0x1079ae,'call','0x18010e410'),(0x107d6d,'call','0x18010e410'),(0x107e4c,'call','0x180106360')]:q(L,base+at,m,o)
for at,m,o in [(0x107980,'lea','r8, [rbp - 0x40]'),(0x107984,'mov','rdx, rsi'),(0x107987,'mov','rcx, r13'),(0x10798a,'mov','r9d, dword ptr [rax + 8]'),(0x1079a6,'mov','dword ptr [rsp + 0x20], 0xffffffff'),(0x10797b,'mov','byte ptr [rsp + 0x40], 0'),(0x107e3c,'mov','r9d, 0x70'),(0x107e42,'lea','r8, [rbp - 0x40]'),(0x107e46,'mov','rdx, rsi'),(0x107e49,'mov','rcx, r13')]:q(L,base+at,m,o)
# Same incoming packet/environment and callback-returned graph-stride target.
for at,m,o in [(0x10e429,'mov','r14, r8'),(0x10e42c,'mov','rsi, rdx'),(0x10e42f,'mov','r15, rcx'),(0x10e8f9,'mov','eax, dword ptr [r14]'),(0x10e8fc,'mov','dword ptr [rsi + 0x18], eax'),(0x10e955,'mov','edx, 0x58'),(0x10e95a,'movsxd','r8, dword ptr [rsi + 0x18]'),(0x10e95e,'mov','rcx, r15'),(0x10e985,'call','r9'),(0x10e988,'mov','qword ptr [rsi + 0x10], rax'),(0x10e9b6,'imul','rdx, rax, 0x58'),(0x10e9c0,'mov','r8, r14'),(0x10e9c3,'add','rdx, qword ptr [rsi + 0x10]'),(0x10e9c7,'mov','rcx, r15'),(0x10e9e7,'call','0x1801941b0')]:q(B,base+at,m,o)
# Actual selected ABI four stack fields, not guessed decompiler argument count.
for at,m,o in [(0x10e9ca,'mov','byte ptr [rsp + 0x38], bpl'),(0x10e9cf,'mov','dword ptr [rsp + 0x30], eax'),(0x10e9d9,'mov','dword ptr [rsp + 0x28], eax'),(0x10e9e3,'mov','dword ptr [rsp + 0x20], eax'),(0x10ea15,'call','0x180111a60')]:q(B,base+at,m,o)
for at,m,o in [(0x10659a,'mov','r8, r13'),(0x1065a2,'mov','rdx, rbx'),(0x1065a9,'mov','rcx, rsi'),(0x1065bd,'call','0x18010e410')]:q(V,base+at,m,o)
# Complete query-interface selector and the exact unregistered version leaf.
assert [(i.mnemonic,i.op_str)for i in by[I].values()]==[('xor','ecx, ecx'),('lea','rax, [rip + 0x11bb17]'),('cmp','edx, 0x11'),('cmovne','rax, rcx'),('ret','')]
lea=by[I][base+0x10e3c2];assert lea.address+lea.size+lea.operands[1].mem.disp==base+0x229ee0
assert read(base+0x106dc0,6)==bytes.fromhex('b806010000c3')
assert struct.unpack('<3Q',read(base+0x229ee0,24))==(L,base+0x106dc0,base+0x109120)
assert struct.unpack('<Q',read(base+0x22a0d8,8))[0]==I
assert struct.unpack('<Q',read(base+0x252e28,8))[0]==base+0x229f30
# Local register identity proof is conditional on standard Win64 nonvolatile
# preservation by called routines. Actual pointed-to object provenance is open.
families={'env':['r13','r13d','r13w','r13b'],'frame':['rbp','ebp','bp','bpl']}
def writes(fn,family,stop):
 return [(i.address,i.mnemonic,i.op_str)for i in by[fn].values()if i.address<stop and set(family)&{i.reg_name(r)for r in i.regs_access()[1]}]
assert writes(L,families['env'],base+0x1079af)==[(base+0x106e35,'mov','r13, rcx')]
assert writes(L,families['frame'],base+0x1079af)==[(base+0x106de0,'lea','rbp, [rsp - 0x5d0]')]
# Ignore only explicit early-return epilogue POPs; these paths cannot reach the
# later constructor call. No loop/body rebind of packet or env is permitted.
for family,at,expected in [(['r14','r14d','r14w','r14b'],base+0x10e429,'r14, r8'),(['r15','r15d','r15w','r15b'],base+0x10e42f,'r15, rcx')]:
 definitions=writes(B,family,base+0x10e9e8)
 for pc,mnemonic,_ in definitions:
  if mnemonic!='pop':continue
  # Every ignored restore has a straight-line, call-free POP...RET suffix.
  # Such a path terminates before the later constructor call; no CFG guess.
  cursor=pc
  for _ in range(16):
   instruction=by[B][cursor];assert instruction.mnemonic in ('pop','ret')
   if instruction.mnemonic=='ret':break
   cursor+=instruction.size
  else:raise AssertionError('Unclosed early-return register restore')
 defs=[x for x in definitions if x[1]!='pop'];assert defs==[(at,'mov',expected)]
raw=(a.models/'en-US/en_US-ak-cur.res').read_bytes();assert sha(raw)=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927';off=1774299
assert struct.unpack_from('<IIIHH',raw,off)==(0x0101fe90,2008041,0x5f,0,0x106)
assert struct.unpack_from('<2I',raw,off+16)==(1,4)
assert [struct.unpack_from('<4I',raw,off+24+16*k)for k in range(4)]==[(1,13,0,2),(8,127,0,2),(8,127,0,2),(8,127,0,2)]
assert struct.unpack_from('<4I',raw,off+88)==(0,182,1,0)
listing='\n'.join(f'{fn:x}\t{i.address:x}\t{i.bytes.hex()}\t{i.mnemonic}\t{i.op_str}'for fn,i in full)+'\n';a.output.with_suffix('.instructions.tsv').write_text(listing)
report=dict(status='PASS bounded STATIC caller ABI and conditional selected reader chronology',binarySHA256=sha(d),functions=bodies,fullInstructions=len(full),pins=P,localDirectCalls=[dict(caller=hex(fn),pc=hex(i.address),target=hex(i.operands[0].imm))for fn,i in full if i.mnemonic=='call' and i.operands[0].type==CS_OP_IMM and i.operands[0].imm in (L,B,C)],selectedResourceVersion=0x106,liveVersionArgumentOriginProven=False,legacy106360PathSelectedForVersion106=False,conditionalVersion106FirstCall='0x1801079ae',conditionalTemporaryAlignmentCall='0x180107d6d',directConstructorCall='0x18010e9e7',sourceEnvironmentSameIncomingRcx=True,packetSameLoaderFrame='rbp-0x40',constructorGraphTarget='callback-returned network+0x10 base plus88*sequentialMember (freshness/disjointness NOT proved)' ,conditionalAbi='Standard Win64 nonvolatile register preservation; all called functions obey valid bounded object contracts',interface17={'accessor':'0x18010e3c0','returnsTable':'0x180229ee0','read':'0x180106dd0','versionLeaf':'0x180106dc0','write':'0x180109120'},remaining=['Genuine outer caller of selector17, installation of method-table owner and live R9w version-argument origin; version106 route is conditional only.','Live resource stream object+10 read-vtable+20 identity, origin, cursor and successful read semantics.','Environment object+8 origin/service registry/default callbacks; callback success, lifetime, freshness/disjointness and nonaliasing allocations remain conditional.','Complete selected packet including all scalar lanes and skip/alignment/import chronology; initial projected lanes alone do not prove whole packet.','Actual constructor register/body composition from loader, not just preserved symbolic ingress markers.','Post-constructor matrix payload import, temporary network conversion and whole alias/store closure.'],runtimePredicateApproved=False,vendorExecuted=False,nnExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS',len(bodies),'full functions,',len(full),'instructions,',len(P),'local binding pins; live stream/environment origin OPEN')
