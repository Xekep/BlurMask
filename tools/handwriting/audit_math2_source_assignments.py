"""041020 source-aware assignment and caller binding; PE reads only."""
from pathlib import Path
import hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll');assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions=[(6442717216, 6442717760, '0eb8647538e57555fb3b2a30a713ef954c06cd08f05707f9a766d242be28faee'), (6442721536, 6442723834, '5759f6dc6cc730baa03b865f1ba5a75a659b75a47801ef520f5fa8041d726599')]
ins={}
for a,z,h in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({i.address:(i.mnemonic,i.op_str) for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)})
pins=[(6442717273, 'cmp', 'dword ptr [rdi + 8], edx'), (6442717276, 'jne', '0x18004106d'), (6442717278, 'mov', 'al, 1'), (6442717436, 'cmp', 'byte ptr [rsp + 0xa8], 0'), (6442717444, 'jne', '0x180041121'), (6442717446, 'test', 'r15, r15'), (6442717449, 'je', '0x180041121'), (6442717451, 'cmp', 'dword ptr [r15 + 8], 0'), (6442717456, 'je', '0x180041121'), (6442717461, 'cmp', 'byte ptr [rax + r14*4 + 3], 0'), (6442717467, 'je', '0x180041204'), (6442717476, 'mov', 'edx, 0xffffffff'), (6442717526, 'comiss', 'xmm0, xmm1'), (6442717529, 'jbe', '0x18004115e'), (6442717540, 'cmovbe', 'eax, edx'), (6442717556, 'mov', 'rax, qword ptr [rbp]'), (6442717563, 'mov', 'ebp, dword ptr [rax + rcx*4]'), (6442717589, 'call', '0x180022df0'), (6442717642, 'call', '0x1801ac820'), (6442717669, 'mov', 'dword ptr [rdx], ebp'), (6442717677, 'mov', 'dword ptr [rdi + 8], eax'), (6442723088, 'mov', 'rdx, qword ptr [rbp - 0x70]'), (6442723092, 'lea', 'rax, [rsp + 0x58]'), (6442723097, 'mov', 'byte ptr [rsp + 0x28], 0'), (6442723102, 'lea', 'r9, [rsp + 0x68]'), (6442723107, 'lea', 'r8, [rbp - 0x20]'), (6442723111, 'mov', 'qword ptr [rsp + 0x20], rax'), (6442723119, 'call', '0x180041020')]
for address,m,o in pins:assert ins[address]==(m,o),(hex(address),ins.get(address))
out=Path(sys.argv[1]).resolve();out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(regions=regions,instructionPins=len(pins),
 semantics=['existing assignment count equals segment count returns before source/flag scan',
 'all-columns byte nonzero or null/empty source recomputes every column; otherwise skips byte3==0 source columns',
 'first strict observable column maximum mapped through original row indices',
 'new index inserted at original source column, shifting existing retained indices',
 'actual042710..72F caller binds existing source, cloned filtered tensor, observable original map and partial assignment vector separately; final flag0'],
 adaptation='Prepared bounded copied arrays and finite probabilities; no native allocation-prefix or request-policy claim',
 missing=['owning general existing source','full EF4 integration'],vendorExecution=False),indent=2)+'\n',encoding='utf8')
print(f'PASS2 complete041020/caller regions/{len(pins)} pins; source-aware assignment indices')
