"""Static1941B0 resolved-line filters, primary-guide division and fresh scope."""
import argparse,hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x1801941b0,0x180195520;raw=b.data[b.off(a):b.off(z)];h=hashlib.sha256(raw).hexdigest();assert h=='8dbf55ad280b0857313412cec6b5721b964d8f9f62397b42e643fdd46cc47169'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);ins={i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)}
pins={0x18019462a:('cmp','dword ptr [rdi + 0x10], -1'),
      0x180194786:('mov','eax, dword ptr [rsi + 0x10]'),0x180194789:('cmp','dword ptr [rcx + 0x10], eax'),
      0x18019478c:('jne','0x1801947a7'),0x1801947a1:('minss','xmm6, dword ptr [rax + rdx + 4]'),
      0x180194ac5:('cmp','dword ptr [r12 + 0x10], -1'),
      0x180194c32:('mov','eax, dword ptr [r15 + 0x10]'),0x180194c36:('cmp','dword ptr [r12 + 0x10], eax'),
      0x180194c3b:('jne','0x180194c68'),0x180194c5a:('movss','xmm0, dword ptr [rax + rcx]'),
      0x180194ef9:('mov','r9, qword ptr [rax + 0x40]'),0x180194efd:('call','r9'),
      0x180194f00:('movss','xmm6, dword ptr [rax + 4]'),0x180194f4b:('divss','xmm1, xmm6'),0x180194ff6:('divss','xmm2, xmm6')}
for address,pin in pins.items():assert ins[address]==pin,(hex(address),ins.get(address),pin)
f=functions();reader=f['180196500'];clone=f['18018d590'];ctor=f['18018d400'];init=f['18018d920']
assert 'if (param_4 < 0x107)' in reader and '*(undefined1 *)(param_2 + 0x134) = 0;' in reader
assert '*(undefined1 *)(param_2 + 0x242) = *(undefined1 *)(param_2 + 0x134)' in clone
assert '*(undefined8 *)(param_2 + 0x1d0) = 0' in ctor and 'if (0x107 < param_4)' in reader
assert '*(longlong *)(param_3 + 0x1d0) != 0' in clone
assert "*(char *)(param_2 + 0x134) == '\\0'" in init and '*(undefined1 *)(param_2 + 0x242) = 0' in init
report=dict(start=hex(a),endExclusive=hex(z),sha256=h,pins=[f'{a:x} {m} {o}'for a,(m,o)in pins.items()],
            ownership='Both fold directions compare descriptor-resolved source line IDs(+10). Source40 current PRIMARY guide+4 supplies transition divisor; cross normalization/group193860 union height is separate. Actual compact selector order retained.',
            freshScope='Actual main sampler106: reader134=0→clone/init242=0; fresh18D4001D0=0 and optional network only serialized forversion>=108. No configured/special/unassigned source support inferred.',vendorExecution=False)
Path(args.output).write_text(json.dumps(report,indent=2)+'\n');print(f'PASS static{len(pins)} line-aware fold/primary-height pins +exact1941B0 PE block/freshno242/no1D0 bindings')
