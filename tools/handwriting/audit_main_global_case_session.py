"""Static-only ordinary global21B -> cache/source68 -> case-session bindings."""
import argparse,hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);blocks=[];ins={}
for a,z,h in [(0x1801c01b0,0x1801c0f60,'f26c7046eb25bc1a123cf388b03698bf6c0b76031c9dac1a1ea7fa279fd05d9c'),
              (0x1801520e0,0x180152280,'ff062814ef63410f259703cc7a9751c5e239a4984cf6d0acfe81f9c4c1eaef31')]:
    raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
    blocks.append(dict(start=hex(a),endExclusive=hex(z),sha256=h));ins.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
pins={0x18015212e:('mov','r10, qword ptr [rax + 0x48]'),0x18015214f:('call','r10'),
      0x1801521c9:('mov','r9, qword ptr [rax + 0x38]'),0x1801521ee:('mov','r10, qword ptr [rcx + 0x70]'),
      0x180152218:('mov','r10, qword ptr [rax + 0x50]'),0x1801c01e9:('mov','rax, qword ptr [rax + 0x38]'),
      0x1801c0258:('call','0x1801b8850'),0x1801c025f:('cmp','byte ptr [rbx + 0x1a2], dl'),
      0x1801c026b:('mov','eax, dword ptr [rbx + 0x318]'),0x1801c02b4:('mov','rax, qword ptr [rax + 0x68]'),
      0x1801c0319:('call','0x18025ee50')}
for a,expected in pins.items():assert ins[a]==expected,(hex(a),ins.get(a),expected)
slots={0x38:0x1801a75f0,0x60:0x1801a77c0,0x68:0x1801a77d0,0x70:0x1801a7850,0x1c8:0x18005a400}
for offset,target in slots.items():assert b.qwords(0x1802e1ff0+offset,1)[0]==target
assert 'param_2 + 0x310' in f['1801c01b0'] and 'param_2 + 0x2f0' in f['1801c01b0']
assert '+ 0x68)' in f['180234840'] and 'FUN_180092840' not in f['180234840']
assert b.qwords(0x1802e85b0+0x80,1)[0]==0x180232450
out=dict(blocks=blocks,pins=[f'{a:x} {m} {o}'for a,(m,o)in pins.items()],
         graphSlots={hex(o):hex(t)for o,t in slots.items()},
         ownership='Ordinary21B grouped arcs from same-source OR actual cross/pruned compact producer. Span/group ID only are needed by these kernels; no seed memberships or document IDs are reconstructed. source68 refresh is distinct from V80.',
         limits='Actual global query source and explicit node/span scheduling. Word geometry, full controller, terminal winner and publication remain separate.',vendorExecution=False)
Path(args.output).write_text(json.dumps(out,indent=2)+'\n');print(f'PASS static{len(pins)} cache/source pins,5 ordinary graph slots +292V80,2 exact PE blocks; no vendor execution')
