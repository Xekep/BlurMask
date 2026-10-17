"""Static21F320 branch correspondence; reads pinned PE, never executes it."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x18021f320,0x18021fbf0;raw=b.data[b.off(a):b.off(z)]
digest=hashlib.sha256(raw).hexdigest();assert digest=='0b5ca736469edf51655bbbaad4f629afc31cda0122fccddc50365bbc55fb5e7c'
instructions={i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)}
pins={
0x18021f692:('test','eax, eax'),0x18021f694:('js','0x18021f73a'),
0x18021f6d8:('test','al, al'),0x18021f6da:('jne','0x18021f73a'),
0x18021f6dc:('cmp','si, word ptr [rbx + 0x24ba]'),0x18021f6e5:('cmp','si, word ptr [rbx + 0x24bc]'),
0x18021f6f7:('cmp','ax, 3'),0x18021f6fb:('jbe','0x18021f73a'),
0x18021f733:('mov','word ptr [rax + 8], r15w'),
0x18021f7a0:('test','eax, eax'),0x18021f7a2:('js','0x18021f90d'),
0x18021f818:('cmp','r15w, ax'),0x18021f81c:('jae','0x18021f87c'),
0x18021f833:('cmp','dword ptr [rax + rdx*4], 1'),
0x18021f884:('cmp','r15w, cx'),0x18021f892:('cmp','r15w, ax'),
0x18021f8a4:('cmp','ax, 3'),0x18021f8a8:('jbe','0x18021f964'),
0x18021f8e3:('mov','word ptr [rax + 8], si'),
0x18021f8e7:('test','r13b, r13b'),0x18021f8ec:('cmp','si, word ptr [rbx + 0x2512]'),
0x18021f8ff:('mov','word ptr [rax + 8], si'),
}
for address,expected in pins.items():assert instructions[address]==expected,(hex(address),instructions.get(address))
cursor_start,cursor_end=0x18021f210,0x18021f320
cursor_raw=b.data[b.off(cursor_start):b.off(cursor_end)]
assert hashlib.sha256(cursor_raw).hexdigest()=='7f77864c48059eea26486fe94e310f63695229d8651564a8781406050a4f1494'
cursor_instructions={i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(cursor_raw,cursor_start)}
cursor_pins={0x18021f24c:('cmp','edx, dword ptr [rax + r11 + 0x28]'),
 0x18021f253:('mov','rax, qword ptr [rcx + 0x20]'),0x18021f28b:('call','0x18021f210'),
 0x18021f295:('mov','rax, qword ptr [rax + r11 + 0x20]'),
 0x18021f29d:('mov','rbx, qword ptr [rax + rdx*8]'),0x18021f2a1:('lea','eax, [rdx + 1]'),
 0x18021f2ab:('mov','dword ptr [r10], eax'),0x18021f2be:('call','r9'),0x18021f2e6:('call','r10')}
for address,expected in cursor_pins.items():assert cursor_instructions[address]==expected,(hex(address),cursor_instructions.get(address))
Path(sys.argv[1]).write_text(json.dumps(dict(start=hex(a),endExclusive=hex(z),sha256=digest,
 pins=len(pins),cursorPins=len(cursor_pins),scope='Prepared numeric decisions and WORD cursor; no enabled owning source/category binding or changed F4 policy',vendorExecution=False),indent=2)+'\n')
print('PASS21F320/21F210 PE fingerprints and',len(pins)+len(cursor_pins),'quote and cursor pins')
