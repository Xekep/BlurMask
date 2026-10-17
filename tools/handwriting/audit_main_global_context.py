"""Static-only descriptor/guide ownership of actual lazy2271C9870."""
import argparse,hashlib,json,pathlib,struct,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x1801c9870,0x1801ca530;raw=b.data[b.off(a):b.off(z)];h=hashlib.sha256(raw).hexdigest();assert h=='60ff25abbb76c0e659a3437fa9479522f2ed6b6a6c9f3cd7e1d5decda76715c4'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);allins={i.address:i for i in md.disasm(raw,a)}
ta,tz=0x1801ca5f0,0x1801cabb0;target_raw=b.data[b.off(ta):b.off(tz)];th=hashlib.sha256(target_raw).hexdigest();assert th=='63f0c5d5b97a95b309441317cf2089f4ebe33f3b103054869e77d0a365a945b8'
allins.update({i.address:i for i in md.disasm(target_raw,ta)});ins={k:(i.mnemonic,i.op_str)for k,i in allins.items()}
pins={0x1801c9cbd:('test','byte ptr [rax], 0xf'),0x1801c9cc6:('cmp','dword ptr [rax + 0x4c], 0'),0x1801c9cca:('jge','0x1801ca4d4'),
0x1801c9cdc:('movaps','xmm8, xmm13'),0x1801c9ce0:('mov','r8d, dword ptr [rax + 0x10]'),0x1801c9cf5:('mov','r9, qword ptr [rax + 0x40]'),0x1801c9cf9:('call','r9'),
0x1801c9d01:('movss','xmm11, dword ptr [rax + 4]'),0x1801c9d07:('movss','xmm10, dword ptr [rax]'),0x1801c9d8b:('mov','ecx, dword ptr [r8 + 0x24]'),
0x1801c9d8f:('mov','edx, dword ptr [r8 + 0x50]'),0x1801c9d93:('mov','r9d, dword ptr [r8 + 0x28]'),0x1801c9d9c:('lea','r12d, [rcx + 1]'),0x1801c9daa:('jle','0x1801c9dc6'),
0x1801ca39c:('mov','edx, dword ptr [rsp + 0x4c]'),0x1801ca3c3:('mov','dword ptr [rcx + rax], edx'),0x1801ca3df:('mov','dword ptr [rcx + rax + 4], r12d'),
0x1801ca3f6:('mov','dword ptr [rcx + rax + 8], edx'),0x1801ca4d8:('inc','ecx'),0x1801ca4e1:('jb','0x1801c9ca0'),
0x1801ca849:('mov','r9, qword ptr [rax + 0x40]'),0x1801ca856:('call','r9'),0x1801ca863:('movss','xmm10, dword ptr [rax + 4]'),
0x1801ca907:('mov','r8d, dword ptr [rdi + r8]'),0x1801ca90b:('call','r9'),0x1801ca90e:('mov','ecx, dword ptr [rax + 0x10]'),
0x1801ca911:('cmp','ecx, ebx'),0x1801ca92e:('cmp','ecx, dword ptr [rsp + 0x30]'),0x1801ca932:('jne','0x1801caaba'),0x1801caa19:('divss','xmm1, xmm10')}
for address,pin in pins.items():assert ins[address]==pin,(hex(address),ins.get(address),pin)
i=allins[0x1801c9c8d];assert(i.mnemonic,i.op_str)==('movss','xmm13, dword ptr [rip + 0x1238ae]')
constant=i.address+i.size+0x1238ae;assert struct.unpack_from('<I',b.data,b.off(constant))[0]==0xff7fffff
f=functions()['1801c9870'];assert "*(uint *)((ulonglong)*(ushort *)(param_2 + 0x82) * 0x20 +\n                         *(longlong *)(param_2 + 0x88)) = local_ec;" in f
ak=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
profile=struct.unpack_from('<3fBBHf4B',ak,195746);assert profile[:6]==(0.,1.,1.,0,3,28)and profile[7:]==(1,2,0,1)
target=functions()['1801ca5f0'];assert '+ 0x78)' in target and '+ 0x80)' not in target
report=dict(start=hex(a),endExclusive=hex(z),sha256=h,targetBlock=dict(start=hex(ta),endExclusive=hex(tz),sha256=th),pins=[f'{k:x} {m} {o}'for k,(m,o)in pins.items()],referenceConstant=hex(constant),configuration=profile,
 ownership='Context+0 receives descriptorordinal. CA5F0 resolves every recorddescriptor to itslineID forselection; targetweightheight isPRIMARYguide+4,notnormalizationunion. Assignedguide+0/+4,physicalFirst+1..Last,reference resetperdescriptor. Fresh4Cnegative/50nonpositive only.',vendorExecution=False)
pathlib.Path(args.output).write_text(json.dumps(report,indent=2)+'\n');print(f'PASS static{len(pins)+1} global227 descriptor/guide/window pins andresource profile; no vendor execution')
