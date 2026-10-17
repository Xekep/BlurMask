"""Actual math outer-loop bound, final-step fallback and completed sorting; read-only."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
a,z=0x180038610,0x18003a260
raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()=='180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)}
pins=[(6442683597, 'mov', 'rax, rbx'), (6442683600, 'inc', 'rbx'), (6442683614, 'mov', 'qword ptr [rbp - 0x38], rbx'), (6442683618, 'cmp', 'rax, rdx'), (6442683621, 'jae', '0x1800399a5'), (6442683643, 'mov', 'ecx, ebx'), (6442683645, 'call', 'rax'), (6442683649, 'je', '0x18003997a'), (6442685255, 'mov', 'dword ptr [r15], 0x7f7fffff'), (6442685311, 'call', '0x180035f70'), (6442685316, 'test', 'r13b, r13b'), (6442685336, 'jne', '0x1800394ec'), (6442685346, 'cmp', 'dword ptr [rdx + 8], 0'), (6442685350, 'jne', '0x1800394ec'), (6442685395, 'cmp', 'ecx, eax'), (6442685397, 'jge', '0x1800394e2'), (6442685409, 'ucomiss', 'xmm0, xmm6'), (6442685414, 'je', '0x1800394d0'), (6442685420, 'cmp', 'dword ptr [rdx + 8], eax'), (6442685429, 'movsxd', 'rsi, dword ptr [r12 + 0x28]'), (6442685440, 'add', 'rsi, qword ptr [rsp + 0x78]'), (6442685445, 'cmp', 'dword ptr [rsi + 8], 0'), (6442685482, 'movzx', 'ebx, word ptr [rax + rcx*2]'), (6442685512, 'lea', 'rdx, [r15 + 0x68]'), (6442685523, 'call', 'r9'), (6442685528, 'jne', '0x18003946f'), (6442685535, 'jb', '0x180039420'), (6442685559, 'mov', 'rax, qword ptr [rbp - 0x30]'), (6442685587, 'mov', 'qword ptr [rsp + 0x30], r12'), (6442685606, 'call', '0x180037430'), (6442685676, 'mov', 'r9d, 1'), (6442687155, 'call', '0x180036810'), (6442687181, 'cmp', 'qword ptr [rax + 0xc0], 0'), (6442687191, 'cmp', 'byte ptr [rax + 0xb6], 0'), (6442687209, 'call', '0x180038170'), (6442687228, 'cmp', 'dword ptr [r13 + 0x5c], 0'), (6442687248, 'call', '0x180038340'), (6442687267, 'cmp', 'byte ptr [r13 + 0x38], 0'), (6442687463, 'call', '0x180036f20'), (6442687500, 'call', '0x180036810'), (6442687509, 'cmp', 'r9d, esi'), (6442687514, 'sub', 'r9d, esi'), (6442687517, 'mov', 'r8d, esi'), (6442687526, 'call', '0x180035000')]
for a,m,o in pins:assert ins[a]==(m,o),(hex(a),ins.get(a))
#Every completed-record call in this entire function, including distinct
#imported-prefix and last-step fallback sites. No omitted first-EOS exit.
recorders=[a for a,(m,o)in ins.items()if m=='call'and o=='0x180037430']
assert recorders==[0x1800390b4,0x180039325,0x1800394a6],list(map(hex,recorders))
report=dict(scope='Static outer driver contract only; no implemented search, fallback owner or final acceptance',regions=1,instructionPins=len(pins),recordingSites=list(map(hex,recorders)),fallback='Successful last position, previously no completion, nonFMAX top-width row with ANY observable OLD primary prefix',sort='036810 full168 before optional postprocessing and again before tail destruction035000')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS outer driver complete region/'+str(len(pins))+' pins/3 distinct recorder sites; static only')
