"""Static207780 request producer and global retained-result ownership."""
import hashlib,json,subprocess,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]);dependency=out.with_name(out.stem+'-compact.json')
subprocess.run([sys.executable,str(root/'tools/handwriting/audit_main_compact_materialization.py'),str(dependency)],check=True)
b=Binary();a,z=0x180207780,0x180207920;raw=b.data[b.off(a):b.off(z)]
assert hashlib.sha256(raw).hexdigest()=='865a0ed642d5b834637d3df25d52faa6f5103a3e490df649aeb0d6781556f52d'
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)}
pins={
0x1802077af:('test','byte ptr [rdx + rdi + 6], 1'),
0x1802077cc:('cmp','dword ptr [rax + 0x30], -1'),
0x1802077d2:('cmp','r9w, word ptr [rbx + 0x230a]'),
0x1802077ea:('movzx','edi, byte ptr [rdx + rdi + 5]'),
0x1802077ef:('shr','dil, 1'),
0x1802077fc:('mov','rax, qword ptr [rax]'),
0x1802077ff:('mov','qword ptr [rbx + 0x1648], rax'),
0x180207806:('mov','dword ptr [rbx + 0x1664], r8d'),
0x18020781a:('mov','qword ptr [rbx + 0x1658], r14'),
0x180207821:('mov','byte ptr [rbx + 0x1660], r14b'),
0x18020782b:('movsx','ecx, byte ptr [rax + rdx + 0x20]'),
0x180207830:('mov','dword ptr [rbx + 0x1668], ecx'),
0x18020785f:('mov','word ptr [rbx + 0x166e], cx'),
0x180207866:('or','ax, 0x100'),
0x18020786a:('mov','word ptr [rbx + 0x166c], ax'),
0x180207874:('mov','dword ptr [rbx + 0x1680], r14d'),
0x180207882:('call','r9'),
0x1802078d6:('call','r9')}
for at,expected in pins.items():assert ins[at]==expected,(hex(at),ins.get(at))
global_source=(root/'src/InkRecognition/LegacyFreshMainGlobalCompactReachability.cs').read_text()
for pin in ('actualTerminal.PassResult','pass.ValidateResultQueryLease','pass.ReadResultQuery(tag,query,false,cancellation)',
            'graph.State.MaximumAtoms','policy.Owner.Contexts.ContainerIdentity','LegacyPreparedMainCompactReachability.Run'):
    assert pin in global_source,pin
old=(root/'src/InkRecognition/LegacyOwnedMainCompactWordReachability.cs').read_text()
assert 'LegacyPreparedMainCompactReachability.Run' in old
out.write_text(json.dumps(dict(dependency=json.loads(dependency.read_bytes()),requestPins=len(pins),ownerPins=7,
    scope='Ordinary primary207780 and full marking; global post-finalV80 lease, no compact allocation or accepted text',vendorExecution=False),indent=2)+'\n')
print('PASS full207780 PE/18 request instructions/7 owner bindings/shared old+global numeric kernel')
